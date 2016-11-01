// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Model
{
    /// <summary>
    /// Holds information about a database connected to a <see cref="ManagedPlatform"/>.
    /// </summary>
    [Serializable]
    [ModelClass(PlatformDatabaseSchema.PlatformDatabaseType)]
    public class PlatformDatabase : StrongEntity, IPlatformDatabase
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformDatabase" /> class.
        /// </summary>
        public PlatformDatabase() : base(typeof(PlatformDatabase)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformDatabase" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal PlatformDatabase(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name given to the entity.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(PlatformDatabaseSchema.NameField); }
            set { SetField(PlatformDatabaseSchema.NameField, value); }
        }

        /// <summary>
        /// The database catalog.
        /// </summary>
        public string Catalog
        {
            get { return (string)GetField(PlatformDatabaseSchema.CatalogField); }
            set { SetField(PlatformDatabaseSchema.CatalogField, value); }
        }

        /// <summary>
        /// The name of the sql server that held the catalog.
        /// </summary>
        public string Server
        {
            get { return (string)GetField(PlatformDatabaseSchema.ServerField); }
            set { SetField(PlatformDatabaseSchema.ServerField, value); }
        }

        /// <summary>
        /// The last time that the database with this information was registered.
        /// </summary>
        public DateTime LastContact
        {
            get { return (DateTime)GetField(PlatformDatabaseSchema.LastContactField); }
            set { SetField(PlatformDatabaseSchema.LastContactField, value); }
        }

        #region Internals

        internal static string PlatformDatabasePreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        PlatformDatabaseSchema.CatalogField + "," +
                        PlatformDatabaseSchema.ServerField + "," +
                        PlatformDatabaseSchema.LastContactField;
            }
        }

        #endregion
    }
}
