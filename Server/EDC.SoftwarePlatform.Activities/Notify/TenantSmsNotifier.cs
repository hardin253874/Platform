// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Notify
{
    /// <summary>
    /// Notifier that is to be used by the tenant
    /// </summary>
    public class TenantSmsNotifier : INotifier
    {
        long _notifierId;
        
        public TenantSmsNotifier()
        {
            _notifierId = Entity.GetId("core:tenantNotificationProvider");
        }
    
        public TenantSmsNotifier(long notifierId)
        {
            _notifierId = notifierId;
        }

        /// <summary>
        /// Send a set of people a message based upon the SMS notifier for the tenant
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="people">The people to attempt</param>
        /// <param name="expectReply">If true a reply is expected</param>
        /// <returns>The send receipts</returns>
        public void Send(Notification notification, IEnumerable<IEntity> people, bool expectReply)
        {
            var smsNotifier = Entity.Get(_notifierId);

            if (smsNotifier == null)
                throw new MissingNotifierException();

            TwilioRouter.Instance.Send(smsNotifier, notification, people, expectReply);
        }

        /// <summary>
        /// Is the tenant notifier configured?
        /// </summary>
        public bool IsConfigured { get { return Entity.Exists(_notifierId); } }

        public class MissingNotifierException: Exception
        {
            public MissingNotifierException() : base("No notifier has been configured for the tenant") { }
        }
    }
}
