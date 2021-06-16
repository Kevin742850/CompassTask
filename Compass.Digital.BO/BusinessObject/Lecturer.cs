using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class Lecturer
    {
        public Lecturer()
        {
            Students = new List<Student>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime SynchDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Student> Students { get; set; }
    }
}
