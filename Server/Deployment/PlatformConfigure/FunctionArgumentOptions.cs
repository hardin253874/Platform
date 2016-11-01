// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace PlatformConfigure
{
	/// <summary>
	///     Function argument options
	/// </summary>
	[Flags]
	public enum FunctionArgumentOptions
	{
		/// <summary>
		///     No flags
		/// </summary>
		None = 0,

		/// <summary>
		///     The function argument is optional.
		/// </summary>
		Optional = 1
	}
}