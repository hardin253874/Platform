// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Model;

namespace ReadiNow.Reporting
{
	/// <summary>
	///     Represents a core report object.
	/// </summary>
	[DataContract]
	public class ReportObject
	{
		/// <summary>
		///     Initializes a new instance of the ReportObject class.
		/// </summary>
		public ReportObject( )
		{
			DataViews = new List<ReportDataView>( );
		}


		///// <summary>
		///// </summary>
		///// <value>
		///// The id.
		///// </value>


		/// <summary>
		///     Gets or sets the analyzer fields.
		/// </summary>
		/// <value>
		///     The analyzer fields.
		/// </value>
		[DataMember]
		public IList<ReportAnalyzerField> AnalyzerFields
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the ClientAggregate.
		/// </summary>
		/// <value>
		///     The ClientAggregate.
		/// </value>
		[DataMember]
		public ClientAggregate ClientAggregate
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the console folder.
		/// </summary>
		/// <value>
		///     The shortcut folder.
		/// </value>
		[DataMember]
		public long ConsoleFolder
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the collection of data views associated with this report object.
		/// </summary>
		[DataMember]
		public IList<ReportDataView> DataViews
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the DefaultConsoleFormId of the Report type.
		/// </summary>
		/// <value>
		///     The default console form id.
		/// </value>
		[DataMember]
		public long DefaultConsoleFormId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the default view id.
		/// </summary>
		/// <value>
		///     The default view id.
		/// </value>
		[DataMember]
		public Guid DefaultDataViewId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		[DataMember]
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the drill down report for report.
		/// </summary>
		/// <value>
		///     The drill down report.
		/// </value>
		[DataMember]
		public long DrillDownReport
		{
			get;
			set;
		}


		///// <summary>
		///// Gets or sets a value indicating whether to the default form in report
		///// </summary>
		///// <value>
		/////   <c>true</c> the report form is default form; otherwise, <c>false</c>.
		///// </value>
		//[DataMember]
		//public bool IsDefaultFormInReport { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this instance is default display report.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is default display report; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool IsDefaultDisplayReport
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is default picker report.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is default picker report; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool IsDefaultPickerReport
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to create the report in a console folder.
		/// </summary>
		/// <value>
		///     <c>true</c> to create a report in a folder; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool IsDefaultReport
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the query.
		/// </summary>
		/// <value>
		///     The query.
		/// </value>
		[DataMember]
		public StructuredQuery Query
		{
			get;
			set;
		}

		[DataMember]
		public EntityRef ReportId
		{
			get;
			set;
		}

		#region Properties related to 'ResourceViewer' 

		/// <summary>
		///     Gets or sets a value indicating whether this instance can create.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can create; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool CanCreate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance can create derived types.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can create derived types; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool CanCreateDerivedTypes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide add button].
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide add button]; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool HideAddButton
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide new button].
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide new button]; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool HideNewButton
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide remove button].
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide remove button]; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool HideRemoveButton
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the resource viewer default console form id.
		/// </summary>
		/// <value>
		///     The resource viewer default console form id.
		/// </value>
		[DataMember]
		public long ResourceViewerConsoleFormId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the resource viewer default tablet form id.
		/// </summary>
		/// <value>
		///     The resource viewer default tablet form id.
		/// </value>
		[DataMember]
		public long ResourceViewerTabletFormId
		{
			get;
			set;
		}

		#endregion
	}
}