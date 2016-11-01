// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Operator Definition class.
	/// </summary>
	[DataContract]
	public class OperatorDefinition
	{
		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = false, IsRequired = false )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///		Should the name be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeName( )
	    {
			return Name != null;
	    }

		/// <summary>
		///     Gets or sets the operator.
		/// </summary>
		/// <value>
		///     The operator.
		/// </value>
		[DataMember( Name = "oper", EmitDefaultValue = false, IsRequired = false )]
		public string Operator
		{
			get;
			set;
		}

		/// <summary>
		///		Should the operator be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOperator( )
	    {
			return Operator != null;
	    }

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		[DataMember( Name = "type", EmitDefaultValue = false, IsRequired = false )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

		/// <summary>
		///     Gets or sets the operator count.
		/// </summary>
		/// <value>
		///     The operator count.
		/// </value>
		[DataMember( Name = "argcount", EmitDefaultValue = false, IsRequired = false )]
		public long OperatorCount
		{
			get;
			set;
		}

		/// <summary>
		///		Should the operator count be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOperatorCount( )
	    {
			return OperatorCount != 0;
	    }
	}
}