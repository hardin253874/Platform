// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;
using ProtoBuf;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///  A queue that that will raise an event when it is enqueued to. 
	/// </summary>
	public class ListeningQueue<T> : IListeningQueue<T>
	{
        const string EnqueueMessage = "enqueue";
        const string IgnoreMessage = "ignore";

        private IQueue<T> InnerQueue { get; }
        private IChannel<ListeningQueueMessage> Channel { get; }

        public string Name { get { return InnerQueue.Name; } }

        /// <summary>
        /// Event when something has been added to the queue
        /// </summary>
        public event EventHandler EnqueuedReceived;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListeningQueue" /> class.
        /// </summary>
        /// <param name="innerQueue">The inner queue to used for storage</param>
        /// <param name="redisMgr">The redis manager to use for messages</param>
        public ListeningQueue( IQueue<T> innerQueue, RedisManager redisMgr )
		{
            InnerQueue = innerQueue;
            Channel = redisMgr.GetChannel<ListeningQueueMessage>($"channel|{innerQueue.Name}");
            Channel.MessageReceived += ChannelMessageReceived;
            Channel.Subscribe();
        }


        public void Enqueue(T value, CommandFlags flags = CommandFlags.None)
        {
            InnerQueue.Enqueue(value, flags);
            NotifyEnqueue();
        }

        public void Enqueue(T[] values, CommandFlags flags = CommandFlags.None)
        {
            InnerQueue.Enqueue(values, flags);
            NotifyEnqueue();
        }

        void NotifyEnqueue()
        {
            Channel.Publish(new ListeningQueueMessage(), PublishMethod.Immediate, options: PublishOptions.FireAndForget, publishToOriginator: true);
        }




        /// <summary>
        /// Retieve a value from the queue
        /// </summary>
        /// <param name="flags">Flags to pass to Redis</param>
        /// <returns>The head element or null if the queue is empty</returns>
        public T Dequeue(CommandFlags flags = CommandFlags.None)
        {
            return InnerQueue.Dequeue(flags);
        }

        /// <summary>
        /// The length of the queue
        /// </summary>
        public long Length
        {
            get
            {
                return InnerQueue.Length;
            }
        }

        private void ChannelMessageReceived(object sender, MessageEventArgs<ListeningQueueMessage> e)
        {
            if (e != null)
            {
                EnqueuedReceived?.Invoke(this, new EventArgs());
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Channel.MessageReceived -= ChannelMessageReceived;
                    Channel.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// Message passed to notify that something has been added to the listening queue
        /// </summary>
        [ProtoContract]
        public class ListeningQueueMessage
        {
        }

    }
}