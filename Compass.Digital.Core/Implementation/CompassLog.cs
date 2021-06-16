using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compass.Digital.Core
{
    public class CompassLog<T> : ILog<T>
    {

        private static ILogger logger = LogManager.GetCurrentClassLogger();
        ILogger jsonLogger = LogManager.GetLogger("jsonLogger");
        public CompassLog()
        {
        }
        public void Information(string message)
        {
            logger.Info(message);
        }

        public void Warning(string message)
        {
            logger.Warn(message);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }
        public void Error(Exception exc, string message, params object[] args)
        {
            logger.Error(exc, message, args);
        }
        public void WriteJsonLog(string message)
        {
            jsonLogger.Info(message);
        }
    }

}
