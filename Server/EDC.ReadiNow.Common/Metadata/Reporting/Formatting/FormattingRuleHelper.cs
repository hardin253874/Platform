// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using EDC.ReadiNow.Metadata.Media;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.Xml;
using EDC.Database;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// Static helper class for dealing with formatting rules.
    /// </summary>
    public static class FormattingRuleHelper
    {
        #region Public Methods
        /// <summary>
        /// Writes the column formats to the specified xml writer.
        /// </summary>
        /// <param name="columnFormats">The column formats to serialize.</param>
        /// <param name="xmlWriter">The writer used to write the xml.</param>
        public static void WriteColumnFormatsXml(IList<ColumnFormatting> columnFormats, XmlWriter xmlWriter)
        {
            if (columnFormats == null)
            {
                throw new ArgumentNullException("columnFormats");
            }

            if (xmlWriter == null)
            {
                throw new ArgumentNullException("xmlWriter");
            }

            if (columnFormats.Count > 0)
            {
                xmlWriter.WriteStartElement("columnFormats");

                foreach (ColumnFormatting columnFormat in columnFormats)
                {
                    xmlWriter.WriteStartElement("columnFormat");
                    xmlWriter.WriteElementString("queryColumnId", columnFormat.QueryColumnId.ToString());
                    xmlWriter.WriteElementString("columnName", columnFormat.ColumnName);
                    columnFormat.ColumnType.ToXml(xmlWriter);

                    xmlWriter.WriteElementString("showText", columnFormat.ShowText.ToString());
                    xmlWriter.WriteElementString("formatString", columnFormat.FormatString);
                    
                    xmlWriter.WriteElementString("decimalPlaces", columnFormat.DecimalPlaces.ToString());
                    xmlWriter.WriteElementString("prefix", columnFormat.Prefix);
                    xmlWriter.WriteElementString("suffix", columnFormat.Suffix);
                    xmlWriter.WriteElementString("lines", columnFormat.Lines.ToString());   
                    if (columnFormat.FormattingRule is ColorFormattingRule)
                    {
                        ColorFormattingRule colorRule = columnFormat.FormattingRule as ColorFormattingRule;
                        WriteColorFormattingRuleXml(colorRule, xmlWriter);
                    }
                    else if (columnFormat.FormattingRule is IconFormattingRule)
                    {
                        IconFormattingRule iconRule = columnFormat.FormattingRule as IconFormattingRule;
                        WriteIconFormattingRuleXml(iconRule, xmlWriter);
                    }
                    else if (columnFormat.FormattingRule is BarFormattingRule)
                    {
                        BarFormattingRule barRule = columnFormat.FormattingRule as BarFormattingRule;
                        WriteBarFormattingRuleXml(barRule, xmlWriter);
                    }
                    else if (columnFormat.FormattingRule is ImageFormattingRule)
                    {
                        var imageRule = columnFormat.FormattingRule as ImageFormattingRule;
                        WriteImageFormattingRuleXml(imageRule, xmlWriter);
                    }
                    xmlWriter.WriteEndElement(); //columnFormat
                }

                xmlWriter.WriteEndElement(); //columnFormats
            }
        }


        /// <summary>
        /// Reconstructs the column formats from an XML image.
        /// </summary>
        /// <param name="node">The data view node containing the column formats.</param>
        /// <returns>The list of column formats.</returns>
        public static List<ColumnFormatting> ReadColumnFormatsXml(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            List<ColumnFormatting> columnFormats = new List<ColumnFormatting>();

            XmlNodeList columnFormatNodes = XmlHelper.SelectNodes(node, "columnFormats/columnFormat");
            foreach (XmlNode columnFormatNode in columnFormatNodes)
            {
                ColumnFormatting columnFormat = new ColumnFormatting();

                columnFormat.QueryColumnId = XmlHelper.ReadElementGuid(columnFormatNode, "queryColumnId", Guid.Empty);
                columnFormat.ColumnName = XmlHelper.ReadElementString(columnFormatNode, "columnName");
                
                //database type
                columnFormat.ColumnType = DatabaseType.UnknownType;
                if (XmlHelper.EvaluateSingleNode(columnFormatNode, "type"))
                {
                    XmlNode typeNode = XmlHelper.SelectSingleNode(columnFormatNode, "type");
                    if (typeNode != null)
                    {
                        columnFormat.ColumnType = DatabaseType.FromXml(typeNode);
                    }
                }
                
                columnFormat.ShowText = XmlHelper.ReadElementBool(columnFormatNode, "showText", true);
                columnFormat.FormatString = XmlHelper.ReadElementString(columnFormatNode, "formatString", string.Empty);
                columnFormat.DecimalPlaces = XmlHelper.ReadElementInt32(columnFormatNode, "decimalPlaces", 2);
                columnFormat.Prefix = XmlHelper.ReadElementString(columnFormatNode, "prefix", string.Empty);
                columnFormat.Suffix = XmlHelper.ReadElementString(columnFormatNode, "suffix", string.Empty);
                columnFormat.Lines = XmlHelper.ReadElementInt32(columnFormatNode, "lines", 2);
                if (XmlHelper.EvaluateSingleNode(columnFormatNode, "colorFormattingRule"))
                {
                    columnFormat.FormattingRule = ReadColorFormattingRuleXml(columnFormatNode);
                }
                else if (XmlHelper.EvaluateSingleNode(columnFormatNode, "iconFormattingRule"))
                {
                    columnFormat.FormattingRule = ReadIconFormattingRuleXml(columnFormatNode);
                }
                else if (XmlHelper.EvaluateSingleNode(columnFormatNode, "barFormattingRule"))
                {
                    columnFormat.FormattingRule = ReadBarFormattingRuleXml(columnFormatNode);
                }
                else if (XmlHelper.EvaluateSingleNode(columnFormatNode, "imageFormattingRule"))
                {
                    columnFormat.FormattingRule = ReadImageFormattingRuleXml(columnFormatNode);
                }

                columnFormats.Add(columnFormat);
            }

            return columnFormats;
        }
        #endregion
        

        #region Non-Public Methods
        /// <summary>
        /// Writes the color formatting rule to the specified XML writer.
        /// </summary>
        /// <param name="colorFormattingRule">The color formatting rule to serialize.</param>
        /// <param name="xmlWriter">The XML writer used to write the image to.</param>
        private static void WriteColorFormattingRuleXml(ColorFormattingRule colorFormattingRule, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("colorFormattingRule");
            xmlWriter.WriteStartElement("rules");
            foreach (ColorRule rule in colorFormattingRule.Rules)
            {
                xmlWriter.WriteStartElement("rule");

                ColorInfoHelper.WriteColorInfoXml("backgroundColor", rule.BackgroundColor, xmlWriter);
                ColorInfoHelper.WriteColorInfoXml("foregroundColor", rule.ForegroundColor, xmlWriter);
                ConditionHelper.WriteConditionXml("condition", rule.Condition, xmlWriter);

                xmlWriter.WriteEndElement(); // rule
            }
            xmlWriter.WriteEndElement(); // rules  
            xmlWriter.WriteEndElement(); // colorFormattingRule  
        }


        /// <summary>
        /// Reconstructs a color formatting rule from the specified XML image.
        /// </summary>
        /// <param name="node">The node containing the color formatting rule XML.</param>
        /// <returns>The color formatting rule.</returns>
        private static ColorFormattingRule ReadColorFormattingRuleXml(XmlNode node)
        {
            ColorFormattingRule formattingRule = new ColorFormattingRule();

            XmlNodeList ruleNodes = XmlHelper.SelectNodes(node, "colorFormattingRule/rules/rule");
            foreach (XmlNode ruleNode in ruleNodes)
            {
                ColorRule rule = new ColorRule();

                rule.BackgroundColor = ColorInfoHelper.ReadColorInfoXml(ruleNode, "backgroundColor");
                rule.ForegroundColor = ColorInfoHelper.ReadColorInfoXml(ruleNode, "foregroundColor");
                rule.Condition = ConditionHelper.ReadConditionXml(ruleNode, "condition");

                formattingRule.Rules.Add(rule);
            }

            return formattingRule;
        }


        /// <summary>
        /// Writes the icon formatting rule to the specified XML writer.
        /// </summary>
        /// <param name="iconFormattingRule">The icon formatting rule to serialize.</param>
        /// <param name="xmlWriter">The XML writer used to write the image to.</param>
        private static void WriteIconFormattingRuleXml(IconFormattingRule iconFormattingRule, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("iconFormattingRule");
            xmlWriter.WriteStartElement("rules");
            foreach (IconRule rule in iconFormattingRule.Rules)
            {
                xmlWriter.WriteStartElement("rule");

                ColorInfoHelper.WriteColorInfoXml("color", rule.Color, xmlWriter);
                xmlWriter.WriteElementString("icon", rule.Icon.ToString());
                ConditionHelper.WriteConditionXml("condition", rule.Condition, xmlWriter);
                // Write out the Icon ID
                if (rule.IconId.HasValue)
                {
                    xmlWriter.WriteStartElement("iconId");
                    xmlWriter.WriteAttributeString("entityRef", "true");
                    xmlWriter.WriteString(rule.IconId.Value.ToString(CultureInfo.InvariantCulture));
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement(); // rule
            }
            xmlWriter.WriteEndElement(); // rules 
            xmlWriter.WriteEndElement(); // iconFormattingRule 
        }


        /// <summary>
        /// Reconstructs an icon formatting rule from the specified XML image.
        /// </summary>
        /// <param name="node">The node containing the icon formatting rule XML.</param>
        /// <returns>The icon formatting rule.</returns>
        private static IconFormattingRule ReadIconFormattingRuleXml(XmlNode node)
        {
            IconFormattingRule formattingRule = new IconFormattingRule();

            XmlNodeList ruleNodes = XmlHelper.SelectNodes(node, "iconFormattingRule/rules/rule");
            foreach (XmlNode ruleNode in ruleNodes)
            {
                IconRule rule = new IconRule();

                rule.Color = ColorInfoHelper.ReadColorInfoXml(ruleNode, "color");
                rule.Condition = ConditionHelper.ReadConditionXml(ruleNode, "condition");
                rule.Icon = (IconType)XmlHelper.ReadElementEnum(ruleNode, "icon", typeof(IconType), IconType.None);
                XmlNode iconIdNode = ruleNode.SelectSingleNode("iconId");
                if (iconIdNode != null)
                {
                    long iconId;
                    if (long.TryParse(iconIdNode.InnerText, out iconId))
                    {
                        rule.IconId = iconId;
                    }
                }
                rule.Scale = 1;
                formattingRule.Rules.Add(rule);
            }

            return formattingRule;
        }


        /// <summary>
        /// Writes the bar formatting rule to the specified XML writer.
        /// </summary>
        /// <param name="barFormattingRule">The bar formatting rule to serialize.</param>
        /// <param name="xmlWriter">The XML writer used to write the image to.</param>
        private static void WriteBarFormattingRuleXml(BarFormattingRule barFormattingRule, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("barFormattingRule");

            ColorInfoHelper.WriteColorInfoXml("color", barFormattingRule.Color, xmlWriter);
            TypedValueHelper.WriteTypedValueXml("minimum", barFormattingRule.Minimum, xmlWriter);
            TypedValueHelper.WriteTypedValueXml("maximum", barFormattingRule.Maximum, xmlWriter);

            xmlWriter.WriteEndElement(); // barFormattingRule
        }


        /// <summary>
        /// Writes the image formatting rule XML.
        /// </summary>
        /// <param name="imageFormattingRule">The image formatting rule.</param>
        /// <param name="xmlWriter">The XML writer.</param>
        private static void WriteImageFormattingRuleXml(ImageFormattingRule imageFormattingRule, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("imageFormattingRule");

            if (imageFormattingRule.ThumbnailScaleId != null)
            {
                xmlWriter.WriteStartElement("thumbnailScaleId");
                xmlWriter.WriteAttributeString("entityRef", "true");
                xmlWriter.WriteString(imageFormattingRule.ThumbnailScaleId.XmlSerializationText);
                xmlWriter.WriteEndElement(); // thumbnailScaleId
            }

            if (imageFormattingRule.ThumbnailSizeId != null)
            {
                xmlWriter.WriteStartElement("thumbnailSizeId");
                xmlWriter.WriteAttributeString("entityRef", "true");
                xmlWriter.WriteString(imageFormattingRule.ThumbnailSizeId.XmlSerializationText);
                xmlWriter.WriteEndElement(); // thumbnailSizeId            
            }

            xmlWriter.WriteEndElement(); // imageFormattingRule
        }


        /// <summary>
        /// Reconstructs a bar formatting rule from the specified XML image.
        /// </summary>
        /// <param name="node">The node containing the bar formatting rule XML.</param>
        /// <returns>The bar formatting rule.</returns>
        private static BarFormattingRule ReadBarFormattingRuleXml(XmlNode node)
        {
            BarFormattingRule formattingRule = new BarFormattingRule();

            if (XmlHelper.EvaluateSingleNode(node, "barFormattingRule"))
            {
                XmlNode barFormattingRuleNode = XmlHelper.SelectSingleNode(node, "barFormattingRule");
                if (barFormattingRuleNode != null)
                {
                    formattingRule.Color = ColorInfoHelper.ReadColorInfoXml(barFormattingRuleNode, "color");
                    formattingRule.Minimum = TypedValueHelper.ReadTypedValueXml(barFormattingRuleNode, "minimum");
                    formattingRule.Maximum = TypedValueHelper.ReadTypedValueXml(barFormattingRuleNode, "maximum");
                }
            }

            return formattingRule;
        }


        /// <summary>
        /// Reconstructs an image formatting rule from the specified XML image.
        /// </summary>
        /// <param name="node">The node containing the image formatting rule XML.</param>
        /// <returns>The image formatting rule.</returns>
        private static ImageFormattingRule ReadImageFormattingRuleXml(XmlNode node)
        {
            var formattingRule = new ImageFormattingRule();

            if (XmlHelper.EvaluateSingleNode(node, "imageFormattingRule"))
            {
                XmlNode imageFormattingRuleNode = XmlHelper.SelectSingleNode(node, "imageFormattingRule");
                if (imageFormattingRuleNode != null)
                {
                    XmlNode scaleIdNode = imageFormattingRuleNode.SelectSingleNode("thumbnailScaleId");
                    if (scaleIdNode != null &&
                        !string.IsNullOrEmpty(scaleIdNode.InnerText))
                    {
                        formattingRule.ThumbnailScaleId = EntityRef.FromXmlString(scaleIdNode.InnerText);
                    }

                    XmlNode sizeIdNode = imageFormattingRuleNode.SelectSingleNode("thumbnailSizeId");
                    if (sizeIdNode != null &&
                        !string.IsNullOrEmpty(sizeIdNode.InnerText))
                    {
                        formattingRule.ThumbnailSizeId = EntityRef.FromXmlString(sizeIdNode.InnerText);
                    }                    
                }
            }

            return formattingRule;
        }
        #endregion        
    }
}