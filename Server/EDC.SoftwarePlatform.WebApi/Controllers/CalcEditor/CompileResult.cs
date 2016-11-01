// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Database;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
	/// <summary>
	///     Compile Result class.
	/// </summary>
	[DataContract]
	public class CompileResult
	{
		/// <summary>
		///     Gets or sets the type of the result.
		/// </summary>
		/// <value>
		///     The type of the result.
		/// </value>
		[IgnoreDataMember]
		public DataType ResultType
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
		[DataMember( Name = "entityTypeId", EmitDefaultValue = true, IsRequired = false )]
		public long EntityTypeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the result type string.
		/// </summary>
		/// <value>
		///     The result type string.
		/// </value>
		[DataMember( Name = "resultType", EmitDefaultValue = true, IsRequired = false )]
		public string ResultTypeString
		{
			get
			{
				return ResultType.ToString( );
			}
		}

		/// <summary>
		///     Gets or sets the error message.
		/// </summary>
		/// <value>
		///     The error message.
		/// </value>
		[DataMember( Name = "error", EmitDefaultValue = true, IsRequired = false )]
		public string ErrorMessage
		{
			get;
			set;
		}
	}
}