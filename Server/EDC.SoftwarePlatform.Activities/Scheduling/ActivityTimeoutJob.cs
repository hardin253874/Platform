// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Quartz;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    /// A scheduled job that is used by activities that have a timeout
    /// </summary>
    public class ActivityTimeoutJob: IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {

            }
            catch (Exception)
            {
                
                throw new JobExecutionException();
            }

        }
    }
}
