// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.Migration.Contract
{
    /// <summary>
    ///     Metadata about relationship types.
    /// </summary>
    public class RelationshipTypeEntry
    {
        /// <summary>
        ///     Relationship type ID.
        /// </summary>
        public Guid TypeId
        {
            get;
            set;
        }

        /// <summary>
        ///     Relationship type alias, or null.
        /// </summary>
        [CanBeNull]
        public string Alias
        {
            get;
            set;
        }

        /// <summary>
        ///     Relationship type reverse alias, or null.
        /// </summary>
        [CanBeNull]
        public string ReverseAlias
        {
            get;
            set;
        }

        /// <summary>
        ///     Relationship type alias, or null.
        /// </summary>
        [CanBeNull]
        public string Namespace
        {
            get;
            set;
        }

        /// <summary>
        ///     Relationship type
        /// </summary>
        public CloneActionEnum_Enumeration? CloneAction
        {
            get;
            set;
        }

        /// <summary>
        ///     Relationship type
        /// </summary>
        public CloneActionEnum_Enumeration? ReverseCloneAction
        {
            get;
            set;
        }
    }
}
