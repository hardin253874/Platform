// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnDateKFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <sp-date-control ng-if="!($ctrl.isMobile || $ctrl.isTablet)" model="$ctrl.model"></sp-date-control>
    <sp-date-mobile-control ng-if="$ctrl.isMobile || $ctrl.isTablet" model="$ctrl.model"></sp-date-mobile-control>
</rn-standard-form-control>
`,
            controller: DateKFieldRenderControlController
        });

    function DateKFieldRenderControlController($scope, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        $ctrl.$onInit = () => {

            console.assert($ctrl.control);

            $scope.$watch(() => {
                const fieldToRender = $ctrl.control.fieldToRender;
                return fieldToRender && $ctrl.resource &&
                    $ctrl.resource.getField(fieldToRender.idP);
            }, value => {
                $ctrl.model.value = value;
            });

            $scope.$watch("$ctrl.model.value", (value) => {
                if (_.isUndefined(value)) return;
                if (!$ctrl.resource) return;

                const fieldToRender = $ctrl.control.fieldToRender;

                //TODO - see spDateKFieldRenderControl ... it is doing some validation here??
                // - just wondering why it does but datetime control doesn't

                const newValue = value || value === 0 ? parseDate(value) : null;
                $ctrl.resource.setField(fieldToRender.eidP, newValue, spEntity.DataType.Date);
            });
        };

        this.$onChanges = changes => {
            // console.log('DateKFieldRenderControlController', _.keys(changes));

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);
            const fieldToRender = $ctrl.control.fieldToRender;

            $ctrl.model = _.assign($ctrl.model || {}, {
                minimumValue: fieldToRender.minDateTime,
                maximumValue: fieldToRender.maxDateTime,
                isRequired: fieldToRender.isRequired
            });

            _.assign($ctrl, spFieldControlService.getFieldControlProps($ctrl.control, options));
            _.assign($ctrl.model, spFieldControlService.getFieldInputModel($ctrl.control, $ctrl));
            _.assign($ctrl.control, spFieldControlService.getFormControlValidators($ctrl.control, $ctrl));
        };

        function parseDate(dateString) {
            return sp.translateToUtc(sp.parseDate(dateString));
        }
    }
}());