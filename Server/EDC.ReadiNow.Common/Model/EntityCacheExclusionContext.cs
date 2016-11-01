// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Remoting.Messaging;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     EntityCache Exclusion Context.
	/// </summary>
	public class EntityCacheExclusionContext : IDisposable
	{
		/// <summary>
		///     Context key.
		/// </summary>
        [ThreadStatic]
        private static bool _isActive = false;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityCacheExclusionContext" /> class.
		/// </summary>
		public EntityCacheExclusionContext( )
		{
            _isActive = true;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
            _isActive = false;
		}

		/// <summary>
		///     Determines whether this instance is active.
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsActive( )
		{
            return _isActive;
		}
	}
}