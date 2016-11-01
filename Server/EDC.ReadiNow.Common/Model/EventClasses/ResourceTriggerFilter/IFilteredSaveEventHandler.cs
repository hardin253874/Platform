// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// Implement this interface along with IfilteredTargetHandlerFactory to hook for ResourceTriggerFilters
    /// </summary>
    public interface IFilteredSaveEventHandler
    {
        /// <summary>
        /// Fired before saving
        /// </summary>
        /// <returns>True if the save is to be cancelled</returns>
        bool OnBeforeSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels);

        /// <summary>
        /// Fired after saving
        /// </summary>
        void OnAfterSave(ResourceTriggerFilterDef policy, IEntity entity, bool isNew, IEnumerable<long> changedFields, IEnumerable<long> changedForwardRels, IEnumerable<long> changedReverseRels);

        /// <summary>
        /// Fired before being added on another resources relationships
        /// </summary>
        /// <returns>True if the save is to be cancelled</returns>
        bool OnBeforeReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew);

        /// <summary>
        /// Fired before being removed from a another resources relationships
        /// </summary>
        /// <returns>True if the save is to be cancelled</returns>
        bool OnBeforeReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity);

        /// <summary>
        /// Fired after being added to another resources relationship
        /// </summary>
        void OnAfterReverseAdd(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity, bool isNew);

        /// <summary>
        /// Fired after being removed to another resources relationship
        /// </summary>
        void OnAfterReverseRemove(ResourceTriggerFilterDef policy, long relationshipId, Direction direction, IEntity policyEntity, IEntity otherEntity);


        /// <summary>
        /// Fired before entity is deleted
        /// </summary>
        /// <returns>True if the delete is to be cancelled</returns>
        bool OnBeforeDelete(ResourceTriggerFilterDef policy, IEntity entity);


        /// <summary>
        /// Fired after an entity is deleted
        /// </summary>
        void OnAfterDelete(ResourceTriggerFilterDef policy, IEntity entity);
    }
}
