// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Security interface.
	/// </summary>
	public interface IEntitySecurity
	{
		/// <summary>
		///     Checks the specified permission.
		/// </summary>
		/// <param name="permission">The permission.</param>
		bool Check( IEntityRef permission );

		/// <summary>
		///     Checks the specified permissions.
		/// </summary>
		/// <param name="permissions">The permissions.</param>
		bool Check( IEnumerable<IEntityRef> permissions );

		/// <summary>
		///     Demands the specified permission.
		/// </summary>
		/// <param name="permission">The permission.</param>
		void Demand( IEntityRef permission );

		/// <summary>
		///     Demands the specified permissions.
		/// </summary>
		/// <param name="permissions">The permissions.</param>
		void Demand( IEnumerable<IEntityRef> permissions );
	}
}