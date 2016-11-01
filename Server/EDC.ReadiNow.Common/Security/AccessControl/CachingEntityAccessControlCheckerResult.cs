// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
	/// <summary>
	///     The results from a call to <see cref="CachingEntityAccessControlChecker.CheckAccess" />.
	/// </summary>
	internal class CachingEntityAccessControlCheckerResult : Dictionary<long, bool>, ICachingEntityAccessControlCheckerResult
	{
		/// <summary>
		///     Create a new <see cref="CachingEntityAccessControlCheckerResult" />.
		/// </summary>
		internal CachingEntityAccessControlCheckerResult( )
		{
			CacheResult = new Dictionary<long, bool>( );
		}

		/// <summary>
		///     Each entity the security check was performed on is included as the key mapped to
		///     whether access was granted (true) or not (false).
		/// </summary>
		public Dictionary<long, bool> CacheResult
		{
			get;
			private set;
		}
	}
}