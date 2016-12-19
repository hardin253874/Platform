// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EDC.Xml
{
    /// <summary>
    /// Generic XML serializer for object of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Serializer<T> where T : class
    {
        /// <summary>
        /// Serializes the object to xml.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="query">The query.</param>
        public static void ToXml(XmlWriter xmlWriter, T query)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlWriter, query);
        }


        /// <summary>
        /// Serializes the object to xml.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static string ToXml(T query)
        {
            StringBuilder sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                ToXml(xmlWriter, query);
                xmlWriter.Flush();
            }

            return sb.ToString();
        }


        /// <summary>
        /// Deserializes the object from xml.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static T FromXml(string xml)
        {
            using (TextReader reader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T result = (T)serializer.Deserialize(reader);
                return result;
            }
        }


        /// <summary>
        /// Deserializes the object from xml.
        /// </summary>
        /// <param name="stream">The XML stream.</param>
        /// <returns></returns>
        public static T FromXml(Stream stream)
        {
            using (TextReader reader = new StreamReader(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T result = (T)serializer.Deserialize(reader);
                return result;
            }
        }
    }
}
