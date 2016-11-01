// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using ReadiNow.Connector.Interfaces;
using System;
using System.Collections.Generic;

namespace ReadiNow.Connector.Payload
{
    /// <summary>
    /// Container object for the bulk-member-processor callback.
    /// </summary>
    class MemberProcessor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bulkAction">Callback that processes the member for multiple source-target pairs.</param>
        public MemberProcessor( Action<IEnumerable<ReaderEntityPair>, IImportReporter> bulkAction )
        {
            if ( bulkAction == null )
                throw new ArgumentNullException( "bulkAction" );
            Action = bulkAction;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="singleAction">Callback that processes the member for a single source-target pairs.</param>
        public MemberProcessor( Action<IObjectReader, IEntity, IImportReporter> singleAction )
        {
            if ( singleAction == null )
                throw new ArgumentNullException( "singleAction" );

            Action = (pairs, reporter) =>
            {
                if ( pairs == null )
                    throw new ArgumentNullException( "pairs" );

                foreach ( ReaderEntityPair pair in pairs)
                {
                    singleAction( pair.ObjectReader, pair.Entity, reporter );
                }
            };
        }
        
        /// <summary>
        /// Callback action that performs the processing.
        /// </summary>
        public Action<IEnumerable<ReaderEntityPair>, IImportReporter> Action { get; private set; }
    }
}
