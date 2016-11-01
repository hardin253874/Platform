// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
    /// <summary>
    /// Any mechanism that can convert aliases to entities.
    /// </summary>
    public interface IAliasResolver
    {
        /// <summary>
        /// Looks up an entity by an entity reference.
        /// Alias name may also be a reverse alias.
        /// If the entity reference already contains the entity itself, then that gets returned.
        /// Throw an exception if not found.
        /// </summary>
        Entity this[EntityRef entityRef] { get; }

        /// <summary>
        /// Looks up an entity by its alias name, or reverse alias.
        /// Throw an exception if not found.
        /// </summary>
        Entity this[Alias alias] { get; }

        /// <summary>
        /// Returns true if the alias is actually a reverse alias.
        /// </summary>
        bool IsReverseAlias(Alias alias);
    }



    /// <summary>
    /// An alias resolver that finds entities from a stream of entities.
    /// </summary>
    public class EntityStreamAliasResolver : IAliasResolver
    {
        // TODO: Only consume entity stream on demand (i.e. on unresolved alias)

        // Maps aliases to entities. Also contains reverse aliases.
        Dictionary<Alias, Entity> _entityByAlias = new Dictionary<Alias, Entity>();

        // Set of aliases that are actually reverse aliases.
        HashSet<Alias> _reverseAlias = new HashSet<Alias>();



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entities">A source of entities.</param>
        public EntityStreamAliasResolver(IEnumerable<Entity> entities)
        {
            Populate(entities);
        }


        /// <summary>
        /// Read the stream of entities and capture alias-to-entity mappings.
        /// </summary>
        /// <remarks>Also captures reverse aliases.</remarks>
        /// <param name="entities">List of entities.</param>
        public void Populate(IEnumerable<Entity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            List<Alias> errors = new List<Alias>();

            // Visit each entity
            foreach (Entity entity in entities)
            {
                // Capture alias
                AddAlias(entity.Alias, entity, errors);

                // Capture reverse alias, if present
                if (entity.ReverseAlias != null)
                {
                    AddAlias(entity.ReverseAlias, entity, errors);
                    _reverseAlias.Add(entity.ReverseAlias);
                }
            }

            if (errors.Count > 0)
            {
                string aliases = string.Join(", ", errors.Select(e => e.ToString()).Distinct());
                throw new BuildException("The following aliases are used by multiple entities: " + aliases, errors.Last());
            }            
        }


        /// <summary>
        /// Map an alias to an entity.
        /// </summary>
        /// <param name="alias">The alias to map. May be a reverse alias.</param>
        /// <param name="entity">The entity it points to.</param>
        /// <param name="errors">Container to place any errors into. (Such as duplicate aliases).</param>
        private void AddAlias(Alias alias, Entity entity, List<Alias> errors)
        {
            if (alias == null)
                return;
            if (_entityByAlias.ContainsKey(alias))
            {
                errors.Add(alias);
            }
            else
            {
                _entityByAlias.Add(alias, entity);
            }
        }


        /// <summary>
        /// Resolve an entity reference to an entity.
        /// </summary>
        /// <remarks>
        /// If the entity reference itself already contains an entity, then that is returned.
        /// Also references based on reverse aliases.
        /// </remarks>
        /// <param name="entityRef">The entity reference.</param>
        /// <returns>The entity.</returns>
        public Entity this[EntityRef entityRef]
        {
            get
            {
                if (entityRef.Entity != null)
                    return entityRef.Entity;

                if (entityRef.Alias != null)
                {
                    entityRef.Entity = this[entityRef.Alias];
                    return entityRef.Entity;
                }

                throw new Exception("Entity reference is invalid.");
            }
        }


        /// <summary>
        /// Resolves an alias to an entity.
        /// Also accepts reverse aliases.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>The entity.</returns>
        public Entity this[Alias alias]
        {
            get
            {
                if (alias == null || alias.Value == string.Empty)
                    throw new ArgumentNullException("alias");

                Entity entity;
                if (!_entityByAlias.TryGetValue(alias, out entity))
                    throw new BuildException(string.Format("The alias '{0}:{1}' could not be resolved.", alias.Namespace, alias.Value), alias);
                return entity;
            }
        }


        /// <summary>
        /// Determines if the requested alias is a reverse alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>True if it is a reverse alias, otherwise false.</returns>
        public bool IsReverseAlias(Alias alias)
        {
            return _reverseAlias.Contains(alias);
        }
    }
}
