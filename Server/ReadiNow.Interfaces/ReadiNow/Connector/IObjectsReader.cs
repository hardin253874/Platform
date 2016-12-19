// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using ReadiNow.Annotations;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Reads a group of objects.
    /// </summary>
    /// <remarks>
    /// Disposing causes any held references to be disposed. Must be held open while objects are being read.
    /// </remarks>
    public interface IObjectsReader : IDisposable

    {
        /// <summary>
        /// Reads a group of objects. May only be called once per instance.
        /// </summary>
        [NotNull]
        IEnumerable<IObjectReader> GetObjects();
    }

}
