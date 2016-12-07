// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnStandardFormControl', {
            bindings: {
                control: '<',
                inline: '<',
                validationMessages: '<'
            },
            template: `
<div class="rn-control rnStandardFormControl" 
     ng-class="{'rnStandardFormControl--inline' : $ctrl.inline}">
    <rn-control-title class="rn-control-title rnStandardFormControl__title"
                      ng-class="{'rnStandardFormControl__title--inline' : $ctrl.inline}"
                      value="$ctrl.name" description="$ctrl.description"
                      is-read-only="$ctrl.isReadOnly" is-required="$ctrl.isRequired"
                      messages="$ctrl.validationMessages"></rn-control-title>

    <ng-transclude class="rnStandardFormControl__value"
                   ng-class="{'rnStandardFormControl__value--inline' : $ctrl.inline}"></ng-transclude>
</div>
`,
            transclude: true,
            controller: StandardFormControlController,
            require: {spInlineEditForm: '^^?spInlineEditForm'}
        });

    function StandardFormControlController($scope) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        this.$onInit = () => {

            const control = $ctrl.control;

            if (control.fieldToRender) {
                _.assign($ctrl, getFieldControlProps(control));
            } else if (control.relationshipToRender) {
                _.assign($ctrl, getRelationshipControlProps(control));
            }

            $ctrl.isReadOnly = $ctrl.isReadOnly || !$ctrl.editing;

            if ($ctrl.spInlineEditForm) {
                $scope.$watch('$ctrl.validationMessages.length', function () {
                    // Note - should generalise this somewhat, but at present adding in special
                    // for inline editing forms. And to make it somewhat more efficient etc.
                    $ctrl.spInlineEditForm.setValidationMessages($ctrl.control, $ctrl.validationMessages);
                });
            }
        };

        this.$onChanges = changes => {
            // console.log('$onChanges', _.keys(changes));
        };
    }

    function getFieldControlProps(control) {
        const fieldToRender = control.fieldToRender;

        const name = control.name || fieldToRenderName(fieldToRender);

        const description = control.description ||
            fieldToRender && fieldToRender.description;

        const isReadOnly = control.readOnlyControl ||
            fieldToRender && fieldToRender.isCalculatedField;

        const isRequired = control.mandatoryControl ||
            fieldToRender && fieldToRender.isRequired;

        return {name, description, isReadOnly, isRequired};
    }

    function getRelationshipControlProps(control) {
        const relToRender = control.relationshipToRender;

        const name = control.name || relToRenderName(control, relToRender);

        const description = control.description ||
            relToRender && relToRender.description;

        const isReadOnly = control.readOnlyControl;

        const isRequired = control.mandatoryControl;

        return {name, description, isReadOnly, isRequired};
    }

    function fieldToRenderName(fieldToRender) {
        return fieldToRender && fieldToRender.name;
    }

    function relToRenderName(control, relToRender) {
        if (!relToRender) return '';
        if (!control.isReversed && relToRender.toName) return relToRender.toName;
        if (control.isReversed && relToRender.fromName) return relToRender.fromName;
        return relToRender.name;
    }
}());