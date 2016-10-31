// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a form builder control.
    * spFormBuilder provides the canvas for building forms.
    *
    * @module spFormBuilder
    * @example

    Using the spFormBuilder:

    &lt;sp-form-builder&gt;&lt;/sp-form-builder&gt

    */
    angular.module('mod.app.chartBuilder.directives.spChartBuilder', [
        'mod.common.spEntityService',
        'mod.app.chartBuilder.services.spChartBuilderService',
        'mod.common.ui.spEditFormDialog',
        'sp.navService',
        'mod.common.alerts',
        'mod.common.ui.spChart',
        'mod.app.navigationProviders',
        'app.controls.dialog.spEntitySaveAsDialog'
    ])
        .directive('spChartBuilder', ['$state', '$q', 'spChartBuilderService', 'spNavService', 'spAlertsService', 'spEditFormDialog', 'spNavigationBuilderProvider', 'spEntitySaveAsDialog', 'spState', function ($state, $q, spChartBuilderService, spNavService, spAlertsService, spEditFormDialog, spNavigationBuilderProvider, spEntitySaveAsDialog, spState) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    model: '='
                },
                templateUrl: 'chartBuilder/directives/spChartBuilder/spChartBuilder.tpl.html',
                link: function (scope, $document) {

                    var navigationBuilderProvider = spNavigationBuilderProvider(scope);
                    
                    scope.svc = spChartBuilderService;
                    scope.model = scope.model || spChartBuilderService.tempModel();

                    scope.viewerOptions = {
                        mode: 'chartBuilder',
                        containerClass: '.chart-view'
                    };

                    scope.refreshChart = function() {
                        if (scope.model && scope.model.chart && scope.viewerOptions.refreshChart) {
                            scope.viewerOptions.refreshChart();
                        }
                    };

                    scope.dropOptions = spChartBuilderService.getDropOptions(function () { scope.$apply(); });

                    scope.$watch("model", scope.refreshChart);

                    scope.$watch("model.refresh", scope.refreshChart);

                    scope.modelCallback = function() {
                        return scope.model;
                    };

                    /////
                    // Buttons
                    /////

                    scope.onClose = function() {
                        //clean the parent navitem's params info, and replace by currentitem's params
                        spNavService.getParentItem().state.params = null;
                        var pageState = spState.getPageState(), navParams;                        

                        if (pageState.isSavingChartIntoNewContainer) {
                            // Chart builder was loaded as a result of creating a new chart via a save as into a new container
                            delete pageState.isSavingChartIntoNewContainer;
                            navParams = sp.result(spState, 'navItem.state.params');                            
                        }

                        if (navParams) {
                            spNavService.navigateToState('chart', navParams).then(function() {
                                spState.getPageState().syncNavTreeWithItem = true;
                            });
                        } else {
                            spNavService.navigateToParent();
                        }
                    };

                    scope.onProps = function () {
                        var options = {
                            title: 'Chart Properties',
                            entity: scope.model.chart,
                            form: 'core:chartPropertiesForm',
                            optionsEnabled:true,
                            formLoaded: function (form) {
								_.find(spEntityUtils.walkEntities(form), function (e) {
                                    return sp.result(e, 'relationshipToRender.nsAlias') === 'core:chartReport';
                                }).readOnlyControl = true;
                            }
                        };

                        spChartBuilderService.applyChange(scope.model, _.partial(spEditFormDialog.showDialog, options));
                    };

                    /////
                    // Save button
                    /////
                    scope.onSave = function() {
                        var name = scope.model.chart.name;
                        var isCreate = scope.model.chart.dataState === spEntity.DataStateEnum.Create;
                        spChartBuilderService.saveChart(scope.model)
                            .then(function (id) {
                                try {
                                    var message = 'Chart \'' + name + '\' saved';
                                    spAlertsService.addAlert(message, { severity: spAlertsService.sev.Success, expires: true });
                                    if (spNavService.getCurrentItem()) {
                                        spNavService.requireRefreshTree();
                                        spNavService.refreshTreeBranch(spNavService.getCurrentItem());
                                    }
                                } catch (e) {
                                    console.error('spChartBuilder.onSave:', e);
                                }
                                return id;
                            }, function (err) {
                                var message = err.status === 403 ? 'You do not have permission to modify ' + name : name + ' failed to save';
                                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Error, expires: false });
                                return -1;
                            })
                            .then(function (id) {
                                if (id === -1)
                                    return;
                                if (isCreate) {
                                    // Redirect
                                    spNavService.navigateToChildState('chartBuilder', id);
                                } else {
                                    // Reload
                                    spChartBuilderService.reloadChart(scope.model);
                                    //update navelement if its a chart.
                                    if(spNavService.getParentItem().state.name === 'chart')
                                      navigationBuilderProvider.updateNavItem(scope.model.chart);
                                }
                            });
                    };

                    scope.onSaveAs = function() {
                        var options = {
                            entity: scope.model.chart,
                            typeName: 'Chart',
                            canSetContainer: true
                        };

                        var container = spNavService.getCurrentItemContainer();
                        if (container) {
                            options.containerId = container.id;   
                        }

                        spEntitySaveAsDialog.showModalDialog(options)
                            .then(function(result) {
                                if (!result) {
                                    return;
                                }

                                var newChartId = result.entityId;
                                var containerId = result.containerId;

                                spNavService.requireRefreshTree();
                                spNavService.refreshTreeBranch(containerId ? { id: containerId } : spState.navItem).then(function() {
                                    if (!containerId) {
                                        return;
                                    }

                                    // Performing a save as to a new section
                                    var newChartNode = spNavService.findInTreeById(spNavService.getNavTree(), newChartId);
                                    if (newChartNode && sp.result(newChartNode, 'item.state.params')) {
                                        spNavService.navigateToState('chartBuilder', newChartNode.item.state.params).then(function() {
                                            spState.getPageState().isSavingChartIntoNewContainer = true;
                                        });
                                    } else {
                                        spNavService.navigateToSibling('chartBuilder', newChartId);
                                    }
                                });

                                if (!containerId) {
                                    spNavService.navigateToSibling('chartBuilder', newChartId);
                                }
                            });
                    };

                    /////
                    // Stops certain characters from being entered into the editable labels.
                    /////
                    scope.validateinput = function (evt) {
                        var e = evt || event;

                        if (e.shiftKey) {
                            switch (e.which) {
                                case 188: // <
                                case 190: // >
                                    e.stopPropagation();
                                    e.preventDefault();
                                    return false;
                            }
                        }

                        return true;
                    };

                    /////
                    // Change validate.
                    /////
                    scope.changeValidate = function (value) {

                        if (value) {
                            return value.replace(/[<>]+/g, '');
                        }
                        return value;
                    };

                    /////
                    // Gets whether this name is valid.
                    /////
                    scope.isValidName = function (newName, oldName) {

                        if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                            return true;
                        }

                        if (!newName) {
                            spAlertsService.addAlert('Invalid chart name specified.', { severity: spAlertsService.sev.Warning, expires: true });
                            return false;
                        }

                        return true;
                    };

                }
                // end link
                
            };

        }]);
}());