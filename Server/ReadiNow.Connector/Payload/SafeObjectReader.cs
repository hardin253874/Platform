// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Connector.Payload
{
    /// <summary>
    /// Provide a safety wrapper over IObjectReader.
    /// Note: this is intended as a stop-gap because the dynamic object deserializer isn't handling nulls well at the moment.
    /// </summary>
    internal class SafeObjectReader : IObjectReader 
    {
        private readonly IObjectReader _inner;

        public SafeObjectReader( IObjectReader inner )
        {
            if ( inner == null )
                throw new ArgumentNullException( "inner" );

            _inner = inner;
        }

        /// <summary>
        /// Get a list of keys.
        /// </summary>
        public ISet<string> GetKeys( )
        {
            return _inner.GetKeys( );
        }

        /// <summary>
        /// Determine if the specified key is present.
        /// </summary>
        public bool HasKey( string key )
        {
            return _inner.HasKey( key );
        }

        /// <summary>
        /// Read an integer field.
        /// </summary>
        public int? GetInt( string key )
        {
            try
            {
                if ( !_inner.HasKey( key ) )
                    return null;

                return _inner.GetInt( key );
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read a decimal field.
        /// </summary>
        public decimal? GetDecimal( string key )
        {
            try
            {
                if ( !_inner.HasKey( key ) )
                    return null;
                
                return _inner.GetDecimal( key );
            }
            catch ( NullReferenceException )
            {
                return null;
            }
        }

        /// <summary>
        /// Read a date field.
        /// </summary>
        public DateTime? GetDate(string key)
        {
            try
            {
                return _inner.GetDate(key);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read a time field.
        /// </summary>
        public DateTime? GetTime(string key)
        {
            try
            {
                return _inner.GetTime(key);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read a date-time field.
        /// </summary>
        public DateTime? GetDateTime( string key )
        {
            try
            {
                return _inner.GetDateTime( key );
            }
            catch ( NullReferenceException )
            {
                return null;
            }
        }

        /// <summary>
        /// Read a string field.
        /// </summary>
        public string GetString( string key )
        {
            try
            {
                if (!_inner.HasKey(key))
                    return null;

                return _inner.GetString( key );
            }
            catch ( NullReferenceException )
            {
                return null;
            }
        }

        /// <summary>
        /// Read a boolean field.
        /// </summary>
        public bool? GetBoolean( string key )
        {
            try
            {
                if ( !_inner.HasKey( key ) )
                    return null;

                return _inner.GetBoolean( key );
            }
            catch ( NullReferenceException )
            {
                return null;
            }
        }

        /// <summary>
        /// Read a child object.
        /// Call when expecting one object, or a null.
        /// </summary>
        public IObjectReader GetObject( string key )
        {
            try
            {
                return _inner.GetObject( key );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Read a collection of child objects.
        /// Call when expecting zero to many objects in an array.
        /// </summary>
        public IReadOnlyList<IObjectReader> GetObjectList( string key )
        {
            try
            {
                return _inner.GetObjectList( key );
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Read a field that contains a list of integers.
        /// </summary>
        public IReadOnlyList<int> GetIntList(string key)
        {
            try
            {
                if (!_inner.HasKey(key))
                    return null;

                return _inner.GetIntList(key);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read a field that contains a list of decimals.
        /// </summary>
        public IReadOnlyList<decimal> GetDecimalList(string key)
        {
            try
            {
                if (!_inner.HasKey(key))
                    return null;

                return _inner.GetDecimalList(key);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Read a field that contains a list of strings.
        /// </summary>
        public IReadOnlyList<string> GetStringList(string key)
        {
            try
            {
                if (!_inner.HasKey(key))
                    return null;

                return _inner.GetStringList(key);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get some sort of reference of where the object came from.
        /// </summary>
        /// <returns>The location.</returns>
        public string GetLocation( )
        {
            return _inner.GetLocation( );
        }
    }
}
