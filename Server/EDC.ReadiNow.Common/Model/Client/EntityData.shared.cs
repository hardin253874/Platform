// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model.Client;

namespace EDC.ReadiNow.Model.Client
{


	/// <summary>
	/// Various helpers for working with EntityData shared between server and client.
	/// </summary>
	public partial class EntityData
	{
		/// <summary>
		/// Walk the relationship tree and look for any changed elements
		/// </summary>
		/// <returns>
		///   <c>true</c> if this instance has changes; otherwise, <c>false</c>.
		/// </returns>
		public bool HasChanges()
		{
			bool result = false;

			WalkRelationshipTree(this, 
				node =>
				{
					result = result || node.DataState != DataState.Unchanged;
				}, 
				null, //TODO: Add relationship field test here
				relInstance =>
				{
					result = result || relInstance.DataState != DataState.Unchanged;
				}
				);

				return result;
		}


		/// <summary>
		/// Walk the relationship tree and mark everything as unchanged
		/// </summary>
		public void ClearChanges()
		{
			WalkRelationshipTree(this,
				node =>
				{
					node.DataState = DataState.Unchanged;
				},
				null, //TODO: Add relationship field test here
				relInstance =>
				{
					relInstance.DataState = DataState.Unchanged;
				}
				);
		}


		/// <summary>
		/// Walk the entityData relationships tree, visiting each node only once and running the provided action.
		/// </summary>
		/// <param name="root">The root.</param>
		/// <param name="nodeAction">The node action.</param>
		/// <param name="relationshipAction">The relationship action.</param>
		/// <param name="relationshipInstanceAction">The relationship instance action.</param>
		public static void WalkRelationshipTree(EntityData root, Action<EntityData> nodeAction, Action<RelationshipData> relationshipAction,  Action<RelationshipInstanceData> relationshipInstanceAction)
		{
			WalkRelationshipTree_internal(root, nodeAction, relationshipAction, relationshipInstanceAction, new List<EntityData>());
		}

		/// <summary>
		/// Walks the relationship tree_internal.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="nodeAction">The node action.</param>
		/// <param name="relationshipAction">The relationship action.</param>
		/// <param name="relationshipInstanceAction">The relationship instance action.</param>
		/// <param name="passedNodes">The passed nodes.</param>
		static void WalkRelationshipTree_internal(EntityData node, Action<EntityData> nodeAction,  Action<RelationshipData> relationshipAction,  Action<RelationshipInstanceData> relationshipInstanceAction, List<EntityData> passedNodes)
		{
			if (! passedNodes.Contains(node))
			{
				nodeAction(node);

				passedNodes.Add(node);
				if (node.Relationships != null)
				{
					foreach (var rel in node.Relationships)
					{
						if (relationshipAction != null)
							relationshipAction(rel);

						if (rel.Instances != null)
						{
							foreach (var inst in rel.Instances)
							{
								if (relationshipInstanceAction != null)
									relationshipInstanceAction(inst);

								WalkRelationshipTree_internal(inst.Entity, nodeAction, relationshipAction, relationshipInstanceAction, passedNodes);
							}
						}
					}
				}
			}
		}
	}
}
