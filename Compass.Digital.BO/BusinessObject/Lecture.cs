using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class Lecture
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime SynchDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public Lecturer Lecturer { get; set; }
    }
}
