using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// Controls the background tasks
    /// </summary>
    public interface IBackgroundTaskController
    {
        bool StopAll();

        void StartAll();

        void SuspendToDb();

        void RestoreFromDb();

        string GetReport();
    }
}
