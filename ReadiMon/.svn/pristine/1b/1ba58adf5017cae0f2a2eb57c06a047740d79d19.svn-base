// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Request;

namespace ReadiMon.Plugin.Database.Diagnostics
{
	/// <summary>
	///     The flush caches request class.
	/// </summary>
	[ProtoContract]
	public class FlushCachesRequest : DiagnosticRequest
	{
		/// <summary>
		///     Gets or sets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long TenantId
		{
			get;
			set;
		}
	}
}