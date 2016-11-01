// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect
{
    /// <summary>
    /// Represents state information kept during the Open Id Connect authorization process.
    /// </summary>
    [DataContract]
    public class OpenIdConnectAuthorizationState
    {
        /// <summary>
        /// The redirect url.
        /// </summary>
        [DataMember(Name = "redirectUrl", EmitDefaultValue = false, IsRequired = true)]
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Timestamp of when the state was created.
        /// </summary>
        [DataMember(Name = "timestamp", EmitDefaultValue = false, IsRequired = true)]
        public long Timestamp { get; set; }

        /// <summary>
        /// The id of the identity provider.
        /// </summary>
        [DataMember(Name = "idpId", EmitDefaultValue = false, IsRequired = true)]
        public long IdentityProviderId { get; set; }

        /// <summary>
        /// The id of the tenant.
        /// </summary>
        [DataMember(Name = "tenantId", EmitDefaultValue = false, IsRequired = true)]
        public long TenantId { get; set; }

        /// <summary>
        /// Nonce value.
        /// </summary>
        [DataMember(Name = "nonce", EmitDefaultValue = false, IsRequired = true)]
        public string Nonce { get; set; }
    }
}