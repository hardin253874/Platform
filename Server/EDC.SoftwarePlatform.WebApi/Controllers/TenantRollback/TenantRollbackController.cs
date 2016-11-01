// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net.Http;
using System.Web.Http;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.TenantRollback
{
	/// <summary>
	/// </summary>
	/// <seealso cref="System.Web.Http.ApiController" />
	[RoutePrefix( "data/v1/tenantRollback" )]
	public class TenantRollbackController : ApiController
	{
		/// <summary>
		///     Creates the specified name.
		/// </summary>
		/// <param name="restorePoint">The restore point.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Failed to get tenant rollback data.</exception>
		[Route( "create" )]
		[HttpPost]
		public HttpResponseMessage Create( [FromBody] RestorePointDetails restorePoint )
		{
			try
			{
				DatabaseChangeTracking.CreateRestorePoint( restorePoint.Name, -1, true );

				return new HttpResponseMessage( );
			}
			catch ( Exception e )
			{
				throw new Exception( "Failed to get tenant rollback data.", e );
			}
		}

		/// <summary>
		///     Gets the tenant rollback data.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Failed to get tenant rollback data.</exception>
		[Route( "" )]
		[HttpGet]
		public HttpResponseMessage<TenantRollbackData> GetTenantRollbackData( )
		{
			try
			{
				int days = 4;

				if ( Factory.FeatureSwitch.Get( "unlimitedTenantRollback" ) )
				{
					days = 0;
				}

				TenantRollbackData data = DatabaseChangeTracking.GetTenantRollbackData( days );

				return new HttpResponseMessage<TenantRollbackData>( data );
			}
			catch ( Exception e )
			{
				throw new Exception( "Failed to get tenant rollback data.", e );
			}
		}

		/// <summary>
		///     Rollbacks the specified rollback.
		/// </summary>
		/// <param name="rollback">The rollback.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Failed to retrieve the last transaction id.</exception>
		/// <exception cref="System.Exception">Failed to get tenant rollback data.</exception>
		[Route( "rollback" )]
		[HttpPost]
		public HttpResponseMessage Rollback( [FromBody] RollbackDetails rollback )
		{
			try
			{
				long tenantId = ReadiNow.IO.RequestContext.TenantId;

				DatabaseChangeTracking.CreateRestorePoint( $"Rollback to '{rollback.Name}'", tenantId, true );

				long transactionId = DatabaseChangeTracking.GetTransactionId( rollback.Date, tenantId );

				if ( transactionId < 0 )
				{
					throw new InvalidOperationException( "Failed to retrieve the last transaction id." );
				}

				DatabaseChangeTracking.RevertTo( transactionId, tenantId );

				return new HttpResponseMessage( );
			}
			catch ( Exception e )
			{
				throw new Exception( "Failed to get tenant rollback data.", e );
			}
		}
	}
}