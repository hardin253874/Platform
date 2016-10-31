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
    angular.module('app.editForm.spInlineRelationshipPicker', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.app.editForm'])
        .directive('spInlineRelationshipPicker', function () {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'editForm/partials/spInlineRelationshipPicker.tpl.html',
                controller: 'spInlineRelationshipPickerController',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {

                }
            };
        });
}());