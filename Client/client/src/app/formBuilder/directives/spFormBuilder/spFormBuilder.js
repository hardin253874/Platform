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
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMeasureArrange',
        'mod.app.navigationProviders',
        'mod.app.editFormCache',
        'mod.app.configureDialogs.spFormActionsDialog',
        'mod.common.ui.spActionsService',
        'mod.common.ui.spEditFormDialog',
        'mod.featureSwitch'
    ])
        .directive('spFormBuilder', spFormBuilder);

    function spFormBuilder($state, $stateParams, spFormBuilderService, spEntityService, spNavService, $timeout, $q,
                           spTypeProperties, spAlertsService, spState, spFormSaveAsDialog, spDialogService, titleService, navDirtyMessage,
                           spMeasureArrangeService, spNavigationBuilderProvider, editFormCache, spFormActionsDialog,
                           spActionsService, spEditForm, spEditFormDialog, rnFeatureSwitch) {

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

                scope.$on('tabRemoved',
                    function (event) {
                        spFormBuilderService.refreshMemoizedFunctions();

                        if (!event.defaultPrevented) {
                            event.defaultPrevented = true;
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

                var workflowTypeId = 0;

                /////
                // Show actions button has been clicked.
                /////
                scope.model.actionButtons = [];
                scope.model.showActionContextMenu = false;
                scope.model.actionContextMenu = {
                    menuItems: [{
                        text: 'Create Record',
                        icon: 'assets/images/16x16/add.svg',
                        type: 'click',
                        click: "showCreateActionDialog()"
                    }, {
                        text: 'Run Workflow',
                        icon: 'assets/images/16x16/run.svg',
                        type: 'click',
                        click: "showWorkflowActionDialog()"
                    }]
                };

                /////
                // Show actions button or not.
                /////
                scope.showActionButton = function () {
                    return spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen ?
                        rnFeatureSwitch.isFeatureOn('screenActions') :
                        scope.model.hasFormActions || scope.model.hasTypeActions;
                };

                /////
                // Present the actions buttons for either forms or screens
                /////
                scope.getActionsClass = function () {
                    return spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen ?
                        'sp-Screen-Builder-Action' :
                        'sp-Form-Builder-Action';
                };

                scope.getAddActionsImage = function () {
                    return spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen ?
                        'assets/images/16x16/add_w.svg' :
                        'assets/images/16x16/add.svg';
                };

                /////
                // Initiate action button configuration for the appropriate context
                /////
                scope.configureActions = function (ab) {
                    if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen) {
                        if (ab) {
                            var included = sp.result(spFormBuilderService.form, 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');
                            var actionId = sp.result(ab, 'action.idP');
                            var actionToEdit = _.find(included, ['idP', actionId]) || ab.action;
                            var actionEntityType = sp.result(actionToEdit, 'isOfType[0].nsAlias') || sp.result(ab, 'action.type.nsAlias');
                            switch (actionEntityType) {
                                case 'console:workflowActionMenuItem':
                                    scope.showWorkflowActionDialog(actionToEdit).then(function(name) {
                                        ab.name = name;
                                        ab.nameshort = spActionsService.getShortName(name);
                                    });
                                    break;
                                case 'console:navigateToCreateFormActionMenuItem':
                                    scope.showCreateActionDialog(actionToEdit).then(function (name) {
                                        ab.name = name;
                                        ab.nameshort = spActionsService.getShortName(name);
                                    });
                                    break;
                            }
                        } else {
                            scope.model.showActionContextMenu = true;
                        }
                    } else {
                        showActionsDialog();
                    }
                };

                scope.showCreateActionDialog = function (actionEntity) {
                    var isNew = false;
                    if (!actionEntity) {
                        actionEntity = spEntity.fromJSON({
                            typeId: 'console:navigateToCreateFormActionMenuItem',
                            'core:name': jsonString(),
                            'core:description': jsonString(),
                            'core:htmlActionMethod': jsonString('navigate'),
                            'core:htmlActionState': jsonString('createForm'),
                            'console:menuIconUrl': jsonString('assets/images/16x16/add_w.svg'),
                            'console:menuOrder': jsonInt(),
                            'console:navigateToCreateFormActionDefinition': jsonLookup(),
                            'console:navigateToCreateFormActionForm': jsonLookup()
                        });
                        isNew = true;
                    }

                    var actionEntityClone = spEntity.fromJSON({});
                    spEntity.augment(actionEntityClone, actionEntity);
                    actionEntityClone._id._id = actionEntity.idP;
                    actionEntityClone.dataState = actionEntity.dataState;

                    var options = {
                        title: 'New Record Action',
                        entity: actionEntityClone,
                        form: 'core:createRecordActionItemDialog',
                        formMode: spEditForm.formModes.edit,
                        optionsEnabled: false,
                        saveEntity: false
                    };

                    return optionallyApplyChanges(actionEntity, actionEntityClone, false, _.partial(spEditFormDialog.showDialog, options)).then(function(result) {
                        if (result === true && isNew) {
                            var order = (_.max(_.map(scope.model.actionButtons, 'order')) || 0) + 1;

                            actionEntity.menuOrder = order;

                            addAction(actionEntity);
                        }
                        return actionEntity.name;
                    });
                };

                scope.showWorkflowActionDialog = function(actionEntity) {
                    var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerOptions', function ($scope, $uibModalInstance, outerOptions) {
                        $scope.model = {
                            reportOptions: outerOptions
                        };
                        $scope.ok = function () {
                            $scope.isModalOpened = false;
                            $uibModalInstance.close($scope.model.reportOptions);
                        };
                        $scope.$on('spReportEventGridDoubleClicked', function (event) {
                            event.stopPropagation();
                            $scope.ok();
                        });
                        $scope.cancel = function () {
                            $scope.isModalOpened = false;
                            $uibModalInstance.dismiss('cancel');
                        };
                        $scope.model.reportOptions.cancelDialog = $scope.cancel;
                        $scope.isModalOpened = true;
                    }];

                    var workflowReportOptions = {
                        reportId: 'core:templateReport',
                        entityTypeId: workflowTypeId,
                        multiSelect: false,
                        isEditMode: false,
                        newButtonInfo: {},
                        isInPicker: true,
                        isMobile: scope.isMobile,
                        fastRun: true
                    };

                    var defaults = {
                        templateUrl: 'entityPickers/entityCompositePicker/spEntityCompositePickerModal.tpl.html',
                        controller: modalInstanceCtrl,
                        windowClass: 'modal inlineRelationPickerDialog',
                        resolve: {
                            outerOptions: function () {
                                return workflowReportOptions;
                            }
                        }
                    };

                    var options = {};

                    return spDialogService.showDialog(defaults, options).then(function (result) {
                        if (workflowReportOptions.selectedItems) {
                            var selected = _.first(workflowReportOptions.selectedItems);
                            if (selected && selected.eid) {
                                var name = _.get(selected, 'cells[0].value');
                                if (actionEntity) {
                                    // update the workflow of the existing action
                                    actionEntity.name = name;
                                    actionEntity.actionMenuItemToWorkflow = spEntity.fromId(selected.eid);
                                } else {
                                    var order = (_.max(_.map(scope.model.actionButtons, 'order')) || 0) + 1;

                                    actionEntity = spEntity.fromJSON({
                                        typeId: 'console:workflowActionMenuItem',
                                        'core:name': jsonString(name),
                                        'core:description': jsonString(),
                                        'core:htmlActionMethod': jsonString('run'),
                                        'console:menuIconUrl': jsonString('assets/images/16x16/run_w.svg'),
                                        'console:menuOrder': jsonInt(order),
                                        'console:actionMenuItemToWorkflow': jsonLookup(selected.eid)
                                    });

                                    addAction(actionEntity);
                                }
                                return name;
                            }
                        }
                    });
                };

                scope.removeActionButton = function (ab) {
                    if (!ab) return;
                    var included = sp.result(spFormBuilderService.form, 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');
                    if (ab.action) {
                        included.deleteEntity(ab.action);
                    }
                    _.remove(scope.model.actionButtons, ab);
                };
                
                function showActionsDialog () {
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
                }

                function createActionButton (action, ignoreIsButton) {
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

                function optionallyApplyChanges (entity, entityClone, readonly, callback) {
                    var bm = entityClone.graph.history.addBookmark();
                    var res = callback();
                    var handleResult = function (result) {
                        if (result === false) {
                            bm.undo();
                        } else {
                            bm.endBookmark();
                            if (readonly !== true) {
                                // apply the changes made to the clone back to the model
                                entity.navigateToCreateFormActionDefinition = entityClone.navigateToCreateFormActionDefinition;
                                entity.navigateToCreateFormActionForm = entityClone.navigateToCreateFormActionForm;

                                var name = sp.result(entity, 'navigateToCreateFormActionForm.name');
                                if (entity.navigateToCreateFormActionDefinition) {
                                    name = 'New \'' + sp.result(entity, 'navigateToCreateFormActionDefinition.name') + '\'';
                                }
                                entity.name = name;
                            }
                        }
                        return result;
                    };
                    if (res && res.then) {
                        // handle promises                    
                        return $q.when(res).then(handleResult);
                    } else {
                        handleResult(res);
                        return null;
                    }
                }

                function buildFormActionButton (actionEntity) {
                    return {
                        action: actionEntity,
                        name: actionEntity.name,
                        nameshort: spActionsService.getShortName(actionEntity.name),
                        description: actionEntity.description,
                        icon: actionEntity.menuIconUrl,
                        order: actionEntity.menuOrder
                    };
                }

                function buildFormActionButtons (actions) {
                    if (actions) {
                        var actionBtns = [];
                        _.forEach(actions, function (actionEntity) {
                            actionBtns.push(buildFormActionButton(actionEntity));
                        });

                        // redress the actions buttons
                        scope.model.actionButtons.length = 0;

                        scope.model.actionButtons = _.sortBy(actionBtns, ['order', 'displayname']);
                    }
                }

                function getFormActionsOnLoad () {
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

                function getScreenActionsOnLoad () {
                    scope.model.actionButtons.length = 0;

                    var formId = spFormBuilderService.form.idP;

                    if (!formId || !_.isNumber(formId)) {
                        return $q.when();
                    }

                    spEntityService.getEntity('core:workflow', 'name').then(function(wf) {
                        workflowTypeId = sp.result(wf, 'idP');
                    });

                    return spActionsService.getActionsOnResource(formId).then(function (screen) {
                        spEntity.augment(spFormBuilderService.form, screen);

                        var screenActions = sp.result(spFormBuilderService.form, 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');
                        if (screenActions && screenActions.length) {
                            buildFormActionButtons(screenActions);
                        }
                    }, function (error) {
                        console.error('spFormBuilder.getScreenActionsOnLoad error:', error);
                    });
                }

                function addAction(actionEntity) {
                    var included = sp.result(spFormBuilderService.form, 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');

                    if (!included) {
                        var menu = spEntity.fromJSON({
                            typeId: 'console:actionMenu',
                            'console:showNewActionsButton': jsonBool(),
                            'console:showExportActionsButton': jsonBool(),
                            'console:menuItems': jsonRelationship(),
                            'console:suppressedActions': jsonRelationship(),
                            'console:suppressedTypesForNewMenu': jsonRelationship(),
                            'console:includeActionsAsButtons': jsonRelationship(),
                            'console:includeTypesForNewButtons': jsonRelationship()
                        });

                        var behavior = spEntity.fromJSON({
                            typeId: 'console:consoleBehavior',
                            'console:suppressActionsForType': jsonBool(),
                            'console:behaviorActionMenu': jsonLookup(menu)
                        });

                        if (!spFormBuilderService.form.resourceConsoleBehavior) {
                            spFormBuilderService.form.resourceConsoleBehavior = behavior;
                        }

                        if (!spFormBuilderService.form.resourceConsoleBehavior.behaviorActionMenu) {
                            spFormBuilderService.form.resourceConsoleBehavior.behaviorActionMenu = menu;
                        }

                        // try again
                        included = sp.result(spFormBuilderService.form, 'resourceConsoleBehavior.behaviorActionMenu.includeActionsAsButtons');
                    }

                    included.add(actionEntity);

                    scope.model.actionButtons.push(buildFormActionButton(actionEntity));
                }

                function initialize() {
                    var state;

                    scope.model.actionButtons = [];

                    spFormBuilderService.reset();

                    if (navItem.dataObject) {
                        forceReload = !!navItem.dataObject.forceReload;

                        if (navItem.dataObject.state) {
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

                        // Actions
                        if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.form) {
                            scope.isNewDefinition = spFormBuilderService.definition.getDataState() === spEntity.DataStateEnum.Create;

                            // fetch form actions if not a new form and definition
                            if (!scope.isNewForm && !scope.isNewDefinition) {
                                getFormActionsOnLoad();
                            }
                        }

                        // Screen Actions
                        if (spFormBuilderService.getBuilder() === spFormBuilderService.builders.screen) {

                            // fetch screen actions, unless it's a new screen
                            if (!scope.isNewForm) {
                                getScreenActionsOnLoad();
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