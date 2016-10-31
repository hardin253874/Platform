// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing a relationship picker dropdown.
      *
      * @module spDropdownRelationshipPicker
      * @example

      Using the spDropdownRelationshipPicker:


      */
    angular.module('app.editForm.spDropdownRelationshipPicker', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.app.editForm', 'mod.common.spEntityService', 'mod.app.editFormCache', 'mod.common.alerts'])
        .directive('spDropdownRelationshipPicker', function ($state, $parse, $compile, $uibPosition, $document, $templateCache, $q, $timeout, spEditForm, spEntityService, spNavService, editFormCache, spAlertsService) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'editForm/partials/spDropdownRelationshipPicker.tpl.html',
                controller: 'singleEntityPickerController',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {
                    //**** restrict the number of records to be displayed in combobox to 100 ***//
                    scope.restrictNumRecords = 100;
                    //********************************//

                    
                    var getDerivedCtxMenuFn = function () { return $q.when(); };

                    scope.pickerReportServiceOptions = {
                        metadata: 'colbasic',
                        reportId: 0,
                        startIndex: 0,
                        pageSize: scope.restrictNumRecords,
                        entityTypeId: 0,
                        relfilters: {},
                    };

                    scope.model = {
                        isAddMenuOpen: false
                    };



                    // Watch the entityTypeId for changes and build the derived types menu for the entiy type
                    scope.$watch('options.entityTypeId', function () {
                        if (scope.options.entityTypeId) {

                            scope.pickerReportServiceOptions.entityTypeId = scope.options.entityTypeId;

                            if (scope.options.useReportToPopulatePicker) {
                                spEditForm.setPickerEntitiesFromReportData(scope.pickerReportServiceOptions, scope.options);
                            }                            

                            if (scope.options.formControl && scope.options.formControl.hasField('canCreate') && scope.options.formControl.hasField('canCreateDerivedTypes')) {
                                if (scope.options.formControl.getCanCreate() === true) {
                                    buildDerivedTypesMenu(scope.options.formControl, scope.options.entityTypeId);
                                }
                            }
                            else {
                                setPageVars();
                            }
                        }
                    });

                    function setPageVars() {

                        if (scope.entityDefaultFormDict) {
                            if (_.keys(scope.entityDefaultFormDict).length > 1) {
                                scope.singleOptionAvailable = false;
                                scope.multiOptionsAvailable = true;
                            }
                            else if (_.keys(scope.entityDefaultFormDict).length === 1) {
                                scope.singleOptionAvailable = true;
                                scope.multiOptionsAvailable = false;
                            }
                            else {
                                hideNewbuttons();
                            }


                        }
                        else {
                            hideNewbuttons();
                        }

                        setTooltip();
                    }

                    function setTooltip() {
                        var tooltip = '';

                        if (scope.entityDefaultFormDict) {
                            if (scope.entityDefaultFormDict.length === 1) {
                                return spEditForm.getDisplayName(scope.entityDefaultFormDict[0]); // TODO: CHANGE THE DICT TO STORE ENTITY INSTEAD OF FORMiD. THAT WAY WE CAN HAVE NAME OF ENTITY IN TOOLTIP
                            }

                            if (scope.entityDefaultFormDict.length > 1) {
                                return 'New';
                            }
                        }
                        return tooltip;
                    }

                    function hideNewbuttons() {

                        scope.singleOptionAvailable = false;
                        scope.multiOptionsAvailable = false;
                    }

                    //-- build context menu
                    function buildDerivedTypesMenu(formControl, entityTypeId) {

                        if (!formControl.hasField('canCreateDerivedTypes')) {
                            window.alert('An error occurred building derived types menu');
                            return;
                        }

                        //******** Note ********//
                        var createHandler = 'handleCreate'; //*** this is the name of function on the scope that will be called when a menu item is clicked in the context menu ***//
                        //*********************//
                        getDerivedCtxMenuFn = _.once(function () {
                            return spEditForm.getDerivedTypesContextMenu(scope, entityTypeId, formControl.getCanCreateDerivedTypes(), createHandler)
                                .then(
                                    function () {
                                        setPageVars();
                                    },
                                    function (error) {
                                        window.alert('error building context menu: ' + error);
                                    });
                        });
                    }

                    //-- context menu item click
                    scope.handleCreate = function (entityId) {

                        if (scope.options.disallowCreateRelatedEntityInNewMode) {
                            var msg = spEditForm.getDisallowCreateRelatedEntityInNewModeMessage(scope.options.formControl.relationshipToRender, scope.options.formControl.isReversed);
                            spAlertsService.addAlert(msg, { severity: 'error', page: $state.current });
                            return;
                        }

                        var params = {};

                        if (scope.entityDefaultFormDict) {
                            var formId = scope.entityDefaultFormDict[entityId];

                            if (!_.isUndefined(formId)) {
                                if (formId && _.isNumber(formId)) {
                                    params.formId = formId;
                                }
                            }
                        }

                        // add params to autofill relationship value
                        var areCreating = $state.current.name === 'createForm';
                        var navItem = spNavService.getCurrentItem();
                        if (!spUtils.isNullOrUndefined(scope.options.formData)) {
                            var entity = areCreating ? undefined : scope.options.formData;  // if creating new entity then we don't want to pass temporary Id but still pass the relationship and direction as this is used for on return from creting child entity
                            spEditForm.addAutoFillRelationshipParams(navItem, entity, scope.options.formControl);
                        }

                        scope.$emit("addOnReturnFromChildCreate", function (fscope, formData) {
                            //SET the relationships to point to the child entity.
                            spEditForm.setLookupOnReturnFromChildCreate(spNavService.getCurrentItem(), scope.options.formControl.getRelationshipToRender(), scope.options.formControl.getIsReversed(), formData);
                        });
                        spNavService.navigateToChildState('createForm', entityId, params);
                        // todo: navigate to create new based on popup or not
                    };


                    scope.addClicked = addClicked;

                    function addClicked(event) {

                        getDerivedCtxMenuFn()
                            .then(function () {                                                // we are not loading the context menu until button is clicked, trading off performance with scale.
                                if (scope.multiOptionsAvailable && scope.contextMenu.menuItems.length > 1) {
                                    $timeout(function () { scope.model.isAddMenuOpen = true;});
                                } else
                                {
                                    singleOptionClick();                
                                }
                            });

                        //return false;
                    }


                    function singleOptionClick () {
                        // if dictionary is available and has exactly one key then use that key(i.e. entityTypeId)
                        if (scope.entityDefaultFormDict && _.keys(scope.entityDefaultFormDict).length === 1) {
                            var keys = _.keys(scope.entityDefaultFormDict);
                            scope.handleCreate(keys[0]);
                        }
                        else {
                            scope.handleCreate(scope.entityTypeId);
                        }
                    }

                    // validate relationship
                    scope.validateRelationship = function () {
                        scope.$emit('validateRelationship');
                    };

                    scope.$watch('options.isDisabled', function (newVal, oldVal) {
                        if (newVal === oldVal)
                            return;

                        setCanModifyDisabled();
                        setCanCreateDisabled();
                    });

                    scope.$watch('disabled', function () {
                        setCanModifyDisabled();
                        setCanCreateDisabled();
                    });

                    scope.$watch('options.modifyAccessDenied', function () {
                        setCanModifyDisabled();
                    });

                    scope.$watch('options.createAccessDenied', function () {
                        setCanCreateDisabled();
                    });

                    function setCanModifyDisabled() {
                        scope.canModifyDisabled = !!scope.options.isDisabled || !!scope.options.disabled || !!scope.options.modifyAccessDenied;
                    }

                    function setCanCreateDisabled() {
                        scope.canCreateDisabled = !!scope.options.isDisabled || !!scope.options.disabled || !!scope.options.createAccessDenied;
                    }

                    // Initialise the picker report from the control
                    function setPickerReport(formControl) {
                        // set picker report from the form control
                        var formId;
                        var pickerRpt = spEditForm.getRelControlPickerReport(formControl);                        

                        if (pickerRpt) {
                            scope.pickerReportServiceOptions.reportId = pickerRpt.id();                                                 
                            formId = sp.result(spNavService.getCurrentItem(), 'data.formControl.idP');

                            if (formId &&
                                scope.pickerReportServiceOptions.reportId) {
                                editFormCache.assignInvalidatingEntityToForm(scope.pickerReportServiceOptions.reportId, formId);
                            }
                        } else if (scope.templateReport) {
                            scope.pickerReportServiceOptions.reportId = scope.templateReport.id();
                        }
                    }
                    

                    if (scope.options.useReportToPopulatePicker) {
                        spEditForm.getTemplateReport(function (report) {
                            scope.templateReport = report;
                        });

                        // Watch for filter changes and run the report
                        scope.$watch('options.relationshipFilters', function (filters) {
                            if (filters &&
                                !_.isEqual(filters, scope.pickerReportServiceOptions.relfilters)) {
                                scope.pickerReportServiceOptions.relfilters = filters;

                                // get entities from the report
                                spEditForm.setPickerEntitiesFromReportData(scope.pickerReportServiceOptions, scope.options);
                            }
                        });


                        // Watch for picker report id
                        scope.$watch('options.pickerReportId', function (pickerReportId) {
                            if (pickerReportId &&                                
                                scope.pickerReportServiceOptions.reportId !== pickerReportId) {
                                scope.pickerReportServiceOptions.reportId = pickerReportId;                                
                                scope.pickerReportServiceOptions.filteredEntityIds = scope.options.filteredEntityIds;
                                // get entities from the report
                                spEditForm.setPickerEntitiesFromReportData(scope.pickerReportServiceOptions, scope.options);
                            }
                        });


                        // Watch for form control
                        scope.$watch('options.formControl', function (formControl) {
                            if (formControl) {
                                if (scope.pickerReportServiceOptions.reportId !== 0) {
                                    return; // picker report id already provided
                                }

                                // set the picker report
                                setPickerReport(formControl);

                                // get entities from the report
                                spEditForm.setPickerEntitiesFromReportData(scope.pickerReportServiceOptions, scope.options);
                            }
                        });
                    }
                }
            };
        });
}());