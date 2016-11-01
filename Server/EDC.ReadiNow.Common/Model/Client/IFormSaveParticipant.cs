// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EDC.ReadiNow.Model;


namespace EDC.ReadiNow.Model.Client
{
    public interface IFormSaveParticipant
    {
		/// <summary>
		/// Executed before the create occurs.
		/// </summary>
		/// <param name="entity">The entity.</param>
        void BeforeCreate(EntityData entity);

		/// <summary>
		/// Executed before the update occurs.
		/// </summary>
		/// <param name="entity">The entity.</param>
        void BeforeUpdate(EntityData entity);

		/// <summary>
		/// Executed after the create occurs.
		/// </summary>
		/// <param name="entityRef">The id of the created item</param>
		/// <param name="newFields">The new fields.</param>
		/// <param name="newRelationships">The new relationships.</param>
        void AfterCreate(EntityRef entityRef, IEnumerable<EntityRef> newFields, IEnumerable<EntityRef> newRelationships);

		/// <summary>
		/// Executed after the create occurs.
		/// </summary>
		/// <param name="entityRef">The id of the saved item</param>
		/// <param name="updatedFields">The updated fields.</param>
		/// <param name="updateRelationships">The update relationships.</param>
        void AfterUpdate(EntityRef entityRef, IEnumerable<EntityRef> updatedFields, IEnumerable<EntityRef> updateRelationships);

        /// <summary>
        /// This is used to evaluate the order interrogating participants. Lowest number first.
        /// </summary>
        /// <returns></returns>
        float Ordinal { get; }
    }
}
