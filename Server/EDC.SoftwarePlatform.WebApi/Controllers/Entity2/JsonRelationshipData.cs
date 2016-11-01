// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Relationship data for an individual entity.
	/// </summary>
	[DataContract]
	public class JsonRelationshipData
	{
		/// <summary>
		///     List contains either IDs (as longs) or JsonRelationshipInstance or mixed.
		/// </summary>
		/// <value>
		///     The forward.
		/// </value>
		[DataMember( Name = "f", EmitDefaultValue = false )]
		public List<object> Forward
		{
			get;
			set;
		}

		/// <summary>
		///		Should the forward value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeForward( )
	    {
			return Forward != null;
	    }

		/// <summary>
		///     List contains either IDs (as longs) or JsonRelationshipInstance or mixed.
		/// </summary>
		/// <value>
		///     The reverse.
		/// </value>
		[DataMember( Name = "r", EmitDefaultValue = false )]
		public List<object> Reverse
		{
			get;
			set;
		}

		/// <summary>
		///		Should the reverse value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReverse( )
	    {
			return Reverse != null;
	    }
	}
}