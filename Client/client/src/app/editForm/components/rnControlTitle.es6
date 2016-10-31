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
<rn-mandatory-indicator ng-if="$ctrl.isRequired && !$ctrl.isReadOnly"></rn-mandatory-indicator>
<div title="{{$ctrl.tooltips}}">{{$ctrl.value}}</div>
<sp-custom-validation-message messages="$ctrl.messages"></sp-custom-validation-message>
`
        });

    /* @ngInject */
    function ControlTitleController($scope, $element) {

        this.$onChanges = changes => {
            if (changes.value) this.value = changes.value.currentValue;
            if (changes.description) this.description = changes.description.currentValue;

            this.tooltips = this.value && this.description ?
                `${this.value}: ${this.description}` : `${this.value || this.description}`;
        };

    }

}());
