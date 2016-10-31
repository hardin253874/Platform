// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to navigation.
 * @module navigation
 */

(function () {
    "use strict";

    angular.module('sp.navService')
        .factory('spNavDataService', spNavDataService);

    /* @ngInject */
    function spNavDataService($q, $http, spEntityService, entityAliases, spWebService, consoleIconService, rnFeatureSwitch) {

        /**
         * AngularJS service to handle navigation entity tree related backend requests.
         *
         * This service returns tree like structures where a "node" is a map of the item and an
         * optional children array, and each child is itself a "node".
         *
         * @class spNavDataService
         * @memberof module:navigation
         */

        var aliases = entityAliases;

        // need to revisit the templating work. 
        var basicFields = 'alias,name,description,isOfType.{alias,name}';
        var behavior = basicFields + ',k:treeIconUrl,k:html5ViewId,k:consoleBehaviorHidden, k:treeIcon.{ name, imageBackgroundColor}';
        var navType = 'alias, name,k:typeConsoleBehavior.{' + behavior + '}';
        var report = 'reportUsesDefinition.{' + navType + '}';
        var navElementFields = basicFields + ',k:consoleOrder,isPrivatelyOwned, k:isTopMenuVisible,k:showApplicationTabs,inSolution.name,k:isAppTab,k:navigationElementIcon.{alias, name, imageBackgroundColor},hideOnDesktop,hideOnTablet,hideOnMobile, k:resourceInFolder.{alias, name}, reportUsesDefinition.{' + navType + '},chartReport.{' + report + '},boardReport.{' + report + '}';
        var behaviorRels = 'k:resourceConsoleBehavior,isOfType.k:typeConsoleBehavior';

        var treeQueryData = {
            basicFields: basicFields,
            navType: navType,
            report: report,
            navElementFields: navElementFields,
            behavior: behavior,
            behaviorRels: behaviorRels
        };

        //var treeQueryData = {
        //    basicFields: 'alias,name,description,isOfType.{alias,name}',
        //    navElementFields: '<%=basicFields%>,k:consoleOrder,k:isTopMenuVisible,k:showApplicationTabs,inSolution.name,k:isAppTab,k:navigationElementIcon.{alias, name, imageBackgroundColor},hideOnDesktop,hideOnTablet,hideOnMobile, k:resourceInFolder.{alias, name}, reportUsesDefinition.{<%=navType%>},chartReport.{<%=report%>},boardReport.{<%=report%>}',
        //    behavior: '<%=basicFields%>,k:treeIconUrl,k:html5ViewId,k:consoleBehaviorHidden, k:treeIcon.{ name, imageBackgroundColor}',
        //    navType: 'alias, name,k:typeConsoleBehavior.{<%=behavior%>}',
        //    report: 'reportUsesDefinition.{<%=navType%>}',
        //    behaviorRels: 'k:resourceConsoleBehavior,isOfType.k:typeConsoleBehavior'
        //};

        var expandTreeRequest = processTemplateOnce('' +
            'k:resourceInFolder*.{' +
            '  <%=navElementFields%>,' +
            '  {<%=behaviorRels%>}.{<%=behavior%>},' +
            '  k:folderContents.{ ' +
            '    <%=navElementFields%>,' +
            '    {<%=behaviorRels%>}.{<%=behavior%>}' +
            '  }' +
            '},' +
            'k:folderContents.{' +
            '  <%=navElementFields%>,' +
            '  {<%=behaviorRels%>}.{<%=behavior%>}' +
            '}', treeQueryData);
       
        var testSolutionImageEntity;

        var service = {
            getNavItemIconUrl: getNavItemIconUrl,
            getNavItemAppImageUrl: getNavItemAppImageUrl,
            navItemFromEntity: navItemFromEntity,
            getNavItems: getNavItems,
            getNavTreeExpanded: getNavTreeExpanded,
            entityToTree: entityToTree
        };

        return service;

        function getTestSolutionImageEntity() {
            return $q.when(testSolutionImageEntity || spEntityService.getEntity('k:topMenuImage_TestSolution', 'id', {
                    hint: 'testSolImg',
                    batch: true
                }).then(function (e) {
                    testSolutionImageEntity = e;
                    return e;
                }));
        }

        function getNavItemIconUrl(e) {
            if (!e) {
                return null;
            }
            return consoleIconService.getNavItemIconUrl(e);
        }        
        
        function getNavItemAppImageUrl(e) {
            if (e && e.navigationElementIcon) {
                return spWebService.getImageApiSmallUrl(e.navigationElementIcon.idP);
            } else {
                return null;
            }
        }

        /**
         * Return a navItem object based on the given navigation Entity and its related entities.
         *
         * @memberof module:navigation.spNavDataService
         */
        function navItemFromEntity(e) {

            var type = e.getLookup(aliases.isOfType);
            var typeBehaviors = type ? type.getRelationship(aliases.typeConsoleBehavior) : [];
            var behaviors = e.getRelationship(aliases.resourceConsoleBehavior);
            var typeBehavior = _.first(typeBehaviors);
            var behavior = _.first(behaviors);

            var firstHtml5ViewId = _.result(behavior, 'getHtml5ViewId');
            if (!firstHtml5ViewId) {
                // Fallback to type
                firstHtml5ViewId = _.result(typeBehavior, 'getHtml5ViewId');
            }

            var consoleBehaviorHidden = _.result(behavior, 'getConsoleBehaviorHidden');
            if (!consoleBehaviorHidden) {
                // Fallback to type
                consoleBehaviorHidden = _.result(typeBehavior, 'getConsoleBehaviorHidden');
            }

            var application = e.getLookup('inSolution');
            var folderContents = e.folderContents;
            var typeAlias = e.getType().alias();
            var item = {
                id: e.id(),
                alias: e.eid().getNsAlias(),
                name: e.getName(),
                order: e.getField(aliases.consoleOrder) || 0,
                typeAlias: typeAlias,
                hidden: consoleBehaviorHidden || (e.getType().getAlias() === 'topMenu' && !e.getField(aliases.isTopMenuVisible)) || rnFeatureSwitch.isAliasHidden(e.alias()),
                showTabs: true,
                viewType: firstHtml5ViewId || 'home',
                applicationId: application && application.id(),
                isAppTab: e.hasField('console:isAppTab') ? e.getIsAppTab() : false,
                isLeaf: (typeAlias !== 'console:topMenu' && typeAlias !== 'core:folder' && typeAlias !== 'console:navSection' && typeAlias !== 'console:privateContentSection') || // a bit hacky, but if you want to fix, ensure that isLeaf gets determined somehow, otherwise nav is slowed down
                (folderContents && folderContents.length === 0),     // if we don't have the relationship we don't know if we are a leaf.
                entity: e
            };

            Object.defineProperties(item, {
                'iconUrl': {
                    get: function () {
                        return service.getNavItemIconUrl(this.entity);
                    },
                    enumerable: true
                }
            });

            Object.defineProperties(item, {
                'appImageUrl': {
                    get: function () {
                        var url = service.getNavItemAppImageUrl(this.entity);

                        // override db until we get it changed
                        if (this.testSolutionEntity) {
                            url = spWebService.getImageApiUrl(this.testSolutionEntity.idP);
                        }

                        return url;
                    },
                    enumerable: true
                }
            });

            if (item.name === 'Test Solution') {
                // override db until we get it changed
                item.showTabs = true;
                getTestSolutionImageEntity().then(function (e) {
                    item.testSolutionEntity = e;
                });
            }

            return item;
        }

        function childNavElements(e) {
            return e.getRelationship(aliases.folderContents);
        }

        /**
         * @memberof module:navigation.spNavDataService
         * @inner
         */
        function makeTree(e, parent, parentNodes) {
            var currentParentNodes = {};

            if (!parentNodes) {
                parentNodes = {
                };
            }

            if (e) {                
                if (parent) {
                    // Set immediate parent
                    currentParentNodes[parent.id()] = true;
                    // Merge parents of parent
                    _.assign(currentParentNodes, parentNodes[parent.id()]);
                }

                // Check for circular references
                if (currentParentNodes[e.id()]) {
                    console.warn('Encountered node ' + e.id() + ' multiple times while building tree.');
                    return null;
                }

                // Assign parent nodes
                parentNodes[e.id()] = currentParentNodes;

                return {
                    item: navItemFromEntity(e),
                    children: _.compact(_.map(childNavElements(e), function (a) { 
                        return makeTree(a, e, parentNodes); 
                    }))
                };
            }

            return null;
        }

        /**
         * Convert the topMenu entity return from the tree webservice into 
         * an appropriate tree structure. This is exposed to help with automated
         * testing.
         *
         * @param {object} An entity.
         *
         * @memberof module:navigation.spNavDataService
         */
        function entityToTree(entity) {       
            var parentNodes = {};
            return {
                item: { entity: entity },
                children: _.compact(entity.getRelationship(aliases.instancesOfType).map(function (e) { return makeTree(e, entity, parentNodes); }))
            };
        }

        /**
         * Return promise for a tree node that is the given item item plus any immediate children.
         * If the item is undefined then we get the root node of the nav tree where children are
         * all the entities of type topMenu.
         *
         * @param {number|object} item - either null, or an id or an object with an id property
         *
         * @memberof module:navigation.spNavDataService
         */
        function getNavItems(item) {

            if (!item) {
                // we are getting the root items

                return $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/console/tree',
                    headers: spWebService.getHeaders()
                }).then(function (response) {
                    if (!response || !response.data) {
                        return $q.reject('Invalid response');
                    }

                    var entities = spEntity.entityDataVer2ToEntities(response.data);
                    if (!_.isArray(entities) || entities.length !== 1) {
                        return $q.reject('spEntity.entityDataVer2ToEntities did not return an array with a single element');
                    }
                    var entity = entities[0];

                    return entityToTree(entity);
                }, function (error) {
                    console.error('ERROR: getNavItems failed with error: ' + (sp.result(error, 'status') || error));
                    return $q.reject(error);
                });
            }
            if (item.id) {
                throw new Error('getNavItems: calling for a specific item is not implemented');
            }

            return $q.reject('getNavItems - invalid parent nav item argument: ' + item.toString());
        }

        /**
         * Requests for and builds a navigation tree expanded to the given node (or its container).
         * All children for any node retrieved are also included.
         *
         * @returns promise for a tree of navigation items
         *
         * @memberof module:navigation.spNavDataService
         */
        function getNavTreeExpanded(items) {

            if (!items || !items.length) {
                return $q.when(null);
            }

            var ids = _.map(items, 'id');

            return spEntityService.getEntities(ids, expandTreeRequest, {
                hint: 'navTreeExp',
                batch: true
            }).then(function (entities) {

                // This entity graph is the inverse of what we wish to return.
                // Whip up to the top of the tree following the resourceInFolder then build
                // recursively back down following folderContents

                // Immediately (before the loop) grab the container, if one.
                // This is so if there is no container then we have no tree.
                var entity = entities[0].getLookup(aliases.resourceInFolder);
                if (!entity) {
                    return null;
                }

                while (entity.getLookup(aliases.resourceInFolder)) {
                    entity = entity.getLookup(aliases.resourceInFolder);
                }

                var tree = makeTree(entity, null, {});

                tree.isLeaf = tree.children.length === 0;   // We no that no children means we are a leaf - (maketree can't always figure this out.)

                return tree;

            });
        }

        /** Internal helper to format entity info requests */
        //function processTemplate(template, data) {
        //    // run twice to allow one level of template parameters within the data
        //    return _.template(_.template(template)(data))(data);
        //}

        function processTemplateOnce(template, data) {
            // run once. parameters within the data already evaluated
            return _.template(template)(data);
        }
    }

})();

