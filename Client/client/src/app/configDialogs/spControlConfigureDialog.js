// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.app.configureDialog.Controller', ['ui.bootstrap','mod.common.ui.spDialogService'])
        .controller('controlConfigureDialogController', function ($scope,$uibModalInstance, options) {

            $scope.options = options;
            $scope.entityType = getEntityType();
            $scope.modalInstance = $uibModalInstance;
            function getEntityType() {
                if (options.relationshipType)
                    return options.relationshipType;
                else
                   return 'field';
                    
            }

        })
        .factory('controlConfigureDialogFactory', function (spDialogService) {
            // setup the dialog
            var exports = {
                createDialog: function (options) {
                    var styleClass = options.relationshipType ? 'modal sp-relationship-properties-modal' : 'modal sp-field-properties-modal';

                    var dialogOptions = {
                        templateUrl: 'configDialogs/controlConfigureDialog.tpl.html',
                        controller: 'controlConfigureDialogController',
                        //windowClass: 'modal sp-relationship-properties-modal',
                        windowClass: styleClass,
                        backdrop :'static',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            ////
            //Get the relationship type by control
            ////
            exports.getRelationshipType = function(relationshipControl) {
                var controlType = relationshipControl.type.getAlias();
                switch (controlType) {
                    case 'imageRelationshipRenderControl':
                        return 'image';
                    case 'multiChoiceRelationshipRenderControl':
                    case 'choiceRelationshipRenderControl':
                        return 'choice';
                    case 'inlineRelationshipRenderControl':
                    case 'dropDownRelationshipRenderControl':
                        return 'lookup';
                    case 'tabRelationshipRenderControl':
                        return 'relationship';
                    case 'verticalStackContainerControl':
                    case 'tabContainerControl':
                    case 'headerColumnContainerControl':
                    case 'chartRenderControl':
                    case 'reportRenderControl':
                    case 'formRenderControl':
                    case 'heroTextControl':
                        return 'container';
                    default:
                        return 'field';
                }
            };
            
            return exports;
        });
}());