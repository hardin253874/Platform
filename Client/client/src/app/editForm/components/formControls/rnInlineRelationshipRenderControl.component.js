// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnInlineRelationshipRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            //TODO tidy up this template ... just copied over to get it working
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <!--viewing-->
    <div ng-if="$ctrl.isReadOnly" class="rn-control-value" >
        <a ng-if="!$ctrl.isInlineEditing"
           ng-repeat="entity in $ctrl.displayEntities" style="cursor: pointer;" class="fieldValue" ng-href=""
           ng-click="$ctrl.handleLinkClick(entity.idP);">{{entity.name ? entity.name : 'Unnamed Resource'}}{{$last ? '' : ', '}}</a>

        <div ng-if="$ctrl.isInlineEditing" ng-repeat="entity in $ctrl.displayEntities" class="fieldValue">
            {{entity.name ? entity.name : 'Unnamed Resource'}}{{$last ? '' : ', '}}
        </div>
    </div>
    <!--editing-->
    <sp-inline-relationship-picker ng-if="!$ctrl.isReadOnly" options="$ctrl.pickerOptions"></sp-inline-relationship-picker>
</rn-standard-form-control>
`,
            controller: InlineRelationshipRenderControlController
        });

    function InlineRelationshipRenderControlController($scope, spEditForm, rnPickerService) {
        'ngInject';

        const $ctrl = this; // match var used in templates
        const logId = 'inlineRel';

        let filtersWatch;

        $ctrl.$onInit = () => {

            // console.log(`${logId}: $onInit`);

            // form data => UI bindings
            // $scope.$watch(() => {
            //     if (!$ctrl.resource || !$ctrl.relTypeId) return null;
            //     return $ctrl.resource.getRelationship($ctrl.relTypeId);
            // }, value => {
            //     console.log(`${logId}: rel changed`, value);
            //     onDataChanged();
            // });
            // $scope.$watch(() => {
            //     return $ctrl.resource && $ctrl.formData.idP;
            // }, value => {
            //     console.log(`${logId}: entity changed`, value);
            //     onDataChanged();
            // });

            // UI bindings => form data
            $scope.$watch("$ctrl.pickerOptions.selectedEntity", (value) => {
                if (_.isUndefined(value)) return;
                if (!$ctrl.resource) return;

                const resource = $ctrl.resource;

                // console.log(`${logId}: UI value changed`, value);
            });
        };

        this.$onChanges = changes => {
            // console.log('InlineRelationshipRenderControlController', _.keys(changes));
            updateModel();
        };

        function updateModel() {

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.isReadOnly = $ctrl.control.readOnlyControl || !options.editing;

            //rnPickerService.configureReportAsPickerSource($ctrl, 'console:enumValuesReport');
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
            $ctrl.displayEntities = selectedEntities;

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