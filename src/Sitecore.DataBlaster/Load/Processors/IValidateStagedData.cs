using Sitecore.DataBlaster.Load.Sql;

namespace Sitecore.DataBlaster.Load.Processors
{
    public interface IValidateStagedData
    {
        bool ValidateLoadStage(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext);
    }
}