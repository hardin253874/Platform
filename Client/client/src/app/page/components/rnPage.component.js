// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('mod.app.page')
        .component('rnPage', {
            controller: Controller,
            bindings: {},
            template: `
            <div>{{$ctrl.state}}</div>
`
        });

    function Controller(spState, spEditForm) {
        'ngInject';

        const $ctrl = this; // assign to match the implicit $ctrl scope var used in the template

        this.$onInit = onInit;

        function onInit() {
            if (!spState.navItem) {
                console.error('unexpected missing navItem');
                return;
            }
            initNavItem();
            initState();
        }

        function initState() {

            $ctrl.state = spState.getPageState();

            if (!$ctrl.state.isInitialised) {

                // Do first time initialisation.

                $ctrl.state.isInitialised = true;
                $ctrl.state.pageTitle = 'this is the page title';
                $ctrl.state.eid = sp.coerseToNumberOrLeaveAlone(spState.params.eid);
                $ctrl.state.formId = sp.coerseToNumberOrLeaveAlone(spState.params.formId);

            } else {

                // Do any 'after nav return' initialisation.

                $ctrl.state.pageTitle = 'this is the page title (returned from nav)';
            }
        }

        function initNavItem() {
            spState.navItem.isDirty = isDirty;
        }

        function isDirty() {
            return false;
        }
    }

}());
