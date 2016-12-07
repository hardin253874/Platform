// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Entity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using EDC.Collections;
using EDC.Database;
using Model = EDC.ReadiNow.Model;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.Database.Types;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
	/// <summary>
	///     Helper class for interacting with structured query objects.
	/// </summary>
	public static class StructuredQueryHelper
	{
		#region Public Methods

        /// <summary>
        /// Create the deepclone / hash serializer.
        /// </summary>
        public static ProtoBuf.Meta.RuntimeTypeModel CreateSerializer()
        {
            var model = ProtoBuf.Meta.TypeModel.Create();
            //TODO: the best way to addSubType is that add protoInclude attribute in EDC.Common.Database.DatabaseType class
            //can't add protobuf-net reference in EDC.Common project now. manually add in this method until change in Database class attribute
            model.Add(typeof(DatabaseType), true)
                 .AddSubType(100, typeof(BinaryType))
                 .AddSubType(101, typeof(BoolType))
                 .AddSubType(102, typeof(GuidType))
                 .AddSubType(103, typeof(StringType))
                 .AddSubType(104, typeof(StructureLevelsType))
                 .AddSubType(105, typeof(ChoiceRelationshipType))
                 .AddSubType(106, typeof(InlineRelationshipType))
                 .AddSubType(107, typeof(XmlType))
                 .AddSubType(108, typeof(UnknownType))
                 .AddSubType(109, typeof(DateTimeType))
                 .AddSubType(110, typeof(DateType))
                 .AddSubType(111, typeof(TimeType))
                 .AddSubType(112, typeof(Int32Type))
                 .AddSubType(113, typeof(IdentifierType))
                 .AddSubType(114, typeof(DecimalType))
                 .AddSubType(115, typeof(CurrencyType))
                 .AddSubType(117, typeof(AutoIncrementType));         
            model.InferTagFromNameDefault = true;
            return model;
        }

		/// <summary>
		/// Serializes the structured query to xml.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="query">The query.</param>
		public static void ToXml(XmlWriter xmlWriter, StructuredQuery query)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(StructuredQuery));
			serializer.Serialize(xmlWriter, query);
		}


		/// <summary>
		/// Serializes the structured query to xml.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static string ToXml(StructuredQuery query)
		{
			StringBuilder sb = new StringBuilder();
			var settings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = true};
		    using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
			{
				ToXml(xmlWriter, query);
				xmlWriter.Flush();
			}

			return sb.ToString();
		}


		/// <summary>
		/// Deserializes the structured query from xml.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static StructuredQuery FromXml(string xml)
		{
			using (TextReader reader = new StringReader(xml))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(StructuredQuery));
				StructuredQuery result = (StructuredQuery)serializer.Deserialize(reader);
				return result;
			}
		}


		/// <summary>
		/// Deserializes the structured query from xml.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns></returns>
		public static StructuredQuery FromXml(XmlNode node)
		{
			// Note: This originally used an XmlNodeReader which did not
			// behave correctly. It resulted in the TypedValue.Value
			// property of the QueryCondition object to be de-serialized
			// to an XmlNode instead of the actual type.            
			using (StringReader reader = new StringReader(node.OuterXml))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(StructuredQuery));
				StructuredQuery result = (StructuredQuery)serializer.Deserialize(reader);
				return result;
			}
		}


		/// <summary>
		/// Returns true if the two expressions are equal, false otherwise.
		/// </summary>
		/// <param name="expression1">The expression1.</param>
		/// <param name="expression2">The expression2.</param>
		/// <returns></returns>
		public static bool AreExpressionsEqual(ScalarExpression expression1, ScalarExpression expression2)
		{
			bool areExpressionsEqual = false;

			if (expression1 != null &&
				expression2 != null &&
				expression1.GetType() == expression2.GetType())
			{
				if (expression1.GetType() == typeof(EntityExpression))
				{
					EntityExpression entityExpression1 = expression1 as EntityExpression;
					EntityExpression entityExpression2 = expression2 as EntityExpression;

					if (entityExpression2 != null && (entityExpression1 != null && entityExpression1.NodeId == entityExpression2.NodeId))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(AggregateExpression))
				{
					AggregateExpression aggregateExpression1 = expression1 as AggregateExpression;
					AggregateExpression aggregateExpression2 = expression2 as AggregateExpression;

					if (aggregateExpression2 != null && (aggregateExpression1 != null && (aggregateExpression1.AggregateMethod == aggregateExpression2.AggregateMethod &&
					                                                                      aggregateExpression1.NodeId == aggregateExpression2.NodeId &&
					                                                                      AreExpressionsEqual(aggregateExpression1.Expression, aggregateExpression2.Expression))))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(ResourceDataColumn))
				{
					ResourceDataColumn resourceDataColumn1 = expression1 as ResourceDataColumn;
					ResourceDataColumn resourceDataColumn2 = expression2 as ResourceDataColumn;

					if (resourceDataColumn2 != null && (resourceDataColumn1 != null && (resourceDataColumn1.NodeId == resourceDataColumn2.NodeId &&
					                                                                    resourceDataColumn1.FieldId.Id == resourceDataColumn2.FieldId.Id)))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(IdExpression))
				{
					IdExpression idExpression1 = expression1 as IdExpression;
					IdExpression idExpression2 = expression2 as IdExpression;

					if (idExpression2 != null && (idExpression1 != null && idExpression1.NodeId == idExpression2.NodeId))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(StructureViewExpression))
				{
					StructureViewExpression structureViewExpression1 = expression1 as StructureViewExpression;
					StructureViewExpression structureViewExpression2 = expression2 as StructureViewExpression;

					if (structureViewExpression2 != null && (structureViewExpression1 != null && (structureViewExpression1.NodeId == structureViewExpression2.NodeId &&
					                                                                              structureViewExpression1.StructureViewId.Id == structureViewExpression2.StructureViewId.Id)))
					{
						areExpressionsEqual = true;
					}
				}               
				else if (expression1.GetType() == typeof(ResourceExpression))
				{
					ResourceExpression resourceExpression1 = expression1 as ResourceExpression;
					ResourceExpression resourceExpression2 = expression2 as ResourceExpression;

					if (resourceExpression2 != null && (resourceExpression1 != null && (resourceExpression1.NodeId == resourceExpression2.NodeId &&
					                                                                    resourceExpression1.FieldId.Id == resourceExpression2.FieldId.Id)))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(ColumnReference))
				{
					ColumnReference columnReference1 = expression1 as ColumnReference;
					ColumnReference columnReference2 = expression2 as ColumnReference;

					if (columnReference2 != null && (columnReference1 != null && columnReference1.ColumnId == columnReference2.ColumnId))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(IfElseExpression))
				{
					IfElseExpression ifElseExpression1 = expression1 as IfElseExpression;
					IfElseExpression ifElseExpression2 = expression2 as IfElseExpression;

					if (ifElseExpression2 != null && (ifElseExpression1 != null && (AreExpressionsEqual(ifElseExpression1.BooleanExpression, ifElseExpression2.BooleanExpression) &&
					                                                                AreExpressionsEqual(ifElseExpression1.ElseBlockExpression, ifElseExpression2.ElseBlockExpression) &&
					                                                                AreExpressionsEqual(ifElseExpression1.IfBlockExpression, ifElseExpression2.IfBlockExpression))))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(ComparisonExpression))
				{
					ComparisonExpression comparisonExpression1 = expression1 as ComparisonExpression;
					ComparisonExpression comparisonExpression2 = expression2 as ComparisonExpression;

					if (comparisonExpression2 != null && (comparisonExpression1 != null && (comparisonExpression1.Operator == comparisonExpression2.Operator &&
					                                                                        AreExpressionEqual(comparisonExpression1.Expressions, comparisonExpression2.Expressions))))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(LogicalExpression))
				{
					LogicalExpression logicalExpression1 = expression1 as LogicalExpression;
					LogicalExpression logicalExpression2 = expression2 as LogicalExpression;

					if (logicalExpression2 != null && (logicalExpression1 != null && (logicalExpression1.Operator == logicalExpression2.Operator &&
					                                                                  AreExpressionEqual(logicalExpression1.Expressions, logicalExpression2.Expressions))))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(CalculationExpression))
				{
					CalculationExpression calculationExpression1 = expression1 as CalculationExpression;
					CalculationExpression calculationExpression2 = expression2 as CalculationExpression;

					if (calculationExpression2 != null && (calculationExpression1 != null && (calculationExpression1.Operator == calculationExpression2.Operator &&
					                                                                          AreExpressionEqual(calculationExpression1.Expressions, calculationExpression2.Expressions))))
					{
						areExpressionsEqual = true;
					}
				}
				else if (expression1.GetType() == typeof(ScriptExpression))
				{
					ScriptExpression scriptExpression1 = expression1 as ScriptExpression;
					ScriptExpression scriptExpression2 = expression2 as ScriptExpression;

					if (scriptExpression2 != null && (scriptExpression1 != null && scriptExpression1.Script == scriptExpression2.Script))
					{
						areExpressionsEqual = true;
					}
                }
                else if ( expression1.GetType( ) == typeof( MutateExpression ) )
                {
                    MutateExpression mutateExpression1 = expression1 as MutateExpression;
                    MutateExpression mutateExpression2 = expression2 as MutateExpression;

                    if ( mutateExpression2 != null )
                    {
                        areExpressionsEqual = AreExpressionsEqual( mutateExpression1, mutateExpression2 );
                    }
                }
				else if (expression1.GetType() == typeof(LiteralExpression))
				{
					LiteralExpression literalExpression1 = expression1 as LiteralExpression;
					LiteralExpression literalExpression2 = expression2 as LiteralExpression;

					if (literalExpression2 != null && (literalExpression1 != null && (literalExpression1.Value != null &&
					                                                                  literalExpression2.Value != null &&
					                                                                  literalExpression1.Value.Type != null &&
					                                                                  literalExpression2.Value.Type != null &&
					                                                                  literalExpression1.Value.Type.GetType() == literalExpression2.Value.Type.GetType() &&                        
					                                                                  DatabaseTypeHelper.ConvertToString(literalExpression1.Value.Type, literalExpression1.Value.Value) == DatabaseTypeHelper.ConvertToString(literalExpression2.Value.Type, literalExpression2.Value.Value))))
					{
						areExpressionsEqual = true;
					}
				}
			}

			return areExpressionsEqual;
		}

		

		/// <summary>
		/// Ares the expression equal.
		/// </summary>
		/// <param name="expressions1">The expressions1.</param>
		/// <param name="expressions2">The expressions2.</param>
		/// <returns></returns>
		private static bool AreExpressionEqual<T>(IList<T> expressions1, IList<T> expressions2) where T : ScalarExpression
		{
			bool areExpressionEqual = false;

			if (expressions1 != null && expressions2 != null && expressions1.Count == expressions2.Count)
			{
			    areExpressionEqual = !expressions1.Where((t, i) => !AreExpressionsEqual(t, expressions2[i])).Any();
			}

			return areExpressionEqual;
		}


		/// <summary>
		/// Gets the entities referenced by the specified expression.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		public static IEnumerable<EntityRef> GetReferencedEntities(ScalarExpression expression)
		{
			List<EntityRef> referencedEntities = new List<EntityRef>();            

			if (expression == null)
			{
				return referencedEntities;
			}

			GetReferencedEntities(expression, referencedEntities);            

			return referencedEntities;
		}


		/// <summary>
		/// Gets the entities referenced by the specified query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns></returns>
		public static IEnumerable<Model.EntityRef> GetReferencedEntities(StructuredQuery query)
		{
			List<Model.EntityRef> referencedEntities = new List<Model.EntityRef>();

			if (query == null)
			{
				return referencedEntities;
			}

			GetReferencedEntities(query.RootEntity, referencedEntities);

			if (query.SelectColumns != null)
			{
				query.SelectColumns.ForEach(sc => GetReferencedEntities(sc.Expression, referencedEntities));                
			}

			if (query.OrderBy != null)
			{
				query.OrderBy.ForEach(ob => GetReferencedEntities(ob.Expression, referencedEntities));                                
			}

			if (query.Conditions != null)
			{
				query.Conditions.ForEach(c => GetReferencedEntities(c.Expression, referencedEntities));                                                
			}
			
			return referencedEntities;
		}

		/// <summary>
		/// Determines the root resource type being reported.
		/// </summary>
		public static EDC.ReadiNow.Model.EntityRef GetRootType(StructuredQuery query)
		{
			if (query == null)
				return null;

			Entity entity = query.RootEntity;
			while (true)
			{
				if (entity is ResourceEntity)
				{
					var resourceEntity = (ResourceEntity)entity;
					return resourceEntity.EntityTypeId;
				}
				if (entity is AggregateEntity)
				{
					entity = ((AggregateEntity)entity).GroupedEntity;
					continue;
				}
				
				// otherwise, we don't know what this reporting entity is, and don't know how to get a type ID from it.
				return null;
			}
		}

        #endregion


        #region Non-Public Methods
		/// <summary>
		/// Gets the referenced entities.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="referencedEntities">The referenced entities.</param>
		private static void GetReferencedEntities(Entity entity, List<Model.EntityRef> referencedEntities)
		{
			if (entity == null)
			{
				return;
			}

			if (entity.RelatedEntities != null)
			{
				entity.RelatedEntities.ForEach(re => GetReferencedEntities(re, referencedEntities));                
			}  

			if (entity.GetType() == typeof(RelatedResource))
			{
				RelatedResource relatedResource = entity as RelatedResource;

				if (relatedResource != null)
				{
                    if (relatedResource.EntityTypeId != null)
                    {
                        referencedEntities.Add(relatedResource.EntityTypeId);
                    }

                    if (relatedResource.RelationshipTypeId != null)
                    {
                        referencedEntities.Add(relatedResource.RelationshipTypeId);
                    }
                }
			}
			else if (entity.GetType() == typeof(ResourceEntity))
			{
				ResourceEntity resourceEntity = entity as ResourceEntity;
				if (resourceEntity != null && resourceEntity.EntityTypeId != null)
				{
					referencedEntities.Add(resourceEntity.EntityTypeId);
				}                                           
			}
			else if (entity.GetType() == typeof(AggregateEntity))
			{
				AggregateEntity aggregateEntity = entity as AggregateEntity;

			    if (aggregateEntity != null)
			    {
			        GetReferencedEntities(aggregateEntity.GroupedEntity, referencedEntities);

			        GetReferencedEntities(aggregateEntity.GroupBy, referencedEntities);
			    }
			}                        
		}


		/// <summary>
		/// Gets the referenced entities.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="referencedEntities">The referenced entities.</param>
		private static void GetReferencedEntities(ScalarExpression expression, List<Model.EntityRef> referencedEntities)
		{
			if (expression == null)
			{
				return;
			}

			if (expression is AggregateExpression)
			{
				AggregateExpression aggregateExpression = expression as AggregateExpression;
				GetReferencedEntities(aggregateExpression.Expression, referencedEntities);
			}
			else if (expression is ResourceDataColumn)
			{
				ResourceDataColumn resourceDataColumn = expression as ResourceDataColumn;
				if (resourceDataColumn.FieldId != null)
				{
					referencedEntities.Add(resourceDataColumn.FieldId);
				}
			}
			else if (expression is StructureViewExpression)
			{
				StructureViewExpression structureViewExpression = expression as StructureViewExpression;
				if (structureViewExpression.StructureViewId != null)
				{                    
					referencedEntities.Add(structureViewExpression.StructureViewId);
				}
			}            
			else if (expression is CalculationExpression)
			{
				CalculationExpression calculationExpression = expression as CalculationExpression;
				GetReferencedEntities(calculationExpression.Expressions, referencedEntities);
			}
			else if (expression is IfElseExpression)
			{
				IfElseExpression ifElseExpression = expression as IfElseExpression;
				GetReferencedEntities(ifElseExpression.BooleanExpression, referencedEntities);
				GetReferencedEntities(ifElseExpression.ElseBlockExpression, referencedEntities);
				GetReferencedEntities(ifElseExpression.IfBlockExpression, referencedEntities);
			}
			else if (expression is LogicalExpression)
			{
				LogicalExpression logicalExpression = expression as LogicalExpression;
				GetReferencedEntities(logicalExpression.Expressions, referencedEntities);
			}
			else if (expression is ComparisonExpression)
			{
				ComparisonExpression comparisonExpression = expression as ComparisonExpression;
				GetReferencedEntities(comparisonExpression.Expressions, referencedEntities);
            }
            else if ( expression is MutateExpression )
            {
                MutateExpression mutateExpression = expression as MutateExpression;
                GetReferencedEntities( mutateExpression.Expression, referencedEntities );
            }
		}   


		/// <summary>
		/// Gets the referenced entities.
		/// </summary>
		/// <param name="expressions">The expressions.</param>
		/// <param name="referencedEntities">The referenced entities.</param>
		private static void GetReferencedEntities(IEnumerable<ScalarExpression> expressions, List<Model.EntityRef> referencedEntities)
		{
			if (expressions == null)
			{
				return;
			}            

			foreach (ScalarExpression expression in expressions)
			{
				GetReferencedEntities(expression, referencedEntities);
			}
		}

	  

        #endregion


        /// <summary>
        /// Walk the entity node tree, returning all nodes.
        /// </summary>
        /// <param name="rootNode">The starting node. This cannot be null.</param>
        /// <returns>Enumerable of all nodes, including the starting node.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="rootNode"/> cannot be null.
        /// </exception>
        public static IEnumerable<Entity> GetAllNodes(Entity rootNode)
        {
            if (rootNode == null)
            {
                throw new ArgumentNullException("rootNode");
            }

            IEnumerable<Entity> result = new[] { rootNode };

            IEnumerable<Entity> descendents;

            var aggregateEntity = rootNode as AggregateEntity;
            if (aggregateEntity != null)
            {
                descendents = GetAllNodes(aggregateEntity.GroupedEntity);
            }
            else
            {
                descendents = rootNode.RelatedEntities != null ? rootNode.RelatedEntities.SelectMany(GetAllNodes) : null;
            }
            if (descendents != null)
            {
                result = result.Concat(descendents);
            }

            return result;
        }


        /// <summary>
        /// Finds the specified node.
        /// </summary>
        /// <param name="nodeId">The node to find.</param>
        /// <param name="rootEntity">The root of the tree.</param>
        /// <returns>The located node, or null.</returns>
        private static Entity GetNode(Guid nodeId, Entity rootEntity)
        {
            return GetAllNodes(rootEntity).FirstOrDefault(node => node.NodeId == nodeId);
        }


        /// <summary>
		///     Get relationship Ref object from the relationship type related resource object
		/// </summary>
		/// <param name="nodeId">node id</param>
		/// <param name="rootEntity">current root entity</param>
		/// <returns></returns>
		public static EntityRef GetRelationshipType( Guid nodeId, Entity rootEntity )
		{
            var node = GetNode(nodeId, rootEntity) as RelatedResource;
            return node == null ? null : node.RelationshipTypeId;
		}

        /// <summary>
        ///     Get relationship Ref object from the relationship type related resource object
        /// </summary>
        /// <param name="nodeId">node id</param>
        /// <param name="rootEntity">current root entity</param>
        /// <param name="direction">output param to return relatinshipdirection</param>
        /// <returns></returns>
        public static EntityRef GetRelationshipType(Guid nodeId, Entity rootEntity, out RelationshipDirection direction)
        {
            var node = GetNode(nodeId, rootEntity) as RelatedResource;
            if (node == null)
            {
                direction = RelationshipDirection.Forward;
                return null;
            }

            direction = node.RelationshipDirection;

            return node.RelationshipTypeId;
        }


		/// <summary>
		///     Get relationship Ref object from the relationship type related resource object
		/// </summary>
		/// <param name="nodeId">node id</param>
		/// <param name="rootEntity">current root entity</param>
		/// <returns></returns>
		public static EntityRef GetEntityType( Guid nodeId, Entity rootEntity )
		{
            var node = GetNode(nodeId, rootEntity) as ResourceEntity;
            return node == null ? null : node.EntityTypeId;
		}


		/// <summary>
		///     to check current entity type is enum type or not
		/// </summary>
		/// <param name="selectColumn">The select column.</param>
		/// <param name="entityType">Type of the entity.</param>
		/// <returns>
		///     <c>true</c> if [is enum type] [the specified select column]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsEnumType( SelectColumn selectColumn, EntityRef entityType )
		{
			bool isEnumType = false;

			var resourceExpr = selectColumn.Expression as ResourceExpression;

			if ( resourceExpr != null )
			{
				isEnumType = resourceExpr.CastType is ChoiceRelationshipType;
			}

			return isEnumType;
		}

	    /// <summary>
	    /// Recursively visit all children, including the node passed in.
	    /// </summary>
	    /// <param name="query">
	    /// The query to walk the expressions for. This cannot be null.
	    /// </param>
	    /// <param name="filteringExpressionsOnly">
	    /// If true, only walk expressions that may affect whether a row get returned - not the content of those rows.
	    /// </param>
	    /// <param name="ignoreUnusedConditions">
	    /// If true, conditions that do not have values set get skipped over.
	    /// </param>
	    /// <exception cref="ArgumentNullException">
	    /// <paramref name="query"/> cannot be null.
	    /// </exception>
	    public static IEnumerable<ScalarExpression> WalkExpressions(StructuredQuery query, bool filteringExpressionsOnly = false, bool ignoreUnusedConditions = false)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (query.RootEntity == null)
            {
                throw new ArgumentException("RootEntity cannot be null", "query");
            }
            if (query.SelectColumns == null)
            {
                throw new ArgumentException("SelectColumns cannot be null", "query");
            }
            if (query.Conditions == null)
            {
                throw new ArgumentException("Conditions cannot be null", "query");
            }
            if (query.OrderBy == null)
            {
                throw new ArgumentException("OrderBy cannot be null", "query");
            }

            var nodes = WalkNodes(query.RootEntity).ToList();

            IEnumerable<ScalarExpression> result = Enumerable.Empty<ScalarExpression>();
            if ( !filteringExpressionsOnly )
            {
                var columns = query.SelectColumns.Select( column => column.Expression );
                var orders = query.OrderBy.Select( order => order.Expression );
                result = result.Concat( columns ).Concat( orders );
            }
            var conditions = query.Conditions
                .Where(cond => !ignoreUnusedConditions || cond.Operator != ConditionType.Unspecified)
                .Where(cond => !filteringExpressionsOnly || IncludeConditionField(cond))
                .Select(cond => cond.Expression);
            var groupBy = nodes.OfType<AggregateEntity>().Where(aggNode => aggNode.GroupBy != null).SelectMany(aggNode => aggNode.GroupBy);
            var nodeConditions = nodes.Where(node => node.Conditions != null).SelectMany(node => node.Conditions);

            result = result.Concat( conditions ).Concat( groupBy ).Concat( nodeConditions );
            result = result.SelectMany( expr => WalkExpressions( expr ) );
            return result;
        }

        /// <summary>
        /// Determine if we actually care about the field that a condition expression is pointing to, based on the operation being performed on it.
        /// </summary>
        /// <param name="condition">A query condition.</param>
        /// <returns>True if the field value might be used, false if it will not be used.</returns>
	    private static bool IncludeConditionField(QueryCondition condition)
	    {
            switch (condition.Operator)
            {
                case ConditionType.AnyOf:
                case ConditionType.AnyExcept:
                case ConditionType.CurrentUser:
                    return false;
                default:
                    return true;
            }
	    }

        /// <summary>
        /// Recursively visit all children, including the node passed in.
        /// </summary>
        public static IEnumerable<ScalarExpression> WalkExpressions(ScalarExpression expr)
        {
            IEnumerable<ScalarExpression> result = new[] { expr };

            ICompoundExpression compound = expr as ICompoundExpression;
            if (compound != null)
            {
                var descendents = compound.Children.Where(child => child != null).SelectMany(WalkExpressions);
                result = result.Concat(descendents);
            }
            return result;
        }

        /// <summary>
        /// Recursively visit all children, including the node passed in.
        /// </summary>
        public static IEnumerable<Entity> WalkNodes(Entity node)
        {
            IEnumerable<Entity> result = new[] { node };

            IEnumerable<Entity> children = node.RelatedEntities.Where( child => child != null );

            AggregateEntity aggEntity = node as AggregateEntity;
            if ( aggEntity != null && aggEntity.GroupedEntity != null )
            {
                children = children.Concat( aggEntity.GroupedEntity.ToEnumerable() );
            }

            var descendents = children.SelectMany( WalkNodes );
            result = result.Concat( descendents );            

            return result;
        }

	    /// <summary>
	    /// Recursively visit all children, including the node passed in.
	    /// </summary>
	    /// <param name="node">
	    /// The node to start at (usually <see cref="StructuredQuery.RootEntity"/>). This
	    /// cannot be null.
	    /// </param>
	    /// <param name="visitor">
	    /// The action to call on each node. This cannot be null. Use LastOrDefault() to access the node's parent.
	    /// </param>
	    /// <exception cref="ArgumentNullException">
	    /// No argument can be null.
	    /// </exception>
	    public static void VisitNodes(Entity node, Action<Entity, IEnumerable<Entity>> visitor)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }

            Entity[] descendants;	        

            descendants = new Entity[0];
            visitor(node, descendants);

	        VisitNodesInternal(node, visitor, descendants);
        }

        /// <summary>
        /// Recursively visit all children, including the node passed in.
        /// </summary>
        /// <param name="childNode">
        /// The node to find the parent for.
        /// </param>
        /// <param name="rootNode">
        /// The node to start at (usually <see cref="StructuredQuery.RootEntity"/>). This
        /// cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public static Entity FindParent( Entity childNode, Entity rootNode )
        {
            if ( childNode == null )
            {
                throw new ArgumentNullException( nameof( childNode ) );
            }
            if ( rootNode == null )
            {
                throw new ArgumentNullException( nameof( rootNode ) );
            }
            Entity result = null;
            VisitNodes( rootNode, ( curNode, path ) =>
            {
                if ( curNode == childNode )
                    result = path.LastOrDefault( );
            } );
            return result;
        }

        /// <summary>
        /// Used by <see cref="VisitNodes"/> to walk all nodes.
        /// </summary>
        /// <param name="node">
        /// The node to start at (usually <see cref="StructuredQuery.RootEntity"/>). This
        /// cannot be null.
        /// </param>
        /// <param name="visitor">
        /// The action to call on each node. This cannot be null.
        /// </param>
        /// <param name="descendants">
        /// The descendants of <see cref="node"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        private static void VisitNodesInternal(Entity node, Action<Entity, IEnumerable<Entity>> visitor,
	        IEnumerable<Entity> descendants)
	    {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }
	        if (descendants == null)
	        {
	            throw new ArgumentNullException("descendants");
	        }

            IEnumerable<Entity> newDescendants;

            newDescendants = descendants.Concat(new [] { node });

	        foreach (Entity child in node.RelatedEntities.Where(entity => entity != null))
            {
                visitor(child, newDescendants);
                VisitNodesInternal(child, visitor, newDescendants);                
            }

            var aggEntity = node as AggregateEntity;
            if (aggEntity != null && aggEntity.GroupedEntity != null)
            {
                visitor(aggEntity.GroupedEntity, newDescendants);
                VisitNodesInternal(aggEntity.GroupedEntity, visitor, newDescendants); 
            }
	    }

        /// <summary>
        /// Recursively visit all children, including the node passed in.
        /// </summary>
        public static Entity FindNode(Entity root, Guid nodeId)
        {
            var result = WalkNodes(root).FirstOrDefault(node => node.NodeId == nodeId);
            return result;
        }

        /// <summary>
        /// Returns all the referenced resource types.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <param name="ignoreRootType">If true, then the type of the root node is not returned (unless it is also happens to be used elsewhere in the query). </param>
        /// <returns></returns>
        public static HashSet<EntityRef> GetReferencedResourceTypes(StructuredQuery structuredQuery, bool ignoreRootType = false)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }
            if (structuredQuery.RootEntity == null)
            {
                throw new ArgumentException("RootEntity cannot be null", "structuredQuery");
            }

            var result = new HashSet<EntityRef>();

            VisitNodes(structuredQuery.RootEntity, (node, ancestors) =>
            {
                var relatedResource = node as RelatedResource;

                if (relatedResource != null)
                {
                    if (relatedResource.EntityTypeId != null)
                    {
                        result.Add(relatedResource.EntityTypeId);
                    }
                    else
                    {
                        var relationship = Model.Entity.Get<Relationship>(relatedResource.RelationshipTypeId);
                        if (relationship != null)
                        {
                            switch (relatedResource.RelationshipDirection)
                            {
                                case RelationshipDirection.Forward:
                                    if (relationship.ToType != null)
                                    {
                                        result.Add(relationship.ToType.Id);                                          
                                    }                                    
                                    break;
                                case RelationshipDirection.Reverse:
                                    if (relationship.FromType != null)
                                    {
                                        result.Add(relationship.FromType.Id);      
                                    }                                    
                                    break;
                            }
                        }
                    }

                    return;   
                }

                var resourceEntity = node as ResourceEntity;

                if (resourceEntity != null &&
                    resourceEntity.EntityTypeId != null)
                {
                    bool ignore = ignoreRootType && node == structuredQuery.RootEntity;
                    if ( !ignore )
                    {
                        result.Add( resourceEntity.EntityTypeId );
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Returns all the referenced relationships.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <returns></returns>
        internal static IEnumerable<EntityRef> GetReferencedRelationships( StructuredQuery structuredQuery )
        {
            if ( structuredQuery == null )
            {
                throw new ArgumentNullException( "structuredQuery" );
            }
            if ( structuredQuery.RootEntity == null )
            {
                throw new ArgumentException( "RootEntity cannot be null", "structuredQuery" );
            }

            var result = new HashSet<EntityRef>( );

            VisitNodes( structuredQuery.RootEntity, ( node, ancestors ) =>
            {
                var relatedResource = node as RelatedResource;

                if ( relatedResource != null )
                {
                    result.Add( relatedResource.RelationshipTypeId );
                }
            } );

            return result;
        }

        /// <summary>
        /// Returns all the referenced fields.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <param name="filteringExpressionsOnly"></param>
        /// <param name="ignoreUnusedConditions">
        /// If true, only walk expressions that may affect whether a row get returned - not the content of those rows.
        /// </param>
        /// <returns></returns>
        internal static IEnumerable<EntityRef> GetReferencedFields( StructuredQuery structuredQuery, bool filteringExpressionsOnly = false, bool ignoreUnusedConditions = false)
        {
            if ( structuredQuery == null )
            {
                throw new ArgumentNullException( "structuredQuery" );
            }
            if ( structuredQuery.RootEntity == null )
            {
                throw new ArgumentException( "RootEntity cannot be null", "structuredQuery" );
            }

            HashSet<EntityRef> fields = new HashSet<EntityRef>( );

            foreach ( ScalarExpression expr in WalkExpressions( structuredQuery, filteringExpressionsOnly, ignoreUnusedConditions) )
            {
                ResourceDataColumn fieldExpr = expr as ResourceDataColumn;
                if ( fieldExpr != null )
                {
                    fields.Add( fieldExpr.FieldId );
                    continue;
                }

                ResourceExpression resourceExpr = expr as ResourceExpression;
                if ( resourceExpr != null && resourceExpr.OrderFieldId != null)
                {
                    fields.Add( resourceExpr.OrderFieldId ); 
                }
            }

            return fields;
        }


        /// <summary>
        /// Returns all the related resource nodes types.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <returns></returns>
        internal static List<RelatedResource> GetRelatedResourceNodes(StructuredQuery structuredQuery)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }
            if (structuredQuery.RootEntity == null)
            {
                throw new ArgumentException("RootEntity cannot be null", "structuredQuery");
            }

            var result = new List<RelatedResource>();

            VisitNodes(structuredQuery.RootEntity, (node, ancestors) =>
            {
                var relatedResource = node as RelatedResource;

                if (relatedResource != null)
                {
                    result.Add(relatedResource);
                }
            });

            return result;
        }


        /// <summary>
        /// Determine which nodes in the tree are unused, and remove them.
        /// </summary>
        /// <param name="structuredQuery">The structured query to prune.</param>
        /// <param name="mutable">If true, allow changes directly to the structured query, otherwise apply changes to a copy.</param>
        public static StructuredQuery PruneQueryTree( StructuredQuery structuredQuery, bool mutable = false )
        {
            if ( structuredQuery == null )
                throw new ArgumentNullException( "structuredQuery" );
            if ( structuredQuery.RootEntity == null )
                throw new ArgumentException( "structuredQuery" );

            var expressions = WalkExpressions(structuredQuery, false, true);

            ISet<Guid> nodeIdInUse = new HashSet<Guid>();

            // Find all nodes used by expressions                        
            foreach (var scalarExpression in expressions)
            {
                var entityExpression = scalarExpression as EntityExpression;
                if (entityExpression != null)
                {
                    nodeIdInUse.Add(entityExpression.NodeId);
                    continue;
                }

                var structureViewExpression = scalarExpression as StructureViewExpression;
                if (structureViewExpression != null)
                {
                    nodeIdInUse.Add(structureViewExpression.NodeId);                    
                }
            }            

            // Explicitly include root node
            nodeIdInUse.Add( structuredQuery.RootEntity.NodeId );

            // Map of nodes to parents
            Dictionary<Entity, Entity> nodeToParent = new Dictionary<Entity,Entity>();

            // Set of all nodes in use
            ISet<Entity> nodeInUsed = new HashSet<Entity>();

            VisitNodes( structuredQuery.RootEntity, ( node, ancestors ) =>
            {
                nodeToParent.Add(node, ancestors.LastOrDefault());
                
                if (nodeIdInUse.Contains( node.NodeId ))
                {
                    nodeInUsed.Add( node );
                    nodeInUsed.AddRange( ancestors );

                    // If an aggregate node is in use then keep all of its children
                    if ( node is AggregateEntity )
                    {
                        VisitNodes( node, ( aggChild, ancestors2 ) =>
                        {
                            nodeInUsed.Add( aggChild );                    
                        } );
                    }
                }
            });

            // Prune
            foreach ( var pair in nodeToParent )
            {
                var node = pair.Key;
                var parent = pair.Value;

                if ( nodeInUsed.Contains( node ) || parent == null )
                    continue;

                // Clone on first attempt to modify
                if ( !mutable )
                {
                    var queryClone = structuredQuery.DeepCopy( );
                    return PruneQueryTree( queryClone, true );
                }
                parent.RelatedEntities.Remove( node );                
            }

            return structuredQuery;
        }


        /// <summary>
        /// Optimises the authorisation query.
        /// </summary>
        /// <param name="authStructuredQuery">The authentication structured query.</param>
        public static void OptimiseAuthorisationQuery(StructuredQuery authStructuredQuery)
        {
            // In order to remove the columns we must ensure they they are not referenced by other query items.            
            // Find all conditions that reference columns and copy the expression from the column to the condition.

            // Find all conditions with column references
            IEnumerable<QueryCondition> qcWithColumnRefs = authStructuredQuery.Conditions.Where(qc => qc.Expression is ColumnReference);

            foreach (QueryCondition qc in qcWithColumnRefs)
            {
                var columnRef = qc.Expression as ColumnReference;
                if (columnRef == null)
                {
                    continue;
                }

                // Get the referenced column
                SelectColumn referencedColumn = authStructuredQuery.SelectColumns.FirstOrDefault(c => c.ColumnId == columnRef.ColumnId);
                if (referencedColumn == null)
                {
                    continue;
                }

                // Copy the expression from the column to the condition
                qc.Expression = referencedColumn.Expression;
            }

            if (authStructuredQuery.SelectColumns.Count() > 1)
            {
                // Remove the extra columns
                authStructuredQuery.SelectColumns.RemoveRange(1, authStructuredQuery.SelectColumns.Count() - 1);   
            }            

            // Remove any order by items
            authStructuredQuery.OrderBy.Clear();
        }

	    /// <summary>
	    /// Locate types, fields and relationships within a query, and flag that changes to them would effect the current cache context.
	    /// Mark the cache context to watch for changes that would invalidate a cached result run for this structured query.
	    /// </summary>
	    /// <param name="query"></param>
        /// <param name="filteringExpressionsOnly"></param>
        /// <param name="ignoreRootType"></param>
        public static void IdentifyResultCacheDependencies(StructuredQuery query, bool filteringExpressionsOnly = false, bool ignoreRootType = false)
        {
            if ( !CacheContext.IsSet( ) )
                return;

            // Tell the cache what entities were referenced to return
            // the StructuredQuery result
            IEnumerable<EntityRef> queryTypes;

            using ( CacheContext cacheContext = CacheContext.GetContext( ) )
            {
                // Invalidate if any instances of any of the types involved (or their derived types) are created/deleted/modified.
                queryTypes = StructuredQueryHelper.GetReferencedResourceTypes(query, ignoreRootType);

                // Calculate derived types
                // (Done in security bypass context to ensure we are watching all types, but also so that the security checks themselves don't cause further
                // unwanted cache invalidation watches).
                using ( new SecurityBypassContext( ) )
                {
                    IEnumerable<long> allDerived;
                    allDerived = queryTypes.SelectMany( type => PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( type.Id ) );

                    cacheContext.EntityTypes.Add( allDerived );
                    cacheContext.Entities.Add( allDerived ); //for the new type which inherited by existing type, add in cacheContext.Entities list.  ticket #26755
                }

            }
        }

        /// <summary>
        /// Locate types, fields and relationships within a query, and flag that changes to them would effect the current cache context.
        /// Mark the cache context to watch for changes that would invalidate a cached SQL query for this structured query.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filteringExpressionsOnly"></param>
        /// <param name="ignoreRootType"></param>        
        public static void IdentifyStructureCacheDependencies(StructuredQuery query, bool filteringExpressionsOnly = false, bool ignoreRootType = false)
        {
            if ( !CacheContext.IsSet( ) )
                return;

            // Tell the cache what entities were referenced to return
            // the StructuredQuery result
            using ( CacheContext cacheContext = CacheContext.GetContext( ) )
            {
                cacheContext.Entities.Add(StructuredQueryHelper.GetReferencedResourceTypes(query, ignoreRootType).Select(er => er.Id));
                cacheContext.Entities.Add( StructuredQueryHelper.GetReferencedRelationships( query ).Select(er => er.Id) );
                cacheContext.Entities.Add( StructuredQueryHelper.GetReferencedFields( query, filteringExpressionsOnly ).Select(er => er.Id) );
                cacheContext.FieldTypes.Add( new long[ ] { WellKnownAliases.CurrentTenant.SecuresFrom, WellKnownAliases.CurrentTenant.SecuresTo } );
            }
        }

        /// <summary>
        /// find current node's parent node path by structuredQuery node tree
        /// </summary>
        /// <param name="nodeId">the nodeId to match</param>
        /// <param name="nodeEntity">the current nodeEntity in structuredQuery node tree </param>
        /// <returns>matched parent node entity path or null</returns>
	    public static List<Entity> FindNodePath(Guid nodeId, Entity nodeEntity)
	    {
	        List<Entity> retEntities = new List<Entity>();

            if (nodeId == Guid.Empty || nodeEntity == null)
                return retEntities;

            AggregateEntity aggregateEntity = nodeEntity as AggregateEntity;
            ResourceEntity resourceEntity = (aggregateEntity != null && aggregateEntity.GroupedEntity != null)? aggregateEntity.GroupedEntity as ResourceEntity : nodeEntity as ResourceEntity;
            if (resourceEntity != null && resourceEntity.RelatedEntities != null)
            {
                //if current nodeentity's relatedEntities contain the match node, return current nodeentity
                if (resourceEntity.RelatedEntities.Any(r => r.NodeId == nodeId))
                {
                    retEntities.Add(nodeEntity);
                    return retEntities;
                }
                    
                //loop through all relatedEnties findout the match nodeentity
                List<Entity> matchedRelatedEntity = new List<Entity>();
                foreach (Entity relatedEntity in resourceEntity.RelatedEntities)
                {
                    matchedRelatedEntity.AddRange(FindNodePath(nodeId, relatedEntity));
                    if (matchedRelatedEntity.Count > 0)
                    {
                        matchedRelatedEntity.Add(relatedEntity);
                        break;
                    }
                }

                if (matchedRelatedEntity.Count > 0)
                    return matchedRelatedEntity;

                return retEntities;
            }
	        return retEntities;
	    }
	}
}