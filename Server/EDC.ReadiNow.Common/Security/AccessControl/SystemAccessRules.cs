// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Annotations;
using EDC.Database;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;

namespace EDC.ReadiNow.Security.AccessControl
{
    internal static class SystemAccessRules
    {
        /// <summary>
        /// This effectively will disable out of the box protection. Using a feature switch to turn it on
        /// </summary>
        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", new[] { "core:create", "core:read", "core:modify", "core:delete" }, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneAllFeatureSwitched =
        {
            "core:resource"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] { "core:create", "core:read", "core:modify", "core:delete" }, nameof(GetNavigationElementsExceptConsolePagesQuery))]
        internal static readonly string[] AdministratorFullAccessNavigationElementTypes = 
        {
            "console:navigationElement"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] {"core:create", "core:read", "core:modify", "core:delete"}, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] AdministratorFullAccessTypes =
        {            
            "core:accessRule",
            "console:actionMenu",
            "console:actionMenuItem",
            "core:activityArgument",
            "core:api",
            "core:apiEndpoint",
            "core:apiFieldMapping",
            "core:apiKey",
            "core:apiMemberMapping",
            "core:apiRelationshipMapping",
            "core:apiResourceEndpoint",
            "core:apiResourceMapping",
            "core:argumentValue",
            "console:consoleBehavior",
            "console:consoleTheme",
            "console:controlOnForm",
            "core:displayFormat",
            "core:documentFolder",
            "core:documentType",            
            "core:exitPoint",            
            "core:importConfig",
            "core:inbox",
            "core:inboxEmailAction",            
            "core:reportColumn",
            "core:reportCondition",
            "core:reportExpression",
            "core:reportNode",
            "core:reportOrderBy",
            "core:reportRollup",
            "core:reportRowGroup",
            "core:reportTemplate",
            "core:schedule",
            "core:scheduledImportConfig",
            "core:solution",
            "core:structureLevel",
            "core:structureView",
            "core:swimlane",
            "core:transitionStart",
            "core:userResource",            
            "core:wfArgumentInstance",
            "core:wfExpression",
            "core:wfTrigger",
            "console:workflowActionMenuItem"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] { "core:modify", "core:delete" }, nameof(GetModifiableProtectableEntities))]        
        internal static readonly string[] AdministratorCanModifyProtectableTypes = {
            "core:protectableType"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] { "core:modify", "core:delete" }, new[] {
            nameof(GetEntitiesInNoApplicationQuery),
            nameof(GetEntitiesInModifiableDirectApplicationQuery),
            nameof(GetEntitiesInModifiableIndirectApplicationQuery)})]        
        [AccessRuleQueryDefinition("core:administratorRole", new[] { "core:create", "core:read" }, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] AdministratorInModifiableSolutionTypes =
        {
            "core:argumentType",
            "core:definition",
            "core:entityWithArgsAndExits",
            "core:enumType",
            "core:enumValue",
            "console:fieldRenderControlType",
            "core:field",
            "core:fieldGroup",
            "core:relationship",
            "core:resourceKey",
            "core:resourceKeyRelationship",
            "core:wfActivity",
            "core:workflow"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", "core:read", nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] AdministratorReadAccessTypes =
        {
            "core:auditLogEntry",
            "console:fieldTypeDisplayName",
            "console:navigationConfigButton",
            "core:parameter",
            "core:permission",
            "core:securityCustomiseUIPageType",
            "core:securityQueriesPageType",
            "core:stringPattern",
            "core:tenantLogEntry",
            "core:type",
            "console:uiContext"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] {"core:create", "core:read", "core:modify"}, nameof(GetAllInstancesIncludingDerived))]
        [AccessRuleQueryDefinition("core:administratorRole", "core:delete", nameof(GetSubjectsExceptSystemRolesQuery))]
        internal static readonly string[] AdministratorSubjectType =
        {
            "core:subject"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:administratorRole", new[] {"core:read", "core:modify"}, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] AdministratorReadModifyAccessTypes =
        {
            "core:auditLogSettings",
            "core:eventLogSettings",
            "core:importRun",
            "core:inboxProvider",
            "core:passwordPolicy",
            "core:tenantEmailSetting",
            "core:tenantGeneralSettings",
            "core:tenantImageSettings",
            "core:workflowRun"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", "core:read", nameof(GetAllInstancesIncludingDerived), true)]
        internal static readonly string[] EveryoneReadAccessIgnoreForReportsTypes =
        {            
            "core:type"            
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", "core:read", nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneReadAccessTypes =
        {
            "core:choiceOptionSet",
            "core:conditionalFormatIcon",
            "core:definition",
            "core:documentFolder",
            "core:enumType",
            "core:enumValue",
            "core:field",
            "core:iconFileType",
            "core:managedObjectLogEntry",
            "console:navigationElement",
            "core:person",
            "core:questionCategory",
            "core:questionLibrary",
            "core:relationship",
            "core:tenantEmailSetting",
            "core:tenantGeneralSettings",
            "core:tenantImageSettings",
            "core:solution",            
            "core:userAccount",
            "core:wfExpression",
            "core:workflowRun"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", new[] { "core:read", "core:modify" }, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneModifyReadAccessTypes =
        {
            "core:baseUserTask",
            "core:documentRevision"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", new[] { "core:create", "core:read", "core:modify" }, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneCreateReadModifyAccessTypes =
        {
            "core:board",
            "core:boardDimension"
        };

        [UsedImplicitly]        
        [AccessRuleQueryDefinition("core:everyoneRole", new[] { "core:modify", "core:delete" }, new[] {
            nameof(GetEntitiesInNoApplicationQuery),
            nameof(GetEntitiesInModifiableDirectApplicationQuery),
            nameof(GetEntitiesInModifiableIndirectApplicationQuery),
            nameof(GetModifiableProtectableEntities)})]
        [AccessRuleQueryDefinition("core:everyoneRole", new[] { "core:create", "core:read" }, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneInModifiableSolutionTypes =
        {            
            "core:workflow"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", new[] {"core:create", "core:read", "core:modify", "core:delete"}, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] EveryoneFullAccessTypes =
        {
            "console:customEditForm",
            "core:documentType",
            "core:fileType",
            "core:imageFileType",
            "core:privatelyOwnable",
            "core:reportTemplate",
            "core:userResource"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:everyoneRole", "core:read", new[] { nameof(GetResourcesByRoleQuery), nameof(GetResourcesByAssignedTaskQuery)}, true)]        
        internal static readonly string[] EveryoneResourceType =
        {
            "core:resource"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:systemAdministrator", new[] {"core:create", "core:read", "core:modify", "core:delete"}, nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] SystemAdministrator =
        {
            "core:resource"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:selfServeRole", "core:read", nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] SelfServerRead =
        {            
            "console:navigationConfigButton",
            "console:uiContext"
        };

        [UsedImplicitly]
        [AccessRuleQueryDefinition("core:selfServeRole", "core:create", nameof(GetAllInstancesIncludingDerived))]
        internal static readonly string[] SelfServerCreate =
        {
            "console:selfServeComponent"
        };

        /// <summary>
        ///     Returns a structured query that returns all instances of the specified type that are not in a solution.
        ///     i.e. not via inSolution and indirectInSolution.
        /// </summary>
        /// <param name="typeAlias"></param>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetEntitiesInNoApplicationQuery(string typeAlias)
        {
            var rootNodeId = Guid.NewGuid();
            var applicationNodeId = Guid.NewGuid();
            var applicationIndirectNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId,
                    RelatedEntities = new List<Entity>
                    {
                        new RelatedResource
                        {
                            RelationshipTypeId = new EntityRef("core:inSolution"),
                            RelationshipDirection = RelationshipDirection.Forward,
                            NodeId = applicationNodeId
                        },
                        new RelatedResource
                        {
                            RelationshipTypeId = new EntityRef("core:indirectInSolution"),
                            RelationshipDirection = RelationshipDirection.Forward,
                            NodeId = applicationIndirectNodeId
                        }
                    }
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = applicationNodeId
                        },
                        Operator = ConditionType.IsNull
                    },
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = applicationIndirectNodeId
                        },
                        Operator = ConditionType.IsNull
                    }
                }
            };

            return structuredQuery;
        }

        /// <summary>
        /// Gets the entities in modifiable direct application query.
        /// </summary>
        /// <param name="typeAlias">The type alias.</param>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetEntitiesInModifiableDirectApplicationQuery(string typeAlias)
        {
            return GetEntitiesInModifiableApplicationQuery(typeAlias, "core:inSolution");
        }

        /// <summary>
        /// Gets the entities in modifiable indirect application query.
        /// </summary>
        /// <param name="typeAlias">The type alias.</param>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetEntitiesInModifiableIndirectApplicationQuery(string typeAlias)
        {
            return GetEntitiesInModifiableApplicationQuery(typeAlias, "core:indirectInSolution");
        }

        /// <summary>
        ///     Returns a structured query that returns all instances of the specified type that are in a modifiable solution.
        ///     This includes instances that are in a solution abd the can modify application field set to true.
        /// </summary>
        /// <param name="typeAlias"></param>
        /// <param name="inSolutionRelationship"></param>
        /// <returns></returns>        
        internal static StructuredQuery GetEntitiesInModifiableApplicationQuery(string typeAlias, string inSolutionRelationship)
        {
            var rootNodeId = Guid.NewGuid();
            var applicationNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId,
                    RelatedEntities = new List<Entity>
                    {
                        new RelatedResource
                        {
                            RelationshipTypeId = new EntityRef(inSolutionRelationship),
                            RelationshipDirection = RelationshipDirection.Forward,
                            NodeId = applicationNodeId
                        }
                    }
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new ResourceDataColumn
                        {
                            NodeId = applicationNodeId,
                            FieldId = new EntityRef("core:canModifyApplication")
                        },
                        Operator = ConditionType.IsTrue
                    }
                }
            };

            return structuredQuery;
        }

        /// <summary>
        ///     Returns a structured query that returns all subjects except the system roles.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetSubjectsExceptSystemRolesQuery(string subjectAlias)
        {
            var rootNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(subjectAlias),
                    NodeId = rootNodeId
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = rootNodeId                            
                        },                        
                        Arguments = new List<TypedValue>
                        {
                            new TypedValue {Type = DatabaseType.InlineRelationshipType, Value = "core:administratorRole"},
                            new TypedValue {Type = DatabaseType.InlineRelationshipType, Value = "core:everyoneRole"},
                            new TypedValue {Type = DatabaseType.InlineRelationshipType, Value = "core:selfServeRole"},
                            new TypedValue {Type = DatabaseType.InlineRelationshipType, Value = "core:importExportRole"}
                        },
                        Operator = ConditionType.AnyExcept
                    }
                }
            };

            return structuredQuery;
        }

        /// <summary>
        ///     Returns a structured query that returns all instances of the specified type.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetAllInstancesIncludingDerived(string typeAlias)
        {
            var rootNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                }
            };

            return structuredQuery;
        }

        /// <summary>
        /// </summary>
        /// <param name="typeAlias"></param>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetResourcesByRoleQuery(string typeAlias)
        {
            var rootNodeId = Guid.NewGuid();
            var userNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId,
                    RelatedEntities = new List<Entity>
                    {
                        new RelatedResource
                        {
                            RelationshipTypeId = new EntityRef("core:allowedAccessInternalBy"),
                            RelationshipDirection = RelationshipDirection.Reverse,
                            RelatedEntities = new List<Entity>
                            {
                                new DownCastResource
                                {
                                    EntityTypeId = new EntityRef("core:role"),
                                    RelatedEntities = new List<Entity>
                                    {
                                        new RelatedResource
                                        {
                                            Recursive = RecursionMode.RecursiveWithSelf,
                                            RelationshipDirection = RelationshipDirection.Forward,
                                            RelationshipTypeId = new EntityRef("core:includesRoles"),
                                            RelatedEntities = new List<Entity>
                                            {
                                                new RelatedResource
                                                {
                                                    NodeId = userNodeId,
                                                    RelationshipDirection = RelationshipDirection.Reverse,
                                                    RelationshipTypeId = new EntityRef("core:userHasRole")
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = userNodeId
                        },
                        Operator = ConditionType.CurrentUser
                    }
                }
            };

            return structuredQuery;
        }

        /// <summary>
        /// </summary>
        /// <param name="typeAlias"></param>
        /// <returns></returns>
        [UsedImplicitly]
        internal static StructuredQuery GetResourcesByAssignedTaskQuery(string typeAlias)
        {
            var rootNodeId = Guid.NewGuid();
            var personNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId,
                    RelatedEntities = new List<Entity>
                    {
                        new RelatedResource
                        {
                            RelationshipTypeId = new EntityRef("core:tasksForRecord"),
                            RelationshipDirection = RelationshipDirection.Reverse,
                            RelatedEntities = new List<Entity>
                            {
                                new RelatedResource
                                {
                                    NodeId = personNodeId,
                                    RelationshipTypeId = new EntityRef("core:taskForUser"),
                                    RelationshipDirection = RelationshipDirection.Forward
                                }
                            }
                        }
                    }
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = personNodeId
                        },
                        Operator = ConditionType.CurrentUser
                    }
                }
            };

            return structuredQuery;
        }


        /// <summary>
        ///     Returns a structured query that returns all instances of the specified type that are modifiable.
        /// </summary>
        /// <param name="typeAlias"></param>
        /// <returns></returns>        
        internal static StructuredQuery GetModifiableProtectableEntities(string typeAlias)
        {
            var rootNodeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(typeAlias),
                    NodeId = rootNodeId                   
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new ResourceDataColumn
                        {
                            NodeId = rootNodeId,
                            FieldId = new EntityRef("core:canModifyProtectedResource")
                        },
                        Operator = ConditionType.IsTrue
                    }
                }
            };

            return structuredQuery;
        }


        [UsedImplicitly]
        internal static StructuredQuery GetNavigationElementsExceptConsolePagesQuery(string navigationElementAlias)
        {
            var rootNodeId = Guid.NewGuid();
            var isOfTypeId = Guid.NewGuid();

            var structuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    EntityTypeId = new EntityRef(navigationElementAlias),
                    NodeId = rootNodeId,
                    RelatedEntities = new List<Entity>
                    {
                        new RelatedResource
                        {
                            NodeId = isOfTypeId,
                            RelationshipTypeId = new EntityRef("core:isOfType"),
                            RelationshipDirection = RelationshipDirection.Forward
                        }
                    }
                },
                SelectColumns = new List<SelectColumn>
                {
                    new SelectColumn
                    {
                        Expression = new IdExpression {NodeId = rootNodeId}
                    }
                },
                Conditions = new List<QueryCondition>
                {
                    new QueryCondition
                    {
                        Expression = new IdExpression
                        {
                            NodeId = isOfTypeId
                        },
                        Arguments = new List<TypedValue>
                        {
                            new TypedValue {Type = DatabaseType.InlineRelationshipType, Value = "console:staticPage"}
                        },
                        Operator = ConditionType.AnyExcept
                    }
                }
            };

            return structuredQuery;
        }
    }
}
