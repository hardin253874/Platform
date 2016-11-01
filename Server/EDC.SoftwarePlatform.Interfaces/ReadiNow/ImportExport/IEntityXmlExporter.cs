// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Xml;
using System.Collections.Generic;

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
        /// <param name="entityIds">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        void GenerateXml( IEnumerable<long> entityIds, XmlWriter xmlWriter, EntityXmlExportSettings settings );

        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="entityIds">ID of entity to export.</param>
        /// <param name="settings">Export settings.</param>
        string GenerateXml( IEnumerable<long> entityIds, EntityXmlExportSettings settings );
    }


    /// <summary>
    /// Extension methods to extend interface.
    /// </summary>
    public static class EntityXmlExporterExtensions
    {
        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="exporter">The exporter</param>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="xmlWriter">Xml Writer to write the exported entity to.</param>
        /// <param name="settings">Export settings.</param>
        public static void GenerateXml( this IEntityXmlExporter exporter, long entityId, XmlWriter xmlWriter, EntityXmlExportSettings settings )
        {
            exporter.GenerateXml( new[ ] { entityId }, xmlWriter, settings );
        }


        /// <summary>
        /// Interface for providing XML export.
        /// </summary>
        /// <param name="exporter">The exporter</param>
        /// <param name="entityId">ID of entity to export.</param>
        /// <param name="settings">Export settings.</param>
        public static string GenerateXml( this IEntityXmlExporter exporter, long entityId, EntityXmlExportSettings settings )
        {
            return exporter.GenerateXml( new[] { entityId }, settings );
        }

    }
}
