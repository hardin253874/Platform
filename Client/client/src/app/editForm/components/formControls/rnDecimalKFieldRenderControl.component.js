// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnDecimalKFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <sp-decimal-control class="rn-control-value" model="$ctrl.model"></sp-decimal-control>
</rn-standard-form-control>
`,
            controller: DecimalKFieldRenderControlController
        });

    function DecimalKFieldRenderControlController($scope, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        console.assert($ctrl.control);

        $ctrl.$onInit = () => {

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
                const newValue = value || value === 0 ? parseFloat(value) : null;
                $ctrl.resource.setField(fieldToRender.eidP, newValue, spEntity.DataType.Decimal);
            });
        };

        this.$onChanges = changes => {
            // console.log('DecimalKFieldRenderControlController', _.keys(changes));

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);
            const fieldToRender = $ctrl.control.fieldToRender;

            $ctrl.model = _.assign($ctrl.model || {}, {
                minimumValue: fieldToRender.minDecimal,
                maximumValue: fieldToRender.maxDecimal,
                decimalPlaces: fieldToRender.decimalPlaces
            });

            _.assign($ctrl, spFieldControlService.getFieldControlProps($ctrl.control, options));
            _.assign($ctrl.model, spFieldControlService.getFieldInputModel($ctrl.control, $ctrl));
            _.assign($ctrl.control, spFieldControlService.getFormControlValidators($ctrl.control, $ctrl));
        };
    }
}());