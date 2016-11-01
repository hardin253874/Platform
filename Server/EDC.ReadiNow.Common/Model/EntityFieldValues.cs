// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Holds a local (possibly incomplete) set of fields and values for an entity.
    /// </summary>
    class EntityFieldValues : IEntityFieldValues
    {
        /// <summary>
        /// Field value container
        /// </summary>
        private ConcurrentDictionary<long, object> _fieldValues = new ConcurrentDictionary<long,object>();

        /// <summary>
        /// Gets a field from the collection.
        /// </summary>
        /// <param name="fieldId">The field to get.</param>
        /// <param name="value">The returned value.</param>
        /// <returns>True if the field was found.</returns>
        public bool TryGetValue( long fieldId, out object value )
        {
            return _fieldValues.TryGetValue( fieldId, out value );
        }

        /// <summary>
        /// Gets/sets a field value.
        /// </summary>
        /// <param name="fieldId">The field</param>
        /// <returns>The value</returns>
        public object this [ long fieldId ]
        {
            get { return _fieldValues [ fieldId ]; }
            set { _fieldValues [ fieldId ] = value; }
        }

        /// <summary>
        /// Remove a field from the collection.
        /// </summary>
        /// <param name="fieldId">The field to remove.</param>
        public void Remove( long fieldId )
        {
            object oldValue;
            _fieldValues.TryRemove( fieldId, out oldValue );
        }

        /// <summary>
        /// Gets the list of field IDs currently stored.
        /// </summary>
        public IEnumerable<long> FieldIds
        {
            get
            {
                return _fieldValues.Select( pair => pair.Key );
            }
        }

        /// <summary>
        /// Determines if the field is currently stored in the collection.
        /// </summary>
        public bool ContainsField( long fieldId )
        {
            return _fieldValues.ContainsKey( fieldId );
        }

        /// <summary>
        /// Returns true if not empty.
        /// </summary>
        public bool Any( )
        {
            return _fieldValues.Any( );
        }

        /// <summary>
        /// Gets an enumerable set of field/value pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<long, object>> GetPairs( )
        {
            return _fieldValues;
        }
    }
}
