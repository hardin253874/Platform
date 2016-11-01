// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The PrewarmTenant Collection class.
	/// </summary>
	/// <seealso cref="ConfigurationElementCollection" />
	public class PrewarmTenantCollection : ConfigurationElementCollection
	{
		/// <summary>
		///     Gets or sets the <see cref="PrewarmTenant" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="PrewarmTenant" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public PrewarmTenant this[ int index ]
		{
			get
			{
				return ( PrewarmTenant ) BaseGet( index );
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
		///     Adds the specified tenant.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		public void Add( PrewarmTenant tenant )
		{
			BaseAdd( tenant );
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public void Clear( )
		{
			BaseClear( );
		}

		/// <summary>
		///     Removes the specified tenant.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		/// <exception cref="System.ArgumentNullException">tenant</exception>
		public void Remove( PrewarmTenant tenant )
		{
			if ( tenant == null )
			{
				throw new ArgumentNullException( "tenant" );
			}

			Remove( tenant.Name );
		}

		/// <summary>
		///     Removes the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		public void Remove( string name )
		{
			BaseRemove( name );
		}

		/// <summary>
		///     Removes at.
		/// </summary>
		/// <param name="index">The index.</param>
		public void RemoveAt( int index )
		{
			BaseRemoveAt( index );
		}

		/// <summary>
		///     When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
		/// </summary>
		/// <returns>
		///     A newly created <see cref="T:System.Configuration.ConfigurationElement" />.
		/// </returns>
		protected override ConfigurationElement CreateNewElement( )
		{
			return new PrewarmTenant( );
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
			return ( ( PrewarmTenant ) element ).Name;
		}
	}
}