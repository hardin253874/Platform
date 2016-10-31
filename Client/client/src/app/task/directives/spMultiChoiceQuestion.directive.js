// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spMultiChoiceQuestion', spMultiChoiceQuestion);
            
            
    function spMultiChoiceQuestion() {
        return {
            restrict: 'E',
            replace: true,
            scope: {
                question: '=',
                answer: '=',
                isReadOnly: '=?'
            },
            templateUrl: 'task/directives/spMultiChoiceQuestion.tpl.html',
            link: function (scope, elem, attrs) {

                scope.choices = [];
                scope.updateSelected = updateSelected;

                scope.$watchCollection('question.choiceQuestionChoiceSet.choiceOptionSetChoices', addChoice);
                
                function addChoice(value) {
                    if (value) {
                        var selectedEntities = sp.result(scope.answer, 'surveyAnswerMultiChoice');
                        scope.choices = createChoices(value, selectedEntities);
                    }
                }

                function createChoices(entities, selectedEntities) {
                    var selectedIds = [];
                    if (selectedEntities && selectedEntities.length) {
                        selectedIds = _.map(selectedEntities, 'idP');
                    }
                    return _.map(entities, function (e) {
                        return {
                            name: e.name,
                            entity: e,
                            selected: _.includes(selectedIds, e.idP)
                        };
                    });
                }

                function createAnswers(choices) {
                    return _.filter(choices, { selected: true });
                }

                function updateSelected(choice) {
                    if (choice.selected) {
                        scope.answer.surveyAnswerMultiChoice.add(choice.entity);
                    } else {
                        scope.answer.surveyAnswerMultiChoice.remove(choice.entity);
                    }
                }
            }
        };
    }
    

}());