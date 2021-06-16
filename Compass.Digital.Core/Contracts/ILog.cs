using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.Core
{
    public interface ILog<T>
    {
        void Information(string message);
        void Warning(string message);
        void Debug(string message);
        void Error(string message);
        void Error(Exception exc, string message, params object[] args);
        void WriteJsonLog(string message);
    }
}
