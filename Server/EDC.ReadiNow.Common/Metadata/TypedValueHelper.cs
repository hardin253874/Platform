// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using EDC.Database;
using EDC.ReadiNow.Model;
using EDC.Xml;
using EDC.Database.Types;
using System.Xml.Serialization;
using System.IO;

namespace EDC.ReadiNow.Metadata
{
    /// <summary>
    /// A static helper class for dealing with TypedValue objects.
    /// </summary>
    public static class TypedValueHelper
    {
        /// <summary>
        /// Writes the typedValue to the specified XML writer using the specified XML element name.
        /// </summary>
        /// <param name="elementName">The XML element name that will contain the serialized typedValue.</param>
        /// <param name="typedValue">The typedValue to serialize.</param>
        /// <param name="xmlWriter">The XML writer used to write the image to.</param>
        // Obsolete("Use ToXml instead.")
        public static void WriteTypedValueXml(string elementName, TypedValue typedValue, XmlWriter xmlWriter)
        {
            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            if (typedValue == null)
            {
                throw new ArgumentNullException("typedValue");
            }

            if (xmlWriter == null)
            {
                throw new ArgumentNullException("xmlWriter");
            }

            xmlWriter.WriteStartElement(elementName);

            ToXml_Impl(xmlWriter, typedValue);

            xmlWriter.WriteEndElement(); // elementName                                        
        }


        /// <summary>
        /// Reconstructs the typedValue object from the specified XML node and XML element name.
        /// </summary>
        /// <param name="node">The XML node containing the typedValue XML.</param>
        /// <param name="elementName">The name of the XML element containing the typeValue data.</param>
        /// <returns>The typedValue object.</returns>
        // Obsolete("Use FromXml instead.")
        public static TypedValue ReadTypedValueXml(XmlNode node, string elementName)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw new ArgumentNullException("elementName");
            }

            TypedValue typedValue = null;

            if (XmlHelper.EvaluateSingleNode(node, elementName))
            {
                XmlNode typedValueNode = XmlHelper.SelectSingleNode(node, elementName);

                typedValue = FromXml(typedValueNode);
            }

