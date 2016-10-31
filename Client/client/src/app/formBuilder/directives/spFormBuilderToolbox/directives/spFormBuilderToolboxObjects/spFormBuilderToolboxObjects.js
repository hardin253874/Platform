// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp, spEntity, jsonInt, jsonBool, jsonLookup */

(function () {
    'use strict';

    /**
     * Module implementing a form builder toolbox objects control.
     * spFormBuilderToolboxObjects provides the toolbox for interacting with various objects.
     *
     * @module spFormBuilderToolboxObjects
     * @example

     Using the spFormBuilderToolboxObjects:

     &lt;sp-form-builder-toolbox-objects&gt;&lt;/sp-form-builder-toolbox-objects&gt

     */
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxObjects', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.common.spEntityService',
        'sp.common.spDialog',
        'sp.spNavHelper',
        'sp.navService',
        'mod.app.formBuilder.spNewTypeDialog',
        'sp.common.spEntityHelper',
        'mod.common.ui.spFocus',
        'mod.app.navigationProviders',
        'mod.common.spCachingCompile',
        'mod.app.editFormCache',
        'sp.app.settings'
    ]);

    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxObjects')
        .directive('spFormBuilderToolboxObjects', spFormBuilderToolboxObjects);

    /* @ngInject */
    function spFormBuilderToolboxObjects($q, spFormBuilderService, spNavService, spEntityService, spNavHelper, spDialog,
                                         spNewTypeDialog, spEntityHelper, spState, focus, spNavigationBuilderProvider, spCachingCompile, editFormCache, spAppSettings) {

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                objectTypeAlias: '=?',
                objectTypeName: '=?',
                disableAddNew: '=?'
            },            
            link: link
        };

        function link(scope, element) {

            var navigationBuilderProvider = spNavigationBuilderProvider(scope);

            // cache the results of requesting all types for an application
            var appObjects = {};

            // cache objects to prevent infinite apply loop
            var wrapperCache = {};

            scope.model = {
                showTypes: false,
                search: {
                    value: null,
                    id: 'searchObjects'
                },
                unassignedSoln: {
                    id: 0,
                    name: 'Unassigned',
                    entity: spEntity.fromJSON({
                        name: 'Unassigned',
                        type: 'core:solution'
                    })
                }
            };


            if (scope.disableAddNew) {
                scope.model.disableAddNew = scope.disableAddNew;
            }

            scope.spAppSettings = spAppSettings;
            scope.spNavService = spNavService;
            scope.spFormBuilderService = spFormBuilderService;
            scope.selectedApp = spFormBuilderService.selectedApp ? spFormBuilderService.selectedApp : undefined;
            scope.$watch('spNavService.getCurrentApplicationMenuEntity()', function (newValue) {
                if (!newValue) return;

                var soln = sp.result(newValue, 'inSolution');

                // Select current App
                if (!scope.selectedApp) {
                    setCurrentApp(soln);
                }
            });

            scope.componentFilterMode = 'all';    // all/public/private

            //
            // Reload if any entity is updated
            //
            scope.$on('sp.entity.updated', refreshSelectedApp);

            /////
            // Get list of solutions.
            /////
            scope.getApplications = function () {
                spEntityService.getInstancesOfType('core:solution', 'name,alias', {
                    hint: 'solutions',
                    batch: true
                }).then(function (result) {
                    scope.applications = result;
                    scope.applications.push(scope.model.unassignedSoln);
                    scope.applications = filterApps(scope.applications);
                    //select previously selected app, or current App, or whatever option is available
                    var soln = spFormBuilderService.selectedApp || sp.result(spNavService.getCurrentApplicationMenuEntity(), 'inSolution') || getSelectedApp();
                    if (scope.applications.length === 1) {
                        soln = scope.applications[0];
                    }
                    setCurrentApp(soln);
                });
            };

            function filterApps(apps) {
                if (spAppSettings.fullConfig)
                    return apps;
                // Self-serve users can't see these apps
                return _.filter(apps, function(app) {
                    return app.alias !== 'core:coreSolution' && app.alias !== 'core:consoleSolution' && app.name !== 'Unassigned';
                });
            }

            /////
            // Set current app, except for system apps
            /////
            function setCurrentApp(soln) {
                if (soln && soln.nsAlias !== 'core:consoleSolution') {
                    scope.selectedApp = _.find(scope.applications, function (app) {
                        return app.id === (soln.idP || soln.id);
                    });
                   spFormBuilderService.selectedApp = scope.selectedApp;
                }
            }

            /////
            // Load filtered objects on selection change.
            /////
            scope.$watch('selectedApp', loadSelectedApp);

            scope.$watch('model.showTypes', function () { loadSelectedApp(scope.selectedApp); });

            /////
            // Get filter string to load definitions.
            /////
            function getFilterString(app) {
                if (app.id === 0) {
                    return '[Resource in application] is null';
                } else {
                    return 'id([Resource in application])=' + app.id;
                }
            }

            /////
            // Toggle context menu
            /////
            scope.toggleContextMenu = {
                menuItems: [
                    {
                        text: 'Expand All',
                        type: 'click',
                        click: 'toggleExpansion(true)'
                    },
                    {
                        text: 'Collapse All',
                        type: 'click',
                        click: 'toggleExpansion(false)'
                    }
                ]
            };

            scope.addContextMenu = {
                menuItems: _.filter([
                    {
                        text: 'Add Object',
                        icon: 'assets/images/16x16/Add.png',
                        type: 'click',
                        click: 'newType()',
                        visible: spAppSettings.fullConfig
                    },
                    {
                        text: 'Add Report',
                        icon: 'assets/images/16x16/report_add.png',
                        type: 'click',
                        click: 'newReport()',
                        visible: true
                    },
                    {
                        text: 'Add Chart',
                        icon: 'assets/images/16x16/chart_add.png',
                        type: 'click',
                        click: 'newChart()',
                        visible: true
                    }
                ], 'visible')
            };

            /////
            // Expand/Collapse each types section.
            /////
            scope.toggleExpansion = function (open) {
                _.forEach(scope.model.types, function (type) {
                    type.expanded = !!open;
                });
            };

            /////
            // Request all objects and their various bits and pieces
            /////
            scope.requestObjects = function (filterString) {
                spFormBuilderService.screenObjectsLoading = true;

                var query =
                    'let @COMPONENT = {' +
                        'name, description,' +
                        'inSolution.id,' +
                        'isDefaultDisplayReportForTypes.id,' +
                        'isDefaultPickerReportForTypes.id,' +
                        'k:isDefaultForEntityType.id,' +
                        'isPrivatelyOwned' +
                    '} ' +
                    'let @TYPE = {' +
                        'name, description,' +
                        'inSolution.id,' +
                        'isOfType.{ name, alias },' +
                        'definitionUsedByReport.reportForAccessRule.id,' +
                        '{ definitionUsedByReport, definitionUsedByReport.reportCharts, k:formsToEditType, k:defaultEditForm }.@COMPONENT' +
                    '} @TYPE';
                var type = scope.objectTypeAlias || (scope.model.showTypes ? 'type' : 'managedType');
                var name = scope.objectTypeName || 'Object';
                var key = cacheKey(scope.selectedApp.id);

                spEntityService.getEntitiesOfType(type, query, {
                    filter: filterString,
                    hint: 'toolboxObjects',
                    batch: true
                }).then(function (typeEntities) {
                    //skip the choice field in object list as per #26915

                    var filterEntities = name === 'Object' ? _.filter(typeEntities, function (e) {
                        var entityType = !isUndefined(e) && !isUndefined(e.type) && !isUndefined(e.type.nsAlias) ? e.type.nsAlias : '';
                        return entityType !== 'core:enumType';
                    }) : typeEntities;

                    scope.model.types = wrapEntities(filterEntities, 'Type', name, null);
                    appObjects[key] = scope.model.types;
                }).finally(function () {
                    spFormBuilderService.screenObjectsLoading = false;
                });

            };

            function isUndefined(obj) {
                if (_.isNull(obj) || _.isUndefined(obj))
                    return true;
                else
                    return false;
            }


            scope.getEntity = function (key) {
                return wrapperCache[key];
            };

            /////
            // Returns the report entities for the specified type
            /////
            scope.getReportsForType = function (typeEntity) {
                var reports = filterAccessRuleReports(typeEntity.definitionUsedByReport);
                var wrappers = wrapEntities(reports, 'Report', 'Report', typeEntity);

                // Inject extra menu item
                _.forEach(wrappers, function (w) {
                    // Just do it once
                    if (w.extended) return;
                    w.extended = true;

                    //Add 'report properties' to the context menu.
                    w.contextMenu.menuItems.splice(1, 0, {
                        text: 'Report Properties',
                        icon: 'assets/images/16x16/properties.png',
                        type: 'click',
                        click: 'reportProperties(\'' + w.key + '\')'
                    });

                    //Add 'create chart' context menu to the end.
                    w.contextMenu.menuItems.push({
                        text: 'Create Chart',
                        icon: 'assets/images/chartType.png',
                        type: 'click',
                        click: 'newReportChart(\'' + w.key + '\')'
                    });
                });

                // If the search matches the type, show all children - otherwise only show matching children
                if (!scope.filterEntity(typeEntity, true))
                    wrappers = _.filter(wrappers, scope.filterEntity);

                return wrappers;
            };

            /////
            // Returns the chart entities for the specified type
            /////
            scope.getChartsForType = function (typeEntity) {
                var reports = filterAccessRuleReports(typeEntity.definitionUsedByReport);
                var charts = _.flatten(_.map(reports, 'reportCharts'));
                var wrappers = wrapEntities(charts, 'Chart', 'Chart', typeEntity);

                // If the search matches the type, show all children - otherwise only show matching children
                if (!scope.filterEntity(typeEntity, true))
                    wrappers = _.filter(wrappers, scope.filterEntity);

                return wrappers;
            };

            /////
            // Returns the form entities for the specified type
            /////
            scope.getFormsForType = function (typeEntity) {
                var forms = typeEntity.formsToEditType;
                var wrappers = wrapEntities(forms, 'Form', 'Form', typeEntity);

                // If the search matches the type, show all children - otherwise only show matching children
                if (!scope.filterEntity(typeEntity, true))
                    wrappers = _.filter(wrappers, scope.filterEntity);

                return wrappers;
            };

            ///////
            //// Predicate for displaying types
            ///////
            scope.filterType = function (type) {
                if (!type)
                    return false;
                if (scope.filterEntity(type.entity, true))
                    return true;

                var getters = [scope.getReportsForType, scope.getChartsForType, scope.getFormsForType];
                var res = _.some(getters, function (getter) {
                    var entities = getter(type.entity);
                    var res2 = _.some(entities, scope.filterEntity);
                    return res2;
                });
                return res;
            };

            ///////
            //// Predicate for filtering entity
            ///////
            scope.filterEntity = function (entity, isType) {
                if (!entity)
                    return false;
                if (entity.isWrapper)
                    return scope.filterEntity(entity.entity);
                var search = scope.model.search.value;
                if (search) {
                    var name = entity.name;
                    if (!name || name.toUpperCase().indexOf(search.toUpperCase()) === -1)
                        return false;
                }

                if (isType && scope.componentFilterMode !== 'all')
                    return false; // objects are always public, so never match then based on privacy level - always defer to individual components

                if (scope.componentFilterMode === 'private')
                    return !!entity.isPrivatelyOwned;
                if (scope.componentFilterMode === 'public')
                    return !entity.isPrivatelyOwned;
                return true;
            };

            /////
            // Drag options.
            /////
            scope.dragOptions = {
                onDragEnd: function (event, data) {
                    dragEnd(event, data);
                },
                onDragStart: function (event, data) {
                    dragStart(event, data);
                }
            };

            /**
             * Drag End handler.
             */

            function dragEnd() {
                /////
                // Perform cleanup
                /////
                spFormBuilderService.destroyInsertIndicator();
            }

            /**
             * Drag Start handler.
             */

            function dragStart() {
                /////
                // Remove these in drag start rather then drag end so that the drop handler has access to them.
                /////
                spFormBuilderService.currentDropTarget = undefined;
                spFormBuilderService.currentDropTargetIsField = undefined;
                spFormBuilderService.currentDropTargetQuadrant = undefined;
            }

            /////
            // Create a control to render a chart
            /////
            scope.getFormDragData = function (form) {
                return {
                    newControlFactory: function () {
                        return spEntity.fromJSON({
                            typeId: 'console:formRenderControl',
                            'k:formToRender': jsonLookup(form.entity),
                            'console:renderingOrdinal': jsonInt(),
                            'console:renderingWidth': jsonInt(),
                            'console:renderingHeight': jsonInt(),
                            'console:renderingBackgroundColor': 'white',
                            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:hideLabel': jsonBool(false),
                            name: form.entity.name
                        });
                    }
                };
            };

            /////
            // Create a control to render a chart
            /////
            scope.getChartDragData = function (chart) {
                return {
                    newControlFactory: function () {
                        return spEntity.fromJSON({
                            typeId: 'console:chartRenderControl',
                            'k:chartToRender': jsonLookup(chart.entity),
                            'console:renderingOrdinal': jsonInt(),
                            'console:renderingWidth': jsonInt(),
                            'console:renderingHeight': jsonInt(),
                            'console:renderingBackgroundColor': 'white',
                            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:hideLabel': jsonBool(false),
                            name: chart.entity.name
                        });
                    }
                };
            };

            /////
            // Create a control to render a report
            /////
            scope.getReportDragData = function (report) {
                return {
                    newControlFactory: function () {
                        return spEntity.fromJSON({
                            typeId: 'console:reportRenderControl',
                            'k:reportToRender': jsonLookup(report.entity),
                            'console:renderingOrdinal': jsonInt(),
                            'console:renderingWidth': jsonInt(),
                            'console:renderingHeight': jsonInt(),
                            'console:renderingBackgroundColor': 'white',
                            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:hideLabel': jsonBool(false),
                            name: report.entity.name
                        });
                    }
                };
            };

            /////
            // Create a new report
            /////
            scope.newReport = function (type) {
                var options = {};
                if (type) {
                    options.typeId = type.entity.id();
                }

                spState.scope.state = spFormBuilderService.getState();

                //the formbuilder and screenbuilder use same service, the getFormId() return screenbuilder id when builder type is screenBuilder
                //if create new report from screenbuilder window, after excute the createReport method, do not redirect to reportBuilder only refresh current screenbuilder
                if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen && spFormBuilderService.getFormId()) {
                    //set createFromScreenBuilder flag
                    spNavHelper.createReport(options, true, true);
                } else {
                    spNavHelper.createReport(options, true);
                }
            };

            /////
            // Edit existing report
            /////
            scope.editReport = function (reportKey) {
                spNavService.requireRefreshTree();

                var report = scope.getEntity(reportKey).entity;

                spState.scope.state = spFormBuilderService.getState();

                spNavService.navigateToChildState(
                    'reportBuilder',
                    report.idP);
            };

            /////
            // Create a new chart for a type
            /////
            scope.newChart = function (type) {
                var options = {};
                if (type) {
                    options.typeId = type.entity.id();
                }

                spState.scope.state = spFormBuilderService.getState();

                spNavHelper.createChart(options);
            };

            /////
            // open report properties dialog
            /////
            scope.reportProperties = function (reportKey) {
                var report = scope.getEntity(reportKey).entity;
                spNavService.setIsEditMode(true);
                navigationBuilderProvider.configureNavItem(report, null, 'core:report');
            };

            /////
            // Create a new chart for a report
            /////
            scope.newReportChart = function (reportKey) {
                var report = scope.getEntity(reportKey).entity;
                var options = {reportId: report.idP};

                spState.scope.state = spFormBuilderService.getState();

                spNavHelper.createChart(options);
            };

            /////
            // Edit existing chart
            /////
            scope.editChart = function (chartKey) {
                spNavService.requireRefreshTree();
                var chart = scope.getEntity(chartKey).entity;

                spState.scope.state = spFormBuilderService.getState();

                spNavService.navigateToChildState(
                    'chartBuilder',
                    chart.idP);
            };

            /////
            // Create a new edit form
            /////
            scope.newForm = function (typeInfo) {
                spState.scope.state = spFormBuilderService.getState();

                var type = typeInfo.entity;

                var baseFormName = 'New \'' + type.name + '\' Form';
                var formName = baseFormName;

                var form = spFormBuilderService.createForm(type, formName, 'A new user-created form.');

                var data = {
                    state: {
                        definition: type,
                        form: form,
                        isDefaultForm: false
                    }
                };

                spNavService.navigateToChildState('formBuilder', 0, undefined, data);
            };

            /////
            // Edit existing form
            /////
            scope.editForm = function (formKey) {
                var formWrapper = scope.getEntity(formKey);
                var form = formWrapper.entity;

                spState.scope.state = spFormBuilderService.getState();

                spNavService.navigateToChildState('formBuilder', form.idP);
            };

            /////
            // Create a new type (definition)
            /////
            scope.newType = function () {
                var options = {
                    types: scope.model.types
                };

                spNewTypeDialog.showDialog(options);
            };

            /////
            // Edit existing type
            /////
            scope.editType = function (typeKey) {
                var type = scope.getEntity(typeKey).entity;
                var params = {};
                var defaultFormId = sp.result(type, 'defaultEditForm.idP') || 0;
                params.definitionId = type.idP;

                spState.scope.state = spFormBuilderService.getState();

                spNavService.navigateToChildState('formBuilder', defaultFormId, params);
            };

            /////
            // Delete the entity.
            /////
            function deleteEntity(entityKey, action) {
                var wrapper = scope.getEntity(entityKey);
                spEntityHelper.promptDelete({entity: wrapper.entity}).then(function (res) {
                    if (res) {
                        spNavService.requireRefreshTree();
                        if (action) {
                            action(wrapper);
                        }
                    }
                });
            }

            /////
            // Delete the chart.
            /////
            scope.deleteChart = function (entityKey) {
                deleteEntity(entityKey, function (wrapper) {
                    var reports = wrapper.type.definitionUsedByReport;

                    _.find(reports, function (report) {
                        if (report.reportCharts.indexOf(wrapper.entity) >= 0) {
                            if (report.reportCharts.remove) {
                                report.reportCharts.remove(wrapper.entity);

                                return true;
                            }
                        }

                        return false;
                    });
                });
            };

            /////
            // Delete the form.
            /////
            scope.deleteForm = function (entityKey) {
                deleteEntity(entityKey, function (wrapper) {
                    var relContainer;

                    if (wrapper.type.formsToEditType.indexOf(wrapper.entity) >= 0) {
                        relContainer = wrapper.type.formsToEditType.getRelationshipContainer();
                        if (relContainer) {
                            _.remove(relContainer.entities, wrapper.entity);
                            _.remove(relContainer.instances, function(i) {
                                return i && (i.entity === wrapper.entity);
                            });
                        }                        
                    }
                });
            };

            /////
            // Delete the report.
            /////
            scope.deleteReport = function (entityKey) {
                deleteEntity(entityKey, function (wrapper) {
                    editFormCache.invalidateFormsForEntity(wrapper.entity.idP);

                    if (wrapper.type.definitionUsedByReport.indexOf(wrapper.entity) >= 0) {
                        if (wrapper.type.definitionUsedByReport.remove) {
                            wrapper.type.definitionUsedByReport.remove(wrapper.entity);
                        }
                    }
                });
            };

            /////
            // Delete the type.
            /////
            scope.deleteType = function (entityKey) {

                deleteEntity(entityKey, function (wrapper) {

                    var i = scope.model.types.indexOf(wrapper);

                    if (i !== -1)
                        scope.model.types.splice(i, 1);
                });
            };

            /////
            // Show option to display types
            /////
            scope.showTypesVisible = function () {
                var devMode = spAppSettings.initialSettings.devMode;
                var app = sp.result(scope, 'selectedApp.name');
                var res = devMode && app && (app.slice(0, 'ReadiNow'.length) === 'ReadiNow');
                return res;
            };

            scope.getComponentFilterText = function getComponentFilterText() {
                var mode = scope.componentFilterMode;
                if (mode === 'all')
                    return 'Public and personal';
                if (mode === 'public')
                    return 'Public only';
                if (mode === 'private')
                    return 'Personal only';
            };

            scope.toggleComponentFilter = function toggleComponentFilter() {
                var mode = scope.componentFilterMode;
                if (mode === 'all') {
                    scope.componentFilterMode = 'public';
                }
                else if (mode === 'public') {
                    scope.componentFilterMode = 'private';
                } else {
                    scope.componentFilterMode = 'all';
                }
            };

            /////
            // Initial load.
            /////
            scope.getApplications();

            focus('searchObjects');




            ///////////////////////////////////////////////////////////////////
            // Implementation functions
            //

            // beware - wrapEntities is called during digest cycles as it is indirectly used in binding expressions
            function wrapEntities(entities, typeName, displayName, type) {
                var result = _.map(entities, function (e) {
                    var key = typeName + e.idP;
                    var wrapper = wrapperCache[key];
                    if (!wrapper || wrapper.entity !== e) {
                        wrapper = {
                            isWrapper: true,
                            key: key,
                            entity: e,
                            col: entities,
                            name: e.name,
                            description: e.description,
                            itemClass: {
                                item: true,
                                'is-default': sp.result(e, 'isDefaultDisplayReportForTypes.length') > 0 || sp.result(e, 'isDefaultPickerReportForTypes.length') > 0 || e.isDefaultForEntityType !== null
                            },
                            type: type,
                            contextMenu: {
                                menuItems: [
                                    {
                                        text: 'Modify ' + displayName,
                                        icon: 'assets/images/16x16/edit.png',
                                        type: 'click',
                                        click: 'edit' + typeName + '(\'' + key + '\')'
                                    },
                                    {
                                        text: 'Delete ' + displayName,
                                        icon: 'assets/images/16x16/delete.png',
                                        type: 'click',
                                        click: 'delete' + typeName + '(\'' + key + '\')'
                                    }
                                ]
                            }
                        };
                        wrapperCache[key] = wrapper;
                    }
                    return wrapper;
                });
                return sp.naturalSort(result, function (w) {
                    return !w.name ? '' : w.name.toLowerCase();
                });
            }

            function getSelectedApp() {
                return spState.scope && spState.scope.state && spState.scope.state.selectedApp && spState.scope.state.selectedApp;
            }

            function loadSelectedApp(app) {
                if (app) {
                    spFormBuilderService.selectedApp = app;
                    var key = cacheKey(app.id);
                    if (!appObjects[key]) {
                        scope.requestObjects(getFilterString(app));
                    } else {
                        scope.model.types = appObjects[key];
                    }
                } else {
                    scope.model.types = null;
                }
            }

            function cacheKey(appId) {
                return appId + ',' + scope.model.showTypes;
            }

            function refreshSelectedApp() {
                // clear cache and reload
                if (scope.selectedApp) {
                    var key = cacheKey(scope.selectedApp.id);
                    appObjects[key] = null;
                }
                loadSelectedApp(scope.selectedApp);
            }

            // Filter reports that apply to access rules
            function filterAccessRuleReports(reports) {
                return _.filter(reports, function (r) {
                    return r && !r.reportForAccessRule;
                });
            }

            var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/directives/spFormBuilderToolboxObjects/spFormBuilderToolboxObjects.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });
        }
    }
}());