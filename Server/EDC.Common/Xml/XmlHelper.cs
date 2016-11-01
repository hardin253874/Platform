// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EDC.Xml
{
	/// <summary>
	///     Provides helper methods for interacting with XML.
	/// </summary>
	public static class XmlHelper
	{
		/// <summary>
		///     Escapes the XML text.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static string EscapeXmlText( string xml )
		{
			if ( !string.IsNullOrEmpty( xml ) )
			{
				var sb = new StringBuilder( xml );
				sb.Replace( "&", "&amp;" );
				sb.Replace( ">", "&gt;" );
				sb.Replace( "<", "&lt;" );
				sb.Replace( "\"", "&quot;" );
				sb.Replace( "'", "&apos;" );

				xml = sb.ToString( );
			}

			return xml;
		}

		/// <summary>
		///     Check if at least one XML node matches the xpath expression.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     true if at least one XML node matches the xpath query; otherwise false.
		/// </returns>
		public static bool EvaluateNodes( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			bool match = false;

			// Check if at least one node matches the specified xpath
			XmlNodeList nodes = node.SelectNodes( xpath );
			if ( nodes != null && ( nodes.Count > 0 ) )
			{
				match = true;
			}

			return match;
		}

		/// <summary>
		///     Checks if an XML node matches the xpath expression.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     true if an XML node matches the xpath expression; otherwise false.
		/// </returns>
		public static bool EvaluateSingleNode( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			bool match = false;

			// Check if a node matches the specified xpath
			XmlNode xmlNode = node.SelectSingleNode( xpath );
			if ( xmlNode != null )
			{
				match = true;
			}

			return match;
		}

		/// <summary>
		///     Gets the names of the child nodes for the specified node.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <returns>
		///     A collection of strings containing the names of the child nodes.
		/// </returns>
		public static StringCollection GetChildNodeNames( XmlNode node )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			var nodeNames = new StringCollection( );

			if ( node.HasChildNodes )
			{
				for ( int x = 0; x < node.ChildNodes.Count; ++x )
				{
					string nodeName = node.ChildNodes[ x ].Name;
					if ( !string.IsNullOrEmpty( nodeName ) )
					{
						nodeNames.Add( nodeName );
					}
				}
			}

			return nodeNames;
		}

		/// <summary>
		///     Reads the Boolean from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The Boolean contained within the specified XML attribute.
		/// </returns>
		public static bool ReadAttributeBool( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToBoolean( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the Boolean from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The Boolean contained within the specified XML attribute.
		/// </returns>
		public static bool ReadAttributeBool( XmlNode node, string xpath, bool alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			bool data = xmlAttribute != null ? Convert.ToBoolean( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the Boolean from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The byte contained within the specified XML attribute.
		/// </returns>
		public static byte ReadAttributeByte( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToByte( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the byte from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The byte contained within the specified XML attribute.
		/// </returns>
		public static byte ReadAttributeByte( XmlNode node, string xpath, byte alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			byte data = xmlAttribute != null ? Convert.ToByte( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the date time from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static DateTime ReadAttributeDateTime( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToDateTime( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the date time from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static DateTime ReadAttributeDateTime( XmlNode node, string xpath, DateTime alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );

			DateTime data = xmlAttribute != null ? Convert.ToDateTime( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the decimal from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Decimal ReadAttributeDecimal( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToDecimal( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the decimal from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Decimal ReadAttributeDecimal( XmlNode node, string xpath, Decimal alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			decimal data = xmlAttribute != null ? Convert.ToDecimal( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the double from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML attribute.
		/// </returns>
		public static double ReadAttributeDouble( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToDouble( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the double from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML attribute.
		/// </returns>
		public static double ReadAttributeDouble( XmlNode node, string xpath, double alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			double data = xmlAttribute != null ? Convert.ToDouble( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the enumeration from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="type">
		///     The type of the enumeration.
		/// </param>
		/// <returns>
		///     The enumeration contained within the specified XML attribute.
		/// </returns>
		public static object ReadAttributeEnum( XmlNode node, string xpath, Type type )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type", @"The specified type parameter is null." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Enum.Parse( type, xmlAttribute.Value, true );
		}

		/// <summary>
		///     Reads the enumeration from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="type">
		///     The type of the enumeration.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The enumeration contained within the specified XML attribute.
		/// </returns>
		public static object ReadAttributeEnum( XmlNode node, string xpath, Type type, object alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type", @"The specified type parameter is null." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			object data = xmlAttribute != null ? Enum.Parse( type, xmlAttribute.Value, true ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the GUID from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The GUID contained within the specified XML attribute.
		/// </returns>
		public static Guid ReadAttributeGuid( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return new Guid( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the GUID from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The GUID contained within the specified XML attribute.
		/// </returns>
		public static Guid ReadAttributeGuid( XmlNode node, string xpath, Guid alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			Guid data = xmlAttribute != null ? new Guid( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static int ReadAttributeInt( XmlNode node, string xpath )
		{
			return ReadAttributeInt32( node, xpath );
		}

		/// <summary>
		///     Reads the integer from the specified XML attribute.
		/// </summary>
		/// <param name="node">The XML node to examine.</param>
		/// <param name="xpath">A string containing the xpath expression.</param>
		/// <param name="alternative">The alternative.</param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static int ReadAttributeInt( XmlNode node, string xpath, int alternative )
		{
			return ReadAttributeInt32( node, xpath, alternative );
		}

		/// <summary>
		///     Reads the integer (16-bit) from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int16 ReadAttributeInt16( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToInt16( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the integer from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int16 ReadAttributeInt16( XmlNode node, string xpath, Int16 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			short data = xmlAttribute != null ? Convert.ToInt16( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer (32-bit) from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int32 ReadAttributeInt32( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToInt32( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the integer (32-bit) from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int32 ReadAttributeInt32( XmlNode node, string xpath, Int32 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			int data = xmlAttribute != null ? Convert.ToInt32( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer (64-bit) from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int64 ReadAttributeInt64( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return Convert.ToInt64( xmlAttribute.Value );
		}

		/// <summary>
		///     Reads the integer (64-bit) from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML attribute.
		/// </returns>
		public static Int64 ReadAttributeInt64( XmlNode node, string xpath, Int64 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			long data = xmlAttribute != null ? Convert.ToInt64( xmlAttribute.Value ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the string from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The string contained within the specified XML attribute.
		/// </returns>
		public static string ReadAttributeString( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			if ( xmlAttribute == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified attribute (xpath: {0})", xpath ) );
			}

			return xmlAttribute.Value;
		}

		/// <summary>
		///     Reads the string from the specified XML attribute.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the attribute does not exist.
		/// </param>
		/// <returns>
		///     The string contained within the specified XML attribute.
		/// </returns>
		public static string ReadAttributeString( XmlNode node, string xpath, string alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the value from the attribute
			XmlNode xmlAttribute = node.SelectSingleNode( xpath );
			string data = xmlAttribute != null ? xmlAttribute.Value : alternate;

			return data;
		}

		/// <summary>
		///     Reads the Boolean from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The Boolean contained within the specified XML element.
		/// </returns>
		public static bool ReadElementBool( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToBoolean( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the Boolean from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The Boolean contained within the specified XML element.
		/// </returns>
		public static bool ReadElementBool( XmlNode node, string xpath, bool alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			bool data = xmlElement != null ? Convert.ToBoolean( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the byte (8-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The byte contained within the specified XML element.
		/// </returns>
		public static byte ReadElementByte( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToByte( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the byte (8-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The byte contained within the specified XML element.
		/// </returns>
		public static byte ReadElementByte( XmlNode node, string xpath, byte alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			byte data = xmlElement != null ? Convert.ToByte( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the date time from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The date time contained within the specified XML element.
		/// </returns>
		public static DateTime ReadElementDateTime( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return DateTime.Parse( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the date time from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static DateTime ReadElementDateTime( XmlNode node, string xpath, DateTime alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			DateTime data = xmlElement != null ? Convert.ToDateTime( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the decimal from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Decimal ReadElementDecimal( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToDecimal( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the decimal from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Decimal ReadElementDecimal( XmlNode node, string xpath, Decimal alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			decimal data = xmlElement != null ? Convert.ToDecimal( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the double from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML element.
		/// </returns>
		public static double ReadElementDouble( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToDouble( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the double from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML element.
		/// </returns>
		public static double ReadElementDouble( XmlNode node, string xpath, double alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			double data = xmlElement != null ? Convert.ToDouble( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the enumeration from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="type">
		///     The type of the enumeration.
		/// </param>
		/// <returns>
		///     The enumeration contained within the specified XML element.
		/// </returns>
		public static object ReadElementEnum( XmlNode node, string xpath, Type type )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type", @"The specified type parameter is null." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Enum.Parse( type, xmlElement.InnerText, true );
		}

		/// <summary>
		///     Reads the enumeration from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="type">
		///     The type of the enumeration.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The enumeration contained within the specified XML element.
		/// </returns>
		public static object ReadElementEnum( XmlNode node, string xpath, Type type, object alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			if ( type == null )
			{
				throw new ArgumentNullException( "type", @"The specified type parameter is null." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			object data = xmlElement != null ? Enum.Parse( type, xmlElement.InnerText, true ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the GUID from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The GUID contained within the specified XML element.
		/// </returns>
		public static Guid ReadElementGuid( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return new Guid( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the GUID from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The GUID contained within the specified XML element.
		/// </returns>
		public static Guid ReadElementGuid( XmlNode node, string xpath, Guid alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			Guid data = xmlElement != null ? new Guid( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static int ReadElementInt( XmlNode node, string xpath )
		{
			return ReadElementInt32( node, xpath );
		}

		/// <summary>
		///     Reads the integer from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static int ReadElementInt( XmlNode node, string xpath, int alternate )
		{
			return ReadElementInt32( node, xpath, alternate );
		}

		/// <summary>
		///     Reads the integer (16-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int16 ReadElementInt16( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToInt16( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the integer (16-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int16 ReadElementInt16( XmlNode node, string xpath, Int16 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			short data = xmlElement != null ? Convert.ToInt16( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer (32-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int32 ReadElementInt32( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToInt32( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the integer (32-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int32 ReadElementInt32( XmlNode node, string xpath, Int32 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			int data = xmlElement != null ? Convert.ToInt32( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the integer (64-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int64 ReadElementInt64( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToInt64( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the integer (64-bit) from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The integer contained within the specified XML element.
		/// </returns>
		public static Int64 ReadElementInt64( XmlNode node, string xpath, Int64 alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			long data = xmlElement != null ? Convert.ToInt64( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the double from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML element.
		/// </returns>
		public static Single ReadElementSingle( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			if ( xmlElement == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return Convert.ToSingle( xmlElement.InnerText );
		}

		/// <summary>
		///     Reads the double from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The double contained within the specified XML element.
		/// </returns>
		public static Single ReadElementSingle( XmlNode node, string xpath, Single alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			float data = xmlElement != null ? Convert.ToSingle( xmlElement.InnerText ) : alternate;

			return data;
		}

		/// <summary>
		///     Reads the string from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The string contained within the specified XML element.
		/// </returns>
		public static string ReadElementString( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement;
			if ( ( xmlElement = node.SelectSingleNode( xpath ) ) == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified element (xpath: {0})", xpath ) );
			}

			return xmlElement.InnerText;
		}

		/// <summary>
		///     Reads the string from the specified XML element.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <param name="alternate">
		///     The default value if the element does not exist.
		/// </param>
		/// <returns>
		///     The string contained within the specified XML element.
		/// </returns>
		public static string ReadElementString( XmlNode node, string xpath, string alternate )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Extract the inner text from the element
			XmlNode xmlElement = node.SelectSingleNode( xpath );
			string data = xmlElement != null ? xmlElement.InnerText : alternate;

			return data;
		}

		/// <summary>
		///     Implemented based on interface, not part of algorithm
		/// </summary>
		/// <param name="xmlDocument">xml document string</param>
		/// <returns>xml document string without namespace</returns>
		public static string RemoveAllNamespaces( string xmlDocument )
		{
			XElement xmlDocumentWithoutNs = RemoveAllNamespaces( XElement.Parse( xmlDocument ) );

			return xmlDocumentWithoutNs.ToString( );
		}

		/// <summary>
		///     Remove namespace of XmlDocument
		/// </summary>
		/// <param name="xDocument">The x document.</param>
		/// <returns>
		///     xml document XDocument without namespace
		/// </returns>
		public static XDocument RemoveNamespace( XDocument xDocument )
		{
			if ( xDocument.Root != null )
			{
				foreach ( XElement oElement in xDocument.Root.DescendantsAndSelf( ) )
				{
					if ( oElement.Name.Namespace != XNamespace.None )
					{
						oElement.Name = XNamespace.None.GetName( oElement.Name.LocalName );
					}
					if ( oElement.Attributes( ).Any( a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None ) )
					{
						oElement.ReplaceAttributes( oElement.Attributes( ).Select( a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute( XNamespace.None.GetName( a.Name.LocalName ), a.Value ) : a ) );
					}
				}
			}
			return xDocument;
		}

		/// <summary>
		///     Selects a list of XML nodes matching the xpath expression.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     A collection of nodes matching the xpath query.
		/// </returns>
		public static XmlNodeList SelectNodes( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			return node.SelectNodes( xpath );
		}

		/// <summary>
		///     Selects the first XML node that matches the xpath expression.
		/// </summary>
		/// <param name="node">
		///     The XML node to examine.
		/// </param>
		/// <param name="xpath">
		///     A string containing the xpath expression.
		/// </param>
		/// <returns>
		///     The first XML node that matches the xpath query.
		/// </returns>
		public static XmlNode SelectSingleNode( XmlNode node, string xpath )
		{
			if ( node == null )
			{
				throw new ArgumentNullException( "node" );
			}

			if ( string.IsNullOrEmpty( xpath ) )
			{
				throw new ArgumentException( "The specified xpath parameter is invalid." );
			}

			// Select the first node that matches the specified xpath
			XmlNode xmlNode = node.SelectSingleNode( xpath );
			if ( xmlNode == null )
			{
				throw new InvalidOperationException( string.Format( "The XML node does not contain the specified node (xpath: {0})", xpath ) );
			}

			return xmlNode;
		}

		/// <summary>
		///     Core recursion function to remove all name space
		/// </summary>
		/// <param name="xmlDocument">xml document XElement</param>
		/// <returns>xml document string without namespace</returns>
		private static XElement RemoveAllNamespaces( XElement xmlDocument )
		{
			if ( !xmlDocument.HasElements )
			{
				var xElement = new XElement( xmlDocument.Name.LocalName )
					{
						Value = xmlDocument.Value
					};

				foreach ( XAttribute attribute in xmlDocument.Attributes( ) )
				{
					xElement.Add( attribute );
				}

				return xElement;
			}
			return new XElement( xmlDocument.Name.LocalName, xmlDocument.Elements( ).Select( RemoveAllNamespaces ) );
		}

	    /// <summary>
        /// Serializes the object query to xml.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string SerializeUsingDataContract<T>(T value) where T : class
	    {
            if (value == null)
                throw new ArgumentNullException("value");

            StringBuilder sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = false;
            settings.Encoding = Encoding.UTF8;
            
            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(xmlWriter, value);
                xmlWriter.Flush();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the object query to xml.
        /// </summary>
        /// <param name="xml">The XML to read.</param>
        /// <returns></returns>
        public static T DeserializeUsingDataContract<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            StringReader stringReader = new StringReader(xml);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            T result = (T)serializer.ReadObject(xmlReader);
            return result;
        }

		/// <summary>
		/// Writes the start element.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="localName">Name of the local.</param>
		/// <param name="xmlStack">The XML stack.</param>
		public static void WriteStartElement( this XmlWriter xmlWriter, string localName, Stack<string> xmlStack )
		{
			WriteStartElement( xmlWriter, localName, null, xmlStack );
		}

		/// <summary>
		/// Writes the start element.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="localName">Name of the local.</param>
		/// <param name="ns">The ns.</param>
		/// <param name="xmlStack">The XML stack.</param>
		public static void WriteStartElement( this XmlWriter xmlWriter, string localName, string ns, Stack<string> xmlStack )
		{
			xmlWriter.WriteStartElement( localName, ns );
			xmlStack.Push( localName );
        }

        /// <summary>
        ///     Serializes the inline relationship.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="elementName">Default name of the element.</param>
        /// <param name="xmlStack">The XML stack.</param>
        public static IDisposable WriteElementBlock( this XmlWriter xmlWriter, string elementName, Stack<string> xmlStack )
        {
            xmlWriter.WriteStartElement( elementName, xmlStack );

            return ContextHelper.Create( ( ) => xmlWriter.WriteEndElement( elementName, xmlStack ) );
        }

        /// <summary>
        ///     Serializes the inline relationship.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="elementName">Default name of the element.</param>
        /// <param name="ns">Namespace.</param>
        /// <param name="xmlStack">The XML stack.</param>
        public static IDisposable WriteElementBlock( this XmlWriter xmlWriter, string elementName, string ns, Stack<string> xmlStack )
        {
            xmlWriter.WriteStartElement( elementName, ns, xmlStack );

            return ContextHelper.Create( ( ) => xmlWriter.WriteEndElement( elementName, xmlStack ) );
        }

        /// <summary>
        /// Writes the end element.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="localName">Name of the local.</param>
        /// <param name="xmlStack">The XML stack.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        public static void WriteEndElement( this XmlWriter xmlWriter, string localName, Stack<string> xmlStack )
		{
			if ( xmlStack.Count <= 0 )
			{
				throw new InvalidOperationException( $"Xml stack has been corrupted. No matching element found for '{localName}'." );
			}

			string existing = xmlStack.Pop( );

			if ( existing != localName )
			{
				throw new InvalidOperationException( $"Xml stack has been corrupted. Expected '{localName}' but found '{existing}'." );
			}

			xmlWriter.WriteEndElement( );
		}
	}
}