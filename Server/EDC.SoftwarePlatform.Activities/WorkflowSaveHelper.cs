// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Cache;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// This class adds field and relationships to the workflow type to support storing values during a workflow run.
    /// </summary>
    public class WorkflowSaveHelper : IEntityEventSave
    {
        Random _rand = new Random();

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                var wf = entity.As<Workflow>();
                if (wf != null)
                {
                    CleanTransitions(wf);
                    CleanArgumentInstanceFromActivity(wf);
                    CleanIndexedInputArguments(wf);
                    CleanArgumentsThroughExpressionMap(wf);
                    CleanExpressionMap(wf);

                    if (string.IsNullOrEmpty(wf.WorkflowUpdateHash))
                    {
                        wf.WorkflowUpdateHash = _rand.Next().ToString(CultureInfo.InvariantCulture);
                    }

                    if (wf.WorkflowVersion == null)
                    {
                        wf.WorkflowVersion = 1;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Remove transitions that are not part of the graph
        /// </summary>
        /// <param name="wf"></param>
        private static void CleanTransitions(Workflow wf)
        {
            var wfTransStart = wf.Transitions.Select(t => t.Id).Union(wf.Terminations.Select(t => t.Id));
            var missing = wf.ContainedActivities.SelectMany(a => a.ForwardTransitions).Select(t => t.Id).Except(wfTransStart).ToList( );

            if (missing.Count > 0)
            {
                EventLog.Application.WriteWarning("Workflow has error {0} transitions removed from {1}.", missing.Count, wf.Name);
                Entity.Delete(missing);
            }
        }

        /// <summary>
        /// Remove expressions that are not pointing at anything
        /// </summary>
        /// <param name="wf"></param>
        private static void CleanExpressionMap(Workflow wf)
        {
            var delete = new List<WfExpression>();

            delete.AddRange(wf.ExpressionMap.Where(e => e.ArgumentToPopulate == null));

            foreach (var activity in wf.ContainedActivities)
            {
                delete.AddRange(activity.ExpressionMap.Where(e => e.ArgumentToPopulate == null));
            }

            if (delete.Count > 0)
            {
                EventLog.Application.WriteWarning("Workflow has {0} expressions that are not pointing at arguments, deleting.", delete.Count);
                Entity.Delete(delete.Select(d => d.Id));
            }
        }

        /// <summary>
        /// Remove indexed arguments that do not have a corresponding value argument 
        /// </summary>
        /// <param name="wf"></param>
        private static void CleanIndexedInputArguments(Workflow wf)
        {
            foreach (var activity in wf.ContainedActivities)
            {
                var eWithArgs = activity.As<EntityWithArgsAndExits>();

                if (eWithArgs != null)
                {
                    var inputArgIds = eWithArgs.InputArguments.Select(e => e.Id);
                    var toDelete = activity.ExpressionMap.Where(e => e.ArgumentToPopulate != null && (!inputArgIds.Contains(e.ArgumentToPopulate.Id) && e.ArgumentToPopulate.Name.Contains("_value"))).ToList( );

                    var deleteIds = toDelete.Select(e => e.Id).Union(toDelete.Select(e => e.ArgumentToPopulate.Id)).ToList( );

                    if (deleteIds.Count > 0)
                    {
                        EventLog.Application.WriteWarning("Workflow has {0} expressions and arguments that are not in the inputArguments, deleting.", deleteIds.Count);
                        Entity.Delete(deleteIds);
                    }
                }
            }
        }

        /// <summary>
        /// Remove argument instances that have no reverse relationships.
        /// </summary>
        /// <param name="wf"></param>
        private static void CleanArgumentInstanceFromActivity(Workflow wf)
        {
            var delete = new List<WfArgumentInstance>();
            
            foreach (var argumentInstanceFromActivity in wf.ArgumentInstanceFromActivity)
            {
                var rels = EntityTypeHelper.GetAllRelationships(argumentInstanceFromActivity, Direction.Reverse).ToList();

                Entity.Get(argumentInstanceFromActivity, rels.ToArray<IEntityRef>());

                if (rels.Any(rel => argumentInstanceFromActivity.GetRelationships(rel, Direction.Reverse).Any(r => r != null)))
                {
                    continue;
                }

                delete.Add(argumentInstanceFromActivity);
            }

            if (delete.Count > 0)
            {
                EventLog.Application.WriteWarning("Workflow has {0} dangling argument instances, deleting.", delete.Count);
                Entity.Delete(delete.Select(d => d.Id));
            }
        }

        /// <summary>
        /// Remove arguments that have no reverse relationships other than a reference back to the expression map.
        /// </summary>
        /// <param name="wf"></param>
        private static void CleanArgumentsThroughExpressionMap(Workflow wf)
        {
            var delete = new List<ActivityArgument>();

            foreach (var wfExpression in wf.ExpressionMap)
            {
                var arg = wfExpression.ArgumentToPopulate;

                if (arg != null)
                {
                    var rels = EntityTypeHelper.GetAllRelationships(arg, Direction.Reverse).ToList();

                    Entity.Get(arg, rels.ToArray<IEntityRef>()); // i.e. core:populatedByExpression

                    var any = (from rel in rels where rel.Alias != "core:argumentToPopulate" select arg.GetRelationships(rel, Direction.Reverse)).Any(members => members.Count > 0);
                    if (!any)
                        delete.Add(arg);
                }
            }

            if (delete.Count > 0)
            {
                EventLog.Application.WriteWarning("Workflow has {0} dangling arguments referenced in the expression map, deleting.", delete.Count);
                Entity.Delete(delete.Select(d => d.Id));
            }
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities.Any())
            {
                // Clear the workflow actions cache
                var cache = Factory.WorkflowActionsFactory as ICacheService;
                if (cache != null)
                    cache.Clear();
            }
        }
    }
}
