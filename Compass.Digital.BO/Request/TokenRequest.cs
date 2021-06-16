using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class TokenRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string MemberId { get; set; }
    }
}
