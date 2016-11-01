// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;

namespace EDC.Xml
{
	/// <summary>
	///     Xml extension methods.
	/// </summary>
	public static class XmlExtensions
	{
		/// <summary>
		///     Reads the GUID.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <param name="xpath">The xpath.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		/// <exception cref="System.NullReferenceException"></exception>
		public static Guid ReadGuid( this XmlDocument doc, string xpath, Guid defaultValue )
		{
			if ( doc == null )
			{
				throw new NullReferenceException( );
			}

			XmlNode node = doc.SelectSingleNode( xpath );

			if ( node != null )
			{
				Guid result;

				if ( Guid.TryParse( node.InnerText, out result ) )
				{
					return result;
				}
			}

			return defaultValue;
		}

		/// <summary>
		///     Reads the GUID.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <param name="xpath">The xpath.</param>
		/// <returns></returns>
		public static Guid ReadGuid( this XmlDocument doc, string xpath )
		{
			return ReadGuid( doc, xpath, Guid.Empty );
		}

		/// <summary>
		///     Reads the string.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <param name="xpath">The x path.</param>
		/// <returns></returns>
		public static string ReadString( this XmlDocument doc, string xpath )
		{
			return ReadString( doc, xpath, null );
		}

		/// <summary>
		///     Reads the string.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <param name="xpath">The x path.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		/// <exception cref="System.NullReferenceException"></exception>
		public static string ReadString( this XmlDocument doc, string xpath, string defaultValue )
		{
			if ( doc == null )
			{
				throw new NullReferenceException( );
			}

			XmlNode node = doc.SelectSingleNode( xpath );

			if ( node != null )
			{
				return node.InnerText;
			}

			return defaultValue;
		}
	}
}