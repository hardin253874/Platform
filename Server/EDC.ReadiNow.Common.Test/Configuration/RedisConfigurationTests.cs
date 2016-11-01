// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Configuration;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Configuration
{
	/// <summary>
	///     Redis configuration tests.
	/// </summary>
	[TestFixture]
	public class RedisConfigurationTests
	{
		/// <summary>
		///     Test adding a server.
		/// </summary>
		[Test]
		[Ignore( "No longer supported by our configuration settings." )]
		public void AddServer( )
		{
			var configuration = ConfigurationSettings.GetRedisConfigurationSection( );

			Assert.AreEqual( 1, configuration.Servers.Count );

			configuration.Servers.Add( new RedisServer( "abc", 123 ) );

			ConfigurationSettings.UpdateRedisConfigurationSection( configuration );

			Assert.AreEqual( 2, configuration.Servers.Count );

			RedisServer server = configuration.Servers[ 0 ];

			Assert.IsNotNull( server );
			Assert.AreEqual( "localhost", server.HostName );
			Assert.AreEqual( 6379, server.Port );

			server = configuration.Servers[ 1 ];

			Assert.IsNotNull( server );
			Assert.AreEqual( "abc", server.HostName );
			Assert.AreEqual( 123, server.Port );

			configuration.Servers.Remove( "abc" );

			ConfigurationSettings.UpdateRedisConfigurationSection( configuration );

			Assert.AreEqual( 1, configuration.Servers.Count );
		}

		/// <summary>
		///     Test retrieving the servers.
		/// </summary>
		[Test]
		public void GetServers( )
		{
			var configuration = ConfigurationSettings.GetRedisConfigurationSection( );

			Assert.AreEqual( 1, configuration.Servers.Count );

			RedisServer server = configuration.Servers[ 0 ];

			Assert.IsNotNull( server );
			Assert.AreEqual( "localhost", server.HostName );
			Assert.AreEqual( 6379, server.Port );
		}

		/// <summary>
		///     Test removing a server.
		/// </summary>
		[Test]
		[Ignore( "No longer supported by our configuration settings." )]
		public void RemoveServer( )
		{
			var configuration = ConfigurationSettings.GetRedisConfigurationSection( );

			Assert.AreEqual( 1, configuration.Servers.Count );

			RedisServer server = configuration.Servers[ 0 ];

			configuration.Servers.RemoveAt( 0 );

			ConfigurationSettings.UpdateRedisConfigurationSection( configuration );

			Assert.AreEqual( 0, configuration.Servers.Count );

			configuration.Servers.Add( server );

			ConfigurationSettings.UpdateRedisConfigurationSection( configuration );

			Assert.AreEqual( 1, configuration.Servers.Count );
		}
	}
}