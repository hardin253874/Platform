// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;
using System.Diagnostics;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Redis configuration class.
	/// </summary>
	[DebuggerStepThrough]
	public class RedisConfiguration : ConfigurationSection
	{
		/// <summary>
		///     Gets or sets the server settings.
		/// </summary>
		/// <value>
		///     The server settings.
		/// </value>
		[ConfigurationProperty( "servers", IsDefaultCollection = false )]
		[ConfigurationCollection( typeof ( RedisServer ), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove" )]
		public RedisServerCollection Servers
		{
			get
			{
				return ( ( RedisServerCollection ) this[ "servers" ] );
			}

			set
			{
				this[ "servers" ] = value;
			}
		}
	}
}