// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ServiceProcess;
using System.Threading;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     Service Helper class.
	/// </summary>
	public static class ServiceHelper
	{
		/// <summary>
		///     The service name
		/// </summary>
		private const string ServiceName = "Redis";

		/// <summary>
		///     Starts the named service.
		/// </summary>
		public static void StartService( )
		{
			var sc = new ServiceController( ServiceName );

			Assert.AreEqual( ServiceControllerStatus.Stopped, sc.Status );

			sc.Start( );
			sc.WaitForStatus( ServiceControllerStatus.Running, TimeSpan.FromSeconds( 10 ) );

			Assert.AreEqual( ServiceControllerStatus.Running, sc.Status );

			Thread.Sleep( 1000 );
		}

		/// <summary>
		///     Stops the named service.
		/// </summary>
		public static void StopService( )
		{
			var sc = new ServiceController( ServiceName );

			Assert.AreEqual( ServiceControllerStatus.Running, sc.Status );

			sc.Stop( );
			sc.WaitForStatus( ServiceControllerStatus.Stopped, TimeSpan.FromSeconds( 10 ) );

			Assert.AreEqual( ServiceControllerStatus.Stopped, sc.Status );

			Thread.Sleep( 1000 );
		}
	}
}