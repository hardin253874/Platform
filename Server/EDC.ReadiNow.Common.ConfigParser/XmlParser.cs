// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
	/// <summary>
	///     Mechanism for converting XML to Entity objects.
	/// </summary>
	/// <remarks>
	///     Note: entities are returned in their raw form. They're not inserted into the database, let alone fully connected to
	///     each other.
	///     Caution: makes use of 'yield return'.
	/// </remarks>
	public static class XmlParser
	{
		public const string RelationshipInstanceSuffix = ".instance";

		/// <summary>
		///     Regular expression for matching namespace:alias combinations.
		/// </summary>
		private static readonly Regex AliasRegex = new Regex( "[a-z]+\\:[a-zA-Z][a-zA-Z0-9]*" );

		/// <summary>
		///     Given a stream of config document paths, returns a stream of parsed entities.
		/// </summary>
		public static IEnumerable<Entity> ReadEntities( IEnumerable<string> paths, IEnumerable<Func<XElement, Entity, List<KeyValuePair<string, object>>>> entityActions = null )
		{
			IList<XDocument> docs = new List<XDocument>( );

			foreach ( string p in paths )
			{
				try
				{
					string[ ] files;
					if ( p.Contains( '*' ) )
					{
						string folder = Path.GetDirectoryName( p );
						if ( folder == null )
							throw new Exception( "Null folder for " + p );
						string search = Path.GetFileName( p );

						files = Directory.GetFiles( folder, search );
					}
					else
					{
						files = new[ ]
						{
							p
						};
					}
					foreach ( string file in files )
					{
						docs.Add( XDocument.Load( file, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri ) );
					}
				}
				catch ( Exception e )
				{
					throw new Exception( "failed to load xml document from " + p + ": " + e.Message, e );
				}
			}
			return ReadEntities( docs, entityActions == null ? null : entityActions.ToList() ).ToList( );
		}

		/// <summary>
		///     Reads the binaries.
		/// </summary>
		/// <param name="paths">The paths.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">failed to load xml document from  + path</exception>
		public static IEnumerable<KeyValuePair<string, string>> ReadBinaries( IEnumerable<string> paths )
		{
			IList<XDocument> docs = new List<XDocument>( );

			string path = null;

			try
			{
				foreach ( string p in paths )
				{
					path = p;
					docs.Add( XDocument.Load( p, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri ) );
				}
			}
			catch ( Exception e )
			{
				throw new Exception( "failed to load xml document from " + path, e );
			}

			return ReadBinaries( docs ).ToList( );
		}

		/// <summary>
		///     Determines if a given XML document is a config file.
		/// </summary>
		public static bool IsConfigXml( string path )
		{
			try
			{
				XAttribute attrib = null;

				XDocument doc = XDocument.Load( path, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri );

				if ( doc.Root != null )
				{
					if ( doc.Root.Name.LocalName != "resources" )
						return false;

					XName defaultSolutionAttrib = XName.Get( "defaultSolution" );
					attrib = doc.Root.Attribute( defaultSolutionAttrib );
				}
				return attrib != null;
			}
			catch ( Exception )
			{
				Console.WriteLine( "Failed to check: " + path );
				return false;
			}
		}

		/// <summary>
		///     Reads the binaries.
		/// </summary>
		/// <param name="docs">The docs.</param>
		/// <returns></returns>
		private static IEnumerable<KeyValuePair<string, string>> ReadBinaries( IEnumerable<XDocument> docs )
		{
			XName defaultSolutionAttrib = XName.Get( "defaultSolution" );

			foreach ( XDocument doc in docs )
			{
				if ( doc.Root != null )
				{
					XAttribute attrib = doc.Root.Attribute( defaultSolutionAttrib );

					if ( attrib == null )
					{
						throw new BuildException( "Missing defaultSolution attribute.", doc.Root );
					}

					Alias[ ] defaultSolution =
					{
						TranslateAliasValue( doc.Root, attrib.Value )
					};

					/////
					// Read and yield document nodes
					/////
					foreach ( string binary in ReadBinaries( doc.Root ) )
					{
						yield return new KeyValuePair<string, string>( defaultSolution.First( ).Value, binary );
					}
				}
			}
		}


		/// <summary>
		///     Given a stream of XML documents, returns a stream of parsed entities.
		/// </summary>
		private static IEnumerable<Entity> ReadEntities( IEnumerable<XDocument> docs, List<Func<XElement, Entity, List<KeyValuePair<string, object>>>> entityActions = null )
		{
			XName defaultSolutionAttrib = XName.Get( "defaultSolution" );
			var inSolution = new EntityRef( Aliases.InSolution );

			foreach ( XDocument doc in docs )
			{
				if ( doc.Root != null )
				{
					// Get the defaultSolution for this document
					XAttribute attrib = doc.Root.Attribute( defaultSolutionAttrib );
					if ( attrib == null )
						throw new BuildException( "Missing defaultSolution attribute.", doc.Root );
					Alias[ ] defaultSolution =
					{
						TranslateAliasValue( doc.Root, attrib.Value )
					};

					// Read and yield document nodes
					IEnumerable<Entity> entities = ReadEntities( doc.Root, null, entityActions );
					foreach ( Entity entity in entities )
					{
						// Manufacture relationship to solution
						// Note: 'inSolution' relationship for implicit relationship instances gets handled in schema resolver.
						entity.Members.Add( new Member
						{
							MemberDefinition = inSolution,
							Value = attrib.Value,
							ValueAsAliases = defaultSolution
						} );

						// Return the entity
						yield return entity;
					}
				}
			}
		}


		/// <summary>
		///     Removes the document namespace.
		/// </summary>
		/// <param name="xdocument">The xdocument.</param>
		/// <returns></returns>
		private static XDocument RemoveDocumentNamespace( XDocument xdocument )
		{
			if ( xdocument.Root != null )
			{
				foreach ( XElement e in xdocument.Root.DescendantsAndSelf( ) )
				{
					if ( e.Name.Namespace != XNamespace.None )
					{
						e.Name = e.Name.LocalName;
					}
					if ( e.Attributes( ).Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None) )
					{
						e.ReplaceAttributes( e.Attributes( ).Select( a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute( a.Name.LocalName, a.Value ) : a ) );
					}
				}
			}

			return xdocument;
		}

		/// <summary>
		///     Reads the binaries.
		/// </summary>
		/// <param name="containerElement">The container element.</param>
		/// <returns></returns>
		private static IEnumerable<string> ReadBinaries( XElement containerElement )
		{
			/////
			// Visit each Element at the current level
			/////
			foreach ( XElement entityElem in containerElement.Elements( ) )
			{
				XAttribute sourceAttribute = entityElem.Attribute( "source" );

				if ( sourceAttribute != null )
				{
					string referencePath = Path.GetDirectoryName( containerElement.BaseUri );

					if ( referencePath != null )
					{
						/////
						// Get an absolute path rooted off the elements base uri.
						/////
						var uri = new Uri( Path.Combine( referencePath, sourceAttribute.Value ) );
						yield return Path.GetFullPath( uri.AbsolutePath );
					}
				}

				/////
				// Visit each member (field or relationship) under the entity element
				/////
				foreach ( XElement memberElem in entityElem.Elements( ) )
				{
					if ( memberElem.HasElements )
					{
						XElement firstChild = memberElem.Elements( ).First( );

						if ( firstChild.Name.LocalName != "xml" )
						{
							/////
							// Visit any child entities that are defined under the member, and yield them as we go
							// Any immediate children also get written into member.Children
							/////
							IEnumerable<string> yieldedEntities = ReadBinaries( memberElem );

							foreach ( string childEntity in yieldedEntities )
							{
								yield return childEntity;
							}
						}
					}
				}
			}
		}


		/// <summary>
		///     This will return a stream of all entities.
		///     Entities will have the following set:
		///     - alias, if applicable
		///     - type (only as defined by element name)
		///     - Scalar Members
		///     - Nested Members
		///     Every entity reference should be ready to resolve, if it is valid.
		/// </summary>
		/// <param name="containerElement"></param>
		/// <param name="outputImmediateChildren">Immediate children are written to this container, if present.</param>
		/// <param name="entityActions"></param>
		/// <returns></returns>
		private static IEnumerable<Entity> ReadEntities( XElement containerElement, List<EntityRef> outputImmediateChildren, List<Func<XElement, Entity, List<KeyValuePair<string, object>>>> entityActions = null )
		{
			// Visit each Element at the current level
			foreach ( XElement entityElem in containerElement.Elements( ) )
			{
				// Create an Entity
				var entity = new Entity
				{
					Type = new EntityRef( entityElem )
				};

				// Set a reference to its type, from the element name

				entity.SetLineInfo( entityElem );

				// Visit each member (field or relationship) under the entity element
				foreach ( XElement memberElem in entityElem.Elements( ) )
				{
					// Get a reference to the definition of the member based on the element member name
					var member = new Member
					{
						MemberDefinition = new EntityRef( memberElem )
					};
					Alias defnAlias = member.MemberDefinition.Alias;

					if ( defnAlias == null )
						throw new Exception( "Config entity without alias" );

					// Member is either:
					// - a simple type, which means a field value or a string containing relationship references
					// - nested values, which means either related entities, or relationship instance entities.
					// - an xml literal

					if ( memberElem.HasElements )
					{
						XElement firstChild = memberElem.Elements( ).First( );
						if ( firstChild.Name.LocalName == "xml" )
						{
							// Simple XML member
							XElement xmlElem = firstChild;
							if ( xmlElem.HasElements )
							{
								XElement element = xmlElem.Elements( ).First( );
								if ( element.GetDefaultNamespace( ) == element.Document.Root.GetDefaultNamespace( ) )
								{
									// The namespace of the xml field is the same as the document
									// so remove the namespace
									XDocument fieldXmlDoc = XDocument.Parse( element.ToString( ) );
									fieldXmlDoc = RemoveDocumentNamespace( fieldXmlDoc );
									member.Value = fieldXmlDoc.ToString( ); // go from the <xml> element, to the embedded root element, and call ToString on the root element.
								}
								else
								{
									member.Value = xmlElem.Elements( ).First( ).ToString( ); // go from the <xml> element, to the embedded root element, and call ToString on the root element.
								}
							}
							else
								member.Value = "";
						}
						else
						{
							// Visit any child entities that are defined under the member, and yield them as we go
							// Any immediate children also get written into member.Children
							IEnumerable<Entity> yieldedEntities = ReadEntities( memberElem, member.Children, entityActions );
							foreach ( Entity childEntity in yieldedEntities )
							{
								yield return childEntity;
							}
						}
					}
					else
					{
						// Simple member
						member.Value = memberElem.Value;

						// Check to see if it's the 'alias' or 'reverseAlias' member.
						if ( defnAlias == Aliases.Alias )
						{
							entity.Alias = TranslateAliasValue( memberElem );
						}
						if ( defnAlias == Aliases.ReverseAlias )
						{
							entity.ReverseAlias = TranslateAliasValue( memberElem );
						}

						// Hack: even if it isn't then we still need to capture the result as an alias, just in case it turns out to 
						// be a relationship, pointing to another entity by alias name - since we need to capture the namespace at this point.
						string[ ] parts = memberElem.Value.Split( ',' );
						member.ValueAsAliases = parts.Select( part => TranslateAliasValue( memberElem, part.Trim( ) ) ).ToArray( );
					}

					// Special parsing required for the 'from' and 'to' elements of explicit relationship instances.
					if ( defnAlias == Aliases.From )
					{
						entity.RelationshipInstanceFrom = member;
					}
					else if ( defnAlias == Aliases.To )
					{
						entity.RelationshipInstanceTo = member;
					}
					else
					{
						entity.Members.Add( member );
					}
				}

				// Output the entity
				if ( outputImmediateChildren != null )
					outputImmediateChildren.Add( new EntityRef( entity ) );

				if ( entityActions != null )
				{
					foreach ( var func in entityActions )
					{
						IEnumerable<KeyValuePair<string, object>> keyValuePairs = func( entityElem, entity );

						if ( keyValuePairs != null )
						{
							foreach ( var pair in keyValuePairs )
							{
								var member = new Member( );

								var memberDefinition = new Alias( );

								/////
								// Alias is returned in namespace:alias format
								/////
								string[ ] parts = pair.Key.Split( ':' );

								string namepspace;
								string alias;

								if ( parts.Length <= 1 )
								{
									namepspace = "core";
									alias = parts[ 0 ];
								}
								else
								{
									namepspace = parts[ 0 ];
									alias = parts[ 1 ];
								}

								memberDefinition.Value = alias;
								memberDefinition.Namespace = namepspace;
								member.MemberDefinition = new EntityRef( memberDefinition );
								member.Value = pair.Value.ToString( );

								entity.Members.Add( member );
							}
						}
					}
				}

				yield return entity;
			}
		}


		/// <summary>
		/// Translates an 'alias' or 'reverseAlias' property element.
		/// </summary>
		/// <param name="aliasElement">The alias element.</param>
		/// <returns></returns>
		private static Alias TranslateAliasValue( XElement aliasElement )
		{
			string aliasText = aliasElement.Value;
			return TranslateAliasValue( aliasElement, aliasText );
		}


		/// <summary>
		/// Translates an 'alias' or 'reverseAlias' property element.
		/// </summary>
		/// <param name="aliasElement">The alias element.</param>
		/// <param name="aliasText">The alias text.</param>
		/// <returns></returns>
		/// <exception cref="BuildException">Namespace could not be resolved for:  + aliasElement.Value</exception>
		private static Alias TranslateAliasValue( XElement aliasElement, string aliasText )
		{
            // E.g.  from <alias xmlns:x="SharedObjects">x:person</alias>
            // resolve the alias that represents SharedObjects:person.

            // Split x:person into 'x' and 'person', and store in aliasText and nsText respectively
            // (Avoid string split for efficiency)

            // Resolve namespace prefix, if specified
            XNamespace ns;
			int divider = aliasText.IndexOf( ':' );
			if ( divider == -1 )
			{
				ns = aliasElement.GetDefaultNamespace( );
			}
			else
			{
				if ( AliasRegex.IsMatch( aliasText ) )
				{
					string nsText = aliasText.Substring( 0, divider );
					ns = aliasElement.GetNamespaceOfPrefix( nsText ) ?? nsText;
					aliasText = aliasText.Substring( divider + 1 );
				}
				else
				{
					ns = aliasElement.GetDefaultNamespace( );
				}
			}

			// Check namespace was found (default or otherwise)
			if ( ns == null || string.IsNullOrEmpty( ns.NamespaceName ) )
				throw new BuildException( "Namespace could not be resolved for: " + aliasElement.Value, aliasElement );

			// Create alias
			var result = new Alias
			{
				Value = aliasText,
				Namespace = ns.NamespaceName
			};
			result.SetLineInfo( aliasElement );
			return result;
		}


		/// <summary>
		///     Format location details for error message.
		/// </summary>
		internal static string GetLineInfo( XElement element )
		{
			IXmlLineInfo elemInfo = element;
			int lineNumber = 0;
			string file = string.Empty;

			if ( elemInfo.HasLineInfo( ) )
			{
				lineNumber = elemInfo.LineNumber;
			}

			if ( element.Document != null )
			{
				file = Path.GetFileName( element.Document.BaseUri );
			}

			return string.Format( " {0} Line {1}", file, lineNumber );
		}
	}
}