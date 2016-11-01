// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
    /// <summary>
    /// Mechanism for discovering 'schema' information described in an XML configuration.
    /// That is, types, inheritance, fields, relationships, etc.
    /// </summary>
    public class SchemaResolver : ISchemaResolver
    {
        IAliasResolver _aliasResolver = null;
        IEnumerable<Entity> _entitySource;
        MultiDict<Entity, Entity> _inherits = new MultiDict<Entity, Entity>();
        MultiDict<Entity, Entity> _is = new MultiDict<Entity, Entity>();
        MultiDict<Entity, Relationship> _relationship = new MultiDict<Entity, Relationship>();
        MultiDict<Entity, Entity> _ancestors = new MultiDict<Entity, Entity>();
        MultiDict<Entity, Entity> _decendants = new MultiDict<Entity, Entity>();
        List<Entity> _allTypes = new List<Entity>();
        Dictionary<Alias, Entity> _reverseAliasToRelationship = new Dictionary<Alias, Entity>();
        List<Entity> _impliedRelationshipEntities = new List<Entity>();

        public SchemaResolver(IEnumerable<Entity> entities, IAliasResolver aliasResolver)
        {
            _aliasResolver = aliasResolver;
            _entitySource = entities;
        }


        public void Initialize()
        {
            Entity eInstancesInheritByDefault = A(Aliases.InstancesInheritByDefault); // 'instancesInheritByDefault' relationship
            Entity eInherits = A(Aliases.Inherits);        // 'inherits' relationship
            Entity eIsAlsoType = A(Aliases.IsAlsoType);    // 'isType' relationship
            Entity eType = A(Aliases.Type);                // 'type' type
            Entity eResource = A(Aliases.Resource);        // 'resource' type
            Entity eRelationship = A(Aliases.Relationship); // 'type' type
            Entity eDefaultPointsTo = A(Aliases.DefaultPointsTo);    // 'defaultPointsTo' relationship

            var explicitRelationshipInstances = new Dictionary<Entity, Relationship>();

            // First pass
            // Get list of type types.
            // I.e. type, fieldType, enumType, relationship.
            // While we're there, get the type that should be inherited by default.
            var typeDefaultInherits =
                (from entity in _entitySource
                 let inherits = GetInlineRelations(entity, eInherits)
                 where entity == eType || inherits != null && inherits.Contains(eType)
                 let inheritByDefaultList = GetInlineRelations(entity, eInstancesInheritByDefault)
                 let inheritByDefault = inheritByDefaultList == null ? null : inheritByDefaultList.FirstOrDefault()
                 select
                     new
                         {
                             Alias = entity.Alias.Value,
                             Entity = entity,
                             InheritByDefault = inheritByDefault == null ? "resource" : inheritByDefault.Alias.Value
                         }
                ).ToDictionary(t => t.Alias);

            // Second pass, get full inheritance details
            foreach (Entity entity in _entitySource)
            {
                // Store entity 'is a' information
                Entity typeEntity = _aliasResolver[entity.Type];
                _is.Add(entity, typeEntity);

                /////
                // Register a relationship from the entity to its type via the 'isOfType' relationship.
                /////
                RegisterRelationship(eIsAlsoType, false, entity, typeEntity);

                var isAlsoType = GetInlineRelations(entity, eIsAlsoType);
                if (isAlsoType != null)
                {
                    _is[entity].AddRange(isAlsoType);

                    foreach (Entity isAlsoTypeEntity in isAlsoType)
                    {
                        /////
                        // Register a relationship from the inline entity to its type via the 'isOfType' relationship.
                        /////
                        RegisterRelationship(eIsAlsoType, false, entity, isAlsoTypeEntity);
                    }
                }

                // Store entity 'inherits' information
                IEnumerable<Entity> inherits = GetInlineRelationsDirectionChecked(entity, eInherits);
                if (inherits != null)
                {
                    _inherits.Add(entity, inherits.ToList());
                }
                else
                {
                    string key = entity.Type.Alias.Value;
                    if (typeDefaultInherits.ContainsKey(key) && entity != eResource)
                    {
                        entity.Members.Add(
                            new Member
                            {
                                MemberDefinition = new EntityRef(Aliases.Inherits),
                                Value = typeDefaultInherits[key].InheritByDefault,
                                ValueAsAliases = new [] { Aliases.Resource }
                            });
                        inherits = GetInlineRelationsDirectionChecked(entity, eInherits);
                        _inherits.Add(entity, inherits.ToList());
                    }
                }

                // Check for reverse alias
                if (entity.ReverseAlias != null)
                {
                    _reverseAliasToRelationship.Add(entity.ReverseAlias, entity);
                }

                // Check if relationship instance
                if (entity.Type.Alias.RelationshipInstance)
                {
                    var from = GetInlineRelations(entity.RelationshipInstanceFrom).FirstOrDefault();
                    var to = GetInlineRelations(entity.RelationshipInstanceTo).FirstOrDefault();
                    var inSolution = entity.Members.Where(m => m.MemberDefinition.Entity != null).Where(m => m.MemberDefinition.Entity.Alias == Aliases.InSolution).SingleOrDefault();
                    if (to == null)
                        throw new BuildException("Explicit relationship must defined 'to' element.", entity.Type);
                    bool isReversed = _aliasResolver.IsReverseAlias(entity.Type.Alias);
                    var relationship = new Relationship()
                    {
                        From = isReversed ? to : from,
                        To = isReversed ? from : to,
                        Type = typeEntity,
                        SolutionEntity = inSolution != null ? A(inSolution.ValueAsAliases[0]) : null
                    };
                    _relationship.Add(typeEntity, relationship);
                    explicitRelationshipInstances.Add(entity, relationship);
                }
            }

            // Find inline relationships
            var rels =
                from entity in _entitySource
                from member in entity.Members
                let memberDefn = member.MemberDefinition
                let isReversed = _aliasResolver.IsReverseAlias(memberDefn.Alias)
                let relationDefn = !isReversed ? _aliasResolver[memberDefn] : _reverseAliasToRelationship[memberDefn.Alias]
                where IsInstance(relationDefn, eRelationship)
                from child in GetInlineRelations(member)
                select new
                {
                    Parent = entity,
                    Child = child,
                    Type = relationDefn,
                    IsReversed = isReversed
                };
            foreach (var rel in rels)
            {
                // Check if implicit or explicit
                var parent = rel.Parent;
                var child = rel.Child;

                if (rel.Child.Type.Alias.RelationshipInstance)
                {
                    // Explicit - just wire up the 'from'
                    Entity instance = child;
                    var relationship = explicitRelationshipInstances[instance];
                    if (rel.IsReversed)
                        relationship.To = parent;
                    else
                        relationship.From = parent;
                }
                else
                {
                    Entity relType = rel.Type;
                    bool isReversed = rel.IsReversed;
                    RegisterRelationship(relType, isReversed, parent, child);
                }
            }

            // Build complete ancestry
            // TODO: this needs serious help
            foreach (Entity e1 in _entitySource)
                foreach (Entity e2 in _entitySource)
                {
                    if (e1 == e2 || IsSameOrDerivedType(e1, e2))
                    {
                        _ancestors.Add(e1, e2);
                        _decendants.Add(e2, e1);
                    }
                }


            // Add default relationships
            var relDefaults =
                from r in GetInstancesOfType(eRelationship)
                let defaultTo = GetRelationshipsFromEntity(r, eDefaultPointsTo).FirstOrDefault()
                where defaultTo != null
                select new { Relationship = r, DefaultTo = defaultTo };
            foreach (var relDefault in relDefaults)
            {
                var relationship = relDefault.Relationship;
                var fromType = GetRelationshipFromType(relationship);

                foreach (var instance in GetInstancesOfType(fromType))
                {
                    if (instance == relDefault.DefaultTo)
                        continue;   // don't add a default relationship from anything to itself... (for now because it breaks resource.inherits, but need to decide if this is a good general rule)
                    if (!GetRelationshipsFromEntity(instance, relationship).Any())
                    {
                        RegisterRelationship(relDefault.Relationship, false, instance, relDefault.DefaultTo);
                    }
                }
            }
        }

        /// <summary>
        /// Registers the specified relationship, as well as an implicit relationship to the solution.
        /// </summary>
        private void RegisterRelationship(Entity relType, bool isReversed, Entity parent, Entity child)
        {
            Entity eInSolution = A(Aliases.InSolution);		// 'inSolution' relationship
            Entity eIsOfType = A(Aliases.IsAlsoType);		// 'isOfType' relationship

            Member inSolution = parent.Members.Where(m => m.MemberDefinition.Entity != null).Single(m => m.MemberDefinition.Entity.Alias == Aliases.InSolution);

            var relationship = new Relationship()
            {
                From = !isReversed ? parent : child,
                To = !isReversed ? child : parent,
                Type = relType,
                SolutionEntity = A(inSolution.ValueAsAliases[0])
            };
            _relationship.Add(relType, relationship);
        }


        /// <summary>
        /// Gets the entities that are implied by relationships, but not found in the input entity stream.
        /// </summary>
        public IEnumerable<Entity> GetImpliedRelationshipEntites()
        {
            return _impliedRelationshipEntities;
        }


        /// <summary>
        /// Returns all direct and indirect ancestors of the specified type, and the type itself.
        /// </summary>
        public IEnumerable<Entity> GetAncestors(Entity type)
        {
            return _ancestors[type];
        }


        /// <summary>
        /// Returns all direct and indirect descendants of the specified type, and the type itself.
        /// </summary>
        public IEnumerable<Entity> GetDecendants(Entity type)
        {
            return _decendants[type];
        }


        /// <summary>
        /// Returns all entities that are either directly or indirectly of the specified type.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IEnumerable<Entity> GetInstancesOfType(Entity type)
        {
            return _entitySource.Where(e => IsInstance(e, type));
        }


        /// <summary>
        /// Returns the native string value of the specified field on the specified entity.
        /// </summary>
        public Alias GetAliasFieldValue(Entity instance, Alias field)
        {
            var eField = _aliasResolver[field];

            return (
                from member in instance.Members
                where _aliasResolver[member.MemberDefinition] == eField
                select member.ValueAsAliases[0]
                ).FirstOrDefault();
        }


        /// <summary>
        /// Returns the native string value of the specified field on the specified entity.
        /// </summary>
        public string GetStringFieldValue(Entity instance, Alias field)
        {
            var eField = _aliasResolver[field];

            return (
                from member in instance.Members
                where _aliasResolver[member.MemberDefinition] == eField
                select member.Value
                ).FirstOrDefault();
        }


        /// <summary>
        /// Returns the Boolean value of the specified field on the specified entity.
        /// </summary>
        public bool GetBoolFieldValue(Entity instance, Alias field)
        {
            string result = GetStringFieldValue(instance, field);

            // TODO check for default
            if (result == null)
                return false;

            return bool.Parse(result);
        }


        /// <summary>
        /// Returns the Boolean value of the specified field on the specified entity.
        /// </summary>
        public int? GetIntFieldValue(Entity instance, Alias field)
        {
            string result = GetStringFieldValue(instance, field);

            // TODO check for default
            if (result == null)
                return null;

            return int.Parse(result);
        }

        /// <summary>
        /// Returns all entities that are related to the specified entity via the specified relationship.
        /// For example, passing in 'EDC' and 'worksFor', will result in employee entities, as worksFor points from employee to company.
        /// </summary>
        /// <param name="instance">The entity to which other entities relate. Must be the 'to' side of the relationship.</param>
        /// <param name="relationType">The relationship.</param>
        /// <returns>Entities on the 'from' side of the relationship.</returns>
        public IEnumerable<Entity> GetRelationshipsToEntity(Entity instance, Entity relationType)
        {
            var result =
                from r in _relationship[relationType]
                where r.To == instance
                select r.From;
            return result;
        }


        /// <summary>
        /// Returns all entities that are related from the specified entity via the specified relationship.
        /// For example, passing in 'Peter' and 'worksFor', will result in the 'EDC' entity, as worksFor points from employee to company.
        /// </summary>
        /// <param name="instance">The entity from which other entities are related. Must be the 'from' side of the relationship.</param>
        /// <param name="relationType">The relationship.</param>
        /// <returns>Entities on the 'to' side of the relationship.</returns>
        public IEnumerable<Entity> GetRelationshipsFromEntity(Entity instance, Entity relationType)
        {
            var result =
                from r in _relationship[relationType]
                where r.From == instance
                select r.To;
            return result;
        }


        /// <summary>
        /// Returns fields that are declared directly on the type. Does not include inherited fields.
        /// </summary>
        public IEnumerable<Entity> GetDeclaredFields(Entity type)
        {
            return GetRelationshipsToEntity(type, A(Aliases.FieldIsOnType));
        }


        /// <summary>
        /// For a given relationship type, returns the source 'from' entity type.
        /// E.g. Pass in 'worksFor', will return 'employee'.
        /// </summary>
        public IEnumerable<Relationship> GetAllRelationships()
        {
            return
                from pair in _relationship
                from value in pair.Value    // list of relationships
                select value;
        }


        /// <summary>
        /// Gets whether the specified relationship exists.
        /// </summary>
        /// <param name="relationType">Type of the relation.</param>
        /// <returns>True if the relationship exists; False otherwise.</returns>
        public bool RelationshipExists(Entity relationType)
        {
            return _relationship.ContainsKey(relationType);
        }


        /// <summary>
        /// For a given relationship type, returns the source 'from' entity type.
        /// E.g. Pass in 'worksFor', will return 'employee'.
        /// </summary>
        public Entity GetRelationshipFromType(Entity relationshipType)
        {
            var result = GetRelationshipsFromEntity(relationshipType, A(Aliases.FromType)).FirstOrDefault();
            if (result == null)
                throw new BuildException("Could not find relationship 'fromType'.", relationshipType);
            return result;
        }


        /// <summary>
        /// For a given relationship type, returns the target 'to' entity type.
        /// E.g. Pass in 'worksFor', will return 'company'.
        /// </summary>
        public Entity GetRelationshipToType(Entity relationshipType)
        {
            var result = GetRelationshipsFromEntity(relationshipType, A(Aliases.ToType)).FirstOrDefault();
            if (result == null)
                throw new BuildException("Could not find relationship 'toType'.", relationshipType);
            return result;
        }


        /// <summary>
        /// Gets just the entities that are declared immediately inline within an entity's relationship element.
        /// </summary>
        /// <param name="member">The entity relationship member.</param>
        /// <returns>The related entities.</returns>
        private IEnumerable<Entity> GetInlineRelations(Member member)
        {
            if (member == null)
                return new List<Entity>();

            // Assumed that member is a relationship type

            if (member.Children.Count > 0)
            {
                return
                    from child in member.Children
                    select _aliasResolver[child];
            }

            // Resolve single related entity that's just based on an XML alias.
            // E.g.   <person> ... <worksFor>edc</worksFor> ... <inTeam>sales,marketing</inTeam> ... </person>
            if (!string.IsNullOrEmpty(member.Value))
            {
                return
                    (from alias in member.ValueAsAliases
                     let relatedEntity = _aliasResolver[alias]
                     select relatedEntity).ToList();
            }

            return new List<Entity>();
        }


        /// <summary>
        /// Gets just the entities that are declared immediately inline within an entity's relationship element.
        /// </summary>
        /// <param name="member">The type of relationship.</param>
        /// <returns>The related entities.</returns>
        private IEnumerable<Entity> GetInlineRelations(Entity entity, Alias alias)
        {
            Member member =
                entity.Members.FirstOrDefault(m => m.MemberDefinition.Alias == alias);

            if (member == null)
                return null;

            return GetInlineRelations(member);
        }


        /// <summary>
        /// Gets just the entities that are declared immediately inline within an entity's relationship element.
        /// </summary>
        /// <param name="member">The type of relationship.</param>
        /// <returns>The related entities.</returns>
        private IEnumerable<Entity> GetInlineRelations(Entity entity, Entity relationType)
        {
            Member member =
                entity.Members.FirstOrDefault(m => _aliasResolver[m.MemberDefinition] == relationType);

            if (member == null)
                return null;

            return GetInlineRelations(member);
        }


        /// <summary>
        /// Gets just the entities that are declared immediately inline within an entity's relationship element.
        /// </summary>
        /// <param name="member">The type of relationship.</param>
        /// <returns>The related entities.</returns>
        private IEnumerable<Entity> GetInlineRelationsDirectionChecked(Entity entity, Entity relationType)
        {
            Member member =
                entity.Members.FirstOrDefault(m => _aliasResolver[m.MemberDefinition] == relationType && m.MemberDefinition.Alias == relationType.Alias);

            if (member == null)
                return null;

            return GetInlineRelations(member);
        }


        /// <summary>
        /// Returns the declared type of an entity. Only use if the entity is known to only be of one type.
        /// Otherwise call GetEntityTypes.
        /// </summary>
        public Entity GetEntityType(Entity instance)
        {
            var types = _is[instance];
            if (types.Count > 1)
                throw new BuildException("Multiple types defined.", instance);
            return types[0];
        }


        /// <summary>
        /// Returns the declared types of an entity.
        /// Does not return inherited types.
        /// </summary>
        public IEnumerable<Entity> GetEntityTypes(Entity instance)
        {
            return _is[instance];
        }


        /// <summary>
        /// Returns the types that a given type directly inherits.
        /// Does not include indirectly inherited types.
        /// </summary>
        public IEnumerable<Entity> GetDirectlyInheritedParentTypes(Entity type)
        {
            return _inherits[type];
        }


        /// <summary>
        /// Returns the true if the first type directly or indirectly derives from, or is the same as, the second type.
        /// </summary>
        public bool IsSameOrDerivedType(Entity potentialDerived, Entity potentialAncestor)
        {
            return IsDerivedType(potentialDerived, potentialAncestor, potentialDerived, 0);
        }


        /// <summary>
        /// Recursive version of TypeInherits that does the work.
        /// </summary>
        private bool IsDerivedType(Entity potentialDerived, Entity potentialAncestor, Entity originalPotentialDerived, int depth)
        {
            if (depth == 100)
                throw new BuildException("Type inheritance too deep. Circular inheritance suspected: " + originalPotentialDerived.Alias.ToString());

            if (potentialDerived == potentialAncestor)
                return true;

            List<Entity> types;
            if (!_inherits.TryGetValue(potentialDerived, out types))
                return false;

            return
                types.Exists(type => IsDerivedType(type, potentialAncestor, originalPotentialDerived, depth + 1));
        }


        /// <summary>
        /// Returns the true if the entity is of the specified type, or of a derived type.
        /// </summary>
        public bool IsInstance(Entity instance, Entity potentialType)
        {
            var types = _is[instance];

            bool result =
                types.Exists(type => type == potentialType)
                || types.Exists(type => IsDerivedType(type, potentialType, type, 0));
            return result;
        }


        /// <summary>
        /// Returns all fields that have data set for the entity.
        /// </summary>
        public IEnumerable<FieldData> GetAllEntityFields(Entity instance)
        {
            return
                from member in instance.Members
                let memberDefn = _aliasResolver[member.MemberDefinition]
                where IsInstance(memberDefn, A(Aliases.Field))
                select new FieldData()
                {
                    Field = memberDefn,
                    Value = member.Value
                };
        }


        /// <summary>
        /// Helper method to resolve aliases in as few characters as possible.
        /// </summary>
        private Entity A(Alias alias)
        {
            return _aliasResolver[alias];
        }

        /// <summary>
        /// Helper method to determine the cardinality of a relationship.
        /// </summary>
        public Alias GetCardinality(Entity relationshipType)
        {

            Alias relType = GetAliasFieldValue(relationshipType, Aliases.RelType);

            if (relType != null)
            {
                if (relType == Aliases.RelLookup || relType == Aliases.RelDependantOf || relType == Aliases.RelComponentOf ||
                    relType == Aliases.RelChoiceField)
                    return Aliases.ManyToOne;

                if (relType == Aliases.RelSingleLookup || relType == Aliases.RelSingleComponentOf ||
                    relType == Aliases.RelSingleComponent)
                    return Aliases.OneToOne;

                if (relType == Aliases.RelExclusiveCollection || relType == Aliases.RelDependants || relType == Aliases.RelComponents)
                    return Aliases.OneToMany;

                if (relType == Aliases.RelManyToMany || relType == Aliases.RelSharedDependants || relType == Aliases.RelSharedDependantsOf || relType == Aliases.RelMultiChoiceField)
                    return Aliases.ManyToMany;
            }

            Alias cardinality = GetAliasFieldValue(relationshipType, Aliases.Cardinality);
            return cardinality;
        }
    }
}
