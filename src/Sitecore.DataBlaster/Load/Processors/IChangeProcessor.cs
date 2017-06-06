using System.Collections.Generic;
using Sitecore.DataBlaster.Load.Sql;

namespace Sitecore.DataBlaster.Load.Processors
{
    public interface IChangeProcessor
    {
        void Process(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext, ICollection<ItemChange> changes);
    }
}