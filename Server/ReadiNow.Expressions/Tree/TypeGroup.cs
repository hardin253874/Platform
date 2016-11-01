// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Represents a group of types. Mainly for convenience in the XML language database.
    /// </summary>
    public enum TypeGroup
    {
        None,

        /// <summary>
        /// Int32, Decimal, Currency
        /// </summary>
        Numeric,

        /// <summary>
        /// All numeric, date, time, date-time, string
        /// </summary>
        Comparable,

        /// <summary>
        /// All comparable, guid, bool, entity
        /// </summary>
        Equatable,

        /// <summary>
        /// All types except bool.
        /// </summary>
        AllExceptBool,

        /// <summary>
        /// All types except entity.
        /// </summary>
        AllExceptEntity,

        /// <summary>
        /// All types.
        /// </summary>
        All,

        /// <summary>
        /// Date, Time, DateTime.
        /// </summary>
        DateOrTime,

        /// <summary>
        /// Custom processing.
        /// </summary>
        Custom,
    }
}
