// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     RedisServer collection class.
	/// </summary>
	public class RedisServerCollection : ConfigurationElementCollection
	{
		/// <summary>
		///     Gets or sets the <see cref="RedisServer" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="RedisServer" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public RedisServer this[ int index ]
		{
			get
			{
				return ( RedisServer ) BaseGet( index );
			}
			set
			{
				if ( BaseGet( index ) != null )
				{
					BaseRemoveAt( index );
				}

				BaseAdd( value );
			}
		}

		/// <summary>
		///     Adds the specified server.
		/// </summary>
		/// <param name="server">The server.</param>
		public void Add( RedisServer server )
		{
			BaseAdd( server );
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public void Clear( )
		{
			BaseClear( );
		}

		/// <summary>
		///     When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
		/// </summary>
		/// <returns>
		///     A new <see cref="T:System.Configuration.ConfigurationElement" />.
		/// </returns>
		protected override ConfigurationElement CreateNewElement( )
		{
			return new RedisServer( );
		}

		/// <summary>
		///     Gets the element key for a specified configuration element when overridden in a derived class.
		/// </summary>
		/// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
		/// <returns>
		///     An <see cref="T:System.Object" /> that acts as the key for the specified
		///     <see cref="T:System.Configuration.ConfigurationElement" />.
		/// </returns>
		protected override object GetElementKey( ConfigurationElement element )
		{
			return ( ( RedisServer ) element ).HostName;
		}

		/// <summary>
		///     Removes the specified server.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <exception cref="System.ArgumentNullException">server</exception>
		public void Remove( RedisServer server )
		{
			if ( server == null )
			{
				throw new ArgumentNullException( "server" );
			}

			Remove( server.HostName );
		}

		/// <summary>
		///     Removes the specified host.
		/// </summary>
		/// <param name="host">The host.</param>
		public void Remove( string host )
		{
			BaseRemove( host );
		}

		/// <summary>
		///     Removes at.
		/// </summary>
		/// <param name="index">The index.</param>
		public void RemoveAt( int index )
		{
			BaseRemoveAt( index );
		}
	}
}