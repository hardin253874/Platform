// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.EventTarget
{

    /// <summary>
    /// Helper for handling notificaitons that have completed.
    /// </summary>
    public static class NotificationHelper
    {
        public static void CheckAndResume(this IEnumerable<Notification> notifications)
        {
            var runner = Factory.Current.Resolve<IWorkflowRunner>();

            foreach (var notification in notifications)
            {
                if (notification.NPendingRun?.PendingActivity != null && notification.Complete())
                {
                    runner.ResumeWorkflowAsync(notification.NPendingRun, new NotifyResumeEvent());
                }
            }
        }

        /// <summary>
        /// Has the notificaiton been completed
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
       public static bool Complete(this Notification notification)
        {
            var sent = notification.SendRecords.Where(s => String.IsNullOrEmpty(s.SrErrorMessage));

            return !sent.Any() || sent.All(s => s.SrToReply.Any());
        }
    }
}
