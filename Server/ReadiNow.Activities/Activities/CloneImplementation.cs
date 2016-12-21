// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Diagnostics;

namespace EDC.SoftwarePlatform.Activities
{
    public sealed class CloneImplementation : ActivityImplementationBase, IRunNowActivity
    {
        //private const string ClonedNameFormat = "{0} (clone)";

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            using (Profiler.MeasureAndSuppress("CloneImplementation.OnRunNow"))
            {
                var resourceKey = GetArgumentKey("resourceToCloneArgument");
                var definitionKey = GetArgumentKey("newDefinitionCloneArgument");
                var clonedKey = GetArgumentKey("clonedResourceArgument");

                var resId = (IEntity)inputs[resourceKey];
                var resource = resId.As<Resource>();

                IEntity clone = null;

                EntityType cloneType = null;


                object definitionObj;
                if (inputs.TryGetValue(definitionKey, out definitionObj))
                {
                    if (definitionObj != null)
                        cloneType = ((IEntity)definitionObj).As<EntityType>();
                }

                try
                {
                    using (CustomContext.SetContext(context.EffectiveSecurityContext))
                    {

                        using (var ctx = DatabaseContext.GetContext(true))
                        {
                            var activityAs = ActivityInstance.Cast<EntityWithArgsAndExits>();

                            Action<IEntity> updateAction = c => UpdateArgsHelper.UpdateEntityFromArgs(activityAs, inputs, c);
                            
                            clone = CreateClone(resource, cloneType, updateAction);

                            clone.Save();

                            ctx.CommitTransaction();
                        }
                    };
                }
                catch (DuplicateKeyException ex)
                {
                    throw new WorkflowRunException("The Clone item failed during saving: " + ex.Message, ex);
                }
                catch (ValidationException ex)
                {
                    throw new WorkflowRunException("The Cloned item failed validation during saving: " + ex.Message, ex);
                }

                context.SetArgValue(ActivityInstance, clonedKey, clone);
            }
        }



        internal static IEntity CreateClone(Resource resource, EntityType cloneType, Action<IEntity> updateAction)
        {
            IEntity clone = null;

            if (cloneType == null || resource.EntityTypes.Count() == 1 && resource.EntityTypes.First().Id == cloneType.Id)
            {
                // simple clone                            
                clone = resource.Clone(CloneOption.Deep).As<Resource>();
            }
            else
            {
                // complex clone
                clone = Entity.Create(cloneType.Id);

                // clear fields
                var resFields = resource.IsOfType.SelectMany(t => t.GetAllFields());
                var cloneFields = cloneType.GetAllFields();
                var fieldsToClone = resFields.Intersect(cloneFields, EntityIdComparer.Singleton).ToList();

                CloneFields(resource, clone, fieldsToClone);

                var cloneTracker = new Dictionary<long, IEntity> { { resource.Id, clone } };

                CloneRelationships(resource, clone, Direction.Forward, cloneTracker);
                CloneRelationships(resource, clone, Direction.Reverse, cloneTracker);
            }

            updateAction(clone);

            return clone;
        }

        private static void CloneFields(Resource resource, IEntity clone, IList<Field> fieldsToClone)
        {
            // prefetch
            Entity.Get(resource, fieldsToClone.ToArray<IEntityRef>());

            foreach (var field in fieldsToClone)
            {
                // cannot set any value on a virtual field
                if (field.IsFieldVirtual != true)
                {
                    clone.SetField(field, resource.GetField(field));
                }
            }
        }
        
        private static void CloneRelationships(Resource resource, IEntity clone, Direction direction, IDictionary<long, IEntity> cloneTracker)
        {
            var relsToIgnore = new List<Relationship> { Resource.IsOfType_Field.As<Relationship>(), Resource.ResourceHasResourceKeyDataHashes_Field.As<Relationship>() };

            var resourceRels = EntityTypeHelper.GetAllRelationships(resource, Direction.Forward);
            var cloneRels = EntityTypeHelper.GetAllRelationships(clone, Direction.Forward);
            var toCloneRels = resourceRels.Intersect(cloneRels, EntityIdComparer.Singleton).Except(relsToIgnore, EntityIdComparer.Singleton).ToList();

            Entity.Get(resource, toCloneRels.ToArray<IEntityRef>());

            foreach (var rel in toCloneRels)
            {
                var relationship = rel;
                switch (relationship.CloneAction_Enum)
                {
                    case CloneActionEnum_Enumeration.CloneEntities:
                        var entities = resource.GetRelationships(relationship, direction);

                        // clone each entity, unless it has already been cloned (in which case substitute that one)
                        var clones = entities.Select(entity => cloneTracker.ContainsKey(entity.Id)
                            ? cloneTracker[entity.Id]
                            : CloneNoDuplicates(entity.As<Resource>(), cloneTracker)).ToList();

                        clone.SetRelationships(relationship, new EntityRelationshipCollection<IEntity>(clones), direction);
                        break;

                    case CloneActionEnum_Enumeration.CloneReferences:
                        var refs = resource.GetRelationships(relationship, direction).ToList();

                        clone.SetRelationships(relationship, new EntityRelationshipCollection<IEntity>(refs), direction);
                        break;
                }
            }
        }

        /// <summary>
        /// Manually clones the fields and relationships from the resource, substituting entities already cloned where appropriate.
        /// </summary>
        /// <param name="resource">The resource to clone.</param>
        /// <param name="cloneTracker">A dictionary of ids and entities already cloned.</param>
        /// <returns>The cloned resource.</returns>
        private static IEntity CloneNoDuplicates(Resource resource, IDictionary<long, IEntity> cloneTracker)
        {
            // create a new type as a vessel
            var clone = Entity.Create(resource.EntityTypes.First().Id);

            // clone the fields
            CloneFields(resource, clone, resource.IsOfType.SelectMany(t => t.GetAllFields()).ToList());

            // keep track of this clone for substitutions
            cloneTracker.Add(resource.Id, clone);

            // clone the relationships
            CloneRelationships(resource, clone, Direction.Forward, cloneTracker);
            CloneRelationships(resource, clone, Direction.Reverse, cloneTracker);

            return clone;
        }
    }
}