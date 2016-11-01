// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.SoftwarePlatform.Services.ApplicationManager;

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Holds information about a managed platform.
    /// </summary>
    [DataContract]
    public class RemotePlatformInfo
    {
        /// <summary>
        /// A unique identifier that tracks the database that the platform points to.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// The list of tenants operating on this platform.
        /// </summary>
        [DataMember(Name = "tenants")]
        public TenantList Tenants { get; set; }

        /// <summary>
        /// The details of all versions of all apps currently available on this platform.
        /// </summary>
        [DataMember(Name = "apps")]
        public IList<AvailableApplication> Apps { get; set; }

        /// <summary>
        /// The name of the machine that this platform is operating on.
        /// </summary>
        [DataMember(Name = "feHost")]
        public string FrontEndHost { get; set; }

        /// <summary>
        /// The domain of the machine that this platform is operating on.
        /// </summary>
        [DataMember(Name = "feDomain")]
        public string FrontEndDomain { get; set; }
        
        /// <summary>
        /// The name of the database server to which this platform is connecting.
        /// </summary>
        [DataMember(Name = "dbServer")]
        public string DatabaseServer { get; set; }

        /// <summary>
        /// The catalog name of the database to which this platform is connecting.
        /// </summary>
        [DataMember(Name = "db")]
        public string Database { get; set; }
    }
}