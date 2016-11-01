// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Xml;

namespace ReadiNow.ImportExport
{
    /// <summary>
    ///     Interface for exporting entities from the system as XML.
    /// </summary>
    public interface IEntityXmlExporter
    {
        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        void GenerateXml( long entityId, XmlWriter xmlWriter, EntityXmlExportSettings settings );

        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="settings">Export settings.</param>
        string GenerateXml( long entityId, EntityXmlExportSettings settings );
    }
}
