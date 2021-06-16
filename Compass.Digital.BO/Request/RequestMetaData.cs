using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class RequestMetaData
    {
        public ClientIdentifier RequestingClient { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
