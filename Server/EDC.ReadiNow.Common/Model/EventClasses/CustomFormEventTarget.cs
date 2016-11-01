// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    ///     Custom Form Event Target
    /// </summary>
    /// <seealso cref="EDC.ReadiNow.Model.IEntityEventSave" />
    public class CustomFormEventTarget : IEntityEventSave
    {
        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                var form = entity.As<CustomEditForm>();

                if (form == null)
                {
                    continue;
                }

                EnsureIsDefaultFormForType(form, state);
            }

            return false;
        }

        /// <summary>
        ///     Ensures the form is the default form for the type.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="state">The state.</param>
        private void EnsureIsDefaultFormForType(CustomEditForm form, IDictionary<string, object> state)
        {
            IEntityFieldValues changedFields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedFwdRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedRevRelationships;

            form.GetChanges(out changedFields, out changedFwdRelationships, out changedRevRelationships, false, false);

            if (changedRevRelationships == null || changedRevRelationships.Count == 0)
            {
                // No changes
                return;
            }

            var saveGraph = EventTargetStateHelper.GetSaveGraph(state);

            ResourceEventTarget.EnsureIsOnlyRelationship(EntityType.DefaultEditForm_Field.Id, true, form, changedRevRelationships, saveGraph);
        }
    }
}