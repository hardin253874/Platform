using System.Collections.Generic;

namespace EDC.ReadiNow.BackgroundTasks
{
    public interface IBackgroundTaskManager
    {
        void AddTenant(long tenantId);
        void EnqueueTask(BackgroundTask task);
        void EnqueueTask(long tenantId, BackgroundTask task);
        void ExecuteImmediately(BackgroundTask task);
        void Start();
        void Stop();

        IEnumerable<QueueLengthEntry> QueueLengths();
    }

    public struct QueueLengthEntry
    {
        public string QueueName;
        public string TenantName;
        public long TenantId;
        public long Length;
    }
}