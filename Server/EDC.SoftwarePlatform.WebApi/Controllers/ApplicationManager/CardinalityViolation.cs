// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ApplicationManager
{
	/// <summary>
	///     Cardinality violation.
	/// </summary>
	[DataContract( Name = "cardViol" )]
	public class CardinalityViolation
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CardinalityViolation" /> class.
		/// </summary>
		public CardinalityViolation( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CardinalityViolation" /> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public CardinalityViolation( string type, string from, string to ) : this( )
		{
			Type = type;
			From = from;
			To = to;
		}

		/// <summary>
		///     Gets or sets from.
		/// </summary>
		/// <value>
		///     From.
		/// </value>
		[DataMember( Name = "from", IsRequired = true, EmitDefaultValue = true )]
		public string From
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets to.
		/// </summary>
		/// <value>
		///     To.
		/// </value>
		[DataMember( Name = "to", IsRequired = true, EmitDefaultValue = true )]
		public string To
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		[DataMember( Name = "type", IsRequired = true, EmitDefaultValue = true )]
		public string Type
		{
			get;
			set;
		}
	}
}