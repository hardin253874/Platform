// Copyright 2011-2014 Global Software Innovation Pty Ltd

using System;
using System.Configuration;
using System.Linq;

namespace ReadiMon.AddinView.Configuration
{
	/// <summary>
	///     PropertyInformationCollection extension methods.
	/// </summary>
	public static class PropertyInformationCollectionExtensions
	{
		/// <summary>
		///     Determines whether the PropertyInformationCollection contains the specified property name.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		public static bool Contains( this PropertyInformationCollection collection, string propertyName )
		{
			return ( from object property in collection select property as PropertyInformation ).Any( info => info != null && info.Name.Equals( propertyName, StringComparison.CurrentCultureIgnoreCase ) );
		}
	}
}