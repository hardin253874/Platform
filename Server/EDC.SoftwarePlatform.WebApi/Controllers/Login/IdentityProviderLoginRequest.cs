// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    ///     Resents an identity provider login request.
    /// </summary>
    public class IdentityProviderLoginRequest
    {
        /// <summary>
        ///     Gets or sets the name of the tenant.
        /// </summary>
        /// <value>
        ///     The name of the tenant.
        /// </value>
        [DataMember(Name = "tenant", EmitDefaultValue = false, IsRequired = true)]
        public string Tenant { get; set; }

        /// <summary>
        ///     Gets or sets the identity provider identifier.
        /// </summary>
        /// <value>The identity provider identifier.</value>
        [DataMember(Name = "idpId", EmitDefaultValue = false, IsRequired = true)]
        public long IdentityProviderId { get; set; }

        /// <summary>
        ///     Gets or sets the redirect URL.
        /// </summary>
        /// <value>The redirect URL.</value>
        [DataMember(Name = "redirectUrl", EmitDefaultValue = false, IsRequired = true)]
        public string RedirectUrl { get; set; }

        /// <summary>
        ///     Should the tenant value be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeTenant()
        {
            return Tenant != null;
        }

        /// <summary>
        ///     Should identity provider id be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeIdentityProviderId()
        {
            return IdentityProviderId >= 0;
        }

        /// <summary>
        ///     Should redirect url be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeRedirectUrl()
        {
            return RedirectUrl != null;
        }
    }
}