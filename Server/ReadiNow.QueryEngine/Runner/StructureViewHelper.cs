// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using AggregateExpression = EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression;
using Entity = EDC.ReadiNow.Model.Entity;
using StructureViewExpression = EDC.ReadiNow.Metadata.Query.Structured.StructureViewExpression;

namespace ReadiNow.QueryEngine.Runner
{
    /// <summary>
    ///     Encapsulates structure level result data processing.
    /// </summary>
    internal class StructureViewHelper
    {
        private const char StructureLevelDelimiter = '\u0002';
        private const char StructureLevelPathDelimiter = '\u0003';

        private const string StructureLevelDelimiterXml = "&#x02;";
        private const string StructureLevelPathDelimiterXml = "&#x03;";


        /// <summary>
        ///     Regex used to get the ids from an encoded structure path.
        /// </summary>
        private static readonly Regex PathIdRegex = new Regex(string.Format(@"[{0}|{1}](?<id1>\d+):|^(?<id2>\d+):", StructureLevelDelimiter, StructureLevelPathDelimiter));


        /// <summary>
        ///     Regex used to get the ids from an encoded structure path which in turn is xml encoded.
        /// </summary>
        private static readonly Regex PathIdFromXmlRegex = new Regex(string.Format(@"[{0}|{1}](?<id1>\d+):|""(?<id2>\d+):", StructureLevelDelimiterXml, StructureLevelPathDelimiterXml));


        /// <summary>
        ///     Regex used to get the id from an encoded level.
        /// </summary>
        private static readonly Regex LevelIdRegEx = new Regex(@"(?<id>\d+):.*");


        /// <summary>
        ///     Initializes the <see cref="StructureViewHelper" /> class.
        /// </summary>
        static StructureViewHelper()
        {
            EntityAccessControlService = Factory.EntityAccessControlService;
        }


        /// <summary>
        ///     Used for security checks
        /// </summary>
        private static IEntityAccessControlService EntityAccessControlService { get; set; }


        /// <summary>
        ///     Secure the structure view results.
        /// </summary>
        /// <param name="settings">The query settings.</param>
        /// <param name="result">The query result.</param>
        /// <param name="dataTable">The query data.</param>
        internal static void SecureStructureViewData(QuerySettings settings, QueryResult result, DataTable dataTable)
        {
            using (Profiler.Measure("StructureViewHelper.SecureStructureViewData"))
            {
                // Skip applying security for aggregate queries
                if (result.AggregateColumns != null &&
                    result.AggregateColumns.Count > 0)
                {
                    return;
                }

                // Find the structure view columns
                List<StructureViewColumInfo> structureViewColumns = GetStructureViewColumns(result);

                // No structure view columns. Early out.
                if (structureViewColumns.Count == 0)
                {
                    return;
                }

                // Get the unsecured structure view ids from the data
                ISet<long> unsecuredLevelIds = GetStructureLevelIdsFromData(dataTable, structureViewColumns);

                // Secure and update the structure view data rows.
                SecureAndUpdateStructureViewData(settings, dataTable, structureViewColumns, unsecuredLevelIds);
            }
        }


        /// <summary>
        ///     Get the structure view column indexes.
        /// </summary>
        /// <param name="result">The query result.</param>
        /// <returns>The structure view column indexes.</returns>
        private static List<StructureViewColumInfo> GetStructureViewColumns(QueryResult result)
        {
            var structureViewColumnIndexes = new List<StructureViewColumInfo>();

            for (int i = 0; i < result.Columns.Count; i++)
            {
                ResultColumn resultColumn = result.Columns[i];

                if (resultColumn.RequestColumn == null)
                {
                    continue;
                }

                if (resultColumn.RequestColumn.Expression is StructureViewExpression)
                {
                    // Column returns an encoded structure view path
                    structureViewColumnIndexes.Add(new StructureViewColumInfo(i, false));
                }
                else if (resultColumn.RequestColumn.Expression is AggregateExpression)
                {
                    var aggregateExpression = resultColumn.RequestColumn.Expression as AggregateExpression;
                    if (aggregateExpression.Expression is StructureViewExpression &&
                        aggregateExpression.AggregateMethod == AggregateMethod.List)
                    {
                        // Column returns xml containing encoded structure view path. Yuck !
                        structureViewColumnIndexes.Add(new StructureViewColumInfo(i, true));
                    }
                }
            }

            return structureViewColumnIndexes;
        }


