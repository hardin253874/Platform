// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Protectable type event target
    /// </summary>
    /// <seealso cref="EDC.ReadiNow.Model.IEntityEventSave" />
    public class ProtectableTypeEventTarget : IEntityEventSave
    {
        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
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
                return false;
            }

            var enumerable = entities as IList<IEntity> ?? entities.ToList();

            foreach (var entity in enumerable)
            {
                var protectableType = entity.As<ProtectableType>();
                if (protectableType == null)
                {
                    continue;
                }

                var entityInternal = entity as IEntityInternal;

                if (entityInternal != null &&
                    entityInternal.IsTemporaryId &&
                    entityInternal.CloneOption == CloneOption.Deep &&
                    entityInternal.CloneSource != null)
                {
                    // The entity being saved is a clone. Get the source entity
                    // and update the CanModifyProtectedResource field of the new entity (which is a clone)
                    // based on the solution of the source
                    var sourceEntity = Entity.Get<ProtectableType>(entityInternal.CloneSource);
                    var sourceSolution = sourceEntity.InSolution;
                    var canModifySourceApplication = sourceSolution == null || (sourceSolution.CanModifyApplication == true);
                    protectableType.CanModifyProtectedResource = canModifySourceApplication ? null : (bool?)true;
                }

                // Get the changes
                IEntityFieldValues changedFields;
                IDictionary<long, IChangeTracker<IMutableIdKey>> changedFwdRelationships;
                IDictionary<long, IChangeTracker<IMutableIdKey>> changedRevRelationships;

                entity.GetChanges(out changedFields, out changedFwdRelationships, out changedRevRelationships, false, true, false);

                UpdateCanModifyProtectedResourceField(protectableType, changedFwdRelationships);
            }

            return false;
        }

        /// <summary>
        /// Updates the can modify protected resource field.
        /// </summary>
        /// <param name="protectableType">Type of the protectable.</param>
        /// <param name="changedFwdRelationships">The changed forward relationships.</param>
        /// <exception cref="PlatformSecurityException"></exception>
        /// <exception cref="EntityRef"></exception>
        private void UpdateCanModifyProtectedResourceField(ProtectableType protectableType, IDictionary<long, IChangeTracker<IMutableIdKey>> changedFwdRelationships)
        {
            IChangeTracker<IMutableIdKey> inSolutionChanges;

            if (changedFwdRelationships == null || !changedFwdRelationships.TryGetValue(Solution.InSolution_Field.Id, out inSolutionChanges)) return;

            // Have inSolution Changes. Update the CanModifyProtectedResource field
            Solution oldSolution = null;

            if (!protectableType.IsTemporaryId)
            {
                var oldProtectableType = Entity.Get<ProtectableType>(protectableType.Id);
                oldSolution = oldProtectableType.InSolution ?? oldProtectableType.IndirectInSolution;
            }

            var newSolution = protectableType.InSolution;

            var canModifyOldApplication = oldSolution == null || (oldSolution.CanModifyApplication == true);
            var canModifyNewApplication = newSolution == null || (newSolution.CanModifyApplication == true);
            var canModifyProtectedResource = protectableType.CanModifyProtectedResource ?? false;

            if (canModifyOldApplication || canModifyProtectedResource)
            {
                protectableType.CanModifyProtectedResource = canModifyNewApplication ? null : (bool?) true;
            }
            else
            {
                var enableAppLockdown = !Factory.FeatureSwitch.Get("disableAppLockdown");
                if (enableAppLockdown)
                {
                    // Application is locked. Shouldn't be here
                    throw new PlatformSecurityException(RequestContext.GetContext().Identity.Name, new[] { Permissions.Modify }, new[] { new EntityRef(protectableType) });
                }                
            }
        }
    }
}