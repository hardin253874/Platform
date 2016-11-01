// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.Xml;
using EDC.Xml;

namespace EDC.ReadiNow.Metadata.Media
{
	/// <summary>
	///     A static helper class for dealing with ColorInfo objects.
	/// </summary>
	public static class ColorInfoHelper
	{
		/// <summary>
		///     Reconstructs the color info object from the specified XML node and XML element name.
		/// </summary>
		/// <param name="node">The XML node containing the color XML.</param>
		/// <param name="elementName">The name of the XML element containing the color data.</param>
		/// <returns>The color info object.</returns>
		public static ColorInfo ReadColorInfoXml( XmlNode node, string elementName )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( elementName ) )
			{
				throw new ArgumentNullException( "elementName" );
			}

			var colorInfo = new ColorInfo( );

			XmlNode colorInfoNode = XmlHelper.SelectSingleNode( node, elementName );
			if ( colorInfoNode != null )
			{
				colorInfo.A = XmlHelper.ReadAttributeByte( colorInfoNode, "@a", 0 );
				colorInfo.R = XmlHelper.ReadAttributeByte( colorInfoNode, "@r", 0 );
				colorInfo.G = XmlHelper.ReadAttributeByte( colorInfoNode, "@g", 0 );
				colorInfo.B = XmlHelper.ReadAttributeByte( colorInfoNode, "@b", 0 );
			}

			return colorInfo;
		}

		/// <summary>
		///     Writes the color info to the specified XML writer using the specified XML element name.
		/// </summary>
		/// <param name="elementName">The XML element name that will contain the serialized color info.</param>
		/// <param name="color">The color to serialize.</param>
		/// <param name="xmlWriter">The XML writer used to write the image to.</param>
		public static void WriteColorInfoXml( string elementName, ColorInfo color, XmlWriter xmlWriter )
		{
			if ( string.IsNullOrEmpty( elementName ) )
			{
				throw new ArgumentNullException( "elementName" );
			}

			if ( color == null )
			{
				throw new ArgumentNullException( "color" );
			}

			if ( xmlWriter == null )
			{
				throw new ArgumentNullException( "xmlWriter" );
			}

			xmlWriter.WriteStartElement( elementName );

			xmlWriter.WriteAttributeString( "a", color.A.ToString( CultureInfo.InvariantCulture ) );
			xmlWriter.WriteAttributeString( "r", color.R.ToString( CultureInfo.InvariantCulture ) );
			xmlWriter.WriteAttributeString( "g", color.G.ToString( CultureInfo.InvariantCulture ) );
			xmlWriter.WriteAttributeString( "b", color.B.ToString( CultureInfo.InvariantCulture ) );

			xmlWriter.WriteEndElement( );
		}
	}
}