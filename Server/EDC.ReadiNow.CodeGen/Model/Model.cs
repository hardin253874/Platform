// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EDC.ReadiNow.CodeGen.Extensions;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;

// ReSharper disable CheckNamespace

namespace EDC.ReadiNow.Templates
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     T4 methods.
	/// </summary>
	public static class Model
	{
		/// <summary>
		///     Instance suffix.
		/// </summary>
		private const string InstanceSuffix = "Instance";

		/// <summary>
		///     List type.
		/// </summary>
		private const string ListType = "IEntityCollection<{0}>";

		/// <summary>
		///     Relationship instance type.
		/// </summary>
		private const string RelationshipInstanceType = "IEntityRelationship<{0},{1}>";

		/// <summary>
		///     Relationship instance collection.
		/// </summary>
		private const string RelationshipInstanceCollectionType = "IEntityRelationshipCollection<{0},{1}>";

		/// <summary>
		///     Entities direct field cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<Entity>> EntityDirectFieldCache = new Dictionary<Entity, List<Entity>>( );

		/// <summary>
		///     Entities forward relationship cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<Entity>> EntityForwardRelationshipCache = new Dictionary<Entity, List<Entity>>( );

		/// <summary>
		///     Inherited entities forward relationship cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<Entity>> InheritedEntityForwardRelationshipCache = new Dictionary<Entity, List<Entity>>( );

		/// <summary>
		///     Inherited entities reverse relationship cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<Entity>> InheritedEntityReverseRelationshipCache = new Dictionary<Entity, List<Entity>>( );

		/// <summary>
		///     Entities reverse relationship cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<Entity>> EntityReverseRelationshipCache = new Dictionary<Entity, List<Entity>>( );

		/// <summary>
		///     Entities inherited field cache.
		/// </summary>
		private static readonly Dictionary<Entity, List<KeyValuePair<Entity, Entity>>> EntityInheritedFieldCache = new Dictionary<Entity, List<KeyValuePair<Entity, Entity>>>( );

		/// <summary>
		///     Type display name cache.
		/// </summary>
		private static readonly Dictionary<Entity, string> TypeDisplayNameCache = new Dictionary<Entity, string>( );

		/// <summary>
		///     Type Generic Type display name cache.
		/// </summary>
		private static readonly Dictionary<Entity, string> TypeGenericTypeDisplayNameCache = new Dictionary<Entity, string>( );

		/// <summary>
		///     Gets or sets the alias resolver.
		/// </summary>
		/// <value>
		///     The alias resolver.
		/// </value>
		public static IAliasResolver AliasResolver
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the namespace.
		/// </summary>
		/// <value>
		///     The namespace.
		/// </value>
		public static string Namespace
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the process types starting with.
		/// </summary>
		/// <value>
		///     The process types starting with.
		/// </value>
		public static char ProcessTypesStartingWith
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the schema resolver.
		/// </summary>
		/// <value>
		///     The schema resolver.
		/// </value>
		public static SchemaResolver SchemaResolver
		{
			get;
			set;
		}

		/// <summary>
		///     Determines whether the specified entity has patterns.
		/// </summary>
		/// <param name="type">
		///     The type to check for patterns.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified entity has patterns; otherwise, <c>false</c>.
		/// </returns>
		public static bool GenerateCode( Entity type )
		{
			if ( type == null || type.Alias == null )
			{
				return false;
			}

			return SchemaResolver.GetBoolFieldValue( type, Aliases.CoreAlias( "generateCode" ) );
		}

		/// <summary>
		///     Gets the types direct fields.
		/// </summary>
		/// <param name="type">
		///     The type whose direct fields are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified entities members.
		/// </returns>
		private static IEnumerable<Entity> GetDeclaredDirectFields( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			List<Entity> fields;

			if ( !EntityDirectFieldCache.TryGetValue( type, out fields ) )
			{
				/////
				// Get the declared fields.
				/////
				fields = SchemaResolver.GetDeclaredFields( type )
				                       .Where( field => field.Alias != null )
				                       .OrderBy( GetTypeDisplayName ).ToList( );

				EntityDirectFieldCache[ type ] = fields;
			}

			return fields;
		}

        /// <summary>
        ///     Gets the types fields (both direct and inherited).
        /// </summary>
        /// <param name="type">
        ///     The type whose fields are to be retrieved.
        /// </param>
        /// <returns>
        ///     An enumeration of entities that represent the specified types fields.
        /// </returns>
        public static IEnumerable<FieldData> GetEntityAllFields(Entity type)
        {
            if (type == null)
            {
                return Enumerable.Empty<FieldData>();
            }

            var direct = GetDeclaredDirectFields(type).Select(f => new FieldData(f, type, false));
            var inherited = GetDeclaredInheritedFields(type).Select(pair => new FieldData(pair.Value, pair.Key, true));

            return direct.Union(inherited);
        }

		/// <summary>
		///     Gets the types fields (both direct and inherited).
		/// </summary>
		/// <param name="type">
		///     The type whose fields are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified types fields.
		/// </returns>
		public static IEnumerable<Entity> GetDeclaredFields( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			return GetDeclaredDirectFields( type )
				.Union( GetDeclaredInheritedFields( type )
					        .Select( pair => pair.Value ) );
		}

		/// <summary>
		///     Gets the types inherited fields.
		/// </summary>
		/// <param name="type">
		///     The type whose inherited fields are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of key-value pairs that represent the inherited fields and their type.
		/// </returns>
		private static IEnumerable<KeyValuePair<Entity, Entity>> GetDeclaredInheritedFields( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<KeyValuePair<Entity, Entity>>( );
			}

			List<KeyValuePair<Entity, Entity>> inheritedFields;

			if ( !EntityInheritedFieldCache.TryGetValue( type, out inheritedFields ) )
			{
				/////
				// Determine the types ancestors.
				/////
				IEnumerable<Entity> ancestors = SchemaResolver.GetAncestors( type ).Except( Enumerable.Repeat( type, 1 ) );

				inheritedFields = new List<KeyValuePair<Entity, Entity>>( );

				inheritedFields = ancestors.Aggregate( inheritedFields, ( current, ancestorEntity ) => current.Union( SchemaResolver.GetDeclaredFields( ancestorEntity ).Select( field => new KeyValuePair<Entity, Entity>( ancestorEntity, field ) ) ).ToList( ) ).ToList( );

				/////
				// Order the results.
				/////
				inheritedFields = inheritedFields.OrderBy( pair => GetTypeDisplayName( pair.Value ) ).ToList( );

				/////
				// Cache the results.
				/////
				EntityInheritedFieldCache[ type ] = inheritedFields;
			}

			return inheritedFields;
		}

		/// <summary>
		///     Gets the derived types.
		/// </summary>
		/// <param name="type">
		///     The type whose descendants are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of descendant types.
		/// </returns>
		public static IEnumerable<Entity> GetDescendants( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			/////
			// Get the entity descendants.
			/////
			return SchemaResolver.GetDecendants( type )
			                     .Where( descendant => descendant != type )
			                     .OrderBy( GetTypeDisplayName );
		}

		/// <summary>
		///     Gets the entity description (if one exists).
		/// </summary>
		/// <param name="entity">
		///     The entity.
		/// </param>
		/// <returns>
		///     The description of the specified entity if one exists.
		/// </returns>
		public static string GetEntityDescription( Entity entity )
		{
			if ( entity != null )
			{
				/////
				// Get the 'description' member.
				/////
				string description = SchemaResolver.GetStringFieldValue( entity, Aliases.Description );

				if ( ! string.IsNullOrEmpty( description ) )
				{
					/////
					// Return the member value.
					/////
					return description;
				}
			}

			/////
			// Return the default description.
			/////
			return string.Format( "Implementation for the <see cref=\"{0}\" /> class.", GetTypeDisplayName( entity ) );
		}

		/// <summary>
		///     Gets the types forward relationships.
		/// </summary>
		/// <param name="type">
		///     The entity type whose forward relationships are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified entities forward relationships.
		/// </returns>
		public static IEnumerable<Entity> GetEntityDirectForwardRelationships( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			List<Entity> forwardRelationships;

			if ( !EntityForwardRelationshipCache.TryGetValue( type, out forwardRelationships ) )
			{
				/////
				// Get Forward Relationships.
				/////
				IEnumerable<Entity> relationshipTypes = SchemaResolver.GetInstancesOfType( ResolveAlias( Aliases.Relationship ) );

				var foundRelationships = new List<Entity>( );

				if ( relationshipTypes != null )
				{
					/////
					// Loop through the relationship types.
					/////
					foreach ( Entity relationshipType in relationshipTypes )
					{
						if ( relationshipType.Alias != null )
						{
							/////
							// Get the relationships from the specified type.
							/////
							Entity fromType = SchemaResolver.GetRelationshipFromType( relationshipType );

							if ( fromType != null && fromType.Alias != null )
							{
								if ( fromType.Alias == type.Alias )
								{
									if ( !foundRelationships.Contains( relationshipType ) )
									{
										/////
										// Found a match.
										/////
										foundRelationships.Add( relationshipType );
									}
								}
							}
						}
					}
				}

				forwardRelationships = foundRelationships;

				List<Entity> existingRelationships;

				/////
				// Exclude this types inherited reverse relationships.
				/////
				if ( InheritedEntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.Alias == null ).ToList( );

					forwardRelationships = forwardRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Exclude this types inherited forward relationships.
				/////
				if ( InheritedEntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					forwardRelationships = forwardRelationships
						.Except( existingRelationships ).ToList( );
				}

				/////
				// Exclude this types reverse relationships.
				/////
				if ( EntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.Alias == null ).ToList( );

					forwardRelationships = forwardRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Order the results.
				/////
				forwardRelationships = forwardRelationships.OrderBy( relationship => GetTypeDisplayName( type, relationship ) ).ToList( );

				/////
				// Cache the results.
				/////
				EntityForwardRelationshipCache[ type ] = forwardRelationships;
			}

			return forwardRelationships;
		}

		/// <summary>
		///     Gets the types reverse relationships.
		/// </summary>
		/// <param name="type">
		///     The type whose direct reverse relationships are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified entities reverse relationships.
		/// </returns>
		public static IEnumerable<Entity> GetEntityDirectReverseRelationships( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			List<Entity> reverseRelationships;

			if ( !EntityReverseRelationshipCache.TryGetValue( type, out reverseRelationships ) )
			{
				var foundRelationships = new List<Entity>( );

				IEnumerable<Entity> relationshipTypes = SchemaResolver.GetInstancesOfType( ResolveAlias( Aliases.Relationship ) );

				/////
				// Get the reverse relationships.
				/////
				if ( relationshipTypes != null )
				{
					/////
					// Loop through the relationship types.
					/////
					foreach ( Entity relationshipType in relationshipTypes )
					{
						if ( relationshipType.Alias != null )
						{
							/////
							// Get the relationships from the specified type.
							/////
							Entity toType = SchemaResolver.GetRelationshipToType( relationshipType );

							if ( toType != null && toType.Alias != null )
							{
								if ( toType.Alias == type.Alias )
								{
									if ( relationshipType.ReverseAlias != null )
									{
										if ( !foundRelationships.Contains( relationshipType ) )
										{
											/////
											// Found a match.
											/////
											foundRelationships.Add( relationshipType );
										}
									}
								}
							}
						}
					}
				}

				reverseRelationships = foundRelationships;

				List<Entity> existingRelationships;

				/////
				// Exclude the inherited reverse relationships.
				/////
				if ( InheritedEntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					reverseRelationships = reverseRelationships
						.Except( existingRelationships ).ToList( );
				}

				/////
				// Exclude the inherited forward relationships.
				/////
				if ( InheritedEntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationahip => existingRelationahip.ReverseAlias == null ).ToList( );

					reverseRelationships = reverseRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Exclude the forward relationships.
				/////
				if ( EntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.ReverseAlias == null ).ToList( );

					reverseRelationships = reverseRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Order the results.
				/////
				reverseRelationships = reverseRelationships.OrderBy( GetTypeDisplayName ).ToList( );

				/////
				// Cache the results.
				/////
				EntityReverseRelationshipCache[ type ] = reverseRelationships;
			}

			return reverseRelationships;
		}

		/// <summary>
		///     Gets the types inherited forward relationships.
		/// </summary>
		/// <param name="type">
		///     The entity type whose inherited forward relationships are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified entities inherited forward relationships.
		/// </returns>
		public static IEnumerable<Entity> GetEntityInheritedForwardRelationships( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			List<Entity> inheritedForwardRelationships;

			if ( !InheritedEntityForwardRelationshipCache.TryGetValue( type, out inheritedForwardRelationships ) )
			{
				/////
				// Get inherited forward relationships.
				/////
				var foundRelationships = new List<Entity>( );

				Alias[] ancestorAliases = SchemaResolver.GetAncestors( type ).Except( Enumerable.Repeat( type, 1 ) ).Select( ancestor => ancestor.Alias ).ToArray( );

				IEnumerable<Entity> relationshipTypes = SchemaResolver.GetInstancesOfType( ResolveAlias( Aliases.Relationship ) );

				if ( relationshipTypes != null )
				{
					/////
					// Loop through the relationship types.
					/////
					foreach ( Entity relationshipType in relationshipTypes )
					{
						if ( relationshipType.Alias != null )
						{
							/////
							// Get the relationships from the specified type.
							/////
							Entity fromType = SchemaResolver.GetRelationshipFromType( relationshipType );

							if ( fromType != null && fromType.Alias != null )
							{
								if ( ancestorAliases.Contains( fromType.Alias ) )
								{
									if ( !foundRelationships.Contains( relationshipType ) )
									{
										/////
										// Found a match.
										/////
										foundRelationships.Add( relationshipType );
									}
								}
							}
						}
					}
				}

				inheritedForwardRelationships = foundRelationships;

				List<Entity> existingRelationships;

				/////
				// Exclude this types inherited reverse relationships.
				/////
				if ( InheritedEntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.Alias == null ).ToList( );

					inheritedForwardRelationships = inheritedForwardRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Exclude this types forward relationships.
				/////
				if ( EntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					inheritedForwardRelationships = inheritedForwardRelationships
						.Except( existingRelationships ).ToList( );
				}

				/////
				// Exclude this types reverse relationships.
				/////
				if ( EntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.Alias == null ).ToList( );

					inheritedForwardRelationships = inheritedForwardRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Order the results.
				/////
				inheritedForwardRelationships = inheritedForwardRelationships.OrderBy( GetTypeDisplayName ).ToList( );

				/////
				// Cache the results.
				/////
				InheritedEntityForwardRelationshipCache[ type ] = inheritedForwardRelationships;
			}

			return inheritedForwardRelationships;
		}

		/// <summary>
		///     Gets the types inherited reverse relationships.
		/// </summary>
		/// <param name="type">
		///     The type whose inherited reverse relationships are to be retrieved.
		/// </param>
		/// <returns>
		///     An enumeration of entities that represent the specified entities reverse relationships.
		/// </returns>
		public static IEnumerable<Entity> GetEntityInheritedReverseRelationships( Entity type )
		{
			if ( type == null )
			{
				return Enumerable.Empty<Entity>( );
			}

			List<Entity> inheritedReverseRelationships;

			if ( !InheritedEntityReverseRelationshipCache.TryGetValue( type, out inheritedReverseRelationships ) )
			{
				/////
				// Get inherited reverse relationships.
				/////
				var foundRelationships = new List<Entity>( );

				Alias[] ancestorAliases = SchemaResolver.GetAncestors( type ).Except( Enumerable.Repeat( type, 1 ) ).Select( ancestor => ancestor.Alias ).ToArray( );

				IEnumerable<Entity> relationshipTypes = SchemaResolver.GetInstancesOfType( ResolveAlias( Aliases.Relationship ) );

				/////
				// Get Inherited reverse relationships.
				/////
				if ( relationshipTypes != null )
				{
					/////
					// Loop through the relationship types.
					/////
					foreach ( Entity relationshipType in relationshipTypes )
					{
						if ( relationshipType.Alias != null )
						{
							/////
							// Get the relationships from the specified type.
							/////
							Entity toType = SchemaResolver.GetRelationshipToType( relationshipType );

							if ( toType != null && toType.Alias != null )
							{
								if ( ancestorAliases.Contains( toType.Alias ) )
								{
									if ( relationshipType.ReverseAlias != null )
									{
										if ( !foundRelationships.Contains( relationshipType ) )
										{
											/////
											// Found a match.
											/////
											foundRelationships.Add( relationshipType );
										}
									}
								}
							}
						}
					}
				}

				inheritedReverseRelationships = foundRelationships;

				List<Entity> existingRelationships;

				/////
				// Exclude the inherited forward relationships.
				/////
				if ( InheritedEntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.ReverseAlias == null ).ToList( );

					inheritedReverseRelationships = inheritedReverseRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Exclude the forward relationships.
				/////
				if ( EntityForwardRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					foundRelationships = existingRelationships.Where( existingRelationship => existingRelationship.ReverseAlias == null ).ToList( );

					inheritedReverseRelationships = inheritedReverseRelationships
						.Except( foundRelationships ).ToList( );
				}

				/////
				// Exclude the reverse relationships.
				/////
				if ( EntityReverseRelationshipCache.TryGetValue( type, out existingRelationships ) )
				{
					inheritedReverseRelationships = inheritedReverseRelationships
						.Except( existingRelationships ).ToList( );
				}

				/////
				// Order the results.
				/////
				inheritedReverseRelationships = inheritedReverseRelationships.OrderBy( GetTypeDisplayName ).ToList( );

				/////
				// Cache the results.
				/////
				InheritedEntityReverseRelationshipCache[ type ] = inheritedReverseRelationships;
			}

			return inheritedReverseRelationships;
		}

        /// <summary>
        ///     Gets the types relationships (both forward and reverse).
        /// </summary>
        /// <param name="type">
        ///     The type whose relationships are to be retrieved (both forward and reverse).
        /// </param>
        /// <returns>
        ///     An enumeration of relationships for the specified type.
        /// </returns>
        public static IEnumerable<RelationshipData> GetEntityAllRelationships(Entity type)
        {
            var directFwd = GetEntityDirectForwardRelationships(type).Select(e => new RelationshipData(e, false, false));
            var directRev = GetEntityDirectReverseRelationships(type).Select(e => new RelationshipData(e, true, false));
            var inhForward = GetEntityInheritedForwardRelationships(type).Select(e => new RelationshipData(e, false, true));
            var inhReverse = GetEntityInheritedReverseRelationships(type).Select(e => new RelationshipData(e, true, true));
    
            return directFwd.Union(directRev).Union(inhForward).Union(inhReverse).OrderBy(rd => rd.Name);
        }

		/// <summary>
		///     Gets the display name of the entity type.
		/// </summary>
		/// <param name="entity">
		///     The entity whose type display name is to be retrieved.
		/// </param>
		/// <returns>
		///     The entity type display name.
		/// </returns>
		public static string GetEntityTypeDisplayName( Entity entity )
		{
			if ( entity == null )
			{
				return string.Empty;
			}

			return GetTypeDisplayName( SchemaResolver.GetEntityType( entity ) );
		}

		/// <summary>
		///     Gets the enumeration values.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>An array of enumeration value aliases.</returns>
		public static EnumValueInfo[] GetEnumValues( Entity type )
		{
			if ( type == null )
			{
				return new EnumValueInfo[0];
			}

			return SchemaResolver.GetRelationshipsToEntity( type, ResolveAlias( Aliases.CoreAlias( "enumValues" ) ) )
			                     .Select( e =>
			                              new EnumValueInfo
				                              {
					                              Alias = e.Alias.Namespace + ":" + e.Alias.Value,
					                              EnumValueName = e.Alias.Value.RemoveSpaces( ).ToPascalCase( )
				                              } ).ToArray( );
		}

		/// <summary>
		///     Gets the ReadiNow DatabaseType for a field type.
		/// </summary>
		/// <param name="fieldType">Type of the field.</param>
		/// <returns>
		///     The description of the specified entity if one exists.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">fieldType</exception>
		public static string GetReadiNowType( Entity fieldType )
		{
			if ( fieldType == null )
			{
				throw new ArgumentNullException( "fieldType" );
			}

			return SchemaResolver.GetStringFieldValue( fieldType, Aliases.ReadiNowType ) ?? "";
		}

		/// <summary>
		///     Gets the name of the relationships reverse alias.
		/// </summary>
		/// <param name="relationship">
		///     The relationship whose reverse alias name is to be retrieved.
		/// </param>
		/// <returns>
		///     The specified relationships reverse alias display name.
		/// </returns>
		internal static string GetRelationshipReverseAliasDisplayName( Entity relationship )
		{
			if ( relationship == null )
			{
				return string.Empty;
			}

			return relationship.ReverseAlias.Value.RemoveSpaces( ).ToPascalCase( );
		}

		/// <summary>
		///     Gets the display name of the type.
		/// </summary>
		/// <param name="typeEntity">The type entity.</param>
		/// <returns>
		///     The type display name.
		/// </returns>
		public static string GetTypeDisplayName( Entity typeEntity )
		{
			if ( typeEntity == null )
			{
				return null;
			}

			string typeName;

			if ( !TypeDisplayNameCache.TryGetValue( typeEntity, out typeName ) )
			{
				/////
				// Attempt to retrieve the 'ClassName' first.
				/////
				typeName = SchemaResolver.GetStringFieldValue( typeEntity, Aliases.ClassName );

				if ( string.IsNullOrEmpty( typeName ) )
				{
					/////
					// Attempt to retrieve the alias name second.
					/////
					typeName = typeEntity.Alias.Value;
				}

				/////
				// Ensure there are no spaces and it is in Pascal case.
				/////
				typeName = typeName.RemoveSpaces( ).ToPascalCase( );

				/////
				// Cache the result.
				/////
				TypeDisplayNameCache[ typeEntity ] = typeName;
			}

			return typeName;
		}

		/// <summary>
		///     Gets the name of the class.
		/// </summary>
		/// <param name="parentEntity">
		///     The parent entity.
		/// </param>
		/// <param name="typeEntity">
		///     The type entity.
		/// </param>
		/// <returns>
		///     The type display name.
		/// </returns>
		public static string GetTypeDisplayName( Entity parentEntity, Entity typeEntity )
		{
			return GetTypeDisplayName( parentEntity, typeEntity, true );
		}

		/// <summary>
		///     Gets the name of the class.
		/// </summary>
		/// <param name="parentEntity">The parent entity.</param>
		/// <param name="typeEntity">The type entity.</param>
		/// <param name="appendSuffix">
		///     if set to <c>true</c> [append suffix].
		/// </param>
		/// <returns>
		///     The type display name.
		/// </returns>
		public static string GetTypeDisplayName( Entity parentEntity, Entity typeEntity, bool appendSuffix )
		{
			if ( typeEntity == null )
			{
				return null;
			}

			/////
			// Get the type display name.
			/////
			string typeName = GetTypeDisplayName( typeEntity );

			if ( appendSuffix )
			{
				/////
				// Check if the parent classes name is the same as the instance name.
				/////
				string parentEntityTypeName;
				TypeDisplayNameCache.TryGetValue( parentEntity, out parentEntityTypeName );

				if ( typeName == parentEntityTypeName )
				{
					/////
					// Member name that matches the type name. Must rename.
					/////
					typeName += InstanceSuffix;
				}
			}

			return typeName;
		}

		/// <summary>
		///     Gets the display name of the type generic type.
		/// </summary>
		/// <param name="type">
		///     The type whose generic type name is to be retrieved.
		/// </param>
		/// <returns>
		///     The types generic type display name.
		/// </returns>
		public static string GetTypeGenericTypeDisplayName( Entity type )
		{
			if ( type == null )
			{
				return string.Empty;
			}

			string typeName;

			/////
			// Attempt a cache hit.
			/////
			if ( !TypeGenericTypeDisplayNameCache.TryGetValue( type, out typeName ) )
			{
				/////
				// Attempt to locate the class type member.
				/////
				typeName = SchemaResolver.GetStringFieldValue( type, Aliases.ClassType );

				if ( string.IsNullOrEmpty( typeName ) )
				{
					/////
					// Get the type display name.
					/////
					typeName = GetTypeDisplayName( type ).RemoveSpaces( ).ToPascalCase( );
				}

				/////
				// Cache the result.
				/////
				TypeGenericTypeDisplayNameCache[ type ] = typeName;
			}

			return typeName;
		}

		/// <summary>
		///     Gets the defined types.
		/// </summary>
		/// <returns>
		///     An enumeration of types.
		/// </returns>
		public static IEnumerable<Entity> GetTypes( )
		{
			/////
			// Returns only types that start with the letter specified by ProcessTypesStartingWith,
			// unless it is *, in which case all types are returned.
			/////			
			Func<Entity, bool> typeFilter;
			if ( ProcessTypesStartingWith == '*' )
			{
				typeFilter = e => true;
			}
			else
			{
				typeFilter = e => e.Alias.Value.StartsWith(
					ProcessTypesStartingWith.ToString( CultureInfo.InvariantCulture ),
					StringComparison.InvariantCultureIgnoreCase );
			}

			/////
			// Return all the entities that represent a type ordered by their display name.
			/////
			return SchemaResolver.GetInstancesOfType( ResolveAlias( Aliases.Type ) )
			                     .OrderBy( GetTypeDisplayName ).Where( typeFilter );
		}

		/// <summary>
		///     Determines whether the specified type is an enumeration type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		///     <c>true</c> if [is enumeration type] [the specified type]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsEnumerationType( Entity type )
		{
			if ( type == null )
			{
				return false;
			}

			return SchemaResolver.IsInstance( type, ResolveAlias( Aliases.CoreAlias( "enumType" ) ) );
		}

		/// <summary>
		///     Determines whether [is field read only] [the specified field].
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     <c>true</c> if [is field read only] [the specified field]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsFieldReadOnly( Entity field )
		{
			if ( field == null )
			{
				return false;
			}

			Entity entityType = SchemaResolver.GetEntityType( field );

			if ( entityType == null )
			{
				return false;
			}

			Entity flags = ResolveAlias( Aliases.Flags );

			if ( flags == null )
			{
				return false;
			}

			IEnumerable<Entity> flagEntities = SchemaResolver.GetRelationshipsFromEntity( entityType, flags );

			if ( flagEntities == null )
			{
				return false;
			}

			return flagEntities.Any( f => f.Alias == Aliases.ReadOnlyFlag );
		}

		/// <summary>
		///     Gets the display name of the forward relationship collection type.
		/// </summary>
		/// <param name="relationship">
		///     The relationship whose cardinality is to be determined.
		/// </param>
		/// <returns>
		///     Returns whether the relationship represents a collection or not.
		/// </returns>
		public static bool IsForwardRelationshipCollection( Entity relationship )
		{
			if ( relationship == null )
			{
				return false;
			}

			/////
			// Look for a cardinality member.
			/////
			Alias cardinality = SchemaResolver.GetCardinality( relationship );

			if ( cardinality != null )
			{
				/////
				// Multiple values.
				/////
				if ( cardinality == Aliases.OneToMany || cardinality == Aliases.ManyToMany )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Gets the display name of the reverse relationship collection type.
		/// </summary>
		/// <param name="relationship">
		///     The relationship whose cardinality is to be determined.
		/// </param>
		/// <returns>
		///     Returns whether the relationship represents a collection or not.
		/// </returns>
		public static bool IsReverseRelationshipCollection( Entity relationship )
		{
			if ( relationship == null )
			{
				return false;
			}

			/////
			// Look for a cardinality member.
			/////
			Alias cardinality = SchemaResolver.GetCardinality( relationship );

			if ( cardinality != null )
			{
				/////
				// Multiple values.
				/////
				if ( cardinality == Aliases.ManyToOne || cardinality == Aliases.ManyToMany )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///     Resolves the specified alias.
		/// </summary>
		/// <param name="alias">
		///     The alias.
		/// </param>
		/// <returns>
		///     The resolved alias.
		/// </returns>
		public static Entity ResolveAlias( Alias alias )
		{
			return AliasResolver[ alias ];
		}

		/// <summary>
		///     Resolves the entities type.
		/// </summary>
		/// <param name="entity">
		///     The entity whose type is to be resolved.
		/// </param>
		/// <returns>
		///     The entities.
		/// </returns>
		public static Entity ResolveType( Entity entity )
		{
			return SchemaResolver.GetEntityType( entity );
		}
	}

	public class EnumValueInfo
	{
		/// <summary>
		///     The namespace qualified alias of the enum value.
		/// </summary>
		public string Alias;

		/// <summary>
		///     The .Net enum identifier name.
		/// </summary>
		public string EnumValueName;
	}
}