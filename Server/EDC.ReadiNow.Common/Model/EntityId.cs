// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    public static class EntityId
    {
        /// <summary>
        ///  The maximium value an ID can be.
        /// </summary>
        public static readonly long Max = (long)Math.Pow(2.0, 53);

        /// <summary>
        /// The minimum temporary ID
        /// </summary>
        /// <remarks>This has been increased primarily due to the fact that we were running out
        /// of ids on the client during regression tests. The server was then considering temp ids
        /// as non-temp ids.</remarks>
        public static readonly long MinTemporary = Max - 1000000;  // our known id 'high watermark'

        /// <summary>
        /// Is the given <paramref name="id"/> a temporary ID?
        /// </summary>
        /// <param name="id">
        /// The ID to check.
        /// </param>
        /// <returns>
        /// True if it is a temporary ID, false otherwise.
        /// </returns>
        public static bool IsTemporary(long id)
        {
            return EntityTemporaryIdAllocator.IsAllocatedId( id ); 
        }
    }
}
