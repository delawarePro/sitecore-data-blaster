using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.DataBlaster.Load.Sql;
using Sitecore.DataBlaster.Util;

namespace Sitecore.DataBlaster.Load.Processors
{
    public class ChangeCacheClearer : IChangeProcessor
    {
	    private readonly CacheUtil _cachUtil;

	    public ChangeCacheClearer(CacheUtil cachehUtil = null)
	    {
		    _cachUtil = cachehUtil ?? new CacheUtil();
	    }

        public void Process(BulkLoadContext loadContext, BulkLoadSqlContext sqlContext, ICollection<ItemChange> changes)
        {
            if (!loadContext.RemoveItemsFromCaches) return;

            var stopwatch = Stopwatch.StartNew();

            // Remove items from database cache.
            // We don't do this within the transaction so that items will be re-read from the committed data.
            var db = Factory.GetDatabase(loadContext.Database, true);
            _cachUtil.RemoveItemsFromCachesInBulk(db, GetCacheClearEntries(loadContext.ItemChanges));

            loadContext.Log.Info($"Caches cleared: {(int)stopwatch.Elapsed.TotalSeconds}s");
        }

        protected virtual IEnumerable<Tuple<ID, ID, string>> GetCacheClearEntries(IEnumerable<ItemChange> itemChanges)
        {
            if (itemChanges == null) yield break;

            foreach (var itemChange in itemChanges)
            {
                yield return new Tuple<ID, ID, string>(ID.Parse(itemChange.ItemId), ID.Parse(itemChange.ParentId), itemChange.ItemPath);

                // Support moved items.
                if (itemChange.ParentId != itemChange.OriginalParentId)
                    yield return new Tuple<ID, ID, string>(ID.Parse(itemChange.ItemId), ID.Parse(itemChange.OriginalParentId), itemChange.ItemPath);
            }
        }
    }
}