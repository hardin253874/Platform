// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    ///     System access rule query attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class AccessRuleQueryDefinitionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessRuleQueryDefinitionAttribute" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="permission">The permission.</param>
        /// <param name="getStructuredQueryMethodName">Name of the get structured query method.</param>
        /// <param name="ignoreForReports"></param>
        public AccessRuleQueryDefinitionAttribute(string subject, string permission, string getStructuredQueryMethodName, bool ignoreForReports = false)
        {
            Subject = subject;
            Permissions = new List<string> {permission};
            GetStructuredQueryMethodNames = new List<string> {getStructuredQueryMethodName};
            IgnoreForReports = ignoreForReports;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessRuleQueryDefinitionAttribute" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="permission">The permission.</param>
        /// <param name="getStructuredQueryMethodNames">The get structured query method names.</param>
        /// <param name="ignoreForReports"></param>
        public AccessRuleQueryDefinitionAttribute(string subject, string permission, string[] getStructuredQueryMethodNames, bool ignoreForReports = false)
        {
            Subject = subject;
            Permissions = new List<string> {permission};
            GetStructuredQueryMethodNames = new List<string>(getStructuredQueryMethodNames);
            IgnoreForReports = ignoreForReports;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessRuleQueryDefinitionAttribute" /> class.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="permissions"></param>
        /// <param name="getStructuredQueryMethodName"></param>
        /// <param name="ignoreForReports"></param>
        public AccessRuleQueryDefinitionAttribute(string subject, string[] permissions, string getStructuredQueryMethodName, bool ignoreForReports = false)
        {
            Subject = subject;
            Permissions = new List<string>(permissions);
            GetStructuredQueryMethodNames = new List<string> {getStructuredQueryMethodName};
            IgnoreForReports = ignoreForReports;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessRuleQueryDefinitionAttribute" /> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="getStructuredQueryMethodNames">The get structured query method names.</param>
        /// <param name="ignoreForReports"></param>
        public AccessRuleQueryDefinitionAttribute(string subject, string[] permissions, string[] getStructuredQueryMethodNames, bool ignoreForReports = false)
        {
            Subject = subject;
            Permissions = new List<string>(permissions);
            GetStructuredQueryMethodNames = new List<string>(getStructuredQueryMethodNames);
            IgnoreForReports = ignoreForReports;
        }

        /// <summary>
        ///     Gets the subject.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        ///     Gets the permissions.
        /// </summary>
        public IEnumerable<string> Permissions { get; }

        /// <summary>
        ///     Gets the structured query creation function names.
        /// </summary>
        public IEnumerable<string> GetStructuredQueryMethodNames { get; }

        /// <summary>
        ///     Gets a value indicating whether to ignore for reports.
        /// </summary>
        /// <value>
        ///     <c>true</c> if ignore for reports; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreForReports { get; }
    }
}