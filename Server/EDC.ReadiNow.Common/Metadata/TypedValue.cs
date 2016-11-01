// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using EDC.Database;
using System.Xml.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.Core;
using EDC.Database.Types;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata
{
    /// <summary>
    /// Defines single value, of variable type, along with its type.
    /// Server-side implementation.
    /// </summary>
    /// <remarks>   
    /// The purpose of this contract is to have a single container object that can be used for transporting
    /// variable type values across the service layer. It internally handles the formatting/unformatting of data.
    /// That way other contracts can simply reference this value and not need to worry about formatting.
    /// </remarks>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType("TypedValue", Namespace = Constants.StructuredQueryNamespace)]
    [XmlRoot("TypedValue", Namespace = Constants.StructuredQueryNamespace)]
    public class TypedValue
    {
        public TypedValue() { }

	    public TypedValue( DateTimeKind defaultDateTimeKind )
		    : this( )
	    {
		    _defaultDateTimeKind = defaultDateTimeKind;
	    }

	    public TypedValue(string value)
        {
            Type = DatabaseType.StringType;
            Value = value;
        }

        public TypedValue(int value)
        {
            Type = DatabaseType.Int32Type;
            Value = value;
        }

        /// <summary>
        /// The object value, in its native .Net type.
        /// Accessible only in code - not directly transported over WCF.
        /// </summary>
        [XmlIgnore] // serialized via ValueString
        public object Value
        {
            get
            {
                if (_tmpValue != null)
                    return _tmpValue;

                // some special cases
                if (Type is DateType)
                {
                    if (ValueString == DateType.DefaultValueToday )
                        return DateTime.Today;
                }

                if (Type is DateTimeType)
                {
                    if (ValueString == DateType.DefaultValueToday )
                    {
                        // TODO: Make this work correctly for timezones
                        // hack until this get fixed.
                        return DateTime.UtcNow; //return DateTime.Today;
                    }

                    if (ValueString == DateTimeType.DefaultValueNow )
                        return DateTime.UtcNow;
                }

	            return DatabaseTypeHelper.ConvertFromString( Type, ValueString, _defaultDateTimeKind );
            }
            set
            {
                if (Type == null)
                {
                    _tmpValue = value;
                }
                else
                {
                    SetValueString(value);
                }
            }
        }


        /// <summary>
        /// ValueString is the primary source of truth. However, conversion can only occur correctly if the 'Type' property is also set.
        /// Therefore this temp object holds 'Value' in scenarios where Type is not yet set.
        /// </summary>
        private object _tmpValue;   // this is to store the value until the 'Type' is also available.

		/// <summary>
		/// The default date time kind
		/// </summary>
	    private DateTimeKind _defaultDateTimeKind;

        /// <summary>
        /// The type of data being represented.
        /// </summary>
        [DataMember]
        [XmlIgnore]
        public DatabaseType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (_tmpValue != null)
                {
                    SetValueString(_tmpValue);
                    _tmpValue = null;
                }
            }
        }
        private DatabaseType _type;


        /// <summary>
        /// The source entity type of relationship data being represented.
        /// </summary>
        /// <remarks>
        /// This gets persisted in the XML serialization section below.
        /// </remarks>
        [DataMember]
        [XmlIgnore]
        public long SourceEntityTypeId { get; set; }


        /// <summary>
        /// The data member used for transporting the value over WCF.
        /// </summary>
        /// <remarks>
        /// This is the main source of truth.
        /// </remarks>
        [XmlIgnore] // serialization below
        public string ValueString { get; set; }


        /// <summary>
        /// WCF serialization implementation of ValueString.
        /// </summary>
        /// <remarks>
        /// This property only exists to support a broken behaviour: namely that null strings were getting converted to empty strings
        /// when DataContract serialization was being performed, and it ended up that various client-side code can't handle this.
        /// TODO: remove this hack and fix the console.
        /// </remarks>
        [DataMember(Name = "ValueString")]
        [XmlIgnore]
        public string WcfImplValueString
        {
            get
            {
                if (Type is StringType)
                    return ValueString ?? "";
                else
                    return ValueString;
            }
            set { ValueString = value; }
        }

        /// <summary>
        /// Logic for setting a value object
        /// </summary>
        /// <param name="value"></param>
        private void SetValueString(object value)
        {
            if (value == null)
            {
                ValueString = null;
                return;
            }

            if (value is string)
            {
                var strValue = (string)value;

                if (Type is DateType)
                {
                    if (strValue == DateType.DefaultValueToday )
                    {
                        ValueString = strValue;
                        return;
                    }
                }

                if (Type is DateTimeType)
                {
                    if (strValue == DateType.DefaultValueToday || strValue == DateTimeType.DefaultValueNow )
                    {
                        ValueString = strValue;
                        return;
                    }
                }

                if (strValue == "")
                {
                    // Work around issue in DatabaseTypeHelper.ConvertFromString whereby empty strings get round-tripped as nulls
                    // Can't easily be corrected in ConvertFromString as some code depends on that behavior.
                    ValueString = "";
                    return;
                }
            }

	        ValueString = DatabaseTypeHelper.ConvertToString( Type, value, _defaultDateTimeKind );
        }

        #region XML Serialization

        [XmlAttribute("type")]
        public string XmlImplTypeAttrib
        {
            get
            {
                if (Type == null)           // This should never occur, but we are getting some corrupt data coming via the default constructor
                    return null;
                    
                string typeName = Type.GetType().Name;
                return typeName.Substring(0, typeName.Length - 4);
            }
            set
            {
                Type = DatabaseTypeHelper.ConvertDatabaseTypeNameToDatabaseType(value + "Type");
            }
        }

        [XmlAttribute("entityRef")]
        [DefaultValue("false")]
        public string XmlImplEntityRefAttrib
        {
            get
            {
                bool isEntity = Type is IdentifierType || Type is ChoiceRelationshipType || Type is InlineRelationshipType;
                return isEntity ? "true" : "false";
            }
            set
            {
            }
        }

        [XmlText]
        public string XmlImplValueString
        {
            get { return ValueString; }
            set { ValueString = value; }
        }


        /// <summary>
        ///     All XML serialization of EntityRef is done via this property, so that it appears as an inline text (for tidy XML).
        /// </summary>
        [XmlAnyElement]
        public XmlNode[] XmlImplChildren
        {
            get
            {
                if (SourceEntityTypeId == 0)
                {
                    return null;
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    XmlElement sourceTypeNode = doc.CreateElement("SourceEntityTypeId", Constants.StructuredQueryNamespace);
                    sourceTypeNode.SetAttribute("entityRef", "true");
                    sourceTypeNode.InnerText = XmlConvert.ToString(SourceEntityTypeId);

                    return new XmlNode[] { sourceTypeNode };
                }
            }
            set
            {
                if (value == null)
                    return;
                foreach (var node in value)
                {
                    XmlElement xmlElem = node as XmlElement;
                    if (xmlElem == null)
                        continue;
                    switch (xmlElem.Name)
                    {
                        case "Value":
                            string valueStr = xmlElem.InnerText;
                            if (valueStr.EndsWith(";"))
                            {
                                bool isEntity = Type is IdentifierType || Type is ChoiceRelationshipType ||
                                                Type is InlineRelationshipType;
                                if (isEntity)
                                {
                                    valueStr = valueStr.Split(':')[0];
                                }
                            }
                            ValueString = valueStr;
                            break;
                        case "value":    // legacy for conditional formatting
                            ValueString = xmlElem.InnerText;
                            break;
                        case "Type":    // legacy type (intentionally ignore any metadata)
                            string xsiType = xmlElem.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
                            Type = DatabaseTypeHelper.ConvertDatabaseTypeNameToDatabaseType(xsiType);
                            break;
                        case "type":    // legacy for conditional formatting
                            string typeName = xmlElem.InnerText;
                            if (string.IsNullOrEmpty(typeName))
                                typeName = xmlElem.GetAttribute("name");
                            Type = DatabaseTypeHelper.ConvertFromDisplayName(typeName);
                            break;
                        case "SourceEntityTypeId":
                            SourceEntityTypeId = XmlConvert.ToInt64(xmlElem.InnerText);
                            break;
                    }
                }
            } 
        }
        #endregion
    }

}