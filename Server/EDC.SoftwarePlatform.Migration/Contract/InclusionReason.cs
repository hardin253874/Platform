// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Inclusion reason.
	/// </summary>
	[DataContract]
	public enum InclusionReason
	{
		/// <summary>
		///     Explicitly included.
		/// </summary>
		[EnumMember(Value = "explicit")]
		Explicit = 0,

		/// <summary>
		///     Following a forward relationship.
		/// </summary>
		[EnumMember( Value = "forward" )]
		Forward = 1,

		/// <summary>
		///     Following a reverse relationship.
		/// </summary>
		[EnumMember( Value = "reverse" )]
		Reverse = 2,

		/// <summary>
		///     Relationship Instance of a forward relationship.
		/// </summary>
		[EnumMember( Value = "forwardInstance" )]
		ForwardInstance = 3,

		/// <summary>
		///     Relationship Instance of a reverse relationship.
		/// </summary>
		[EnumMember( Value = "reverseInstance" )]
		ReverseInstance = 4,

		/// <summary>
		///     The relationship type responsible for the inclusion.
		/// </summary>
		[EnumMember( Value = "typeInstance" )]
		TypeInstance = 5
	}
}