// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Sets the tenant administrators context for the duration of this objects lifetime on the current thread.
	///     Once disposed, the original calling context is restored.
	/// </summary>
	public class TenantAdministratorContext : ContextBlock
	{
		/// <summary>
		///     Default constructor for the TenantAdministratorContext object.
		/// </summary>
		/// <param name="tenantEntityId">The tenant entity id.</param>
		/// <remarks></remarks>
		public TenantAdministratorContext( long tenantEntityId )
			: base( ( ) => RequestContext.SetTenantAdministratorContext( tenantEntityId ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantAdministratorContext" /> class.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		public TenantAdministratorContext( string tenantName )
			: base( ( ) => RequestContext.SetTenantAdministratorContext( GetTenantId( tenantName ) ) )
		{
		}

		/// <summary>
		///     Sets the context.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <returns></returns>
		public static TenantAdministratorContext SetContext( long tenantId )
		{
			return new TenantAdministratorContext( tenantId );
		}

		/// <summary>
		///     Sets the context.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		public static TenantAdministratorContext SetContext( string tenantName )
		{
			return new TenantAdministratorContext( tenantName );
		}

		/// <summary>
		///     Gets the tenant id.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		/// <exception cref="EntityNotFoundException">Unable to locate Tenant with name ' + tenantName + '.</exception>
		private static long GetTenantId( string tenantName )
		{
			using ( new AdministratorContext( ) )
			{
				/////
				// Get the tenant with the specified name
				/////
				Tenant tenant = TenantHelper.Find( tenantName );

				if ( tenant == null )
				{
					throw new EntityNotFoundException( "Unable to locate Tenant with name '" + tenantName + "'." );
				}

				return tenant.Id;
			}
		}
	}
}