// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Packages information about tenants.
    /// </summary>
    [DataContract]
    public class TenantInfoResponse : CastResponse
    {
        /// <summary>
        /// List of tenant information.
        /// </summary>
        [DataMember(Name = "tenants")]
        public TenantList Tenants { get; set; }
    }
}
