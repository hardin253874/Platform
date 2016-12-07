// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

// import _ from 'lodash';
// import './rnHorizontalStackContainerControl.scss';
// import template from './rnHorizontalStackContainerControl.html';
// import {getTransformedControls} from 'js/editForm/editForm';

(function () {


    angular.module('app.editFormComponents')
        .component('rnHorizontalStackContainerControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/layoutControls/rnHorizontalStackContainerControl.tpl.html',
            controller: HorizontalContainerController
        });

    /* @ngInject */
    function HorizontalContainerController($scope, rnEditFormDndService) {

        const $ctrl = this;

        this.childControlOptions = {};

        $scope.$watchCollection('$ctrl.control.containedControlsOnForm', () => {
            updateControls();
        });

        $scope.$watch(() => rnEditFormDndService.getTransform(), () => {
            updateControls();
        });

        $scope.$watch('$ctrl.resource', resource => {
            // console.log('ContainerControlController $watch resource', $scope.$id, resource);
        });

        function updateControls() {
            $ctrl.controls = rnEditForm.getTransformedControls($ctrl.control, rnEditFormDndService.getTransform());
            _.forEach($ctrl.controls, c => {
                c.cssClass = `rn-${c.control.renderingHorizontalResizeMode.aliasOnly}`;
                if (c.state) c.cssClass += ` ${c.state && 'rn-control-state-' + c.state || ''}`;
                if (c.state) c.cssClass += ` ${c.dragging && 'rn-control-dragging' || ''}`;
            });
        }
    }

}());