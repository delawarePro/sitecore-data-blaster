using System.Diagnostics;
using Sitecore.Configuration;
using Sitecore.DataBlaster.Load.Sql;

namespace Sitecore.DataBlaster.Load.Processors
{
    public class SyncHistoryTable : ISyncInTransaction
    {
        public void Process(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext)
        {
            if (!loadContext.UpdateHistory.GetValueOrDefault()) return;
            if (loadContext.ItemChanges.Count == 0) return;

            // In Sitecore 9, history engine is disabled by default
            if (!HistoryEngineEnabled(loadContext))
            {
                loadContext.Log.Warn($"Skipped updating history because history engine is not enabled");
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            var sql = sqlContext.GetEmbeddedSql(loadContext, "Sql.09.UpdateHistory.sql");
            sqlContext.ExecuteSql(sql,
                commandProcessor: cmd => cmd.Parameters.AddWithValue("@UserName", Sitecore.Context.User.Name));

            loadContext.Log.Info($"Updated history: {(int) stopwatch.Elapsed.TotalSeconds}s");
        }

        private bool HistoryEngineEnabled(BulkLoadContext context)
        {
            var db = Factory.GetDatabase(context.Database, true);
            return db.Engines.HistoryEngine.Storage != null;
        }
    }
}