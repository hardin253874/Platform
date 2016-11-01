// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Subject permission tuple.
    /// </summary>
    public class SubjectPermissionTuple : Tuple<long, long>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="permissionId"></param>
        public SubjectPermissionTuple(long subjectId, long permissionId) : base(subjectId, permissionId)
        {
            if (subjectId < 0)
            {
                throw new ArgumentNullException(nameof(subjectId));
            }

            if (permissionId <= 0)
            {
                throw new ArgumentNullException(nameof(permissionId));
            }
        }

        /// <summary>
        ///     Gets the subject id.
        /// </summary>
        public long SubjectId => Item1;

        /// <summary>
        ///     Gets the permission id.
        /// </summary>
        public long PermissionId => Item2;
    }
}