// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnTimeKFieldRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<rn-standard-form-control control="$ctrl.control" inline="$ctrl.formOptions.inline" validation-messages="$ctrl.customValidationMessages">
    <sp-time-control model="$ctrl.model"></sp-time-control>
</rn-standard-form-control>
`,
            controller: TimeKFieldRenderControlController
        });

    function TimeKFieldRenderControlController($scope, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        //TODO see spTimeKFieldRenderControl for some things we need to consider doing
        // such as validation, checking for change before setting the resource (which I thought the
        // spEntity stuff already handles, and ignoring the watch fired on the data bound value
        // when we set it from the entity

        $ctrl.$onInit = () => {

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

                //todo - validation ... see spTimeKFieldRenderControl

                const newValue = _.isDate(value) ? value : null;
                $ctrl.resource.setField(fieldToRender.eid(), newValue, spEntity.DataType.Time);
            });
        };

        this.$onChanges = changes => {
            // console.log('TimeKFieldRenderControlController', _.keys(changes));

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