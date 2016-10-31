// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the symbol properties dialog.
    * 
    * @module spSymbolProperties
    * @example
        
    Using the spSymbolProperties:
    
    var model = { series: series }
    spSymbolProperties.showDialog(model).then(function(result) {
    });
       
    */
    angular.module('mod.app.chartBuilder.controllers.spSymbolProperties', ['ui.bootstrap', 'mod.common.alerts', 'mod.common.ui.spDialogService'])

     .controller("spSymbolPropertiesController", function ($scope, $uibModalInstance, model) {

         // model defines .series
         $scope.model = model;

         // Handle OK
         $scope.ok = function () {
             $uibModalInstance.close(true);
         };

         // Handle Cancel
         $scope.cancel = function () {
             $uibModalInstance.close(false);
         };
     })
    .service('spSymbolProperties', function (spDialogService) {

        // Show dialog, return promise
        this.showDialog = function (model) {
            var defaults = {
                templateUrl: 'chartBuilder/controllers/symbolProperties/spSymbolProperties.tpl.html',
                controller: 'spSymbolPropertiesController',
                resolve: {
                    model: function () { return model; }
                }
            };
            return spDialogService.showDialog(defaults);
        };

    });

}());