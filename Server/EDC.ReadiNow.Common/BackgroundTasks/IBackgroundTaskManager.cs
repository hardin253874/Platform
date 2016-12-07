using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.BackgroundTasks
{
    public interface IBackgroundTaskManager
    {
        bool IsActive { get; set; }
        void AddTenant(long tenantId);
        void EnqueueTask(BackgroundTask task);
        void EnqueueTask(long tenantId, BackgroundTask task);
        void ExecuteImmediately(BackgroundTask task);
        void Start();
        void Stop();
        void SuspendAllTasks();
        void RestoreAllTasks();

        void GenerateReport(StringBuilder sb);


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