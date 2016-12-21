// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using ReadiNow.Integration.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Notify
{
    // 
    // IN PROGRESS - Please leave
    // This code is in development and switched off until email and SMS approvals are required by PM.
    //

    /// <summary>
    /// A simple router that pushes notications to either SMS or email
    /// </summary>
    public class NotificationRouter: INotificationRouter
    {
        public static NotificationRouter Instance { get; } = new NotificationRouter();      

        public void Send(IEntity notifier, Notification notification, IEnumerable<IEntity> people, bool expectReply)
        {
            var twilioNotifier = notifier.As<TwilioNotifier>();

            if (twilioNotifier != null)
                TwilioRouter.Instance.Send(twilioNotifier, notification, people, expectReply);
            else
            {
                var emailNotifier = notifier.As<EmailNotifier>();

                if (emailNotifier != null)
                    EmailRouter.Instance.Send(emailNotifier, notification, people, expectReply);
                else
                    throw new Exception("Unrecognised notification provider");
            }
        }
        
    }
}
