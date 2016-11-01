// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ReadiNow.ValidateConfig
{
    class Program
    {
        static TextWriter writer = Console.Error;
        
        static void Main(string[] args)
        {
            string path;
            if (args.Length == 0)
                path = Environment.CurrentDirectory;
            else
                path = args[0];

            path = Path.GetFullPath(path);

            // Load all XSD
            string[] xsdFiles = Directory.GetFiles(path, "*.xsd");
            XmlSchemaSet schemas = new XmlSchemaSet();

            foreach (string xsdPath in xsdFiles)
            {
                using (Stream schemaStream = File.OpenRead(xsdPath))
                {
                    XmlSchema schema = XmlSchema.Read(schemaStream, delegate (Object sender, ValidationEventArgs e)
                    {
                        ValidateSchema(sender, e, xsdPath);
                    });
                    schemas.Add(schema);
                }
            }

            // Process all XML
            string[] xmlFiles = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
            foreach (string xmlPath in xmlFiles)
            {
                XDocument doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
                doc.Validate(schemas, delegate (Object sender, ValidationEventArgs e)
                {
                    ValidateSchema(sender, e, xmlPath);
                });
            }
        }

        private static void ValidateSchema(Object sender, ValidationEventArgs e, string xmlPath)
        {
            string message = e.Message;

            // Truncate the 'list of possible' messages, because they're too verbose.
            int i = e.Message.IndexOf(" List of possible");
            if (i > 0)
            {
               message = message.Substring(0, i);
            }
            
            // Formatted for Visual studio
            writer.WriteLine("{0}({1},{2}): {3} E1: XSD {4}",
                xmlPath,
                e.Exception.LineNumber,
                e.Exception.LinePosition,
                e.Severity.ToString().ToLower(),
                message);
        }
    }
}
