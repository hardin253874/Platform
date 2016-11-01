// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
	/// <summary>
	///     App Domain helper class.
	/// </summary>
	public static class AppDomainHelper
	{
		/// <summary>
		///     Creates the application domain.
		/// </summary>
		/// <returns></returns>
		public static AppDomain CreateAppDomain( string friendlyName = "RedisManagerTest Domain" )
		{
			AppDomain callingDomain = Thread.GetDomain( );

			var setup = new AppDomainSetup
			{
				ApplicationBase = callingDomain.SetupInformation.ApplicationBase
			};

			AppDomain domain = AppDomain.CreateDomain( friendlyName, null, setup );

			return domain;
		}

		/// <summary>
		/// Unloads the specified domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		public static void Unload( AppDomain domain )
		{
			AppDomain.Unload( domain );
		}

		/// <summary>
		///     Injects the type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="domain">The domain.</param>
		/// <param name="constructorArgs">The constructor arguments.</param>
		/// <returns></returns>
		public static T InjectType<T>( AppDomain domain, object[ ] constructorArgs = null )
		{
			var type = typeof ( T );

			var instance = ( T ) domain.CreateInstanceAndUnwrap( type.Assembly.FullName, type.FullName, false, BindingFlags.Default, null, constructorArgs, CultureInfo.CurrentCulture, null );

			return instance;
		}
	}
}