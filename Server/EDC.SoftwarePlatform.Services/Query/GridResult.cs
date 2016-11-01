// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.SoftwarePlatform.Services.Data;

namespace EDC.SoftwarePlatform.Services.Query
{
	/// <summary>
	///     Container that encapsulates all grid results.
	/// </summary>
	[DataContract]
	public class GridResult
	{
		/// <summary>
		///     Additional column information that doesn't naturally fit anywhere else.
		/// </summary>
		[DataMember]
		public GridResultColumn[ ] AdditionalColumnInfo
		{
			get;
			set;
		}

		/// <summary>
		///     The aggregate tabular data.
		/// </summary>
		[DataMember]
		public DbDataTable AggregateDataTable
		{
			get;
			set;
		}

		/// <summary>
		///     Report chart formatting information.
		/// </summary>
		[DataMember]
		public ChartDataView ChartDataView
		{
			get;
			set;
		}

		/// <summary>
		///     The tabular data.
		/// </summary>
		[DataMember]
		public DbDataTable DbDataTable
		{
			get;
			set;
		}

		/// <summary>
		///     Report grid formatting information.
		/// </summary>
		[DataMember]
		public GridReportDataView GridReportDataView
		{
			get;
			set;
		}

		/// <summary>
		///     Report matrix formatting information.
		/// </summary>
		[DataMember]
		public MatrixReportDataView MatrixReportDataView
		{
			get;
			set;
		}
	}
}