// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Notify
{
    public interface INotificationRouter
    {
        /// <summary>
        /// Route to the people it can handle, return the ones it can't handle
        /// </summary>
        /// <param name="notifier">The notifier to use</param>
        /// <param name="message">The message to send</param>
        /// <param name="people">The people to attempt</param>
        /// <param name="expectReply">If true a reply is expected</param>
        /// <returns>The send receipts</returns>
        void Send(IEntity notifier, Notification notification, IEnumerable<IEntity> people, bool expectReply);
    }
}
