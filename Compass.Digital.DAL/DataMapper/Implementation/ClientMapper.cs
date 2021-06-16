using Compass.Digital.BO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Compass.Digital.DAL
{
    public class ClientMapper : IDataMapper<Client>
    {
        public Client MapSingle(DataSet ds)
        {
            throw new NotImplementedException();
        }

        public List<Client> MapMultiple(DataSet dsClients)
        {
            List<Client> clients = new List<Client>();
            Client client = null;
            ClientMember member = null;
            ClientMemberRole clientMemberRole = null;
        
            try
            {
                foreach (DataRow datarow in dsClients.Tables[0].Rows)
                {
                        client = new Client();
                        client.ID = datarow.Field<string>("Id");
                        client.Name = datarow.Field<string>("Name");
                        member = new ClientMember();
                        member.MemberId = datarow.Field<string>("MemberID");
                        member.SecretKey = datarow.Field<string>("SecretKey");
                        member.TokenLifeTime = datarow.Field<int>("TokenLifeTime");
                        member.AllowedOrigin = datarow.Field<string>("AllowedOrigin");
                        clientMemberRole = new ClientMemberRole();
                        clientMemberRole.Name = datarow.Field<string>("RoleName");
                        member.Roles.Add(clientMemberRole);
                        client.Members.Add(member);
                }
                    clients.Add(client);                
            }
            finally
            {
                client = null;
                member = null;
            }

            return clients;
        }
        #region IDisposable
        public void Dispose()
        {
            // If this function is being called the user wants to release the
            // resources. lets call the Dispose which will do this for us.
            Dispose(true);

            // Now since we have done the cleanup already there is nothing left
            // for the Finalizer to do. So lets tell the GC not to call it later.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                //someone want the deterministic release of all resources
                //Let us release all the managed resources
                DisposeManagedResources();
            }
            else
            {
                // Do nothing, no one asked a dispose, the object went out of
                // scope and finalized is called so lets next round of GC 
                // release these resources
            }

            // Release the unmanaged resource in any case as they will not be 
            // released by GC
            DisposeUnManagedResources();
        }
        private void DisposeManagedResources()
        {

        }
        private void DisposeUnManagedResources()
        {

        }
        #endregion
    }

}
