// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;

namespace SolutionHash
{
	/// <summary>
	///     Xml extension methods.
	/// </summary>
	internal static class XmlExtensions
	{
		/// <summary>
		///     Reads the attribute string.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="xpath">The xpath.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		/// <exception cref="System.NullReferenceException"></exception>
		/// <exception cref="System.ArgumentException">The specified xpath parameter is invalid.</exception>
		public static string ReadAttributeString( this XmlNode node, string xpath, string defaultValue )
		{
			if ( node == null )
			{
				throw new NullReferenceException( );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( node.Attributes == null )
			{
				return defaultValue;
			}

			XmlNode attribute = node.SelectSingleNode( xpath );

			return attribute != null ? attribute.Value : defaultValue;
		}
	}
}