        /// <summary>
        ///     Gets the structure level ids from the result set.
        /// </summary>
        /// <param name="dataTable">The datatable.</param>
        /// <param name="structureViewColumns">The structure view columns.</param>
        /// <returns>The unsecured structure level ids.</returns>
        private static ISet<long> GetStructureLevelIdsFromData(DataTable dataTable, List<StructureViewColumInfo> structureViewColumns)
        {
            string[] namedGroups = {"id1", "id2"};
            var levelIds = new HashSet<long>();

            // Find all the structure level ids
            foreach (DataRow dataRow in dataTable.Rows)
            {
                foreach (StructureViewColumInfo columnInfo in structureViewColumns)
                {
                    if (columnInfo.ColumnIndex >= dataTable.Columns.Count)
                    {
                        continue;
                    }

                    if (dataRow.IsNull(columnInfo.ColumnIndex))
                    {
                        continue;
                    }

                    var value = dataRow[columnInfo.ColumnIndex] as string;

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    // Use the appropriate regex to get the entity ids
                    MatchCollection matches = !columnInfo.IsAggregateList ? PathIdRegex.Matches(value) : PathIdFromXmlRegex.Matches(value);

                    foreach (Match match in matches)
                    {
                        Match m = match;

                        foreach (string groupName in namedGroups.Where(groupName => m.Groups[groupName].Success))
                        {
                            long id;

                            if (long.TryParse(match.Groups[groupName].Value, out id))
                            {
                                levelIds.Add(id);
                            }
                        }
                    }
                }
            }

            return levelIds;
        }


        /// <summary>
        ///     Secure the structure level ids and update the structure view results.
        /// </summary>
        /// <param name="settings">The query settings.</param>
        /// <param name="dataTable">The datatable.</param>
        /// <param name="structureViewColumns">The structure view columns.</param>
        /// <param name="unsecuredLevelIds">The unsecured level ids.</param>
        private static void SecureAndUpdateStructureViewData(QuerySettings settings, DataTable dataTable, List<StructureViewColumInfo> structureViewColumns, ISet<long> unsecuredLevelIds)
        {
            if (unsecuredLevelIds.Count == 0 || !settings.SecureQuery || settings.RunAsUser == 0)
            {
                return;
            }

            // Get the level ids the specified user has access to
            IDictionary<long, bool> securedLevelIds = SecureEntityIds(settings.RunAsUser, unsecuredLevelIds);

            if (securedLevelIds.Values.All(v => v))
            {
                // Early out, have access to all
                return;
            }

            // Update the result set and remove any structure levels we don't have access to
            foreach (DataRow dataRow in dataTable.Rows)
            {
                foreach (StructureViewColumInfo columnInfo in structureViewColumns)
                {
                    if (dataRow.IsNull(columnInfo.ColumnIndex))
                    {
                        continue;
                    }

                    var value = dataRow[columnInfo.ColumnIndex] as string;

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    // Secure the path and return either an encoded path or xml containing the encoded path
                    string securedPath = columnInfo.IsAggregateList ? GetSecuredXmlPath(value, securedLevelIds) : GetSecuredPath(value, securedLevelIds);

                    if (securedPath != value)
                    {
                        dataRow[columnInfo.ColumnIndex] = securedPath;
                    }
                }
            }
        }


        /// <summary>
        ///     Gets the secured path from an unsecured path.
        /// </summary>
        /// <param name="unsecuredPaths"></param>
        /// <param name="securedLevelIds"></param>
        /// <returns></returns>
        private static string GetSecuredPath(string unsecuredPaths, IDictionary<long, bool> securedLevelIds)
        {
            if (string.IsNullOrEmpty(unsecuredPaths))
            {
                return unsecuredPaths;
            }

            string[] paths = unsecuredPaths.Split(StructureLevelPathDelimiter);

            var securedPaths = new List<string>();

            foreach (string path in paths)
            {
                // Reverse the path and keep adding to the beginning of the list until
                // we get to one we don't have access to.
                IEnumerable<string> levels = path.Split(StructureLevelDelimiter).Reverse();

                var securedLevels = new List<string>();
                var securedLevelsSet = new HashSet<string>();

                foreach (string level in levels)
                {
                    Match match = LevelIdRegEx.Match(level);

                    if (!match.Success || !match.Groups["id"].Success) continue;

                    long id;
                    if (!long.TryParse(match.Groups["id"].Value, out id))
                    {
                        continue;
                    }

                    bool haveAccess;
                    securedLevelIds.TryGetValue(id, out haveAccess);

                    if (!haveAccess)
                    {
                        // Stop at the first one we don't have access to
                        break;
                    }

                    if (!securedLevelsSet.Contains(level))
                    {
                        securedLevels.Insert(0, level);
                        securedLevelsSet.Add(level);
                    }
                }

                if (securedLevels.Count > 0)
                {
                    securedPaths.Add(string.Join(StructureLevelDelimiter.ToString(CultureInfo.InvariantCulture), securedLevels));
                }
            }

            string securedPath = string.Join(StructureLevelPathDelimiter.ToString(CultureInfo.InvariantCulture), securedPaths);
            return securedPath;
        }


