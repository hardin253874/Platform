// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnSingleLineTextControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/formControls/rnSingleLineTextControl.tpl.html',
            controller: SingleLineTextControlController
        });

    function SingleLineTextControlController($scope, spEditForm, spFieldValidator, spFieldControlService) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        this.$onInit = () => {
            console.assert($ctrl.control);

            $ctrl.httperizeUrl = spEditForm.httperizeUrl;
            $ctrl.cleanFieldValue = cleanFieldValue;
            $ctrl.sanitizer = null;

            // form data => UI (via model object)
            $scope.$watch(() => {
                const fieldToRender = $ctrl.control.fieldToRender;
                return fieldToRender && $ctrl.resource &&
                    $ctrl.resource.getField(fieldToRender.idP);
            }, value => {
                $ctrl.model.fieldValue = value;
                $ctrl.model.displayString = rnEditForm.getFieldDisplayString($ctrl.fieldDisplayType, value);
            });

            // UI (via model object) => form data
            $scope.$watch("$ctrl.model.fieldValue", (value) => {
                if (_.isUndefined(value)) return;
                if (!$ctrl.resource) return;

                const fieldToRender = $ctrl.control.fieldToRender;
                $ctrl.resource.setField(fieldToRender.eidP, value, spEntity.DataType.Int32);

                $ctrl.model.displayString = rnEditForm.getFieldDisplayString($ctrl.fieldDisplayType, value);
            });
        };

        this.$onChanges = changes => {
            // console.log('SingleLineTextControlController', _.keys(changes));

            updateModel();
        };

        function updateModel() {
            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);
            $ctrl.model = $ctrl.model || {};

            _.assign($ctrl, spFieldControlService.getFieldControlProps($ctrl.control, options));
            _.assign($ctrl.model, spFieldControlService.getFieldInputModel($ctrl.control, $ctrl));
            _.assign($ctrl.control, spFieldControlService.getFormControlValidators($ctrl.control, $ctrl));

            const fieldToRender = $ctrl.control.fieldToRender;
            if (fieldToRender) {
                $ctrl.isReadOnly = $ctrl.isReadOnly || fieldToRender.isCalculatedField;
                $ctrl.fieldDisplayType = rnEditForm.getFieldDisplayType(fieldToRender) || '';
                $ctrl.fieldInputType = rnEditForm.getFieldInputType(fieldToRender) || '';
                $ctrl.allowMultiline = fieldToRender.allowMultiLines;
                $ctrl.sanitizer = spFieldValidator.getSanitizer(fieldToRender);

                $ctrl.model.fieldValue = $ctrl.resource && $ctrl.resource.getField(fieldToRender.eidP) || '';
                $ctrl.model.displayString = $ctrl.model.fieldValue || '';
            }
        }

        function cleanFieldValue() {
            $ctrl.fieldValue = $ctrl.sanitizer && $ctrl.sanitizer($ctrl.fieldValue) || $ctrl.fieldValue;
        }
    }

})();