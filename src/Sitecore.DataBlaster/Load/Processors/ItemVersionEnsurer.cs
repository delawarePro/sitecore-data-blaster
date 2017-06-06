using System.Collections.Generic;

namespace Sitecore.DataBlaster.Load.Processors
{
    public class ItemVersionEnsurer : IItemProcessor
    {
        public IEnumerable<BulkLoadItem> Process(BulkLoadContext context, IEnumerable<BulkLoadItem> items)
        {
            // Statistic fields need to be present to represent the item versions.
            foreach (var item in items)
            {
                item.EnsureLanguageVersions(forceUpdate: context.ForceUpdates);
                yield return item;
            }
        }
    }
}