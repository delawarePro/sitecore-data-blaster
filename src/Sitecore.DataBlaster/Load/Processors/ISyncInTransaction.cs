using Sitecore.DataBlaster.Load.Sql;

namespace Sitecore.DataBlaster.Load.Processors
{
    public interface ISyncInTransaction
    {
        void Process(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext);
    }
}