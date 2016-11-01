// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EDC.Threading;
using System.Diagnostics;

namespace EDC.Syslog
{
    /// <summary>
    ///     Represents a queueing syslog writer.
    ///     It spins up a task and waits until messages are
    ///     available and then sends them.
    /// </summary>
    public class SyslogQueueingMessageWriter : ISyslogMessageWriter, IDisposable
    {
        /// <summary>
        ///     The maximum retries.
        /// </summary>
        private const int MaxRetries = 5;


        /// <summary>
        ///     The _cancellation token source
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();        


        /// <summary>
        ///     The maximum retries.
        /// </summary>
        private readonly int _maxRetries;


        /// <summary>
        ///     The message queue.
        /// </summary>
        private BlockingCollection<SyslogMessage> _messageQueue = new BlockingCollection<SyslogMessage>();


        /// <summary>
        ///     The actual message sender.
        /// </summary>
        private readonly ISyslogMessageWriter _messageWriter;


        /// <summary>
        ///     The disposed flag.
        /// </summary>
        private bool _disposed;


        /// <summary>
        /// The message writer task.
        /// </summary>
        private Task _msgWriterTask;


        /// <summary>
        /// The sync root
        /// </summary>
        private readonly object _syncRoot = new object();


        /// <summary>
        /// 
        /// </summary>
        private bool _canWriteMessages = true;


        /// <summary>
        ///     Initializes a new instance of the <see cref="SyslogQueueingMessageWriter" /> class.
        /// </summary>
        /// <param name="messageWriter">The message writer.</param>        
        /// <param name="maxRetries">The maximum retries.</param>
        /// <exception cref="System.ArgumentNullException">messageSender</exception>
        public SyslogQueueingMessageWriter(ISyslogMessageWriter messageWriter, int maxRetries = MaxRetries)
        {
            if (messageWriter == null)
            {
                throw new ArgumentNullException(nameof(messageWriter));
            }            

            _messageWriter = messageWriter;
            _maxRetries = maxRetries;            

            if (_maxRetries > MaxRetries)
            {
                _maxRetries = MaxRetries;
            }

            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            // Start listening for messages
            _msgWriterTask = Task.Run(() =>
            {
                try
                {
                    foreach (SyslogMessage message in _messageQueue.GetConsumingEnumerable(cancellationToken))
                    {
                        WriteWithRetry(message);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _canWriteMessages = false;                                        
                    Trace.TraceError("An unexpected error occurred in the syslog message writer worker thread. No more messages will be written to the syslog server. Error:{0}.", ex);
                }                
            }, _cancellationTokenSource.Token);


            // Register an action with the application exist rendezvous point so that any messages will be flushed.
            RendezvousPoint.RegisterAction(WellKnownRendezvousPoints.ApplicationExit, () => Dispose(true));
        }


        #region ISyslogMessageWriter Members


        /// <summary>
        ///     Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">message</exception>
        public void Write(SyslogMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_disposed || !_canWriteMessages) return;

            _messageQueue.TryAdd(message);
        }


        #endregion


        /// <summary>
        /// Writes the specified message with a retry.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteWithRetry(SyslogMessage message)
        {
            int retryCount = 0;

            while (true)
            {
                bool retry = false;

                try
                {
                    _messageWriter.Write(message);
                }
                catch (Exception ex)
                {
                    retry = (_maxRetries > 0) && (++retryCount <= _maxRetries);
                    
                    if (retry)
                    {
                        Trace.TraceWarning("An error occurred writing syslog message MsgId:{0}. Retry count:{1}. Error:{2}.", message.MsgId, retryCount, ex);
                    }
                    else
                    {
                        Trace.TraceError("An error occurred writing syslog message MsgId:{0}. Error:{1}.", message.MsgId, ex);
                    }                    
                }

                if (!retry)
                {
                    break;
                }
            }
        }


        #region IDisposable Methods


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        ///     Dispose the object.
        /// </summary>
        /// <param name="disposing">True if Dispose is called from user code.</param>
        private void Dispose(bool disposing)
        {            
            lock (_syncRoot)
            {
                if (_disposed) return;
                _disposed = true;                
            }

            if (!disposing) return;            
               
            try
            {
                _cancellationTokenSource.Cancel();
                _msgWriterTask.Wait();                
            }
            catch (Exception ex)
            {
                Trace.TraceInformation($"An exception occurred while canceling the syslog writer task. Error: {ex}.");
            }

            _msgWriterTask = null;

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;            

            FlushMessages();

            _messageQueue.Dispose();
            _messageQueue = null;            
        }


        /// <summary>
        ///     Flushes the messages.
        /// </summary>
        private void FlushMessages()
        {
            try
            {
                SyslogMessage[] messages = _messageQueue.ToArray();
                if (messages.Length <= 0) return;

                foreach (SyslogMessage message in messages)
                {
                    _messageWriter.Write(message);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation($"An exception occurred while flushing syslog message queue. Error: {ex}.");
            }
        }


        #endregion
    }
}