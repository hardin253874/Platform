// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportParameters.
	/// </summary>
	[DataContract]
	public class ReportParameters
	{
		/// <summary>
		///     Gets or sets the sort columns.
		/// </summary>
		/// <value>The sort columns.</value>
		[DataMember( Name = "sort", EmitDefaultValue = false, IsRequired = false )]
		public List<ReportSortOrder> SortColumns
		{
			get;
			set;
		}

		/// <summary>
		///		Should the sort columns be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeSortColumns( )
	    {
			return SortColumns != null;
	    }

		/// <summary>
		///     Gets or sets the analyser conditions.
		/// </summary>
		/// <value>The analyser conditions.</value>
		[DataMember( Name = "conds", EmitDefaultValue = false, IsRequired = false )]
		public List<AnalyserColumnCondition> AnalyserConditions
		{
			get;
			set;
		}

		/// <summary>
		///		Should the serialize analyser conditions be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAnalyserConditions( )
	    {
			return AnalyserConditions != null;
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
		///     Gets or sets the relationship entities.
		/// </summary>
		/// <value>The relationship entities.</value>
		[DataMember( Name = "relent", EmitDefaultValue = false, IsRequired = false )]
		public RelationshipDetail RelationshipEntities
		{
			get;
			set;
		}

		/// <summary>
		///	 Should the relationship entities be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelationshipEntities( )
	    {
			return RelationshipEntities != null;
	    }

		/// <summary>
		///     Gets or sets the group and aggregate rules.
		/// </summary>
		/// <value>The group and aggregate rules.</value>
		[DataMember( Name = "rollup", EmitDefaultValue = false, IsRequired = false )]
		public ReportMetadataAggregate GroupAggregateRules
		{
			get;
			set;
		}

		/// <summary>
		///		Should the group aggregate rules be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeGroupAggregateRules( )
	    {
			return GroupAggregateRules != null;
	    }

		/// <summary>
		///     Gets or sets the quick search value.
		/// </summary>
		/// <value>The quick search.</value>
		[DataMember( Name = "qsearch", EmitDefaultValue = false, IsRequired = false )]
		public string QuickSearch
		{
			get;
			set;
		}

		/// <summary>
		///		Should the quick search be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeQuickSearch( )
	    {
			return QuickSearch != null;
	    }

        /// <summary>
        ///		Gets or sets the related entity filters.
        /// </summary>
        /// <value>
        /// The related entity filters.
        /// </value>
        [DataMember(Name = "relfilters", EmitDefaultValue = false, IsRequired = false)]
        public List<RelatedEntityFilter> RelatedEntityFilters
        {
            get;
            set;
        }

		/// <summary>
		///		Should the related entity filters be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRelatedEntityFilters( )
	    {
			return RelatedEntityFilters != null;
	    }


        /// <summary>
        ///     Gets or sets the filtered entity identifiers.
        /// </summary>
        /// <value>The filtered entity identifiers.</value>
        [DataMember(Name = "filtereids", EmitDefaultValue = false, IsRequired = false)]
        public List<long> FilteredEntityIdentifiers
        {
            get;
            set;
        }

		/// <summary>
		///		Should the filtered entity identifiers be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeFilteredEntityIdentifiers( )
	    {
			return FilteredEntityIdentifiers != null;
	    }

        /// <summary>
		///     Gets or sets the isReset value.
		/// </summary>
		/// <value>The quick search.</value>
        [DataMember(Name = "isreset", EmitDefaultValue = false, IsRequired = false)]
		public bool IsReset
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is reset be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsReset( )
	    {
			return IsReset;
	    }
        
	}
}