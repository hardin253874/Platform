// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
    /// <summary>
    /// The encrypted portion of the forms authentication ticket.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("TenantId: {TenantId} IdentityProviderId: {IdentityProviderId} IdentityProviderUserName: {IdentityProviderUserName} UserAccountId: {UserAccountId} XsrfToken: {XsrfToken} Persist: {Persist} HostIp: {HostIp} UserAgent: {UserAgent}")]
    public class AuthenticationToken
    {       
        [DataMember(Name = "xsrfToken", IsRequired = true, EmitDefaultValue = true)]
        [DefaultValue("")]
        public string XsrfToken { get; set; }

        [DataMember(Name = "persist", IsRequired = true, EmitDefaultValue = true)]
        [DefaultValue(false)]
        public bool Persist { get; set; }

        [DataMember(Name = "tenantId", IsRequired = true, EmitDefaultValue = true)]
        [DefaultValue(0)]
        public long TenantId { get; set; }

        [DataMember(Name = "idpId", IsRequired = true, EmitDefaultValue = true)]
        [DefaultValue(0)]
        public long IdentityProviderId { get; set; }

        [DataMember(Name = "idpUserName", IsRequired = true, EmitDefaultValue = true)]
        [DefaultValue("")]
        public string IdentityProviderUserName { get; set; }

        [DataMember(Name = "userId", IsRequired = false, EmitDefaultValue = true)]
        [DefaultValue(0)]
        public long UserAccountId { get; set; }

		[DataMember( Name = "hostIp", IsRequired = true, EmitDefaultValue = true )]
		[DefaultValue( "" )]
		public string HostIp { get; set; }

		[DataMember( Name = "userAgent", IsRequired = true, EmitDefaultValue = true )]
		[DefaultValue( "" )]
		public string UserAgent { get; set; }
	}
}