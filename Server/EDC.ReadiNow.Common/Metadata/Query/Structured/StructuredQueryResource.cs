// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Xml;
using EDC.Core;
using System.IO;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// A structured query, stored as a resource.
    /// </summary>
    public class StructuredQueryResource
    {
		/// <summary>
		/// Performs any special processing when the resource is reconstructed from its XML image.
		/// </summary>
		/// <param name="node">An XML node containing the resource's XML image.</param>
        protected void OnFromXml(System.Xml.XmlNode node )
        {
            XmlNamespaceManager namespaces = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            namespaces.AddNamespace("q", Constants.StructuredQueryNamespace);
            XmlNode queryNode = node.SelectSingleNode("q:Query", namespaces);

            StructuredQueryHelper.FromXml(queryNode);
        }

		/// <summary>
		/// Performs any special processing when the resource is serialized into XML.
		/// </summary>
		/// <param name="xmlWriter">The writer used to write the image.</param>
        protected void OnToXml(System.Xml.XmlWriter xmlWriter )
        {
            if (this.StructuredQuery != null)
            {
                StructuredQueryHelper.ToXml(xmlWriter, this.StructuredQuery);
            }
        }

		/// <summary>
		/// Creates an XML image of the resource and its state.
		/// </summary>
		/// <returns>
		/// A string containing an XML image of the resource and its state.
		/// </returns>
		/// <remarks>
		/// To minimize database storage size, please ensure that this method only serializes properties
		/// that are different from the default value(s).
		/// </remarks>
		public virtual string ToXml( )
		{
			string image = string.Empty;

			// Create the XML writer
			using ( StringWriter writer = new StringWriter( ) )
			{
				using ( XmlTextWriter xmlWriter = new XmlTextWriter( writer ) )
				{
					xmlWriter.Formatting = Formatting.Indented;

					xmlWriter.WriteStartElement( "resource" );

					this.OnToXml( xmlWriter );

					xmlWriter.WriteEndElement( );

					image = writer.ToString( );
				}
			}

			return image;
		}

        /// <summary>
        /// Gets or sets the structured query.
        /// </summary>
        /// <value>
        /// The structured query.
        /// </value>
        public StructuredQuery StructuredQuery { get; set; }
    }
}
