// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// Updates the 'Entry point' value in the request context.
    /// </summary>
    public static class EntryPointContext
    {
        [ThreadStatic]
        private static string _entryPoint;

        public static string EntryPoint
        {
            get { return _entryPoint; }
        }

        /// <summary>
        /// Updates the 'entry point' setting within a context block.
        /// </summary>
        /// <param name="entryPoint">The name of the entry point.</param>
        /// <returns></returns>
        public static IDisposable SetEntryPoint( string entryPoint )
        {
            return SetEntryPoint( entryPoint, false );
        }

        /// <summary>
        /// Updates the 'entry point' setting within a context block.
        /// </summary>
        /// <param name="entryPoint">The name of the entry point.</param>
        /// <returns></returns>
        public static IDisposable AppendEntryPoint( string entryPoint )
        {
            return SetEntryPoint( entryPoint, true );
        }

		/// <summary>
		/// Indicates that the enclosed code should not write 'entry point' in SQL statements, because the addition of the parameter may cause problems.
		/// For example, call if creating temp tables that must be persisted on the database connection.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// The entry point may be set to null to prevent the parameter from being added, which may be necessary in some
		/// cases such as persisting temporary tables on a connection.
		/// </remarks>
        public static IDisposable UnsafeToIncludeEntryPoint( )
        {
            return SetEntryPoint( null, false );
        }

		/// <summary>
		/// Updates the 'entry point' setting within a context block.
		/// </summary>
		/// <param name="entryPoint">The name of the entry point.</param>
		/// <param name="append">if set to <c>true</c> [append].</param>
		/// <returns></returns>
        private static IDisposable SetEntryPoint( string entryPoint, bool append )
        {
            var context = RequestContext.GetContext( );
            string oldEntryPoint = _entryPoint;
            _entryPoint = append ? (oldEntryPoint ?? "") + "/" + entryPoint : entryPoint;

            return ContextHelper.Create( ( ) =>
            {
                _entryPoint = oldEntryPoint;
            } );
        }
    }
}
