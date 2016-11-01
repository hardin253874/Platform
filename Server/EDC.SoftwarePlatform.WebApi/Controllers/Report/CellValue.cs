// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class CellValue.
	/// </summary>
	[DataContract]
	public class CellValue
	{
		/// <summary>
		///     Gets or sets the conditional format index.
		/// </summary>
		/// <value>The conditional format index.</value>
		[DataMember( Name = "cfidx", EmitDefaultValue = false, IsRequired = false )]
		public long? ConditionalFormatIndex
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[DataMember( Name = "val", EmitDefaultValue = false, IsRequired = false )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the values.
		/// </summary>
		/// <value>The values.</value>
		[DataMember( Name = "vals", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, string> Values
		{
			get;
			set;
		}

		/// <summary>
		///     Should the index of the conditional format be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeConditionalFormatIndex( )
		{
			return ConditionalFormatIndex != null;
		}

		/// <summary>
		///     Should the value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValue( )
		{
			return Value != null;
		}

		/// <summary>
		///     Should the values be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValues( )
		{
			return Values != null;
		}
	}
}