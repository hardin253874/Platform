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
        /// <param name="xmlReader">XML reader to read content from.</param>
        /// <param name="settings">Export settings.</param>
        EntityXmlImportResult ImportXml( XmlReader xmlReader, EntityXmlImportSettings settings );

        /// <summary>
        /// Interface for providing XML import.
        /// </summary>
        /// <param name="xml">XML content to import.</param>
        /// <param name="settings">Export settings.</param>
        EntityXmlImportResult ImportXml( string xml, EntityXmlImportSettings settings );
    }
}
