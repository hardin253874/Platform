// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spTextQuestion', spTextQuestion);
            
            
    function spTextQuestion() {
        return { 
            restrict: 'E',
            replace: true,
            scope: {
                question: '=',
                answer: '=',
                isReadOnly: '=?'
            },
            templateUrl: 'task/directives/spTextQuestion.tpl.html',
            link: function (scope, elem, attrs) {
                
            }
        };
    }
    

}());