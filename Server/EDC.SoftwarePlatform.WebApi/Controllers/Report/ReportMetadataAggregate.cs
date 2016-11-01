// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Report MetaData Aggregate class.
	/// </summary>
	[DataContract]
	public class ReportMetadataAggregate
	{
		/// <summary>
		///     Gets or sets a value indicating whether to show grand totals.
		/// </summary>
		/// <value>
		///     <c>true</c> if showing grand totals; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "showgt", EmitDefaultValue = false, IsRequired = false )]
		public bool ShowGrandTotals
		{
			get;
			set;
		}

		/// <summary>
		///		Should the show grand totals be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeShowGrandTotals( )
		{
			return ShowGrandTotals;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show sub totals.
		/// </summary>
		/// <value>
		///     <c>true</c> if showing sub totals; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "showst", EmitDefaultValue = false, IsRequired = false )]
		public bool ShowSubTotals
		{
			get;
			set;
		}

		/// <summary>
		///		Should the show sub totals be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeShowSubTotals( )
		{
			return ShowSubTotals;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show the count.
		/// </summary>
		/// <value>
		///     <c>true</c> if showing the count; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "showcnt", EmitDefaultValue = false, IsRequired = false )]
		public bool ShowCount
		{
			get;
			set;
		}

		/// <summary>
		///		Should the show count be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeShowCount( )
		{
			return ShowCount;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show group labels.
		/// </summary>
		/// <value>
		///     <c>true</c> if showing group labels; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "showlbl", EmitDefaultValue = false, IsRequired = false )]
		public bool ShowGroupLabel
		{
			get;
			set;
		}

		/// <summary>
		///		Should the show group label be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeShowGroupLabel( )
		{
			return ShowGroupLabel;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show option labels.
		/// </summary>
		/// <value>
		///     <c>true</c> if showing option labels; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "showoplbl", EmitDefaultValue = true, IsRequired = true )]
		public bool ShowOptionLabel
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the groups.
		/// </summary>
		/// <value>
		///     The groups.
		/// </value>
		[DataMember( Name = "groups", EmitDefaultValue = false, IsRequired = false )]
		public List<Dictionary<long, GroupingDetail>> Groups
		{
			get;
			set;
		}

		/// <summary>
		///		Should the groups be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeGroups( )
		{
			return Groups != null;
		}

		/// <summary>
		///     Gets or sets the aggregates.
		/// </summary>
		/// <value>
		///     The aggregates.
		/// </value>
		[DataMember( Name = "aggs", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, List<AggregateDetail>> Aggregates
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aggregates be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAggregates( )
		{
			return Aggregates != null;
		}

        /// <summary>
        ///     Gets or sets whether rolled up grand totals, etc, should be included.
        /// </summary>
        /// <value>
        ///     The groups.
        /// </value>
        [DataMember(Name = "rollup", EmitDefaultValue = false, IsRequired = false)]
        public bool IncludeRollup
        {
            get;
            set;
        }

		/// <summary>
		///		Should the include rollup be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIncludeRollup( )
		{
			return IncludeRollup;
		}

        /// <summary>
        ///     If true, do not get individual row data - just get the rollup/aggregate only.
        /// </summary>
        [DataMember( Name = "ignorerows", EmitDefaultValue = false, IsRequired = false )]
        public bool IgnoreRows
        {
            get;
            set;
        }

		/// <summary>
		///		Should the ignore rows be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIgnoreRows( )
		{
			return IgnoreRows;
		}
	}
}