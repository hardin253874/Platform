// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Cache;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Services.Console
{
	/// <summary>
	///     The ActionCacheInvalidator class.
	/// </summary>
	public class ActionCacheInvalidator : SecurityCacheInvalidatorBase<int, ActionResponse>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ActionCacheInvalidator" /> class.
		/// </summary>
		/// <param name="cache">The cache.</param>
		public ActionCacheInvalidator( ICache<int, ActionResponse> cache )
			: base( cache, "Get Action Cache" )
		{
			// Do nothing
		}
	}
}