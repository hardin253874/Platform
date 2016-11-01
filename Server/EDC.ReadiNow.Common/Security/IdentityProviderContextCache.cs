// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Data;
using EDC.Database;
using EDC.Globalization;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Class IdentityProviderContextCache.
    /// </summary>
    internal class IdentityProviderContextCache : GenericCacheService<IdentityProviderContextCacheKey, IdentityProviderContextCacheValue>, IIdentityProviderRequestContextCache
    {        
        private readonly ConcurrentDictionary<long, QueryBuild> _queryNonSystem = new ConcurrentDictionary<long, QueryBuild>();
        private readonly ConcurrentDictionary<long, QueryBuild> _querySystem = new ConcurrentDictionary<long, QueryBuild>();        

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdentityProviderContextCache" /> class.
        /// </summary>
        public IdentityProviderContextCache() : base("IdentityProviderContextCache", new CacheFactory
        {
            BlockIfPending = true,
            CacheName = "IdentityProviderContextCache",
            Distributed = true,
            MaxCacheEntries = CacheFactory.DefaultMaximumCacheSize,
            IsolateTenants = false
        })
        {            
        }

        /// <summary>
        ///     Gets the request context data.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="identityProviderId"></param>
        /// <param name="identityProviderUserName"></param>
        /// <param name="ensureAccountIsActive"></param>
        /// <returns>RequestContextData.</returns>
        public RequestContextData GetRequestContextData(long tenantId, long identityProviderId, string identityProviderUserName, bool ensureAccountIsActive)
        {
            if (tenantId <= 0 || identityProviderId <= 0 || string.IsNullOrWhiteSpace(identityProviderUserName))
            {
                return null;
            }

            var key = new IdentityProviderContextCacheKey(tenantId, identityProviderId, identityProviderUserName);
            IdentityProviderContextCacheValue cacheValue;                        

            TryGetOrAdd(key, out cacheValue, GetRequestContextDataFactory);
            
            if (cacheValue == null)
            {
                // Prevent the caching of nulls
                Cache.Remove(key);
            }

            if (ensureAccountIsActive)
            {
                if (cacheValue == null)
                {
                    // Unknown account status, return null
                    return null;
                }

                using (new TenantAdministratorContext(tenantId))
                {
                    if (WellKnownAliases.CurrentTenant.ActiveAccountStatus != cacheValue.AccountStatus)
                    {
                        // The account is not valid return null 
                        return null;
                    }
                }                               
            }

            if (cacheValue?.RequestContextData != null)
            {
                // change the culture to the current one just in case someone has switched cultures.
                cacheValue.RequestContextData.Culture = CultureHelper.GetUiThreadCulture(CultureType.Specific);
            }

            return cacheValue?.RequestContextData;
        }

        /// <summary>
        ///     Builds the structured query for a non system identity provider.
        /// </summary>
        /// <returns>StructuredQuery.</returns>
        private StructuredQuery BuildStructuredQueryForNonSystemIdentityProvider()
        {
            var query = new StructuredQuery();

            // User account root entity
            var rootEntity = new ResourceEntity
            {
                EntityTypeId = new EntityRef("core:userAccount")
            };
            query.RootEntity = rootEntity;

            // Account status
            var userAccountStatus = new RelatedResource
            {
                RelationshipTypeId = new EntityRef("core:accountStatus"),
                RelationshipDirection = RelationshipDirection.Forward                
            };
            rootEntity.RelatedEntities.Add(userAccountStatus);

            // Identity provider user associated with user account
            var identityProviderUser = new RelatedResource
            {
                RelationshipTypeId = new EntityRef("core:associatedUserAccount"),
                RelationshipDirection = RelationshipDirection.Reverse,
                ResourceMustExist = true
            };
            rootEntity.RelatedEntities.Add(identityProviderUser);

            // Identity provider associated with identity provider user
            var identityProvider = new RelatedResource
            {
                RelationshipTypeId = new EntityRef("core:identityProviderForUser"),
                RelationshipDirection = RelationshipDirection.Forward,
                ResourceMustExist = true
            };
            identityProviderUser.RelatedEntities.Add(identityProvider);

            // Identity provider type
            var identityProviderType = new RelatedResource
            {
                RelationshipTypeId = new EntityRef("core:isOfType"),
                RelationshipDirection = RelationshipDirection.Forward,
                ResourceMustExist = true
            };
            identityProvider.RelatedEntities.Add(identityProviderType);

            // User account id column
            var userAccountIdColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = rootEntity.NodeId
                }
            };
            query.SelectColumns.Add(userAccountIdColumn);

            // User account name column
            var userAccountNameColumn = new SelectColumn
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = rootEntity.NodeId,
                    FieldId = new EntityRef("core:name")
                }
            };
            query.SelectColumns.Add(userAccountNameColumn);

            // Identity provider id
            var identityProviderIdColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = identityProvider.NodeId
                }
            };
            query.SelectColumns.Add(identityProviderIdColumn);

            // Identity provider user id
            var identityProviderUserIdColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = identityProviderUser.NodeId
                }
            };
            query.SelectColumns.Add(identityProviderUserIdColumn);

            // Identity provider type
            var identityProviderTypeAliasColumn = new SelectColumn
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = identityProviderType.NodeId,
                    FieldId = new EntityRef("core:alias")
                }
            };
            query.SelectColumns.Add(identityProviderTypeAliasColumn);

            // Account status column
            var userAccountStatusColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = userAccountStatus.NodeId
                }                
            };
            query.SelectColumns.Add(userAccountStatusColumn);

            // Identity provider user name condition
            var identityProviderUserNameCondition = new QueryCondition
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = identityProviderUser.NodeId,
                    FieldId = new EntityRef("core:name")
                },
                Operator = ConditionType.Equal,
                Parameter = "@identityProviderUser"
            };
            query.Conditions.Add(identityProviderUserNameCondition);

            // Identity provider condition
            var identityProviderCondition = new QueryCondition
            {
                Expression = new IdExpression
                {
                    NodeId = identityProvider.NodeId
                },
                Operator = ConditionType.Equal,
                Parameter = "@identityProviderId"
            };
            query.Conditions.Add(identityProviderCondition);            

            return query;
        }

        /// <summary>
        ///     Builds the structured query for system identity provider.
        /// </summary>
        /// <returns>StructuredQuery.</returns>
        private StructuredQuery BuildStructuredQueryForSystemIdentityProvider()
        {
            var query = new StructuredQuery();

            // User account root entity
            var rootEntity = new ResourceEntity
            {
                EntityTypeId = new EntityRef("core:userAccount")
            };
            query.RootEntity = rootEntity;

            // Account status
            var userAccountStatus = new RelatedResource
            {
                RelationshipTypeId = new EntityRef("core:accountStatus"),
                RelationshipDirection = RelationshipDirection.Forward                                
            };
            rootEntity.RelatedEntities.Add(userAccountStatus);

            // User account id column
            var userAccountIdColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = rootEntity.NodeId
                }
            };
            query.SelectColumns.Add(userAccountIdColumn);

            // User account name column
            var userAccountNameColumn = new SelectColumn
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = rootEntity.NodeId,
                    FieldId = new EntityRef("core:name")
                }
            };
            query.SelectColumns.Add(userAccountNameColumn);

            // Account status column
            var userAccountStatusColumn = new SelectColumn
            {
                Expression = new IdExpression
                {
                    NodeId = userAccountStatus.NodeId
                }
            };
            query.SelectColumns.Add(userAccountStatusColumn);

            // Identity provider user name condition
            var identityProviderUserNameCondition = new QueryCondition
            {
                Expression = new ResourceDataColumn
                {
                    NodeId = rootEntity.NodeId,
                    FieldId = new EntityRef("core:name")
                },
                Operator = ConditionType.Equal,
                Parameter = "@identityProviderUser"
            };
            query.Conditions.Add(identityProviderUserNameCondition);            

            return query;
        }

        /// <summary>
        ///     Gets the request context data factory.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>RequestContextData.</returns>
        private IdentityProviderContextCacheValue GetRequestContextDataFactory(IdentityProviderContextCacheKey key)
        {
            using (new SecurityBypassContext())
            using (new TenantAdministratorContext(key.TenantId))
            {
                try
                {
                    return GetRequestContextDataFactoryImpl(key);
                }
                catch (Exception exception)
                {
                    EventLog.Application.WriteError("An error occurred getting the request context for tenant:{1}, identity provider:{2}, user:{3}, Error: {0}", exception, key.TenantId, key.IdentityProviderId, key.IdentityProviderId, key.IdentityProviderUserName);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        /// <returns>QueryResult.</returns>
        private QueryBuild GetSql(bool isSystem)
        {
            ConcurrentDictionary<long, QueryBuild> queryDictionary = isSystem ? _querySystem : _queryNonSystem;

            return queryDictionary.GetOrAdd(RequestContext.TenantId, q =>
            {
                var settings = new QuerySettings
                {
                    SecureQuery = false,
                    SupportPaging = false,
                    Hint = "resolve idp user " + (isSystem ? "system" : "non system")
                };

                StructuredQuery query = isSystem ? BuildStructuredQueryForSystemIdentityProvider() : BuildStructuredQueryForNonSystemIdentityProvider();

                var queryResult = Factory.NonCachedQuerySqlBuilder.BuildSql(query, settings);

                return queryResult;
            });
        }

        /// <summary>
        ///     Gets the request context data factory implementation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>RequestContextData.</returns>
        private IdentityProviderContextCacheValue GetRequestContextDataFactoryImpl(IdentityProviderContextCacheKey key)
        {
            IdentityProviderContextCacheValue cacheValue;

            var isSystem = key.IdentityProviderId == WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance;

            QueryBuild queryResult = GetSql(isSystem);

            using (var dbContext = DatabaseContext.GetContext())
            {
                using (var command = dbContext.CreateCommand(queryResult.Sql))
                {
                    command.AddParameterWithValue("@identityProviderUser", key.IdentityProviderUserName, 500);                    

                    if (!isSystem)
                    {
                        command.AddParameterWithValue("@identityProviderId", key.IdentityProviderId);
                    }
                    dbContext.AddParameter(command, "@tenant", DbType.Int64, key.TenantId);

                    if (queryResult.SharedParameters != null)
                    {
                        foreach (var parameter in queryResult.SharedParameters)
                        {
                            dbContext.AddParameter(command, parameter.Value, parameter.Key.Type, parameter.Key.Value);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        var userAccountId = reader.GetInt64(0);
                        var userAccountName = reader.GetString(1);
                        var identityProviderId = isSystem ? key.IdentityProviderId : reader.GetInt64(2);
                        var identityProviderUserId = isSystem ? userAccountId : reader.GetInt64(3);
                        var identityProviderTypeAlias = isSystem ? "core:readiNowIdentityProvider" : "core:" + reader.GetString(4);

                        int accountStatusColumnIndex = isSystem ? 2 : 5;
                        long accountStatusId = -1;

                        if (!reader.IsDBNull(accountStatusColumnIndex))
                        {
                            accountStatusId = reader.GetInt64(accountStatusColumnIndex);
                        }                                             

                        var identityInfo = new IdentityInfo(userAccountId, userAccountName)
                        {
                            IdentityProviderId = identityProviderId,
                            IdentityProviderUserId = identityProviderUserId,
                            IdentityProviderTypeAlias = identityProviderTypeAlias
                        };
                        var tenantInfo = new TenantInfo(key.TenantId);

                        var contextData = new RequestContextData(identityInfo, tenantInfo,
                            CultureHelper.GetUiThreadCulture(CultureType.Specific));

                        cacheValue = new IdentityProviderContextCacheValue(contextData, accountStatusId);

                        if (CacheContext.IsSet())
                        {
                            using (CacheContext cacheContext = CacheContext.GetContext())
                            {
                                cacheContext.Entities.Add(userAccountId);
                                cacheContext.Entities.Add(identityProviderId);
                                if (identityProviderUserId != userAccountId)
                                {
                                    cacheContext.Entities.Add(identityProviderUserId);
                                }                                
                            }
                        }
                    }
                }
            }
            
            return cacheValue;
        }
    }
}