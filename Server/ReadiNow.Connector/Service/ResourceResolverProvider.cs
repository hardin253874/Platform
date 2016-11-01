// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ReadiNow.Connector.Interfaces;
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using ReadiNow.Annotations;


namespace ReadiNow.Connector.Service
{
    /// <summary>
    /// Creates a resource resolver.
    /// </summary>
    class ResourceResolverProvider : IResourceResolverProvider
    {
        internal IEntityResolverProvider EntityResolverProvider { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityResolverProvider">The entity resolver provider.</param>
        /// <exception cref="System.ArgumentNullException">entityResolver</exception>
        public ResourceResolverProvider( [NotNull] IEntityResolverProvider entityResolverProvider )
        {
            if ( entityResolverProvider == null )
                throw new ArgumentNullException( nameof( entityResolverProvider ) );

            EntityResolverProvider = entityResolverProvider;
        }


        /// <summary>
        /// Look up an entity according to its identifier and any rules specified in the mapping resource.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <exception cref="System.ArgumentNullException">typeId</exception>
        public IResourceResolver GetResolverForType( long typeId )
        {
            return GetResolverImpl( typeId, null );
        }


		/// <summary>
		/// Look up an entity according to its identifier and any rules specified in the mapping resource.
		/// </summary>
		/// <param name="mapping">The mapping rules.</param>
		/// <returns>
		/// The entity
		/// </returns>
		/// <exception cref="System.ArgumentNullException">mapping</exception>
		/// <exception cref="ConnectorConfigException"></exception>
        public IResourceResolver GetResolverForResourceMapping( ApiResourceMapping mapping )
        {
            if ( mapping == null )
                throw new ArgumentNullException( nameof( mapping ) );

            // Get type of entity being mapped, and mapping field
            long typeId;
            long? fieldId;
            using ( new SecurityBypassContext( ) )
            {
                EntityType mappedType = mapping.MappedType;
                if ( mappedType == null )
                    throw new ConnectorConfigException( Messages.ResourceMappingHasNoType );
                typeId = mappedType.Id;

                Field field = mapping.ResourceMappingIdentityField;
                fieldId = field?.Id;
            }

            return GetResolverImpl( typeId, fieldId );
        }


        /// <summary>
        /// Creates a resolver to look up entities at the far end of a relationship.
        /// </summary>
        /// <param name="mapping">The relationship mapping rules.</param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <exception cref="System.ArgumentNullException">mapping</exception>
        /// <exception cref="ConnectorConfigException"></exception>
        public IResourceResolver GetResolverForRelationshipMapping( ApiRelationshipMapping mapping )
        {
            if ( mapping == null )
                throw new ArgumentNullException( nameof( mapping ) );

            // Get type of entity being mapped, and mapping field
            long typeId;
            long? fieldId;
            using ( new SecurityBypassContext( ) )
            {
                bool isRev = mapping.MapRelationshipInReverse == true;
                Relationship relationship = mapping.MappedRelationship;
                if ( relationship == null )
                    throw new ConnectorConfigException( Messages.RelationshipMappingHasNoRelationship );

                EntityType entityType = isRev ? relationship.FromType : relationship.ToType;
                if ( entityType == null )
                    throw new Exception( "Relationship is missing from-type or to-type." );

                typeId = entityType.Id;

                Field field = mapping.MappedRelationshipLookupField;
                fieldId = field?.Id;
            }

            return GetResolverImpl( typeId, fieldId );
        }


        /// <summary>
        /// Creates a resolver to look up an entity of a given type by a given field.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldId">The field to look up, or null.</param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <exception cref="System.ArgumentNullException">typeId</exception>
        private IResourceResolver GetResolverImpl( long typeId, long? fieldId )
        {
            if ( typeId <= 0 )
                throw new ArgumentNullException( nameof( typeId ) );

            bool allowGuids = fieldId == null;
            long fieldIdToUse = fieldId ?? WellKnownAliases.CurrentTenant.Name;

            IEntityResolver resolver = EntityResolverProvider.GetResolverForField( typeId, fieldIdToUse, secured: true );

            return new ResourceResolver( resolver, typeId, allowGuids );
        }
    }
}
