// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {


    angular.module('app.editFormComponents')
        .component('rnVerticalStackContainerControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/layoutControls/rnVerticalStackContainerControl.tpl.html',
            controller: VerticalContainerController
        });

    function VerticalContainerController($scope, $element, rnEditFormDndService) {
        'ngInject';

        const $ctrl = this;

        this.childControlOptions = {};

        $element.addClass('rnVerticalStackContainerControl');

        $scope.$watchCollection('$ctrl.control.containedControlsOnForm', () => {
            updateControls();
        });

        $scope.$watch(() => rnEditFormDndService.getTransform(), () => {
            updateControls();
        });

        $scope.$watch('$ctrl.resource', resource => { // eslint-disable-line no-unused-vars
            //console.log('ContainerControlController $watch resource', $scope.$id, resource);
        });

        this.dragOptions = {
            onDragStart(e, d) {
                // console.log('onDragStart event', e);
                // console.log('onDragStart data', d);
            },
        };

        function updateControls() {
            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.controls = rnEditForm.getTransformedControls($ctrl.control, rnEditFormDndService.getTransform());
            _.forEach($ctrl.controls, c => {
                c.cssClass = `rn-${c.control.renderingVerticalResizeMode.aliasOnly}`;
                if (c.state) c.cssClass += ` ${c.state && 'rn-control-state-' + c.state || ''}`;
                if (c.state) c.cssClass += ` ${c.dragging && 'rn-control-dragging' || ''}`;
            });

            $ctrl.hideContainerLabel = options.noTitle || $ctrl.control.hideLabel;
        }
    }

})();