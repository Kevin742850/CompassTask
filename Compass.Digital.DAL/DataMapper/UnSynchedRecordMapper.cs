
using Compass.Digital.BO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Compass.Digital.DAL
{

    public class UnSynchedRecordMapper : IDataMapper<Lecture>
    {
        public Lecture MapSingle(DataSet ds)
        {
            throw new NotImplementedException();

        }
        public List<Lecture> MapMultiple(DataSet ds)
        {
            List<Lecture> Lecture = new List<Lecture>();
            Lecture lecture = null;
            Lecturer lecturer = null;
            Student student = null;
            try
            {
                string strColumn = @"LECTURE_ID,LECTURE_NAME,LECTURE_CREATEDDATE,LECTURE_SYNCHDATE";
                foreach (DataRow row in ds.Tables[0].DefaultView.ToTable(true, strColumn.Replace(" ", "").Replace("\t", "").Replace("\r\n", "").Split(',')).Select())
                {
                    lecture = new Lecture();
                    lecture.Id = Convert.ToString(row["LECTURE_ID"]);
                    lecture.Name = Convert.ToString(row["LECTURE_NAME"]);
                    lecture.CreatedDate = Convert.ToDateTime(row["LECTURE_CREATEDDATE"]);
                    lecture.SynchDate = Convert.ToDateTime(row["LECTURE_SYNCHDATE"]);
                    strColumn = @"LECTURER_ID,LECTURER_NAME,LECTURE_ID,LECTURER_CREATEDDATE,LECTURER_SYNCHDATE";
                    foreach (DataRow lecturerRow in ds.Tables[0].DefaultView.ToTable(true, strColumn.Replace(" ", "").Replace("\t", "").Replace("\r\n", "").Split(',')).
                                        Select("LECTURE_ID=" + "'" + Convert.ToString(row["LECTURE_ID"]) + "'"))
                    {
                        lecturer = new Lecturer();
                        lecturer.Id = Convert.ToString(lecturerRow["LECTURER_ID"]);
                        lecturer.Name = Convert.ToString(lecturerRow["LECTURER_NAME"]);
                        lecturer.CreatedDate = Convert.ToDateTime(lecturerRow["LECTURER_CREATEDDATE"]);
                        lecturer.SynchDate = Convert.ToDateTime(lecturerRow["LECTURER_SYNCHDATE"]);

                        strColumn = @"STUDENT_ID,STUDENT_NAME,LECTURE_ID,STUDENT_CREATEDDATE,STUDENT_SYNCHDATE";
                        foreach (DataRow stuentRow in ds.Tables[0].DefaultView.ToTable(true, strColumn.Replace(" ", "").Replace("\t", "").Replace("\r\n", "").Split(',')).
                                        Select("LECTURE_ID=" + "'" + Convert.ToString(row["LECTURE_ID"]) + "'"))
                        {
                            student = new Student();
                            student.Id = Convert.ToString(stuentRow["STUDENT_ID"]);
                            student.Name = Convert.ToString(stuentRow["STUDENT_NAME"]);
                            student.CreatedDate = Convert.ToDateTime(stuentRow["STUDENT_CREATEDDATE"]);
                            student.SynchDate = Convert.ToDateTime(stuentRow["STUDENT_SYNCHDATE"]);
                            lecturer.Students.Add(student);
                        }
                        lecture.Lecturer = lecturer;
                    }
                    Lecture.Add(lecture);
                }
            }            
            finally
            {

            }

            return Lecture;
           
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