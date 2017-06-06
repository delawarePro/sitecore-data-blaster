namespace Unicorn.DataBlaster.Sync
{
	/// <summary>
	/// Parameters that allow tweaking the data blaster integration.
	/// </summary>
    public class DataBlasterParameters
    {
		/// <summary>
		/// Temporarily disable the data blaster integration.
		/// </summary>
	    public bool DisableDataBlaster { get; set; }

		/// <summary>
		/// Flag to only stage data in database, used for debugging the data blaster.
		/// </summary>
		public bool StageDataWithoutProcessing { get; set; }
    }
}