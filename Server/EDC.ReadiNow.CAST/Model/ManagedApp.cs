// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.CAST.Template.Model;
using System;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// An object on the marketplace that refers to an application that may be deployed to a tenant.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedAppSchema.ManagedAppType)]
    public class ManagedApp : StrongEntity, IManagedApp
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedApp" /> class.
        /// </summary>
        public ManagedApp() : base(typeof(ManagedApp)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedApp" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedApp(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the application.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedAppSchema.NameField); }
            set { SetField(ManagedAppSchema.NameField, value); }
        }

        /// <summary>
        /// The publisher of the application.
        /// </summary>
        public string Publisher
        {
            get { return (string)GetField(ManagedAppSchema.PublisherField); }
            set { SetField(ManagedAppSchema.PublisherField, value); }
        }

        /// <summary>
        /// The URL of the publisher of this application.
        /// </summary>
        public string PublisherUrl
        {
            get { return (string)GetField(ManagedAppSchema.PublisherUrlField); }
            set { SetField(ManagedAppSchema.PublisherUrlField, value); }
        }

        /// <summary>
        /// The release date of this application.
        /// </summary>
        public DateTime? ReleaseDate
        {
            get { return (DateTime?)GetField(ManagedAppSchema.ReleaseDateField); }
            set { SetField(ManagedAppSchema.ReleaseDateField, value); }
        }

        /// <summary>
        /// The identifier of the application that this entity refers to.
        /// </summary>
        public Guid? ApplicationId
        {
            get { return (Guid?)GetField(ManagedAppSchema.AppIdField); }
            set { SetField(ManagedAppSchema.AppIdField, value); }
        }

        /// <summary>
        /// The known versions of this application.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> Versions
        {
            get { return GetRelationships<ManagedAppVersion>(ManagedAppSchema.VersionsRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedAppSchema.VersionsRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// List of apps that this app requires to be installed.
        /// </summary>
        public IEntityCollection<ManagedApp> RequiredApps
        {
            get { return GetRelationships<ManagedApp>(ManagedAppSchema.IsRequiredByAppRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedAppSchema.IsRequiredByAppRelationship, value, Direction.Forward); }
        }

        /// <summary>
        /// List of app versions that this app requires to be installed.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> RequiredAppVersions
        {
            get { return GetRelationships<ManagedAppVersion>(ManagedAppSchema.RequiredAppVersionsRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedAppSchema.RequiredAppVersionsRelationship, value, Direction.Forward); }
        }

        #region Internals

        internal static string ManagedAppPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        ManagedAppSchema.PublisherField + "," +
                        ManagedAppSchema.PublisherUrlField + "," +
                        ManagedAppSchema.ReleaseDateField + "," +
                        ManagedAppSchema.AppIdField + "," +
                        ManagedAppSchema.VersionsRelationship + ".{alias,name,isOfType.{alias,name}," + ManagedAppVersionSchema.AppVersionIdField + "}," +
                        ManagedAppSchema.IsRequiredByAppRelationship + ".{alias,name,isOfType.{alias,name}," + ManagedAppSchema.AppIdField + "}," +
                        ManagedAppSchema.RequiredAppVersionsRelationship + ".{alias,name,isOfType.{alias,name}," + ManagedAppVersionSchema.AppVersionIdField + "}";
            }
        }

        #endregion
    }
}
