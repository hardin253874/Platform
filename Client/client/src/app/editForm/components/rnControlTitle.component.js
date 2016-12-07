// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.editFormComponents')
        .component('rnControlTitle', {
            controller: ControlTitleController,
            bindings: {
                value: '<', description: '<',
                isReadOnly: '<', isRequired: '<', messages: '<'
            },
            template: `
<div class="rnControlTitle">
    <rn-mandatory-indicator class="rnControlTitle__mandatory" ng-if="$ctrl.isRequired && !$ctrl.isReadOnly"></rn-mandatory-indicator>
    <div class="rnControlTitle__label" title="{{$ctrl.tooltips}}">{{$ctrl.value}}</div>
    <rn-validation-messages class="rnControlTitle__messages" messages="$ctrl.messages"></rn-validation-messages>
</div>`
        });

    /* @ngInject */
    function ControlTitleController($scope) {

        this.$onChanges = changes => {
            if (changes.value) this.value = changes.value.currentValue;
            if (changes.description) this.description = changes.description.currentValue;

            this.tooltips = this.value && this.description ?
                `${this.value}: ${this.description}` : `${this.value || this.description}`;
        };

    }

}());
