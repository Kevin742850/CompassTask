using Compass.Digital.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Compass.Digital.Synchronize.Records
{
    public static class Util
    {
        public static ResponseMetaData GetResponse(string requestType, ErrorCodeEnum errorCode, string errorMessage, DateTime timeStamp, string version)
        {
            switch (requestType.ToLower())
            {
                 
                 
                default:
                    return null;
            }
        }

    }

}
