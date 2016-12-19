// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Dynamic;
using ReadiNow.Annotations;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Exposes a dynamic object for access via IObjectReader.
    /// </summary>
    public interface IDynamicObjectReaderService
    {
        /// <summary>
        /// Create an IObjectReader.
        /// </summary>
        [NotNull]
        IObjectReader GetObjectReader( IDynamicMetaObjectProvider dynamicProvider );
    }
}
