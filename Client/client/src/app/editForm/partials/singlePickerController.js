// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, sp */

(function () {
    'use strict';

    angular.module('app.editForm.singlePickerController', [
        'mod.app.editForm',
        'mod.common.ui.spDialogService'
    ]);

    angular.module('app.editForm.singlePickerController')
        .controller('singlePickerController', SinglePickerController)
        .factory('rnPickerService', rnPickerService);

    function rnPickerService($state, spEditForm) {
        'ngInject';

        return {
            configureReportAsPickerSource,
            setDefaultPickerOptions,
            initForRelControl
        };

        function configureReportAsPickerSource($ctrl, reportId) {
            $ctrl.useReportToPopulatePicker = true;
            $ctrl.pickerReportServiceOptions = {
                reportId: reportId,
                metadata: 'colbasic',
                relfilters: {},
                entityTypeId: 0
            };
        }

        function initForRelControl($ctrl) {
            const resource = $ctrl.resource || $ctrl.formData; // temp handling of old and new way

            spEditForm.commonRelFormControlInit($ctrl, {
                validator: () => {
                    const selectedEntities = resource.getRelationship($ctrl.relTypeId);
                    spEditForm.validateRelationshipControl($ctrl, selectedEntities);
                },
                areCreating: $state.current.name === 'createForm'
            });
        }

        function setDefaultPickerOptions($ctrl, formControl, options = {}) {

            const pickerOptions = {
                formControl: formControl,
                parentControl: options.parentControl,
                entityTypeId: options.entityType,
                selectedEntity: null,
                selectedEntities: null,
                multiSelect: getIsAToManyRelationship(formControl.relationshipToRender),
                isDisabled: options.isDisabled,
                disabled: options.isInDesign || options.isReadOnly,
                modifyAccessDenied: options.modifyAccessDenied,
                createAccessDenied: options.createAccessDenied,
                useReportToPopulatePicker: options.useReportToPopulatePicker,
                useEntitiesToPopulatePicker: options.useReportToPopulatePicker,
                disallowCreateRelatedEntityInNewMode: options.disallowCreateRelatedEntityInNewMode
            };

            if (!options.isReadOnly && options.useReportToPopulatePicker && options.pickerReportServiceOptions) {

                options.pickerReportServiceOptions.entityTypeId = pickerOptions.entityTypeId;
                loadPickerFromReport();

                // NOTE any using components or controllers should watch on 'pickerOptions.relationshipFilters' and
                // refresh using something like:
                // $scope.$watch('pickerOptions.relationshipFilters', $scope.pickerOptions.onFiltersChanged);

                pickerOptions.onFiltersChanged = filters => {

                    console.log('onFiltersChanged', filters);

                    if (filters && !_.isEqual(filters, options.pickerReportServiceOptions.relfilters)) {
                        options.pickerReportServiceOptions.relfilters = filters;
                        loadPickerFromReport();
                    }
                };
            }

            $ctrl.pickerOptions = pickerOptions;
            return $ctrl;

            function loadPickerFromReport() {
                // the following will async fill the pickerOptions.entities based on the report run
                spEditForm.setPickerEntitiesFromReportData(options.pickerReportServiceOptions, pickerOptions);
            }
        }

    }

    function SinglePickerController($state, $scope, spEditForm, spDialogService, spNavService, $uibModalStack) {
        'ngInject';

        initScopeForFormControl();

        var form = sp.result(spNavService, 'getCurrentItem.data.formControl');
        if (!form) {
            var modal = $uibModalStack.getTop();
            if (modal) {
                form = _.get(modal, 'value.modalScope.model.form');
            }
        }

        var isSourceRelationshipFilterControl = spEditForm.isSourceRelationshipFilterControl(form, $scope.formControl);

        $scope.openDetail = openDetail;

        $scope.pickerOptions = {
            formControl: $scope.formControl,
            parentControl: $scope.parentControl,
            entityTypeId: $scope.entityType,
            selectedEntity: null,
            selectedEntities: null,
            multiSelect: getIsAToManyRelationship($scope.formControl.relationshipToRender),
            isDisabled: $scope.isDisabled,
            disabled: ($scope.isInDesign || $scope.isReadOnly),
            modifyAccessDenied: undefined,
            createAccessDenied: $scope.createAccessDenied,
            useReportToPopulatePicker: $scope.useReportToPopulatePicker,
            useEntitiesToPopulatePicker: $scope.useReportToPopulatePicker,
            disallowCreateRelatedEntityInNewMode: $scope.disallowCreateRelatedEntityInNewMode
        };

        if (!$scope.isReadOnly && $scope.useReportToPopulatePicker && $scope.pickerReportServiceOptions) {

            $scope.pickerReportServiceOptions.entityTypeId = $scope.pickerOptions.entityTypeId;

            spEditForm.setPickerEntitiesFromReportData($scope.pickerReportServiceOptions, $scope.pickerOptions);

            $scope.$watch('pickerOptions.relationshipFilters', function (filters) {
                if (filters && !_.isEqual(filters, $scope.pickerReportServiceOptions.relfilters)) {
                    $scope.pickerReportServiceOptions.relfilters = filters;

                    // Run report and populate entities
                    spEditForm.setPickerEntitiesFromReportData($scope.pickerReportServiceOptions, $scope.pickerOptions);
                }
            });
        }

        $scope.$on("validateForm", validate);
        $scope.$on("validateRelationship", validate);

        $scope.$on('formControlUpdated', initScopeForFormControl);

        $scope.$watch('customValidationMessages.length', function () {
            // Note - should generalise this somewhat, but at present adding in special
            // for inline editing forms. And to make it somewhat more efficient etc.
            $scope.$emit('validationMessages', {
                formControl: $scope.formControl,
                messages: $scope.customValidationMessages
            });
        });

        $scope.$watch("formMode", function (value) {
            if (!value) return;

            // console.log('singlePickerController: formMode', value);

            // if we came in as createForm (from a report or a screen) and after saving we switched to viewForm
            // without transitioning to viewForm state, then reset the flag
            var navItem = spNavService.getCurrentItem();
            var href = sp.result(navItem, 'href');
            if (href && href.indexOf('viewForm?') >= 0 && $state.current.name === 'createForm') {
                $scope.disallowCreateRelatedEntityInNewMode = false;
                if ($scope.pickerOptions) {
                    $scope.pickerOptions.disallowCreateRelatedEntityInNewMode = $scope.disallowCreateRelatedEntityInNewMode;
                }
            }
        });

        // if the entity changes or the relationship is updated.
        var dataWatch = 'formData && relTypeId && (formData.getLookup(relTypeId).id() + "|" + formData.id())';

        $scope.$watch(dataWatch, function (value) {

            // console.log('singlePickerController: dataWatch', value);

            let selectedEntity = getSelectedEntity($scope);
            let selectedEntities = getSelectedEntities($scope);

            if (selectedEntities) {
                // use a copy of the array so we are not modifying the internals of the entity
                selectedEntities = selectedEntities.slice(0);
            }
            //"Icon"
            var formattingType = '';
            if ($scope.formControl.relationshipToRender &&
                    $scope.formControl.relationshipToRender.enumValueFormattingType
            )
                formattingType = $scope.formControl.relationshipToRender.enumValueFormattingType.name;
            

            $scope.displayString = spEditForm.getDisplayName(selectedEntities);
            $scope.displayStyle = spEditForm.getDisplayStyle(selectedEntities, formattingType);
            $scope.pickerOptions.displayString = $scope.displayString;
            setShowExpander();
            $scope.displayEntities = selectedEntities;

            setAccessControlFieldsValue($scope.formData);

            if (!$scope.isReadOnly) {
                $scope.pickerOptions.selectedEntityId = selectedEntity ? selectedEntity.id() : 0;
                $scope.pickerOptions.selectedEntity = selectedEntity ? selectedEntity : null;
                $scope.pickerOptions.selectedEntities = selectedEntities;
                $scope.pickerOptions.formData = $scope.formData;
                if (_.isUndefined($scope.pickerOptions.relationshipFilters)) {
                    // This is here to handle the initial load.
                    // The updateFilteredControlData events will handle updates from then on.
                    $scope.pickerOptions.relationshipFilters = spEditForm.getRelationshipFilterData($scope.formControl, $scope.formData);
                }

                if (isSourceRelationshipFilterControl) {
                    // This control is a filter source.
                    // Emit a message to have other filtered controls update themselves.
                    $scope.$emit('filterSourceControlDataChanged', {sourceControl: $scope.formControl});
                }
            }
        });


        $scope.$on('updateFilteredControlData', function (event, data) {
            // A filter source has changed.
            // Only update this control if it is filtered by the changed filter source.
            if (data &&
                data.filteredControlIds &&
                _.includes(data.filteredControlIds, $scope.formControl.id())) {
                // Update the filters
                $scope.pickerOptions.relationshipFilters = spEditForm.getRelationshipFilterData($scope.formControl, $scope.formData);
            }
        });


        if (!$scope.isReadOnly) {
            $scope.$watch("pickerOptions.selectedEntity", function (value) {
                if ($scope.formData) {
                    var existingValue = $scope.formData.getLookup($scope.relTypeId);

                    if (existingValue !== value) {
                        $scope.formData.setLookup($scope.relTypeId, value);
                        $scope.displayString = spEditForm.getDisplayName(value);
                        validate();
                    }
                }
            });
        }

        if (!$scope.isReadOnly) {
            $scope.$watchCollection("pickerOptions.selectedEntities", function (value) {
                if ($scope.formData) {
                    var existingValue = $scope.formData.getRelationship($scope.relTypeId);
                    if (!angular.equals(existingValue, value)) {
                        $scope.formData.setRelationship($scope.relTypeId, value);
                        $scope.displayString = spEditForm.getDisplayName(value);
                        validate();
                    }
                }
            });
        }

        // functions for link
        $scope.handleLinkClick = function (selectedEntityId) {
            if (selectedEntityId > 0) {
                var params = {};

                var formId = spEditForm.getControlConsoleFormId($scope.formControl);
                if (formId && _.isNumber(formId)) {
                    params.formId = formId;
                }

                $scope.$emit("addOnReturnFromChildUpdate", function (fscope, formData) {
                    // update the name of lookup if child entity has been updated
                    spEditForm.updateLookupNameOnReturnFromChildUpdate(spNavService.getCurrentItem(), $scope.formControl.getRelationshipToRender(), $scope.formControl.getIsReversed(), formData);
                });

                spNavService.navigateToChildState('viewForm', selectedEntityId, params);
            }
            else {
                console.log('Cannot navigate to child as no entity is selected.');
            }

        };

        function initScopeForFormControl() {
            spEditForm.commonRelFormControlInit($scope, {
                validator: validate,
                areCreating: $state.current.name === 'createForm'
            });
        }

        function validate() {
            spEditForm.validateRelationshipControl($scope, getSelectedEntities($scope));
        }

        function setShowExpander() {
            var showExpanderlength = 29;    // currently this is the magic number after which the text overflow is triggered. this may break if the stlye is changed. todo: add test for this
            $scope.showExpander = $scope.displayString && $scope.displayString.length > showExpanderlength;
        }

        function modalInstanceCtrl(scope, $uibModalInstance, modalTitle, displayEntities) {
            scope.model = {};
            scope.model.title = modalTitle;
            scope.model.displayEntities = displayEntities;

            scope.linkClicked = function (selectedEntityId) {
                $uibModalInstance.close(false);
                $scope.handleLinkClick(selectedEntityId);
            };

            scope.closeDetail = function () {
                $uibModalInstance.close(scope.model);
            };
        }

        function openDetail(templateUrl) {
            var defaults = {
                templateUrl: templateUrl,
                controller: ['$scope', '$uibModalInstance', 'modalTitle', 'displayEntities', 'handleLinkClick', modalInstanceCtrl],
                resolve: {
                    modalTitle: function () {
                        return $scope.titleModel.name;
                    },
                    displayEntities: function () {
                        return $scope.displayEntities;
                    },
                    handleLinkClick: function () {
                        return $scope.handleLinkClick;
                    }
                }
            };

            var options = {
                callback: function (result) {
                }
            };

            spDialogService.showDialog(defaults, options);
        }

        function setAccessControlFieldsValue(formData) {
            if (formData) {
                var canModify = sp.result(formData, 'canModify');
                $scope.pickerOptions.modifyAccessDenied = canModify === false;
            }
        }
    }

    function getSelectedEntities(scope) {
        if (scope.formData && scope.relTypeId) {
            return scope.formData.getRelationship(scope.relTypeId);
        }
    }

    function getSelectedEntity(scope) {
        var entities = getSelectedEntities(scope);
        return _.first(entities) || null;
    }

    function getIsAToManyRelationship(rel) {
        const card = sp.result(rel, 'relationshipToRender.cardinality.eidP.nsAlias');
        return card === 'core:manyToMany' ||
            !rel.isReversed && card === 'core:oneToMany' ||
            rel.isReversed && card === 'core:manyToOne';
    }
})();
