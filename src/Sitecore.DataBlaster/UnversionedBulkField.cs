using System;
using System.IO;

namespace Sitecore.DataBlaster
{
    /// <summary>
    /// Field which has specific values per language but shares the values across versions.
    /// </summary>
    public class UnversionedBulkField : BulkField
    {
        public string Language { get; private set; }

        internal UnversionedBulkField(BulkItem item, Guid id, string language, string value,
            Func<Stream> blob = null, bool isBlob = false, string name = null)
            : base(item, id, value, blob, isBlob, name)
        {
            if (language == null) throw new ArgumentNullException("language");

            this.Language = language;
        }

        internal override BulkField CopyTo(BulkItem targetItem)
            => new UnversionedBulkField(targetItem, Id, Language, Value, Blob, IsBlob, Name);
    }
}