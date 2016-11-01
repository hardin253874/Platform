// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.XmlUtil
{
    /// <summary>
    /// Utility for checking XML config files. Current checks include:
    /// - ensure every config entity has an alias.
    /// - ensure every alias also appears in the ugprade map
    /// </summary>
    class ConfigChecker
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="configPaths">The config paths.</param>
        /// <param name="mapFile">The map file.</param>
        public void CheckConfig(List<string> configPaths, string mapFile)
        {
            HashSet<string> aliases = new HashSet<string>();

            CheckAliases(configPaths, aliases);

            CheckMaps(mapFile, aliases);
        }


        /// <summary>
        /// Ensure every entity has an alias.
        /// </summary>
        /// <param name="configPaths">The config file paths.</param>
        /// <param name="aliases">Set that gets filled with the discovered alaises.</param>
        private static void CheckAliases(List<string> configPaths, HashSet<string> aliases)
        {
            // Process config files and get stream of entities
            IEnumerable<Entity> entities = XmlParser.ReadEntities(configPaths);
            var aliasResolver = new EntityStreamAliasResolver(entities);
            foreach (Entity e in entities)
            {
                // Ensure that every entity has an alias
                if (e.Alias == null || string.IsNullOrEmpty(e.Alias.Value))
                {
                    string msg = string.Format("{0}: error E1: Entity does not specify alias.", e.LocationInfo);
                    Console.WriteLine(msg);
                }
                else
                {
                    aliases.Add(e.Alias.Namespace + ":" + e.Alias.Value);
                }
            }
        }


        /// <summary>
        /// Ensure that every alias in the config is also in the upgrade map.
        /// </summary>
        private static void CheckMaps(string mapFile, ISet<string> configAliases)
        {
            var mapAliases = UpgradeMapTool.LoadMapFile(mapFile);

            foreach (string missing in configAliases.Except(mapAliases))
            {
                Console.WriteLine("{0}: error E2: Missing map for {1}", mapFile, missing);
            }

        }

    }

}
