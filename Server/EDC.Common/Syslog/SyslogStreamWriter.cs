// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using EDC.IO;

namespace EDC.Syslog
{
    /// <summary>
    /// Represents a syslog stream writer.
    /// </summary>
    public class SyslogStreamWriter : ISyslogMessageWriter, IDisposable
    {
        /// <summary>
        ///     The serializer
        /// </summary>
        private readonly ISyslogMessageSerializer _serializer;


        /// <summary>
        ///     The stream provider
        /// </summary>
        private IStreamProvider _streamProvider;


        /// <summary>
        ///     The disposed flag.
        /// </summary>
        private bool _disposed;


        /// <summary>
        /// Initializes a new instance of the <see cref="SyslogStreamWriter" /> class.
        /// </summary>
        /// <param name="streamProvider">The stream provider.</param>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="System.ArgumentNullException">serializer
        /// or
        /// streamProvider</exception>
        public SyslogStreamWriter(IStreamProvider streamProvider, ISyslogMessageSerializer serializer)
        {
            if (streamProvider == null)
            {
                throw new ArgumentNullException(nameof(streamProvider));
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }            

            _streamProvider = streamProvider;
            _serializer = serializer;            
        }


        #region IDisposable Members


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion


        #region ISyslogMessageWriter Members


        /// <summary>
        ///     Write the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(SyslogMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                Stream stream = _streamProvider.GetStream();

                byte[] data = SerializeMessage(message);

                stream.Write(data, 0, data.Length);                
            }
            catch
            {
                _streamProvider.CloseStream();
                throw;
            }
        }


        #endregion


        /// <summary>
        ///     Serializes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private byte[] SerializeMessage(SyslogMessage message)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(message, stream);

                data = stream.ToArray();
            }

            return data;
        }


        /// <summary>
        ///     Disposes the specified stream.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        private void Dispose(bool disposing)
        {            
            if (_disposed) return;
            _disposed = true;

            if (!disposing) return;
            
            _streamProvider.Dispose();
            _streamProvider = null;               
        }
    }
}