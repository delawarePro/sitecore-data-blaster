using Sitecore.DataBlaster.Load;

namespace Unicorn.DataBlaster.Sync
{
    /// <summary>
    /// Parameters that allow tweaking the data blaster integration.
    /// </summary>
    public class DataBlasterParameters
    {
        /// <summary>
        /// Explicitly enable the data blaster integration.
        /// </summary>
        public bool EnableDataBlaster { get; set; }

        /// <summary>
        /// Flag to only stage data in database, used for debugging the data blaster.
        /// </summary>
        public bool StageDataWithoutProcessing { get; set; }

        /// <summary>
        /// Flag to skip post processing of Unicorn configuration like e.g. users.
        /// </summary>
        public bool SkipUnicornSyncComplete { get; set; }

        /// <summary>
        /// Flag to skip post processing of Unicorn like e.g. publishing.
        /// </summary>
        public bool SkipUnicornSyncEnd { get; set; }
        
        /// <summary>
        /// Force this BulkLoadAction to be used during item extraction instead of the BulkLoadAction derived from the configured evaluator.
        /// </summary>
        public BulkLoadAction? ForceBulkLoadAction { get; set; }
    }
}