// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// Event target that triggers on all resource changes and passes through the event to all applicable filtered targets.
    /// </summary>
    public class ResourceTriggerFilterEventTarget : IEntityEventSave, IEntityEventDelete
    {
        const string PostActionKey = "ResourceTriggerFilterEventTarget PostActionKey";


        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            using (new SecurityBypassContext())
            {
                if (WorkflowRunContext.Current.DisableTriggers)
                    return false;

                // Deal with the upgrade situation where fsSwitches entity hasn't been laid down yet - 2016/04/28 - should be able to be removed safely after version 2.xx 
                if (!Entity.Exists(new EntityRef("core:fsSwitches")))
                    return false;

                var policyCache = Factory.ResourceTriggerFilterPolicyCache;

                using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnBeforeSave"))
                {
                    var postSaveActions = EnsurePostSaveAction(state);

                    foreach (var entity in entities)
                    {
                        IEnumerable<long> changedFields, changedRels, changedRevRels;
                        entity.GetChanges(out changedFields, out changedRels, out changedRevRels);

                        // expand the enumerations as they will be referenced multiple times.
                        changedFields = changedFields.ToList();
                        changedRels = changedRels.ToList();
                        changedRevRels = changedRevRels.ToList();

                        // Forward triggers
                        if (EvaluateForwardTriggers(policyCache, entity, changedFields, changedRels, changedRevRels, postSaveActions))
                            return true;


                        // Reverse triggers
                        if (EvaluateReverseTriggers(policyCache, entity, changedFields, changedRels, changedRevRels, postSaveActions))
                            return true;
                    }

                    return false;
                }
            }
        }

        bool EvaluateForwardTriggers(IResourceTriggerFilterPolicyCache policyCache, IEntity entity, IEnumerable<long> changedFields, IEnumerable<long> changedRels, IEnumerable<long> changedRevRels, List<Action> postSaveActions)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.EvaluateForwardTriggers"))
            {
                var typeId = entity.TypeIds.FirstOrDefault();

                if (typeId != 0)
                {
                    List<ResourceTriggerFilterDef> policies;
                    if (policyCache.TypePolicyMap.TryGetValue(typeId, out policies))
                    {
                        if (policies.Any())
                        {
                            // we match on type
                            var allChanges = changedFields.Union(changedRels).Union(changedRevRels);

                            //
                            // Check if our entity matches any of the fields and rels
                            foreach (var policy in policies)
                            {
                                if ( policy == null )
                                    continue; // assert false

                                var watchedFields = policyCache.PolicyToFieldsMap[policy.Id];
                                if (!watchedFields.Any() || watchedFields.Intersect(allChanges).Any())
                                {
                                    var handler = policyCache.PolicyTypeHandlerMap[policy.TypeIds.First()];

                                    if (handler != null)
                                    {
                                        var isNew = entity.IsTemporaryId;

                                        bool result = false;
                                        SecurityBypassContext.RunAsUser(() =>
                                        {
                                            if (handler.OnBeforeSave(policy, entity, isNew, changedFields, changedRels, changedRevRels))
                                                result = true;
                                            else
                                                AddWrappedPostSaveAction(postSaveActions, () => handler.OnAfterSave(policy, entity, isNew, changedFields, changedRels, changedRevRels));
                                        });

                                        if (result)
                                            return true;        // We failed, so bug out
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        bool EvaluateReverseTriggers(IResourceTriggerFilterPolicyCache policyCache, IEntity entity, IEnumerable<long> changedFields, IEnumerable<long> changedRels, IEnumerable<long> changedRevRels, List<Action> postSaveActions)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.EvaluateReverseTriggers"))
            {
                //
                // Check if any of the related changed resources match any of our policies
                // TODO: Can we filter this earlier so I don't have to grab the type of every other end?
                var allChangedRels = changedRels.Select(rId => new { Id = rId, Direction = Direction.Forward }).Union(changedRevRels.Select(rId => new { Id = rId, Direction = Direction.Reverse }));
                var relChanges = allChangedRels.ToDictionary(rd => rd, rd => entity.GetRelationships(rd.Id, rd.Direction));  // TODO: Can we make sure that all the type information for the entities is grabbed in a single hit? (It might already be happening)

                foreach (var kvp in relChanges)
                {
                    var relId = kvp.Key.Id;
                    var direction = kvp.Key.Direction;
                    var otherEntities = kvp.Value;

                    using (Profiler.Measure("ResourceTriggerFilterEventTarget.EvaluateReverseTriggers adds"))
                    {
                        //
                        // Test for any added entities that a policy may apply to
                        //
                        var added = GetAdded(otherEntities);

                        foreach (var otherEntity in added)
                        {
                            if (otherEntity != null)
                            {
                                bool result = false;

                                TestOtherEndPolicyAndAct(policyCache, relId, direction, entity, otherEntity, (policy) =>
                                {
                                    var handler = policyCache.PolicyTypeHandlerMap[policy.TypeIds.First()];

                                    SecurityBypassContext.RunAsUser(() =>
                                    {
                                        if (handler.OnBeforeReverseAdd(policy, relId, direction.Reverse(), otherEntity, entity, entity.IsTemporaryId))
                                            result = true;
                                        else
                                            AddWrappedPostSaveAction(postSaveActions, () => handler.OnAfterReverseAdd(policy, relId, direction.Reverse(), otherEntity, entity, entity.IsTemporaryId));
                                    });

                                });

                                if (result)
                                    return true; // It failed so bug out
                            }
                        }
                    }

                    using (Profiler.Measure("ResourceTriggerFilterEventTarget.EvaluateReverseTriggers removes"))
                    {
                        //
                        // Test for any removed policies that might apply to
                        //
                        var removed = GetRemoved(otherEntities);

                        foreach (var otherEntity in removed)
                        {
                            if (otherEntity != null)
                            {
                                bool result = false;

                                TestOtherEndPolicyAndAct(policyCache, relId, direction, entity, otherEntity, (policy) =>
                                {
                                    var handler = policyCache.PolicyTypeHandlerMap[policy.TypeIds.First()];

                                    SecurityBypassContext.RunAsUser(() =>
                                    {
                                        if (handler.OnBeforeReverseRemove(policy, relId, direction.Reverse(), otherEntity, entity))
                                            result = true;
                                        else
                                            AddWrappedPostSaveAction(postSaveActions, () => handler.OnAfterReverseRemove(policy, relId, direction.Reverse(), otherEntity, entity));
                                    });

                                });

                                if (result)
                                    return true; // It failed so bug out
                            }
                        }
                    }

                }

            }


            return false;
        }



        /// <summary>
        /// Get the added entities in a rel collection
        /// </summary>
        IEnumerable<IEntity> GetAdded(IEntityRelationshipCollection<IEntity> rels)
        {
            var removedIds = rels.Tracker.Removed.Select(i => i.Key);
            var addedIds = rels.Tracker.Added.Select(i => i.Key).Except(removedIds);

            if (addedIds.Any())
            {
                return rels.Where(e => e != null && addedIds.Contains(e.Id)).ToList();
            }
            else
            {
                return Enumerable.Empty<IEntity>();
            }
        }

        /// <summary>
        /// Get the removed entities in a rel collection, ignoring temporary entities
        /// </summary>
        IEnumerable<IEntity> GetRemoved(IEntityRelationshipCollection<IEntity> rels)
        {
            var addedIds = rels.Tracker.Added.Select(i => i.Key);
            var removedIds = rels.Tracker.Removed.Select(i => i.Key).Except(addedIds).Where(i => !EntityId.IsTemporary(i));

            if (removedIds.Any())
            {
                return Entity.Get(removedIds);
            }
            else
            {
                return Enumerable.Empty<IEntity>();
            }
        }

        /// <summary>
        /// Test the policy for the other end of the relationship
        /// </summary>
        /// <returns></returns>
        void TestOtherEndPolicyAndAct(IResourceTriggerFilterPolicyCache policyCache, long relId, Direction direction, IEntity entity, IEntity otherEntity, Action<ResourceTriggerFilterDef> action)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.TestOtherEndPolicyAndAction"))
            {
                var typeId = otherEntity.TypeIds.First();

                List<ResourceTriggerFilterDef> policies;
                if (policyCache.TypePolicyMap.TryGetValue(typeId, out policies))
                {
                    foreach (var policy in policies)
                    {
                        if (policy.UpdatedRelationshipsToTriggerOn.Any(rel => rel.Id == relId))
                            action(policy);
                    }
                }
            }
        }



        /// <summary>
        /// Add a post save action to the post save actions list, wrapped to pass the required parts of the  WorkflowRunContext through.
        /// </summary>
        /// <param name="postSaveActions"></param>
        /// <param name="act"></param>
        void AddWrappedPostSaveAction(List<Action> postSaveActions, Action act)
        {

            var beforeSaveContext = WorkflowRunContext.Current;
            Action wrappedAction = () =>
            {
                using (new WorkflowRunContext(beforeSaveContext) { TriggerDepth = beforeSaveContext.TriggerDepth, RunTriggersInCurrentThread = beforeSaveContext.RunTriggersInCurrentThread })
                {
                    act();
                }
            };

            postSaveActions.Add(wrappedAction);
        }



        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnAfterSave"))
            {
                RunAfterAction(state);
            }
        }


        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            using (new SecurityBypassContext())
            {
                entities = entities.Where(e => !e.IsTemporaryId);       // we don't care about deleted temporary entities

                if (WorkflowRunContext.Current.DisableTriggers)
                    return false;

                using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnBeforeDelete"))
                {
                    var postSaveActions = EnsurePostSaveAction(state);

                    if (OnBeforeRelsThisEnd(entities, postSaveActions))
                        return true;

                    if (OnBeforeRelsOtherEnds(entities, postSaveActions))
                        return true;
                }
            }

            return false;
        }

        bool OnBeforeRelsThisEnd(IEnumerable<IEntity> entities, List<Action> postSaveActions)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnBeforeDelete OnBeforeRelsThisEnd"))
            {
                var _policy = Factory.ResourceTriggerFilterPolicyCache;

                foreach (var entity in entities)
                {
                    var typeId = entity.TypeIds.FirstOrDefault();

                    if (typeId != 0)
                    {
                        var policies = _policy.TypePolicyMap[typeId];
                        if (policies != null)
                        {
                            foreach (var policy in policies)
                            {
                                var handler = _policy.PolicyTypeHandlerMap[policy.TypeIds.First()];

                                if (handler != null)
                                {
                                    var isNew = entity.IsTemporaryId;

                                    bool result = false;
                                    SecurityBypassContext.RunAsUser(() =>
                                    {
                                        if (handler.OnBeforeDelete(policy, entity))
                                            result = true;
                                        else
                                            postSaveActions.Add(() => handler.OnAfterDelete(policy, entity));
                                    });

                                    if (result)
                                        return true;        // We failed, so bug out
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        bool OnBeforeRelsOtherEnds(IEnumerable<IEntity> entities, List<Action> postSaveActions)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnBeforeDelete OnBeforeRelsOtherEnds"))
            {
                var policyCache = Factory.ResourceTriggerFilterPolicyCache;

                foreach (var entity in entities)
                {
                    //
                    // Get the filtered list of relationships that could possibly apply to some of the related entities
                    //
                    var filteredRels = GetRelsWithPotentialPolicies(policyCache, entity).ToList();

                    if (!filteredRels.Any())
                        continue;

                    // Prefill the cache
                    var requestString = string.Join(",", filteredRels.Select(r => (r.Item2 == Direction.Reverse ? "-#" : "#") + r.Item1.Id + ".id"));
                    BulkPreloader.Preload(new EntityRequest(entity.Id, requestString));

                    //
                    // Check each entity on the other end of the filtered rels t see if any policies apply
                    foreach (var relDir in filteredRels)
                    {
                        var rel = relDir.Item1;
                        var direction = relDir.Item2;

                        var relInstances = entity.GetRelationships(rel, direction);

                        foreach (var otherEntity in relInstances)
                        {
                            var otherType = otherEntity.TypeIds.First();

                            var policies = policyCache.TypePolicyMap[otherType].Where(p => p.UpdatedRelationshipsToTriggerOn.Contains(rel, EntityIdEqualityComparer<Relationship>.Instance));

                            foreach (var policy in policies)
                            {
                                var handler = policyCache.PolicyTypeHandlerMap[policy.TypeIds.First()];

                                bool result = false;
                                SecurityBypassContext.RunAsUser(() =>
                                {
                                if (handler.OnBeforeReverseRemove(policy, rel.Id, direction.Reverse(), otherEntity, entity))
                                        result = true;
                                    else
                                        AddWrappedPostSaveAction(postSaveActions, () => handler.OnAfterReverseRemove(policy, rel.Id, direction.Reverse(), otherEntity, entity));
                                });

                                if (result)
                                    return true;
                            }
                            
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Get the relationships that may have policies that are effected by the deletion of the entity.
        /// </summary>
        IEnumerable<Tuple<Relationship, Direction>> GetRelsWithPotentialPolicies(IResourceTriggerFilterPolicyCache policyCache, IEntity entity)
        {
            var forwardRelIds = EntityTypeHelper.GetAllRelationships(entity, Direction.Forward).Select(r => r.Id);
            var reverseRelIds = EntityTypeHelper.GetAllRelationships(entity, Direction.Reverse).Select(r => r.Id);

            // Prefill cache - note that we are requesting both the toType and fromType for each rel so we only need to do a single trip to the DB
            var allRelIds = forwardRelIds.Union(reverseRelIds).Select(r => new EntityRef(r));
            BulkPreloader.Preload(new EntityRequest(allRelIds, "toType.id, fromType.id"));

            return GetRelsWithPotentialPolicies(policyCache, forwardRelIds, Direction.Forward).Union(GetRelsWithPotentialPolicies(policyCache, reverseRelIds, Direction.Reverse));
        }

        IEnumerable<Tuple<Relationship, Direction>> GetRelsWithPotentialPolicies(IResourceTriggerFilterPolicyCache policyCache, IEnumerable<long> relIds, Direction direction)
        {
            // Get the rels policies for the other end of the relationships
            var filteredRels = Entity.Get<Relationship>(relIds).Where(r =>
            {
                var eType = direction == Direction.Forward ? r?.ToType : r?.FromType;

                if (eType == null) return false;        // to handle some malformed relationships

                var policies = policyCache.TypePolicyMap[eType.Id];

                if (policies == null) return false;

                return policies.Any(p => p.UpdatedRelationshipsToTriggerOn.Contains(r, EntityIdEqualityComparer<Relationship>.Instance));
            });

            return filteredRels.Select(r => new Tuple<Relationship, Direction>(r, direction));
        }



        /// <summary>
        /// Make sure postSaveActions have been added to the state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        List<Action> EnsurePostSaveAction(IDictionary<string, object> state)
        {
            List<Action> postSaveActions;
            object postSaveActionsO = null;

            if (!state.TryGetValue(PostActionKey, out postSaveActionsO))
            {
                postSaveActions = new List<Action>();
                state[PostActionKey] = postSaveActions;
            }
            else
            {
                postSaveActions = (List<Action>)postSaveActionsO;
            }

            return postSaveActions;
        }

        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            using (Profiler.Measure("ResourceTriggerFilterEventTarget.OnAfterDelete"))
            {
                RunAfterAction(state);
            }
        }


        void RunAfterAction(IDictionary<string, object> state)
        {
            if (WorkflowRunContext.Current.DisableTriggers)
                return;

            var postSaveActions = (List<Action>)state[PostActionKey];

            foreach (var act in postSaveActions)
            {
                act();
            }

            postSaveActions.Clear();
        }

    }
}
