// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Model
{
    public interface IFormSaveParticipant
    {
        /// <summary>
        /// Executed before the create occurs.
        /// </summary>
        /// <param name="entityRef"></param>
        void BeforeCreate(EntityData entity);

        /// <summary>
        /// Executed before the update occurs.
        /// </summary>
        /// <param name="entityRef"></param>
        void BeforeUpdate(EntityData entity);

        /// <summary>
        /// Executed after the create occurs.
        /// </summary>
        /// <param name="entityRef">The id of the created item</param>
        void AfterCreate(EntityRef entityRef);

        /// <summary>
        /// Executed after the create occurs.
        /// </summary>
        /// <param name="entityRef">The id of the saved item</param>
        void AfterUpdate(EntityRef entityRef);

        /// <summary>
        /// This is used to evalate the order interogating participants. Lowest number first.
        /// </summary>
        /// <returns></returns>
        float Ordinal { get; }
    }
}
