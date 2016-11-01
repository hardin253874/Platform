// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.Exceptions;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.SoftwarePlatform.WebApi.Controllers.Exceptions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;

namespace EDC.SoftwarePlatform.WebApi
{
	/// <summary>
	///     Various helpers.
    /// </summary>
	internal static class WebApiHelpers
	{
		/// <summary>
		///     Create an EntityRef object from an id or NS and alias.
		/// </summary>
		public static EntityRef GetId( string idOrAlias )
		{
			if ( idOrAlias == null )
				throw new WebArgumentNullException( "idOrAlias" );

			long id;
			if ( long.TryParse( idOrAlias, out id ) )
				return new EntityRef( id );

			string[ ] parts = idOrAlias.Split( ':' );
			return parts.Length > 1 ? new EntityRef( parts[ 0 ], parts[ 1 ] ) : new EntityRef( idOrAlias );
		}


        /// <summary>
        ///     Gets the entity ref.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static EntityRef GetIdWithDashedAlias(string id)
        {
            string id2 = id.Replace("-", ":");
            return GetId(id2);
        }


		/// <summary>
		///     Create an EntityRef object from an id or NS and alias.
		///     Either the ID can be passed, or the alias can be passed (in which case the NS is optional).
		/// </summary>
		public static EntityRef MakeEntityRef( string id, string ns, string alias )
		{
			if ( String.IsNullOrEmpty( alias ) && String.IsNullOrEmpty( id ) )
                throw new WebArgumentNullException("No id or alias");

			long idNumber;
			if ( long.TryParse( id, out idNumber ) )
			{
				return new EntityRef( idNumber );
			}
			return new EntityRef( ns, alias );
		}


        /// <summary>
        ///     Create an EntityRef object from an id or NS and alias.
        ///     Either the ID can be passed, or the alias can be passed (in which case the NS is optional).
        /// </summary>
        public static void CheckEntityId<T>(string paramName, long entityId) where T : class, IEntity
        {
            if (entityId <= 0)
            {
                throw new WebArgumentOutOfRangeException(paramName, "Invalid " + typeof(T).Name + " ID.");
            }
            
            var report = EDC.ReadiNow.Model.Entity.Get<T>(entityId);
            if (report == null)
            {
                throw new WebArgumentNotFoundException(paramName, typeof(T).Name + " not found.");
            }
        }

        /// <summary>
        /// Given the name of a tenant, create a context block for that tenant
        /// </summary>
        public static IDisposable GetTenantContext(string tenantName)
        {
            Tenant tenant;

            if (string.IsNullOrEmpty(tenantName))
                throw new InvalidTenantException();

            using (new GlobalAdministratorContext())
            {
                tenant = TenantHelper.Find(tenantName);
                if (tenant == null)
                    throw new InvalidTenantException(tenantName);
            }

            return new TenantAdministratorContext(tenant.Id);
        }
    }
}