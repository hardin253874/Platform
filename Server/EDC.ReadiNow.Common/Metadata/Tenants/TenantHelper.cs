// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Cache;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.BackgroundTasks;

namespace EDC.ReadiNow.Metadata.Tenants
{
	/// <summary>
	///     Provides helper methods for interacting with tenants.
	/// </summary>
	public static class TenantHelper
	{
        // Cache of tenant names to IDs
        // Note* This needs to be distributed so that modifications from other
        // app domains (i.e. PlatformConfigure) get processed in the WebApi app domain.
        private static readonly ICache<string, long> TenantIdCache
            = new CacheFactory { Distributed = true, IsolateTenants = false }
            .Create<string, long>("Tenant ID cache");

        private static readonly ICache<long, string> TenantNameCache
            = new CacheFactory { Distributed = true, IsolateTenants = false }
            .Create<long, string>("Tenant Name cache");

        /// <summary>
        ///     Creates the tenant.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static long CreateTenant( string name, string description = null )
		{
			using ( new GlobalAdministratorContext( ) )
			using ( DatabaseContextInfo.SetContextInfo( $"Create tenant '{name}'" ) )
			{
				/////
				// Create the specified tenant
				/////
				var tenant = Entity.Create<Tenant>( );

				tenant.Name = name;
				tenant.Description = description ?? name + " tenant";

				if ( name == "EDC" )
				{
					tenant.Alias = "core:edcTenant";
				}

				tenant.Save( );

				return tenant.Id;
			}
		}

		/// <summary>
		///     Deletes the tenant from the entity model.
		/// </summary>
		/// <param name="tenantId">The tenant id.</param>
		public static void DeleteTenant( long tenantId )
		{
			EventLog.Application.WriteWarning( "Deleting tenant " + tenantId );

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				// Delete the tenant entity instance itself.
				// Note: this is stored in the root tenant                
				using ( new AdministratorContext( ) )
				{
					Entity.Delete( tenantId );
				}

				// Delete the data
				using ( DatabaseContextInfo.SetContextInfo( $"Delete tenant {tenantId}" ) )
				using ( IDbCommand command = ctx.CreateCommand( "spDeleteTenant", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					command.AddParameter( "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					command.ExecuteNonQuery( );
				}
			}

			/////
			// Remove the cached tenant id.
			/////
			List<string> matchingTenants = TenantIdCache.Where( pair => pair.Value == tenantId ).Select( pair => pair.Key ).ToList( );

			foreach ( string matchingTenant in matchingTenants )
			{
				TenantIdCache.Remove( matchingTenant );
			}

		    TenantNameCache.Remove(tenantId);

            EventLog.Application.WriteWarning( "Deleted tenant " + tenantId );
		}

		/// <summary>
		/// Renames the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <param name="newName">The new name.</param>
		/// <exception cref="EntityNotFoundException"></exception>
		public static void RenameTenant( string tenantName, string newName )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				/////
				// Check for existing tenant
				/////
				Tenant tenant = Find( tenantName );

				if ( tenant == null )
				{
					throw new EntityNotFoundException( string.Format( @"Tenant '{0}' not found.", tenantName ) );
				}

				tenant = tenant.AsWritable<Tenant>( );

				tenant.Name = newName;
				tenant.Save( );

				/////
				// Invalidate the tenant name to id cache
				/////
				TenantIdCache.Remove( tenantName );

			    TenantNameCache.Remove(tenant.Id);

			}
		}

		/// <summary>
		///     Finds the tenant associated with the specified name.
		/// </summary>
		/// <param name="tenantName">
		///     A string containing the name of the tenant to search for.
		/// </param>
		/// <returns>
		///     An object that represents the specified tenant; otherwise null if the tenant cannot be found.
		/// </returns>
		public static Tenant Find( string tenantName )
		{
			if ( String.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentException( "The specified tenantName parameter is invalid." );
			}

			Tenant tenant = FindByName( tenantName );

			if ( tenant == null )
			{
				TenantIdCache.Remove( tenantName );

				tenant = FindByName( tenantName );
			}

			return tenant;
		}

		/// <summary>
		///     Finds the tenant instance by name.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns>The tenant entity if found, null otherwise.</returns>
		private static Tenant FindByName( string tenantName )
		{
			/////
			// Get tenant ID
			/////
			long tenantId = GetTenantId( tenantName );

			if ( tenantId == -1 )
				return null;

			/////
			// Get tenant entity
			/////
			return Entity.Get<Tenant>( tenantId );
		}

		/// <summary>
		///     Flushes this instance.
		/// </summary>
		public static void Flush( )
		{
			TenantIdCache.Clear( );
		    TenantNameCache.Clear();

		}


		/// <summary>
		///     Gets all.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Tenant> GetAll( )
		{
			return Entity.GetInstancesOfType<Tenant>( false, "name" );
		}

