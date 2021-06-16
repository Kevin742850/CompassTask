using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public enum ErrorCodeEnum
    {
        Success = 0,
        Created = 1,
        Error = 2,
        Unauthorized = 3,
        BadRequest = 4,
        MissingClientId = 5,
        MissingSecretKey = 6,
        MissingRequiredInfo = 7,
        ValidationErrors = 8,
        ConnectivityError = 9,
        NotFound = 10,
    }
}
