// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Cache an entity identified by a defined namespace and alias per tenant.
    /// </summary>
    public class TenantEntityCache
    {
        /// <summary>
        /// Create a new <see cref="TenantEntityCache"/>.
        /// </summary>
        /// <param name="entityNamespace">
        /// The namespace of the entity being cached. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="entityAlias">
        /// The alias of the entity being cached. This cannot be null, empty or whitespace.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null, empty or whitespace.
        /// </exception>
        public TenantEntityCache(string entityNamespace, string entityAlias)
        {
            if (String.IsNullOrWhiteSpace(entityNamespace))
            {
                throw new ArgumentNullException("entityNamespace");
            }
            if (String.IsNullOrWhiteSpace(entityAlias))
            {
                throw new ArgumentNullException("entityAlias");
            }

            EntityNamespace = entityNamespace;
            EntityAlias = entityAlias;
        }

        /// <summary>
        /// Synchronization object.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Resource type map.
        /// </summary>
        internal readonly Dictionary<long, EntityRef> TenantEntityMap = new Dictionary<long, EntityRef>();

        /// <summary>
        /// The namespace of the entity being cached.
        /// </summary>
        public string EntityNamespace { get; private set; }

        /// <summary>
        /// The alias of the entity being cached.
        /// </summary>
        public string EntityAlias { get; private set; }

        /// <summary>
        /// Lookup the tenant's <see cref="Model.EntityRef"/>. If not found, create it, 
        /// add it to the map and return it. This method is threadsafe.
        /// </summary>
        /// <returns>
        /// An <see cref="Model.EntityRef"/> containing the entity for the current tenant.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> is not set.
        /// </exception>
        protected internal EntityRef EntityRef
        {
            get
            {
                if (!RequestContext.IsSet)
                {
                    throw new InvalidOperationException("Request context not set.");
                }

                EntityRef result;
                long tenantId = RequestContext.TenantId;

                if (!TenantEntityMap.TryGetValue(tenantId, out result))
                {
                    lock (_syncRoot)
                    {
                        if (!TenantEntityMap.TryGetValue(tenantId, out result))
                        {
                            result = new EntityRef(EntityNamespace, EntityAlias);
                            TenantEntityMap[tenantId] = result;
                        }
                    }
                }

                return result;
            }
        }
    }
}
