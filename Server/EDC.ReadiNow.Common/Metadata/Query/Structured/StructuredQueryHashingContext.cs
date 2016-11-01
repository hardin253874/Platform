// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Context to help StructuredQueryHelper.GetCacheKeyToken generate more deterministic results.
    /// The problem : StructuredQuery internally uses lots of Guids to connect internal pieces together. This was to support various serializers that could
    /// not handle cycles well. However, this means that logically identical queries may contain different GUIds. Further, the StructuredQuery to SQL cache
    /// uses a hash of the serilized query as the cache key. So the above means we get lots of cache misses.
    /// The solution : Wrap this context around a serialization. It will dynamically substitute GUIDs determinitically generated in the order they are encountered.
    /// (And we hope that the serializiser visits members in a deterministic order).
    /// </summary>
    public class StructuredQueryHashingContext : IDisposable
    {
        private static byte [ ] EightZeroBytes = new byte [ 8 ];

        [ThreadStatic]
        private static Dictionary<Guid, Guid> GuidMap = null;

        private Dictionary<Guid, Guid> _parentMap = null;

        public StructuredQueryHashingContext( )
        {
            _parentMap = GuidMap;
            if ( GuidMap == null )
                GuidMap = new Dictionary<Guid, Guid>( );
        }

        public void Dispose( )
        {
            GuidMap = _parentMap;
        }

        /// <summary>
        /// Detects if there is a hashing context during serialization, and if so it normalizes GUIDs (in the order that they appear).
        /// This is to enhance cache hits.
        /// </summary>
        public static Guid GetGuid( Guid realGuid )
        {
            if ( GuidMap == null )
                return realGuid;

            Guid fauxGuid;
            if ( !GuidMap.TryGetValue( realGuid, out fauxGuid ) )
            {
                int count = GuidMap.Count;
                fauxGuid = new Guid( count + 1, 0, 0, EightZeroBytes );
                GuidMap [ realGuid ] = fauxGuid;
            }
            return fauxGuid;
        }

    }
}
