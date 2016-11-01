// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Hierarchy events.
    /// </summary>
    public class HierarchyEventTarget : IEntityEventSave
    {
        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="state"></param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // I wish interfaces in C# had an optional modifier!
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return false;
            }

            foreach (IEntity entity in entities)
            {
                var structureView = entity.As<StructureView>();

                if (structureView == null)
                {
                    continue;
                }

                var definition = structureView.ReportUsesDefinition;
                var relationship = structureView.StructureHierarchyRelationship;
                if (definition == null || relationship == null)
                {
                    continue;
                }

                // check the relationship belongs to the selected definition. And its recursive.
                if (definition.Id == relationship.FromType.Id && definition.Id == relationship.ToType.Id)
                {
                    var cardinalityEnum = relationship.Cardinality.Alias;
                    switch (cardinalityEnum)
                    {
                        case "core:oneToMany":
                            structureView.FollowRelationshipInReverse = false;
                            break;
                        case "core:manyToOne":
                            structureView.FollowRelationshipInReverse = true;
                            break;
                    }
                }
                else
                {
                    throw new InvalidValueException(String.Format("Invalid relationship selected for Hierarchy: {0}. Selected relationship should be a recursive relationship with both ends pointing to '{1}' object.", structureView.Name, definition.Name));                    
                }
            }

            return false;
        }
    }
}
