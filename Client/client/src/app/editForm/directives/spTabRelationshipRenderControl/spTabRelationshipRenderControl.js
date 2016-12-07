// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp, spEntity */

(function () {
    'use strict';

    /////
    // The spTabRelationshipRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spTabRelationshipRenderControl', [
        'mod.app.editForm', 'mod.ui.spReportModelManager', 'sp.themeService', 'mod.common.spMobile',
        'mod.common.alerts', 'mod.common.spCachingCompile', 'mod.app.editFormCache'
    ]);

    angular.module('mod.app.editForm.designerDirectives.spTabRelationshipRenderControl')
        .directive('spTabRelationshipRenderControl', spTabRelationshipRenderControl);

    /* @ngInject */
    function spTabRelationshipRenderControl($q, $state, spEditForm, spNavService, spDialogService,
                                            spReportModelManager, spThemeService, spMobileContext, spAlertsService,
                                            spCachingCompile, $timeout, editFormCache) {
        return {
            restrict: 'E',
            scope: {
                formControl: '=?',
                parentControl: '=?',
                formData: '=?',
                formTheme: '=?',
                formMode: '=?',
                isInTestMode: '=?',
                isReadOnly: '=?',
                isInDesign: '=?',
                isEmbedded: '=?'
            },
            link: link
        };

        function link(scope, iElement) {
            var modalInstanceCtrl;

            scope.isMobile = spMobileContext.isMobile;

            /////
            // Convert the current edit form scope values into a generic model with
            // no ties to edit form.
            /////
            scope.model = {
                isReadOnly: scope.isReadOnly,
                isInDesign: scope.isInDesign,
                isInTestMode: scope.isInTestMode,
                isEmbedded: scope.isEmbedded
            };

            /////
            // Setup a title model for the title control to use.
            /////
            scope.titleModel = {
                hasName: false
            };

            // init
            scope.relatedEntities = [];
            spEditForm.commonRelFormControlInit(scope, { metadataOnly: true, areCreating: $state.current.name === 'createForm' });

            // display report
            scope.displayReportOptions = {
                reportId: 0,
                multiSelect: true,
                isEditMode: false,
                selectedItems: null,
                entityTypeId: scope.entityType,
                formControlEntity: scope.formControl,
                getActionExecutionContext: defaultGetActionExecutionContext,
                isInDesign: scope.isInDesign,
                disableActions: scope.isEmbedded,
                title: scope.titleModel.name,
                modifyAccessDenied: undefined,
                formDataEntity: undefined,
                isMobile: scope.isMobile,
                fastRun: true
            };

            // picker Report
            scope.pickerReportOptions = {
                reportId: 0,
                multiSelect: true,
                isEditMode: false,
                selectedItems: null,
                entityTypeId: scope.entityType,
                newButtonInfo: {},
                isInPicker: true,
                isMobile: scope.isMobile,
                fastRun: true
            };

            // handles the remove action
            scope.handleRemove = function () {
                return scope.remove();
            };

            // handles the add action
            scope.handleAdd = function () {
                scope.openDetail('entityPickers/entityCompositePicker/spEntityCompositePickerModal.tpl.html');
                return $q.when(false); // don't refresh displayReport just because we popped up a window..
            };

            scope.openDetail = function (templateUrl) {
                // reset selected items
                scope.pickerReportOptions.selectedItems = null;

                var defaults = {
                    templateUrl: templateUrl,
                    controller: modalInstanceCtrl,
                    windowClass: 'modal inlineRelationPickerDialog',
                    resolve: {
                        outerScopePickerReportOptions: function () {
                            return scope.pickerReportOptions;
                        }
                    },
                };

                var options = {};

                spDialogService.showDialog(defaults, options).then(function (result) {
                    // note: not using dialog 'result' as 'scope.pickerReportOptions' is passed in to dialog byRef. so when dialog is closed, 'scope.pickerReportOptions.selectedItems' are used to add new items
                    scope.handleOk();
                });

            };

            // Get the relationship data array (includes change tracking)
            scope.getRelationship = function () {
                var rel = null;
                if (scope.relationshipToRender) {
                    var relId = {id: scope.relationshipToRender.eid(), isReverse: scope.formControl.isReversed};
                    rel = scope.formData.getRelationship(relId);
                }
                return rel;
            };

            scope.getTitleStyle = function () {
                return spThemeService.getHeadingStyle();
            };

            scope.handleOk = function () {

                if (scope.pickerReportOptions.selectedItems) {
                    var relEntities = scope.getRelationship();
                    _.forEach(scope.pickerReportOptions.selectedItems, function (selectedItem) {
                        relEntities.add(selectedItem.eid);
                    });
                    if (($state.current.name === 'viewForm' && scope.model.isReadOnly) || $state.current.name === 'screen') {
                        //save the formData
                        return saveFormDataEntityAndRefreshDisplayReport(true);
                    } else {
                        return updateRelationshipInstance(true);
                    }
                }
                return $q.when();
            };

            scope.remove = function () {
                if (scope.displayReportOptions.selectedItems) {
                    var relEntities = scope.getRelationship();
                    var navItem = spNavService.getCurrentItem();
                    _.forEach(scope.displayReportOptions.selectedItems, function (selectedItem) {
                        relEntities.remove(selectedItem.eid);

                        // remove the entity from created entites array if the removed entity was newly created
                        spEditForm.removeCreatedChildEntity(navItem, scope.relationshipToRender.idP, selectedItem.eid);
                    });
                    if (($state.current.name === 'viewForm' && scope.model.isReadOnly) || $state.current.name === 'screen') {
                        return saveFormDataEntityAndRefreshDisplayReport(false);
                    } else {
                        return updateRelationshipInstance();
                    }
                }
                return $q.when();
            };

            // new - new button (when no context menu items are available)
            scope.singleOptionClick = function () {
                // if dictionary is available and has exactly one key then use that key(i.e. entityTypeId)
                if (scope.entityDefaultFormDict && _.keys(scope.entityDefaultFormDict).length === 1) {
                    var keys = _.keys(scope.entityDefaultFormDict);
                    scope.handleCreate(keys[0]);
                }
                else {
                    scope.handleCreate(scope.entityTypeId);
                }
            };

            scope.handleCreate = function (entityId) {
                var params = {};

                if (scope.entityDefaultFormDict) {
                    var formId = scope.entityDefaultFormDict[entityId];

                    if (!_.isUndefined(formId)) {
                        //var formId = getFormId();
                        if (formId && _.isNumber(formId)) {
                            params.formId = formId;
                        }
                    }
                }

                // add params to autofill relationship value.
                var areCreating = $state.current.name === 'createForm';
                var navItem = spNavService.getCurrentItem();
                if (!sp.isNullOrUndefined(scope.formData)) {
                    var entity = areCreating ? undefined : scope.formData;  // if creating new entity then we don't want to pass temporary Id but still pass the relationship and direction as this is used for on return from creting child entity
                    spEditForm.addAutoFillRelationshipParams(navItem, entity, scope.formControl);
                }


                scope.$emit("addOnReturnFromChildCreate", function (fscope, formData) {
                    //SET the relationships to point to the child entity.
                    spEditForm.setRelationshipOnReturnFromChildCreate(spNavService.getCurrentItem(), scope.relationshipToRender, scope.formControl.isReversed, scope.formData);
                });
                spNavService.navigateToChildState('createForm', entityId, params);
            };

            var pickerRpt = spEditForm.getRelControlPickerReport(scope.formControl);
            if (pickerRpt) {
                scope.pickerReportOptions.reportId = pickerRpt.id();
                var formId = sp.result(spNavService.getCurrentItem(), 'data.formControl.idP');

                if (formId &&
                    scope.pickerReportOptions.reportId) {
                    editFormCache.assignInvalidatingEntityToForm(scope.pickerReportOptions.reportId, formId);
                }
            }
            else {
                if (!scope.templateReport) {
                    spEditForm.getTemplateReportP().then(function (tr) {
                        if (tr) {
                            scope.templateReport = tr;
                            scope.pickerReportOptions.reportId = tr.idP;
                        }
                    });
                } else {
                    scope.pickerReportOptions.reportId = scope.templateReport.idP;
                }
            }

            // Load actions, even in designer, for configuration
            if (scope.isInDesign) {
                scope.displayReportOptions.relationDetail = {
                    eid: -1,
                    relid: scope.relationshipToRender ? scope.relationshipToRender.idP : -1,
                    direction: scope.isReversed ? 'fwd' : 'rev'
                };

                setDisplayReport();
            }

            //**** picker report Modal ******************
            modalInstanceCtrl = [
                '$scope', '$uibModalInstance', 'outerScopePickerReportOptions',
                function ($scope, $uibModalInstance, outerScopePickerReportOptions) {

                    $scope.isModalOpened = true;
                    $scope.model = {};
                    $scope.model.reportOptions = outerScopePickerReportOptions;

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

                        $scope.ok();
                    });

                    $scope.$on('$locationChangeSuccess', function (event, newUrl, oldUrl) {
                        if (newUrl !== oldUrl &&
                            $uibModalInstance &&
                            $scope.isModalOpened) {
                            try {
                                $scope.isModalOpened = false;
                                $uibModalInstance.dismiss('cancel');
                            } catch (e) {
                            }
                        }
                    });

                    $scope.cancel = function () {
                        $scope.isModalOpened = false;
                        $uibModalInstance.dismiss('cancel');
                    };

                    $scope.model.reportOptions.cancelDialog = $scope.cancel;    // this is here to provide a handle to close dialog from the 'spEntityReportPicker' when creating a new related entity.
                }
            ];

            /////
            // Determine if this control is directly hosted in a tab (it may not be despite its name!)
            /////
            scope.$watch('parentControl', function (value) {
                if (!value) {
                    scope.isInTab = scope.isInDesign; // parentControl is not set for tabs in the builder (fix this?)
                } else {
                    scope.isInTab = sp.result(value, 'isOfType.0.nsAlias') === 'console:tabContainerControl';
                }
            });

            /////
            // When the is-read-only value changes, update the model.
            /////
            scope.$watch("isReadOnly", function (value) {
                scope.model.isReadOnly = value;

                // If we are in view mode and we have display report
                // changes, clear them and refresh the report.
                if (scope.model.isReadOnly &&
                    (sp.result(scope, 'displayReportOptions.reportModel.inclids.length') ||
                    sp.result(scope, 'displayReportOptions.reportModel.exclids.length'))) {
                    scope.displayReportOptions.reportModel.inclids = [];
                    scope.displayReportOptions.reportModel.exclids = [];
                    refreshDisplayReport();
                }
            });

            scope.$watch("formMode", function (value) {
                if (!value) {
                    return;
                }

                // if we came in as createForm (from a report or a screen) and after saving we switched to viewForm without transitioning to viewForm state, then reset the flag
                var navItem = spNavService.getCurrentItem();
                if (navItem && navItem.href && navItem.href.includes('viewForm?') && $state.current.name === 'createForm') {
                    scope.disallowCreateRelatedEntityInNewMode = false;
                }
            });

            /////
            // When the is-embedded value changes, update the model.
            /////
            scope.$watch("isEmbedded", function (value) {
                scope.model.isEmbedded = value;
                if (scope.displayReportOptions) {
                    scope.displayReportOptions.disableActions = value;
                }
            });

            scope.$watch('formData', function () {
                if (scope.formData) {
                    $timeout(function () {
                        var existingReportId, existingRelationDetail,
                            reportChanged, relationDetailChanged;

                        scope.displayReportOptions.formDataEntity = scope.formData;
                        scope.displayReportOptions.modifyAccessDenied = sp.result(scope.formData, 'canModify') === false;

                        if (scope.displayReportOptions) {
                            existingReportId = scope.displayReportOptions.reportId;
                            existingRelationDetail = scope.displayReportOptions.relationDetail;

                            var relationDetail = {};
                            relationDetail.eid = scope.formData.id();
                            relationDetail.relid = scope.relationshipToRender ? scope.relationshipToRender.id() : undefined;
                            relationDetail.direction = scope.isReversed ? 'fwd' : 'rev'; // THIS LOOKS WRONG but isn't, the relationship direction on the report filter is reversed because the report is on the pointed at type not this type.
                            scope.displayReportOptions.relationDetail = relationDetail;

                            setDisplayReport();

                            reportChanged = existingReportId !== scope.displayReportOptions.reportId;
                            relationDetailChanged = !_.isEqual(existingRelationDetail, scope.displayReportOptions.relationDetail);

                            if (reportChanged || relationDetailChanged) {
                                scope.displayReportOptions.selectedItems = [];
                            }
                        }

                        // update faux related entities that need to be included or excluded from display report.
                        updateResourcesToIncludeExclude();

                        if (_.isUndefined(scope.pickerReportOptions.relationshipFilters)) {
                            // This is here to handle the initial load.
                            // The updateFilteredControlData events will handle updates from then on.
                            scope.pickerReportOptions.relationshipFilters = spEditForm.getRelationshipFilterData(scope.formControl, scope.formData);
                        }
                    }, 100);
                }
            });

            scope.$on('refreshFormData', function (event, data) {
                //
                // The edit form is forcing a refresh of all data
                //
                refreshDisplayReport({ isRefresh: true });
            });

            scope.$on('updateFilteredControlData', function (event, data) {
                // A filter source has changed.
                // Only update this control if it is filtered by the changed filter source.
                if (data &&
                    data.filteredControlIds &&
                    _.includes(data.filteredControlIds, scope.formControl.id())) {
                    scope.pickerReportOptions.relationshipFilters = spEditForm.getRelationshipFilterData(scope.formControl, scope.formData);
                }
            });

            // add
            scope.$on('spReportEventGridDoubleClicked', function (event, selectedItems) {
                event.stopPropagation();

                // only run this if double click originated form **picker report** grid
                if (event.targetScope.options === scope.pickerReportOptions) {
                    scope.handleOk();
                }
            });

            // Register to be notified when the report model is ready
            scope.$on('spReportEventModelReady', function (event, model) {

                // only run this if event originated form **display report**
                if (event.targetScope.options === scope.displayReportOptions) {
                    scope.displayReportOptions.reportModel = model;
                }
            });

            scope.$on('formRenderControlEventNoParentResourceAvailable', function () {
                dummyReportRun();
            });

            scope.$on('gather', function (event, callback) {
                callback(scope.formControl, scope.parentControl, iElement);
            });

            // #23850. this event is to check if this control is going to run report in context of a resource. If no resource is available then it does a dummy run of the report to bring the report columns.
            scope.$emit('spTabRelRenderCtrlEventIsParentResourceAvailable', function (parentResourceAvailable) {
                if (parentResourceAvailable === false) {
                    dummyReportRun();
                }
            });

            var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spTabRelationshipRenderControl/spTabRelationshipRenderControl.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                iElement.append(clone);
            });

            // helpers
            function updateRelationshipInstance(isHandleOk) {
                // update faux resources to incl and exclude lists
                updateResourcesToIncludeExclude();
                if (isHandleOk === true) {
                    return refreshDisplayReport({isRefresh: true});
                } else {
                    // Report gets refreshed by the context menu refresh callback
                    return $q.when();
                }
            }

            // Remove the deleted relationship instances from the form data.
            // This is required because after a related instance is removed and
            // the parent entity is saved we can get into situations where the user
            // does not have access to the related entity anymore.
            // This means that subsequent saves will raise a security exception as
            // the related entity (which was removed) is still present in the
            // entity graph.
            function removeDeletedInstancesFromFormData(deletedInstances) {
                var relEntities;

                if (!deletedInstances) {
                    return;
                }

                relEntities = scope.getRelationship();

                if (!relEntities) {
                    return;
                }

                _.remove(relEntities.getRelationshipContainer().instances, function (ri) {
                    if (!ri || !ri.entity) {
                        return false;
                    }

                    return _.some(deletedInstances, function (di) {
                        return di && di.entity && (di.entity.id() === ri.entity.id());
                    });
                });
            }

            function saveFormDataEntityAndRefreshDisplayReport(isHandleOk) {
                var relEntitiesForDelete;
                var deletedInstances;

                if (!isHandleOk) {
                    relEntitiesForDelete = scope.getRelationship();
                    if (relEntitiesForDelete) {
                        deletedInstances = _.filter(relEntitiesForDelete.getRelationshipContainer().instances, function (ri) {
                            return ri && (ri.getDataState() === spEntity.DataStateEnum.Delete);
                        });
                    }
                }

                // todo: scope.busyOptions.isBusy = true;
                return spEditForm.saveFormData(scope.formData).then(
                    function () {
                        removeDeletedInstancesFromFormData(deletedInstances);
                        return updateRelationshipInstance(isHandleOk);
                    },
                    function (error) {
                        //raise alert
                        spAlertsService.addAlert(spEditForm.formatSaveErrorMessage(error), {severity: spAlertsService.sev.Error});
                        var relEntities = scope.getRelationship();
                        if (isHandleOk === true) {
                            //remove all entities from inclids list
                            _.forEach(scope.displayReportOptions.reportModel.inclids, function (includeId) {
                                relEntities.remove(includeId);
                            });
                        } else {
                            //add back all entities from exclids list
                            _.forEach(scope.displayReportOptions.reportModel.exclids, function (exclid) {
                                relEntities.add(exclid);
                            });
                        }

                        //update and reload report
                        return updateRelationshipInstance(isHandleOk);
                    }
                    //$scope.busyOptions.isBusy = false;
                );
            }

            function refreshDisplayReport(overrideParams) {
                if (scope.displayReportOptions && scope.displayReportOptions.reportModel) {

                    var params = {
                        includeMetadata: true,
                        isMobile: scope.isMobile
                    };

                    if (overrideParams) {
                        params = _.extend(params, overrideParams);
                    }

                    if (overrideParams && overrideParams.hint && overrideParams.hint === 'delete') {
                        var deletedInstances = _.map(scope.displayReportOptions.selectedItems, 'eid');

                        if (deletedInstances && deletedInstances.length) {
                            var relEntities = scope.getRelationship();

                            if (relEntities) {
                                var instances = relEntities.getRelationshipContainer().instances;

                                if (instances) {
                                    _.forEach(deletedInstances, function (deletedInstanceId) {
                                        _.remove(instances, function (inst) {
                                            return inst.entity.id() === deletedInstanceId;
                                        });
                                    });
                                }
                            }

                            var navItem = spNavService.getCurrentItem();
                            _.forEach(deletedInstances, function (deletedInstanceId) {
                                spEditForm.removeCreatedChildEntity(navItem, scope.relationshipToRender.idP, deletedInstanceId);
                            });
                            
                        }
                    }

                    return spReportModelManager(scope.displayReportOptions.reportModel).refreshReportData(params);
                }
                return $q.when();
            }

            // set display report
            function setDisplayReport() {
                var formId;
                var rpt = spEditForm.getRelControlDisplayReport(scope.formControl);
                if (rpt) {
                    scope.displayReportOptions.reportId = rpt.idP;
                    formId = sp.result(spNavService.getCurrentItem(), 'data.formControl.idP');

                    if (formId &&
                        scope.displayReportOptions.reportId) {
                        editFormCache.assignInvalidatingEntityToForm(scope.displayReportOptions.reportId, formId);
                    }
                }
                else {
                    if (!scope.templateReport) {
                        spEditForm.getTemplateReportP().then(function (tr) {
                            if (tr) {
                                scope.templateReport = tr;
                                scope.displayReportOptions.reportId = tr.idP;
                            }
                        });
                    } else {
                        scope.displayReportOptions.reportId = scope.templateReport.idP;
                    }
                }
            }

            function updateResourcesToIncludeExclude() {
                var rel = scope.getRelationship();
                if (!rel || !rel.getInstances || !scope.displayReportOptions.reportModel) {
                    return; // rel could not be found yet
                }

                var relInstances = rel.getInstances();

                scope.displayReportOptions.reportModel.inclids = _.chain(relInstances)
                    .filter({dataState: spEntity.DataStateEnum.Create})
                    .map('entity').map('idP')
                    .value();

                scope.displayReportOptions.reportModel.exclids = _.chain(relInstances)
                    .filter({dataState: spEntity.DataStateEnum.Delete})
                    .map('entity').map('idP')
                    .value();
            }

            function defaultGetActionExecutionContext(action, ids) {
                return {
                    scope: scope,
                    state: action.state,
                    selectionEntityIds: ids,
                    entityContextId: scope.formData.idP,            // The entity the tab is on
                    isEditMode: false,
                    returnToParent: true,
                    disallowCreateRelatedEntityInNewMode: scope.disallowCreateRelatedEntityInNewMode,
                    refreshDataCallback: function (hint) {
                        refreshDisplayReport({isRefresh: true, hint: hint});
                    },
                    onBeforeNavigate: function (state, id, params) {
                        // add params to autofill relationship value
                        if (state === 'createForm') {   // if creating new child entity
                            var navItem = spNavService.getCurrentItem();
                            if (!sp.isNullOrUndefined(scope.formData)) {
                                var entity = (scope.formData.dataState === spEntity.DataStateEnum.Create) ? undefined : scope.formData; // if  entity the tab is on is 'new' then we don't want to pass temporary Id but still pass the relationship and direction as this is used for on return from creting child entity
                                spEditForm.addAutoFillRelationshipParams(navItem, entity, scope.formControl);
                            }

                            scope.$emit("addOnReturnFromChildCreate", function (fscope, formData) {
                                spEditForm.setRelationshipOnReturnFromChildCreate(spNavService.getCurrentItem(), scope.relationshipToRender, scope.formControl.isReversed, scope.formData);
                            });
                        }
                        return true;
                    }
                };
            }

            // to get the report columns, do a dummy run of report (with related entityId of 1 so that when the report is run, it doesn't bring any data.)
            function dummyReportRun() {
                if (scope.displayReportOptions) {
                    var relationDetail = {};
                    relationDetail.eid = 1; // hack: running with related entityId of 1 so that when the report is run, it doesn't bring any data.
                    relationDetail.relid = scope.relationshipToRender ? scope.relationshipToRender.id() : undefined;
                    relationDetail.direction = scope.isReversed ? 'fwd' : 'rev'; // THIS LOOKS WRONG but isn't, the relationship direction on the report filter is reversed because the report is on the pointed at type not this type.
                    scope.displayReportOptions.relationDetail = relationDetail;

                    setDisplayReport();
                }
            }
        }
    }
}());