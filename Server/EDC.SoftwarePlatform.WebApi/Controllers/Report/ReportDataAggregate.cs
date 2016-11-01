// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Report Data Aggregate class.
	/// </summary>
	[DataContract]
	public class ReportDataAggregate
	{
		/// <summary>
		///     Gets or sets the group bitmap.
		/// </summary>
		/// <value>
		///     The group bitmap.
		/// </value>
		[DataMember( Name = "map", EmitDefaultValue = true, IsRequired = false )]
		public long GroupBitmap
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the total.
		/// </summary>
		/// <value>
		///     The total.
		/// </value>
		[DataMember( Name = "total", EmitDefaultValue = false, IsRequired = false )]
		public long? Total
		{
			get;
			set;
		}

		/// <summary>
		///		Should the total be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTotal( )
		{
			return Total != null;
		}

		/// <summary>
		///     Gets or sets the group headings.
		/// </summary>
		/// <value>
		///     The group headings.
		/// </value>
		[DataMember( Name = "hdrs", EmitDefaultValue = false, IsRequired = false )]
		public List<Dictionary<long, CellValue>> GroupHeadings
		{
			get;
			set;
		}

		/// <summary>
		///		Should the group headings be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeGroupHeadings( )
		{
			return GroupHeadings != null;
		}

		/// <summary>
		///     Gets or sets the aggregates.
		/// </summary>
		/// <value>
		///     The aggregates.
		/// </value>
		[DataMember( Name = "aggs", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, List<AggregateItem>> Aggregates
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
	}
}