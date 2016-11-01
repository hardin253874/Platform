// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
    /// <summary>
    /// Interface for querying information declared in configuration about entities, and their types.
    /// </summary>
    /// <remarks>
	/// Note 1: These APIs generally return information regardless of where it is declared in the configuration.
    /// Note 2: Unless otherwise indicated, inheritance APIs generally refer to implied ancestors/descendants, rather than direct parent/children.
    /// Note 3: Many APIs are intentionally missing. Use the APIs themselves. For example, to learn the type of a field, just check the field definitions's type.
    /// Or to learn a relationships cardinality, follow the 'relationship cardinality' relationship itself. Etc.
	/// These APIs are primarily here to wire up the circular information from within the configuration itself, not to answer all schema question.
    /// </remarks>
    public interface ISchemaResolver
    {

        void Initialize();


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Information about an instance's type
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the type of an entity. Assumes the entity is of one type only.</summary>
        Entity GetEntityType(Entity instance);

        /// <summary>Returns true if the entity is of the specified type, or a derived type.</summary>
        bool IsInstance(Entity instance, Entity potentialType);

        /// <summary>Gets the types of an entity. Use for entities that may be of multiple types.</summary>
        IEnumerable<Entity> GetEntityTypes(Entity instance);



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Information about an instance's relationships
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets entities related to the specified entity, along the specified relationship type, where the specified entity is at the 'from' end of the relationship, and the related entities are at the 'to' end of the relationship.</summary>
        IEnumerable<Entity> GetRelationshipsFromEntity(Entity instance, Entity relationType);

        /// <summary>Gets entities related to the specified entity, along the specified relationship type, where the specified entity is at the 'to' end of the relationship, and the related entities are at the 'from' end of the relationship.</summary>
        IEnumerable<Entity> GetRelationshipsToEntity(Entity instance, Entity relationType);



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Information about an instance's fields
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns all fields that have data set for the entity.</summary>
        IEnumerable<FieldData> GetAllEntityFields(Entity instance);

        /// <summary>Reads the specified string field of the entity.</summary>
        string GetStringFieldValue(Entity instance, Alias field);

        /// <summary>Reads the specified bool field of the entity.</summary>
        bool GetBoolFieldValue(Entity instance, Alias field);
        
        /// <summary>Reads the specified integer field of the entity.</summary>
        int? GetIntFieldValue(Entity instance, Alias field);

        /// <summary>Reads the specified alias field of the entity. I.e. either 'alias' or 'reverseAlias'.</summary>
        Alias GetAliasFieldValue(Entity instance, Alias field);



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Information about types
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns true if the first type derives (directly or indirectly) from the second. Also returns true if </summary>
        bool IsSameOrDerivedType(Entity potentialDerived, Entity potentialAncestor);

        /// <summary>The type(s) that this type directly inherits from.</summary>
        IEnumerable<Entity> GetDirectlyInheritedParentTypes(Entity type);

        /// <summary>Direct and indirect inheritance ancestors, and the type itself.</summary>
        IEnumerable<Entity> GetAncestors(Entity type);

        /// <summary>Direct and indirect inheritance descendants, and the type itself.</summary>
        IEnumerable<Entity> GetDecendants(Entity type);

        /// <summary>All entities that are directly or indirectly of the specified type.</summary>
        IEnumerable<Entity> GetInstancesOfType(Entity type);

        /// <summary>Gets the fields that are declared to exactly this type. Excludes ancestors.</summary>
        IEnumerable<Entity> GetDeclaredFields(Entity type);



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Information about relationship definitions
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets target 'to' type of the specified relationship definition.</summary>
        Entity GetRelationshipToType(Entity relationshipType);

        /// <summary>Gets source 'from' type of the specified relationship definition.</summary>
        Entity GetRelationshipFromType(Entity relationshipType);

        /// <summary>Gets all relationship instances, regardless of type.</summary>
        IEnumerable<Relationship> GetAllRelationships();

        /// <summary>Gets the entities that are implied by relationships, but not found in the input entity stream.</summary>
        IEnumerable<Entity> GetImpliedRelationshipEntites();

        /// <summary>Gets the effective cardinality of the relationship type.</summary>
        Alias GetCardinality(Entity relationshipType);
    }

}
