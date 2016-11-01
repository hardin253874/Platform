// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.XmlUtil
{
    /// <summary>
    /// Utility to automatically add (and remove) entries in the UpgradeMap.xml file so that it has
    /// exactly the same alises as appear in the main config files.
    /// </summary>
    static class UpgradeMapTool
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="configFiles">The XML config files.</param>
        /// <param name="upgradeMapFile">The upgrade map XML file.</param>
        public static void AddRemoveEntries(IEnumerable<string> configFiles, string upgradeMapFile, bool force)
        {
            // Load all aliases in config
            var configAliases = Program.FindAllAliases(configFiles);

            ISet<string> currentMap = LoadMapFile(upgradeMapFile);

            var add = configAliases.Except(currentMap).ToList();
            var remove = currentMap.Except(configAliases).ToList();

			bool addAny = add.Count > 0;
			bool removeAny = remove.Count > 0;

            if (addAny && removeAny && !force)
            {
                // Do not remove this check
                // It is here to prevent the scenario of an alias getting renamed without the upgrade ID code being preserved.
                Console.WriteLine("Cannot update map file automatically.");
                Console.WriteLine("Aliases need to be both added and removed.");
                foreach (string value in add)
                    Console.WriteLine("* need to add " + value);
                foreach (string value in remove)
                    Console.WriteLine("* need to remove " + value);
                Console.WriteLine("Update renames manually to preserve upgradeId, or use -f to force!");
                return;
            }

            if (addAny)
                AppendAliases(upgradeMapFile, add);

            if (removeAny)
                RemoveAliases(upgradeMapFile, remove);
        }

		/// <summary>
		//	Check for duplicates and output a warning. Doing this as it can happen during merges of the xml file. 
		//  And since it doesn't fail builds, only installs, this warning is to maybe help detect earlier.
		/// </summary>
		public static void CheckForDuplicates(string mapFile, bool force)
		{
			var aliasesVisited = new HashSet<string>();
			var nodesToRemove = new List<XmlElement>();
			var xmlDoc = new XmlDocument();

			xmlDoc.Load(mapFile);
			var rootElem = xmlDoc.DocumentElement;
	
			foreach (XmlElement elem in rootElem.SelectNodes("entity"))
			{
				string alias = elem.Attributes["alias"].Value;
				if (aliasesVisited.Contains(alias))
				{
					Console.WriteLine("Warning - the alias \"{0}\" occurs more than once", alias);
					nodesToRemove.Add(elem);
					if (force)
					{
						Console.WriteLine("Removed " + alias);
					}
				}
				else
				{
					aliasesVisited.Add(alias);
				}
			}
			if ( nodesToRemove.Count > 0 )
			{
				if (force)
				{
					foreach (XmlElement elem in nodesToRemove)
					{
						rootElem.RemoveChild(elem);
					}
					xmlDoc.Save(mapFile);
				}
				else
				{
					Console.WriteLine("Duplicates exist - use -force to remove");
				}
			}
		}

        /// <summary>
        /// Loads an upgrademap file and returns the set of aliases that it contains.
        /// </summary>
        public static ISet<string> LoadMapFile(string upgradeMapFile)
        {
            // Load all aliases in map
            XDocument xdoc = XDocument.Load(upgradeMapFile);
            ISet<string> currentMap = new HashSet<string>(xdoc.Root.Elements("entity").Select(x => x.Attribute("alias").Value));
            return currentMap;
        }


        /// <summary>
        /// Removes a list of aliases aliases from the upgrademap.
        /// </summary>
        /// <param name="upgradeMapFile">The upgrade map file.</param>
        /// <param name="aliasesToRemove">The aliases to remove.</param>
        private static void RemoveAliases(string upgradeMapFile, IEnumerable<string> aliasesToRemove)
        {
            var removeHash = new HashSet<string>(aliasesToRemove);
            var nodesToRemove = new List<XmlElement>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(upgradeMapFile);
            var rootElem = xmlDoc.DocumentElement;
            foreach (XmlElement elem in rootElem.SelectNodes("entity"))
            {
                string alias = elem.Attributes["alias"].Value;
                if (removeHash.Contains(alias))
                {
                    nodesToRemove.Add(elem);
                    Console.WriteLine("Removed " + alias);
                }
            }
            foreach (XmlElement elem in nodesToRemove)
            {
                rootElem.RemoveChild(elem);
            }
            xmlDoc.Save(upgradeMapFile);
        }


        /// <summary>
        /// Adds new aliases to the ugprade map. Generates a new upgrade-id for each new alias.s
        /// </summary>
        /// <param name="upgradeMapFile">The upgrade map file.</param>
        /// <param name="aliasesToAdd">The aliases to add.</param>
        private static void AppendAliases(string upgradeMapFile, IEnumerable<string> aliasesToAdd)
        {
            XElement upgradeMapElement = XElement.Load(upgradeMapFile);

            // Load existing entities
            var existingEntities = upgradeMapElement
                .Elements("entity")
                .Select(e => new { Alias = e.Attribute("alias").Value, UpgradeId = e.Attribute("upgradeId").Value });

            // Generate Guids for new aliases
            var newEntities = aliasesToAdd
                .Select(alias => new { Alias = alias, UpgradeId = Guid.NewGuid().ToString() });

            // Combine, sort, and format to XML
            var combinedSorted = existingEntities
                .Union(newEntities)
                .OrderBy(entity => entity.Alias)
                .Select(entity =>
                    new XElement("entity",
                        new XAttribute("alias", entity.Alias),
                        new XAttribute("upgradeId", entity.UpgradeId)));

            // Replace content and save
            upgradeMapElement.ReplaceAll(combinedSorted);
            upgradeMapElement.Save(upgradeMapFile);
        }
    }
}
