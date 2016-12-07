// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing calculation editor.
    * 
    * @module spCalculatedFieldController
    * @example
        
    Using the spCalculatedField:
    
    spCalculatedFieldDialog.showModalDialog(options).then(function(result) {
    });
       
    where calculationOptions is available on the controller with the following properties:
         - selectedNode - {object} - the selected entitytype node to load all relationships for current entity type
           
    * 
    */
    angular.module('mod.common.ui.spCalculatedField', ['ui.bootstrap', 'mod.common.alerts', 'mod.common.ui.spEntityComboPicker', 'mod.common.spEntityService', 'sp.app.settings', 'sp.common.ui.expressionEditor', 'mod.common.spEntityService', 'sp.common.spCalcEngineService'])
    .controller("spCalculatedFieldController", function ($scope, $http, $uibModalInstance, $stateParams, options, spAppSettings,
                                                         spAlertsService, spEntityService, spCalcEngineService) {
        $scope.options = options || {};
        $scope.model = { selectedNode: null, context: null, host: 'Report', script: '',columnName: '' };
        $scope.message = '';
       
        $scope.$watch('options', function () {            
            $scope.model.context = options.entityTypeId ? options.entityTypeId : null;
            $scope.model.host = options.stateParams && options.stateParams.host ? options.stateParams.host : 'Report';
            $scope.model.columnName = options.columnName ? options.columnName : '';
            $scope.model.script = options.script ? options.script : '';
        });

        $scope.$watch('model.script', $scope.onCodeChanged);

        // click ok button to return calculated script
        $scope.ok = function () {
            var result;
            var code = $scope.model.script;
            var tracker = {};
            $scope.currentTracker = tracker;
            $scope.validateCode(code, tracker, function (data) {
                if (data.error) {
                    spAlertsService.addAlert('Script contains errors.', spAlertsService.sev.Error);
                } else {
                    //if the resultType is entity, need get following information to detect it is choicefield or inlinerelationship
                    if (data.resultType === 'Entity') {
                        var entityTypeId = data.entityTypeId;
                        var rq = 'name,alias,inherits.alias';
                        spEntityService.getEntity(entityTypeId, rq).then(function (type) {
                            var inh = type ? type.getInherits()[0] : null;
                            var isChoiceField = inh ? inh.eid().getNsAlias() === 'core:enumValue' : false;
                            var resultType = isChoiceField ? 'ChoiceRelationship' : 'InlineRelationship';
                            result = { script: code, resultType: resultType, columnName: $scope.model.columnName, entityTypeId: entityTypeId };
                            $uibModalInstance.close(result);

                        });

                    } else {
                        result = { script: code, resultType: data.resultType, columnName: $scope.model.columnName };
                        $uibModalInstance.close(result);
                    }
                }
            });

            
        };
        
        // click cancel to return report builder
        $scope.cancel = function () {
            $uibModalInstance.close(null);
        };
        

        // Handle code change by user
        $scope.onCodeChanged = function () {
            var tracker = {};
            $scope.currentTracker = tracker;
            setTimeout(function () {
                if (tracker === $scope.currentTracker) {
                    var code = $scope.model.script;
                    $scope.validateCode(code, tracker);
                }
            }, 500);
        };       

        // Validate code at server
        $scope.validateCode = function (script, tracker, callback) {

            if (script === '') {
                $scope.message = '';
               
                return;
            }

            var params = [];

            spCalcEngineService.compileExpression(script, $scope.model.context, $scope.model.host, params)
                .success(function (data) {
                    if (tracker === $scope.currentTracker) {
                        if (data.error) {
                            $scope.message = data.error;
                        }
                        else {
                            $scope.message = '';
                            $scope.resultType = data.resultType;
                        }
                        $scope.currentTracker = null;
                        callback(data);
                    }
                  
                })
                .error(function (data) {
                    $scope.message = 'Error contacting server.';
                });

        };

    })
    .factory('spCalculatedFieldDialog', function (spDialogService) {
        // setup the dialog
        var exports = {

            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Calculation Editor',
                    keyboard: true,
                    backdropClick: false,                    
                    windowClass: 'modal calculatedfielddialog-view',
                    templateUrl: 'reportBuilder/dialogs/CalculatedField/spCalculatedField.tpl.html',
                    controller: 'spCalculatedFieldController',
                    resolve: {
                        options: function () {
                            return options;
                        }
                    }
                };

                if (defaultOverrides) {
                    angular.extend(dialogDefaults, defaultOverrides);
                }

                return spDialogService.showModalDialog(dialogDefaults);
            }

        };

        return exports;
    });
}());