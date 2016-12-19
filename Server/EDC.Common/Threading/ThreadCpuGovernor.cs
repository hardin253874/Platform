// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Threading;

namespace EDC.Threading
{
    /// <summary>
    /// Rudimentary thread cpu governor. 
    /// </summary>
    public class ThreadCpuGovernor
    {
        private Stopwatch _stopWatch = new Stopwatch();
        private int _waitPercentage;


        /// <summary>
        /// Creates a new governor.
        /// </summary>
        /// <param name="waitPercentage">The percentage of the elapsed time to wait when yielding the CPU.</param>
        public ThreadCpuGovernor(int waitPercentage)
        {
            if (waitPercentage <= 0 || waitPercentage > 100)
            {
                throw new ArgumentOutOfRangeException( nameof( waitPercentage ) );
            }

            _waitPercentage = waitPercentage;
        }


        /// <summary>
        /// Causes the current thread to sleep to reduce CPU load
        /// and to give other threads a chance to execute.
        /// </summary>
        public void Yield()
        {    
            // First call, simply return.        
            if (!_stopWatch.IsRunning)
            {
                _stopWatch.Restart();
                return;
            }            

            // Only wait when at least 100ms of work has been done.
            if (_stopWatch.Elapsed.TotalMilliseconds < 100)
            {                
                return;
            }            

            // Calculate the wait time
            int waitMillseconds = (int)(_stopWatch.Elapsed.TotalMilliseconds * (_waitPercentage / 100.0));

            Thread.Sleep(waitMillseconds);

            _stopWatch.Restart();
        }
    }
}
