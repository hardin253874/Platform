// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Static helper methods for working with EntityRef
	/// </summary>
	public static class EntityRefHelper
	{
		private static readonly Lazy<Dictionary<string, string>> Namespaces = new Lazy<Dictionary<string, string>>(
			( ) => new Dictionary<string, string>
				{
					{
						string.Empty, "core"
					},
					{
						"s", "shared"
					},
					{
						"k", "console"
					},
					{
						"b", "bcm"
					},
					{
						"t", "test"
					},
					{
						"oldshared", "oldshared"
					},
                    {
                        "cast", "cast"
                    }
				}, true );

		/// <summary>
		///     Get dictionary of standard namespace prefixes.
		/// </summary>
		public static Dictionary<string, string> DefaultNsPrefixes
		{
			get
			{
				return Namespaces.Value;
			}
		}

		/// <summary>
		///     Converts a string that has an abbreviated alias (or no alias) to its full alias.
		/// </summary>
		public static string ConvertAliasPrefixToFull( string nsAlias )
		{
			EntityRef eref = ConvertAliasWithNamespace( nsAlias );

			return string.Format( "{0}:{1}", eref.Namespace, eref.Alias );
		}

		/// <summary>
		///     Converts a string to an entity-ref.
		///     The string can be of the format of either "alias" or "namespace:alias" or "prefix:alias".
		/// </summary>
		public static EntityRef ConvertAliasWithNamespace( string nsAlias )
		{
            if ( string.IsNullOrEmpty( nsAlias ) )
            {
                return null;
            }

            // Express for common use case
            if ( nsAlias.StartsWith( "core:" ) )
            {
                return new EntityRef( "core", nsAlias.Substring( 5 ) );
            }
            if ( nsAlias.StartsWith( "console:" ) )
            {
                return new EntityRef( "console", nsAlias.Substring( 8 ) );
            }

			return ConvertAliasWithNamespace( nsAlias, DefaultNsPrefixes );
		}

		/// <summary>
		///     Converts a string to an entity-ref.
		///     The string can be of the format of either "alias" or "namespace:alias".
		/// </summary>
		public static EntityRef ConvertAliasWithNamespace( string nsAlias, Dictionary<string, string> namespacePrefixes )
		{
			if ( string.IsNullOrEmpty( nsAlias ) )
			{
				return null;
			}

			// Split string
			int i = nsAlias.IndexOf( ':' );
			string ns = string.Empty;
			string alias;
			if ( i == -1 )
			{
				alias = nsAlias;
			}
			else
			{
				ns = nsAlias.Substring( 0, i );
				alias = nsAlias.Substring( i + 1 );
			}

			// Resolve NS if possible
			if ( namespacePrefixes != null )
			{
				string realNs;
				if ( namespacePrefixes.TryGetValue( ns, out realNs ) )
				{
					ns = realNs;
				}
			}

			// Create EntityRef
			var entityId = new EntityRef
				{
					Namespace = ns,
					Alias = alias
				};

			return entityId;
		}

		/// <summary>
		///     Converts the alias with namespace and id to an EntityRef.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static EntityRef ConvertAliasWithNamespaceAndId( string alias, long id )
		{
			EntityRef entityRef = ConvertAliasWithNamespace( alias ) ?? new EntityRef( );

			entityRef.Id = id;

			return entityRef;
		}


		/// <summary>
		///     Formats an alias for configuration XML by collapsing its namespace down to the standard prefix.
		/// </summary>
		/// <param name="ns">The namespace.</param>
		/// <param name="alias">The alias, without namespace.</param>
		/// <returns>NamespacePrefix:Alias, or just alias if the core namespace is passed</returns>
		public static string FormatForConfigXml( string ns, string alias )
		{
			string prefix = DefaultNsPrefixes.Where( pair => pair.Value == ns ).Select( pair => pair.Key ).FirstOrDefault( );
			if ( string.IsNullOrEmpty( prefix ) )
			{
				return alias;
			}
			return prefix + ":" + alias;
		}


		/// <summary>
		///     Formats an alias for configuration XML by collapsing its namespace down to the standard prefix.
		/// </summary>
		/// <param name="qualifiedAlias">The namespace:alias pair.</param>
		public static string FormatForConfigXml( string qualifiedAlias )
		{
			string[] parts = qualifiedAlias.Split( ':' );
			if ( parts.Length != 2 )
			{
				throw new Exception( "Expected qualified alias." );
			}
			return FormatForConfigXml( parts[ 0 ], parts[ 1 ] );
		}


		/// <summary>
		///     Formats an alias for configuration XML by collapsing its namespace down to the standard prefix.
		/// </summary>
		/// <param name="entityRef">An entity reference.</param>
		public static string FormatForConfigXml( EntityRef entityRef )
		{
			return FormatForConfigXml( entityRef.Namespace, entityRef.Alias );
		}


        /// <summary>
        /// Search string for locating entityRef nodes in an XML document.
        /// </summary>
        /// <remarks>
        /// Please check with Pete if you want to modify this XPath!!
        /// This locates the actual text node itself, not the element.
        /// </remarks>
        public static string EntityRefXPath = ".//node()[@entityRef='true']/text()";

        /// <summary>
        /// Visits all entityRef nodes in a document.
        /// </summary>
	    public static void VisitEntityRefTextNodes(XmlDocument doc, Action<XmlText> callback)
	    {
            if (doc == null)
                throw new ArgumentNullException("doc");
            if (callback == null)
                throw new ArgumentNullException("callback");

            XmlNodeList nodes = doc.SelectNodes(EntityRefXPath);
            if (nodes == null)
                return;

	        foreach (XmlNode node in nodes)
	        {
                var textNode = node as XmlText;
                if (textNode == null || string.IsNullOrEmpty(textNode.Value))
                    continue;

                callback(textNode);
	        }
	    }

		/// <summary>
		/// Visits the entity preference text nodes.
		/// </summary>
		/// <param name="doc">The document.</param>
		/// <param name="callback">The callback.</param>
		/// <exception cref="System.ArgumentNullException">
		/// doc
		/// or
		/// callback
		/// </exception>
		public static void VisitEntityRefTextNodes( XDocument doc, Action<XElement> callback )
		{
			if ( doc == null )
			{
				throw new ArgumentNullException( "doc" );
			}

			if ( doc.Root == null )
			{
				throw new ArgumentException( "doc.Root is null" );
			}

			if ( callback == null )
			{
				throw new ArgumentNullException( "callback" );
			}

			IEnumerable<XElement> nodes = doc.Root.DescendantNodesAndSelf( ).Where( e => e.NodeType == XmlNodeType.Element ).Cast<XElement>( ).Where( e => e.Attribute( "entityRef" ) != null ).Where( n => n.FirstNode != null && n.FirstNode.NodeType == XmlNodeType.Text );

			foreach ( XElement node in nodes )
			{
				if ( node == null || string.IsNullOrEmpty( node.Value ) )
				{
					continue;
				}

				callback( node );
			}
		}

		/// <summary>
		/// Gets the entity preference text nodes.
		/// </summary>
		/// <param name="doc">The document.</param>
		/// <returns></returns>
		public static IEnumerable<XmlText> GetEntityRefTextNodes( XmlDocument doc )
		{
			var results = new List<XmlText>( );

			VisitEntityRefTextNodes( doc, results.Add );
			
			return results;
		}

		/// <summary>
		/// Gets the entity preference text nodes.
		/// </summary>
		/// <param name="doc">The document.</param>
		/// <returns></returns>
		public static IEnumerable<XElement> GetEntityRefTextNodes( XDocument doc )
		{
			var results = new List<XElement>( );

			VisitEntityRefTextNodes( doc, results.Add );

			return results;
		}
	}

	/// <summary>
	///     Compares two entities, based on their IDs.
	///     Note: Entities that contain aliases will hit the entity model to resolve to ID.
	/// </summary>
	public class EntityRefComparer : IEqualityComparer<EntityRef>
	{
		public static EntityRefComparer Instance = new EntityRefComparer( );

		/// <summary>
		///     Determines whether the specified objects are equal.
		/// </summary>
		public bool Equals( EntityRef x, EntityRef y )
		{
			if ( x == null )
			{
				return y == null;
			}
			if ( y == null )
			{
				return false;
			}

		    return x.Equals(y);
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		public int GetHashCode( EntityRef entityRef )
		{
		    return entityRef == null ? 0 : entityRef.GetHashCode();
		}
	}
}