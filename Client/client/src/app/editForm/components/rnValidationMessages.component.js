// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.editFormComponents')
        .component('rnValidationMessages', {
            controller: ValidationMessagesController,
            bindings: { messages: '<' },
            template: `
<img class="rnValidationMessages" ng-show="$ctrl.validationMessage" 
    src="assets/images/validationMessage.png" alt="Error on field"
    uib-tooltip="{{$ctrl.validationMessage}}" tooltip-trigger="{{$ctrl.tooltipTrigger}}"
    tooltip-placement="{{$ctrl.tooltipPlacement}}">
`
        });

    /* @ngInject */
    function ValidationMessagesController($scope, spMobileContext) {

        this.tooltipTrigger = spMobileContext.isMobile ? 'click' : 'mouseenter';
        this.tooltipPlacement = spMobileContext.isMobile ? 'left' : 'top';

        // this.$onChanges = changes => {
        //     if (changes.messages) this.messages = changes.messages.currentValue;
        //
        //     this.validationMessage = this.messages.join(' - ');
        // };

        // TODO - have the caller set messages be a new array rather than mutating the existing
        // and then we wouldn't have to do a deep watch.
        $scope.$watch('$ctrl.messages', messages => {
            this.validationMessage = (messages || []).join(' - ');
        }, true);
    }

}());
