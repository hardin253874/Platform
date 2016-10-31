// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a button which displays the analyzer in a popup when clicked.            
    * spAnalyzerButton displays the analyzer in a popup when clicked  
    *
    * @module spAnalyzerButton    
    * @example           
    
    Using the spAnalyzerButton:

    &lt;div sp-analyzer-button="options"&gt;&lt;/div&gt      

    where options is available on the controller with the following properties:        
        - analyzerOptions - {object}. See the options of the sp-analyzer directive. 
    */
    angular.module('mod.common.ui.spAnalyzerButton', ['mod.common.ui.spAnalyzerPopup'])
        .directive('spAnalyzerButton', function () {
            return {
                restrict: 'E',
                templateUrl: 'analyzer/spAnalyzerButton.tpl.html',
                replace: true,
                transclude: false,
                scope: {
                    options: '=',
                    onButtonClicked: '&'
                },
                link: function (scope, iElement, iAttrs) {                   

                    scope.model = {
                        areAnalyzerFiltersActive: false,
                        popupOptions: {
                            isOpen: false,
                            analyzerOptions: scope.options
                        },
                        isInDesign: !!scope.options.isInDesign
                    };


                    scope.$watch('options.analyzerFields', function () {                        
                        updateAnalyzerFiltersActiveState();
                    });


                    scope.$watch('options', function (options) {
                        scope.model.popupOptions.analyzerOptions = options;
                        updateAnalyzerFiltersActiveState();
                    });
                   

                    scope.$on('spAnalyzerEventApplyConditions', function () {
                        updateAnalyzerFiltersActiveState();
                    });


                    // Handle drop down button click events
                    scope.analyzerButtonClick = function () {
                        var isOpen = !scope.model.popupOptions.isOpen;
                        
                        if (scope.onButtonClicked) {
                            scope.onButtonClicked({ isOpen: isOpen });
                        }                        

                        scope.model.popupOptions.isOpen = isOpen;                        
                    };


                    function updateAnalyzerFiltersActiveState() {
                        var analyzerFields;

                        if (scope.model.popupOptions.analyzerOptions) {
                            analyzerFields = scope.model.popupOptions.analyzerOptions.analyzerFields;
                        }

                        if (!analyzerFields) {
                            scope.model.areAnalyzerFiltersActive = false;
                        } else {
                            scope.model.areAnalyzerFiltersActive = _.some(analyzerFields, function (af) {
                                var operator;

                                if (af.operator) {
                                    operator = _.find(af.operators, function (op) {
                                        return op.id === af.operator;
                                    });
                                }

                                if (operator &&
                                    operator.id !== 'Unspecified') {
                                    if (operator.argCount > 0) {
                                        // Check that an argument is specified
                                        return !_.isNull(af.value) && !_.isUndefined(af.value);
                                    } else {
                                        return true;
                                    }
                                } else {
                                    return false;
                                }
                            });
                        }
                    }
                }
            };
        });
}());