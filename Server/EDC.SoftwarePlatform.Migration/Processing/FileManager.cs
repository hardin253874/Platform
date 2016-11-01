// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Sources;
using EDC.SoftwarePlatform.Migration.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using EDC.SoftwarePlatform.Migration.Processing.Xml;
using EDC.SoftwarePlatform.Migration.Processing.Xml.Version1;
using EDC.SoftwarePlatform.Migration.Targets;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    ///     Helper methods for managing .db and .xml files.
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        ///     Default file format for exported files.
        /// </summary>
        public static readonly Format DefaultFormat = Format.Xml;

        /// <summary>
        /// Creates the data source.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Application file format not supported.</exception>
        internal static IDataSource CreateDataSource( string path )
        {
            Format format = GetImportFileFormat( path );

            IDataSource source = null;

            switch ( format )
            {
                case Format.Sqlite:
                    /////
                    // Check the file format
                    /////
                    SqliteStorageProvider sqliteStorageProvider;

                    if ( IsValidSqliteFile( path, out sqliteStorageProvider ) )
                    {
                        /////
                        // Create source to load app data from tenant
                        /////
                        source = new SqLiteSource
                        {
                            StorageProvider = sqliteStorageProvider
                        };
                    }
                    break;

                case Format.Xml:
                case Format.XmlVer2:
                    /////
                    // Create source to load app data from tenant
                    /////
                    source = new XmlPackageSource
                    {
                        XmlReader = XmlReader.Create( path ),
                        Version = format
                    };
                    break;
                default:
                    throw new InvalidOperationException( "Application file format not supported." );
            }

            return source;
        }

        /// <summary>
        /// Creates the data target.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="path">The path.</param>
        /// <param name="versionOverride">The version override.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Invalid storage format specified.</exception>
        internal static IDataTarget CreateDataTarget( Format format, string path, string versionOverride = null )
        {
            if ( format == Format.Undefined )
            {
                format = DefaultFormat;
            }

            IDataTarget target;

            switch ( format )
            {
                case Format.Sqlite:
                    target = new SqLiteTarget
                    {
                        StorageProvider = SqliteStorageProvider.CreateNewDatabase( path )
                    };
                    break;

                case Format.Xml:
                case Format.XmlVer2:
                    XmlWriterSettings settings = EntityXmlExporter.GetXmlWriterSettings( );

                    /////
                    // Create target to write to Xml file
                    /////
                    XmlWriter writer = XmlWriter.Create( path, settings );

                    target = new XmlPackageTarget
                    {
                        XmlWriter = writer,
                        NameResolver = new UpgradeMapNameResolver( ),
                        VersionOverride = versionOverride,
                        Format = format
                    };
                    break;
                default:
                    throw new InvalidOperationException( "Invalid storage format specified." );
            }

            return target;
        }

        /// <summary>
        /// Gets the file format.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static Format GetImportFileFormat( string packagePath )
        {
            if ( string.IsNullOrEmpty( packagePath ) )
            {
                throw new ArgumentNullException( nameof( packagePath ) );
            }

            if ( !File.Exists( packagePath ) )
            {
                throw new FileNotFoundException( $"Fail '{packagePath}' was not found.", packagePath );
            }

            FileInfo info = new FileInfo( packagePath );

            string extension = info.Extension;

            if ( string.Equals( extension, ".db", StringComparison.OrdinalIgnoreCase ) )
            {
                return Format.Sqlite;
            }

            if ( string.Equals( extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
            {
                Format format = DetermineXmlFileVersion( packagePath );
                return format;
            }

            return Format.Undefined;
        }

        /// <summary>
        /// Gets the file format to use when exporting.
        /// </summary>
        /// <param name="packagePath">The package path.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static Format GetExportFileFormat( string packagePath )
        {
            if ( string.IsNullOrEmpty( packagePath ) )
            {
                throw new ArgumentNullException( nameof( packagePath ) );
            }

            FileInfo info = new FileInfo( packagePath );

            string extension = info.Extension;

            if ( string.Equals( extension, ".db", StringComparison.OrdinalIgnoreCase ) )
            {
                return Format.Sqlite;
            }

            if ( string.Equals( extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
            {
                return DefaultFormat;
            }

            return Format.Undefined;
        }

        /// <summary>
        /// Determines whether [is valid XML file] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>Version string, or null if invalid.</returns>
        private static Format DetermineXmlFileVersion( string path )
        {
            int limit = 100;

            using ( XmlReader reader = XmlReader.Create( path ) )
            {
                while ( reader.Read( ) )
                {
                    if ( reader.NodeType == XmlNodeType.Element )
                    {
                        if ( reader.Name != XmlConstants.Xml )
                            return Format.Undefined; // invalid
                        string version = reader.GetAttribute( XmlConstants.Version );

                        if ( version == "1.0" )
                            return Format.Xml;
                        if ( version == "2.0" )
                            return Format.XmlVer2;

                        return Format.Undefined;
                    }
                    if ( limit-- <= 0 )
                        break;
                }
            }
            return Format.Undefined;
        }

        /// <summary>
        /// Determines whether the specified sqlite file is valid.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private static bool IsValidSqliteFile( string path, out SqliteStorageProvider provider )
        {
            provider = new SqliteStorageProvider( path );

            if ( provider.IsValid( ) )
            {
                return true;
            }

            provider.Dispose( );
            provider = null;

            return false;
        }
    }
}
