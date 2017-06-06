using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.spi;
using Sitecore.Configuration;
using Sitecore.IO;

namespace Sitecore.DataBlaster.Util.Logging
{
    /// <summary>
    /// Wraps a supplied logger and writes all logging events to a dedicated file, so that they can be read back.
    /// </summary>
    public class ReadableLoggerWrapper : IReadableLog
    {
        private readonly ILog _innerLog;
        private readonly Logger _innerLogger;
        private readonly SitecoreLogFileAppender _tempLogAppender;

        public ReadableLoggerWrapper(ILog log, Level readableLevel)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            var logger = (Logger) log.Logger;

            _innerLogger = new RootLogger(readableLevel) { Hierarchy = logger.Hierarchy };
            foreach (var appender in logger.Appenders)
            {
                _innerLogger.AddAppender(appender);
            }

            // Add an appender that writes to a dedicated log file.
            _tempLogAppender = new SitecoreLogFileAppender
            {
                Name = "TempBulkLoadLogger",
                AppendToFile = false,
                File = FileUtil.MapPath($"{Settings.LogFolder}/log.tmp.{Guid.NewGuid():N}.txt"),
                Layout = new PatternLayout("%d|%p|%m%n")
            };
            _tempLogAppender.ActivateOptions();
            _innerLogger.AddAppender(_tempLogAppender);

            _innerLog = new LogImpl(_innerLogger);
        }

        /// <summary>
        /// Cleanup file in destructor, so we don't have to user of this class doesn't have to dispose.
        /// We don't care about releasing soon anyway.
        /// </summary>
        ~ReadableLoggerWrapper()
        {
            try
            {
                var tempFilePath = _tempLogAppender.File;

                _innerLogger.RemoveAppender(_tempLogAppender);
                _tempLogAppender.Close();

                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            catch
            {
            }
        }

        public IEnumerable<LoggingEvent> ReadLog()
        {
            using (var file = File.Open(_tempLogAppender.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(file))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var firstSeparator = line.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    var secondSeparator = line.IndexOf("|", firstSeparator + 1, StringComparison.OrdinalIgnoreCase);
                    yield return new LoggingEvent(new LoggingEventData
                    {
                        TimeStamp = DateTime.Parse(line.Substring(0, firstSeparator)),
                        Level = _innerLogger.Repository.LevelMap[
                                line.Substring(firstSeparator + 1, secondSeparator - firstSeparator - 1)],
                        Message = line.Substring(secondSeparator + 1)
                    });
                }
            }
        }

        #region Forward to _innerLog

        public ILogger Logger => _innerLog.Logger;

        public void Debug(object message)
        {
            _innerLog.Debug(message);
        }

        public void Debug(object message, Exception t)
        {
            _innerLog.Debug(message, t);
        }

        public void Info(object message)
        {
            _innerLog.Info(message);
        }

        public void Info(object message, Exception t)
        {
            _innerLog.Info(message, t);
        }

        public void Warn(object message)
        {
            _innerLog.Warn(message);
        }

        public void Warn(object message, Exception t)
        {
            _innerLog.Warn(message, t);
        }

        public void Error(object message)
        {
            _innerLog.Error(message);
        }

        public void Error(object message, Exception t)
        {
            _innerLog.Error(message, t);
        }

        public void Fatal(object message)
        {
            _innerLog.Fatal(message);
        }

        public void Fatal(object message, Exception t)
        {
            _innerLog.Fatal(message, t);
        }

        public bool IsDebugEnabled => _innerLog.IsDebugEnabled;

        public bool IsInfoEnabled => _innerLog.IsInfoEnabled;

        public bool IsWarnEnabled => _innerLog.IsWarnEnabled;

        public bool IsErrorEnabled => _innerLog.IsErrorEnabled;

        public bool IsFatalEnabled => _innerLog.IsFatalEnabled;

        #endregion
    }
}
