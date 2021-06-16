
using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class SynchRecordsRequest : RequestMetaData
    {
        public SynchRecordsRequest()
        {
            Lectures = new List<Lecture>();
        }
        public DateTime LastSyncDate { get; set; }
        public List<Lecture> Lectures { get; set; }
    }
}
