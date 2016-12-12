// Copyright 2011-2016 Global Software Innovation Pty Ltd
extern alias EdcReadinowCommon;
using Profiler = EdcReadinowCommon::EDC.ReadiNow.Diagnostics.Profiler;
using SecurityBypassContext = EdcReadinowCommon::EDC.ReadiNow.Security.SecurityBypassContext;

using System;
using System.Xml;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Main entry point for generating documents.
    /// </summary>
    public class Generator : IDocumentGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="externalServices">Contains references to external services.</param>
        public Generator(ExternalServices externalServices)
        {
            if (externalServices == null)
                throw new ArgumentNullException("externalServices");
            ExternalServices = externalServices;
        }


        /// <summary>
        /// Contains references to external services.
        /// </summary>
        public ExternalServices ExternalServices { get; }


        /// <summary>
        /// Generate a document from a template.
        /// </summary>
        /// <param name="templateStream">File stream of the template Word document.</param>
        /// <param name="outputStream">Output stream to receive the generated document.</param>
        /// <param name="settings">Various settings to pass into generation.</param>
        public void CreateDocument( Stream templateStream, Stream outputStream, GeneratorSettings settings = null)
        {
            if ( templateStream == null )
                throw new ArgumentNullException( nameof( templateStream ) );
            if ( outputStream == null )
                throw new ArgumentNullException( nameof( outputStream ) );

            using (Profiler.Measure("Generator.CreateDocument"))
            {
                if (settings == null)
                    settings = new GeneratorSettings();

                // First copy the template to the output, and then modify the output
                settings.UpdateProgress("Copying template");
                templateStream.CopyTo(outputStream);

                // Open both streams for Open XML processing
                settings.UpdateProgress("Reading template");
                using (WordprocessingDocument sourceDoc = WordprocessingDocument.Open(templateStream, false))
                using (WordprocessingDocument targetDoc = WordprocessingDocument.Open(outputStream, true))
                {
                    var sourceBody = GetBody(sourceDoc);
                    var sourceBodyHost = sourceBody?.Parent;
                    if ( sourceBodyHost == null )
                        throw new Exception( "Source document body host not found." );

                    var targetBodyHost = GetBody(targetDoc)?.Parent;
                    if ( targetBodyHost == null )
                        throw new Exception( "Source document body not found." );

                    if (settings.WriteDebugFiles)
                        DebugXml(sourceBody.OuterXml, @"template.xml");

                    // Purge the document body of the target document
                    targetBodyHost.RemoveAllChildren();

                    // Build instruction tree from the source stream
                    settings.UpdateProgress("Parsing template");
                    OpenXmlReader reader = new OpenXmlReader( ExternalServices )
                    {
                        ReaderContext = new ReaderContext( ),
                        GeneratorSettings = settings
                    };

                    TemplateData template;

                    using (new SecurityBypassContext())
                    {
                        template = reader.BuildInstructionTree(sourceBody);
                    }

                    Instruction rootInstruction = template.RootInstruction;
                    if ( rootInstruction == null )
                        throw new Exception( "Root instruction not found." );

                    settings.UpdateProgress("Writing instructions debug");
                    if (settings.WriteDebugFiles)
                        DebugInstruction(rootInstruction);

                    // Run instructions and write to the result stream
                    settings.UpdateProgress("Generating result");
                    OpenXmlWriter writer = new OpenXmlWriter(targetBodyHost, sourceBodyHost);
                    WriterContext context = new WriterContext {
                        ExternalServices = ExternalServices,
                        Writer = writer,
                        Template = template,
                        Settings = settings };
                    rootInstruction.Generate(context);
                }
            }
        }


        /// <summary>
        /// Locate the OpenXml document body.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>The body element.</returns>
        static Body GetBody(WordprocessingDocument doc)
        {
            var mainPart = doc.MainDocumentPart;
            var document = mainPart.Document;
            var body = document.Body;
            return body;
        }

        static void DebugInstruction(Instruction instruction)
        {
            using (TextWriter writer = new StreamWriter(@"instruction.txt"))
            {
                instruction.Debug(writer, 0);
            }
        }

        static void DebugXml(string xml, string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };
            XmlWriter w = XmlWriter.Create(path, settings);
            doc.WriteTo(w);
            w.Flush();
        }
    }

}
