// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Database
{
    /// <summary>
    /// Provides access to the database.
    /// </summary>
    class DatabaseProvider : IDatabaseProvider
    {
        /// <summary>
        ///     Gets an object that encapsulates an open and authorized database connection.
        /// </summary>
        /// <param name="requireTransaction">if set to <c>true</c> a valid database transaction is required.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">The application database has not been configured.</exception>
        /// <exception cref="System.InvalidOperationException">The application database configuration settings are invalid.</exception>
        public IDatabaseContext GetContext(bool requireTransaction)
        {
            return DatabaseContext.GetContext(requireTransaction);
        }
    }
}
