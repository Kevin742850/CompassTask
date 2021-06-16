using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class Client 
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public List<ClientMember> Members { get; set; }

        public Client()
        {
            Members = new List<ClientMember>();
        }

        public byte ContextId { get; set; }
    }
}
