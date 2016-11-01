// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC
{
	/// <summary>
	///     Indicates that the type is immutable
	/// </summary>
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct )]
	public class ImmutableAttribute : Attribute
	{
	
	}
}