// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Diagnostics.Response;
using EDC.ReadiNow.Metadata.Tenants;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics.Request
{
	/// <summary>
	///     The flush caches request class.
	/// </summary>
	/// <seealso cref="EDC.ReadiNow.Diagnostics.DiagnosticRequest" />
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

		/// <summary>
		///     Gets the response.
		/// </summary>
		/// <returns></returns>
		public override DiagnosticResponse GetResponse( )
		{
			if ( TenantId >= 0 )
			{
				TenantHelper.Invalidate( TenantId );
				EventLog.Application.WriteInformation( $"Diagnostics Cache flush for tenant {TenantId}" );
			}
			else
			{
				CacheManager.ClearCaches( );
				EventLog.Application.WriteInformation( $"Diagnostics Cache flush" );
			}

			return new FlushCachesResponse( );
		}
	}
}