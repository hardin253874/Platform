// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using ETenant = EDC.ReadiNow.Model.Tenant;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     Handles the creation and deletion of a tenant.
	/// </summary>
	/// <remarks></remarks>
	public class Tenant
	{
		/// <summary>
		///     Creates the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <remarks></remarks>
		public static void CreateTenant( string tenantName )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				/////
				// Check for existing tenant
				/////
				long tenantId = TenantHelper.GetTenantId( tenantName );

				if ( tenantId != -1 )
				{
					// Fail, the tenant already exists.
					Console.WriteLine( @"The tenant {0} already exists.", tenantName );
					return;
				}

				TenantHelper.CreateTenant( tenantName );
			}
		}

		/// <summary>
		/// Renames the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="newName">The new name.</param>
		/// <returns></returns>
		public static void RenameTenant( string tenantName, string newName )
		{
			TenantHelper.RenameTenant( tenantName, newName );
		}

		/// <summary>
		///     Creates the user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="password">The password.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="roleName">Name of the role or null or empty if no role is to be assigned.</param>
		public static void CreateUser( string userName, string password, string tenantName, string roleName )
		{
			using ( new TenantAdministratorContext( tenantName ) )
			{
				// Check for existing user
				long userId = GetUserIdByName( userName );

				if ( userId != -1 )
				{
					/////
					// Fail, the user already exists.
					/////
					Console.WriteLine( @"The user {0} already exists.", userName );
					return;
				}

				/////
				// Create the specified user
				/////
				var userAccount = Entity.Create<UserAccount>( );
				userAccount.Name = userName;
				userAccount.Password = password;
				userAccount.Description = userName + " user account.";
				userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;

                if (!string.IsNullOrEmpty(roleName))
                {
                    var role = Entity.GetByName<Role>(roleName).FirstOrDefault();

                    if (role == null)
                    {
                        Console.WriteLine(@"There is no role called '{0}'.", roleName);
                        return;
                    }

                    userAccount.UserHasRole.Add(role);
                }

                userAccount.Save();
			}
		}

		/// <summary>
		///     Deletes the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <remarks></remarks>
		public static void DeleteTenant( string tenantName )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				long tenantId = TenantHelper.GetTenantId( tenantName );

				if ( tenantId == -1 )
				{
					/////
					// Fail, the tenant does not exist.
					/////
					Console.WriteLine( @"The tenant {0} does not exist.", tenantName );
					return;
				}

				TenantHelper.DeleteTenant( tenantId );
			}
		}

		/// <summary>
		///     Deletes the user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		public static void DeleteUser( string userName, string tenantName )
		{
			using ( new TenantAdministratorContext( tenantName ) )
			{
				/////
				// Check for existing user
				/////
				long userId = GetUserIdByName( userName );

				if ( userId == -1 )
				{
					/////
					// Fail, the user does not exist.
					/////
					Console.WriteLine( @"The user {0} does not exist.", userName );
					return;
				}

				/////
				// Delete user
				/////
				Entity.Delete( userId );
			}
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetTenants( )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				IEnumerable<ETenant> instancesOfType = TenantHelper.GetAll( );

				if ( instancesOfType != null )
				{
					return instancesOfType.Select( t => t.Name ).ToList( );
				}

				return Enumerable.Empty<string>( );
			}
		}

		/// <summary>
		///     Gets the name of the user identifier by.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Multiple users found with name  + userName</exception>
		public static long GetUserIdByName( string userName )
		{
			IEnumerable<UserAccount> matches = Entity.GetByName<UserAccount>( userName );

			IList<UserAccount> users = matches as IList<UserAccount> ?? matches.ToList( );

			if ( users.Count <= 0 )
			{
				return -1;
			}

			if ( users.Count > 1 )
			{
				throw new Exception( "Multiple users found with name " + userName );
			}

			return users.First( ).Id;
		}

        /// <summary>
        ///     Disables the tenant.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        public static void DisableTenant(string tenantName)
        {
            using (new GlobalAdministratorContext())
            {
                var tenant = TenantHelper.Find(tenantName);
                if (tenant == null)
                {
                    throw new Exception("Tenant " + tenantName + " not found.");
                }

                var tenantWrite = tenant.AsWritable<ETenant>();
                tenantWrite.IsTenantDisabled = true;
                tenantWrite.Save();
            }
        }

        /// <summary>
        ///     Enables the tenant.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        public static void EnableTenant(string tenantName)
        {
            using (new GlobalAdministratorContext())
            {
                var tenant = TenantHelper.Find(tenantName);
                if (tenant == null)
                {
                    throw new Exception("Tenant " + tenantName + " not found.");
                }
                
                var tenantWrite = tenant.AsWritable<ETenant>();
                tenantWrite.IsTenantDisabled = false;
                tenantWrite.Save();
            }
        }
    }
}