		/// <summary>
		///     Gets the entity identifiers.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">@Invalid tenant identifier;tenantId</exception>
		private static List<long> GetEntityIdentifiers( long tenantId )
		{
			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid tenant identifier", "tenantId" );
			}

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			using ( var entityTypeContext = new EntityTypeContext( ) )
			{
				using ( var command = ctx.CreateCommand( ) )
				{
					command.CommandText = @"-- Get Tenant Ids
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )

SELECT
	Id = FromId,
	TypeId = ToId
FROM
	dbo.Relationship
WHERE
	TenantId = @tenantId AND TypeId = @isOfType
ORDER BY
	FromId";

					ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );

					using ( var reader = command.ExecuteReader( ) )
					{
						var ids = new List<long>( );

						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							long typeId = reader.GetInt64( 1 );

							entityTypeContext.Merge( id, typeId );

							ids.Add( id );
						}

						return ids;
					}
				}
			}
		}

		/// <summary>
		///     Finds the ID of the tenant associated with the specified name.
		/// </summary>
		/// <param name="tenantName">A string containing the name of the tenant to search for.</param>
		/// <param name="throwIfMissing">if set to <c>true</c> throws an exception if the tenant name cannot be found.</param>
		/// <returns>
		///     An object that represents the specified tenant; otherwise -1 if the tenant cannot be found.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">tenantName</exception>
		public static long GetTenantId( string tenantName, bool throwIfMissing = false )
		{
			if ( String.IsNullOrEmpty( tenantName ) )
			{
				throw new ArgumentNullException( "tenantName" );
			}

			if ( tenantName.Equals( SpecialStrings.GlobalTenant, StringComparison.CurrentCultureIgnoreCase ) )
			{
				return 0;
			}

			/////
			// Check tenant ID cache
			/////
			long tenantId;

			if ( TenantIdCache.TryGetValue( tenantName, out tenantId ) )
			{
				return tenantId;
			}

			using ( new GlobalAdministratorContext( ) )
			{
				/////
				// Find tenant by name
				/////
				Tenant tenant = Entity.GetByName<Tenant>( tenantName ).FirstOrDefault( );

				if ( tenant == null )
				{
					if ( throwIfMissing )
					{
						throw new Exception( "No tenants found with name " + tenantName );
					}

					return -1;
				}

				/////
				// Cache and return
				/////
				TenantIdCache [ tenantName ] = tenant.Id;

				return tenant.Id;
			}
		}

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        internal static string GetTenantName(long tenantId)
	    {
            if (tenantId < 0)
            {
                return string.Empty;
            }

            if (tenantId == 0)
            {
                return SpecialStrings.GlobalTenant;
            }

	        string name;
	        TenantNameCache.TryGetOrAdd(tenantId, out name, (id) =>
	        {
                using (new GlobalAdministratorContext())
                {                    
                    var tenant = Entity.Get<Tenant>(tenantId);

                    return tenant == null ? string.Empty : tenant.Name;
                }
            });

	        return name;
	    }

        /// <summary>
        ///     Invalidates this instance.
        /// </summary>
        internal static void Invalidate(EntityRef tenant)
        {
            Invalidate((IEntityRef)tenant);
        }

		/// <summary>
		///     Invalidates the current tenant.
		/// </summary>
		public static void Invalidate( )
		{
			Invalidate( RequestContext.TenantId );
		}

		/// <summary>
		///     Invalidates the specified tenant.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		public static void Invalidate( IEntityRef tenant )
		{
			if ( tenant == null )
			{
				return;
			}

            using ( new SecurityBypassContext() )
            {
			    using ( new EntityTypeContext( ) )
			    {
				    /////
				    // Invalidate all the caches that could possible hold this entity.
				    /////
				    var ids = GetEntityIdentifiers( tenant.Id );

				    using ( new DeferredChannelMessageContext( ) )
				    {
					    foreach ( var id in ids )
					    {
						    /////
						    // EntityCache implicitly removes from the EntityFieldCache
						    // as well as the EntityRelationshipCache (in both directions).
						    /////
						    EntityCache.Instance.Remove( id );
					    }
				    }


                    InvalidateLocalProcessImpl( tenant.Id );

				    using ( var channel = CreateMessageChannel( ) )
				    {
					    if ( channel != null )
					    {
                            var message = new TenantHelperMessage
                            {
                                MessageType = TenantHelperMessageType.InvalidateTenant,
							    TenantId = tenant.Id
						    };

						    message.EntityIds.UnionWith( ids );

						    channel.Publish( message, PublishOptions.None, false, MergeMessages );
					    }
				    }
			    }

			    string tenantName;

                using (new TenantAdministratorContext(0))
                {
                    tenantName = Entity.GetName(tenant.Id);
                }

			    UserAccountCache.Invalidate( tenantName );
            }
        }

        /// <summary>
        ///     Invalidates the specified tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        private static void InvalidateLocalProcessImpl( long tenantId )
        {
            // Invalidate all per-tenant cache entries.
            // Note: this only works for the PerTenantNonSharingCache, not the PerTenantCache.
            Factory.Current.Resolve<IPerTenantCacheInvalidator>( ).InvalidateTenant( tenantId );

            MetadataCacheInvalidator.Instance.InvalidateMetadataCaches( tenantId );
        }

        /// <summary>
        /// Merges the messages.
        /// </summary>
        /// <param name="existingMessage">The existing message.</param>
        /// <param name="newMessage">The new message.</param>
        private static void MergeMessages( TenantHelperMessage existingMessage, TenantHelperMessage newMessage )
		{
			existingMessage.EntityIds.UnionWith( newMessage.EntityIds );
		}

        #region Redis

        /// <summary>
        /// Notify the system that a new tenant was created
        /// </summary>
        /// <param name="tenantId"></param>
        public static void NotifyTenantCreate(long tenantId)
        {
            using (new SecurityBypassContext())
            {
                using (var channel = CreateMessageChannel())
                {
                    if (channel != null)
                    {
                        var message = new TenantHelperMessage
                        {
                            MessageType = TenantHelperMessageType.NewTenant,
                            TenantId = tenantId
                        };

                        channel.Publish(message, PublishOptions.None, true);
                    }
                }
            }
        }

        /********************************************************************************
        | !NOTE�
        |
        | This is entirely for the BulkResultCache cross app domain invalidation.
        |
        | !!! It has nothing to do with the Tenant ID cache that is contained herein ���
        |
        *********************************************************************************/
        private const string BulkResultCacheName = "TenantHelper_BulkResultCache";
        private static IChannel<TenantHelperMessage> _messageChannel;

	    /// <summary>
        /// Gets the message channel used for invalidating the BulkResultCache.
        /// </summary>
		private static IChannel<TenantHelperMessage> MessageChannel
        {
            get { return _messageChannel ?? (_messageChannel = CreateMessageChannel()); }
        }

        /// <summary>
        /// Creates the channel used to invalidate the BulkResultCache for pub/sub messaging.
        /// </summary>
        /// <returns>The channel.</returns>
		private static IChannel<TenantHelperMessage> CreateMessageChannel( )
	    {
			IChannel<TenantHelperMessage> channel = null;

	        try
	        {
                channel = Entity.DistributedMemoryManager.GetChannel<TenantHelperMessage>(BulkResultCacheName);
                channel.MessageReceived += ChannelOnMessageReceived;
                channel.Subscribe();
	        }
// ReSharper disable once EmptyGeneralCatchClause
	        catch
	        {
	            // Not a big deal. It may be during an install.
	        }

	        return channel;
	    }

		/// <summary>
		/// Handles the receipt of a message notifying that the tenant was invalidated in another app domain.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event args.</param>
		private static void ChannelOnMessageReceived( object sender, MessageEventArgs<TenantHelperMessage> e )
        {            
            if (e == null)
            {
                return;
            }

            long tenantId = e.Message.TenantId;

            switch (e.Message.MessageType)
            {
                case TenantHelperMessageType.InvalidateTenant:

                    using (Entity.DistributedMemoryManager.Suppress())
                    {
                        // To do: Consider using e.Message.EntityIds
                        InvalidateLocalProcessImpl( tenantId);
                    }
                    break;

                case TenantHelperMessageType.NewTenant:
                    Factory.Current.Resolve<IBackgroundTaskManager>().AddTenant(tenantId);
                    break;
            }               
        }

        /// <summary>
        /// Initializes the internal messaging channel used by the <see cref="TenantHelper"/> when cache invalidation
        /// must occur across app domains.
        /// </summary>
	    public static void InitializeMessageChannel()
        {
            var channel = MessageChannel;
#if !DEBUG
            if (channel == null)
            {
                EventLog.Application.WriteWarning("Failed when initializing message channel. [{0}]", BulkResultCacheName);
            }
#endif
        }

        #endregion

        /// <summary>
		///     Checks the tenant being used on the current request context for being disabled.
		/// </summary>
		/// <returns>True if the tenant has been disabled.</returns>
		public static bool IsDisabled( )
		{
			var ctx = RequestContext.GetContext( );

			if ( ctx == null || ctx.Tenant == null || ctx.Tenant.Id <= 0 ) return false;

			using ( new GlobalAdministratorContext( ) )
			{
				var tenant = Entity.Get<Tenant>( ctx.Tenant.Id, new IEntityRef[ ]
				{
					Tenant.IsTenantDisabled_Field
				} );
				if ( tenant != null )
				{
					return tenant.IsTenantDisabled == true;
				}
			}

			return false;
		}

		/// <summary>
		///     Island class.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class Island<T>
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="Island{T}" /> class.
			/// </summary>
			/// <param name="start">The start.</param>
			/// <param name="end">The end.</param>
			public Island( T start, T end )
			{
				Start = start;
				End = end;
			}

			/// <summary>
			///     Gets the end.
			/// </summary>
			/// <value>
			///     The end.
			/// </value>
			public T End
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets the start.
			/// </summary>
			/// <value>
			///     The start.
			/// </value>
			public T Start
			{
				get;
				private set;
			}
		}
	}
}