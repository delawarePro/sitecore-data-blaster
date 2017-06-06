using System.Collections.Generic;

namespace Sitecore.DataBlaster.Load.Processors
{
    public interface IItemProcessor
    {
        IEnumerable<BulkLoadItem> Process(BulkLoadContext context, IEnumerable<BulkLoadItem> items);
    }
}