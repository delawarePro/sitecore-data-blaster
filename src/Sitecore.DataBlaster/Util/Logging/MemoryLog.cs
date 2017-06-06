using System;
using System.Collections.Generic;
using log4net;
using log4net.spi;

namespace Sitecore.DataBlaster.Util.Logging
{
    public class MemoryLog : ILog
    {
        public string LoggerName { get; set; }

        private readonly LinkedList<LoggingEvent> _log = new LinkedList<LoggingEvent>();

        public MemoryLog(string loggerName)
        {
            if (string.IsNullOrEmpty(loggerName)) throw new ArgumentNullException(nameof(loggerName));
            LoggerName = loggerName;
        }

        public void Log(Level level, string message, Exception exception = null)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

            _log.AddLast(new LoggingEvent(new LoggingEventData
            {
                LoggerName = LoggerName,
                Level = level,
                Message = message,
                ExceptionString = exception == null
                    ? null
                    : $"{exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}"
            }));
        }

        public void WriteTo(ILog log)
        {
            foreach (var loggingEvent in _log)
            {
                log.Logger.Log(loggingEvent);
            }
        }

        #region ILog implementation

        public bool IsDebugEnabled => true;
        public bool IsInfoEnabled => true;
        public bool IsWarnEnabled => true;
        public bool IsErrorEnabled => true;
        public bool IsFatalEnabled => true;

        public ILogger Logger { get { throw new NotSupportedException(); } }

        public void Debug(object message)
        {
            Log(Level.DEBUG, message?.ToString());
        }

        public void Debug(object message, Exception t)
        {
            Log(Level.DEBUG, message?.ToString(), t);
        }

        public void Info(object message)
        {
            Log(Level.INFO, message?.ToString());
        }

        public void Info(object message, Exception t)
        {
            Log(Level.INFO, message?.ToString(), t);
        }

        public void Warn(object message)
        {
            Log(Level.WARN, message?.ToString());
        }

        public void Warn(object message, Exception t)
        {
            Log(Level.WARN, message?.ToString(), t);
        }

        public void Error(object message)
        {
            Log(Level.ERROR, message?.ToString());
        }

        public void Error(object message, Exception t)
        {
            Log(Level.ERROR, message?.ToString(), t);
        }

        public void Fatal(object message)
        {
            Log(Level.FATAL, message?.ToString());
        }

        public void Fatal(object message, Exception t)
        {
            Log(Level.FATAL, message?.ToString(), t);
        }

        #endregion
    }
}
