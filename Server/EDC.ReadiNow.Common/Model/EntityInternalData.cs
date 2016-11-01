// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Holds the data members of an entity instance.
    /// </summary>
    [Serializable]
    internal class EntityInternalData
    {
        #region Instance Members

        /// <summary>
        ///     Whether the instance has been disposed.
        /// </summary>
        public bool Disposed;

        /// <summary>
        ///     Whether this instance is read-only or not.
        /// </summary>
        public bool IsReadOnly = true;

        /// <summary>
        ///     This instances modification token.
        /// </summary>
        public IEntityModificationToken ModificationToken;

        /// <summary>
        ///     Mutable key.
        /// </summary>
        public IMutableIdKey MutableId;

        /// <summary>
        ///     Gets or sets the tenant unique identifier.
        /// </summary>
        /// <value>
        ///     The tenant unique identifier.
        /// </value>
        public long TenantId;

        /// <summary>
        ///     Gets or sets the clone option.
        /// </summary>
        /// <value>
        ///     The clone option.
        /// </value>
        public CloneOption CloneOption;

        /// <summary>
        ///     Gets or sets the clone source.
        /// </summary>
        /// <value>
        ///     The clone source.
        /// </value>
        public long? CloneSource;

        /// <summary>
        ///     A local cache of the TypeIDs. This is only set for read-only instances, and is only for performance.
        /// </summary>
        /// <value>
        ///     The TypeIDs, or null if they have not been cached (e.g. on writable entities).
        /// </value>
        public IReadOnlyCollection<long> TypeIdsEntityLocalCopy;


        #endregion Instance Members

        /// <summary>
        /// Clone the data.
        /// </summary>
        /// <returns></returns>
        internal EntityInternalData Clone()
        {
            return (EntityInternalData)MemberwiseClone();
        }
    }
}
