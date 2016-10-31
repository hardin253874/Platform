// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spChoiceRelationshipRenderControl', ['mod.common.spInclude', 'mod.common.spCachingCompile'])
        .directive('spChoiceRelationshipRenderControl', function (spCachingCompile) {

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
                require: '^^?spInlineEditForm',
                link: function (scope, $element, attrs, spInlineEditForm) {

					// Configure the underlying picker
					// to fetch data using a report rather than the entity info service.
					// This is needed so that the results can be filtered.
                    scope.useReportToPopulatePicker = true;
                    scope.pickerReportServiceOptions = {
                        reportId: 'console:enumValuesReport',
                        metadata: 'colbasic',
                        relfilters: {},
                        entityTypeId: 0
                    };                   

                    function updateReadonly() {
                        if (scope.formControl) {
                            scope.isReadOnly = (scope.formControl.readOnlyControl || scope.isReadOnly) && !scope.isInDesign;
                        }
                    }

                    scope.$watch('formControl', updateReadonly);
                    scope.$watch('isReadOnly', updateReadonly);

                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, $element);
                    });

                    if (spInlineEditForm) {
                        scope.$watch('customValidationMessages.length', function () {
                            // Note - should generalise this somewhat, but at present adding in special
                            // for inline editing forms. And to make it somewhat more efficient etc.
                            spInlineEditForm.setValidationMessages(scope.formControl, scope.customValidationMessages);
                        });

                        // There is a mix of controllers/templates and directives so to get
                        // to the single picker based control validation we are doign this for the moment.
                        scope.$on('validationMessages', function (event, data) {
                            var formControl = data.formControl;
                            var messages = data.messages;
                            if (formControl === scope.formControl) {
                                spInlineEditForm.setValidationMessages(formControl, messages);
                            }
                        });
                    }

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spChoiceRelationshipRenderControl/spChoiceRelationshipRenderControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        $element.append(clone);
                    });
                }
            };
        });
}());