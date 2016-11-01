// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using ReadiNow.Annotations;

namespace ReadiNow.Connector
{
    /// <summary>
    /// General interface for accessing object structures. (e.g. JSON)
    /// </summary>
    public interface IObjectReader
    {
        /// <summary>
        /// Get a list of keys.
        /// </summary>
        ISet<string> GetKeys( );


        /// <summary>
        /// Determine if the specified key is present.
        /// </summary>
        bool HasKey( [NotNull] string key );


        /// <summary>
        /// Read an integer field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        int? GetInt( [NotNull] string key );


        /// <summary>
        /// Read a decimal field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        decimal? GetDecimal( [NotNull] string key );


        /// <summary>
        /// Read a date field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        DateTime? GetDate( [NotNull] string key );


        /// <summary>
        /// Read a time field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        DateTime? GetTime( [NotNull] string key );


        /// <summary>
        /// Read a date-time field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        DateTime? GetDateTime( [NotNull] string key );


        /// <summary>
        /// Read a string field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        string GetString( [NotNull] string key );


        /// <summary>
        /// Read a boolean field.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        bool? GetBoolean( [NotNull] string key );


        /// <summary>
        /// Read a child object.
        /// Call when expecting one object, or a null.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        IObjectReader GetObject( [NotNull] string key );


        /// <summary>
        /// Read a field that contains a list of integers.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        IReadOnlyList<int> GetIntList( [NotNull] string key );


        /// <summary>
        /// Read a field that contains a list of decimals.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        IReadOnlyList<decimal> GetDecimalList( [NotNull] string key );


        /// <summary>
        /// Read a field that contains a list of strings.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        IReadOnlyList<string> GetStringList( [NotNull] string key );


        /// <summary>
        /// Read a collection of child objects.
        /// Call when expecting zero to many objects in an array.
        /// </summary>
        /// <exception cref="InvalidCastException">
        /// Throws an invalid cast exception if data is of incorrect type.
        /// </exception>
        [CanBeNull]
        IReadOnlyList<IObjectReader> GetObjectList( [NotNull] string key );


        /// <summary>
        /// Return some form of description of where the object came from. Or null.
        /// </summary>
        [CanBeNull]
        string GetLocation( );

    }
}
