// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace EDC.Diagnostics
{
    /// <summary>
    /// Represents the core data associated with each application request.
    /// This is used for logging purposes only.
    /// </summary>
    [Serializable]
    [DebuggerStepThrough]
    public class DiagnosticsRequestContextData : ILogicalThreadAffinative
    {
        /// <summary>
        /// The user name
        /// </summary>
        /// 
        private readonly string _userName;

        /// <summary>
        /// The tenant id
        /// </summary>
        private readonly long _tenantId;

        /// <summary>
        /// The _tenant name
        /// </summary>
        private readonly string _tenantName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsRequestContextData" /> class.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="userName">Name of the user.</param>
        public DiagnosticsRequestContextData(long tenantId, string tenantName, string userName)
        {
            _tenantId = tenantId;
            _tenantName = tenantName;
            _userName = userName;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName
        {
            get
            {
                return _userName;
            }          
        }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        /// <value>
        /// The tenant id.
        /// </value>
        public long TenantId
        {
            get
            {
                return _tenantId;
            }            
        }

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        public string TenantName
        {
            get
            {
                return _tenantName;
            }
        }
    }
}
