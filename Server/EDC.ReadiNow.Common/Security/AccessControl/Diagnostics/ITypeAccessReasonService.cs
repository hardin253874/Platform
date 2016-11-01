// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ReadiNow.Annotations;
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    /// <summary>
    /// Service that will list access.
    /// </summary>
    public interface ITypeAccessReasonService
    {
        /// <summary>
        /// Return the list of all objects that the subject has access to, and the reason for the access.
        /// </summary>
        /// <param name="subjectId">The role or user </param>
        /// <returns>List of access reasons.</returns>
        IReadOnlyList<AccessReason> GetTypeAccessReasons( long subjectId, [NotNull] TypeAccessReasonSettings settings );
    }


    /// <summary>
    /// Settings for evaluating type access.
    /// </summary>
    public class TypeAccessReasonSettings
    {
        /// <summary>
        /// Default settings.
        /// </summary>
        public static TypeAccessReasonSettings Default = new TypeAccessReasonSettings( true );

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="advancedTypes">If true, advanced types are included. If false, they are not.</param>
        public TypeAccessReasonSettings( bool advancedTypes )
        {
            AdvancedTypes = advancedTypes;
        }

        /// <summary>
        /// If true, advanced types are included. If false, they are not.
        /// Advanced types includes 'managedType' and the root 'resource' type. Otherwise only definitions are shown. 
        /// </summary>
        public bool AdvancedTypes { get; }
    }
}
