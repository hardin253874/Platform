// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {

    angular.module('app.editFormComponents')
        .component('rnDefaultFieldControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<div class="rn-field-control" random-bgc>
    <label class="rn-label">{{$ctrl.titleModel.name}}</label>
    <div class="rn-value">{{$ctrl.displayString}}</div>
</div>`,
            controller: DefaultFieldControlController
        });

    function DefaultFieldControlController($scope, spEditForm) {
        'ngInject';

        const $ctrl = this; // match var used in templates

        console.assert($ctrl.control);
        $ctrl.options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

        $scope.$watch('$ctrl.control', (control) => {
            //console.log('rnDefaultFieldControl $watch', $scope.$id, (control || {}).debugString);
            updateModel();
        });

        $scope.$watch('$ctrl.resource', (resource) => {
            //console.log('rnDefaultFieldControl $watch resource', $scope.$id, (resource || {}).debugString);
            updateModel();
        });

        function updateModel() {
            // console.assert($ctrl.control);

            const fieldToRender = $ctrl.control.fieldToRender;

            if (fieldToRender) {
                $ctrl.fieldLabel = fieldToRender.name;
                if ($ctrl.resource) {
                    $ctrl.fieldValue = $ctrl.resource.getField(fieldToRender.eidP);
                }
            }

            $ctrl.fieldValue = $ctrl.fieldValue || $ctrl.control.typesP[0].nsAlias;

            $ctrl.titleModel = getTitleModel($ctrl.control);
            $ctrl.displayString = $ctrl.fieldValue || '';
        }

        function getTitleModel(control) {
            // var fieldToRender = control.getLookup('console:fieldToRender');
            // var relToRender = control.getLookup('console:relationshipToRender');
            var fieldToRender = control.fieldToRender;
            var relToRender = control.relationshipToRender;

            return {
                control,
                fieldToRender,
                relToRender,
                readonly: true,
                name: control.name || fieldToRenderName() || relToRenderName()
            };

            function fieldToRenderName() {
                return fieldToRender && fieldToRender.name;
            }

            function relToRenderName() {
                if (!relToRender) return '';
                if (!control.isReversed && relToRender.toName) return relToRender.toName;
                if (control.isReversed && relToRender.fromName) return relToRender.fromName;
                return relToRender.name;
            }
        }
    }

})();