            return typedValue;
        }


        /// <summary>
        /// Serializes the entire TypedValue element using the XML serializer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer.</param>
        /// <param name="typedValue">The TypedValue.</param>
        public static void ToXml(XmlWriter xmlWriter, TypedValue typedValue)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TypedValue));
            serializer.Serialize(xmlWriter, typedValue);
        }


        /// <summary>
        /// Serializes the entire TypedValue to an XML string.
        /// </summary>
        /// <param name="typedValue">The TypedValue.</param>
        public static string ToXml(TypedValue typedValue)
        {
            StringBuilder sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            using (XmlWriter xmlWriter = XmlTextWriter.Create(sb, settings))
            {
                ToXml(xmlWriter, typedValue);
                xmlWriter.Flush();
            }

            return sb.ToString();
        }


        /// <summary>
        /// Serializes the TypedValue object data to XML. Does not include the outer element tag.
        /// </summary>
        /// <param name="writer">The XML writer.</param>
        /// <param name="typedValue">The TypedValue.</param>
        public static void ToXml_Impl(XmlWriter writer, TypedValue typedValue)
        {
            bool isEntity = typedValue.Type is ChoiceRelationshipType || typedValue.Type is InlineRelationshipType || typedValue.Type is IdentifierType;
            string typeName = typedValue.Type.GetType().Name;
            typeName = typeName.Substring(0, typeName.Length - 4);

            writer.WriteAttributeString("type", typeName);
            if (isEntity)
            {
                writer.WriteAttributeString("entityRef", "true");
            }
            writer.WriteValue(typedValue.ValueString);

            if (typedValue.SourceEntityTypeId > 0)
            {
                writer.WriteStartElement("SourceEntityTypeId");
                writer.WriteAttributeString("entityRef", "true");
                writer.WriteValue(typedValue.SourceEntityTypeId);
                writer.WriteEndElement();
            }
        }


        /// <summary>
        /// Deserializes the TypedValue from xml. Includes the outer tag.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static TypedValue FromXml(string xml)
        {
            using (TextReader reader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TypedValue));
                TypedValue result = (TypedValue)serializer.Deserialize(reader);
                return result;
            }
        }


        /// <summary>
        /// Deserializes the TypedValue from xml.
        /// </summary>
        /// <param name="typedValueNode">The TypedValue node.</param>
        /// <returns></returns>
        public static TypedValue FromXml(XmlNode typedValueNode)
        {
            if (typedValueNode == null)
                throw new ArgumentNullException("typedValueNode");

            using (XmlNodeReader reader = new XmlNodeReader(typedValueNode))
            {
                reader.Read();  // Move from 'None' to the typed value element.
                TypedValue result = FromXml(reader);
                return result;
            }
        }


        /// <summary>
        /// Deserializes the TypedValue from xml.
        /// </summary>
        /// <param name="reader">An XML reader, positioned at the TypedValue element.</param>
        /// <returns></returns>
        public static TypedValue FromXml(XmlReader reader)
        {
            TypedValue result = new TypedValue();
            FromXml_Impl(reader, result);
            return result;
        }


        /// <summary>
        /// Deserializes the TypedValue from xml.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="target">The target.</param>
        internal static void FromXml_Impl(XmlReader reader, TypedValue target)
        {
            // Current formats:
            // <TypedValue type="String">Hello</TypedValue>
            // <TypedValue type="Int32">1234</TypedValue>
            // <TypedValue type="InlineRelationship" entityRef="true">123<SourceEntityTypeId entityRef="true">456</SourceEntityTypeId></TypedValue>
            // etc..

            // Legacy (pre 1.0) formats:
            // <TypedValue><Value xsi:type="xsd:int">4</Value><Type xsi:type="Int32Type" /></TypedValue>
            // <TypedValue>
            //   <Type xsi:type="InlineRelationshipType">
            //   </Type>
            //   <SourceEntityTypeId>5678</SourceEntityTypeId>
            //   <Value>1234:Not Available;11293:unknown;</Value>
            // </TypedValue>
            // etc..

            bool typeNameUsesDisplayName = false;
            string valueString = string.Empty;

            // Read 'type' attribute
            string typeName = reader.GetAttribute("type");
            if (typeName != null)
                typeName = typeName + "Type";

            // Read any content nodes
            bool rootIsEmpty = reader.IsEmptyElement;
            int depth = rootIsEmpty ? 0 : 1;
            bool isLegacyValueElem = false;
            bool isLegacyTypeElem = false;
            bool isSourceEntityTypeIdElem = false;
            reader.Read();
            while (depth > 0)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        isLegacyValueElem = false;
                        isLegacyTypeElem = false;
                        isSourceEntityTypeIdElem = false;
                        if (depth == 1)
                        {
                            switch (reader.Name)
                            {
                                // Legacy <Value> element
                                case "Value":
                                case "value":    // (conditional formatting)
                                    isLegacyValueElem = true;
                                    break;

                                // Legacy <type> element (conditional formatting) .. type might be in text or name attribute.
                                case "type":
                                    isLegacyTypeElem = true;
                                    if (typeName == null)
                                    {
                                        typeName = reader.GetAttribute("name");
                                        if (typeName != null)
                                            typeNameUsesDisplayName = true;
                                    }
                                    break;

                                // Legacy <Type> element
                                case "Type":
                                    if (typeName == null)
                                        typeName = reader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");    // xsi:type
                                    break;

                                // Deprecated <SourceEntityTypeId> element
                                case "SourceEntityTypeId":  // deprecated
                                    isSourceEntityTypeIdElem = true;
                                    break;
                            }
                        }
                        if (!reader.IsEmptyElement)
                        {
                            depth++;
                        }
                        break;

                    case XmlNodeType.Text:
                        if (!string.IsNullOrEmpty(reader.Value))
                        {
                            // Depth == 1 : Current Value as Text
                            // Also, legacy Value element
                            if (depth == 1 || isLegacyValueElem)
                            {
                                valueString = reader.Value;
                            }
                            // SourceEntityTypeId element  (current)
                            else if (isSourceEntityTypeIdElem)
                            {
                                target.SourceEntityTypeId = XmlConvert.ToInt64(reader.Value);
                            }
                            // Legacy Type element
                            else if (isLegacyTypeElem && typeName == null)
                            {
                                typeName = reader.Value;
                                typeNameUsesDisplayName = true;
                            }
                        }
                        break;

                    case XmlNodeType.EndElement:
                        depth--;
                        break;

                    case XmlNodeType.None:
                        throw new Exception("Unexpected end of XML.");
                }
                reader.Read();
            }

            // Validate type
            if (typeName == null)
            {
                throw new InvalidOperationException("No type data");
            }

            // Perform value-string corrections to legacy (pre 1.0) format.
            if (typeName == "ChoiceRelationshipType" || typeName == "InlineRelationshipType" ||
                typeName == "IdentifierType")
            {
                if (valueString.EndsWith(";"))
                {
                    valueString = valueString.Split(':')[0];
                }
            }

            if (typeNameUsesDisplayName)
            {
                // Legacy
                target.Type = DatabaseTypeHelper.ConvertFromDisplayName(typeName);
            }
            else
            {
                // Current
                target.Type = DatabaseTypeHelper.ConvertDatabaseTypeNameToDatabaseType(typeName);
            }

            if (rootIsEmpty && target.Type is StringType)
                target.ValueString = null;
            else
                target.ValueString = valueString ?? "";
        }

        /// <summary>
        /// Create a new TypedValue instance from a data type and object representation.
        /// </summary>
        /// <param name="dataType">The data type of the object.</param>
        /// <param name="value">The object.</param>
        public static TypedValue FromDataType(DataType dataType, object value)
        {
            var result = new TypedValue
            {
                Type = DataTypeHelper.ToDatabaseType(dataType),
                Value = value
            };
            return result;
        }

        /// <summary>
        /// Method for creating typed values from DatabaseType.
        /// Caller must save.
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="forceResourceLists"></param>
        /// <returns></returns>
        public static ActivityArgument CreateTypedValueEntity(DatabaseType dbType, bool forceResourceLists = false)
        {
            IEntity result;

            if (dbType is UnknownType || dbType == null)
            {
                return null;
            }
            if (dbType is StringType || dbType is XmlType)
            {
                result = new StringArgument();
            }
            else if (dbType is Int32Type) // includes autonumber
            {
                result = new IntegerArgument();
            }
            else if (dbType is CurrencyType)
            {
                result = new CurrencyArgument();
            }
            else if (dbType is DecimalType)
            {
                result = new DecimalArgument();
            }
            else if (dbType is DateType)
            {
                result = new DateArgument();
            }
            else if (dbType is TimeType)
            {
                result = new TimeArgument();
            }
            else if (dbType is DateTimeType)
            {
                result = new DateTimeArgument();
            }
            else if (dbType is GuidType)
            {
                result = new GuidArgument();
            }
            else if (dbType is BoolType)
            {
                result = new BoolArgument();
            }
            else if (dbType is ChoiceRelationshipType || dbType is InlineRelationshipType || dbType is IdentifierType)
            {
                // Hmm.. not sure if this is a good idea for identifier types..
                if (forceResourceLists)
                    result = new ResourceListArgument();
                else
                    result = new ResourceArgument();
            }
            else
            {
                // IdentifierType
                // BinaryType
                // StructureLevelsType
                throw new Exception("Unhandled expression result type: " + dbType.GetType().Name);
            }

            // Caller must save
            return result.As<ActivityArgument>();
        }


        /// <summary>
        /// Method for creating typed values from DatabaseType.
        /// Caller must save.
        /// </summary>
        /// <param name="typedValue"></param>
        /// <param name="forceResourceLists"></param>
        /// <returns></returns>
        public static ActivityArgument CreateTypedValueEntity(TypedValue typedValue, bool forceResourceLists = false)
        {
            DatabaseType dbType = typedValue.Type;
            IEntity result;

            if (dbType is UnknownType || dbType == null)
            {
                return null;
            }
            if (dbType is StringType || dbType is XmlType)
            {
                result = new StringArgument { StringParameterValue = (string) typedValue.Value };
            }
            else if (dbType is Int32Type) // includes autonumber
            {
                result = new IntegerArgument { IntParameterValue = (int?)typedValue.Value };
            }
            else if (dbType is CurrencyType)
            {
                result = new CurrencyArgument { DecimalParameterValue = (decimal?)typedValue.Value };
            }
            else if (dbType is DecimalType)
            {
                result = new DecimalArgument { DecimalParameterValue = (decimal?)typedValue.Value };
            }
            else if (dbType is DateType)
            {
                result = new DateArgument { DateParameterValue = (DateTime?)typedValue.Value };
            }
            else if (dbType is TimeType)
            {
                result = new TimeArgument { TimeParameterValue = (DateTime?)typedValue.Value };
            }
            else if (dbType is DateTimeType)
            {
                result = new DateTimeArgument { DateTimeParameterValue = (DateTime?)typedValue.Value };
            }
            else if (dbType is GuidType)
            {
                result = new GuidArgument { GuidParameterValue = (Guid?)typedValue.Value };
            }
            else if (dbType is BoolType)
            {
                result = new BoolArgument { BoolParameterValue = (bool?)typedValue.Value };
            }
            else if (dbType is ChoiceRelationshipType || dbType is InlineRelationshipType || dbType is IdentifierType)
            {
                // Hmm.. not sure if this is a good idea for identifier types..

                long targetId;
                if (typedValue.Value == null || !long.TryParse(typedValue.Value.ToString(), out targetId))
                {
                    targetId = 0;
                }

                IEntity target = targetId > 0 ? Entity.Get(targetId) : null;

                if (forceResourceLists)
                {
                    var resourceListArgument = new ResourceListArgument();
                    if (target != null)
                        resourceListArgument.ResourceListParameterValues.Add(target.As<Resource>());
                    result = resourceListArgument;
                }
                else
                {
                    var resourceArgument = new ResourceArgument();
                    if (target != null)
                        resourceArgument.ResourceParameterValue = target.As<Resource>();
                    result = resourceArgument;
                }

                TypedArgument tResult = result.As<TypedArgument>();
                if (typedValue.SourceEntityTypeId > 0)
                {
                    var targetType = Entity.Get<EntityType>(typedValue.SourceEntityTypeId);
                    tResult.ConformsToType = targetType;
                }
            }
            else
            {
                // IdentifierType
                // BinaryType
                // StructureLevelsType
                throw new Exception("Unhandled expression result type: " + dbType.GetType().Name);
            }

            // Caller must save
            return result.As<ActivityArgument>();
        }

        /// <summary>
        /// Typeds the value from entity.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>List{TypedValue}.</returns>
        /// <exception cref="System.Exception">Unhandled expression result type</exception>
        public static List<TypedValue> TypedValueFromEntity(ActivityArgument argument)
        {
            List<TypedValue> typedValues = new List<TypedValue>();
            TypedValue typedValue = new TypedValue();
            if (argument == null)
            {
                typedValue.Type = DatabaseType.UnknownType;
                typedValues.Add(typedValue);
            }
            else if (argument.Is<StringArgument>())
            {
                typedValue.Type = DatabaseType.StringType;
                StringArgument stringArgument = argument.As<StringArgument>();
                if (stringArgument.StringParameterValue != null)
                {
                    typedValue.Value = stringArgument.StringParameterValue;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<IntegerArgument>())
            {
                typedValue.Type = DatabaseType.Int32Type;
                IntegerArgument integerArgument = argument.As<IntegerArgument>();
                if (integerArgument.IntParameterValue.HasValue)
                {
                    typedValue.Value = integerArgument.IntParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<CurrencyArgument>())
            {
                typedValue.Type = DatabaseType.CurrencyType;
                CurrencyArgument currencyArgument = argument.As<CurrencyArgument>();
                if (currencyArgument.DecimalParameterValue.HasValue)
                {
                    typedValue.Value = currencyArgument.DecimalParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<DecimalArgument>())
            {
                typedValue.Type = DatabaseType.DecimalType;
                DecimalArgument decimalArgument = argument.As<DecimalArgument>();
                if (decimalArgument.DecimalParameterValue.HasValue)
                {
                    typedValue.Value = decimalArgument.DecimalParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<DateArgument>())
            {
                typedValue.Type = DatabaseType.DateType;
                DateArgument dateArgument = argument.As<DateArgument>();
                if (dateArgument.DateParameterValue.HasValue)
                {
                    typedValue.Value = dateArgument.DateParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<TimeArgument>())
            {
                typedValue.Type = DatabaseType.TimeType;
                TimeArgument timeArgument = argument.As<TimeArgument>();
                if (timeArgument.TimeParameterValue.HasValue)
                {
                    typedValue.Value = timeArgument.TimeParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<DateTimeArgument>())
            {
                typedValue.Type = DatabaseType.DateTimeType;
                DateTimeArgument dateTimeArgument = argument.As<DateTimeArgument>();
                if (dateTimeArgument.DateTimeParameterValue.HasValue)
                {
                    typedValue.Value = dateTimeArgument.DateTimeParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<GuidArgument>())
            {
                typedValue.Type = DatabaseType.GuidType;
                GuidArgument guidArgument = argument.As<GuidArgument>();
                if (guidArgument.GuidParameterValue.HasValue)
                {
                    typedValue.Value = guidArgument.GuidParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<BoolArgument>())
            {
                typedValue.Type = DatabaseType.BoolType;
                BoolArgument boolArgument = argument.As<BoolArgument>();
                if (boolArgument.BoolParameterValue.HasValue)
                {
                    typedValue.Value = boolArgument.BoolParameterValue.Value;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<ResourceArgument>())
            {
                TypedArgument typedArgument = argument.As<TypedArgument>();
                if (typedArgument != null && typedArgument.ConformsToType != null)
                {
                    // Interrogate to get it's base type
                    EntityType type = Entity.Get<EntityType>(typedArgument.ConformsToType.Id);
                    EntityRef enumType = new EntityRef("core", "enumValue");
                    if (type.GetAncestorsAndSelf().FirstOrDefault(a => a.Id == enumType.Id) != null)
                    {
                        typedValue.Type = DatabaseType.ChoiceRelationshipType;
                    }
                    else
                    {
                        typedValue.Type = DatabaseType.InlineRelationshipType;
                    }
                }

                ResourceArgument resourceArgument = argument.As<ResourceArgument>();
                if (resourceArgument.ResourceParameterValue != null)
                {
                    // Is this an enum type (or are any of it's base types an enum type??
                    var conformsToType = resourceArgument.ConformsToType;
                    typedValue.SourceEntityTypeId = conformsToType != null ? conformsToType.Id : 0;
                    typedValue.Value = resourceArgument.ResourceParameterValue.Id;
                }
                typedValues.Add(typedValue);
            }
            else if (argument.Is<ResourceListArgument>())
            {
                TypedArgument typedArgument = argument.As<TypedArgument>();
                if (typedArgument != null && typedArgument.ConformsToType != null)
                {
                    // Interrogate to get it's base type
                    EntityType type = Entity.Get<EntityType>(typedArgument.ConformsToType.Id);
                    EntityRef enumType = new EntityRef("core", "enumValue");
                    if (type.GetAncestorsAndSelf().FirstOrDefault(a => a.Id == enumType.Id) != null)
                    {
                        typedValue.Type = DatabaseType.ChoiceRelationshipType;
                    }
                    else
                    {
                        typedValue.Type = DatabaseType.InlineRelationshipType;
                    }
                }
                ResourceListArgument resourceList = argument.As<ResourceListArgument>();
                if (resourceList.ResourceListParameterValues == null || resourceList.ResourceListParameterValues.Count <= 0)
                {
                    typedValues.Add(typedValue);
                }
                else
                {
                    typedValues.AddRange(resourceList.ResourceListParameterValues.Select(parameterValue => new TypedValue
                        {
                            Type = typedValue.Type, SourceEntityTypeId = resourceList.ConformsToType.Id, Value = parameterValue.Id
                        }));
                }
            }
            else 
            {
                // Throw as we cannot convert type:(
                throw new Exception("Unhandled expression result type");
            }

            return typedValues;
        }

        /// <summary>
        /// Gets the database type represented by a typed value.
        /// (Avoids unnecessarily attempting to resolve values).
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns>Database type.</returns>
        /// <exception cref="System.Exception">Unhandled expression result type</exception>
        public static DatabaseType GetDatabaseType( ActivityArgument argument )
        {
            if ( argument == null )
            {
                return DatabaseType.UnknownType;
            }
            else if ( argument.Is<StringArgument>( ) )
            {
                return DatabaseType.StringType;
            }
            else if ( argument.Is<IntegerArgument>( ) )
            {
                return DatabaseType.Int32Type;
            }
            else if ( argument.Is<CurrencyArgument>( ) )
            {
                return DatabaseType.CurrencyType;
            }
            else if ( argument.Is<DecimalArgument>( ) )
            {
                return DatabaseType.DecimalType;
            }
            else if ( argument.Is<DateArgument>( ) )
            {
                return DatabaseType.DateType;
            }
            else if ( argument.Is<TimeArgument>( ) )
            {
                return DatabaseType.TimeType;
            }
            else if ( argument.Is<DateTimeArgument>( ) )
            {
                return DatabaseType.DateTimeType;
            }
            else if ( argument.Is<GuidArgument>( ) )
            {
                return DatabaseType.GuidType;
            }
            else if ( argument.Is<BoolArgument>( ) )
            {
                return DatabaseType.BoolType;
            }
            else if ( argument.Is<ResourceArgument>( ) || argument.Is<ResourceListArgument>( ) )
            {
                TypedArgument typedArgument = argument.As<TypedArgument>( );
                if ( typedArgument != null && typedArgument.ConformsToType != null )
                {
                    // Interrogate to get it's base type
                    EntityType type = Entity.Get<EntityType>( typedArgument.ConformsToType.Id );
                    EntityRef enumType = new EntityRef( "core", "enumValue" );
                    if ( type.GetAncestorsAndSelf( ).FirstOrDefault( a => a.Id == enumType.Id ) != null )
                    {
                        return DatabaseType.ChoiceRelationshipType;
                    }
                }
                return DatabaseType.InlineRelationshipType;
            }

            throw new Exception( "Result type is not of a recognised type." );
        }

    }
}
