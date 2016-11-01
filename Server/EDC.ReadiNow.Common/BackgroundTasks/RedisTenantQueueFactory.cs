// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Messaging.Redis;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// Factory for generating distributed tenant queues that are stored in Redis
    /// </summary>
    public class RedisTenantQueueFactory : ITenantQueueFactory
    {
        readonly RedisManager _redisManager;
        readonly string _queuePrefix;

        /// <summary>
        /// Create a factory for generating tenant queues
        /// </summary>
        /// <param name="queuePrefix">The prefix to use for the queue in Redis</param>
        public RedisTenantQueueFactory(string queuePrefix)
        {
            _queuePrefix = queuePrefix;

            _redisManager = new RedisManager();
            _redisManager.Connect();
        }

        public IListeningQueue<BackgroundTask> Create(long tenantId)
        {
            return _redisManager.GetQueue<BackgroundTask>($"{_queuePrefix} | {tenantId}", false);
        }
    }
}
