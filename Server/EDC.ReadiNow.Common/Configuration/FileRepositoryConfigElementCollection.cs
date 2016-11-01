// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
	/// <summary>
	///     Represents a file repository configuration collection
	/// </summary>
	public class FileRepositoryConfigElementCollection : ConfigurationElementCollection
	{
		/// <summary>
		///     Adds the specified element.
		/// </summary>
		/// <param name="element">The element.</param>
		public void Add( FileRepositoryConfigElement element )
		{
			BaseAdd( element );
		}

		/// <summary>
		///     Creates a new element.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement( )
		{
			return new FileRepositoryConfigElement( );
		}

		/// <summary>
		///     Gets the element key.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey( ConfigurationElement element )
		{
			return ( ( FileRepositoryConfigElement ) element ).Name;
		}
	}
}