// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.editFormComponents')
        .component('rnVerticalStackContainerControl', {
            bindings: {formControl: '=', formData: '=?'},
            template: `
<div class="rn-flex-column rn-flex-item" random-bgc>
    <div ng-if="!$ctrl.formControl.hideLabel">{{$ctrl.formControl.name || $ctrl.formControl.debugString}}</div>
    <div class="rn-flex-column rn-flex-item">
        <rn-form-control ng-repeat="c in $ctrl.controls"
                         form-control="c" form-data="$ctrl.formData"></rn-form-control>
    </div>
</div>`,
            controller: VerticalContainerController
        });

    /* @ngInject */
    function VerticalContainerController($scope, $element) {

        $scope.$watch('$ctrl.formControl', formControl => {
            // console.log('ContainerControlController $watch', $scope.$id, (formControl || {}).debugString);
            this.controls = _.sortBy((formControl || {}).containedControlsOnForm, 'renderingOrdinal');
        });

        $scope.$watch('$ctrl.formData', formData => {
            // console.log('ContainerControlController $watch formData', $scope.$id, formData);
        });

        $element.addClass('rn-flex-row');
        $element.addClass('rn-flex-item');
    }

}());
