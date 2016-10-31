// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spMultiChoiceRelationshipRenderControl', ['mod.common.spInclude', 'mod.common.spCachingCompile'])
        .directive('spMultiChoiceRelationshipRenderControl', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=?',
                    parentControl: '=?',
                    formData: '=?',
                    formMode: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?',
                    isInDesign: '=?',
                    isInlineEditing: '=?'
                },
                link: function ($scope, $element) {
					// Configure the underlying picker
					// to fetch data using a report rather than the entity info service.
					// This is needed so that the results can be filtered.
                    $scope.useReportToPopulatePicker = true;
                    $scope.pickerReportServiceOptions = {
                        reportId: 'console:enumValuesReport',
                        metadata: 'colbasic',
                        relfilters: {},
                        entityTypeId: 0
                    };                                                          

                    $scope.$watch('formControl.readOnlyControl || isReadOnly', function (value) {
                        $scope.readOnly = value && !$scope.isInDesign;
                    });

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, $element);
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spMultiChoiceRelationshipRenderControl/spMultiChoiceRelationshipRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        $element.append(clone);
                    });
                }
            };
        });
}());