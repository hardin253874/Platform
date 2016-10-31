// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('app.editForm.spInlineRelationshipPickerController', [
        'mod.app.editForm', 'mod.common.ui.spDialogService', 'mod.common.ui.spPopupStackManager',
        'mod.common.ui.spFocus', 'mod.common.spMobile', 'mod.app.editFormCache', 'mod.common.alerts']);

    angular.module('app.editForm.spInlineRelationshipPickerController')
        .controller('spInlineRelationshipPickerController', InlineRelationshipPickerController);

    /* @ngInject */
    function InlineRelationshipPickerController($scope, $element, $q, $uibModalStack, spEditForm, spNavService, spEntityService,
                                                $state, spDialogService, spPopupStackManager, focus,
                                                spMobileContext, editFormCache, spAlertsService) {

        if (!$scope.options) {
            $scope.options = {};
        }

        // init
        spEditForm.getTemplateReport(function (report) {
            $scope.templateReport = report;
        });

        var getDerivedCtxMenuFn = function () {
            return $q.when();
        };

        $scope.multiSelect = false;
        $scope.allowCreateRecords = true;
        $scope.isDisabled = false;
        // picker Report
        $scope.reportOptions = {
            reportId: 0,
            multiSelect: false,
            isEditMode: false,
            selectedItems: null,
            entityTypeId: $scope.entityType,
            newButtonInfo: {},
            isInPicker: true,
            isMobile: spMobileContext.isMobile
        };

        $scope.controlId = _.uniqueId('EntityName');

        //$scope.$watch("templateReport", function () {
        //    if ($scope.reportOptions.reportId || $scope.reportOptions.reportId===0)
        //      $scope.reportOptions.reportId = $scope.templateReport.id();
        //});
        
        $scope.$watch("options.multiSelect", function () {
            if ($scope.options.multiSelect && $scope.reportOptions) {
                $scope.reportOptions.multiSelect = $scope.options.multiSelect;
            }
        });

        $scope.$watch("options.pickerReportId", function () {
            if ($scope.options.pickerReportId && $scope.reportOptions) {
                $scope.reportOptions.reportId = $scope.options.pickerReportId;
            }
        });

        $scope.$watch("options.disallowCreateRelatedEntityInNewMode", function () {
            if ($scope.options.disallowCreateRelatedEntityInNewMode && $scope.reportOptions) {
                $scope.reportOptions.disallowCreateRelatedEntityInNewMode = $scope.options.disallowCreateRelatedEntityInNewMode;
            }
        });

        $scope.$watch("options.entityTypeId", function () {
            // Try and get the picker report from the form control
            if (sp.result($scope, 'options.formControl') && !sp.result($scope, 'options.pickerReportId')) {
                setPickerReport($scope.options.formControl);

                if (sp.result($scope, 'reportOptions.reportId')) {
                    // Found one, return
                    return;
                }
            }

            //bug 26730 If the sp-inline-relationship-picker is provided with a typeId, but no reportId, then it should load the defaultPicker report for that type.
            if (!$scope.options.pickerReportId && $scope.options.entityTypeId && $scope.reportOptions) {
                spEntityService.getEntity($scope.options.entityTypeId, 'defaultPickerReport.id', {
                    batch: 'true',
                    hint: 'defaultPickerReport'
                }).then(function (type) {
                    var defPickerReportId = sp.result(type, 'defaultPickerReport.id');
                    if (defPickerReportId) {
                        $scope.reportOptions.reportId = defPickerReportId;
                    } else {
                        //otherwise get the template report
                        spEntityService.getEntity('core:templateReport', 'name', {
                            hint: 'templateReportId',
                            batch: true
                        }).then(function (report) {
                            if (report)
                                $scope.reportOptions.reportId = report.id();
                        });
                    }
                });

            }
        });

        $scope.$watch("options.allowCreateRecords", function () {

            if ($scope.options.allowCreateRecords) {
                $scope.allowCreateRecords = $scope.options.allowCreateRecords;
            }
        });

        $scope.$watch("options.reportOptions", function () {

            if ($scope.options.reportOptions && $scope.reportOptions) {
                _.assign($scope.reportOptions, $scope.options.reportOptions);
            }
        });

        // Watch for filter changes and update the report picker options
        $scope.$watch('options.relationshipFilters', function () {
            if ($scope.options.relationshipFilters && $scope.reportOptions) {
                $scope.reportOptions.relationshipFilters = $scope.options.relationshipFilters;
            }
        });

        $scope.$watch('options.relationDetail', function () {
            if ($scope.options.relationDetail && $scope.reportOptions) {
                $scope.reportOptions.relationDetail = $scope.options.relationDetail;
            }
        });

        $scope.$watch('options.filteredEntityIds', function () {
            if ($scope.options.filteredEntityIds && $scope.reportOptions) {
                $scope.reportOptions.filteredEntityIds = $scope.options.filteredEntityIds;
            }
        });

        $scope.$watch("options.formControl", function () {

            if ($scope.options.formControl) {

                if ($scope.reportOptions && $scope.reportOptions.reportId !== 0) {
                    return; // picker report id already provided
                }

                // set the picker report
                setPickerReport($scope.options.formControl);
            }
        });

        $scope.$watch("options.displayString", function () {
            if (!sp.isNullOrUndefined($scope.options.displayString)) {
                $scope.displayString = $scope.options.displayString;
            }
        });

        // Watch the selected entities for changes and set the display string
        $scope.$watch('options.selectedEntities', function () {
            checkEntityNames();
            if (!$scope.options.displayString) {    // in relationship properties dialog, checking 'useCurrentUser' for default value sets the 'displayString' and sets 'selectedEntities' to null. watch on 'selectedEntities' recalcultes the 'displayString' and overwrites the displayString passed in through the options
                $scope.displayString = spEditForm.getEntitiesDisplayName($scope.options.selectedEntities);
                if ($scope.displayString.length > 0) {
                    focus($scope.controlId);
                }
            }

            if ($scope.options.selectedEntities) {
                $scope.reportOptions.selectedItems = _.map($scope.options.selectedEntities, function (se) {
                    return {
                        eid: se.id()
                    };
                });
            } else {
                $scope.reportOptions.selectedItems = null;
            }
        });

        $scope.$watch('options.isDisabled', function () {
            setIsDisabled();
        });

        $scope.$watch('options.modifyAccessDenied', function (newVal, oldVal) {
            if (newVal === oldVal)
                return;
            setIsDisabled();
        });

        // Watch the entityTypeId for changes and build the derived types menu for the entiy type
        $scope.$watch('options.entityTypeId', function () {
            if ($scope.options.entityTypeId) {

                // update report entityId
                if ($scope.reportOptions) {
                    $scope.reportOptions.entityTypeId = $scope.options.entityTypeId;
                }

                // create from modal is not supported yet
                var modalsExist = !!$uibModalStack.getTop();

                if ($scope.allowCreateRecords && $scope.options.formControl && !modalsExist) {

                    var canCreate = $scope.options.formControl.getField('canCreate');
                    if (sp.isNullOrUndefined(canCreate)) {
                        canCreate = true;   // if the flag is not set then by default allow creating type
                    }

                    var canCreateDerivedTypes = $scope.options.formControl.getField('canCreateDerivedTypes');
                    if (sp.isNullOrUndefined(canCreateDerivedTypes)) {
                        canCreateDerivedTypes = true;   // if the flag is not set then by default allow creating type and its derived types
                    }

                    if (canCreate && !$scope.isDisabled) {

                        //******** Note ********//
                        var createHandler = 'handleCreate'; //*** this is the name of function on the scope that will be called when a menu item is clicked in the context menu ***//
                        //*********************//

                        getDerivedCtxMenuFn = _.once(function () {
                            return spEditForm.getDerivedTypesContextMenu($scope, $scope.options.entityTypeId, canCreateDerivedTypes, createHandler)
                                .then(
                                    updateNewButtonsInfo,
                                    function (error) {
                                        console.error('error building context menu: ' + error);
                                    });
                        });
                    }
                }
                else {
                    updateNewButtonsInfo();
                }
            }
        });


        $scope.$on('spDataGridEventGridDoubleClicked', function (event, selectedItems) {
            event.stopPropagation();

            $scope.selectedReportItems = selectedItems;
            $scope.handleOk();
        });

        $scope.handleOk = function () {
            if (!$scope.options || !$scope.selectedReportItems) {
                return;
            }

            var selectedEntities;
            var selectedReportItem;
            var hasChanges = false;
            var i;
            var max;
            var foundEntity;
            var selectedEntitiesResult = [];
            var selectedNewItemsIds = [];

            function findEntity(entity) {
                return entity.id() === selectedReportItem.eid;
            }

            if ($scope.options.selectedEntities) {
                // copy the selected entities
                selectedEntities = $scope.options.selectedEntities.slice();
            }

            if (selectedEntities && $scope.selectedReportItems && selectedEntities.length !== $scope.selectedReportItems.length) {
                hasChanges = true;
            }

            for (i = 0, max = $scope.selectedReportItems.length; i < max; i = i + 1) {
                selectedReportItem = $scope.selectedReportItems[i];

                // See if this entity is already in the list of selected entities
                foundEntity = _.find(selectedEntities, findEntity);

                if (foundEntity) {
                    // Found existing entity add it to result as is
                    selectedEntitiesResult.push(foundEntity);
                } else {
                    // Not found, add it's id
                    hasChanges = true;
                    selectedNewItemsIds.push(selectedReportItem.eid);
                }
            }

            if (hasChanges) {
                if (selectedNewItemsIds.length > 0) {
                    spEntityService.getEntities(selectedNewItemsIds, 'name').then(function (result) {

                        if (!result) {
                            return;
                        }

                        _.forEach(result, function (entity) {
                            selectedEntitiesResult.push(entity);
                        });

                        $scope.options.selectedEntities = selectedEntitiesResult;
                    });
                } else {
                    $scope.options.selectedEntities = selectedEntitiesResult;
                }
            }
        };

        $scope.handleCreate = function (entityId) {

            if ($scope.options.disallowCreateRelatedEntityInNewMode) {
                var msg = spEditForm.getDisallowCreateRelatedEntityInNewModeMessage($scope.options.formControl.relationshipToRender, $scope.options.formControl.isReversed);
                spAlertsService.addAlert(msg, { severity: 'error', page: $state.current });
                return;
            }

            // close the modal if it is open
            //$scope.modalInfo.isOpen = false;
            var params = {};

            var formId = getCreateFormId(entityId);
            if (formId && _.isNumber(formId)) {
                params.formId = formId;
            }

            // add params to autofill relationship value
            params.returnToParent = true;
            var navItem = spNavService.getCurrentItem();
            if (!sp.isNullOrUndefined($scope.options.formData)) {
                var entity = ($scope.options.formData.dataState === spEntity.DataStateEnum.Create) ? undefined : $scope.options.formData;  // if creating new entity then we don't want to pass temporary Id but still pass the relationship and direction as this is used for on return from creting child entity
                spEditForm.addAutoFillRelationshipParams(navItem, entity, $scope.options.formControl);
            }

            $scope.$emit("addOnReturnFromChildCreate", function (fscope, formData) {
                //SET the relationships to point to the child entity.
                spEditForm.setLookupOnReturnFromChildCreate(spNavService.getCurrentItem(), $scope.options.formControl.getRelationshipToRender(), $scope.options.formControl.getIsReversed(), formData);
            });
            spNavService.navigateToChildState('createForm', entityId, params);
            // todo: navigate to create new based on popup or not
        };

        //-- new button (when no context menu items are available)
        $scope.singleOptionClick = function () {
            $scope.handleCreate($scope.options.entityTypeId);
        };

        function setIsDisabled() {
            $scope.isDisabled = !!$scope.options.isDisabled || !!$scope.options.disabled || !!$scope.options.modifyAccessDenied;
        }

        // clears the local selected entity
        $scope.clear = function () {
            if ($scope.options.selectedEntity) {
                $scope.options.selectedEntity = null;
            }

            if ($scope.options.selectedEntities) {
                $scope.options.selectedEntities = [];
            }
        };

        // validate relationship
        $scope.validateRelationship = function () {
            $scope.$emit('validateRelationship');
        };

        // helpers

        function updateNewButtonsInfo() {
            // update new buttons click handlers, tooltip and visibility
            var newBtnInfo = {};

            if ($scope.contextMenu && $scope.contextMenu.menuItems) {

                newBtnInfo.contextMenu = $scope.contextMenu;

                // add the create handlers
                newBtnInfo.singleOptionClick = $scope.singleOptionClick;
                newBtnInfo.handleCreate = $scope.handleCreate;

                if ($scope.contextMenu.menuItems.length === 1) {
                    newBtnInfo.newBtnTooltip = 'New ' + $scope.contextMenu.menuItems[0].text;
                    newBtnInfo.singleOptionAvailable = true;
                    newBtnInfo.multiOptionsAvailable = false;

                }
                else if ($scope.contextMenu.menuItems.length > 1) {
                    newBtnInfo.newBtnTooltip = 'New';
                    newBtnInfo.singleOptionAvailable = false;
                    newBtnInfo.multiOptionsAvailable = true;
                }

                $scope.reportOptions.newButtonInfo = newBtnInfo;
            }
        }

        function setPickerReport(formControl) {
            // set picker report
            var formId;
            var pickerRpt = spEditForm.getRelControlPickerReport(formControl);
            if (pickerRpt) {
                $scope.reportOptions.reportId = pickerRpt.id();
                formId = sp.result(spNavService.getCurrentItem(), 'data.formControl.idP');

                if (formId &&
                    $scope.reportOptions.reportId) {
                    editFormCache.assignInvalidatingEntityToForm($scope.reportOptions.reportId, formId);
                }
            }
            else if ($scope.templateReport) {
                $scope.reportOptions.reportId = $scope.templateReport.id();
            }
        }

        function checkEntityNames() {
            var entities = $scope.options.selectedEntities;
            if (_.some(entities, function (e) {
                    return !e.name;
                })) {
                var ids = _.map(entities, 'idP');
                spEntityService.getEntities(ids, 'name').then(function (results) {
                    var names = _.keyBy(results, 'idP');
                    _.forEach(entities, function (e) {
                        var name = sp.result(names[e.idP], 'name');
                        if (name) {
                            e.setName(name);
                        }
                    });
                    $scope.displayString = spEditForm.getEntitiesDisplayName(entities);
                });
            }
        }

        function getCreateFormId(entityId) {
            var createFormId;

            // check if a form is selected
            var selectedForm = $scope.options.formControl.resourceViewerConsoleForm;
            if (selectedForm) {
                var relationship = $scope.options.formControl.relationshipToRender;
                var isReversed = $scope.options.formControl.isReversed;
                if (relationship) {
                    var entityType = isReversed ? relationship.getFromType() : relationship.getToType();

                    // only do this if creating instance of the 'type' that this end of relationship is pointing to ( and not if creating instance of a derived type).
                    // e.g only do this if this end of relationship point to 'Employee' and we are creating instance of 'Employee'. (and not if creating instance of a derived type of 'Employee')
                    if (entityType && entityType.idP === entityId) {
                        var forms = entityType.formsToEditType;

                        var foundForm = _.find(forms, function (form) {
                            return form.idP === selectedForm.idP;
                        });   // check if selected form belongs to the type being created

                        if (foundForm) {
                            createFormId = selectedForm.idP;
                        }
                    }
                }
            }
            return createFormId;
        }

        $scope.cancelDialog = function () {

        };

        //**** picker report Modal ******************

// ReSharper disable DuplicatingLocalDeclaration
        // '$' must be used to inject the scope here.
        var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerScopeReportOptions', function ($scope, $uibModalInstance, outerScopeReportOptions) {
            // ReSharper restore DuplicatingLocalDeclaration
            $scope.isModalOpened = true;
            $scope.model = {};
            $scope.model.reportOptions = outerScopeReportOptions;
            $scope.ok = function () {
                $scope.isModalOpened = false;
                $uibModalInstance.close($scope.model.reportOptions);

            };

            $scope.$on('spReportEventGridDoubleClicked', function (event, selectedItems) {
                event.stopPropagation();

                $scope.ok();
            });

            $scope.$on('spEntityCompositePickerEventNodeDoubleClicked', function (event, selectedItems) {
                event.stopPropagation();

                $scope.selectedReportItems = selectedItems;
                $scope.ok();
            });


            $scope.$on('$locationChangeSuccess', function (event, newUrl, oldUrl) {
                if (newUrl !== oldUrl &&
                    $uibModalInstance &&
                    $scope.isModalOpened) {
                    try {
                        $scope.isModalOpened = false;
                        $uibModalInstance.dismiss('cancel');
                    }
                    catch (e) {
                    }
                }
            });

            $scope.cancel = function () {
                $scope.isModalOpened = false;
                $uibModalInstance.dismiss('cancel');
            };

            $scope.model.reportOptions.cancelDialog = $scope.cancel;    // this is here to provide a hadle to close dialog from the 'spEntityReportPicker' when creating a new related entity.
        }];


        $scope.openDetail = function (templateUrl) {
            var defaults = {
                templateUrl: templateUrl,
                controller: modalInstanceCtrl,
                windowClass: 'modal inlineRelationPickerDialog',
                resolve: {
                    outerScopeReportOptions: function () {
                        return $scope.reportOptions;
                    },
                    outerScopeCancelDialog: function () {
                        return $scope.cancelDialog;
                    }
                }
            };

            var options = {};

            spPopupStackManager.pushPopup($element);


            getDerivedCtxMenuFn().then(function () {
                spDialogService.showDialog(defaults, options).then(function (result) {
                    spPopupStackManager.popPopup($element);
                    if (result) {
                        $scope.selectedReportItems = result.selectedItems;
                    }
                    $scope.handleOk();
                }, function () {
                    spPopupStackManager.popPopup($element);
                });
            });

        };
    }
}());