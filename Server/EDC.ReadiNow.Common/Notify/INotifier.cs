// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Notify
{
    public interface INotifier
    {
        /// <summary>
        /// Send a set of people a message
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="people">The people to attempt</param>
        /// <param name="expectReply">If true a reply is expected</param>
        /// <returns>The send receipts</returns>
        void Send(Notification notification, IEnumerable<IEntity> people, bool expectReply);

        /// <summary>
        /// Is the notifier configured?
        /// </summary>
        bool IsConfigured { get; }
    }
}
