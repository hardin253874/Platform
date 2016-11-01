// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
	/// <summary>
	///     Expression Parameter class.
	/// </summary>
	[DataContract]
	public class ExpressionParameter
	{
		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = true, IsRequired = true )]
		public string Name
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
		[DataMember( Name = "type", EmitDefaultValue = true, IsRequired = true )]
		public string TypeName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is list.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is list; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "isList", EmitDefaultValue = true, IsRequired = false )]
		public bool IsList
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity type identifier.
		/// </summary>
		/// <value>
		///     The entity type identifier.
		/// </value>
		[DataMember( Name = "entityTypeId", EmitDefaultValue = false, IsRequired = false )]
		public string EntityTypeId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the entity type identifier be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeEntityTypeId( )
	    {
		    return EntityTypeId != null;
	    }

		/// <summary>
		///     Gets or sets the string value.
		/// </summary>
		/// <value>
		///     The string value.
		/// </value>
		[DataMember( Name = "value", EmitDefaultValue = false, IsRequired = false )]
		public string StringValue
		{
			get;
			set;
		}

		/// <summary>
		/// Should the string value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeStringValue( )
	    {
		    return StringValue != null;
	    }

		/// <summary>
		///     Gets or sets the expr.
		/// </summary>
		/// <value>
		///     The expr.
		/// </value>
		[DataMember( Name = "expr", EmitDefaultValue = false, IsRequired = false )]
		public string Expr
		{
			get;
			set;
		}

		/// <summary>
		///		Should the expression be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeExpr( )
	    {
		    return Expr != null;
	    }
	}
}