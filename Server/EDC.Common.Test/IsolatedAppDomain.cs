// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.Reflection;

namespace EDC.Test
{
	/// <summary>
	///     Creates an instance of the specified type in an isolated app domain.
	/// </summary>
	/// <typeparam name="T">Type of object to be created in the isolated app domain.</typeparam>
	public sealed class IsolatedAppDomain<T> : IDisposable
		where T : MarshalByRefObject
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="IsolatedAppDomain{T}" /> class.
		/// </summary>
		public IsolatedAppDomain( )
			: this( null )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="IsolatedAppDomain{T}" /> class.
		/// </summary>
		/// <param name="args">The instance constructor arguments.</param>
		public IsolatedAppDomain( params object[] args )
		{
			Domain = AppDomain.CreateDomain( "IsolatedAppDomain:" + Guid.NewGuid( ), null, AppDomain.CurrentDomain.SetupInformation );

			Type type = typeof ( T );

			Instance = ( T ) Domain.CreateInstanceAndUnwrap( type.Assembly.FullName, type.FullName, true, BindingFlags.CreateInstance, null, args, CultureInfo.CurrentCulture, null );
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		/// <value>
		///     The instance.
		/// </value>
		public T Instance
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the domain.
		/// </summary>
		/// <value>
		///     The domain.
		/// </value>
		private AppDomain Domain
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Domain != null )
			{
				AppDomain.Unload( Domain );

				Domain = null;
			}
		}
	}
}