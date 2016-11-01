// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The PrewarmTenant class.
	/// </summary>
	/// <seealso cref="ConfigurationElement" />
	public class PrewarmTenant : ConfigurationElement
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PrewarmTenant" /> class.
		/// </summary>
		public PrewarmTenant( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PrewarmTenant" /> class.
		/// </summary>
		/// <param name="name">The tenant name.</param>
		public PrewarmTenant( string name ) : this( )
		{
			Name = name;
		}

		/// <summary>
		///     Gets or sets the tenant name.
		/// </summary>
		/// <value>
		///     The tenant name.
		/// </value>
		[ConfigurationProperty( "name", IsRequired = true, IsKey = true )]
		public string Name
		{
			get
			{
				return ( string ) this[ "name" ];
			}
			set
			{
				this[ "name" ] = value;
			}
		}
	}
}