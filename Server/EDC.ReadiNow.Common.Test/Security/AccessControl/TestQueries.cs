// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ReadiNow.QueryEngine.ReportConverter;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    public static class TestQueries
    {
        /// <summary>
        /// Convert a <see cref="StructuredQuery"/> to a <see cref="Report"/>.
        /// </summary>
        /// <param name="structuredQuery">
        /// The <see cref="StructuredQuery"/> to convert. This cannot be null.
        /// </param>
        /// <returns>
        /// The converted report.
        /// </returns>
        public static Report ToReport(this StructuredQuery structuredQuery)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }

            // The structure query already has a report. Return it.
            if (structuredQuery.Report != null)
            {
                return structuredQuery.Report;
            }

            Report report = ReportToEntityModelHelper.ConvertToEntity( structuredQuery );

            // Attach the report to the structured query.
            structuredQuery.Report = report;


            return report;
        }

        public static StructuredQuery Entities(EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            Guid resourceGuid = Guid.NewGuid();
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = type,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>()
            };

            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = resourceGuid }
            });
            return structuredQuery;
        }

        public static StructuredQuery Entities(EntityType rootEntityType, IEnumerable<Relationship> relationshipsToFollow, bool relationshipsAreForward = true)
        {
            List<Guid> nodeIds = new List<Guid>();
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = rootEntityType.Id,
                    ExactType = false,
                    NodeId = Guid.NewGuid()
                },
                SelectColumns = new List<SelectColumn>()
            };

            ResourceEntity parentNode = structuredQuery.RootEntity as ResourceEntity;
            nodeIds.Add(parentNode.NodeId);

            foreach (Relationship relationship in relationshipsToFollow)
            {
                RelatedResource relatedResource = new RelatedResource
                {
                    EntityTypeId = relationship.ToType.Id,
                    NodeId = Guid.NewGuid(),
                    RelationshipDirection = relationshipsAreForward ? RelationshipDirection.Forward : RelationshipDirection.Reverse,
                    RelationshipTypeId = relationship.Id
                };

                parentNode.RelatedEntities.Add(relatedResource);
                parentNode = relatedResource;

                nodeIds.Add(relatedResource.NodeId);
            }

            foreach (Guid nodeId in nodeIds)
            {
                structuredQuery.SelectColumns.Add(new SelectColumn
                {
                    Expression = new IdExpression() { NodeId = nodeId }
                });
            }

            return structuredQuery;
        }

        public static StructuredQuery EntitiesWithNameA(EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            return EntitiesWithName(type, "A");
        }

        public static StructuredQuery EntitiesWithNameB(EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            return EntitiesWithName(type, "B");
        }

        public static StructuredQuery EntitiesWithNameC(EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            return EntitiesWithName(type, "C");
        }

        public static StructuredQuery EntitiesWithName(string name)
        {
            return EntitiesWithName(new EntityRef("core:resource"), name);
        }

        public static StructuredQuery EntitiesWithName(EntityRef type, string name)
        {
            return EntitiesWithField(type, new EntityRef("core:name"), name);
        }

        public static StructuredQuery EntitiesWithDescription(string description = "a", EntityRef type = null)
        {
            return EntitiesWithField(type, new EntityRef("core:description"), description);
        }

        public static StructuredQuery WorkflowWithName(string name)
        {
            return EntitiesWithField(Workflow.Workflow_Type, new EntityRef("core:name"), name);
        }

        /// <summary>
        /// Construct a query looking for entities of type <paramref name="type"/> with the 
        /// field <paramref name="field"/> set to <paramref name="value"/>.
        /// </summary>
        /// <param name="type">
        /// If null, use core:resource.
        /// </param>
        /// <param name="field">
        /// The field to use. This cannot be null.
        /// </param>
        /// <param name="value">
        /// The value to use. This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="StructuredQuery"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Neither <see cref="field"/> nor <see cref="value"/> can be null.
        /// </exception>
        public static StructuredQuery EntitiesWithField(EntityRef type, EntityRef field, string value)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Guid resourceGuid = Guid.NewGuid();
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = type,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>()
            };

            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(structuredQuery.RootEntity, field),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(value),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = resourceGuid }
            });

            return structuredQuery;
        }

        public static StructuredQuery EntitiesOrderByDescription( string description = "a", EntityRef type = null )
        {
            if ( type == null )
            {
                type = new EntityRef( "core:resource" );
            }

            Guid resourceGuid = Guid.NewGuid( );
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = type,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>( )
            };

            structuredQuery.OrderBy.Add( new OrderByItem
            {
                Expression = new ResourceDataColumn( structuredQuery.RootEntity, new EntityRef( "core:description" ) ),
                Direction = OrderByDirection.Ascending
            } );
            structuredQuery.SelectColumns.Add( new SelectColumn
            {
                Expression = new IdExpression( ) { NodeId = resourceGuid }
            } );
            return structuredQuery;
        }

        public static StructuredQuery EntitiesWithNameDescription(string name, string description, EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            Guid resourceGuid = Guid.NewGuid();
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = type,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>()
            };

            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(structuredQuery.RootEntity, new EntityRef("core:name")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(name),
            });
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(structuredQuery.RootEntity, new EntityRef("core:description")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(description),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = resourceGuid }
            });
            return structuredQuery;
        }

        public static StructuredQuery EntitiesWithNameAndDescriptionInResults(string name, EntityRef type = null)
        {
            if (type == null)
            {
                type = new EntityRef("core:resource");
            }

            Guid resourceGuid = Guid.NewGuid();
            StructuredQuery structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = type,
                    ExactType = false,
                    NodeId = resourceGuid
                },
                SelectColumns = new List<SelectColumn>()
            };
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(structuredQuery.RootEntity, new EntityRef("core:name")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(name),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = resourceGuid }
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                ColumnName = "Description",
                Expression = new ResourceDataColumn(structuredQuery.RootEntity, new EntityRef("core:description"))
            });
            return structuredQuery;
        }

        /// <summary>
        /// Find all enabled <see cref="AccessRule"/> entities related to the 
        /// <see cref="Permission"/> named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name to check. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public static StructuredQuery AccessRulesWithNamedPermission(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            ResourceEntity rootEntity;
            ResourceEntity relatedPermissionEntity;
            StructuredQuery structuredQuery;

            relatedPermissionEntity = new RelatedResource(new EntityRef("core:permissionAccess"))
            {
                NodeId = Guid.NewGuid()
            };
            rootEntity = new ResourceEntity
            {
                EntityTypeId = Entity.GetId("core:accessRule"),
                ExactType = false,
                NodeId = Guid.NewGuid(),
                RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>()
                {
                    relatedPermissionEntity
                }
            };

            structuredQuery = new StructuredQuery
            {
                RootEntity = rootEntity,
                SelectColumns = new List<SelectColumn>()
            };
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(relatedPermissionEntity, new EntityRef("core:name")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(name),
            });
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(rootEntity, new EntityRef("core:accessRuleEnabled")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(1),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = rootEntity.NodeId }
            });

            return structuredQuery;
        }

        /// <summary>
        /// All <see cref="UserAccount"/>s in the <see cref="Role"/> with the name
        /// <paramref name="roleName"/>.
        /// </summary>
        /// <param name="roleName">
        /// The role name. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="roleName"/> cannot be null, empty or whitespace.
        /// </exception>
        public static StructuredQuery ActiveUsersInRole(string roleName)
        {
            ResourceEntity rootEntity;
            ResourceEntity relatedRoleEntity;
            ResourceEntity activeStatusEntity;
            StructuredQuery structuredQuery;

            activeStatusEntity = new RelatedResource(new EntityRef("core:accountStatus"))
            {
                NodeId = Guid.NewGuid()
            };
            relatedRoleEntity = new RelatedResource(new EntityRef("core:userHasRole"))
            {
                NodeId = Guid.NewGuid(),
                Recursive = RecursionMode.RecursiveWithSelf
            };
            rootEntity = new ResourceEntity
            {
                EntityTypeId = Entity.GetId("core:userAccount"),
                ExactType = false,
                NodeId = Guid.NewGuid(),
                RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>()
                {
                    relatedRoleEntity,
                    activeStatusEntity
                }
            };

            structuredQuery = new StructuredQuery
            {
                RootEntity = rootEntity,
                SelectColumns = new List<SelectColumn>()
            };
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(relatedRoleEntity, new EntityRef("core:name")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(roleName),
            });
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(activeStatusEntity, new EntityRef("core:alias")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue("active"),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = rootEntity.NodeId }
            });

            return structuredQuery;
        }

        /// <summary>
        /// All <see cref="EntityType"/>s that have access rules/security queries
        /// from <see cref="Subject"/>s with the name <see cref="subjectName"/>.
        /// </summary>
        /// <param name="subjectName">
        /// The role name. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// The query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="subjectName"/> cannot be null, empty or whitespace.
        /// </exception>
        public static StructuredQuery TypesSecuredBySubjects(string subjectName)
        {
            ResourceEntity rootEntity;
            ResourceEntity allowAccessEntity;
            ResourceEntity controlAccessEntity;
            StructuredQuery structuredQuery;

            allowAccessEntity = new RelatedResource(new EntityRef("core:allowAccess"), RelationshipDirection.Reverse)
            {
                NodeId = Guid.NewGuid()
            };
            controlAccessEntity = new RelatedResource(new EntityRef("core:controlAccess"), RelationshipDirection.Reverse)
            {
                NodeId = Guid.NewGuid(),
                RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>()
                {
                    allowAccessEntity
                }
            };
            rootEntity = new ResourceEntity
            {
                EntityTypeId = Entity.GetId("core:type"),
                ExactType = false,
                NodeId = Guid.NewGuid(),
                RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>()
                {
                    controlAccessEntity
                }
            };

            structuredQuery = new StructuredQuery
            {
                RootEntity = rootEntity,
                SelectColumns = new List<SelectColumn>()
            };
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(allowAccessEntity, new EntityRef("core:name")),
                Operator = ConditionType.Equal,
                Argument = new TypedValue(subjectName),
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = rootEntity.NodeId }
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new ResourceDataColumn(rootEntity, new EntityRef("core:name"))
            });

            return structuredQuery;
        }

        /// <summary>
        /// Entities of <paramref name="fromType"/> related to entities of a second type
        /// by <paramref name="relationship"/>. The destination (second) entity must have
        /// a non-null name.
        /// </summary>
        /// <param name="fromType">
        /// The source or from type. This cannot be null.
        /// </param>
        /// <param name="relationship">
        /// The relationship from <paramref name="fromType"/>. This cannot be null and 
        /// its <see cref="Relationship.FromType"/> must be <paramref name="fromType"/>.
        /// </param>
        /// <returns>
        /// The query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="relationship"/> must have <paramref name="fromType"/> as 
        /// its <see cref="Relationship.FromType"/>.
        /// </exception>
        public static StructuredQuery ToNamed(EntityType fromType, Relationship relationship)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }
            if (relationship == null)
            {
                throw new ArgumentNullException("relationship");
            }
            if (!EntityRefComparer.Instance.Equals(relationship.FromType, fromType))
            {
                throw new ArgumentException("Not a relationship from fromType", "relationship");
            }

            ResourceEntity rootEntity;
            ResourceEntity relatedType;
            StructuredQuery structuredQuery;

            relatedType = new RelatedResource(new EntityRef(relationship))
            {
                NodeId = Guid.NewGuid()
            };
            rootEntity = new ResourceEntity
            {
                EntityTypeId = fromType,
                ExactType = false,
                NodeId = Guid.NewGuid(),
                RelatedEntities = new List<ReadiNow.Metadata.Query.Structured.Entity>()
                {
                    relatedType
                }
            };

            structuredQuery = new StructuredQuery
            {
                RootEntity = rootEntity,
                SelectColumns = new List<SelectColumn>()
            };
            structuredQuery.Conditions.Add(new QueryCondition
            {
                Expression = new ResourceDataColumn(relatedType, new EntityRef("core:name")),
                Operator = ConditionType.IsNotNull
            });
            structuredQuery.SelectColumns.Add(new SelectColumn
            {
                Expression = new IdExpression() { NodeId = rootEntity.NodeId }
            });

            return structuredQuery;
        }


        /// <summary>
        /// Is the given report created from the given structured query?
        /// </summary>
        /// <param name="query">
        /// The <see cref="StructuredQuery"/> to check.
        /// </param>
        /// <param name="report">
        /// The <see cref="Report"/> to check.
        /// </param>
        /// <returns>
        /// True if they are effectively equal, false otherwise.
        /// </returns>
        public static bool IsCreatedFrom(StructuredQuery query, Report report)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            StructuredQuery reportQuery;

            reportQuery = ReportToQueryConverter.Instance.Convert( report );

            return new StructuredQueryEqualityComparer().Equals(query, reportQuery);
        }
    }
}
