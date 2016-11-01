// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;

namespace EDC.IO
{
    /// <summary>
    /// Interface for providing a stream.
    /// </summary>
    public interface IStreamProvider : IDisposable
    {
        /// <summary>
        ///     Gets the stream.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();


        /// <summary>
        ///     Closes the stream.
        /// </summary>
        void CloseStream();
    }
}