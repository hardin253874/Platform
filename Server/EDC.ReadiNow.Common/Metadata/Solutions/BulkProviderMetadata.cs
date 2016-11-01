// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using EDC.ReadiNow.Diagnostics;
using EDC.Xml;

namespace EDC.ReadiNow.Metadata.Solutions
{
	/// <summary>
	///     Bulk Provider metadata.
	/// </summary>
	public class BulkProviderMetadata
	{
		/// <summary>
		///     The default batch size
		/// </summary>
		public const int DefaultBatchSize = 10000;

		/// <summary>
		///     Prevents a default instance of the <see cref="BulkProviderMetadata" /> class from being created.
		/// </summary>
		private BulkProviderMetadata( )
		{
			Columns = new List<DataColumn>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="BulkProviderMetadata" /> class.
		/// </summary>
		/// <param name="metadataNode">The metadata node.</param>
		/// <exception cref="System.ArgumentNullException">metadataNode</exception>
		public BulkProviderMetadata( XmlNode metadataNode )
			: this( )
		{
			if ( metadataNode == null )
			{
				throw new ArgumentNullException( "metadataNode" );
			}

			/////
			// Get the table name.
			/////
			string tableName = XmlHelper.ReadAttributeString( metadataNode, "@tableName", null );

			if ( string.IsNullOrEmpty( tableName ) )
			{
				EventLog.Application.WriteError( "Unable to perform bulk insert. The target name is missing. (Configuration: {0}, Xml: {1}).", metadataNode.OwnerDocument == null ? "<Unknown>" : metadataNode.OwnerDocument.BaseURI, metadataNode.ParentNode == null ? "<Unknown" : metadataNode.ParentNode.OuterXml );
				throw new ArgumentException( @"Missing Target Attribute.", "metadataNode" );
			}

			TableName = tableName;

			/////
			// Get the batch size.
			/////
			string batchSizeString = XmlHelper.ReadAttributeString( metadataNode, "@batchSize", null );

			int batchSize = DefaultBatchSize;

			if ( !string.IsNullOrEmpty( batchSizeString ) )
			{
				int parseValue;

				if ( int.TryParse( batchSizeString, out parseValue ) )
				{
					batchSize = parseValue;
				}
			}

			BatchSize = batchSize;

			/////
			// Get the schema.
			/////
			XmlNode schema = XmlHelper.SelectSingleNode( metadataNode, "schema" );

			if ( schema == null )
			{
				EventLog.Application.WriteError( "Unable to perform bulk insert. The target schema is missing. (Configuration: {0}, Xml: {1}).", metadataNode.OwnerDocument == null ? "<Unknown>" : metadataNode.OwnerDocument.BaseURI, metadataNode.ParentNode == null ? "<Unknown" : metadataNode.ParentNode.OuterXml );
				throw new ArgumentException( @"Missing Schema Node.", "metadataNode" );
			}

			XmlNodeList columnNodes = XmlHelper.SelectNodes( schema, "column" );

			if ( columnNodes == null || columnNodes.Count <= 0 )
			{
				EventLog.Application.WriteError( "Unable to perform bulk insert. The target schema contains no columns. (Configuration: {0}, Xml: {1}).", metadataNode.OwnerDocument == null ? "<Unknown>" : metadataNode.OwnerDocument.BaseURI, metadataNode.ParentNode == null ? "<Unknown" : metadataNode.ParentNode.OuterXml );
				throw new ArgumentException( @"Missing Column Node.", "metadataNode" );
			}

			/////
			// Process the columns.
			/////
			foreach ( XmlNode columnNode in columnNodes )
			{
				string name = XmlHelper.ReadAttributeString( columnNode, "@name", null );
				string typeString = XmlHelper.ReadAttributeString( columnNode, "@type", null );
				string nullableString = XmlHelper.ReadAttributeString( columnNode, "@nullable", null );

				/////
				// Get the column type.
				/////
				Type type = ConvertToType( typeString );

				if ( type == null )
				{
					EventLog.Application.WriteError( "Unable to perform bulk insert. Unknown target schema column type '{2}'. (Configuration: {0}, Xml: {1}).", metadataNode.OwnerDocument == null ? "<Unknown>" : metadataNode.OwnerDocument.BaseURI, metadataNode.ParentNode == null ? "<Unknown" : metadataNode.ParentNode.OuterXml, typeString );
					throw new TypeLoadException( string.Format( "Unknown column type. '{0}'", typeString ) );
				}

				bool nullable;

				bool.TryParse( nullableString, out nullable );

				/////
				// Create the column.
				/////
				var column = new DataColumn( name, type )
					{
						AllowDBNull = nullable
					};

				/////
				// Column ordinals should match the order they are added to this collection.
				/////
				Columns.Add( column );
			}
		}

		/// <summary>
		///     Gets the size of the batch.
		/// </summary>
		/// <value>
		///     The size of the batch.
		/// </value>
		public int BatchSize
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the columns.
		/// </summary>
		/// <value>
		///     The columns.
		/// </value>
		public List<DataColumn> Columns
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name of the table.
		/// </summary>
		/// <value>
		///     The name of the table.
		/// </value>
		public string TableName
		{
			get;
			private set;
		}

		/// <summary>
		///     Converts the type of the automatic.
		/// </summary>
		/// <param name="typeName">Name of the type.</param>
		/// <returns></returns>
		private static Type ConvertToType( string typeName )
		{
			if ( string.IsNullOrEmpty( typeName ) )
			{
				return null;
			}

			return Type.GetType( typeName, false );
		}
	}
}