// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.Common;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using EDC.ReadiNow.Metadata.Solutions.DataUpgrade;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model.EventClasses
{
    public class SolutionEventTarget : IEntityEventSave, IEntityEventDeploy, IEntityEventDelete, IEntityEventError, IEntityEventPublish, IEntityEventUpgrade
    {
        /// <summary>
        ///     The audit log event target
        /// </summary>
        private readonly AuditLogSolutionEventTarget _auditLogEventTarget = new AuditLogSolutionEventTarget(AuditLogInstance.Get());

        /////
        // New Solutions key.
        /////
        private const string NewSolutionsKey = "newSolutions";

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList();

            ProcessNewSolutions(enumerable, state);

            ProcessNameChanges(enumerable, state);

            foreach (long entityId in enumerable.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(true, entityId, state);
            }
        }

        /// <summary>
        /// Processes the name changes.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        private static void ProcessNameChanges(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            object dictObject;

            if (!state.TryGetValue("NameChanges", out dictObject))
            {
                return;
            }

            var nameChanges = (Dictionary<long, Tuple<string, string>>) dictObject;

            foreach (Solution solution in entities.Select(e => e.As<Solution>()))
            {
                Tuple<string, string> names;

                if (nameChanges.TryGetValue(solution.Id, out names))
                {
                    UpdateTabName(solution, names.Item1, names.Item2);
                    UpdateSectionName(solution, names.Item1, names.Item2);
                }
            }
        }

        /// <summary>
        /// Updates the name of the section.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="oldName">The old name.</param>
        private static void UpdateSectionName(Solution solution, string newName, string oldName)
        {
            IEnumerable<NavSection> navSections = Entity.GetInstancesOfType<NavSection>(true, "name, inSolution.isOfType.id");

            if (navSections != null)
            {
                foreach (NavSection navSection in navSections)
                {
                    if (navSection.InSolution != null && navSection.InSolution.Id == solution.Id && navSection.Name.Equals(oldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var section = navSection.AsWritable<NavSection>();
                        section.Name = newName;
                        section.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the name of the tab.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="oldName">The old name.</param>
        private static void UpdateTabName(Solution solution, string newName, string oldName)
        {
            IEnumerable<TopMenu> existingTopMenus = Entity.GetInstancesOfType<TopMenu>(false, "name, inSolution.isOfType.id");

            if (existingTopMenus != null)
            {
                foreach (TopMenu existingTopMenu in existingTopMenus)
                {
					if ( existingTopMenu.InSolution != null && existingTopMenu.InSolution.Id == solution.Id && existingTopMenu.Name.Equals( oldName, StringComparison.CurrentCultureIgnoreCase ) )
                    {
                        var topMenu = existingTopMenu.AsWritable<TopMenu>();
                        topMenu.Name = newName;
                        topMenu.Save();
                    }
                }
            }
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var enableAppLockdown = !Factory.FeatureSwitch.Get("disableAppLockdown");

            foreach (Solution solution in entities.Select(e => e.As<Solution>()))
            {
                AssignPackageId(solution);

                _auditLogEventTarget.GatherAuditLogEntityDetailsForSave(solution, state);

                if (enableAppLockdown)
                {
                    EnsureNotRemovingProtectedEntitiesFromSolution(solution);
                }                

                ValidateSolutionVersion(solution);

                TrackNewSolutions(state, solution);

                MakeNewSolutionModifiable(solution);
            }

            return false;
        }

        /// <summary>
        /// Makes the new solution modifiable.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <exception cref="System.Exception">Could not convert Solution entity to IEntityInternal.</exception>
        private static void MakeNewSolutionModifiable(Solution solution)
        {
            if (solution == null)
            {
                return;
            }

            var entityInternal = solution as IEntityInternal;

            if (entityInternal == null)
            {
                throw new Exception("Could not convert Solution entity to IEntityInternal.");
            }

            if (entityInternal.IsTemporaryId && solution.CanModifyApplication == null)
            {
                solution.CanModifyApplication = true;
            }
        }

        /// <summary>
        /// Tracks the name changes.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="solution">The solution.</param>
        private static void TrackNameChanges(IDictionary<string, object> state, Solution solution)
        {
            if (solution == null)
            {
                return;
            }

            var entityInternal = solution as IEntityInternal;

            string newName = null;
            string oldName = null;

            IEntityFieldValues changes;
            object dictObject;

            if (EntityFieldModificationCache.Instance.TryGetValue(new EntityFieldModificationCache.EntityFieldModificationCacheKey(entityInternal.ModificationToken), out changes))
            {
                if (changes.TryGetValue(Solution.Name_Field.Id, out dictObject))
                {
                    newName = (string) dictObject;
                }
            }

            if (string.IsNullOrEmpty(newName))
            {
                return;
            }

            if (EntityFieldCache.Instance.TryGetValue(solution.Id, out changes))
            {
                if (changes.TryGetValue(Solution.Name_Field.Id, out dictObject))
                {
                    oldName = (string) dictObject;
                }
            }

            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }

            if (!state.TryGetValue("NameChanges", out dictObject))
            {
                dictObject = new Dictionary<long, Tuple<string, string>>();
                state["NameChanges"] = dictObject;
            }

            var dict = (Dictionary<long, Tuple<string, string>>) dictObject;
            dict.Add(solution.Id, new Tuple<string, string>(newName, oldName));
        }

        /// <summary>
        ///     Assigns the package id.
        /// </summary>
        /// <param name="solution">The solution.</param>
        private static void AssignPackageId(Solution solution)
        {
            /////
            // Allocate a new package id if one is not already set.
            /////
            if (solution.PackageId == null)
            {
                solution.PackageId = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Creates the section.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="name">The name.</param>
        /// <param name="isAppTab">if set to <c>true</c> [is app tab].</param>
        /// <returns></returns>
        private static NavSection CreateSection(NavContainer parent, Solution solution, string name, bool isAppTab)
        {
            var section = new NavSection();

            if (solution != null)
            {
                section.InSolution = solution;
            }
            section.Name = name;
            section.ResourceInFolder.Add(parent);
            section.IsAppTab = isAppTab;

            return section;
        }

        /// <summary>
        /// Creates the solution tab.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="consoleOrder">The console order.</param>
        private static void CreateTab(Solution solution, int consoleOrder)
        {
            var tab = new TopMenu();

            tab.InSolution = solution;
            tab.Name = solution.Name;
            tab.ConsoleOrder = consoleOrder;
            tab.HideOnDesktop = solution.HideOnDesktop;
            tab.HideOnTablet = solution.HideOnTablet;
            tab.HideOnMobile = solution.HideOnMobile;
            tab.NavigationElementIcon = solution.ApplicationIcon;
            tab.IsTopMenuVisible = true;
            tab.Save();

            var appTabSection = CreateSection(tab.As<NavContainer>(), solution, solution.Name, true);
            appTabSection.Save();
        }

        /// <summary>
        ///     Determines if new tab needed.
        /// </summary>
        /// <param name="entities">The entities.</param>
        private static void DetermineIfNewTabNeeded(IEnumerable<IEntity> entities)
        {
            foreach (IEntity entity in entities)
            {
                var solution = entity.As<Solution>();
                int maxConsoleOrder = -1;

                if (solution != null && !string.IsNullOrEmpty(solution.Name))
                {
                    bool foundTopMenu = false;

                    IEnumerable<TopMenu> existingTopMenus = Entity.GetInstancesOfType<TopMenu>();

                    if (existingTopMenus != null)
                    {
                        foreach (TopMenu existingTopMenu in existingTopMenus)
                        {
                            if (existingTopMenu.ConsoleOrder != null)
                            {
                                var consoleOrder = existingTopMenu.ConsoleOrder.Value;
                                if (consoleOrder > maxConsoleOrder)
                                {
                                    maxConsoleOrder = consoleOrder;
                                }
                            }

                            if (existingTopMenu.InSolution != null && existingTopMenu.InSolution.Id == solution.Id)
                            {
                                foundTopMenu = true;
                            }
                        }
                    }

                    if (!foundTopMenu)
                    {
                        if (maxConsoleOrder < 0)
                        {
                            maxConsoleOrder = 0;
                        }
                        CreateTab(solution, maxConsoleOrder + 1);
                    }
                }
            }
        }

        /// <summary>
        ///     Processes the new solutions.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        private static void ProcessNewSolutions(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            object newSolutionsObject;

            if (state.TryGetValue(NewSolutionsKey, out newSolutionsObject))
            {
                var newSolutions = (List<IEntity>) newSolutionsObject;

                IList<IEntity> enumerable = entities as IList<IEntity> ?? entities.ToList();

                foreach (IEntity newSolution in newSolutions)
                {
                    long temporaryId = EventTargetStateHelper.GetIdFromTemporaryId(state, newSolution.Id);

                    IEntity entity = enumerable.FirstOrDefault(e => e.Id == temporaryId);

                    if (entity != null)
                    {
                        var solution = entity.As<Solution>();

                        if (solution == null)
                        {
                            continue;
                        }

                        if (solution.IsReadOnly)
                        {
                            solution = solution.AsWritable<Solution>();
                        }

                        SetInSolutionRelationship(solution);

                        solution.Save();
                    }
                }

                DetermineIfNewTabNeeded(enumerable);

                state.Remove(NewSolutionsKey);
            }
        }

        /// <summary>
        ///     Sets the in solution relationship.
        /// </summary>
        /// <param name="solution">The solution.</param>
        private static void SetInSolutionRelationship(Solution solution)
        {
            if (solution == null)
            {
                return;
            }

            solution.InSolution = solution;
        }

        /// <summary>
        ///     Tracks the new solutions.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="solution">The solution.</param>
        private static void TrackNewSolutions(IDictionary<string, object> state, Solution solution)
        {
            IEntityInternal entityInternal = solution as IEntityInternal;

            if (entityInternal == null)
            {
                throw new Exception("Could not convert Solution entity to IEntityInternal.");
            }
            if (entityInternal.IsTemporaryId)
            {
                List<IEntity> newSolutions;
                object newSolutionsObject;

                if (!state.TryGetValue(NewSolutionsKey, out newSolutionsObject))
                {
                    newSolutions = new List<IEntity>();
                    state[NewSolutionsKey] = newSolutions;
                }
                else
                {
                    newSolutions = (List<IEntity>) newSolutionsObject;
                }

                newSolutions.Add(solution);
            }
            else
            {
                TrackNameChanges(state, solution);
            }
        }

        /// <summary>
        ///     Validates the solution version.
        /// </summary>
        /// <param name="solution">The solution.</param>
        private void ValidateSolutionVersion(Solution solution)
        {
            IEntityInternal entityInternal = solution as IEntityInternal;
            string version;

            if (entityInternal == null)
            {
                throw new Exception("Could not convert Solution entity to IEntityInternal.");
            }

            if (entityInternal.TryGetField(Solution.SolutionVersionString_Field, out version))
            {
                if (!string.IsNullOrEmpty(version))
                {
                    Version ver;

                    string stringVersion = version.Trim().TrimStart('.').TrimEnd('.');

                    if (!Version.TryParse(stringVersion, out ver))
                    {
                        int intValue;

                        if (int.TryParse(stringVersion, out intValue))
                        {
                            ver = new Version(intValue, 0, 0, 0);
                        }
                        else
                        {
                            ver = new Version(1, 0, 0, 0);
                        }
                    }

                    int major = ver.Major >= 0 ? ver.Major : 1;
                    int minor = ver.Minor >= 0 ? ver.Minor : 0;
                    int build = ver.Build >= 0 ? ver.Build : 0;
                    int revision = ver.Revision >= 0 ? ver.Revision : 0;

                    var canonicalVersion = new Version(major, minor, build, revision);

                    string canonicalVersionString = canonicalVersion.ToString();

                    if (canonicalVersionString != version)
                    {
                        solution.SolutionVersionString = canonicalVersionString;
                    }
                }
            }
        }


        /// <summary>
        /// Called after deploying an application.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        public void OnAfterDeploy(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var enumerable = entities as IList<IEntity> ?? entities.ToList();

            var solutions = enumerable.Select(e => e.As<Solution>()).Where(s => s != null);
            foreach (var sol in solutions)
            {
                EventLog.Application.WriteInformation("Deployed application '{0}'.", sol.Name);
                _auditLogEventTarget.AuditLog.OnDeployApplication(true, sol.Name, sol.SolutionVersionString);
            }

            RemoveDanglingEntities();

            UpgradeTopMenuNavSections();

            FixNavigationSections();

            UpgradeSolutionData(solutions);

            AutoNumber.Upgrade(solutions);

            InvalidateBulkRequestCache();

            InvalidateMetadataCaches(); // we need to do this explicitly as the app import will not necessarily fire the normal event targets.

        }

        /// <summary>
        ///		Removes the dangling entities.
        /// </summary>
        private void RemoveDanglingEntities()
        {
            var upgradeIdsToDelete = new[]
            {
                new Guid("2D96806F-3FA5-4FD5-B755-A35AF2F5A5CE") // core:folderUniqueNameKey (Resource Key that is no longer in use)
            };

            long[] ids = upgradeIdsToDelete.Select(Entity.GetIdFromUpgradeId).Where(id => id >= 0).ToArray();

            if (ids.Length > 0)
            {
                Entity.Delete(ids);
            }
        }

        /// <summary>
        /// Invalidates the bulk request cache.
        /// </summary>
        private void InvalidateBulkRequestCache( )
        {
            long coreSolution;
            long consoleTopMenu;

            var invalidationIds = new List<long>( );

            if ( EntityIdentificationCache.TryGetId( new EntityAlias( "core", "solution" ), out coreSolution ) )
            {
                invalidationIds.Add( coreSolution );
            }

            if ( EntityIdentificationCache.TryGetId( new EntityAlias( "console", "topMenu" ), out consoleTopMenu ) )
            {
                invalidationIds.Add( consoleTopMenu );
            }

            //TODO: Can we now rely on the entity invalidation mechanism, or do we need to support that.
            //BulkResultCache.Invalidate( invalidationIds );
        }

        /// <summary>
        /// Invalidate all the metadata caches.
        /// </summary>
        private void InvalidateMetadataCaches()
        {
            MetadataCacheInvalidator.Instance.InvalidateMetadataCaches(RequestContext.TenantId);
        }


        /// <summary>
        /// Upgrades the top menu navigation sections.
        /// Inserts a new NavSection with IsAppTab true under the root topMenu and
        /// sets any existing navigation sections to be children of the newly created IsAppTab NavSection.
        /// </summary>
        private static void UpgradeTopMenuNavSections()
        {
            IEnumerable<TopMenu> topMenus = null;

            if (Entity.Exists(new EntityRef("console", "topMenu")))
            {
                topMenus = Entity.GetInstancesOfType<TopMenu>().ToList();
            }

            // Early out, there are no top menus
            if (topMenus == null)
            {
                return;
            }

            foreach (TopMenu topMenu in topMenus)
            {
                UpgradeTopMenuNavSections(topMenu);
            }
        }


        /// <summary>
        /// Upgrades the top menu navigation sections.
        /// </summary>
        /// <param name="topMenu">The top menu.</param>
        private static void UpgradeTopMenuNavSections(TopMenu topMenu)
        {
            NavSection appTabNavSection = null;
            var resourcesToMove = new List<Resource>();
            bool saveAppTab = false;

            if (topMenu.FolderContents == null)
            {
                return;
            }

            // Get all the immediate children of the top menu
            foreach (Resource resource in topMenu.FolderContents)
            {
                bool isAppTab = false;
                var navSection = resource.As<NavSection>();

                if (navSection != null && ((navSection.IsAppTab ?? false) || navSection.Name == topMenu.Name))
                {
                    isAppTab = true;
                }

                if (!isAppTab)
                {
                    resourcesToMove.Add(resource);
                }
                else
                {
                    if (appTabNavSection == null)
                    {
                        appTabNavSection = navSection.AsWritable<NavSection>();
                    }
                }
            }

            if (resourcesToMove.Count > 0)
            {
                // Clear the existing top menu items
                var topMenuWritable = topMenu.AsWritable<TopMenu>();
                var folderContents = topMenuWritable.FolderContents;
                folderContents.RemoveRange(resourcesToMove);
                topMenuWritable.FolderContents = folderContents;
                topMenuWritable.Save();
            }

            if (appTabNavSection == null)
            {
                // Create a new app tab as a child of the top menu
                appTabNavSection = CreateSection(topMenu.As<NavContainer>(), topMenu.InSolution, topMenu.Name, true);
                appTabNavSection.Name = topMenu.Name;
                saveAppTab = true;
            }

            if (resourcesToMove.Count > 0)
            {
                var folderContents = appTabNavSection.FolderContents;
                folderContents.AddRange(resourcesToMove);
                appTabNavSection.FolderContents = folderContents;
                saveAppTab = true;
            }

            if (saveAppTab)
            {
                appTabNavSection.Save();
            }
        }

        /// <summary>
        /// Perform any upgrades that are required to solution data.
        /// </summary>
        private static void UpgradeSolutionData(IEnumerable<Solution> solutions)
        {
        }


        /// <summary>
        /// Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            InvalidateBulkRequestCache();
            InvalidateMetadataCaches();

            foreach (long entityId in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(true, entityId, state);
            }
        }

        /// <summary>
        /// Called before deleting an enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>
        /// True to cancel the delete operation; false otherwise.
        /// </returns>
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var solution in entities.Select(entity => entity.As<Solution>()).Where(solution => solution != null))
            {
                // Save solution details required OnAfterDelete
                _auditLogEventTarget.GatherAuditLogEntityDetailsForDelete(solution, state);
            }

            return false;
        }

        /// <summary>
        /// Called after publishing an application.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnAfterPublish(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var enumerable = entities as IList<IEntity> ?? entities.ToList();

            foreach (IEntity entity in enumerable)
            {
                var sol = entity.As<Solution>();

                if (sol != null)
                {
                    _auditLogEventTarget.AuditLog.OnPublishApplication(true, sol.Name, sol.SolutionVersionString);
                }
            }
        }

        /// <summary>
        /// Called if a failure occurs saving the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnSaveFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities.Select(e => e.Id))
            {
                _auditLogEventTarget.WriteSaveAuditLogEntries(false, entity, state);
            }
        }

        /// <summary>
        /// Called if a failure occurs deleting the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnDeleteFailed(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (long entity in entities)
            {
                _auditLogEventTarget.WriteDeleteAuditLogEntries(false, entity, state);
            }
        }

        /// <summary>
        /// Called if a failure occurs deploying an application
        /// </summary>
        /// <param name="solutions">The solutions.</param>
        /// <param name="state">The state.</param>
        public void OnDeployFailed(IEnumerable<ISolutionDetails> solutions, IDictionary<string, object> state)
        {
            if (solutions == null)
            {
                return;
            }

            foreach (ISolutionDetails solutionDetails in solutions)
            {
                _auditLogEventTarget.AuditLog.OnDeployApplication(false, solutionDetails.Name, solutionDetails.Version);
            }
        }


        /// <summary>
        /// Called if a failure occurs publishing an application
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state.</param>
        public void OnPublishFailed(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
            {
                return;
            }

            foreach (var solution in entities.Select(entity => entity.As<Solution>()).Where(solution => solution != null))
            {
                _auditLogEventTarget.AuditLog.OnPublishApplication(false, solution.Name, solution.SolutionVersionString);
            }
        }

        /// <summary>
        /// Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before upgrade and after upgrade call backs.</param>
        public void OnAfterUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var enumerable = entities as IList<IEntity> ?? entities.ToList();

            var solutions = enumerable.Select(e => e.As<Solution>()).Where(s => s != null);

            AutoNumber.Upgrade(solutions);

            FixNavigationSections();

            InvalidateBulkRequestCache();
            InvalidateMetadataCaches();
        }

        /// <summary>
        /// Fixes the navigation sections.
        /// </summary>
        private static void FixNavigationSections()
        {
            var navSectionGuid = new Guid("67815A18-4F32-470E-9690-6B45CB2A55AC");

            FixNavSectionUpgradeId("Administration", navSectionGuid, "adminSection", "console");
            FixNavSectionParentFolder(new Guid("43F45B7A-0B51-4ED3-91A4-54FC494224B6"), navSectionGuid);
            FixNavSectionParentFolder(new Guid("7525D499-F690-4D0C-887E-7116F20EF2C7"), navSectionGuid);
        }

        /// <summary>
        /// Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save call backs.</param>
        /// <returns>
        /// True to cancel the upgrade operation; false otherwise.
        /// </returns>
        public bool OnBeforeUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }

        /// <summary>
        /// Fixes the navigation section parent folder.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="parent">The parent.</param>
        private static void FixNavSectionParentFolder(Guid child, Guid parent)
        {
            try
            {
                const string commandText = @"-- Fix NavSection Parent Folder
DECLARE @resourceInFolder BIGINT = dbo.fnAliasNsId( 'resourceInFolder', 'console', @tenantId )

DECLARE @childId BIGINT
DECLARE @parentId BIGINT
DECLARE @currentParentId BIGINT

IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

SELECT
	@childId = Id
FROM
	Entity
WHERE
	TenantId = @tenantId
	AND UpgradeId = @child

IF ( @childId IS NULL )
BEGIN
	RETURN
END

SELECT
	@parentId = Id
FROM
	Entity
WHERE
	TenantId = @tenantId
	AND UpgradeId = @parent

IF ( @parentId IS NULL )
BEGIN
	RETURN
END

MERGE
	Relationship AS t
USING (
	SELECT
		@tenantId,
		@resourceInFolder,
		@childId )
	AS s (
		TenantId,
		TypeId,
		FromId )
	ON (
		s.TenantId = t.TenantId
		AND s.TypeId = t.TypeId
		AND s.FromId = t.FromId )
WHEN MATCHED AND ( t.ToId <> @parentId ) THEN
	UPDATE SET ToId = @parentId
WHEN NOT MATCHED THEN
	INSERT (TenantId, TypeId, FromId, ToId )
	VALUES ( @tenantId, @resourceInFolder, @childId, @parentId );";

				long userId;
				RequestContext.TryGetUserId( out userId );

				using ( DatabaseContextInfo.SetContextInfo( "Fix nav section parent folder" ) )
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( var command = ctx.CreateCommand( commandText ) )
					{
						ctx.AddParameter( command, "@tenantId", System.Data.DbType.Int64, RequestContext.TenantId );
						ctx.AddParameter( command, "@child", System.Data.DbType.Guid, child );
						ctx.AddParameter( command, "@parent", System.Data.DbType.Guid, parent );
						ctx.AddParameter( command, "@context", System.Data.DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

						command.ExecuteNonQuery( );
					}
				}
            }
            catch (Exception exc)
            {
                EventLog.Application.WriteError("Failed to fix the navigation section parent folder '{0}'. {1}", child.ToString("B"), exc);
            }
        }

        /// <summary>
        ///		Fixes the navigation section upgrade identifier.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="upgradeId">The upgrade identifier.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="nameSpace">The name space.</param>
        private static void FixNavSectionUpgradeId(string name, Guid upgradeId, string alias, string nameSpace)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (upgradeId == Guid.Empty)
            {
                throw new ArgumentNullException("upgradeId");
            }

            if (string.IsNullOrEmpty(alias))
            {
                throw new ArgumentNullException("alias");
            }

            if (string.IsNullOrEmpty(nameSpace))
            {
                throw new ArgumentNullException("nameSpace");
            }

            try
            {
                const string commandText = @"-- Fix NavSection Update Id
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @navSection BIGINT = dbo.fnAliasNsId( 'navSection', 'console', @tenantId )
DECLARE @resourceInFolder BIGINT = dbo.fnAliasNsId( 'resourceInFolder', 'console', @tenantId )
DECLARE @topMenu BIGINT = dbo.fnAliasNsId( 'topMenu', 'console', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )

DECLARE @entityId BIGINT

IF ( @context IS NOT NULL )
BEGIN
	DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
	SET CONTEXT_INFO @contextInfo
END

SELECT
	@entityId = e.Id
FROM
	dbo.Entity e
JOIN
	dbo.Data_NVarChar n ON
		e.Id = n.EntityId
		AND e.TenantId = n.TenantId
		AND n.FieldId = @name
		AND n.Data_StartsWith = @targetName
JOIN
	dbo.Relationship t ON
		t.TenantId = n.TenantId
		AND t.TypeId = @isOfType
		AND t.FromId = n.EntityId
		AND t.ToId = @navSection
JOIN
	dbo.Relationship r ON
		r.TenantId = n.TenantId
		AND r.TypeId = @resourceInFolder
		AND r.FromId = n.EntityId
JOIN
	dbo.Relationship tt ON
		tt.TenantId = n.TenantId
		AND tt.TypeId = @isOfType
		AND tt.FromId = r.ToId
		AND tt.ToId = @topMenu
WHERE
	e.TenantId = @tenantId AND
	e.UpgradeId <> @upgradeId

IF ( @entityId IS NOT NULL )
BEGIN
	UPDATE
		dbo.Entity
	SET
		UpgradeId = @upgradeId
	WHERE
		Id = @entityId
		AND TenantId = @tenantId

	MERGE INTO
		[dbo].[Data_Alias] AS t
	USING ( 
		SELECT
			@entityId AS [EntityId],
			@tenantId AS [TenantId],
			@alias AS [FieldId],
			@targetAlias AS [Data],
			@targetNamespace AS [Namespace],
			0 AS [AliasMarkerId]
	) s ON s.EntityId = t.EntityId AND s.TenantId = t.TenantId AND s.FieldId = t.FieldId
	WHEN NOT MATCHED THEN
		INSERT ( [EntityId], [TenantId], [FieldId], [Data], [Namespace], [AliasMarkerId] )
		VALUES ( s.[EntityId], s.[TenantId], s.[FieldId], s.[Data], s.[Namespace], s.[AliasMarkerId] );
END";

				long userId;
				RequestContext.TryGetUserId( out userId );

				using (DatabaseContext ctx = DatabaseContext.GetContext())
                {
					using ( DatabaseContextInfo.SetContextInfo( "Fix nav section upgrade id" ) )
					using (var command = ctx.CreateCommand(commandText))
                    {
                        ctx.AddParameter(command, "@tenantId", System.Data.DbType.Int64, RequestContext.TenantId);
                        ctx.AddParameter(command, "@upgradeId", System.Data.DbType.Guid, upgradeId);
                        ctx.AddParameter(command, "@targetName", System.Data.DbType.String, name);
                        ctx.AddParameter(command, "@targetAlias", System.Data.DbType.String, alias);
                        ctx.AddParameter(command, "@targetNamespace", System.Data.DbType.String, nameSpace);
						ctx.AddParameter( command, "@context", System.Data.DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

						command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {
                EventLog.Application.WriteError("Failed to fix the navigation section '{0}'. {1}", name, exc);
            }
        }

        /// <summary>
        /// Ensures that entities being removed from the solution are not instances of the protected types
        /// </summary>.
        /// <param name="solution">The solution.</param>
        /// <exception cref="System.Exception">Could not convert Solution entity to IEntityInternal.</exception>
        private void EnsureNotRemovingProtectedEntitiesFromSolution(Solution solution)
        {
            if (solution == null)
            {
                return;
            }                      
          
            var entityInternal = solution as IEntityInternal;

            if (entityInternal == null)
            {
                throw new Exception("Could not convert solution entity to IEntityInternal.");
            }

            bool canModifyApplication = solution.CanModifyApplication ?? false;
            if (entityInternal.IsTemporaryId || canModifyApplication)
            {
                // If the solution is a new solution or if it is modifiable return
                return;
            }

            // Check if global or system administrator            
            var userId = RequestContext.GetContext().Identity.Id;

            if (userId == 0)
            {
                return;
            }            

            // Get the relationship changes
            IEntityFieldValues changedFields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedFwdRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> changedRevRelationships;

            solution.GetChanges(out changedFields, out changedFwdRelationships, out changedRevRelationships, false, false);

            if (changedRevRelationships == null || changedRevRelationships.Count == 0)
            {
                // No changes
                return;
            }                        

            var inSolutionRemovals = GetInSolutionRemovals(changedRevRelationships, Solution.InSolution_Field.Id);
            var indirectInSolutionRemovals = GetInSolutionRemovals(changedRevRelationships, Solution.IndirectInSolution_Field.Id);

            if (inSolutionRemovals != null || indirectInSolutionRemovals != null)
            {
                // Get the protected type ids
                var protectedTypeIds = SystemAccessRules.AdministratorInModifiableSolutionTypes.Select(EntityIdentificationCache.GetId).ToSet();

                EnsureNotRemovingProtectedEntitiesFromSolution(inSolutionRemovals, protectedTypeIds);
                EnsureNotRemovingProtectedEntitiesFromSolution(indirectInSolutionRemovals, protectedTypeIds);
            }            
        }

        /// <summary>
        /// Gets the in solution removals.
        /// </summary>
        /// <param name="changedRelationships">The changed relationships.</param>
        /// <param name="inSolutionRelationshipId">The in solution relationship identifier.</param>
        /// <returns></returns>
        private IEnumerable<IMutableIdKey> GetInSolutionRemovals(IDictionary<long, IChangeTracker<IMutableIdKey>> changedRelationships, long inSolutionRelationshipId)
        {
            IChangeTracker<IMutableIdKey> inSolutionChanges;

            if (!changedRelationships.TryGetValue(inSolutionRelationshipId, out inSolutionChanges)) return null;

            // Check for removals only
            if (inSolutionChanges.Removed == null || !inSolutionChanges.Removed.Any()) return null;

            return inSolutionChanges.Removed;
        }


        /// <summary>
        /// Ensures that entities being removed from the solution are not instances of the protected types.
        /// </summary>
        /// <param name="inSolutionRemovals"></param>
        /// <param name="protectedTypeIds">The protected type ids.</param>
        /// <exception cref="PlatformSecurityException"></exception>
        /// <exception cref="EntityRef"></exception>
        private void EnsureNotRemovingProtectedEntitiesFromSolution(IEnumerable<IMutableIdKey> inSolutionRemovals, ISet<long> protectedTypeIds)
        {
            if (inSolutionRemovals == null)
            {
                return;
            }

            // Get the entities being removed from the solution
            var removedEntities = Entity.Get(inSolutionRemovals.Select(k => k.Key));

            var protectableTypeId = ProtectableType.ProtectableType_Type.Id;

            foreach (var removedEntity in removedEntities)
            {
                // Get its types
                var entityTypes = new HashSet<long>(PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(removedEntity));                

                // Check if it is any of the protected types                
                if (entityTypes.Intersect(protectedTypeIds).Any())
                {
                    if (entityTypes.Contains(protectableTypeId))
                    {
                        // Is a protectable type
                        var protectableType = removedEntity.As<ProtectableType>();
                        if (protectableType != null &&
                            protectableType.CanModifyProtectedResource == true)
                        {
                            continue;
                        }
                    }

                    throw new PlatformSecurityException(RequestContext.GetContext().Identity.Name, new[] {Permissions.Modify}, new[] {new EntityRef(removedEntity)});
                }
            }
        }
    }
}