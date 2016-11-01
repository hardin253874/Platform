// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EDC.ReadiNow.XmlUtil
{
    /// <summary>
    /// Utility to locate entities in the config that have no alias, and generate an alias for them.
    /// </summary>
    class AliasAdder
    {
        XmlNamespaceManager _namespaces;
        ISet<string> _takenAliases;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="takenAliases">Aliases that have already been used.</param>
        public void AddMissingAliases(string path, ISet<string> takenAliases)
        {
            var header = CaptureHeader(path);

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(path);

            _takenAliases = takenAliases;
            _namespaces = new XmlNamespaceManager(doc.NameTable);
            _namespaces.AddNamespace("", "core");
            _namespaces.AddNamespace("q", "core");

            if (doc.DocumentElement.Name != "resources")
            {
                Console.WriteLine("Skipping " + path);
                return;
            }

            UpdateEntities(doc.DocumentElement, "");
            Console.WriteLine("Saving " + path);
            doc.Save(path);

            RewriteHeader(path, header);
        }

        
        /// <summary>
        /// Process any node that may contain multiple entities.
        /// (Either the document root, or a related entity container)
        /// </summary>
        void UpdateEntities(XmlElement elementContainer, string parentAlias)
        {
            foreach (XmlNode node in elementContainer.ChildNodes)
            {
                XmlElement entity = node as XmlElement;
                if (entity == null)
                    continue;

                // Xml Fields appear as relationships because they have child elements - so we need to explicitly skip over them.
                if (entity.Name == "xml")
                    continue;

                UpdateEntity(entity, parentAlias);
            }
        }

        /// <summary>
        /// Process an individual entity element.
        /// 1. Add an alias if it is missing.
        /// 2. Recursively process children (via relationships)
        /// </summary>
        void UpdateEntity(XmlElement entity, string parentAlias)
        {
            // Add alias if necessary
            XmlElement aliasNode = entity.SelectSingleNode("q:alias", _namespaces) as XmlElement;
            string alias;
            if (aliasNode == null)
            {
                string whitespace = null;
                XmlNode whitespaceNode = entity.FirstChild;
                if (whitespaceNode != null)
                    whitespace = whitespaceNode.InnerText;

                aliasNode = entity.OwnerDocument.CreateElement("alias", "core");
                alias = GenerateAlias(entity, parentAlias);
                aliasNode.InnerText = alias;
                entity.InsertAfter(aliasNode, null);

                if (whitespace != null)
                {
                    entity.InsertAfter(entity.OwnerDocument.CreateTextNode(whitespace), null);
                }
            }
            else
            {
                alias = aliasNode.InnerText;
            }

            // Test members
            foreach (XmlNode node in entity)
            {
                XmlElement member = node as XmlElement;
                if (member == null)
                    continue;

                // Determine if it is a relationship
                if (member.HasChildNodes)
                {
                    UpdateEntities(member, alias);
                }
            }
        }

        /// <summary>
        /// Select a suitable alias for the 
        /// </summary>
        /// <param name="entity">The element for the entity that needs to be named.</param>
        /// <returns>An alias</returns>
        string GenerateAlias(XmlElement entity, string parentAlias)
        {
            var nameNode = entity.SelectSingleNode("q:name", _namespaces);
            if (nameNode == null || string.IsNullOrEmpty(nameNode.InnerText))
            {
                return GenerateAlias();
            }

            // Get prefix
            string parentPrefix = "";
            string parentAliasPart = parentAlias;
            if (parentAlias.Contains(':'))
            {
                string[] parts = parentAlias.Split(':');
                parentPrefix = parts[0];
                parentAliasPart = parts[1];
            }
            
            // Pick a suitable name
            string name = nameNode.InnerText;
            string alias = GenerateAlias(name + " " + entity.Name);

            // Read prefix
            if (parentPrefix != "")
                alias = parentPrefix + ":" + alias;

            if (_takenAliases.Contains(alias))
            {
                int i = 2;
                while (true)
                {
                    string proposed = alias + i.ToString();
                    if (!_takenAliases.Contains(proposed))
                    {
                        alias = proposed;
                        break;
                    }
                    i++;
                }
            }

            _takenAliases.Add(alias);
            return alias;
        }

        /// <summary>
        /// Generate a potential alias from nothing.
        /// </summary>
        string GenerateAlias(string name)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            bool newWord = false;
            foreach (char ch in name)
            {
                if (!char.IsLetterOrDigit(ch))
                {
                    newWord = true;
                    continue;
                }
                if (first)
                {
                    if (char.IsDigit(ch))
                        sb.Append("a");
                    sb.Append(char.ToLower(ch));
                    first = false;
                    newWord = false;
                }
                else if (newWord)
                {
                    sb.Append(char.ToUpper(ch));
                    newWord = false;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            string result = sb.ToString();
            if (result == "")
            {
                return GenerateAlias();
            }
            return result;

        }

        /// <summary>
        /// Generate an alias from nothing.
        /// </summary>
        string GenerateAlias()
        {
            return "auto" + Guid.NewGuid().ToString().Replace("-", "");
        }

        // Hack to preserve layout of document header (which I like)
        List<string> CaptureHeader(string path)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(path))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    lines.Add(line);
                    if (line.Contains(@""">"))
                        break;
                }
            }
            return lines;
        }

        // Hack to preserve layout of document header (which I like)
        void RewriteHeader(string path, List<string> header)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in header)
                sb.AppendLine(line);

            var lines = new List<string>();
            bool first = true;
            bool write = false;
            using (var reader = new StreamReader(path))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    if (write)
                    {
                        if (!first)
                            sb.AppendLine();
                        first = false;
                        sb.Append(line);
                    }
                    if (line.Contains(@""">"))
                        write = true;
                }
            }
            using (var writer = new StreamWriter(path, false))
            {
                byte[] preamble = new byte[] { 0xef, 0xbb, 0xbf };
                writer.BaseStream.Write(preamble, 0, preamble.Length);
                writer.BaseStream.Position = preamble.Length;
                writer.Write(sb.ToString());
            }
        }


    }
}
