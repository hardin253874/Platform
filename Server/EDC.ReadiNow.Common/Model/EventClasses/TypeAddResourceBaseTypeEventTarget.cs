// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Type event target, called to ensure all types inherit from "core:resource".
    /// </summary>
    public class TypeAddResourceBaseTypeEventTarget : IEntityEventSave
    {
        /// <summary>
        /// core:resource's alias.
        /// </summary>
        internal static readonly string CoreResourceAlias = "resource";

        /// <summary>
        /// core namespace (used for both resource and inherits).
        /// </summary>
        internal static readonly string CoreNamespace = "core";

        /// <summary>
        /// core:inherit's alias.
        /// </summary>
        internal static readonly string CoreInheritsAlias = "inherits";

        /// <summary>
        /// Store the entity refs for each tenant.
        /// </summary>
        internal static TenantEntityCache TenantCoreResourceMap = new TenantEntityCache(CoreNamespace, CoreResourceAlias);

        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // Do nothing
        }

        /// <summary>
        /// Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        /// True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                // Intentionally do not throw
                return false;
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException();
            }

            IEntityRelationshipCollection<IEntity> inheritsRelationships;
            EntityRef inheritsRef;

            inheritsRef = new EntityRef(CoreNamespace, CoreInheritsAlias);
            foreach (IEntity entity in entities)
            {
                inheritsRelationships = entity.GetRelationships(inheritsRef);

				if ( !IsCoreResource( entity ) && inheritsRelationships.Count <= 0 )
                {
                    inheritsRelationships.Entities.Add(TenantCoreResourceMap.EntityRef.Entity);
                    entity.SetRelationships(inheritsRef, inheritsRelationships);
                }
            }

            return false;
        }

        /// <summary>
        /// Is the given entity core:resource?
        /// </summary>
        /// <param name="entity">
        /// The entity to check.
        /// </param>
        /// <returns>
        /// True if it is core:resource, false otherwise.
        /// </returns>
        internal bool IsCoreResource(IEntity entity)
        {
            return entity.Alias == CoreResourceAlias && entity.Namespace == CoreNamespace;
        }
    }
}