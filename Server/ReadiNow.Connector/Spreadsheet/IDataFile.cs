// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using ReadiNow.Connector.ImportSpreadsheet;

namespace ReadiNow.Connector.Spreadsheet
{
    /// <summary>
    /// Interface for interracting with an imported data file, such as a spreadsheet.
    /// </summary>
    public interface IDataFile : IObjectsReader
    {
        /// <summary>
        /// Return a list of sheets/parts. Or null if not applicable.
        /// </summary>
        /// <returns>List of sheets, or null.</returns>
        IReadOnlyList<SheetInfo> GetSheets( );

        /// <summary>
        /// Read the metadata for a particular sheet.
        /// </summary>
        /// <remarks>
        /// Can only be called once, and must be called before <see cref="IObjectsReader.GetObjects"/>
        /// </remarks>
        /// <returns></returns>
        SheetMetadata ReadMetadata( );
    }


    /// <summary>
    /// Spreadsheet metadata.
    /// </summary>
    public class SheetMetadata
    {
        public IReadOnlyList<FieldMetadata> Fields { get; set; }
    }


    /// <summary>
    /// Column metadata
    /// </summary>
    public class FieldMetadata
    {
        /// <summary>
        /// The reference to the field - e.g. cell column
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The title of the field - e.g. the heading row for the column
        /// </summary>
        public string Title { get; set; }
    }
}
