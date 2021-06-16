using Compass.Digital.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Compass.Digital.GateKeeper
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        protected readonly ILog<LoggerMiddleware> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoggerMiddleware(RequestDelegate next, ILog<LoggerMiddleware> logger, IHttpContextAccessor httpContextAccessor)
        {
            this._next = next;
            this._logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using (MemoryStream requestBodyStream = new MemoryStream())
            {
                using (MemoryStream responseBodyStream = new MemoryStream())
                {
                    Stream originalRequestBody = context.Request.Body;
                    context.Request.EnableRewind();
                    Stream originalResponseBody = context.Response.Body;

                    try
                    {
                        await context.Request.Body.CopyToAsync(requestBodyStream);
                        requestBodyStream.Seek(0, SeekOrigin.Begin);

                        string requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();

                        requestBodyStream.Seek(0, SeekOrigin.Begin);
                        context.Request.Body = requestBodyStream;

                        string responseBody = "";


                        context.Response.Body = responseBodyStream;

                        Stopwatch watch = Stopwatch.StartNew();
                        await _next(context);
                        watch.Stop();

                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        responseBody = new StreamReader(responseBodyStream).ReadToEnd();
                        string str = $"{context.Request.Host.Host}|{context.Request.Path}|{context.Request.QueryString.ToString()}|{context.Connection.RemoteIpAddress.MapToIPv4().ToString()}|{string.Join(",", context.Request.Headers.Select(he => he.Key + ":[" + he.Value + "]").ToList())}|{requestBodyText}|{responseBody}|{DateTime.Now}|{watch.ElapsedMilliseconds}";

                        
                        responseBodyStream.Seek(0, SeekOrigin.Begin);

                        await responseBodyStream.CopyToAsync(originalResponseBody);
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc, "Following error has occured", null);
                        //ExceptionLogger.LogToDatabse(ex);
                        byte[] data = System.Text.Encoding.UTF8.GetBytes("Unhandled Error occured, the error has been logged and the persons concerned are notified!! Please, try again in a while.");
                        originalResponseBody.Write(data, 0, data.Length);
                    }
                    finally
                    {
                        context.Request.Body = originalRequestBody;
                        context.Response.Body = originalResponseBody;
                    }
                }
            }
        }
    }
}
