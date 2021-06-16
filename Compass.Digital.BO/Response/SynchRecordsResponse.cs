using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class SynchRecordsResponse : ResponseMetaData
    {
        public List<Lecture> LectureDetails { get; set; }
    }
}
