using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Compass.Digital.DAL
{
    public interface IDataMapper<T> : IDisposable
    {
        T MapSingle(DataSet ds);
        List<T> MapMultiple(DataSet ds);
    }
}
