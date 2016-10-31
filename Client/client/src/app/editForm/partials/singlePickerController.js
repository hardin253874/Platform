// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, sp */

(function () {
    'use strict';

    angular.module('app.editForm.singlePickerController', [
        'mod.app.editForm',
        'mod.common.ui.spDialogService'
    ]);

    angular.module('app.editForm.singlePickerController')
        .controller('singlePickerController', SinglePickerController);

    function SinglePickerController($state, $scope, spFieldValidator, spEditForm, spDialogService, spNavService) {
        'ngInject';

        var form, navItem, isSourceRelationshipFilterControl;
        $scope.openDetail = openDetail;

        // init
        spEditForm.commonRelFormControlInit($scope, {
            validator: validate,
            areCreating: $state.current.name === 'createForm'
        });
        var customValidationMessages = $scope.customValidationMessages;

        navItem = spNavService.getCurrentItem();
        form = sp.result(navItem, 'data.formControl');

        isSourceRelationshipFilterControl = spEditForm.isSourceRelationshipFilterControl(form, $scope.formControl);

        function getSelectedEntities(scope) {
            if (scope.formData && scope.relTypeId) {
                return scope.formData.getRelationship(scope.relTypeId);
            }
        }

        function getSelectedEntity(scope) {
            var entities = getSelectedEntities(scope);
            return _.first(entities) || null;
        }

        var card = sp.result($scope, 'relationshipToRender.cardinality.eidP.nsAlias');
        var isToMany = card === 'core:manyToMany' || !$scope.isReversed && card === 'core:oneToMany' || $scope.isReversed && card === 'core:manyToOne';

        $scope.pickerOptions = {
            formControl: $scope.formControl,
            parentControl: $scope.parentControl,
            entityTypeId: $scope.entityType,
            selectedEntity: null,
            selectedEntities: null,
            multiSelect: isToMany,
            isDisabled: $scope.isDisabled,
            disabled: ($scope.isInDesign || $scope.isReadOnly),
            modifyAccessDenied: undefined,
            createAccessDenied: $scope.createAccessDenied,
            useReportToPopulatePicker: $scope.useReportToPopulatePicker,
            useEntitiesToPopulatePicker: $scope.useReportToPopulatePicker,
            disallowCreateRelatedEntityInNewMode: $scope.disallowCreateRelatedEntityInNewMode
        };

        if (!$scope.isReadOnly &&
            $scope.useReportToPopulatePicker &&
            $scope.pickerReportServiceOptions) {
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

        $scope.$on("validateForm", function (event) {
            validate();
        });

        $scope.$on('formControlUpdated', function () {
            spEditForm.commonRelFormControlInit($scope, {
                validator: validate,
                areCreating: $state.current.name === 'createForm'
            });
        });


        $scope.$on("validateRelationship", function (event) {
            validate();
        });

        $scope.$watch('customValidationMessages.length', function () {
            // Note - should generalise this somewhat, but at present adding in special
            // for inline editing forms. And to make it somewhat more efficient etc.
            $scope.$emit('validationMessages', {
                formControl: $scope.formControl,
                messages: $scope.customValidationMessages
            });
        });

        $scope.$watch("formMode", function (value) {
            if (!value) {
                return;
            }

            // if we came in as createForm (from a report or a screen) and after saving we switched to viewForm without transitioning to viewForm state, then reset the flag
            var navItem = spNavService.getCurrentItem();
            if (navItem && navItem.href && navItem.href.includes('viewForm?') && $state.current.name === 'createForm') {
                $scope.disallowCreateRelatedEntityInNewMode = false;
                if ($scope.pickerOptions) {
                    $scope.pickerOptions.disallowCreateRelatedEntityInNewMode = $scope.disallowCreateRelatedEntityInNewMode;
                }
            }
        });

        var dataWatch = 'formData && relTypeId && (formData.getLookup(relTypeId).id() + "|" + formData.id())'; // if the entity changes or the relationship is updated.

        $scope.$watch(dataWatch, function () {
            var selectedEntity, selectedEntities;

            selectedEntity = getSelectedEntity($scope);
            selectedEntities = getSelectedEntities($scope);

            if (selectedEntities) {
                selectedEntities = selectedEntities.slice(0);    // use a copy of the array so we are not modifying the internals of the entity
            }

            $scope.displayString = spEditForm.getDisplayName(selectedEntities);

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

        function setShowExpander() {
            var showExpanderlength = 29;    // currently this is the magic number after which the text overflow is triggered. this may break if the stlye is changed. todo: add test for this

            if ($scope.displayString && $scope.displayString.length > showExpanderlength) {
                $scope.showExpander = true;
            } else {
                $scope.showExpander = false;
            }
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

        function validate() {
            spEditForm.validateRelationshipControl($scope, getSelectedEntities($scope));
        }
    }
})();
