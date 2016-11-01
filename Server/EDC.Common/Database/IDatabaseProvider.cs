// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Database
{
    /// <summary>
    /// Top-level provider for database connections.
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        ///     Gets an object that encapsulates an open and authorized database connection.
        /// </summary>
        /// <param name="requireTransaction">if set to <c>true</c> a valid database transaction is required.</param>
        /// <returns></returns>
        IDatabaseContext GetContext(bool requireTransaction = false);
    }
}
