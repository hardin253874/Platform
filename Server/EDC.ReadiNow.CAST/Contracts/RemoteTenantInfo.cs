// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds information about a managed tenant.
    /// </summary>
    [DataContract]
    public class RemoteTenantInfo
    {
        /// <summary>
        /// The id that is unique for the tenant on each platform.
        /// </summary>
        [DataMember(Name = "remoteId")]
        public long RemoteId { get; set; }

        /// <summary>
        /// The name of the tenant.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Is the tenant disabled.
        /// </summary>
        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// The details of all versions of all apps currently installed on this tenant.
        /// </summary>
        [DataMember(Name = "apps")]
        public IList<InstalledApplication> Apps { get; set; }
        
        /// <summary>
        /// The user accounts on this tenant.
        /// </summary>
        [DataMember(Name = "users")]
        public UserList Users { get; set; }

        /// <summary>
        /// The roles the exist on this tenant.
        /// </summary>
        [DataMember(Name = "roles")]
        public RoleList Roles { get; set; }
    }

    /// <summary>
    /// A collection of tenant information.
    /// </summary>
    [CollectionDataContract(ItemName = "tenant")]
    public class TenantList : List<RemoteTenantInfo>
    {
        public TenantList() { }
        public TenantList(int capacity) : base(capacity) { }
        public TenantList(IEnumerable<RemoteTenantInfo> collection) : base(collection) { }
    }
}
