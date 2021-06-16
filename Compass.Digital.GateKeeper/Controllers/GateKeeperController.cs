using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Compass.Digital.Core;
using Compass.Digital.BO;
using Compass.Digital.DAL;

namespace Compass.Digital.GateKeeper.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/GateKeeper")]
    [ApiController]
    public class GateKeeperController : ControllerBase
    {
        protected readonly ILog<GateKeeperController> _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GateKeeperController(IOptions<AppSettings> appSettings, ILog<GateKeeperController> logger, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _appSettings = appSettings;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("Site is up.");
        }
        [HttpPost]
        [Route("CreateToken")]
        public async Task<ActionResult<TokenResponse>> CreateToken([FromBody]TokenRequest tokenRequest)
        {
            var tokenResponse = new TokenResponse();
            List<Client> clients = new List<Client>();
            Client client = null;
            ClientMember clientMember = null;
            string jwtToken = string.Empty;
            try
            {
                _logger.Information($"GateKeeperController===>CreateToken(): A new create token request is received from client '{tokenRequest.ClientId}'.");
                if (tokenRequest == null || (string.IsNullOrWhiteSpace(tokenRequest.ClientId) || string.IsNullOrWhiteSpace(tokenRequest.MemberId)))
                {
                    return BadRequest(new TokenResponse()
                    {
                        ErrorCode = ErrorCodeEnum.MissingClientId,
                        ErrorDescription = "BadRequest-Missing ClientId.",
                        Token = null
                    });
                }

                using (ClientDBManager clientDBManager = new ClientDBManager(_appSettings.Value.DBConnectionString, _appSettings.Value.DBCredential))
                {
                    clients = await clientDBManager.GetAllClients(tokenRequest.ClientId, tokenRequest.MemberId);
                    client = clients.FirstOrDefault();
                }
                if (client != null )
                {
                    clientMember = client.Members.FirstOrDefault(m => m.MemberId == tokenRequest.MemberId);
                    if (clientMember == null)
                    {
                        return Unauthorized(new TokenResponse()
                        {
                            ErrorCode = ErrorCodeEnum.Unauthorized,
                            ErrorDescription = "Unauthorized-Invalid ClientId or SecretKey.",
                            Token = null
                        });
                    }
                    
                        if (string.IsNullOrWhiteSpace(tokenRequest.ClientSecret))
                        {
                            return BadRequest(new TokenResponse()
                            {
                                ErrorCode = ErrorCodeEnum.MissingSecretKey,
                                ErrorDescription = "BadRequest-Missing Secret Key.",
                                Token = null
                            });
                        }

                        if (clientMember.SecretKey != tokenRequest.ClientSecret)
                        {
                            return BadRequest(new TokenResponse()
                            {
                                ErrorCode = ErrorCodeEnum.MissingSecretKey,
                                ErrorDescription = "BadRequest-Invalid Secret Key.",
                                Token = null
                            });
                        }
                     

                    // authentication successful so generate jwt token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Value.JwtSecret);
                    var encKey = Encoding.ASCII.GetBytes(_appSettings.Value.JwtSecret);
                    var member = client.Members.FirstOrDefault(m => m.MemberId == tokenRequest.MemberId);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, member.MemberId),
                            new Claim(ClaimTypes.Name, client.Name),
                            new Claim(CompassClaimTypes.CLIENTID, client.ID),
                            new Claim(ClaimTypes.Role,string.Join(",", from role in member.Roles select role.Name)),                            
                        }),
                        IssuedAt = DateTime.UtcNow,
                        Expires = DateTime.UtcNow.AddHours(clientMember.TokenLifeTime),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encKey), JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512),
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    jwtToken = tokenHandler.WriteToken(token);
                    tokenResponse.Token = jwtToken;
                    return Ok(new TokenResponse()
                    {
                        ErrorCode = ErrorCodeEnum.Success,
                        ErrorDescription = "Success-Token generated successfully.",
                        Token = tokenHandler.WriteToken(token)
                    });
                }
                else
                {
                    return Unauthorized(new TokenResponse()
                    {
                        ErrorCode = ErrorCodeEnum.Unauthorized,
                        ErrorDescription = "Unauthorized-Invalid ClientId or SecretKey.",
                        Token = null
                    });
                }
            }
            catch (AggregateException aggExc)
            {
                foreach (Exception exc in aggExc.Flatten().InnerExceptions)
                {
                    _logger.Error(exc, "Following aggregate exception has occured:", null);
                }

                return BadRequest(new TokenResponse()
                {
                    ErrorCode = ErrorCodeEnum.Error,
                    ErrorDescription = "An unhandled exception had occured.Please check the server log.",
                    Token = null
                });
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Following aggregate exception has occured:", null);

                return BadRequest(new TokenResponse()
                {
                    ErrorCode = ErrorCodeEnum.MissingSecretKey,
                    ErrorDescription = "An unhandled exception had occured.Please check the server log.",
                    Token = null
                });
            }
            finally
            {
                client = null;
                clientMember = null;
                jwtToken = null;
            }
        }
    }
}
