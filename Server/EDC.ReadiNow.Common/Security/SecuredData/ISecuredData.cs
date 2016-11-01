// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// Provide a way of securing data so that it is not stored in plain text.
    /// </summary>
    public interface ISecuredData
    {
        /// <summary>
        /// Create a secured data value.
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="value">The value to be stored</param>
        /// <returns>The secureId for the new value</returns>
        /// <exception cref="ArgumentNullException"
        Guid Create(long tenantId, string context, string value);

        /// <summary>
        /// Set a value into secured data.
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <param name="value">The value to be stored</param>
        /// <exception cref="SecureIdNotFoundException">Thrown if the securedId is not found</exception>
        /// <exception cref="ArgumentNullException">Thrown if a null value is provided.</exception>
        /// <exception cref="ArgumentException">Thrown if an empty securedId is provided</exception>
        void Update(Guid secureId, string value);

        /// <summary>
        /// Get a value stored securely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <returns>The secured value, null if no value </returns>
        /// <exception cref="SecureIdNotFoundException">Thrown if the securedId is not found</exception>
        /// <exception cref="ArgumentException">Thrown if an empty securedId is provided</exception>
        string Read(Guid secureId);



        /// <summary>
        /// Clear a value stored in secured data
        /// </summary>
        /// <param name="tenantId">The tenant Id</param>
        /// <param name="context">A string representing the context of the storage.</param>
        /// <param name="secureId">The secured Id as proved by the Set call.</param>
        /// <exception cref="SecureIdNotFoundException">Thrown if the securedId is not found</exception>
        /// <exception cref="ArgumentException">Thrown if an empty securedId is provided</exception>
        void Delete(Guid secureId);
    }
}
