using Compass.Digital.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compass.Digital.DAL
{
    public class ClientDBManager : DBManager, IDisposable
    {
        Client client = null;
        ClientMember member = null;
        ClientMemberRole clientMemberRole = null;
        public ClientDBManager(string connectionString, SqlCredential sqlCredential) : base(connectionString, sqlCredential)
        {

        }
        public async Task<List<Client>> GetAllClients(string clientId, string memberId)
        {
            string sql = string.Empty;
            DataSet dsClients;
            List<Client> clients;
            try
            {
                sql = string.Format(@" SELECT
                    c.Id,
                    c.Name,
                    cm.MemberID,
                    c.TokenLifeTime,
                    c.AllowedOrigin,
                    cm.SecretKey,
	                r.Name as RoleName
                    FROM
                    Client AS c
                    JOIN ClientMember AS cm ON
                    C.ID = CM.ClientID
                    JOIN ClientMemberRole AS cmr ON C.ID = CMR.ClientId
                    JOIN [Role] AS r ON r.Id=cmr.RoleId
                    WHERE C.ID = @clientId AND CM.MemberID = @memberId  AND c.[Active]=1 ");
                using (DataAccessHelper dataAccessHelper = new DataAccessHelper(ConnectionString, Credential))
                {
                    dsClients = dataAccessHelper.GetData(sql, new System.Data.IDbDataParameter[] {
                        new SqlParameter("@clientId",clientId),
                         new SqlParameter("@memberId",memberId),
                    });
                }

                using (IDataMapper<Client> clientMapper = new ClientMapper())
                {
                    clients = clientMapper.MapMultiple(dsClients);
                }

                return await Task.FromResult(clients);
            }

            finally
            {
                dsClients = null;
                sql = null;
            }
        }
        public async Task<Client> GetClient(string clientId, string memberId)
        {
            List<Client> clients =  await GetAllClients(clientId, memberId);              
            return clients.FirstOrDefault(c => c.ID == clientId && c.Members.FirstOrDefault(m =>  m.MemberId == memberId) != null);
        }

        public async Task<List<Lecture>> GetAllUnsynchedRecords(DateTime LastSychDate)
        {
            string sql = string.Empty;
            DataSet dsClients;
            List<Lecture> lecture;
            try
            {
                sql = string.Format(@" 
                    SELECT S.ID AS [STUDENT_ID] ,L.ID AS [LECTURE_ID],LR.ID AS [LECTURER_ID] ,
                    S.NAME AS [STUDENT_NAME],L.NAME AS [LECTURE_NAME],LR.NAME  AS [LECTURER_NAME],
                    L.SynchDate AS [LastSynchDate],
                    S.CREATEDDATE AS [STUDENT_CREATEDDATE],S.SYNCHDATE AS [STUDENT_SYNCHDATE],
                    LR.CREATEDDATE AS [LECTURER_CREATEDDATE],LR.SYNCHDATE AS [LECTURER_SYNCHDATE],
                    L.CREATEDDATE AS [LECTURE_CREATEDDATE],L.SYNCHDATE AS [LECTURE_SYNCHDATE]

FROM LECTURER_STUDENT LS
JOIN LECTURER LR ON LR.ID=LS.LECTURER_ID
JOIN LECTURE L ON L.ID=LR.LECTURE_ID
JOIN STUDENT S ON S.ID=LS.STUDENT_ID
WHERE L.SynchDate >@LastSychDate");
                using (DataAccessHelper dataAccessHelper = new DataAccessHelper(ConnectionString, Credential))
                {
                    dsClients = dataAccessHelper.GetData(sql, new System.Data.IDbDataParameter[] {
                        new SqlParameter("@LastSychDate",LastSychDate),
                          
                    });
                }
                using (IDataMapper<Lecture> clientMapper = new UnSynchedRecordMapper())
                {
                    lecture = clientMapper.MapMultiple(dsClients);
                }
                return await Task.FromResult(lecture);
            }
            finally
            {
                dsClients = null;
                sql = null;
            }
        }

        public void MergedRecordsFromClientToServer(Lecture lecture)
        {
            /*
             * STEP 01 : MERGE LECTURE
             * STEP 02 : MERGE LECTURER
             * STEP 03 : MERGE STUDENT
             * STEP 04 : MERGE STUDENT_LECTURER RELATIONSHIP
             */
            string sql = string.Empty;
            sql = string.Format(@" IF NOT EXISTS ( SELECT TOP 1 (1) FROM LECTURE WHERE ID='{0}' )
                                    BEGIN
                                    INSERT INTO LECTURE(ID,NAME,CREATEDDATE,SYNCHDATE)
                                    VALUES('{0}','{1}','{2}','{3}')
                                    END
                                    ELSE 
                                    BEGIN
                                    UPDATE LECTURE SET NAME='{1}',SYNCHDATE='{3}'
                                    WHERE ID='{0}' 
                                    END

                                    IF NOT EXISTS ( SELECT TOP 1 (1) FROM LECTURER WHERE ID='{4}' )
                                    BEGIN
                                    INSERT INTO LECTURER(ID,NAME,LECTURE_ID,CREATEDDATE,SYNCHDATE)
                                    VALUES('{4}','{5}','{0}','{6}','{7}')
                                    END
                                    ELSE 
                                    BEGIN
                                    UPDATE LECTURER SET NAME='{5}',SYNCHDATE='{7}',LECTURE_ID='{0}'
                                    WHERE ID='{4}' 
                                    END
                                    ", lecture.Id,lecture.Name,lecture.CreatedDate,lecture.SynchDate, lecture.Lecturer.Id, lecture.Lecturer.Name, lecture.Lecturer.CreatedDate, lecture.Lecturer.SynchDate);

            using (DataAccessHelper dataAccessHelper = new DataAccessHelper(ConnectionString, Credential))
            {
                  dataAccessHelper.ExecuteNonQuery(sql, new System.Data.IDbDataParameter[] {
                    });
            }
            foreach (Student s in lecture.Lecturer.Students)
            {

                sql = string.Empty;
                sql = string.Format(@" IF NOT EXISTS ( SELECT TOP 1 (1) FROM STUDENT WHERE ID='{0}' )
                                    BEGIN
                                    INSERT INTO STUDENT(ID,NAME,CREATEDDATE,SYNCHDATE)
                                    VALUES('{0}','{1}','{2}','{3}')
                                    END
                                    ELSE 
                                    BEGIN
                                    UPDATE LECTURE SET NAME='{1}',SYNCHDATE='{3}'
                                    WHERE ID='{0}' 
                                    END
                                    ", s.Id, s.Name, s.CreatedDate, s.SynchDate);
                using (DataAccessHelper dataAccessHelper = new DataAccessHelper(ConnectionString, Credential))
                {
                    dataAccessHelper.ExecuteNonQuery(sql, new System.Data.IDbDataParameter[] {
                    });
                }
            }

            foreach (Student s in lecture.Lecturer.Students)
            {

                sql = string.Empty;
                sql = string.Format(@" IF NOT EXISTS ( SELECT TOP 1 (1) FROM LECTURER_STUDENT WHERE LECTURER_ID='{1}' AND STUDENT_ID ='{2}' )
                                    BEGIN
                                    INSERT INTO LECTURER_STUDENT(ID,LECTURER_ID,STUDENT_ID)
                                    VALUES('{0}','{1}','{2}')
                                    END
                                    ELSE 
                                    BEGIN
                                    UPDATE LECTURER_STUDENT SET LECTURER_ID='{1}',STUDENT_ID='{2}'
                                    WHERE LECTURER_ID='{1}' AND STUDENT_ID='{2}' 
                                    END
                                    ", Guid.NewGuid(), lecture.Id,s.Id);
                using (DataAccessHelper dataAccessHelper = new DataAccessHelper(ConnectionString, Credential))
                {
                    dataAccessHelper.ExecuteNonQuery(sql, new System.Data.IDbDataParameter[] {
                    });
                }
            }

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
