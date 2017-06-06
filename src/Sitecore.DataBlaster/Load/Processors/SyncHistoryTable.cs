using System.Diagnostics;
using Sitecore.DataBlaster.Load.Sql;

namespace Sitecore.DataBlaster.Load.Processors
{
    public class SyncHistoryTable : ISyncInTransaction
    {
        public void Process(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext)
        {
            if (!loadContext.UpdateHistory.GetValueOrDefault()) return;
	        if (loadContext.ItemChanges.Count == 0) return;

			var stopwatch = Stopwatch.StartNew();

            var sql = sqlContext.GetEmbeddedSql(loadContext, "Sql.09.UpdateHistory.sql");
	        sqlContext.ExecuteSql(sql, 
                commandProcessor: cmd => cmd.Parameters.AddWithValue("@UserName", Sitecore.Context.User.Name));

            loadContext.Log.Info($"Updated history: {(int) stopwatch.Elapsed.TotalSeconds}s");
        }
    }
}