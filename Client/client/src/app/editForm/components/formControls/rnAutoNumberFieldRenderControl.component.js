// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnAutoNumberFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <div class="rn-control-value">{{$ctrl.formattedValue}}</div>
</rn-standard-form-control>
`,
            controller: AutoNumberFieldRenderControlController
        });

    function AutoNumberFieldRenderControlController($scope, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        this.$onInit = () => {

            console.assert($ctrl.control);

            $ctrl.options = _.defaults({}, $ctrl.options, $ctrl.formOptions);
            $ctrl.model = {};

            _.assign($ctrl, spFieldControlService.getFieldControlProps($ctrl.control, $ctrl.options));
            // _.assign($ctrl.model, spFieldControlService.getFieldInputModel($ctrl.control, $ctrl));
            // _.assign($ctrl.control, spFieldControlService.getFormControlValidators($ctrl.control, $ctrl));

            // resource data => UI
            $scope.$watch(() => {
                const fieldToRender = $ctrl.control.fieldToRender;
                return fieldToRender && $ctrl.resource &&
                    $ctrl.resource.getField(fieldToRender.idP);
            }, value => {
                const fieldToRender = $ctrl.control.fieldToRender;
                if (fieldToRender && $ctrl.resource) {
                    const pattern = fieldToRender.autoNumberDisplayPattern;
                    $ctrl.formattedValue = value && pattern ?
                        jQuery.formatNumber(value, {format: pattern, locale: 'us'}).toString() :
                        value;
                }
            });
        };
    }
}());