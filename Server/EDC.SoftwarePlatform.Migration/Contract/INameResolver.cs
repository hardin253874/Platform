// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     INameResolver interface.
	/// </summary>
	public interface INameResolver
	{
		/// <summary>
		///     Resolves the specified upgrade identifier.
		/// </summary>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <returns></returns>
		EntityAlias Resolve( Guid upgradeId );
	}
}