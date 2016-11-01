// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Test.MessageQueue
{
    /// <summary>
    /// Message queue mock consume event arguments.
    /// </summary>
    public class MessageQueueMockConsumeEventArgs : EventArgs
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="message">The message to return with the event.</param>
        public MessageQueueMockConsumeEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// The message received.
        /// </summary>
        public string Message { get; private set; }
    }
}
