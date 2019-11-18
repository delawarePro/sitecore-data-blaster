using System;
using log4net;
using log4net.spi;

namespace Unicorn.DataBlaster.Logging
{
    /// <summary>
    /// Logs both to standard log4net target and Unicorn target.
    /// </summary>
    public class SitecoreAndUnicornLog : ILog
    {
        private readonly Unicorn.Logging.ILogger _unicornLogger;
        private readonly ILog _sitecoreLog;

        public SitecoreAndUnicornLog(ILog sitecoreLog, Unicorn.Logging.ILogger unicornLogger)
        {
            if (sitecoreLog == null) throw new ArgumentNullException(nameof(sitecoreLog));
            if (unicornLogger == null) throw new ArgumentNullException(nameof(unicornLogger));

            _sitecoreLog = sitecoreLog;
            _unicornLogger = unicornLogger;
        }

        #region Forward to Sitecore and Unicorn log

        public bool IsDebugEnabled => _sitecoreLog.IsDebugEnabled;

        public bool IsInfoEnabled => _sitecoreLog.IsInfoEnabled;

        public bool IsWarnEnabled => _sitecoreLog.IsWarnEnabled;

        public bool IsErrorEnabled => _sitecoreLog.IsErrorEnabled;

        public bool IsFatalEnabled => _sitecoreLog.IsFatalEnabled;

        public ILogger Logger => _sitecoreLog.Logger;

        public void Debug(object message)
        {
            if (message == null) return;

            _sitecoreLog.Debug(message);
            _unicornLogger.Debug(message.ToString());
        }

        public void Debug(object message, Exception t)
        {
            if (message == null) return;

            _sitecoreLog.Debug(message, t);
            _unicornLogger.Debug(message.ToString());
        }

        public void Info(object message)
        {
            if (message == null) return;

            _sitecoreLog.Info(message);
            _unicornLogger.Info(message.ToString());
        }

        public void Info(object message, Exception t)
        {
            if (message == null) return;

            _sitecoreLog.Info(message, t);
            _unicornLogger.Info(message.ToString());
        }

        public void Warn(object message)
        {
            if (message == null) return;

            _sitecoreLog.Warn(message);
            _unicornLogger.Warn(message.ToString());
        }

        public void Warn(object message, Exception t)
        {
            if (message == null) return;

            _sitecoreLog.Warn(message, t);
            _unicornLogger.Warn(message.ToString());
        }

        public void Error(object message)
        {
            if (message == null) return;

            _sitecoreLog.Error(message);
            _unicornLogger.Error(message.ToString());
        }

        public void Error(object message, Exception t)
        {
            if (message == null) return;

            _sitecoreLog.Error(message, t);
            _unicornLogger.Error(message.ToString());
        }

        public void Fatal(object message)
        {
            if (message == null) return;

            _sitecoreLog.Fatal(message);
            _unicornLogger.Error(message.ToString());
        }

        public void Fatal(object message, Exception t)
        {
            if (message == null) return;

            _sitecoreLog.Fatal(message, t);
            _unicornLogger.Error(message.ToString());
        }

        #endregion
    }
}