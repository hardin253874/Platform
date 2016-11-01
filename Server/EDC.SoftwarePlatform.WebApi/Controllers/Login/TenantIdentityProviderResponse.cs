// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    /// Represents basic information about the identity providers for a tenant.
    /// </summary>
    [DataContract]
    public class TenantIdentityProviderResponse
    {
        /// <summary>
        ///     Gets or sets the identity providers for the tenant.
        /// </summary>
        [DataMember(Name = "identityProviders", EmitDefaultValue = false, IsRequired = false)]
        public List<IdentityProviderResponse> IdentityProviders { get; set; }

        /// <summary>
        ///     Should the identity providers be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeIdentityProviders()
        {
            return IdentityProviders != null;
        }
    }
}