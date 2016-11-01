// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     Redis Manager tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class RedisManagerTests
	{
		/// <summary>
		///     The default timeout
		/// </summary>
		private const int DefaultTimeout = 2500;

		/// <summary>
		///     Gets the user account cache count.
		/// </summary>
		/// <returns></returns>
		private int GetUserAccountCacheCount( )
		{
			object cache = typeof ( UserAccountCache ).InvokeMember( "_cache", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField, null, null, null );

			var count = ( int ) cache.GetType( ).InvokeMember( "Count", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, cache, null );

			return count;
		}

		/// <summary>
		///     Gets the user account cache tenant count.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		/// <returns></returns>
		private int GetUserAccountCacheTenantCount( string tenantName )
		{
			object cache = typeof ( UserAccountCache ).InvokeMember( "_cache", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField, null, null, null );

			object tenantCache = cache.GetType( ).InvokeMember( "get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, cache, new object[ ]
			{
				tenantName
			} );

			int count = 0;

			if ( tenantCache != null )
			{
				object innerCache = tenantCache.GetType( ).InvokeMember( "_innerCache", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, tenantCache, null );

				count = ( int ) innerCache.GetType( ).InvokeMember( "Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, null, innerCache, null );
			}

			return count;
		}

		/// <summary>
		///     Tests the bulk publisher.
		/// </summary>
		[Test]
		public void TestBulkPublisher( )
		{
			const string channelName = "myChannel";

			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<TestMessage>, bool> predicate = m => m.Message.Action == PublisherActions.Remove && m.Message.Items.Count == 2 && m.Message.Items.Contains( "a,:" ) && m.Message.Items.Contains( "b,:" );

				var instance = domain.InjectType<SubscriberRefObject<TestMessage>>( channelName, RunAsDefaultTenant.DefaultTenantName, predicate );

				using ( IDistributedMemoryManager manager = new RedisManager( ) )
				{
					manager.Connect( );

                    using(DeferredChannelMessageContext context = new DeferredChannelMessageContext())
                    {
                        using (IChannel<TestMessage> channel1 = manager.GetChannel<TestMessage>(channelName))
                        {
                            var message1 = new TestMessage
                            {
                                Action = PublisherActions.Remove
                            };

                            message1.Items.Add("a,:");

                            channel1.Publish(message1, PublishOptions.None, false, (e, n) => e.Items.AddRange(n.Items));

                            using (IChannel<TestMessage> channel2 = manager.GetChannel<TestMessage>(channelName))
                            {
                                var message2 = new TestMessage
                                {
                                    Action = PublisherActions.Remove
                                };

                                message2.Items.Add("b,:");

                                channel2.Publish(message2, PublishOptions.None, false, (e, n) => e.Items.AddRange(n.Items));
                            }
                        }
                    }					

					bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

					Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

					Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

					MessageEventArgs<TestMessage> args = instance.ReceivedMessages.FirstOrDefault( predicate );

					Assert.IsNotNull( args, "Received message is invalid." );
				}
			}
		}

		/// <summary>
		///     Tests connecting.
		/// </summary>
		[Test]
		public void TestConnect( )
		{
			using ( var manager = new RedisManager( ) )
			{
				bool isConnected = manager.IsConnected;

				Assert.IsFalse( isConnected, "New redis manager should start disconnected." );

				manager.Connect( );

				isConnected = manager.IsConnected;

				Assert.IsTrue( isConnected, "Failed to connect to redis." );
			}
		}

		/// <summary>
		///     Tests the disconnect.
		/// </summary>
		[Test]
		public void TestDisconnect( )
		{
			using ( var manager = new RedisManager( ) )
			{
				bool isConnected = manager.IsConnected;

				Assert.IsFalse( isConnected, "New redis manager should start disconnected." );

				manager.Connect( );

				isConnected = manager.IsConnected;

				Assert.IsTrue( isConnected, "Failed to connect to redis." );

				manager.Disconnect( );

				isConnected = manager.IsConnected;

				Assert.IsFalse( isConnected, "Failed to disconnect from redis." );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntityCacheClear( )
		{
			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<EntityCacheMessage>, bool> predicate = m => m.Message.Clear;

				var instance = domain.InjectType<SubscriberRefObject<EntityCacheMessage>, EntityCacheMessage>( EntityCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

				/////
				// Clear the cache from the primary domain.
				/////
				EntityCache.Instance.Clear( );

				bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

				Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

				Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

				MessageEventArgs<EntityCacheMessage> message = instance.ReceivedMessages.FirstOrDefault( predicate );

				Assert.IsNotNull( message, "Received message is invalid" );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntityCacheRemove( )
		{
			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<EntityCacheMessage>, bool> predicate = m => m.Message.RemoveKeys.Any( rk => rk.Id == new EntityRef( "core:resource" ).Id );

				var instance = domain.InjectType<SubscriberRefObject<EntityCacheMessage>, EntityCacheMessage>( EntityCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

				var entityRef = new EntityRef( "core:resource" );

				/////
				// Get an entity in the primary domain.
				/////
				Entity.Get( entityRef );

				/////
				// Clear the cache from the primary domain.
				/////
				EntityCache.Instance.Remove( entityRef.Id );

				bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

				Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

				Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

				MessageEventArgs<EntityCacheMessage> message = instance.ReceivedMessages.FirstOrDefault( predicate );

				Assert.IsNotNull( message, "Received message is invalid" );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntityFieldCacheBulkRemove( )
		{
			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<EntityFieldCacheMessage>, bool> predicate = m => m.Message.RemoveKeys.Any( rk => rk.Id == new EntityRef( "core:resource" ).Id );

				var instance = domain.InjectType<SubscriberRefObject<EntityFieldCacheMessage>, EntityFieldCacheMessage>( EntityFieldCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

				var entityRef = new EntityRef( "core:resource" );
				var fieldRef = new EntityRef( "core:name" );

				/////
				// Get an entity in the primary domain.
				/////
				Entity.Get( entityRef, fieldRef );

				MessageEventArgs<EntityFieldCacheMessage> message;

                using (DeferredChannelMessageContext context = new DeferredChannelMessageContext())
                {
                    using (Entity.DistributedMemoryManager.GetChannel<EntityFieldCacheMessage>(EntityFieldCache.CacheName))
                    {
                        /////
                        // Clear the cache from the primary domain.
                        /////
                        EntityFieldCache.Instance.Remove(entityRef.Id);

                        Thread.Sleep(DefaultTimeout);

                        message = instance.ReceivedMessages.FirstOrDefault(predicate);

                        Assert.IsNull(message, "No message should have been received.");
                    }
                }

				bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

				Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

				Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

				message = instance.ReceivedMessages.FirstOrDefault( predicate );

				Assert.IsNotNull( message, "Received message is invalid" );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntityFieldCacheClear( )
		{
			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<EntityFieldCacheMessage>, bool> predicate = m => m.Message.Clear;

				var instance = domain.InjectType<SubscriberRefObject<EntityFieldCacheMessage>, EntityFieldCacheMessage>( EntityFieldCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

				var entityRef = new EntityRef( "core:resource" );
				var fieldRef = new EntityRef( "core:name" );

				/////
				// Get an entity in the primary domain.
				/////
				Entity.Get( entityRef, fieldRef );

				/////
				// Clear the cache from the primary domain.
				/////
				EntityFieldCache.Instance.Clear( );

				bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

				Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

				Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

				MessageEventArgs<EntityFieldCacheMessage> message = instance.ReceivedMessages.FirstOrDefault( predicate );

				Assert.IsNotNull( message, "Received message is invalid" );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestEntityFieldCacheRemove( )
		{
			using ( var domain = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<EntityFieldCacheMessage>, bool> predicate = m => m.Message.RemoveKeys.Any( rk => rk.Id == new EntityRef( "core:resource" ).Id );

				var instance = domain.InjectType<SubscriberRefObject<EntityFieldCacheMessage>, EntityFieldCacheMessage>( EntityFieldCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

				var entityRef = new EntityRef( "core:resource" );
				var fieldRef = new EntityRef( "core:name" );

				/////
				// Get an entity in the primary domain.
				/////
				Entity.Get( entityRef, fieldRef );

				/////
				// Clear the cache from the primary domain.
				/////
				EntityFieldCache.Instance.Remove( entityRef.Id );

				bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

				Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

				Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1, "Received message count is invalid." );

				MessageEventArgs<EntityFieldCacheMessage> message = instance.ReceivedMessages.FirstOrDefault( predicate );

				Assert.IsNotNull( message, "Received message is invalid" );
			}
		}

		/// <summary>
		///     Tests reconnecting.
		/// </summary>
		[Test]
		public void TestReconnect( )
		{
			using ( var manager = new RedisManager( ) )
			{
				bool isConnected = manager.IsConnected;

				Assert.IsFalse( isConnected, "New redis manager should start disconnected." );

				manager.Connect( );

				isConnected = manager.IsConnected;

				Assert.IsTrue( isConnected, "Failed to connect to redis." );

				ServiceHelper.StopService( );

				isConnected = manager.IsConnected;

				Assert.IsFalse( isConnected, "Failed to automatically disconnect when redis shutdown." );

				ServiceHelper.StartService( );

				int retries = 0;

				while ( retries < 5 && ! isConnected )
				{
					isConnected = manager.IsConnected;

					if ( !isConnected )
					{
						Thread.Sleep( 1000 );
						retries++;
					}
				}

				Assert.IsTrue( isConnected, "Failed to automatically reconnect when redis started back up." );
			}
		}

		/// <summary>
		///     Tests the subscriber.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestSubscriber( )
		{
			const string channelName = "myChannel";

			using ( var domain1 = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<TestMessage>, bool> predicate = m => m.Message.Action == "myMessage";

				var instance1 = domain1.InjectType<SubscriberRefObject<TestMessage>>( channelName, RunAsDefaultTenant.DefaultTenantName, predicate );

				using ( var domain2 = new TestAppDomain( ) )
				{
					var instance2 = domain2.InjectType<SubscriberRefObject<TestMessage>>( channelName, RunAsDefaultTenant.DefaultTenantName, predicate );

					using ( IDistributedMemoryManager manager = new RedisManager( ) )
					{
						manager.Connect( );

						using ( IChannel<TestMessage> channel = manager.GetChannel<TestMessage>( channelName ) )
						{
							var message = new TestMessage( );
							message.Action = "myMessage";

							channel.Publish( message );

							bool waitOne1 = instance1.MessageReceived.WaitOne( DefaultTimeout );
							bool waitOne2 = instance2.MessageReceived.WaitOne( DefaultTimeout );

							Assert.IsTrue( waitOne1, "No message received in " + DefaultTimeout + "ms for app domain 1." );
							Assert.IsTrue( waitOne2, "No message received in " + DefaultTimeout + "ms for app domain 2." );

							instance2.Unsubscribe( );

							channel.Publish( message );

							waitOne1 = instance1.MessageReceived.WaitOne( DefaultTimeout );
							waitOne2 = instance2.MessageReceived.WaitOne( DefaultTimeout );

							Assert.IsTrue( waitOne1, "No message received in " + DefaultTimeout + "ms for app domain 1." );
							Assert.IsFalse( waitOne2, "Message received in app domain 1 when it should not have been." );

							instance1.Unsubscribe( );

							var channelMessage = new TestMessage( );

							channel.Publish( channelMessage );

							waitOne1 = instance1.MessageReceived.WaitOne( DefaultTimeout );
							waitOne2 = instance2.MessageReceived.WaitOne( DefaultTimeout );

							Assert.IsFalse( waitOne1, "Message received in app domain 1 when it should not have been." );
							Assert.IsFalse( waitOne2, "Message received in app domain 2 when it should not have been." );
						}
					}
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void UserAccountCacheTest( )
		{
			const string username = "TestUser123abc";
			const string password = "Abc123!@#";

			UserAccount account = null;

			try
			{
				UserAccount existingAccount = Entity.GetByName<UserAccount>( username, true ).FirstOrDefault( );

				if ( existingAccount != null )
				{
					existingAccount.Delete( );
				}

				account = new UserAccount
				{
					Name = username,
					Password = password,
					AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active
				};

				account.Save( );

				Assert.AreEqual( 0, GetUserAccountCacheCount( ) );
				Assert.AreEqual( 0, GetUserAccountCacheTenantCount( RunAsDefaultTenant.DefaultTenantName ) );

				UserAccountCache.GetRequestContext( username, password, RunAsDefaultTenant.DefaultTenantName );

				Assert.AreEqual( 1, GetUserAccountCacheCount( ) );
				Assert.AreEqual( 1, GetUserAccountCacheTenantCount( RunAsDefaultTenant.DefaultTenantName ) );

				using ( var domain = new TestAppDomain( ) )
				{
					Func<MessageEventArgs<TestMessage>, bool> predicate = m => m.Message.Action == "EDC" && m.Message.Items.Contains( username );

					var instance = domain.InjectType<SubscriberRefObject<TestMessage>>( UserAccountCache.CacheName, RunAsDefaultTenant.DefaultTenantName, predicate );

					UserAccountCache.Invalidate( username, RunAsDefaultTenant.DefaultTenantName );

					bool waitOne = instance.MessageReceived.WaitOne( DefaultTimeout );

					Assert.IsTrue( waitOne, "No message received in " + DefaultTimeout + "ms." );

					Assert.GreaterOrEqual( instance.ReceivedMessages.Count, 1 );

					MessageEventArgs<TestMessage> message = instance.ReceivedMessages.FirstOrDefault( predicate );

					Assert.IsNotNull( message, "Received message is invalid" );
				}
			}
			finally
			{
				if ( account != null )
				{
					account.Delete( );
				}
			}
		}

		[Test]
		public void TestAsync( )
		{
			const string channelName = "myChannel";

			using ( var domain1 = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<TestMessage>, bool> predicate = m => m.Message.Action == "myMessage";

				var instance1 = domain1.InjectType<SubscriberRefObject<TestMessage>>( channelName, RunAsDefaultTenant.DefaultTenantName, predicate );

				using ( IDistributedMemoryManager manager = new RedisManager( ) )
				{
					manager.Connect( );

					const int count = 1000;

					using ( IChannel<TestMessage> channel = manager.GetChannel<TestMessage>( channelName ) )
					{
						RunAsync( channel, count );

						instance1.MessageReceived.WaitOne( DefaultTimeout );

						Assert.Greater( instance1.ReceivedMessages.Count, 0 );
					}
				}
			}
		}

		[Test]
		public void TestSync( )
		{
			const string channelName = "myChannel";

			using ( var domain1 = new TestAppDomain( ) )
			{
				Func<MessageEventArgs<TestMessage>, bool> predicate = m => m.Message.Action == "myMessage";

				var instance1 = domain1.InjectType<SubscriberRefObject<TestMessage>>( channelName, RunAsDefaultTenant.DefaultTenantName, predicate );

				using ( IDistributedMemoryManager manager = new RedisManager( ) )
				{
					manager.Connect( );

					const int count = 1000;

					using ( IChannel<TestMessage> channel = manager.GetChannel<TestMessage>( channelName ) )
					{
						RunSync( channel, count );

						instance1.MessageReceived.WaitOne( DefaultTimeout );

						Assert.Greater( instance1.ReceivedMessages.Count, 0 );
					}
				}
			}
		}

		private void RunAsync( IChannel<TestMessage> channel, int count )
		{
			var tasks = new List<Task>( );

			var message = new TestMessage( );

			var sw = new Stopwatch( );
			sw.Start( );

			for ( int i = 0; i < count; i++ )
			{
				var task = channel.PublishAsync( message, PublishMethod.Immediate );

				tasks.Add( task );
			}

			Task.WaitAll( tasks.ToArray( ), -1 );

			sw.Stop( );

			Console.WriteLine( @"Async Time: " + sw.ElapsedMilliseconds + @"ms");
		}

		private void RunSync( IChannel<TestMessage> channel, int count )
		{
			var message = new TestMessage( );

			var sw = new Stopwatch( );
			sw.Start( );

			for ( int i = 0; i < count; i++ )
			{
				channel.Publish( message, PublishMethod.Immediate );
			}

			sw.Stop( );

			Console.WriteLine( @"Sync Time: " + sw.ElapsedMilliseconds + @"ms" );
		}
	}
}