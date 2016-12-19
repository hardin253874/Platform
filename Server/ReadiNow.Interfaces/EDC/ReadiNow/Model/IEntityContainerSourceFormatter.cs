// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity container formatter.
	/// </summary>
	public interface IEntityContainerSourceFormatter
	{
		/// <summary>
		///     Formats the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>A collection of alias->value pairs.</returns>
		IEnumerable<KeyValuePair<string, object>> Format( string source );
	}
}