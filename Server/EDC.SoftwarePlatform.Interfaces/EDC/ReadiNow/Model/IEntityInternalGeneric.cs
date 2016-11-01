// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Diagnostics.CodeAnalysis;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    ///     IEntityGeneric interface.
    /// </summary>
    public interface IEntityInternalGeneric<in TGeneric>
    {
        /// <summary>
        ///     Attempts to retrieve the specified field of the specified entity from the internal cache.
        ///     No database hit will be made if the field is not currently cached.
        /// </summary>
        /// <param name="field">The field alias.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        bool TryGetField(TGeneric field, out object value);

        /// <summary>
        ///     Attempts to retrieve the specified field of the specified entity from the internal cache.
        ///     No database hit will be made if the field is not currently cached.
        /// </summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="field">The field alias.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the field value was found; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetField<T>(TGeneric field, out T value);
    }
}