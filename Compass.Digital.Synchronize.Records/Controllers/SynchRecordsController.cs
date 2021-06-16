using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compass.Digital.BO;
using Compass.Digital.Core;
using Compass.Digital.DAL;
using Compass.Digital.GateKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Compass.Digital.Synchronize.Records.Controllers
{
    [Authorize(Roles = "Compass,Vendor")]
    [ApiVersion("2.0")]
    [Route("api/SynchRecords")]
    [ApiController]
    public class SynchRecordsController : ControllerBase
    {
        protected readonly ILog<SynchRecordsController> _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SynchRecordsController(IOptions<AppSettings> appSettings, ILog<SynchRecordsController> logger, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _appSettings = appSettings;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            //API_VERSION = _httpContextAccessor.HttpContext.GetRequestedApiVersion().MajorVersion + "." + _httpContextAccessor.HttpContext.GetRequestedApiVersion().MinorVersion;
        }
        [HttpGet]
        [Route("Test")]
        [IgnoreRequestValidation]
        public async Task<ActionResult<string>> Test()
        {
            return await Task.FromResult<string>("SUCCESS");
        }
        [HttpPost]
        [Route("SynchRecords")]
        public async Task<ActionResult<ResponseMetaData>> SynchRecords(SynchRecordsRequest synchRecordsRequest)
        {
            // DATA PUSHED FROM SERVER TO CLIENT MACHINE 
            #region SERVER TO CLIENT
            try
            {
                _logger.Information($"SynchRecordsController===>SynchRecords(): A synch record request is received as: {Environment.NewLine}{JsonConvert.SerializeObject(synchRecordsRequest)}");
                List<Lecture> lecture;
                using (ClientDBManager clientDBManager = new ClientDBManager(_appSettings.Value.DBConnectionString, _appSettings.Value.DBCredential))
                {
                    lecture = await clientDBManager.GetAllUnsynchedRecords(synchRecordsRequest.LastSyncDate);
                }
                #endregion SERVER TO CLIENT

                #region CLIENT DATA MERGED ON SERVER

                foreach (Lecture merged in synchRecordsRequest.Lectures)
                {
                    using (ClientDBManager clientDBManager = new ClientDBManager(_appSettings.Value.DBConnectionString, _appSettings.Value.DBCredential))
                    {
                        clientDBManager.MergedRecordsFromClientToServer(merged);
                    }
                }


                #endregion CLIENT DATA MERGED ON SERVER
                return Ok(new SynchRecordsResponse
                {
                    ErrorCode = ErrorCodeEnum.Success,
                    ErrorMessage = "",
                    Timestamp = DateTime.UtcNow,
                    Version = "1",
                    LectureDetails = lecture
                });
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "SynchRecordsController===>SynchRecords(): Following error has occured.", null);
                return BadRequest(new SynchRecordsResponse()
                {
                    ErrorCode = ErrorCodeEnum.Error,
                    ErrorMessage = $"Following exception has occured while processing the request :{Environment.NewLine}Message:{exc.Message}{Environment.NewLine}StackTrace:{exc.StackTrace}",
                    Timestamp = DateTime.UtcNow,
                    Version = "1",
                    LectureDetails = null
                });
            }
            finally
            {
            }

        }
    }
}