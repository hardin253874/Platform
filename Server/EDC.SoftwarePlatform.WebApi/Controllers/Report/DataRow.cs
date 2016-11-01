// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class DataRow.
	/// </summary>
	[DataContract]
	public class DataRow
	{
		/// <summary>
		///     Gets or sets the entity unique identifier.
		/// </summary>
		/// <value>The entity unique identifier.</value>
		[DataMember( Name = "eid", EmitDefaultValue = false, IsRequired = false )]
		public long EntityId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the entity identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityId( )
		{
			return EntityId != 0;
		}

		/// <summary>
		///     Gets or sets the values.
		/// </summary>
		/// <value>The values.</value>
		[DataMember( Name = "values", EmitDefaultValue = false, IsRequired = true )]
		public List<CellValue> Values
		{
			get;
			set;
		}

		/// <summary>
		///		Should the values be serialize.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValues( )
		{
			return Values != null;
		}
	}
}