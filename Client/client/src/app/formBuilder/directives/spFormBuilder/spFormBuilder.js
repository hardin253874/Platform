// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
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
    angular.module('mod.app.formBuilder.directives.spFormBuilder', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.common.spEntityService',
        'mod.app.formBuilder.spTypeProperties',
        'sp.navService',
        'mod.common.alerts',
        'mod.app.formBuilder.spFormSaveAsDialog',
        'mod.common.ui.spMeasureArrange',
        'mod.app.navigationProviders',
        'mod.app.editFormCache',
        'mod.app.configureDialogs.spFormActionsDialog',
        'mod.common.ui.spActionsService',
        'mod.featureSwitch'
    ])
        .directive('spFormBuilder', spFormBuilder);

    function spFormBuilder($state, $stateParams, spFormBuilderService, spEntityService, spNavService, $timeout, $q,
                           spTypeProperties, spAlertsService, spState, spFormSaveAsDialog, titleService, navDirtyMessage,
                           spMeasureArrangeService, spNavigationBuilderProvider, editFormCache, spFormActionsDialog,
                           spActionsService, rnFeatureSwitch) {

        'ngInject';

        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {},
            templateUrl: 'formBuilder/directives/spFormBuilder/spFormBuilder.tpl.html',
            link: function(scope) {

                //temp while working on builder and layout
                scope.allowFlexForm = rnFeatureSwitch.isFeatureOn('flexEditForm');
                scope.showStructureEditor = false;//scope.allowFlexForm;
                scope.toggleFormStructureEditor = function () {
                    if (!scope.allowFlexForm) return;
                    scope.showStructureEditor = !scope.showStructureEditor;
                    //doLayout();
                };
                //end temp

                var navigationBuilderProvider = spNavigationBuilderProvider(scope);
                /////
                // Ensure a model is present.
                /////
                scope.model = scope.model || {};

                var navItem = spNavService.getCurrentItem();
                var setBookmark = false;
                var forceReload = false;

                initialize();

                scope.$on('measureArrangeComplete', function () {
                    if (!spFormBuilderService.initialFormBookmark || setBookmark) {
                        spFormBuilderService.setInitialFormBookmark();
                        setBookmark = false;
                    }
                });

                scope.spFormBuilderService = spFormBuilderService;

                navItem.isDirty = function () {
                    return spFormBuilderService.unsavedChanges();
                };

                navItem.dirtyMessage = function () {
                    var formDirty = spFormBuilderService.unsavedFormChanges();

                    if (formDirty) {
                        return navDirtyMessage.defaultMsg;
                    }

                    return undefined;
                };

                /////
                // Properties button has been clicked.
                /////
                scope.onPropertiesClick = function () {
                    spTypeProperties.showDialog({ definition: spFormBuilderService.definition, form: spFormBuilderService.form, mode: spFormBuilderService.getBuilder() });
                };

                /////
                // Show actions button has been clicked.
                /////
                scope.model.actionButtons = [];

                /////
                // Show actions button or not.
                /////
                scope.showActionButton = function () {
                    return scope.model.hasFormActions || scope.model.hasTypeActions;
                };

                function createActionButton(action, ignoreIsButton) {
                    if (action.isbutton || ignoreIsButton) {
                        scope.model.actionButtons.push({
                            name: action.name,
                            description: action.description,
                            nameshort: spActionsService.getShortName(action.name),
                            multiname: action.multiname,
                            emptyname: action.emptyname,
                            displayname: action.displayname || action.name,
                            displaynameshort: spActionsService.getShortName(action.displayname || action.name),
                            icon: action.icon,
                            order: action.order,
                            action: action,
                            disabled: !action.isenabled,
                            select: action.isselect,
                            multiselect: action.ismultiselect
                        });
                    }
                }
                scope.showActionsDialog = function () {
                    var formActionsOptions = {
                        formControlEntity: spFormBuilderService.form,
                        actionDisplayContext: 'all',
                        formId: spFormBuilderService.form.idP,
                        entityTypeId: spFormBuilderService.definition.idP
                    };
                    spFormActionsDialog.showModalDialog(formActionsOptions).then(function (result) {
                        if (result) {

                            // redress the actions buttons
                            scope.model.actionButtons.length = 0;

                            _.forEach(result.actionButtons, createActionButton);
                        }
                    });
                };

                function getFormActionsOnLoad() {
                    var formId = spFormBuilderService.form.idP;

                    if (!formId || !_.isNumber(formId)) {
                        return;
                    }

                    var typesActionsRequest = {
                        formId: formId,
                        entityTypeId: spFormBuilderService.definition.idP,
                        data: {},
                        display: 'all',
                        hostIds: [formId]
                    };

                    var formActionsP = spActionsService.getFormActionButtons(formId);
                    var typesActionsP = spActionsService.getFormActionsMenu(typesActionsRequest);

                    return $q.all([formActionsP, typesActionsP])
                        .then(function (response) {

                            var formActions = sp.result(response[0], 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');
                            var typesActions = sp.result(response[1], 'actions');

                            scope.model.hasFormActions = formActions && formActions.length > 0;
                            scope.model.hasTypeActions = typesActions && typesActions.length > 0;

                            if (scope.model.hasFormActions) {
                                buildFormActionButtons(formActions);
                            }
                        });
                }

                function buildFormActionButtons(actions) {
                    if (actions) {
                        var actionBtns = [];
                        _.forEach(actions, function (actionEntity) {
                            actionBtns.push({
                                name: actionEntity.name,
                                description: actionEntity.description,
                                nameshort: spActionsService.getShortName(actionEntity.name),
                                multiname: actionEntity.multiname,
                                emptyname: actionEntity.emptyname,
                                displayname: actionEntity.displayname || actionEntity.name,
                                displaynameshort: spActionsService.getShortName(actionEntity.displayname || actionEntity.name),
                                icon: actionEntity.menuIconUrl,
                                order: actionEntity.menuOrder,
                                action: {
                                    method: actionEntity.htmlActionMethod,
                                    state: actionEntity.htmlActionMethod
                                },
                                disabled: !actionEntity.isenabled,
                                select: actionEntity.isselect,
                                multiselect: actionEntity.ismultiselect
                            });
                        });

                        // redress the actions buttons
                        scope.model.actionButtons.length = 0;

                        scope.model.actionButtons = _.sortBy(actionBtns, ['order', 'displayname']);
                    }
                }


                function initialize() {
                    var state;

                    scope.model.actionButtons = [];

                    spFormBuilderService.reset();

                    if (navItem.dataObject != null) {
                        forceReload = !!navItem.dataObject.forceReload;

                        if (navItem.dataObject.state != null) {
                            state = navItem.dataObject.state;
                            delete navItem.dataObject;
                        }
                    }

                    if (!state) {
                        if (spState && spState.scope && spState.scope.hasOwnProperty('state')) {
                            state = spState.scope.state;
                            delete spState.scope.state;
                        }
                    }

                    spNavService.middleLayoutBusy = true;

                    spFormBuilderService.initialize(!state).then(function() {
                        if (state) {
                            // Restore state
                            spFormBuilderService.setState(state);
                        }
                    }).finally(function () {

                        if (forceReload) {
                            spFormBuilderService.serviceRevision++;
                        }

                        scope.isNewForm = spFormBuilderService.form.getDataState() === spEntity.DataStateEnum.Create;

                        spNavService.middleLayoutBusy = false;

                        spFormBuilderService.refreshMemoizedFunctions();

                        if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.form) {
                            scope.isNewDefinition = spFormBuilderService.definition.getDataState() === spEntity.DataStateEnum.Create;

                            // fetch form actions if not a new form and definition
                            if (!scope.isNewForm && !scope.isNewDefinition) {
                                getFormActionsOnLoad();
                            }
                        }

                        $timeout(function () {
                            setBookmark = true;
                            spMeasureArrangeService.performLayout('formBuilder');
                        });
                    });
                }

                /////
                // Save button has been clicked.
                /////
                scope.onSaveClick = function() {

                    if (scope.windowBusy.isBusy)
                        return;
                    scope.windowBusy.isBusy = true;

                    /////
                    // Save options
                    /////
                    var options = {
                        onSaveComplete: function () {

                            editFormCache.removeAll();

                            scope.windowBusy.isBusy = false;
                            if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen) {
                                //update screen navItem
                                navigationBuilderProvider.updateNavItem(spFormBuilderService.form);
                            }
                        }
                    };

                    /////
                    // Save the definition and form. Also reloads both.
                    /////
                    spFormBuilderService.saveFormAndDefinition(options).then(function (result) {
                        scope.windowBusy.isBusy = false;

                        $timeout(function() {
                            var message = spFormBuilderService.definition ? 'Object \'' + spFormBuilderService.definition.name + '\' and ' : '';
                            message += spFormBuilderService.getBuilder() === spFormBuilderService.builders.form ? 'Form \'' : 'Screen \'';
                            message += spFormBuilderService.form.name + '\' successfully saved.';
                            spAlertsService.addAlert(message, { severity: spAlertsService.sev.Success, expires: true });

                            if (result.formIsNew) {
                                $stateParams.eid = result.formId;
                                $state.transitionTo($state.current, $stateParams, { reload: false, inherit: true, notify: true, location: 'replace' });
                                spFormBuilderService.getTypeRequest(result.definitionId).then(function(type) {
                                    spFormBuilderService.definition = type;
                                }).then(function () {
                                    spFormBuilderService.setInitialFormBookmark();
                                });
                            }

                            setBookmark = true;
                            spMeasureArrangeService.performLayout('formBuilder');
                        });


                    }, function (error) {
                        var errorText;

                        if (error && error.data) {
                            errorText = error.data.ExceptionMessage || error.data.Message;
                        } else {
                            errorText = error;
                        }

                        spAlertsService.addAlert(errorText, { severity: spAlertsService.sev.Error, expires: true });

                        if (error && error.status === 403) {
                            // Restore changes if failed to save.
                            initialize();
                        }

                        console.log(error);
                        throw error;
                    }).finally(function () {
                        if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen) {
                            spNavService.requireRefreshTree();
                            spNavService.refreshTreeBranch(navItem);
                        }
                        spFormBuilderService.refreshMemoizedFunctions();

                        scope.windowBusy.isBusy = false;

                        spFormBuilderService.setInitialFormBookmark();
                    });
                };

                /////
                // SaveAs button has been clicked.
                /////
                scope.onSaveAsClick = function() {
                    if (spFormBuilderService.form) {

                        var builder = spFormBuilderService.getBuilder();

                        var saveAsOptions = {
                            formEntity: spFormBuilderService.form,
                            mode: builder
                        };

                        var isScreenBuilder = builder === spFormBuilderService.builders.screen;

                        if (isScreenBuilder) {
                            var container = spNavService.getCurrentItemContainer();
                            if (container) {
                                saveAsOptions.containerId = container.id;   
                            }   
                        }                        

                        spFormSaveAsDialog.showModalDialog(saveAsOptions).then(function (result) {

                            editFormCache.removeAll();

                            if (!result) {
                                return;
                            }

                            var newFormId = result.formId;
                            var containerId = isScreenBuilder ? result.containerId : null;
                            
                            if (isScreenBuilder) {
                                spNavService.requireRefreshTree();
                                spNavService.refreshTreeBranch(containerId ? { id: containerId } : navItem).then(function() {
                                    if (!containerId) {
                                        return;
                                    }

                                    // Performing a save as to a new section
                                    var newScreenNode = spNavService.findInTreeById(spNavService.getNavTree(), newFormId);
                                    if (newScreenNode && sp.result(newScreenNode, 'item.state.params')) {
                                        spNavService.navigateToState(builder, newScreenNode.item.state.params).then(function() {
                                            spState.getPageState().isSavingScreenIntoNewContainer = true;
                                        });
                                    } else {
                                        spNavService.navigateToSibling(builder, newFormId, null, {
                                            forceReload: true
                                        });
                                    }
                                });
                            }

                            spFormBuilderService.setInitialFormBookmark();

                            if (!containerId) {
                                spNavService.navigateToSibling(builder, newFormId, null, {
                                    forceReload: true
                                });
                            }                            
                        });

                    }
                };

                /////
                // Cancel button has been clicked.
                /////
                scope.onCancelClick = function () {
                    //clean the parent navitem's params info, and replace by currentitem's params
                    spNavService.getParentItem().state.params = null;

                    var returnToReport = spFormBuilderService.returnToReport;
                    if (returnToReport) {
                        spFormBuilderService.returnToReport = null;
                        spNavService.navigateToSibling('report', returnToReport);
                    } else {
                        var pageState = spState.getPageState();
                        var navParams;

                        if (pageState.isSavingScreenIntoNewContainer) {
                            // Screen builder was loaded as a result of creating a new chart via a save as into a new section
                            delete pageState.isSavingScreenIntoNewContainer;
                            navParams = sp.result(spState, 'navItem.state.params');                            
                        }

                        if (navParams) {
                            spNavService.navigateToState('screen', navParams).then(function() {
                                spState.getPageState().syncNavTreeWithItem = true;
                            });
                        } else {
                            spNavService.navigateToParent();   
                        }
                    }
                };

                scope.windowBusy = {
                    type: 'spinner',
                    text: 'Saving...',
                    placement: 'element',
                    isBusy: false
                };

                if (navItem &&
                    navItem.state) {
                    switch (navItem.state.name) {
                        case 'screenBuilder':
                            titleService.setTitle('Screen Builder');
                            break;
                        case 'formBuilder':
                            titleService.setTitle('Form Builder');
                            break;
                    }
                }
            }
        };
    }
}());