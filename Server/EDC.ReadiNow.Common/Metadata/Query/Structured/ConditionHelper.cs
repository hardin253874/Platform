// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Xml;
using EDC.Database;
using EDC.Xml;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// A static helper class for dealing with Condition objects.
    /// </summary>
    public static class ConditionHelper
    {
        /// <summary>
        /// Writes the condition to the specified XML writer using the specified XML element name.
        /// </summary>
        /// <param name="elementName">The XML element name that will contain the serialized condition.</param>
        /// <param name="condition">The condition to serialize.</param>
        /// <param name="xmlWriter">The XML writer used to write the image to.</param>
        public static void WriteConditionXml(string elementName, Condition condition, XmlWriter xmlWriter)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }

            if (xmlWriter == null)
            {
                throw new ArgumentNullException("xmlWriter");
            }

            xmlWriter.WriteStartElement(elementName);
            xmlWriter.WriteElementString("operator", condition.Operator.ToString());
            xmlWriter.WriteElementString("columnName", condition.ColumnName ?? string.Empty);
            condition.ColumnType.ToXml(xmlWriter);

            xmlWriter.WriteStartElement("arguments");
            if (condition.Arguments != null)
            {
                foreach (TypedValue arg in condition.Arguments)
                {
                    TypedValueHelper.WriteTypedValueXml("argument", arg, xmlWriter);
                }
            }
            xmlWriter.WriteEndElement(); // arguments

            xmlWriter.WriteEndElement(); //elementName
        }


        /// <summary>
        /// Reconstructs the condition object from the specified XML node and XML element name.
        /// </summary>
        /// <param name="node">The XML node containing the condition XML.</param>
        /// <param name="elementName">The name of the XML element containing the condition data.</param>
        /// <returns>The condition object.</returns>
        public static Condition ReadConditionXml(XmlNode node, string elementName)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            var condition = new Condition();

            if (XmlHelper.EvaluateSingleNode(node, elementName))
            {
                XmlNode conditionNode = XmlHelper.SelectSingleNode(node, elementName);
                
                condition.Operator = (ConditionType)XmlHelper.ReadElementEnum(conditionNode, "operator", typeof(ConditionType), ConditionType.Unspecified);
                condition.ColumnName = XmlHelper.ReadElementString(conditionNode, "columnName", string.Empty);

                //database type
                condition.ColumnType = DatabaseType.UnknownType;
                if (XmlHelper.EvaluateSingleNode(conditionNode, "type"))
                {
                    XmlNode typeNode = XmlHelper.SelectSingleNode(conditionNode, "type");                    
                    condition.ColumnType = DatabaseType.FromXml(typeNode);                    
                }

                if (DatabaseTypeHelper.IsTypeRelationshipField(condition.ColumnType))
                {
                    XmlNode argumentsNode = XmlHelper.SelectSingleNode(conditionNode, "arguments");
                    
                    foreach (XmlNode argumentNode in argumentsNode.ChildNodes)
                    {
                        TypedValue argument = TypedValueHelper.FromXml(argumentNode);
                        if (argument != null)
                        {
                            condition.Arguments.Add(argument);
                            if (argument.Value != null)
                            {
                                string strArgumentValue = argument.Value.ToString();
                                long longArgumentValue;
                                if (long.TryParse(strArgumentValue, out longArgumentValue))
                                {
                                    Resource resource = Model.Entity.Get<Resource>(new EntityRef(longArgumentValue));
                                    ConvertArgumentValue(strArgumentValue, resource == null ? "Missing" : resource.Name);
                                }
                            }
                        }
                    }
                }
                else
                {
                    XmlNodeList argumentNodes = XmlHelper.SelectNodes(conditionNode, "arguments");
                    foreach (TypedValue argument in argumentNodes.Cast<XmlNode>().Select(argumentNode => TypedValueHelper.ReadTypedValueXml(argumentNode, "argument")).Where(argument => argument != null))
                    {
                        condition.Arguments.Add(argument);
                    }
                }
            }

            return condition;
        }

        /// <summary>
        /// convert relationship type argument value to standard format 
        /// </summary>
        /// <param name="strArgumentValue">argument id value</param>
        /// <param name="resourceName">resource name</param>
        /// <returns></returns>
        private static string ConvertArgumentValue(string strArgumentValue, string resourceName)
        {
            const string argumentValueFormat = "<e id=\"{0}\" text=\"{1}\"/>"; // We can probably do this better!

            return string.Format(argumentValueFormat, strArgumentValue, resourceName);
        }
    }
}
