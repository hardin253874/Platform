// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.ImportSpreadsheet
{
	/// <summary>
	///     Information about an import run
	/// </summary>
	public class ImportResultInfo
	{
	    /// <summary>
	    ///     The status of the import run.
	    /// </summary>
	    public ImportStatus ImportStatus
	    {
	        get;
	        set;
	    }

	    /// <summary>
        ///     Contains any messages.
        /// </summary>
        public string ImportMessages
		{
			get;
			set;
		}

		/// <summary>
		///     Number of records found.
		/// </summary>
		public int RecordsTotal
        {
			get;
			set;
        }

        /// <summary>
        ///     Number of records processed successfully.
        /// </summary>
        public int RecordsSucceeded
        {
            get;
            set;
        }

        /// <summary>
        ///     Number of records failed.
        /// </summary>
        public int RecordsFailed
        {
            get;
            set;
        }
    }
}