using Compass.Digital.BO;
using Compass.Digital.DAL;
using Compass.Digital.GateKeeper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Compass.Digital.Synchronize.Records
{
    public class RequestValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            RequestMetaData request = null;
            KeyValuePair<string, object> args = default(KeyValuePair<string, object>);
            Client client;
            string API_VERSION = "";
            try
            {

                if (context.ActionDescriptor.EndpointMetadata.FirstOrDefault(FilterDescriptor => FilterDescriptor.GetType() == typeof(IgnoreRequestValidation)) != null)
                {
                    return;
                }

                var _memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                var _appSettings = context.HttpContext.RequestServices.GetService<IOptions<AppSettings>>();
                var _httpContextAccessor = context.HttpContext.RequestServices.GetService<IHttpContextAccessor>();
                //API_VERSION = _httpContextAccessor.HttpContext.GetRequestedApiVersion().MajorVersion + "." + _httpContextAccessor.HttpContext.GetRequestedApiVersion().MinorVersion;

                args = context.ActionArguments.FirstOrDefault();
                if (!args.Equals(default(KeyValuePair<string, object>)))
                {
                    request = args.Value as RequestMetaData;
                    if (request == null)
                    {
                        context.Result = new BadRequestObjectResult(Util.GetResponse(args.Key, ErrorCodeEnum.BadRequest, "Request object is null.", DateTime.UtcNow, API_VERSION));
                    }
                    else if (request.RequestingClient == null || string.IsNullOrWhiteSpace(request.RequestingClient.ClientId) || string.IsNullOrWhiteSpace(request.RequestingClient.MemberId))
                    {
                        context.Result = new BadRequestObjectResult(Util.GetResponse(args.Key, ErrorCodeEnum.MissingRequiredInfo, "Request received with missing client information.", DateTime.UtcNow, API_VERSION));
                    }

                    using (ClientDBManager clientDBManager = new ClientDBManager(_appSettings.Value.DBConnectionString, _appSettings.Value.DBCredential))
                    {
                        client = clientDBManager.GetClient(request.RequestingClient.ClientId, request.RequestingClient.MemberId).Result;
                    }

                    if (client == null)
                    {
                        context.Result = new UnauthorizedObjectResult(Util.GetResponse(args.Key, ErrorCodeEnum.Unauthorized, "Client is not authorized to access the data.", DateTime.UtcNow, API_VERSION));
                    }                    
                }
                else
                {
                    //.....Intentionally not using Util.GetResponse becuase args is null in this case
                    context.Result = new BadRequestObjectResult(new ResponseMetaData()
                    {
                        ErrorCode = ErrorCodeEnum.MissingRequiredInfo,
                        ErrorMessage = "Missing Required information.",
                        Timestamp = DateTime.UtcNow,
                        Version = API_VERSION,
                    });
                }
            }
            catch (Exception exc)
            {
                if (!args.Equals(default(KeyValuePair<string, object>)))
                {
                    context.Result = new BadRequestObjectResult(Util.GetResponse(args.Key,
                        ErrorCodeEnum.Error,
                        $"Following exception has occured while validating the request :{Environment.NewLine}Message:{exc.Message}{Environment.NewLine}StackTrace:{exc.StackTrace}",
                        DateTime.UtcNow,
                        API_VERSION));
                }
                else
                {
                    context.Result = new BadRequestObjectResult(new ResponseMetaData()
                    {
                        ErrorCode = ErrorCodeEnum.Error,
                        ErrorMessage = $"Following exception has occured while processing the request :{Environment.NewLine}Message:{exc.Message}{Environment.NewLine}StackTrace:{exc.StackTrace}",
                        Timestamp = DateTime.UtcNow,
                        Version = API_VERSION,
                    });
                }
            }
            finally
            {
                request = null;
            }
        }
    }
    public class IgnoreRequestValidation : Attribute
    {

    }
}
