// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnImageRelationshipRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/formControls/rnImageRelationshipRenderControl.tpl.html',
            controller: ImageRelationshipRenderControlController
        });

    function ImageRelationshipRenderControlController($scope, spFieldControlProvider) {
        'ngInject';

        const $ctrl = this; // match var used in templates

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

                const fieldToRender = $ctrl.control.fieldToRender;
                const resource = $ctrl.resource;

                if (!fieldToRender || !resource) return null;

                resource.setField(fieldToRender.eidP, value, spEntityUtils.dataTypeForField(fieldToRender));
            });
        };

        this.$onChanges = changes => {
            // console.log('ImageRelationshipRenderControlController', _.keys(changes));
            updateModel();
        };

        function updateModel() {

            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.model = $ctrl.model || {};

            // image template is using the existing controller and needs formMode
            $ctrl.formMode = options.editing ? 'edit' : 'view';

            // spFieldControlProvider($ctrl);
        }
    }
}());