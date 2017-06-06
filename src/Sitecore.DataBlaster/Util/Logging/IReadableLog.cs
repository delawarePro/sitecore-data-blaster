using System.Collections.Generic;
using log4net;
using log4net.spi;

namespace Sitecore.DataBlaster.Util.Logging
{
    public interface IReadableLog : ILog
    {
        IEnumerable<LoggingEvent> ReadLog();
    }
}