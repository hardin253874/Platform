// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function() {
    'use	strict';

    angular.module('app.formBuilderTestPage', ['mod.app.formBuilder.services.spFormBuilderService', 'mod.common.spEntityService'])
        .config(function($stateProvider) {
            $stateProvider.state('formBuilderTestPage', {
                url: '/{tenant}/{eid}/formBuilderTestPage?path',
                templateUrl: 'devViews/formBuilder/formBuilderTestPage.tpl.html'
            });

            // Obviously for testing purposes only...
            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.formBuilderTestPage = { name: 'Form Builder Test Page' };
            window.testNavItems.screenBuilder = { name: 'Screen Builder Test Page' };
        })
        .controller('formBuilderTestPageController', function ($scope, spNavService, spFormBuilderService, spEntityService, spState) {

            spState.scope.formBuilderTestState = spState.scope.formBuilderTestState || {};

            if (!spState.scope.formBuilderTestState.selectedIndex && spState.scope.formBuilderTestState.selectedIndex !== 0) {
                spState.scope.formBuilderTestState.selectedIndex = 0;
            }

            $scope.onOptionChanged = function() {
                
                spState.scope.formBuilderTestState.selectedIndex = $scope.model.selectedOption.ordinal;
            };

            //---- Form Builder Test Page
            $scope.model = {
                options: [
                    {
                        name: 'Create a new definition',
                        ordinal: 0,
                        url: ''
                    },
                    {
                        name: 'Create a new screen',
                        ordinal: 1,
                        url: ''
                    },
                    {
                        name: 'Create a new form for an existing definition',
                        ordinal: 2,
                        url: ''
                    },
                    {
                        name: 'Edit an existing form',
                        ordinal: 3,
                        url: ''
                    },
                    {
                        name: 'Edit an existing screen',
                        ordinal: 4,
                        url: ''
                    }
                ]
            };

            $scope.model.selectedOption = $scope.model.options[spState.scope.formBuilderTestState.selectedIndex || 0];
            //---- End Form Builder Test Page


            //---- Create new Definition
            $scope.createNewDefinitionModel = spState.scope.formBuilderTestState.createNewDefinitionModel || {
                name: 'Test Definition',
                description: '',
                isAbstract: false
            };

            spState.scope.formBuilderTestState.createNewDefinitionModel = $scope.createNewDefinitionModel;

            $scope.createNewDefinition = function () {

                $scope.createNewDefinitionModel.errorMessage = undefined;

                var name = $scope.createNewDefinitionModel.name.toLowerCase();

                if (_.some($scope.existingDefinitionsModel.definitions, function(defn) {
                    return defn.getName().toLowerCase() === name;
                })) {
                    $scope.createNewDefinitionModel.errorMessage = 'Definition with that name already exists';
                } else {

                    var definition = spFormBuilderService.createDefinition($scope.createNewDefinitionModel.name, $scope.createNewDefinitionModel.description);
                    spFormBuilderService.createForm(definition, $scope.createNewDefinitionModel.name + ' Form');

                    var data = {
                        state: spFormBuilderService.getState()
                    };

                    spNavService.navigateToChildState('formBuilder', 0, undefined, data);
                }
            };
            //---- End Create new Definition


            //---- Create new Screen
            $scope.createNewScreenModel = spState.scope.formBuilderTestState.createNewScreenModel || {
                name: 'Test Screen',
                description: ''
            };

            spState.scope.formBuilderTestState.createNewScreenModel = $scope.createNewScreenModel;

            $scope.createNewScreen = function () {

                spFormBuilderService.createScreen($scope.createNewScreenModel.name || 'Screen');

                var data = {
                    state: spFormBuilderService.getState()
                };

                spNavService.navigateToChildState('screenBuilder', 0, undefined, data);
            };
            //---- End Create new Screen


            //---- Create new form for existing definition
            $scope.existingDefinitionsModel = {};
            $scope.existingFormsModel = {};

            $scope.getDefinitionList = function () {

                return spEntityService.getEntity('core:definition', 'instancesOfType.{name,id,isOfType.id}').then(function (requestResult) {
                    return requestResult.getInstancesOfType();
                });
            };

            $scope.createNewFormForDefinition = function () {

                spFormBuilderService.createForm($scope.existingDefinitionsModel.selectedDefinition, $scope.existingDefinitionsModel.name, $scope.existingDefinitionsModel.description);

                var data = {
                    state: spFormBuilderService.getState()
                };

                spNavService.navigateToChildState('formBuilder', 0, undefined, data);
            };

            $scope.getDefinitionList().then(
                function (data) {
                    $scope.existingDefinitionsModel.definitions = data;
                    $scope.existingDefinitionsModel.selectedDefinition = _.first(_.sortBy($scope.existingDefinitionsModel.definitions, function (item) { return item.getName(); }));
                    $scope.existingDefinitionsModel.name = 'New ' + $scope.existingDefinitionsModel.selectedDefinition.getName() + ' form';

                    $scope.existingFormsModel.definitions = data;
                    $scope.existingFormsModel.selectedDefinition = $scope.existingDefinitionsModel.selectedDefinition;
                },
                function (error) { $scope.error = error; });
            //---- End Create new form for existing definition


            //---- Edit existing form
            $scope.getFormsList = function () {

                return spEntityService.getEntity('console:customEditForm', 'instancesOfType.{name,id,console:typeToEditWithForm.{name,id, inherits*.{name,id}}}').then(function (requestResult) {
                    return requestResult.getInstancesOfType();
                });
            };

            $scope.editExistingForm = function () {

                spNavService.navigateToChildState('formBuilder', $scope.existingFormsModel.selectedForm.id());
            };

            $scope.getName = function(form) {
                return form.getName() || '<unnamed>';
            };

            $scope.getFormsList().then(
                function (data) {

                    $scope.existingFormsModel.forms = data;
                },
                function (error) { $scope.error = error; });

            $scope.filterByDefinition = function (item) {

                var typeToEditWithForm = item.getRelationship('console:typeToEditWithForm');

                var typeMap = _.map(typeToEditWithForm, function(entity) {
                    return entity.id();
                });

                var hierarchy = _.map(_.flatten(_.invokeMap(typeToEditWithForm, 'getRelationship', 'inherits')), function (entity) {
                    return entity.id();
                });

                var result = _.includes(hierarchy, $scope.existingFormsModel.selectedDefinition.id()) || _.includes(typeMap, $scope.existingFormsModel.selectedDefinition.id());

                if (result && !$scope.existingFormsModel.selectedForm) {
                    $scope.existingFormsModel.selectedForm = item;
                }

                return result;
            };
            //---- End Edit existing form


            //---- Edit existing screen
            $scope.existingScreensModel = {};

            $scope.getScreensList = function () {

                return spEntityService.getEntitiesOfType('console:screen', 'name').then(function (screens) {
                    $scope.existingScreensModel.screens = screens;
                });
            };

            $scope.editExistingScreen = function () {
                var screenId = $scope.existingScreensModel.selectedScreen.id();
                spNavService.navigateToChildState('screenBuilder', screenId);
            };

            $scope.getScreensList();
            //---- End Edit existing screen
        });

}());