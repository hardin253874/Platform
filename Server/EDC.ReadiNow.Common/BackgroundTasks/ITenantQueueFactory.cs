using EDC.ReadiNow.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// Factory for generating tenant queues
    /// </summary>
    public interface ITenantQueueFactory
    {
        IListeningQueue<BackgroundTask> Create(long tenantId);
    }
}
