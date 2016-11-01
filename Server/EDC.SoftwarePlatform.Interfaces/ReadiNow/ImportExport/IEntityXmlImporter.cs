// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Xml;

namespace ReadiNow.ImportExport
{
    /// <summary>
    ///     Interface for importing entities from the system as XML.
    /// </summary>
    public interface IEntityXmlImporter
    {
        /// <summary>
        /// Interface for providing XML import.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        EntityXmlImportResult ImportXml( XmlReader xmlReader, EntityXmlImportSettings settings );

        /// <summary>
        /// Interface for providing XML import.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="settings">Export settings.</param>
        EntityXmlImportResult ImportXml( string xml, EntityXmlImportSettings settings );
    }
}
