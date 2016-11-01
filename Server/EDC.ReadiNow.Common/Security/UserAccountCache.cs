// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Cache;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Caches user account information against a tenant.
	/// </summary>
	public static class UserAccountCache
	{
		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private static readonly object LockObject = new object( );

		/// <summary>
		///		The cache name
		/// </summary>
		public static readonly string CacheName = "User Account";

		/// <summary>
		///     Cache that represents tenants and their associated user accounts.
		/// </summary>
		private static readonly ICache<string, TenantUserAccountCacheEntry> _cache
            = (new CacheFactory { CacheName = CacheName, Lru = true, IsolateTenants = false })          // can this be tenant-isolated?
            .Create<string, TenantUserAccountCacheEntry>();

		/// <summary>
		/// Initializes the <see cref="UserAccountCache"/> class.
		/// </summary>
		static UserAccountCache( )
		{
			IChannel<UserAccountCacheMessage> channel = Entity.DistributedMemoryManager.GetChannel<UserAccountCacheMessage>( CacheName );
			channel.MessageReceived += Channel_MessageReceived;
			channel.Subscribe( );

			MessageChannel = channel;
		}

		/// <summary>
		/// Gets or sets the message channel.
		/// </summary>
		/// <value>
		/// The message channel.
		/// </value>
		private static IChannel<UserAccountCacheMessage> MessageChannel
		{
			get;
			set;
		}

		/// <summary>
		/// Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{UserAccountCacheMessage}"/> instance containing the event data.</param>
		private static void Channel_MessageReceived( object sender, MessageEventArgs<UserAccountCacheMessage> e )
		{
            using (Entity.DistributedMemoryManager.Suppress())
            {
                if (MessageReceivedAction != null)
                {
                    MessageReceivedAction(e);
                }

                foreach (string username in e.Message.Usernames)
                {
                    InvalidateInternal(username, e.Message.TenantName);
                }
            }
		}

		/// <summary>
		///     Gets or sets the message received action.
		/// </summary>
		/// <value>
		///     The message received action.
		/// </value>
		public static Action<MessageEventArgs<UserAccountCacheMessage>> MessageReceivedAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the cache instance ensuring that it has been initialized.
		/// </summary>
		private static ICache<string, TenantUserAccountCacheEntry> Cache
		{
			get { return _cache; }
		}

		/// <summary>
		///     Obtain a request context for the specified user belonging to the indicated tenant
		///     if available. If the specified tenant or user does not exist, or the given password
		///     is incorrect, null is returned.
		/// </summary>
		/// <param name="username">Username for which the request context is to be retrieved.</param>
		/// <param name="password">Users password to validate that the user is known.</param>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns>
		///     A new <see cref="RequestContextData" /> representing the specified user and tenant.
		/// </returns>
		/// <remarks>
		///     The returned RequestContextData object is not reused between requests so modifications made to
		///     its parameters dictionary etc are specific to this instance. The internal IdentityInfo and TenantInfo
		///     are references to the cached instances so modifications should not be made to them.
		/// </remarks>
		public static RequestContextData GetRequestContext( string username, string password, string tenantName )
		{
			TenantUserAccountCacheEntry tenantEntry;
			TenantUserAccountCacheEntry.UserAccountCacheEntry accountEntry = null;

			bool foundValue;

			lock ( LockObject )
			{
				foundValue = Cache.TryGetValue( tenantName, out tenantEntry );
			}

			if ( foundValue )
			{
				/////
				// Tenant has been previously cached.
				/////
				if ( !tenantEntry.Valid )
				{
					return null;
				}
			}
			else
			{
			    TenantInfo tenantInfo;
				

			    if (tenantName == SpecialStrings.GlobalTenant)
			    {
			        tenantInfo = new TenantInfo(0) {Name = SpecialStrings.GlobalTenant};
			    }
			    else
			    {
                    Tenant tenant;

			        using (new AdministratorContext())
			        {
			            tenant = Entity.GetByName<Tenant>(tenantName).FirstOrDefault();
			        }


			        if (tenant == null)
			        {
			            lock (LockObject)
			            {
			                /////
			                // Store an invalid entry to ensure multiple requests to this tenant don't
			                // continually hit the database.
			                /////
			                Cache[tenantName] = new TenantUserAccountCacheEntry();
			            }

			            return null;
			        }

			        using (new AdministratorContext())
			        {
			            tenantInfo = new TenantInfo(tenant.Id) { Name = tenantName.ToUpperInvariant() };
                    }
			    }

			    /////
				// Construct the new cache entry and cache it.
				/////
				using ( new AdministratorContext( ) )
				{
					tenantEntry =
						new TenantUserAccountCacheEntry( tenantInfo );
				}

				lock ( LockObject )
				{
					Cache[ tenantName ] = tenantEntry;
				}
			}

			/////
			// Get the account information from the cache if possible.
			/////
			if ( tenantEntry != null )
			{
				accountEntry = tenantEntry.Get( username, password );
			}

			if ( accountEntry == null )
			{
				return null;
			}

			/////
			// Construct a new RequestContextData instance and return it.
			/////
			var identityInfo = new IdentityInfo( accountEntry.Id, username );
			return new RequestContextData( identityInfo, tenantEntry.Tenant, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
		}

		/// <summary>
		///     Invalidate the entire cache.
		/// </summary>
		public static void Invalidate( )
		{
			if ( _cache != null )
			{
				lock ( LockObject )
				{
					Cache.Clear( );
				}
			}

			/////
			// TODO: Reimplement this as a cross app-domain aware cache.
			/////
			//Message.Raise( new UserAccountCacheRemoveMessage( true ), new TypeMessageContext( typeof( UserAccountCacheMessageRecipient ), MessageScope.Site ) );
		}

		/// <summary>
		///     Invalidates the specified tenant from the cache.
		/// </summary>
		/// <param name="tenant">
		///     Tenant to be removed from the cache.
		/// </param>
		public static void Invalidate( string tenant )
		{
			if ( _cache != null )
			{
				lock ( LockObject )
				{
					Cache.Remove( tenant );
				}
			}

			/////
			// TODO: Reimplement this as a cross app-domain aware cache.
			/////
			//Message.Raise( new UserAccountCacheRemoveTenantMessage( tenant, true ), new TypeMessageContext( typeof( UserAccountCacheMessageRecipient ), MessageScope.Site ) );
		}

		/// <summary>
		/// Invalidates the internal.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="tenant">The tenant.</param>
		private static void InvalidateInternal( string username, string tenant )
		{
			if ( _cache != null )
			{
				bool found;

				TenantUserAccountCacheEntry entry;
				lock ( LockObject )
				{
					found = Cache.TryGetValue( tenant, out entry );
				}

				if ( found )
				{
					entry.CacheEntryInvalidateInternal( username );
				}
			}
		}

		/// <summary>
		///     Invalidates the specified user belonging to the specified tenant from the cache.
		/// </summary>
		/// <param name="username">
		///     User to be removed from the cache.
		/// </param>
		/// <param name="tenant">
		///     Tenant that the specified user belongs to which is to be removed from the cache.
		/// </param>
		public static void Invalidate( string username, string tenant )
		{
			if ( _cache != null )
			{
				bool found;

				TenantUserAccountCacheEntry entry;
				lock ( LockObject )
				{
					found = Cache.TryGetValue( tenant, out entry );
				}

				if ( found )
				{
					entry.CacheEntryInvalidate( username );
				}
			}

			/////
			// TODO: Reimplement this as a cross app-domain aware cache.
			/////
			//Message.Raise( new UserAccountCacheRemoveUserMessage( tenant, username, true ), new TypeMessageContext( typeof( UserAccountCacheMessageRecipient ), MessageScope.Site ) );
		}

		/// <summary>
		///     Cache entry object that holds the tenant information as well as a cache of user accounts associated with it.
		/// </summary>
		private class TenantUserAccountCacheEntry
		{
			/// <summary>
			///     Thread synchronization object.
			/// </summary>
			private readonly object _innerLockObject = new object( );

			/// <summary>
			///     Cache that stores the user accounts for a particular tenant.
			/// </summary>
			private ICache<string, UserAccountCacheEntry> _innerCache;

			/// <summary>
			///     Default constructor that represents an invalid entry within the cache.
			/// </summary>
			public TenantUserAccountCacheEntry( )
				: this( false )
			{
			}

			/// <summary>
			///     Constructor that represents a valid entry within the cache.
			/// </summary>
			/// <param name="tenant">
			///     Tenant information.
			/// </param>
			/// <remarks>
			///     Each tenant can customize how long their user accounts are cached for and
			///     the maximum number of entries that can be stored.
			/// </remarks>
			public TenantUserAccountCacheEntry( TenantInfo tenant )
				: this( true )
			{
				Tenant = tenant;
			}

			/// <summary>
			///     Constructor that defines whether the current entry is valid or not.
			/// </summary>
			/// <param name="valid">
			///     True if the entry is valid; False otherwise.
			/// </param>
			private TenantUserAccountCacheEntry( bool valid )
			{
				Valid = valid;
			}

			/// <summary>
			///     Gets the tenant information.
			/// </summary>
			public TenantInfo Tenant
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets whether this cache entry is valid or not.
			/// </summary>
			public bool Valid
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets the cache object ensuring it has been initialized.
			/// </summary>
			private ICache<string, UserAccountCacheEntry> InnerCache
			{
				get
				{
                    if ( _innerCache == null )
                    {
                        CacheFactory factory = new CacheFactory { ExpirationInterval = TimeSpan.FromMinutes(30), IsolateTenants = false };  // can this be tenant-isolated?
                        _innerCache = factory.Create<string, UserAccountCacheEntry>( "User Account Cache" );
                    }
                    return _innerCache;
				}
			}

			/// <summary>
			///     Gets or sets the password policy id.
			/// </summary>
			/// <value>
			///     The password policy id.
			/// </value>
			private long PasswordPolicyId
			{
				get;
				set;
			}

			/// <summary>
			///     Invalidates the specified user from the cache.
			/// </summary>
			/// <param name="username">
			///     Username to be removed from the cache.
			/// </param>
			public void CacheEntryInvalidate( string username )
			{
				CacheEntryInvalidateInternal( username );

				var message = new UserAccountCacheMessage
				{
					TenantName = Tenant.Name
				};

				message.Usernames.Add( username );
				MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
			}

			/// <summary>
			/// Merges the messages.
			/// </summary>
			/// <param name="existingMessage">The existing message.</param>
			/// <param name="newMessage">The new message.</param>
			private void MergeMessages( UserAccountCacheMessage existingMessage, UserAccountCacheMessage newMessage )
			{
				existingMessage.Usernames.UnionWith( newMessage.Usernames );
			}

			/// <summary>
			/// Caches the entry invalidate internal.
			/// </summary>
			/// <param name="username">The username.</param>
			public void CacheEntryInvalidateInternal( string username )
			{
				if ( _innerCache != null )
				{
					lock ( _innerLockObject )
					{
						InnerCache.Remove( username );
					}
				}
			}

			/// <summary>
			///     Retrieves the specified cache entry if found or null otherwise.
			/// </summary>
			/// <param name="username">
			///     Key representing which object to be retrieved from the cache.
			/// </param>
			/// <param name="password">
			///     The current requests password that must match that currently stored in memory.
			/// </param>
			/// <returns>
			///     The specified cache entry if found; Null otherwise.
			/// </returns>
			public UserAccountCacheEntry Get( string username, string password )
			{
			    if (string.IsNullOrEmpty("userName"))
			    {
                    throw new ArgumentNullException("username");
			    }
			    if (password == null)
			    {
			        throw new ArgumentNullException("password");
			    }

				UserAccountCacheEntry entry;

				using ( new TenantAdministratorContext( Tenant.Id ) )
				{
					try
					{
						entry = GetUserAccountCacheEntry( username );

						// Get the user account
						var userAccount = Entity.Get<UserAccount>( entry.Id );
						if ( userAccount == null )
						{
							// Handle the case where the cache user account is invalid

							// Invalidate the name to id lookup cache
							CacheEntryInvalidate( username );

							// Re-cache the id
							entry = GetUserAccountCacheEntry( username );

							// Get the user account
							userAccount = Entity.Get<UserAccount>( entry.Id );
						}

						// Get the password policy
						var passwordPolicy = Entity.Get<PasswordPolicy>( PasswordPolicyId );

						// Validate the account
						UserAccountValidator.ValidateAccount( userAccount, passwordPolicy, password, false );
					}
					catch ( Exception ex )
					{
						EventLog.Application.WriteError( "Failed to validate account {0}. Error {1}.", username, ex.ToString( ) );
						entry = null;
					}
				}

				return entry;
			}

			/// <summary>
			///     Adds a new user account resource into the account cache.
			/// </summary>
			/// <param name="userAccount">
			///     The user account resource being added to the cache.
			/// </param>
			/// <returns>
			///     A new UserAccountCacheEntry object if added; null otherwise.
			/// </returns>
			private UserAccountCacheEntry AddAccount( UserAccount userAccount )
			{
				/////
				// Cache the entry.
				/////
				var entry = new UserAccountCacheEntry( userAccount.Id );

				lock ( _innerLockObject )
				{
					InnerCache[ userAccount.Name ] = entry;
				}

				return entry;
			}

			/// <summary>
			///     Gets the user account cache entry.
			/// </summary>
			/// <param name="username">The username.</param>
			/// <returns></returns>
			private UserAccountCacheEntry GetUserAccountCacheEntry( string username )
			{
				UserAccountCacheEntry entry;

				lock ( _innerLockObject )
				{
					if ( PasswordPolicyId == 0 )
					{
						// Cache the password policy instance id
						PasswordPolicyId = Entity.GetId( "core:passwordPolicyInstance" );
					}

					if ( !InnerCache.TryGetValue( username, out entry ) )
					{
						// We don't have a cache entry for this user name.
						// Get the user account by user name and add it to the cache
						UserAccount userAccountByName = Entity.GetByField<UserAccount>( username, true, new EntityRef( "core", "name" ) ).FirstOrDefault( );

						if ( userAccountByName == null )
						{
							return null;
						}

						entry = AddAccount( userAccountByName );
					}
				}

				return entry;
			}

			/// <summary>
			///     Cache entry object that holds user account information.
			/// </summary>
			public class UserAccountCacheEntry
			{
				/// <summary>
				///     Initializes a new instance of the <see cref="UserAccountCacheEntry" /> class.
				/// </summary>
				/// <param name="id">The id.</param>
				public UserAccountCacheEntry( long id )
				{
					Id = id;
				}

				/// <summary>
				///     Gets or sets the id.
				/// </summary>
				/// <value>
				///     The id.
				/// </value>
				public long Id
				{
					get;
					private set;
				}
			}
		}
	}
}