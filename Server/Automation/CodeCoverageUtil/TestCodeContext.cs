// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeCoverageUtil
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class TestCodeContext
    {
        #region Constants
        /// <summary>
        /// 
        /// </summary>
        private const string MethodRegExPattern = @"^\s*(\S+\s+)?\s*(\S+\s+)?\s*\S+\s+(?<MethodName>\S+)\s*\(.*\)\s*";


        /// <summary>
        /// 
        /// </summary>
        private const string ClassRegExPattern = @"^\s*(\S+\s+)?(\S+\s+)?\s*class\s+(?<ClassName>\S+)\s*";


        /// <summary>
        /// 
        /// </summary>
        private const string NamespaceRegExPattern = @"^\s*namespace\s+(?<NamespaceName>\S+)\s*";
        #endregion


        #region Properties
        /// <summary>
        /// Gets the method name qualified.
        /// </summary>
        public string MethodNameQualified { get; private set; }


        /// <summary>
        /// Gets or sets the class name qualified.
        /// </summary>
        /// <value>
        /// The class name qualified.
        /// </value>
        public string ClassNameQualified { get; set; }


        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public string MethodName { get; private set; }


        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        /// <value>
        /// The name of the class.
        /// </value>
        public string ClassName { get; set; }


        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        /// <value>
        /// The name of the namespace.
        /// </value>
        public string NamespaceName { get; private set; }


        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; private set; }
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCodeContext"/> class.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="testAssemblyFilePath">The test assembly file path.</param>
        public TestCodeContext(string sourceFilePath, int lineNumber, string testAssemblyFilePath)
        {
            InitialiseContext(sourceFilePath, lineNumber);
            IsValid = IsContextValid(testAssemblyFilePath);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(MethodNameQualified))
            {
                return MethodNameQualified;
            }
            else if (!string.IsNullOrEmpty(ClassNameQualified))
            {
                return ClassNameQualified;
            }
            else if (!string.IsNullOrEmpty(NamespaceName))
            {
                return NamespaceName;
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion


        #region Non-Public Methods
        /// <summary>
        /// Initialises the context.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="lineNumber">The line number.</param>
        private void InitialiseContext(string sourceFilePath, int lineNumber)
        {
            if (lineNumber < 0)
            {
                throw new ArgumentOutOfRangeException("lineNumber");
            }

            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentNullException("sourceFileName");
            }

            string[] lines = File.ReadAllLines(sourceFilePath);

            if (lineNumber > lines.Length - 1)
            {
                throw new ArgumentOutOfRangeException("lineNumber", "The lineNumber exceeds the total lines in the file");
            }

            string selectedLine = lines[lineNumber - 1];

            string methodName = string.Empty;
            string className = string.Empty;
            string namespaceName = string.Empty;
            bool foundClassName = false;
            bool foundNamespaceName = false;

            bool isMethodDef = TryGetMethodName(selectedLine, out methodName);
            bool isClassDef = TryGetClassName(selectedLine, out className);
            bool isNamespaceDef = TryGetNamespaceName(selectedLine, out namespaceName);

            if (isClassDef)
            {
                foundClassName = true;
            }
            else if (isNamespaceDef)
            {
                foundClassName = true;
                foundNamespaceName = true;
            }

            if (isMethodDef ||
                isClassDef ||
                isNamespaceDef)
            {
                while (lineNumber >= 0 &&
                       (!foundClassName || !foundNamespaceName))
                {
                    string line = lines[lineNumber];

                    if (!foundClassName)
                    {
                        string temp;
                        // Search for the first line that matches a class definition
                        if (TryGetClassName(line, out temp))
                        {
                            if (IsClassTextFixture(lineNumber, lines))
                            {
                                foundClassName = true;
                                className = temp;   
                            }                            
                        }
                    }

                    if (!foundNamespaceName)
                    {
                        string temp;
                        // Search for the first line that matches a class definition
                        if (TryGetNamespaceName(line, out temp))
                        {
                            foundNamespaceName = true;
                            namespaceName = temp;
                        }
                    }

                    lineNumber--;
                }

                if (!string.IsNullOrEmpty(methodName) ||
                    !string.IsNullOrEmpty(className) ||
                    !string.IsNullOrEmpty(namespaceName))
                {
                    InitialiseProperties(methodName, className, namespaceName);
                }
            }
        }


        private bool IsClassTextFixture(int lineNumber, string[] lines)
        {
            lineNumber--;

            if (lineNumber < 0)
            {
                return false;
            }

            string line = lines[lineNumber];

            return line.Trim().StartsWith(@"[TestFixture");
        }


        /// <summary>
        /// Initialises the properties.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="namespaceName">Name of the namespace.</param>
        private void InitialiseProperties(string methodName, string className, string namespaceName)
        {
            MethodName = methodName;
            ClassName = className;
            NamespaceName = namespaceName;

            MethodNameQualified = string.Empty;
            ClassNameQualified = string.Empty;

            StringBuilder fqNameBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(namespaceName))
            {
                fqNameBuilder.Append(namespaceName);
            }
            if (!string.IsNullOrEmpty(className))
            {
                if (fqNameBuilder.Length > 0)
                {
                    fqNameBuilder.Append(".");
                }
                fqNameBuilder.Append(className);
                ClassNameQualified = fqNameBuilder.ToString();
            }
            if (!string.IsNullOrEmpty(methodName))
            {
                if (fqNameBuilder.Length > 0)
                {
                    fqNameBuilder.Append(".");
                }
                fqNameBuilder.Append(methodName);
                MethodNameQualified = fqNameBuilder.ToString();
            }
        }


        /// <summary>
        /// Determines whether [is context valid] [the specified test assembly file path].
        /// </summary>
        /// <param name="testAssemblyFilePath">The test assembly file path.</param>
        /// <returns>
        ///   <c>true</c> if [is context valid] [the specified test assembly file path]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsContextValid(string testAssemblyFilePath)
        {
            using (AssemblyLoader loader = new AssemblyLoader())
            {
                return loader.IsTestContextValid(this, testAssemblyFilePath);
            }
        }


        /// <summary>
        /// Tries the match regex.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="regExPattern">The reg ex pattern.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="groupValue">The group value.</param>
        /// <returns></returns>
        private static bool TryMatchRegex(string line, string regExPattern, string groupName, out string groupValue)
        {
            bool isMatch = false;
            groupValue = string.Empty;

            Regex regex = new Regex(regExPattern);
            Match match = regex.Match(line);

            isMatch = match.Success;
            if (isMatch)
            {
                groupValue = match.Groups[groupName].Value;
            }

            return isMatch;
        }


        /// <summary>
        /// Tries the name of the get method.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static bool TryGetMethodName(string line, out string methodName)
        {
            return TryMatchRegex(line, MethodRegExPattern, "MethodName", out methodName);
        }


        /// <summary>
        /// Tries the name of the get class.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        private static bool TryGetClassName(string line, out string className)
        {
            return TryMatchRegex(line, ClassRegExPattern, "ClassName", out className);
        }


        /// <summary>
        /// Tries the name of the get namespace.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="namespaceName">Name of the namespace.</param>
        /// <returns></returns>
        private static bool TryGetNamespaceName(string line, out string namespaceName)
        {
            return TryMatchRegex(line, NamespaceRegExPattern, "NamespaceName", out namespaceName);
        }
        #endregion
    }
}
