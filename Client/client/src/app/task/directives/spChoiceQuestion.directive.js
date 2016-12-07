// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spChoiceQuestion', spChoiceQuestion);
            
            
    function spChoiceQuestion() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                question: '=',
                answer: '=',
                isReadOnly: '=?'
            },
            templateUrl: 'task/directives/spChoiceQuestion.tpl.html',
            link: function (scope, elem, attrs) {

                scope.radioPickerOptions = {
                    disabled: scope.isReadOnly || false,
                    orderField: 'choiceOptionOrder'
                };
                scope.dropDownOptions = {};
                
                scope.$watchCollection('question.choiceQuestionChoiceSet.choiceOptionSetChoices', addChoice);
                
                scope.$watchCollection('answer.surveyAnswerSingleChoice', function (newValue) {
                    if (newValue && newValue !== scope.dropDownOptions.selected) {
                        scope.dropDownOptions.selected = newValue;
                        scope.radioPickerOptions.selectedEntityId = newValue.idP;
                    }
                });

                scope.$watch('dropDownOptions.selected', function (newValue, oldValue) {
                    if ((scope.choices.length >= 5) && (oldValue !== newValue) && scope.answer) {
                        scope.answer.surveyAnswerSingleChoice = newValue;
                    }
                    scope.$emit('rnSurveyProgressEvent');
                });

                scope.$watch('radioPickerOptions.selectedEntityId', function (newValue, oldValue) {
                    if ((scope.choices.length < 5) && (oldValue !== newValue) && scope.answer) {
                        scope.answer.surveyAnswerSingleChoice = _.find(scope.choices, function (c) { return c.idP === newValue; });
                    }
                    scope.$emit('rnSurveyProgressEvent');
                });

                function addChoice(value) {
                    if (value) {
                        scope.choices = value;
                        scope.style = scope.choices.length < 5 ? 'radio' : 'dropdown';

                        if (scope.style === 'radio') {
                            scope.radioPickerOptions.entities = value;
                            //scope.radioPickerOptions.selectedEntityId = scope.answer.surveyAnswerSingleChoice;
                        } else {
                            if (scope.answer) {
                                scope.dropDownOptions.selected = scope.answer.surveyAnswerSingleChoice;
                            }
                        }
                    }
                }
            }
        };
    }
    

}());