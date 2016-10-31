// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

/**
 * @ngdoc module
 * @name editForm.service:spEditFormMobile
 *
 *  @description This service is by mobile edit forms
 */

(function () {
    'use strict';

    angular.module('mod.app.editFormMobileServices', ['mod.app.editFormServices']);

    angular.module('mod.app.editFormMobileServices')
        .factory('spEditFormMobile', spEditFormMobile);

    /* @ngInject */
    function spEditFormMobile(spEditForm) {

        return {
            pagifyForm: pagifyForm,
            dummyPagifyForm: dummyPagifyForm
        };

        /**
         * @ngdoc method
         * @name pagifyForm
         * @description Given a formControl split it into a number of 'pages' each page is a clone of the
         * form control with only the elements from each page.
         * @returns {Array.<*>} An array of formcontrols.
         */

        function pagifyForm(formControl) {

            var modifiedFormControl = spEditForm.simplifyForm(formControl);

            //spEditForm.dumpFormToConsole(formControl, controlToString);
            //spEditForm.dumpFormToConsole(modifiedFormControl, controlToString);

            var allControls = spEditForm.flattenControls(formControl);
            var pagedControls = getPagedControls(allControls);
            return [modifiedFormControl].concat(_.map(pagedControls, wrapInContainer));
        }

        function controlToString(c) {
            return [c.debugString, c.renderingOrdinal].join();
        }

        /**
         * @ngdoc method
         * @name dummyPagifyForm
         * @description Produce a pager compatible object with only one page in a vertical container. Used in Screen
         * @returns {Object[]} An array of formControls.
         */
        function dummyPagifyForm(formControl) {

            var json = {
                typeId: 'console:verticalStackContainerControl',
                description: 'specialContainerForMobile',
                'console:containedControlsOnForm': formControl.containedControlsOnForm
            };

            return [spEntity.fromJSON(json)];
        }

        function getPagedControls(formControls) {
            return _.filter(formControls, function (c) {
                return (sp.result(c, 'isOfType.0.pagerSupportMobile') === true);
            });
        }

        function wrapInContainer(control) {

            var isContainer = false;// control.firstTypeId().getAlias() === 'tabContainerControl';
            var containedControls = isContainer ? control.containedControlsOnForm : [control];
            var json = {
                typeId: 'console:verticalStackContainerControl',
                'console:containedControlsOnForm': containedControls
            };

            return spEntity.fromJSON(json);
        }
    }
}());