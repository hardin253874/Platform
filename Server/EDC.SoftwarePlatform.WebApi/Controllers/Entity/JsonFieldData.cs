// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Field Data class.
	/// </summary>
	[DataContract]
	public class JsonFieldData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonFieldData" /> class.
		/// </summary>
		public JsonFieldData( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonFieldData" /> class.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="context">The context.</param>
		public JsonFieldData( FieldData field, JsonEntityQueryResult context )
		{
			FieldId = context.GetEntityRef( field.FieldId ).Id;
			Value = field.Value.ValueString;
			TypeName = field.Value.Type.GetDisplayName( );
		}

		/// <summary>
		///     Gets or sets the string value.
		/// </summary>
		/// <value>
		///     The string value.
		/// </value>
		[DataMember( Name = "value" )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the field identifier.
		/// </summary>
		/// <value>
		///     The field identifier.
		/// </value>
		[DataMember( Name = "fieldId" )]
		public long FieldId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		[DataMember( Name = "typeName" )]
		public string TypeName
		{
			get;
			set;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return string.Format( "{0}: {1}){2}", FieldId, Value ?? "(null)", TypeName ?? "(no type)" );
		}
	}
}