// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Holds a local (possibly incomplete) set of fields and values for an entity.
    /// </summary>
    public interface IEntityFieldValues
    {
        /// <summary>
        /// Gets a field from the collection.
        /// </summary>
        /// <param name="fieldId">The field to get.</param>
        /// <param name="value">The returned value.</param>
        /// <returns>True if the field was found.</returns>
        bool TryGetValue( long fieldId, out object value );

        /// <summary>
        /// Gets/sets a field value.
        /// </summary>
        /// <param name="fieldId">The field</param>
        /// <returns>The value</returns>
        object this [ long fieldId ] { get; set; }

        /// <summary>
        /// Remove a field from the collection.
        /// </summary>
        /// <param name="fieldId">The field to remove.</param>
        void Remove( long fieldId );

        /// <summary>
        /// Gets the list of field IDs currently stored.
        /// </summary>
        IEnumerable<long> FieldIds { get; }

        /// <summary>
        /// Determines if the field is currently stored in the collection.
        /// </summary>
        bool ContainsField( long fieldId );

        /// <summary>
        /// Returns true if not empty.
        /// </summary>
        bool Any();

        /// <summary>
        /// Gets an enumerable set of field/value pairs.
        /// </summary>
        IEnumerable<KeyValuePair<long, object>> GetPairs( );
    }
}
