// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.Security;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model.CacheInvalidation;
using System.Collections.Generic;
using EDC.Database;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Validates user account credentials.
	/// </summary>
	public static class UserAccountValidator
	{
        /// <summary>
        /// Exception message thrown with an <see cref="InvalidCredentialException"/>.
        /// </summary>
        public static readonly string InvalidUserNameOrPasswordMessage = "Invalid user name or password.";

        /// <summary>
        /// Maximum password length.
        /// </summary>
        public static readonly int MaxPasswordLength = 100;

        /// <summary>
		///     Authenticates the specified user account credentials.
		/// </summary>
		/// <param name="username">A string containing the username associated with the user account. This cannot be null or empty.</param>
		/// <param name="password">A string containing the password associated with the user account. This can be empty but cannot be null.</param>
        /// <param name="tenant">A string containing the tenant associated with the user account. This cannot be null or empty.</param>
		/// <param name="setContext">A Boolean that controls whether or not this method sets the context data within the logical thread for subsequent security checks.</param>
        /// <param name="skipPasswordExpiryCheck">A Boolean that controls whether to perform the password expiry check.</param>
		/// <remarks>This method updates any user accounts associated with the request.</remarks>
		/// <exception cref="ArgumentNullException">
		/// No argument can be null. <paramref name="username"/> and <paramref name="tenant"/> cannot be empty.
		/// </exception>
		/// <exception cref="InvalidCredentialException">
		/// The <paramref name="username"/> and <paramref name="password"/> are not valid for the <paramref name="tenant"/>.
		/// </exception>
		/// <exception cref="TenantDisabledException">
		/// The <paramref name="tenant"/> is disabled, meaning no users can authenticate.
		/// </exception>
		public static void Authenticate( string username, string password, string tenant, bool setContext, bool skipPasswordExpiryCheck = false )
		{
            if (String.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
		    if (password == null)
		    {
		        throw new ArgumentNullException("password");
		    }
            if (String.IsNullOrEmpty(tenant))
            {
                throw new ArgumentNullException("tenant");
            }

		    RequestContextData contextData;
            using (Profiler.Measure("Authenticate " + tenant + "\\" + username + ", setContext " + setContext))
            {
                // Check the credentials
                try
                {
                    contextData = ValidateAccount(username, password, tenant, true, skipPasswordExpiryCheck);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidCredentialException(ex.Message);
                }
                if (contextData == null)
                {
                    throw new InvalidCredentialException(InvalidUserNameOrPasswordMessage);
                }

                if (setContext)
                {
                    // Set the user context data within the logical thread
                    RequestContext.SetContext(contextData);
                }
            }
		}


		/// <summary>
		///     Challenges the user associated with the current Request Context.
		/// </summary>
		public static void Challenge( )
		{
			/////
			// Cache the original request context
			/////
			RequestContext originalContextData = RequestContext.GetContext( );

			if ( originalContextData == null )
			{
				throw new InvalidOperationException( "No request context has been set." );
			}

			try
			{
				/////
				// Set the tenant administrators context
				/////
				RequestContext.SetTenantAdministratorContext( originalContextData.Tenant.Id );

				/////
				// Get the user account with the specified name
				/////
				var userAccount = Entity.Get<UserAccount>( originalContextData.Identity.Id, true );

				if ( userAccount == null )
				{
                    throw new AccountInvalidException("Does not exist");
				}

				/////
				// Get the password policy
				/////
				var passwordPolicy = Entity.Get<PasswordPolicy>( new EntityRef( "core:passwordPolicyInstance" ) );

				if ( passwordPolicy == null )
				{
                    throw new AccountInvalidException("Unable to obtain password policy.");
				}

                CheckAccountStatus(userAccount, passwordPolicy, true);
                CheckAccountExpiry(userAccount, true);
			}
			finally
			{
				/////
				// Restore the original request context
				/////
				if ( originalContextData.IsValid )
				{
					RequestContext.SetContext( originalContextData );
				}
			}
		}

		/// <summary>
		///     Validates the specified user account credentials.
		/// </summary>
		/// <param name="username">A string containing the username associated with the user account.</param>
		/// <param name="password">A string containing the password associated with the user account.</param>
		/// <param name="tenant">A string containing the tenant associated with the user account.</param>
		/// <param name="setContext">A Boolean that controls whether or not this method sets the context data within the logical thread for subsequent security checks.</param>
		/// <remarks>This method does not update any user account information associated with the request.</remarks>
		public static void Validate( string username, string password, string tenant, bool setContext )
		{
			if ( string.IsNullOrEmpty( username ) )
			{
                throw new ArgumentNullException("username");
			}
		    if ( password == null)
		    {
		        throw new ArgumentNullException("password");
		    }
			if ( string.IsNullOrEmpty( tenant ) )
			{
                throw new ArgumentNullException("tenant");
			}

			// Check the credentials
			RequestContextData contextData = UserAccountCache.GetRequestContext( username, password, tenant );

			if ( contextData == null )
			{
				EventLog.Application.WriteWarning( string.Format( "Invalid username or password. {0}\\{1} : {2}", tenant, username,
				                                                  ( password.Length == 0 ? "blank" : "nonblank" ) ) );

                throw new InvalidCredentialException(InvalidUserNameOrPasswordMessage);
			}

			if ( setContext )
			{
				// Set the user context data within the logical thread
				RequestContext.SetContext( contextData );
			}
		}


		/// <summary>
		/// Validates the account.
		/// </summary>
		/// <param name="userAccount">The user account.</param>
		/// <param name="passwordPolicy">The password policy.</param>
		/// <param name="password">The password.</param>
		/// <param name="updateAccount"><param name="skipPasswordExpiryCheck">A Boolean that controls whether to perform the password expiry check.</param>
		/// if set to <c>true</c> [update account].</param>
		/// <param name="skipPasswordExpiryCheck">if set to <c>true</c> [skip password expiry check].</param>
		/// <exception cref="System.ArgumentNullException">
		/// userAccount
		/// or
		/// password
		/// or
		/// passwordPolicy
		/// </exception>
        internal static void ValidateAccount( UserAccount userAccount, PasswordPolicy passwordPolicy, string password, bool updateAccount, bool skipPasswordExpiryCheck = false)
		{
			if ( userAccount == null )
			{
				throw new ArgumentNullException( "userAccount" );
			}
		    if (password == null)
		    {
                throw new ArgumentNullException( "password" );
		    }
			if ( passwordPolicy == null )
			{
				throw new ArgumentNullException( "passwordPolicy" );
			}

            CheckCredentials(userAccount, password, passwordPolicy, updateAccount);
            CheckAccountStatus(userAccount, passwordPolicy, updateAccount);
            CheckAccountExpiry(userAccount, updateAccount);

            if (!skipPasswordExpiryCheck)
            {
                CheckAccountPasswordExpiry(userAccount, passwordPolicy);
            }            
		}

        /// <summary>
        /// Returns true if the user account password has expired, false otherwise.
        /// </summary>
        /// <param name="userAccount">The user account to check.</param>        
        /// <param name="passwordPolicy">The password policy.</param>        
        /// <returns>True if the password has expired, false otherwise.</returns>
        private static bool HasAccountPasswordExpired(UserAccount userAccount, PasswordPolicy passwordPolicy)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException("userAccount");
            }           
            
            if (passwordPolicy == null)
            {
                throw new ArgumentNullException("passwordPolicy");
            }
            
            return DateTime.UtcNow > GetUserAccountPasswordExpiryDate(userAccount, passwordPolicy);
        }

        /// <summary>
        /// Returns true if the user account password has expired, false otherwise.
        /// </summary>
        /// <param name="userAccount">The user account to check.</param>        
        /// <returns>True if the password has expired, false otherwise.</returns>
        public static bool HasAccountPasswordExpired(UserAccount userAccount)
        {
            return HasAccountPasswordExpired(userAccount, Entity.Get<PasswordPolicy>("core:passwordPolicyInstance"));
        }

        /// <summary>
        ///     Validates the account status, without regard to the password.
        /// </summary>
        /// <param name="userAccount">The user account.</param>
        /// <param name="updateAccount">
        ///     If set to <c>true</c> any relevant changes, such as switching the state to expired, will be saved into the user account.
        /// </param>
        public static void ValidateAccountStatus( UserAccount userAccount, bool updateAccount )
        {
            if ( userAccount == null )
            {
                throw new ArgumentNullException( "userAccount" );
            }

            var passwordPolicy = Entity.Get<PasswordPolicy>( "core:passwordPolicyInstance" );
            if ( passwordPolicy == null )
            {
                throw new Exception( "Could not find passwordPolicyInstance." );
            }

            CheckAccountStatus( userAccount, passwordPolicy, updateAccount );
            CheckAccountExpiry( userAccount, updateAccount );
            CheckAccountPasswordExpiry(userAccount, passwordPolicy);
        }


		/// <summary>
		///     Checks the account expiry.
		/// </summary>
		/// <param name="userAccount">The user account.</param>
		/// <param name="updateAccount">
		///     if set to <c>true</c> [update account].
		/// </param>
		/// <exception cref="System.IdentityModel.Tokens.SecurityTokenException">The specified account has expired.</exception>
		private static void CheckAccountExpiry( UserAccount userAccount, bool updateAccount )
		{
		    if (userAccount == null)
		    {
		        throw new ArgumentNullException("userAccount");
		    }

			// Throw an exception if the account has expired.
			if ( HasUserAccountExpired( userAccount ) )
			{
				if ( updateAccount )
				{
					userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Expired;
					userAccount.Save( );
				}

				throw new AccountExpiredException();
			}
		}

		/// <summary>
		///     Checks the account status.
		/// </summary>
		/// <param name="userAccount">The user account.</param>
		/// <param name="passwordPolicy">The password policy.</param>
		/// <param name="updateAccount">
		///     if set to <c>true</c> [update account].
		/// </param>
		private static void CheckAccountStatus( UserAccount userAccount, PasswordPolicy passwordPolicy, bool updateAccount )
		{
		    if (userAccount == null)
		    {
		        throw new ArgumentNullException("userAccount");
		    }
            if (passwordPolicy == null)
            {
                throw new ArgumentNullException("passwordPolicy");
            }

			// Check that the account is active
			if ( userAccount.AccountStatus_Enum != UserAccountStatusEnum_Enumeration.Active )
			{
				switch ( userAccount.AccountStatus_Enum )
				{
					case UserAccountStatusEnum_Enumeration.Expired:
						throw new AccountExpiredException();

					case UserAccountStatusEnum_Enumeration.Locked:
						bool isLocked = true;
                        int accountLockoutDuration = passwordPolicy.AccountLockoutDuration ?? 0;

						if ( updateAccount )
						{
							// The account is locked. Check if we need to automatically unlock the account
							bool autoUnlockAccount = false;

							if ( accountLockoutDuration > 0 &&
							     userAccount.LastLockout != null )
							{
								DateTime autoUnlockTime = userAccount.LastLockout.Value.AddMinutes( accountLockoutDuration );
								if ( DateTime.UtcNow > autoUnlockTime )
								{
									autoUnlockAccount = true;
								}
							}

							if ( autoUnlockAccount )
							{
								userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
								userAccount.BadLogonCount = 0;
								userAccount.Save( );
								isLocked = false;
							}
						}

						if ( isLocked )
						{
                            throw new AccountLockedException(accountLockoutDuration);
						}
						break;

                    case UserAccountStatusEnum_Enumeration.Disabled:
                        throw new AccountDisabledException();

					default:
                        throw new ArgumentException(
                            string.Format("Unknown account status '{0}'", userAccount.AccountStatus_Enum), 
                            "userAccount");
				}
			}
		}


		/// <summary>
		///     Checks the password for the specified user account.
		/// </summary>
		/// <param name="userAccount">An object representing a valid user account.</param>
		/// <param name="password">A string containing the password to check.</param>
		/// <param name="passwordPolicy">The password policy.</param>
		/// <param name="updateAccount">A bool that controls whether or not user account information associated with the request is updated.</param>
		private static void CheckCredentials( UserAccount userAccount, string password, PasswordPolicy passwordPolicy, bool updateAccount )
		{
		    if (userAccount == null)
		    {
		        throw new ArgumentNullException("userAccount");
		    }
		    if (password == null)
		    {
		        throw new ArgumentNullException("password");
		    }
            if (passwordPolicy == null)
            {
                throw new ArgumentNullException("passwordPolicy");
            }

			bool valid = false;

			// Check the password is correct            
			if ( CryptoHelper.ValidatePassword( password, userAccount.Password ) )
			{
				if ( updateAccount )
				{
					UpdateUserAccountLastLogon( userAccount.Id );

                    if (userAccount.BadLogonCount.HasValue && userAccount.BadLogonCount != 0)
				    {
				        userAccount.BadLogonCount = 0;
				    }
				}
				valid = true;
			}
			else
			{
				if ( updateAccount )
				{
					int accountLockoutThreshold = passwordPolicy.AccountLockoutThreshold ?? 0;
					int badLoginCount = userAccount.BadLogonCount ?? 0;

					if ( accountLockoutThreshold > 0 &&
					     ++badLoginCount >= accountLockoutThreshold &&
                        userAccount.AccountStatus_Enum == UserAccountStatusEnum_Enumeration.Active)
					{
						userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Locked;
						userAccount.LastLockout = DateTime.UtcNow;
						userAccount.BadLogonCount = badLoginCount;
					}
					else
					{
						userAccount.BadLogonCount = badLoginCount;
					}
				}
			}

			// Update the user data (if requested)
            if (updateAccount && userAccount.HasChanges(null))
			{
				userAccount.Save( );
			}

			// Check if the credentials were validated
			if ( !valid )
			{
                throw new InvalidCredentialException(InvalidUserNameOrPasswordMessage);
			}
		}

		/// <summary>
		/// Updates the user accounts last logon time.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <remarks>
		/// This intentionally bypasses the entity model and directly updates the database asynchronously.
		/// Any cached values are removed from the read-only cache upon completion.
		/// Cache invalidators are not invoked due to this call.
		/// </remarks>
		private static void UpdateUserAccountLastLogon( long entityId  )
		{
			long tenantId = RequestContext.TenantId;
			long fieldId = WellKnownAliases.CurrentTenant.LastLogon;
			DateTime now = DateTime.UtcNow;

			ThreadPool.QueueUserWorkItem( state =>
			{
				using ( DatabaseContextInfo.SetContextInfo( $"Update last logon for user '{entityId}'" ) )
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( var command = ctx.CreateCommand( "spData_DateTimeMerge", CommandType.StoredProcedure ) )
					{
						ctx.AddParameter( command, "@entityId", DbType.Int64, entityId );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
						ctx.AddParameter( command, "@fieldId", DbType.Int64, fieldId );
						ctx.AddParameter( command, "@data", DbType.DateTime, now );
						ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( entityId ) );

						command.ExecuteNonQuery( );

						IEntityFieldValues values;

						/////
						// Remove the last logon time from the field cache (if it exists).
						/////
						if ( EntityFieldCache.Instance.TryGetValue( entityId, out values ) )
						{
							object lastLogon;

							if ( values.TryGetValue( fieldId, out lastLogon ) )
							{
								values [ fieldId ] = now;
							}
						}

						/////
						// Invalidate any items referencing the LastLogon item (NOT the UserAccount item)
						////
						var cacheInvalidators = new CacheInvalidatorFactory( ).CacheInvalidators.ToList( );

						List<IEntity> fieldTypes = new List<IEntity>
						{
							new IdEntity( fieldId )
						};

						foreach ( ICacheInvalidator cacheInvalidator in cacheInvalidators )
						{
							cacheInvalidator.OnEntityChange( fieldTypes, InvalidationCause.Save, null );
						}
					}
				}
			} );
		}

		/// <summary>
		/// Gets the user account password expiry date.
		/// </summary>
		/// <param name="userAccount">The user account.</param>
		/// <param name="passwordPolicy">The password policy.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// userAccount
		/// or
		/// passwordPolicy
		/// </exception>
		private static DateTime GetUserAccountPasswordExpiryDate(UserAccount userAccount, PasswordPolicy passwordPolicy)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException("userAccount");
            }

            if (passwordPolicy == null)
            {
                throw new ArgumentNullException("passwordPolicy");
            }

            DateTime passwordExpiryDate = DateTime.MaxValue;
                        
            bool passwordNeverExpires = userAccount.PasswordNeverExpires ?? false;

            if (passwordNeverExpires)
            {
                return passwordExpiryDate;
            }
            
            int maximumPasswordAge = passwordPolicy.MaximumPasswordAge ?? 0;
            DateTime? passwordLastChanged = userAccount.PasswordLastChanged;

            if (maximumPasswordAge > 0 && passwordLastChanged != null)
            {
                passwordExpiryDate = passwordLastChanged.Value.AddDays(maximumPasswordAge);
            }                            

            return passwordExpiryDate;
        }

        /// <summary>
        /// Determines whether the user account password has expired.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="passwordPolicy"></param>
        private static void CheckAccountPasswordExpiry(UserAccount userAccount, PasswordPolicy passwordPolicy)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException("userAccount");
            }
            
            if (passwordPolicy == null)
            {
                throw new ArgumentNullException("passwordPolicy");
            }

            if (HasAccountPasswordExpired(userAccount, passwordPolicy))
            {
                throw new PasswordExpiredException();
            }            
        }

        /// <summary>
        ///     Determines whether the user account has expired.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>
        ///     <c>true</c> if the user account has expired; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasUserAccountExpired( UserAccount account )
		{
			return account.AccountExpiration != null && DateTime.UtcNow > account.AccountExpiration.Value;
		}

        /// <summary>
        ///     Checks the credentials for the specified user account.
        /// </summary>
        /// <param name="username">A string containing the username associated with the user account. This cannot be null or empty.</param>
        /// <param name="password">A string containing the password associated with the user account. This cannot be null.</param>
        /// <param name="tenantName">Name of the tenant. This cannot be null or empty.</param>
        /// <param name="updateAccount">A Boolean value that controls whether or not user account information associated with the request is updated.</param>
        /// <param name="skipPasswordExpiryCheck">A Boolean that controls whether to perform the password expiry check.</param>
        /// <returns>An object representing the identity of the user account</returns>
        /// <exception cref="ArgumentException">
        /// The given account details are incorrect.
        /// </exception>
        /// <exception cref="TenantDisabledException">
        /// The tenant is disabled, meaning no user in that tenant can authenticate.
        /// </exception>
        private static RequestContextData ValidateAccount( string username, string password, string tenantName, bool updateAccount, bool skipPasswordExpiryCheck = false)
		{
			if ( String.IsNullOrEmpty( username ) )
			{
                throw new ArgumentException("The specified username parameter is invalid.");
			}
		    if (password == null)
		    {
		        throw new ArgumentNullException("password");
		    }
			if ( String.IsNullOrEmpty( tenantName ) )
			{
                throw new ArgumentException("The specified tenant parameter is invalid.");
			}

			RequestContextData contextData;

			// Cache the original request context
			RequestContext originalContextData = RequestContext.GetContext( );

			try
			{
				TenantInfo tenantInfo;
                UserAccount userAccount;
                PasswordPolicy passwordPolicy;

				// Set the system administrators context
				RequestContext.SetSystemAdministratorContext( );

			    try
			    {
                    if (password.Length > MaxPasswordLength)
                    {
                        throw new ArgumentException(
                            string.Format("Password cannot be longer than {0} characters", MaxPasswordLength),
                            "password");
                    }

				    if ( tenantName == SpecialStrings.GlobalTenant )
				    {
					    // Create a Dummy Tenant Info
					    // No need to set the context as we are already the global admin
				        tenantInfo = new TenantInfo(0) {Name = SpecialStrings.GlobalTenant};
				    }
				    else
				    {
					    // Get the tenant with the specified name
					    Tenant tenant = TenantHelper.Find( tenantName );
					    if ( tenant == null )
					    {
						    throw new ArgumentException(string.Format("Unknown tenant '{0}'", tenantName), "tenantName");
					    }

				        if (tenant.IsTenantDisabled ?? false)
				        {
                            throw new TenantDisabledException(tenantName);
				        }

					    // Set the tenant administrators context
					    RequestContext.SetTenantAdministratorContext( tenant.Id );

					    tenantInfo = new TenantInfo( tenant.Id ) { Name = tenantName.ToUpperInvariant() };
                    }

				    // Get the user account with the specified name
				    userAccount = Entity.GetByField<UserAccount>( username, true, new EntityRef( "core", "name" ) ).FirstOrDefault( );
                    if ( userAccount == null )
                    {
                        throw new ArgumentException(string.Format("Could not find user '{0}' in tenant '{1}'", username, tenantName));
                    }

				    // Get the password policy
				    passwordPolicy = Entity.Get<PasswordPolicy>( new EntityRef( "core:passwordPolicyInstance" ) );
				    if ( passwordPolicy == null )
				    {
                        throw new ArgumentException(string.Format("Could not find password policy for tenant '{0}'", tenantName));
				    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteWarning("Login failed: " + ex.Message);

                    // Validate a password here to mitigate timing attacks. An attacker could use this 
                    // to guess which passwords are valid by timing the login. Without this, logins that 
                    // have a invalid user name and tenant will be quicker than those with a valid user name.
                    CryptoHelper.CreateEncodedSaltedHash("test password");

                    throw;
                }


				ValidateAccount( userAccount, passwordPolicy, password, updateAccount, skipPasswordExpiryCheck);

				// Set the context data
				var identityInfo = new IdentityInfo( userAccount.Id, userAccount.Name );

                contextData = new RequestContextData(identityInfo, tenantInfo, CultureHelper.GetUiThreadCulture(CultureType.Specific));
			}
			finally
			{
				// Restore the original request context
				if ( ( originalContextData != null ) && ( originalContextData.IsValid ) )
				{
					RequestContext.SetContext( originalContextData );
				}
			}

			return contextData;
		}
	}
}