// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.CAST.Template.Model;
using System;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// The managed platform object.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedPlatformSchema.ManagedPlatformType)]
    public class ManagedPlatform : StrongEntity, IManagedPlatform
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedPlatform" /> class.
        /// </summary>
        public ManagedPlatform() : base(typeof(ManagedPlatform)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedPlatform" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedPlatform(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name given to the platform instance.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedPlatformSchema.NameField); }
            set { SetField(ManagedPlatformSchema.NameField, value); }
        }

        /// <summary>
        /// The unique identifier of the database instance belonging to this platform.
        /// </summary>
        public string DatabaseId
        {
            get { return (string)GetField(ManagedPlatformSchema.DatabaseIdField); }
            set { SetField(ManagedPlatformSchema.DatabaseIdField, value); }
        }

        /// <summary>
        /// The time that this platform was last heard from.
        /// </summary>
        public DateTime? LastContact
        {
            get { return (DateTime?)GetField(ManagedPlatformSchema.LastContactField); }
            set { SetField(ManagedPlatformSchema.LastContactField, value); }
        }

        /// <summary>
        /// The versions of applications that this platform has available to it.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> AvailableAppVersions
        {
            get { return GetRelationships<ManagedAppVersion>(ManagedPlatformSchema.HasAppVersionsRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedPlatformSchema.HasAppVersionsRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// The tenants known to exist on this platform.
        /// </summary>
        public IEntityCollection<ManagedTenant> ContainsTenants
        {
            get { return GetRelationships<ManagedTenant>(ManagedPlatformSchema.ContainsTenantRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedPlatformSchema.ContainsTenantRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// Details of the database installations that this platform has been connected to.
        /// </summary>
        public IEntityCollection<PlatformDatabase> DatabaseHistory
        {
            get { return GetRelationships<PlatformDatabase>(ManagedPlatformSchema.DatabaseHistoryRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedPlatformSchema.DatabaseHistoryRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// Details of the frontend installations that this platform has been connected with.
        /// </summary>
        public IEntityCollection<PlatformFrontEnd> FrontEndHistory
        {
            get { return GetRelationships<PlatformFrontEnd>(ManagedPlatformSchema.FrontEndHistoryRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedPlatformSchema.FrontEndHistoryRelationship, value, Direction.Forward); }
        }

        #region Internals

        internal static string ManagedPlatformPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        ManagedPlatformSchema.DatabaseIdField + "," +
                        ManagedPlatformSchema.LastContactField + "," +
                        ManagedPlatformSchema.HasAppVersionsRelationship + ".{" + ManagedAppVersion.ManagedAppVersionPreloadQuery + "}," +
                        ManagedPlatformSchema.ContainsTenantRelationship + ".{" + ManagedTenant.ManagedTenantPreloadQuery + "}," +
                        ManagedPlatformSchema.DatabaseHistoryRelationship + ".{" + PlatformDatabase.PlatformDatabasePreloadQuery + "}," +
                        ManagedPlatformSchema.FrontEndHistoryRelationship + ".{" + PlatformFrontEnd.PlatformFrontEndPreloadQuery + "}";
            }
        }

        #endregion
    }
}
