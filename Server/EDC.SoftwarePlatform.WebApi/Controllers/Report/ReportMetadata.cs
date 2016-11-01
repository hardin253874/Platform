// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportMetadata.
	/// </summary>
	[DataContract]
	public class ReportMetadata
	{
		/// <summary>
		///     Gets or sets the report title.
		/// </summary>
		/// <value>The report title.</value>
		[DataMember( Name = "title", EmitDefaultValue = false, IsRequired = true )]
		public string ReportTitle
		{
			get;
			set;
		}

		/// <summary>
		///		Should the report title be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReportTitle( )
		{
			return ReportTitle != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide add button].
		/// </summary>
		/// <value><c>true</c> if [hide add button]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hideadd", EmitDefaultValue = false, IsRequired = false )]
		public bool HideAddButton
		{
			get;
			set;
		}

		/// <summary>
		///		Should the hide add button be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeHideAddButton( )
		{
			return HideAddButton;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide new button].
		/// </summary>
		/// <value><c>true</c> if [hide new button]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hidenew", EmitDefaultValue = false, IsRequired = false )]
		public bool HideNewButton
		{
			get;
			set;
		}

		/// <summary>
		///		Should the hide new button be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeHideNewButton( )
		{
			return HideNewButton;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide delete button].
		/// </summary>
		/// <value><c>true</c> if [hide delete button]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hidedel", EmitDefaultValue = false, IsRequired = false )]
		public bool HideDeleteButton
		{
			get;
			set;
		}

		/// <summary>
		///		Should the hide delete button be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeHideDeleteButton( )
		{
			return HideDeleteButton;
		}


		/// <summary>
		///     Gets or sets a value indicating whether [show action menu].
		/// </summary>
		/// <value><c>true</c> if [show action menu]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hideact", EmitDefaultValue = true, IsRequired = true )]
		public bool HideActionBar
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [hide report header].
		/// </summary>
		/// <value><c>true</c> if [hide report header]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "hideheader", EmitDefaultValue = true, IsRequired = true )]
		public bool HideReportHeader
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the report style.
		/// </summary>
		/// <value>report style</value>
		[DataMember( Name = "style", EmitDefaultValue = false, IsRequired = true )]
		public string ReportStyle
		{
			get;
			set;
		}

		/// <summary>
		///		Should the report style be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReportStyle( )
		{
			return ReportStyle != null;
		}

		/// <summary>
		///     Gets or sets the default form.
		/// </summary>
		/// <value>The default form.</value>
		[DataMember( Name = "dfid", EmitDefaultValue = false, IsRequired = false )]
		public long DefaultFormId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the default form identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDefaultFormId( )
		{
			return DefaultFormId != 0;
		}

        /// <summary>
        /// Gets or sets the resource viewer form identifier.
        /// </summary>
        /// <value>
        /// The resource viewer form identifier.
        /// </value>
        [DataMember(Name = "rvfid", EmitDefaultValue = false, IsRequired = false)]
        public long ResourceViewerFormId { get; set; }

        /// <summary>
        /// Shoulds the serialize resource viewer form identifier.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeResourceViewerFormId()
        {
            return ResourceViewerFormId != 0;
        }

        /// <summary>
        ///     Gets or sets the report views.
        /// </summary>
        /// <value>The report views.</value>
        [DataMember( Name = "rviews", EmitDefaultValue = false, IsRequired = false )]
		public List<string> ReportViews
		{
			get;
			set;
		}

		/// <summary>
		///		Should the report views be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReportViews( )
		{
			return ReportViews != null;
		}

		/// <summary>
		///     Gets or sets the formats for the reported data types.
		/// </summary>
		/// <value>The type formats.</value>
		[DataMember( Name = "typefmtstyle", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, List<string>> TypeConditionalFormatStyles
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type conditional format styles be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTypeConditionalFormatStyles( )
		{
			return TypeConditionalFormatStyles != null;
		}

		/// <summary>
		///     Gets or sets the type value format styles.
		/// </summary>
		/// <value>The type value format styles.</value>
		[DataMember( Name = "typevalfmtstyle", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, List<TypeValueName>> TypeValueFormatStyles
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type value format styles be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTypeValueFormatStyles( )
		{
			return TypeValueFormatStyles != null;
		}

		/// <summary>
		///     Gets or sets the report columns.
		/// </summary>
		/// <value>The report columns.</value>
		[DataMember( Name = "rcols", EmitDefaultValue = false, IsRequired = true )]
		public Dictionary<string, ReportColumn> ReportColumns
		{
			get;
			set;
		}

		/// <summary>
		///		Should the report columns be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReportColumns( )
		{
			return ReportColumns != null;
		}

		/// <summary>
		///     Gets or sets the sort orders.
		/// </summary>
		/// <value>The sort orders.</value>
		[DataMember( Name = "sort", EmitDefaultValue = false, IsRequired = false )]
		public List<ReportSortOrder> SortOrders
		{
			get;
			set;
		}

		/// <summary>
		///		Should the sort orders be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeSortOrders( )
		{
			return SortOrders != null;
		}

		/// <summary>
		///     Gets or sets the choice selections.
		/// </summary>
		/// <value>The choice selections.</value>
		[DataMember( Name = "choice", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, List<ChoiceItemDefinition>> ChoiceSelections
		{
			get;
			set;
		}

		/// <summary>
		///		Should the choice selections be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeChoiceSelections( )
		{
			return ChoiceSelections != null;
		}

		/// <summary>
		///     Gets or sets the inline report pickers.
		/// </summary>
		/// <value>The inline report pickers.</value>
		[DataMember( Name = "inline", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, long> InlineReportPickers
		{
			get;
			set;
		}

		/// <summary>
		///		Should the inline report pickers be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeInlineReportPickers( )
		{
			return InlineReportPickers != null;
		}

		/// <summary>
		///     Gets or sets the analyser columns.
		/// </summary>
		/// <value>The analyser columns.</value>
		[DataMember( Name = "anlcols", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, ReportAnalyserColumn> AnalyserColumns
		{
			get;
			set;
		}

		/// <summary>
		///		Should the analyser columns be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAnalyserColumns( )
		{
			return AnalyserColumns != null;
		}

		/// <summary>
		///     Gets or sets the conditional format rules.
		/// </summary>
		/// <value>The conditional format rules.</value>
		[DataMember( Name = "cfrules", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, ReportColumnConditionalFormat> ConditionalFormatRules
		{
			get;
			set;
		}

		/// <summary>
		///		Should the conditional format rules be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeConditionalFormatRules( )
		{
			return ConditionalFormatRules != null;
		}

		/// <summary>
		///     Gets or sets the value format rules.
		/// </summary>
		/// <value>The value format rules.</value>
		[DataMember( Name = "valrules", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, ReportColumnValueFormat> ValueFormatRules
		{
			get;
			set;
		}

		/// <summary>
		///		Should the value format rules be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValueFormatRules( )
		{
			return ValueFormatRules != null;
		}

		/// <summary>
		///     Gets or sets the format value type selectors.
		/// </summary>
		/// <value>The format value type selectors.</value>
		[DataMember( Name = "valsels", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<string, List<DateTimeValueFormat>> FormatValueTypeSelectors
		{
			get;
			set;
		}

		/// <summary>
		///		Should the format value type selectors be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeFormatValueTypeSelectors( )
		{
			return FormatValueTypeSelectors != null;
		}

		/// <summary>
		///     Gets or sets the aggregate metadata.
		/// </summary>
		/// <value>The aggregate metadata.</value>
		[DataMember( Name = "rmeta", EmitDefaultValue = false, IsRequired = false )]
		public ReportMetadataAggregate AggregateMetadata
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aggregate metadata be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAggregateMetadata( )
		{
			return AggregateMetadata != null;
		}

		/// <summary>
		///     Gets or sets the aggregate (rollup) data.
		/// </summary>
		/// <value>The aggregate data.</value>
		[DataMember( Name = "rdata", EmitDefaultValue = false, IsRequired = false )]
		public List<ReportDataAggregate> AggregateData
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aggregate data be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAggregateData( )
		{
			return AggregateData != null;
		}


        /// <summary>
        ///     Gets or sets the format value type selectors.
        /// </summary>
        /// <value>The format value type selectors.</value>
        [DataMember(Name = "invalid", EmitDefaultValue = false, IsRequired = false)]
        public Dictionary<string, Dictionary<long, string>> InvalidReportInformation
        {
            get;
            set;
        }

		/// <summary>
		///		Should the invalid report information be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeInvalidReportInformation( )
		{
			return InvalidReportInformation != null;
		}

        /// <summary>
        /// Gets or sets the time that the report was last modified.
        /// </summary>
        [DataMember(Name = "modified", EmitDefaultValue = false, IsRequired = false)]
	    public DateTime Modified
	    {
	        get;
            set;
        }

		/// <summary>
		///		Should the modified be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeModified( )
		{
			return Modified != DateTime.MinValue;
		}
	}
}