// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    ///     Identity provider repository.
    /// </summary>
    public class IdentityProviderRepository
    {
        /// <summary>
        ///     Gets the identity providers for the specified tenant.
        /// </summary>
        /// <returns>TenantIdentityProviderResponse.</returns>
        public TenantIdentityProviderResponse GetIdentityProviders(string tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            var response = new TenantIdentityProviderResponse {IdentityProviders = new List<IdentityProviderResponse>()};

            using (new SecurityBypassContext())
            {
                TenantAdministratorContext tenantAdminContext;

                try
                {
                    tenantAdminContext = new TenantAdministratorContext(tenant);
                }
                catch (EntityNotFoundException)
                {
                    return response;
                }

                try
                {
                    var idProviderId = new EntityRef("core:identityProvider");
                    var isOfTypeId = new EntityRef("core:isOfType");
                    var nameId = new EntityRef("core:name");
                    var aliasId = new EntityRef("core:alias");
                    var isEnabledId = new EntityRef("core:isProviderEnabled");
                    var ordinalId = new EntityRef("core:providerOrdinal");

                    // Get the identity providers for the given tenant            
                    var entityRequest = new EntityRequest
                    {
                        Entities = idProviderId.ToEnumerable(),
                        RequestString = "name, isProviderEnabled, providerOrdinal, isOfType.alias",
                        Hint = "tenant ip providers",
                        QueryType = QueryType.Instances
                    };

                    // Run the query
                    var instances = BulkRequestRunner.GetEntitiesByType(entityRequest);

                    if (instances == null)
                    {
                        return response;
                    }

                    foreach (var instance in instances)
                    {
                        var isEnabled = false;
                        string name = null;
                        var ordinal = 0;
                        var id = instance.Id.Id;

                        foreach (var fieldData in instance.Fields)
                        {
                            if (fieldData.FieldId == null || fieldData.Value?.Type == null)
                            {
                                continue;
                            }

                            if (fieldData.FieldId.Id == nameId.Id)
                            {
                                name = fieldData.Value.ValueString;
                            }
                            else if (fieldData.FieldId.Id == ordinalId.Id &&
                                     fieldData.Value.Type.GetRunTimeType() == typeof(int) &&
                                     fieldData.Value.Value != null)
                            {
                                ordinal = (int) fieldData.Value.Value;
                            }
                            else if (fieldData.FieldId.Id == isEnabledId.Id &&
                                     fieldData.Value.Type.GetRunTimeType() == typeof(bool) &&
                                     fieldData.Value.Value != null)
                            {
                                isEnabled = (bool) fieldData.Value.Value;
                            }
                        }

                        if (!isEnabled || string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        var singleOrDefault =
                            instance.Relationships.SingleOrDefault(r => r.RelationshipTypeId.Id == isOfTypeId.Id);

                        if (singleOrDefault == null)
                        {
                            continue;
                        }

                        var isOfTypeEntityData = singleOrDefault.Entities;

                        var identityProvider = new IdentityProviderResponse
                        {
                            Id = id,
                            Name = name,
                            Ordinal = ordinal
                        };

                        if (isOfTypeEntityData != null)
                        {
                            var typeEntity = isOfTypeEntityData.FirstOrDefault();
                            if (typeEntity == null)
                            {
                                continue;
                            }

                            var typeAliasData = typeEntity.Fields.SingleOrDefault(f => f.FieldId.Id == aliasId.Id);

                            if (typeAliasData?.Value != null)
                            {
                                identityProvider.TypeAlias = typeAliasData.Value.ValueString;
                            }
                        }

                        response.IdentityProviders.Add(identityProvider);
                    }
                }
                finally
                {
                    tenantAdminContext.Dispose();
                }
            }

            return response;
        }
    }
}