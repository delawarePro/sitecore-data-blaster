using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Summary;

namespace Sitecore.DataBlaster.Util
{
    public static class ISearchIndexExtensions
    {
        /// <summary>
        /// Sitecore moved summary from the interface to the <see cref="AbstractSearchIndex"/> base class.
        /// but we only have access to the index from the <see cref="ContentSearchManager"/>.
        /// </summary>
        public static ISearchIndexSummary RequestSummary(this ISearchIndex index)
        {
            return index is IIndexSummarySource summarSource
                ? summarSource.GetClient().RequestSummary()
                : null;
        }
    }
}