// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spQuestionNotes', spTextQuestion);
            
            
    function spTextQuestion(spDialogService) {
        return { 
            restrict: 'E',
            replace: true,
            scope: {
                notes: '=',
                readOnly: '='
            },
            templateUrl: 'task/directives/spQuestionNotes.tpl.html',
            link: function (scope, elem, attrs) {

                var dialogModel = {};

                scope.openNotes = openNotes;

                function modalInstanceCtrl(m_scope, $uibModalInstance, model) {
                    m_scope.model = model;
                    model.readOnly = scope.readOnly;

                    m_scope.closeNotes = function () {
                        $uibModalInstance.close();
                    };
                }

                function openNotes() {

                    scope.dialogModel = { notes: scope.notes };
                    

                    var defaults = {
                        templateUrl: 'task/directives/spQuestionNotesDialog.tpl.html',
                        controller: ['$scope', '$uibModalInstance', 'model', modalInstanceCtrl],
                        resolve: {
                            model: function () {
                                return scope.dialogModel;
                            }
                        }
                    };

                    var options = {
                    };

                    scope.$watch("dialogModel.notes", function (value) {
                        scope.notes = value;
                    });
                    
                    spDialogService.showDialog(defaults, options);
                }
            }
        };
    }
    

}());