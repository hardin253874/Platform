// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     The Pre-warm Configuration class.
	/// </summary>
	/// <seealso cref="ConfigurationSection" />
	[DebuggerStepThrough]
	public class PrewarmConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets the server settings.
		/// </summary>
		/// <value>
		///     The server settings.
		/// </value>
		[ConfigurationProperty( "tenants", IsDefaultCollection = false )]
		[ConfigurationCollection( typeof ( PrewarmTenant ), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove" )]
		public PrewarmTenantCollection Tenants
		{
			get
			{
				return ( PrewarmTenantCollection ) this[ "tenants" ];
			}

			set
			{
				this[ "tenants" ] = value;
			}
		}
	}
}