// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace ReadiNow.Connector.Payload
{
    internal class JilDynamicObjectReader : IObjectReader
    {
        readonly IDynamicMetaObjectProvider _dynamicProvider;
        DynamicMetaObject _metaObject;
        dynamic _object;
        ISet<string> _keys;

        // To ensure we always get the same object references back
        IDictionary<string, IObjectReader> _valueCache;

        // To ensure we always get the same object references back
        IDictionary<string, IReadOnlyList<IObjectReader>> _listCache;

        public JilDynamicObjectReader( IDynamicMetaObjectProvider dynamicProvider )
        {
            if ( dynamicProvider == null )
                throw new ArgumentNullException( "dynamicProvider" );

            _dynamicProvider = dynamicProvider;
        }

        private DynamicMetaObject MetaObject
        {
            get
            {
                if ( _metaObject == null )
                {
                    _metaObject = _dynamicProvider.GetMetaObject( Expression.Constant( _dynamicProvider ) );
                }
                
                return _metaObject;
            }
        }

        private dynamic Object
        {
            get
            {
                if (_object == null)
                {
                    _object = MetaObject.Value;
                }
                return _object;
            }
        }

        /// <summary>
        /// Get a list of keys.
        /// </summary>
        public ISet<string> GetKeys( )
        {
            if ( _keys == null )
            {
                IEnumerable<string> keys = MetaObject.GetDynamicMemberNames( );
                _keys = new HashSet<string>( keys );
            }
            return _keys;
        }

        /// <summary>
        /// Determine if the specified key is present.
        /// </summary>
        public bool HasKey( string key )
        {
            return GetKeys( ).Contains( key );
        }

        /// <summary>
        /// Read an integer field.
        /// </summary>
        public int? GetInt( string key )
        {
            return ( int? ) Object [ key ];
        }

        /// <summary>
        /// Read a decimal field.
        /// </summary>
        public decimal? GetDecimal( string key )
        {
            return ( decimal? ) Object [ key ];
        }

        /// <summary>
        /// Read a date field.
        /// </summary>
        public DateTime? GetDate(string key)
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
                return null;
            DateTime result;
            if (!JsonValueParser.TryParseDate(value, out result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// Read a time field.
        /// </summary>
        public DateTime? GetTime(string key)
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
                return null;
            DateTime result;
            if (!JsonValueParser.TryParseTime(value, out result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// Read a date-time field.
        /// </summary>
        public DateTime? GetDateTime( string key )
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
                return null;
            DateTime result;
            if (!JsonValueParser.TryParseDateTime(value, out result))
                throw new FormatException();
            return result; 
        }

        /// <summary>
        /// Read a string field.
        /// </summary>
        public string GetString( string key )
        {
            return ( string ) Object [ key ];
        }

        /// <summary>
        /// Read a boolean field.
        /// </summary>
        public bool? GetBoolean( string key )
        {
            object obj = Object [ key ];
            if ( obj == null )
                return null;
            return ( bool ) Object [ key ];
        }

        /// <summary>
        /// Read an integer field.
        /// </summary>
        public IReadOnlyList<int> GetIntList(string key)
        {
            IEnumerable<int?> actual = Object[key];
            IReadOnlyList<int> result = null;

            if (actual != null)
            {
                try
                {
                    result = actual.Where( o => o != null ).Select( o => o.Value ).ToList( );
                }
                catch ( NullReferenceException )
                {
                    throw new FormatException( "List contained nulls." );
                }
            }
            return result;
        }

        /// <summary>
        /// Read a decimal field.
        /// </summary>
        public IReadOnlyList<decimal> GetDecimalList(string key)
        {
            IEnumerable<double> actual = Object[key];
            IReadOnlyList<decimal> result = null;

            if (actual != null)
            {
                try
                {
                    result = actual.Select( o => ( decimal ) o ).ToList( );
                }
                catch ( NullReferenceException )
                {
                    throw new FormatException( "List contained nulls." );
                }
            }
            return result;
        }

        /// <summary>
        /// Read a string field.
        /// </summary>
        public IReadOnlyList<string> GetStringList(string key)
        {
            IEnumerable<string> actual = Object[key];
            IReadOnlyList<string> result = null;

            if (actual != null)
            {
                try
                {
                    result = actual.Where( o => o != null ).ToList( );
                }
                catch ( NullReferenceException )
                {
                    throw new FormatException( "List contained nulls." );
                }
            }
            return result;
        }

        /// <summary>
        /// Read a child object.
        /// Call when expecting one object, or a null.
        /// </summary>
        public IObjectReader GetObject( string key )
        {
            IObjectReader result;

            if (_valueCache == null)
            {
                _valueCache = new Dictionary<string, IObjectReader>( );
            }
            else if (_valueCache.TryGetValue(key, out result))
            {
                return result;
            }

            object actual = Object [ key ];
            result = CaptureObject(actual);

            _valueCache [ key ] = result;
            return result;
        }

        /// <summary>
        /// Read a collection of child objects.
        /// Call when expecting zero to many objects in an array.
        /// </summary>
        public IReadOnlyList<IObjectReader> GetObjectList( string key )
        {
            IReadOnlyList<IObjectReader> result = null;

            if ( _listCache == null )
            {
                _listCache = new Dictionary<string, IReadOnlyList<IObjectReader>>( );
            }
            else if ( _listCache.TryGetValue( key, out result ) )
            {
                return result;
            }

            IEnumerable<object> actual = Object [ key ];
            if ( actual != null )
            {
                result = actual.Select( CaptureObject ).ToList( );                
            }

            _listCache [ key ] = result;
            return result;
        }

        /// <summary>
        /// Read a collection of child objects.
        /// Call when expecting zero to many objects in an array.
        /// </summary>
        public static IReadOnlyList<IObjectReader> GetObjectList( IDynamicMetaObjectProvider listObjectProvider )
        {
            if ( listObjectProvider == null )
                return null;

            DynamicMetaObject metaObject = listObjectProvider.GetMetaObject( Expression.Constant( listObjectProvider ) );
            dynamic dynamicObject = metaObject.Value;
            
            IEnumerable<object> actual = dynamicObject;
            if (actual == null)
                return null;

            IReadOnlyList<IObjectReader>  result = actual.Select( CaptureObject ).ToList( );
            return result;
        }

        /// <summary>
        /// Get some sort of reference of where the object came from.
        /// </summary>
        /// <returns>Empty.</returns>
        public string GetLocation( )
        {
            return string.Empty;
        }

        /// <summary>
        /// Wraps a JIL dynamic object in a JilDynamicObject reader. 
        /// </summary>
        private static IObjectReader CaptureObject( object actual )
        {
            IObjectReader result = null;

            if ( actual != null )
            {
                var dynamicProvider = actual as IDynamicMetaObjectProvider;
                if ( dynamicProvider == null )
                {
                    throw new Exception( "Internal error : Serializer gave us an object that did not support IDynamicMetaObjectProvider." );
                }

                result = new JilDynamicObjectReader( dynamicProvider );
            }

            return result;
        }
    }
}
