// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Engine
{
    /// <summary>
    /// A very simple cache for workflow metadata that relies on the knowledge that whenever a workflow is updated it's workflow update hash is changes as well.
    /// </summary>
    public class CachingWorkflowMetadataFactory : GenericCacheService<long, WorkflowMetadata>, IWorkflowMetadataFactory
    {
        /// <summary>
        /// Constructor for test code.
        /// </summary>
        public CachingWorkflowMetadataFactory() : this(new WorkflowMetadataFactory())
        { }


        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="innerFactory">An inner factory.</param>
        public CachingWorkflowMetadataFactory(IWorkflowMetadataFactory innerFactory)
            : base("WorkflowMetadataFactory", new CacheFactory { MetadataCache = true })
        {
            if (innerFactory == null)
                throw new ArgumentNullException("innerFactory");

            InnerFactory = innerFactory;
        }


        /// <summary>
        /// The inner factory
        /// </summary>
        internal IWorkflowMetadataFactory InnerFactory { get; private set; }


        /// <summary>
        /// Get the metadata from cache. Refresh cache if hash is not up-to-date.
        /// </summary>
        public WorkflowMetadata Create(Workflow wf)
        {
            long key = wf.Id;
            WorkflowMetadata result;

            bool fromCache = TryGetOrAdd(key, out result, k => InnerFactory.Create(wf) );

            if (fromCache && result.WorkflowUpdateHash != wf.WorkflowUpdateHash)
            {
                result = InnerFactory.Create(wf);
                Cache.Add(key, result);
            }

            return result;
        }
    }
}
