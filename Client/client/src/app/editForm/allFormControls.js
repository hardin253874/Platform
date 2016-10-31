// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function() {
    'use	strict';

    angular.module('mod.app.editForm.allFormControls', ['ui.router', 'ui.bootstrap', 'titleService', 'mod.common.spEntityService', 'ngGrid', 'mod.app.editFormServices',
                   'mod.app.editForm.designerDirectives', 'mod.app.editForm.designerDirectives', 'mod.common.editForm.editFormDirectives'])

        .config(function($stateProvider){
            $stateProvider.state('allFormControls', {
                url: '/{tenant}/allFormControls',
                controller: 'allFormControlsController',
                templateUrl: 'editForm/allFormControls.tpl.html'
            });
        })
        .controller('allFormControlsController', function allFormControlsController($scope, $stateParams, titleService, spEntityService, $q, $timeout, spEditForm) {

            $scope.controlList = [];
            spEntityService.getEntity('console:controlOnForm', 'core:derivedTypes*.{name, description}').then(
                function(controlOnForm) {
                    
                    $scope.controlList = flattenInheritance(controlOnForm);
                   
                },
                function(error) {
                    window.alert(error);
                });

        });

    function flattenInheritance(entity) {
        var result = [entity];
        var derivedTypes = entity.getDerivedTypes() || [];

        derivedTypes.forEach(function(e) {
            result = result.concat(flattenInheritance(e));
        });
        
        return result;
    }

   
}());