// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnDateAndTimeKFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <sp-date-and-time-control ng-if="!(isMobile || isTablet)" model="$ctrl.model"></sp-date-and-time-control>
    <sp-date-and-time-mobile-control ng-if="(isMobile || isTablet)" model="$ctrl.model"></sp-date-and-time-mobile-control>
</rn-standard-form-control>
`,
            controller: DateAndTimeKFieldRenderControlController
        });

    function DateAndTimeKFieldRenderControlController($scope, spFieldControlService) {
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
                const newValue = value || value === 0 ? parseDate(value) : null;
                $ctrl.resource.setField(fieldToRender.eid(), newValue, spEntity.DataType.DateTime);
            });
        };

        this.$onChanges = changes => {
            // console.log('DateAndTimeKFieldRenderControlController', _.keys(changes));

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
            return sp.parseDate(dateString);
        }
    }
}());