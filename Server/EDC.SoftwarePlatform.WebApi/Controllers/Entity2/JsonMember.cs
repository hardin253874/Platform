// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Field metadata stored in the members section.
	/// </summary>
	[DataContract]
	public class JsonMember
	{
		/// <summary>
		///     Gets or sets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		/// <remarks>Used for fields only</remarks>
		[DataMember( Name = "alias", EmitDefaultValue = false )]
		public string Alias
		{
			get;
			set;
		}

		/// <summary>
		///		Should the alias value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAlias( )
	    {
			return Alias != null;
	    }

		/// <summary>
		///     Gets or sets the type of the data.
		/// </summary>
		/// <value>
		///     The type of the data.
		/// </value>
		/// <remarks>Used for fields only</remarks>
		[DataMember( Name = "dt", EmitDefaultValue = false )]
		public string DataType
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type of the data be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDataType( )
	    {
			return DataType != null;
	    }


		/// <summary>
		///     Gets or sets the forward.
		/// </summary>
		/// <value>
		///     The forward.
		/// </value>
		/// <remarks>Used for relationships only</remarks>
		[DataMember( Name = "frel", EmitDefaultValue = false )]
		public JsonRelationshipMember Forward
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
		///     Gets or sets the reverse.
		/// </summary>
		/// <value>
		///     The reverse.
		/// </value>
		/// <remarks>Used for relationships only</remarks>
		[DataMember( Name = "rrel", EmitDefaultValue = false )]
		public JsonRelationshipMember Reverse
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