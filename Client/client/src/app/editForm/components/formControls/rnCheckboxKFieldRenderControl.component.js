// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnCheckboxKFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" 
                          validation-messages="$ctrl.customValidationMessages">
    <sp-checkbox-control model="$ctrl.model"></sp-checkbox-control>
</rn-standard-form-control>
`,
            controller: CheckboxKFieldRenderControlController
        });

    function CheckboxKFieldRenderControlController($scope, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        this.$onInit = () => {

            console.assert($ctrl.control);

            // form data => UI (via model object)
            $scope.$watch(() => {
                const fieldToRender = $ctrl.control.fieldToRender;
                return fieldToRender && $ctrl.resource &&
                    $ctrl.resource.getField(fieldToRender.idP);
            }, value => {
                $ctrl.model.value = value;
            });

            // UI (via model object) => form data
            $scope.$watch("$ctrl.model.value", (value) => {
                if (_.isUndefined(value)) return;
                if (!$ctrl.resource) return;

                const fieldToRender = $ctrl.control.fieldToRender;
                const resource = $ctrl.resource;

                resource.setField(fieldToRender.eidP, value, spEntityUtils.dataTypeForField(fieldToRender));
            });
        };

        this.$onChanges = changes => {
            // console.log('CheckboxKFieldRenderControlController', _.keys(changes));

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.model = $ctrl.model || {};

            _.assign($ctrl, spFieldControlService.getFieldControlProps($ctrl.control, options));
            _.assign($ctrl.model, spFieldControlService.getFieldInputModel($ctrl.control, $ctrl));
            _.assign($ctrl.control, spFieldControlService.getFormControlValidators($ctrl.control, $ctrl));
        };
    }
}());