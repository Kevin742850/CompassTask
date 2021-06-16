using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.BO
{
    public class ClientMember
    {
        public string ClientId { get; set; }
        public string MemberId { get; set; }
        public string Name { get; set; }
        public string SecretKey { get; set; }
        public int TokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }
        public List<ClientMemberRole> Roles { get; set; }
        public bool Active { get; set; }
        public ClientMember()
        {
            Roles = new List<ClientMemberRole>();
        }
    }
}
