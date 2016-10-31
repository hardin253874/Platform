// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spQuestion', spQuestion);
            
            
    function spQuestion() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                question: '=',
                answer: '=',
                isReadOnly: '=?'
            },
            templateUrl: 'task/directives/spQuestion.tpl.html',
            link: link
        };
    }

    function link(scope, elem, attrs) {
        var isMultiDeregister;

        scope.$watch('question', function (value) {
            if (isMultiDeregister) {
                isMultiDeregister();
            }
            if (scope.question) {
                var type = sp.result(scope.question.type, 'nsAlias') || sp.result(scope.question.isOfType, '0.nsAlias');
                switch (type) {
                    case 'core:numericQuestion':
                        scope.questionType = 'numeric';
                        break;
                    case 'core:textQuestion':
                        scope.questionType = 'text';
                        break;
                    case 'core:choiceQuestion':
                        scope.questionType = scope.question.choiceQuestionIsMulti ? 'multichoice' : 'singlechoice';
                        isMultiDeregister = scope.$watch('question.choiceQuestionIsMulti', isMultiWatcher);
                        break;
                }
            }
        });

        function isMultiWatcher(value) {
            scope.questionType = scope.question.choiceQuestionIsMulti ? 'multichoice' : 'singlechoice';
        }
    }
    

}());