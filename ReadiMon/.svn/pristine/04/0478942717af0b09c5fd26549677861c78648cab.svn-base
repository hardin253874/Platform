// Copyright 2011-2014 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.Linq;

namespace ReadiMon.AddinView.Configuration
{
	/// <summary>
	///     ConfigurationSectionCollection Extension Methods.
	/// </summary>
	public static class ConfigurationSectionCollectionExtensionMethods
	{
		/// <summary>
		///     Determines whether the configuration section collection contains the specified name.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static bool Contains( this ConfigurationSectionCollection collection, string name )
		{
			if ( collection == null )
			{
				return false;
			}

			return collection.Keys.Cast<string>( ).Contains( name, StringComparer.InvariantCulture );
		}
	}
}