// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnMultiChoiceRelationshipRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <div ng-if="$ctrl.isReadOnly" class="rn-control-value" >{{$ctrl.displayString}}</div>
    <sp-entity-multi-combo-picker ng-if="!$ctrl.isReadOnly" options="$ctrl.pickerOptions"></sp-entity-multi-combo-picker>
</rn-standard-form-control>
`,
            controller: MultiChoiceRelationshipRenderControlController
        });

    function MultiChoiceRelationshipRenderControlController($scope, spEditForm, rnPickerService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        let filtersWatch;

        $ctrl.$onInit = () => {

            // form data => UI bindings
            $scope.$watch(() => {
                if (!$ctrl.resource || !$ctrl.relTypeId) return null;
                const related = $ctrl.resource.getLookup($ctrl.relTypeId);
                return related && related.idP;
            }, value => {
                onDataChanged();
            });
            $scope.$watch(() => {
                return $ctrl.resource && $ctrl.resource.idP;
            }, value => {
                onDataChanged();
            });

            // UI bindings => form data
            $scope.$watch("$ctrl.pickerOptions.selectedEntity", (value) => {
                if (_.isUndefined(value)) return;
                if (!$ctrl.resource) return;

            });
        };

        this.$onChanges = changes => {
            // console.log('MultiChoiceRelationshipRenderControlController', _.keys(changes));
            updateModel();
        };

        function updateModel() {

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.isReadOnly = $ctrl.control.readOnlyControl || !options.editing;

            rnPickerService.configureReportAsPickerSource($ctrl, 'console:enumValuesReport');
            rnPickerService.initForRelControl($ctrl);
            rnPickerService.setDefaultPickerOptions($ctrl, $ctrl.control, $ctrl);

            if (filtersWatch) filtersWatch(); // cancel the previous
            filtersWatch = $scope.$watch('$ctrl.pickerOptions.relationshipFilters', $ctrl.pickerOptions.onFiltersChanged);

            onDataChanged();
        }

        function onDataChanged() {

            if (!$ctrl.resource) return;

            const {control, resource, pickerOptions} = $ctrl;

            const selectedEntities = (resource.getRelationship($ctrl.relTypeId) || []).slice(0);
            const selectedEntity = _.first(selectedEntities);

            $ctrl.displayString = spEditForm.getDisplayName(selectedEntities);

            if (!$ctrl.isReadOnly) {
                pickerOptions.displayString = $ctrl.displayString;
                pickerOptions.modifyAccessDenied = !resource.canModify;
                pickerOptions.selectedEntityId = selectedEntity ? selectedEntity.idP : 0;
                pickerOptions.selectedEntity = selectedEntity ? selectedEntity : null;
                pickerOptions.selectedEntities = selectedEntities;
                pickerOptions.formData = resource;

                if (_.isUndefined($ctrl.pickerOptions.relationshipFilters)) {
                    // This is here to handle the initial load.
                    // The updateFilteredControlData events will handle updates from then on.
                    $ctrl.pickerOptions.relationshipFilters = spEditForm.getRelationshipFilterData(control, resource);
                }

                //todo - what is this about???
                // if (isSourceRelationshipFilterControl) {
                //     // This control is a filter source.
                //     // Emit a message to have other filtered controls update themselves.
                //     $ctrl.$emit('filterSourceControlDataChanged', {sourceControl: $ctrl.control});
                // }
            }
        }

    }
}());