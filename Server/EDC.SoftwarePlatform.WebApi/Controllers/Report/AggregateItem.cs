// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Aggregate Item class.
	/// </summary>
	[DataContract]
	public class AggregateItem
	{
		/// <summary>
		///     Gets or sets the aggregate value.
		/// </summary>
		/// <value>
		///     The aggregate value.
		/// </value>
		[DataMember( Name = "value", EmitDefaultValue = false, IsRequired = false )]
		public string AggregateValue
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aggregate value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAggregateValue( )
		{
			return AggregateValue != null;
		}

		/// <summary>
		///     Gets or sets the aggregate values.
		/// </summary>
		/// <value>
		///     The aggregate values.
		/// </value>
		[DataMember( Name = "values", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, string> AggregateValues
		{
			get;
			set;
		}

		/// <summary>
		///		Should the aggregate values be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAggregateValues( )
		{
			return AggregateValues != null;
		}
	}
}