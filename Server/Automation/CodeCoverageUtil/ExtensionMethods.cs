// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace CodeCoverageUtil
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets the specified attribute as a boolean.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public static bool GetAttributeAsBool(this XPathNavigator reader, string name, bool defaultValue)
        {
            bool result = defaultValue;

            if (!bool.TryParse(reader.GetAttribute(name, string.Empty), out result))
            {
                result = defaultValue;
            }

            return result;
        }


        /// <summary>
        /// Gets the specified attribute as a int.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public static int GetAttributeAsInt(this XPathNavigator reader, string name, int defaultValue)
        {
            int result = defaultValue;

            if (!int.TryParse(reader.GetAttribute(name, string.Empty), out result))
            {
                result = defaultValue;
            }

            return result;
        }
    }
}
