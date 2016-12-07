// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {

    angular.module('app.editForm.multiPickerController', ['mod.app.editForm', 'sp.navService']);

    angular.module('app.editForm.multiPickerController')
        .controller('multiPickerController', MultiPickerController);

    function MultiPickerController($scope, $state, spEditForm, spNavService, $uibModalStack) {
        'use strict';

        // init

        spEditForm.commonRelFormControlInit($scope, {
            validator: validate,
            areCreating: $state.current.name === 'createForm'
        });

        let form = spUtils.result(spNavService, 'getCurrentItem.data.formControl');
        if (!form) {
            var modal = $uibModalStack.getTop();
            if (modal) {
                form = _.get(modal, 'value.modalScope.model.form');
            }
        }

        let isSourceRelationshipFilterControl = spEditForm.isSourceRelationshipFilterControl(form, $scope.formControl);

        // update picker optios that would be bound to UI control
        $scope.pickerOptions = {
            entityTypeId: $scope.relationshipToRender && $scope.relationshipToRender.toType ? $scope.relationshipToRender.toType.idP : null,
            selectedEntities: null,
            hideDescription: true,
            entityType: $scope.relationshipToRender,
            disabled: $scope.isInDesign,
            modifyAccessDenied: undefined,
            isInDesign: $scope.isInDesign,
            isInPicker: true,
            useReportToPopulatePicker: $scope.useReportToPopulatePicker,
            useEntitiesToPopulatePicker: $scope.useReportToPopulatePicker
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

        // NOTE: *** The following watch has a suspected bug in the case where a trigger changes a form value as part of the save. It will keep the old value.
        var dataWatch = 'formData && relTypeId && (formData.getRelationshipContainer(relTypeId).changeId + "|" + formData.id())'; // if the entity changes or the relationship is updated.

        $scope.$watch(dataWatch, function (value) {
            var selectedEntities;

            selectedEntities = getSelectedEntities($scope);
            $scope.displayString = spEditForm.getEntitiesDisplayName(selectedEntities);
            setAccessControlFieldsValue($scope.formData);
            if (!$scope.isReadOnly) {
                $scope.pickerOptions.selectedEntityIds = selectedEntityIds($scope);

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
            $scope.$watchCollection("pickerOptions.selectedEntities", function (value) {
                if ($scope.formData && _.isArray(value)) {
                    var existingValue = $scope.formData.getRelationship($scope.relationshipToRender.id());
                    if (!angular.equals(existingValue, value)) {
                        $scope.formData.setRelationship($scope.relationshipToRender.id(), value);
                        $scope.displayString = spEditForm.getEntitiesDisplayName(value);
                        validate();
                    }
                }
            });
        }

        function setAccessControlFieldsValue(formData) {
            if (formData) {
                var canModify = sp.result(formData, 'canModify');
                $scope.pickerOptions.modifyAccessDenied = canModify === false;
            }
        }

        function validate() {
            spEditForm.validateRelationshipControl($scope, getSelectedEntities($scope));
        }

        // validate relationship form control
        $scope.$on("validateForm", function (event) {
            validate();
        });

        $scope.$on('formControlUpdated', function () {
            spEditForm.commonRelFormControlInit($scope, {
                validator: validate,
                areCreating: $state.current.name === 'createForm'
            });
        });


        // get selected entities
        function getSelectedEntities(scope) {
            if (scope.formData) {
                return scope.formData.getRelationship($scope.relationshipToRender.id());
            }
        }

        // get selected entity Ids
        function selectedEntityIds(scope) {
            var arr = [];
            var selectedEntities = getSelectedEntities(scope);
            if (selectedEntities && selectedEntities.length > 0) {
                for (var i = 0; i < selectedEntities.length; i++) {
                    if (selectedEntities[i]) {
                        arr.push(selectedEntities[i].id());
                    }
                }
            }
            return arr;
        }


    }

})();

