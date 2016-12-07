// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular */
(function () {
    'use strict';

    /**
     * Module implementing a inline relationship picker.
     *
     * @module spInlineRelationshipPicker
     * @example

     Using the spInlineRelationshipPicker:

     */
    angular.module('app.editForm.spInlineRelationshipPicker', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.app.editForm', 'mod.common.spMobile'])
        .directive('spInlineRelationshipPicker', function (spMobileContext) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: spMobileContext.isMobile ? 'editForm/partials/spInlineRelationshipPickerMobile.tpl.html' : 'editForm/partials/spInlineRelationshipPicker.tpl.html',
                controller: 'spInlineRelationshipPickerController',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {

                }
            };
        });
}());