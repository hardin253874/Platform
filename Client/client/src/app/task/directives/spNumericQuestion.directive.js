// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spNumericQuestion', spNumberQuestion);
            
    function spNumberQuestion() {
        return { 
            restrict: 'E',
            replace: true,
            scope: {
                question: '=',
                answer: '=',
                isReadOnly: '=?'
            },
            templateUrl: 'task/directives/spNumericQuestion.tpl.html',
            link: function (scope, elem, attrs) {
                //
                // NOTE: If you are trying to fix the bug where you try to type in "-1" but it won't let you type the minus, that's because the 
                // default parser for the input[number] in angular turns the contents into a number. You can't turn "-" into a number so it throws it away.
                // 
                scope.$watch('answer.surveyAnswerNumber', function (value, oldValue) {
                    if (value && value !== '-') {
                        if (scope.question.numericQuestionIsInteger) {
                            scope.answer.surveyAnswerNumber = Math.round(value);
                        }
                    }
                });
            }
        };
    }

}());