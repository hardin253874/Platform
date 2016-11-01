// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// An object on the marketplace that describes a particular version of an application that may be deployed to a tenant.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedAppVersionSchema.ManagedAppVersionType)]
    public class ManagedAppVersion : StrongEntity, IManagedAppVersion
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAppVersion" /> class.
        /// </summary>
        public ManagedAppVersion() : base(typeof(ManagedAppVersion)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedAppVersion" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedAppVersion(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the application version.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedAppVersionSchema.NameField); }
            set { SetField(ManagedAppVersionSchema.NameField, value); }
        }

        /// <summary>
        /// The version information.
        /// </summary>
        public string Version
        {
            get { return (string)GetField(ManagedAppVersionSchema.VersionField); }
            set { SetField(ManagedAppVersionSchema.VersionField, value); }
        }
        
        /// <summary>
        /// The publish date of this application version.
        /// </summary>
        public DateTime? PublishDate
        {
            get { return (DateTime?)GetField(ManagedAppVersionSchema.PublishDateField); }
            set { SetField(ManagedAppVersionSchema.PublishDateField, value); }
        }

        /// <summary>
        /// The identifier of the application version that this entity refers to.
        /// </summary>
        public Guid? VersionId
        {
            get { return (Guid?)GetField(ManagedAppVersionSchema.AppVersionIdField); }
            set { SetField(ManagedAppVersionSchema.AppVersionIdField, value); }
        }

        /// <summary>
        /// The application this this is a version of.
        /// </summary>
        public IManagedApp Application
        {
            get { return GetLookup<ManagedApp>(ManagedAppVersionSchema.ApplicationLookup, Direction.Reverse); }
            set { SetLookup(ManagedAppVersionSchema.ApplicationLookup, (ManagedApp)value, Direction.Reverse); }
        }

        /// <summary>
        /// List of apps that this app version requires to be installed.
        /// </summary>
        public IEntityCollection<ManagedApp> RequiredApps
        {
            get { return GetRelationships<ManagedApp>(ManagedAppVersionSchema.RequiredAppsRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedAppVersionSchema.RequiredAppsRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// List of app versions that this app version requires to be installed.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> RequiredAppVersions
        {
            get { return GetRelationships<ManagedAppVersion>(ManagedAppVersionSchema.IsRequiredByAppVersionRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedAppVersionSchema.IsRequiredByAppVersionRelationship, value, Direction.Forward); }
        }

        #region Internals

        internal static string ManagedAppVersionPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        ManagedAppVersionSchema.VersionField + "," +
                        ManagedAppVersionSchema.PublishDateField + "," +
                        ManagedAppVersionSchema.AppVersionIdField + "," +
                        ManagedAppVersionSchema.ApplicationLookup + ".{" + ManagedApp.ManagedAppPreloadQuery + "}," +
                        ManagedAppVersionSchema.RequiredAppsRelationship + ".{" + ManagedApp.ManagedAppPreloadQuery + "}," +
                        ManagedAppVersionSchema.IsRequiredByAppVersionRelationship + ".{alias,name,isOfType.{alias,name}," + ManagedAppVersionSchema.AppVersionIdField + "}";
            }
        }

        #endregion
    }
}
