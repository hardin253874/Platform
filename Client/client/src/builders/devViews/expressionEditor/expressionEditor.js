// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spEntity */

(function () {
    'use strict';

    angular.module('app.expressionEditor', ['sp.common.ui.expressionEditor', 'mod.common.spEntityService', 'ui.bootstrap'])
        .config(function ($stateProvider) {
            $stateProvider.state('expressionEditor', {
                url: '/{tenant}/{eid}/expressionEditor?path',
                templateUrl: 'devViews/expressionEditor/expressionEditor.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.expressionEditor = { name: 'Expression Editor Test' };
        })
        .controller('expressionEditorDialogController', function ($scope, options, data, $uibModalInstance) {
            $scope.options = options;
            $scope.data = data;
        })
        .controller('expressionEditorController', function ($scope, spEntityService, $uibModal) {

            $scope.myTitle = 'hello world';

            $scope.data = {
                expressionText: '',
                expressionText2: '',
                params: [
                    { name: 'Default Name', typeName: 'String' }
                ],
                contextId: null
            };

            spEntityService.getEntity('test:employee', 'name,isOfType.name').then(function (entity) {
                $scope.data.contextId = entity.id();
            });

            spEntityService.getEntity('test:aaSteveGibbon', 'name,isOfType.id').then(function (entity) {
                $scope.data.params = $scope.data.params.concat([
                    { name: 'Resource', typeName: 'Entity', entityTypeId: entity.getType().id() }
                ]);
            });

            spEntityService.getEntity('test:aaScottHopwood', 'name,isOfType.id').then(function (entity) {
                $scope.data.params = $scope.data.params.concat([
                    { name: 'Manager Lookup.Value', typeName: 'Entity', entityTypeId: entity.getType().id() }
                ]);
            });

            $scope.showDialog = function () {

                var options = {
                    backdrop: true,
                    keyboard: true,
                    templateUrl: 'devViews/expressionEditor/expressionEditorDialog.tpl.html',
                    controller: 'expressionEditorDialogController',
                    resolve: {
                        data: function () {
                            return {
                                expressionText: $scope.data.expressionText,
                                contextId: null,
                                params: $scope.data.params,
                                mode: 'full',
                                intro: 'The expression context is test:employee so you can enter fields and relationships of employee. ' +
                                    'Try typing "Manager."  Or click on the "Resource" parameter (or type "[") and then a "." (dot)...  ' +
                                    'More to follow...'
                            };
                        },
                        options: function () {
                            return {
                                apply: function (result) {
                                    $scope.data.expressionText = result;
                                }
                            };
                        }
                    }
                };

                $uibModal.open(options).result.then(function (result) {
                    $scope.data.expressionText = result;
                });
            };

            $scope.showDialog2 = function () {

                var options = {
                    backdrop: true,
                    keyboard: true,
                    templateUrl: 'devViews/expressionEditor/expressionEditorDialog.tpl.html',
                    controller: 'expressionEditorDialogController',
                    resolve: {
                        data: function () {
                            return {
                                expressionText: $scope.data.expressionText2,
                                contextId: $scope.data.contextId,
                                params: [],
                                intro: 'The expression context is test:employee so you can enter fields and relationships of employee. ' +
                                    'Try typing "Manager." ' +
                                    'More to follow...'
                            };
                        },
                        options: function () {
                            return {
                                apply: function (result) {
                                    $scope.data.expressionText2 = result;
                                }
                            };
                        }
                    }
                };

                $uibModal.open(options).result.then(function (result) {
                    $scope.data.expressionText2 = result;
                });
            };

        });
}());
