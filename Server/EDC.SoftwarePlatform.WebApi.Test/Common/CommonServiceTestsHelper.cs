// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using EDC.Globalization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Common
{
	/// <summary>
	///     Static helper class containing common functions for interacting with services.
	/// </summary>
	public static class CommonServiceTestsHelper
	{
		/// <summary>
		///     The synchronize root
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     Default context data.
		/// </summary>
		public static RequestContextData ZeroTenantContextData = Global.DefaultContextData;

		/// <summary>
		///     Default context data.
		/// </summary>
		private static RequestContextData _testDefaultContextData;


		/// <summary>
		///     Gets the EDC Tenant info object.
		/// </summary>
		public static TenantInfo EdcTenant
		{
			get
			{
				return Global.EdcTenant;
			}
		}

		/// <summary>
		///     Gets the test default context data.
		/// </summary>
		/// <value>
		///     The test default context data.
		/// </value>
		public static RequestContextData TestDefaultContextData
		{
			get
			{
				if ( _testDefaultContextData == null )
				{
					lock ( SyncRoot )
					{
						if ( _testDefaultContextData == null )
						{
							_testDefaultContextData = new RequestContextData( new IdentityInfo( 0, SpecialStrings.TenantAdministratorUser ), EdcTenant, CultureHelper.GetUiThreadCulture( CultureType.Neutral ) );
						}
					}
				}

				return _testDefaultContextData;
			}
		}

		/// <summary>
		///     Run an action and assert it is faster than specified.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="timeInMs">time in MS</param>
		/// <param name="message"></param>
		public static void AssertFasterThan( Action action, int timeInMs, string message )
		{
			string timeMessage = "";
			var start = DateTime.Now;

			action( );

			var end = DateTime.Now;

			var diff = end - start;


			if ( diff >= new TimeSpan( 0, 0, 0, 0, timeInMs ) )
				timeMessage = string.Format( "{0}. Expected maximum {1}ms. Time taken: {2}ms", message, timeInMs, diff.Milliseconds );

			Assert.IsTrue( diff < new TimeSpan( 0, 0, 0, 0, timeInMs ), timeMessage );
		}

		/// <summary>
		///     Calls a service
		/// </summary>
		/// <param name="action">
		///     Action to execute when running the service call.
		/// </param>
		public static void CallService<T>( Action<T, AutoResetEvent> action ) where T : new( )
		{
			var service = new T( );
			var waitHandle = new AutoResetEvent( false );

#if DEBUG
			TimeSpan waitTime = TimeSpan.FromSeconds( 60000 );
#else
            TimeSpan waitTime = TimeSpan.FromSeconds( 5000 );
#endif

			using ( new CustomContext( TestDefaultContextData ) )
			{
				action( service, waitHandle );
			}

			/////
			// Wait for the call to complete.
			/////
			if ( !waitHandle.WaitOne( waitTime ) )
			{
				throw new TimeoutException( "The callback timeout has expired" );
			}
		}

		/// <summary>
		///     Calls a service
		/// </summary>
		/// <param name="action">
		///     Action to execute when running the service call.
		/// </param>
		public static void CallServiceWithEdcContext<T>( Action<T, AutoResetEvent> action ) where T : new( )
		{
			var service = new T( );
			var waitHandle = new AutoResetEvent( false );

#if DEBUG
			TimeSpan waitTime = TimeSpan.FromSeconds( 60000 );
#else
            TimeSpan waitTime = TimeSpan.FromSeconds( 5000 );
#endif

			using ( new CustomContext( TestDefaultContextData ) )
			{
				action( service, waitHandle );
			}

			/////
			// Wait for the call to complete.
			/////
			if ( !waitHandle.WaitOne( waitTime ) )
			{
				throw new TimeoutException( "The callback timeout has expired" );
			}
		}


		/// <summary>
		///     Serializes the structured query to xml.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static string SerializeToXml( object obj )
		{
			var sb = new StringBuilder( );
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = false,
				Encoding = Encoding.UTF8
			};

			using ( XmlWriter xmlWriter = XmlWriter.Create( sb, settings ) )
			{
				var serializer = new DataContractSerializer( obj.GetType( ) );
				serializer.WriteObject( xmlWriter, obj );
				xmlWriter.Flush( );
			}

			return sb.ToString( );
		}
	}
}