// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
	/// <summary>
	///     Import Result Info object
	/// </summary>
	[DataContract]
	public class ImportResultInfo
	{
        /// <summary>
        ///     The status of the import run.
        /// </summary>
        [DataMember( Name = "importStatus" )]
        public ImportStatus ImportStatus
        {
            get;
            set;
        }

        /// <summary>
        ///     Contains any messages.
        /// </summary>
        [DataMember( Name = "importMessages" )]
        public string ImportMessages
        {
            get;
            set;
        }

        /// <summary>
        ///     Number of records processed successfully.
        /// </summary>
        [DataMember( Name = "recordsSucceeded" )]
        public int RecordsSucceeded
        {
            get;
            set;
        }

        /// <summary>
        ///     Number of records failed.
        /// </summary>
        [DataMember( Name = "recordsFailed" )]
        public int RecordsFailed
        {
            get;
            set;
        }

        /// <summary>
        ///     Total number of records available.
        /// </summary>
        [DataMember( Name = "recordsTotal" )]
        public int RecordsTotal
        {
            get;
            set;
        }
    }
}