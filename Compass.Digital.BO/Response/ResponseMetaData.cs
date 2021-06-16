using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class ResponseMetaData
    {
        public string Version { get; set; }
        public ErrorCodeEnum ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public long? Size { get; set; }
    }
}