        /// <summary>
        ///     Gets the secured xml path from an unsecured path.
        /// </summary>
        /// <param name="unsecuredPaths"></param>
        /// <param name="securedLevelIds"></param>
        /// <returns></returns>
        private static string GetSecuredXmlPath(string unsecuredPaths, IDictionary<long, bool> securedLevelIds)
        {
            if (string.IsNullOrEmpty(unsecuredPaths))
            {
                return unsecuredPaths;
            }

            var unsecuredToSecuredPath = new Dictionary<string, string>();

            using (var stringReader = new StringReader("<root>" + unsecuredPaths + "</root>"))
            {
                var xmlReaderSettings = new XmlReaderSettings {CheckCharacters = false};
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    XDocument xdoc = XDocument.Load(xmlReader);
                    if (xdoc.Root == null)
                    {
                        return unsecuredPaths;
                    }

                    IEnumerable<XElement> elements = xdoc.Root.Descendants("e");

                    foreach (XElement element in elements)
                    {
                        XAttribute textAttr = element.Attribute("text");
                        // Note this automatically unescapes the delimeters
                        string unsecuredPath = textAttr == null ? "" : textAttr.Value;
                        string[] paths = unsecuredPath.Split(StructureLevelDelimiter);

                        var securedPaths = new List<string>();

                        foreach (string path in paths)
                        {
                            // Reverse the path and keep adding to the beginning of the list until
                            // we get to one we don't have access to.
                            IEnumerable<string> levels = path.Split(StructureLevelPathDelimiter).Reverse();

                            var securedLevels = new List<string>();
                            var securedLevelsSet = new HashSet<string>();

                            foreach (string level in levels)
                            {
                                Match match = LevelIdRegEx.Match(level);

                                if (!match.Success || !match.Groups["id"].Success) continue;

                                long id;
                                if (!long.TryParse(match.Groups["id"].Value, out id))
                                {
                                    continue;
                                }

                                bool haveAccess;
                                securedLevelIds.TryGetValue(id, out haveAccess);

                                if (!haveAccess)
                                {
                                    // Stop at the first one we don't have access to
                                    break;
                                }

                                if (!securedLevelsSet.Contains(level))
                                {
                                    securedLevels.Insert(0, level);
                                    securedLevelsSet.Add(level);
                                }
                            }

                            if (securedLevels.Count > 0)
                            {
                                securedPaths.Add(string.Join(StructureLevelDelimiterXml, securedLevels));
                            }
                        }

                        string securedPath = string.Join(StructureLevelPathDelimiterXml, securedPaths);

                        // Escape the delimeters as the original string is escaped
                        unsecuredPath = unsecuredPath.Replace(StructureLevelDelimiter.ToString(CultureInfo.InvariantCulture), StructureLevelDelimiterXml);
                        unsecuredPath = unsecuredPath.Replace(StructureLevelPathDelimiter.ToString(CultureInfo.InvariantCulture), StructureLevelPathDelimiterXml);

                        if (unsecuredPath != securedPath)
                        {
                            // Store the old and new paths so they can be updated later.
                            unsecuredToSecuredPath[unsecuredPath] = securedPath;
                        }
                    }
                }
            }
            
            return unsecuredToSecuredPath.Aggregate(unsecuredPaths, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }


        /// <summary>
        ///     Secure the specified entity ids.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="entityIds">The entity ids to secure.</param>
        /// <returns></returns>
        private static IDictionary<long, bool> SecureEntityIds(long userId, IEnumerable<long> entityIds)
        {
            IDictionary<long, bool> securedIds;

            var userAccount = Entity.Get<UserAccount>(userId);

            using (new SetUser(userAccount))
            {
                // Ensure any security by pass is disabled
                using (new SecurityBypassContext(false))
                {
                    securedIds = EntityAccessControlService.Check(entityIds.Select(id => new EntityRef(id)).ToList(), new[] {Permissions.Read});
                }
            }

            return securedIds;
        }


        #region Nested type: StructureViewColumInfo

        /// <summary>
        /// Class used to hold structure view column info.
        /// </summary>
        private class StructureViewColumInfo
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="columnIndex">The column index.</param>
            /// <param name="isAggregateList">True if the column is an aggregate list, false otherwise.</param>
            public StructureViewColumInfo(int columnIndex, bool isAggregateList)
            {
                ColumnIndex = columnIndex;
                IsAggregateList = isAggregateList;
            }


            /// <summary>
            /// The column index.
            /// </summary>
            public int ColumnIndex { get; private set; }


            /// <summary>
            /// True if the column is an aggregate list.
            /// </summary>
            public bool IsAggregateList { get; private set; }
        }


        #endregion
    }
}