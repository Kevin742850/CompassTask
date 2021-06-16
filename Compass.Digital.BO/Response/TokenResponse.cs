using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class TokenResponse
    {
        public ErrorCodeEnum ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string Token { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
