// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function() {
    'use	strict';

    angular.module('mod.app.editFormTestPage', ['ui.router', 'ui.bootstrap', 'titleService', 'mod.common.spEntityService', 'ngGrid', 'mod.app.editFormServices', 'mod.app.editForm.designerDirectives'])
        .config(function($stateProvider){
            $stateProvider.state('editFormTestPage', {
                url: '/{tenant}/{eid}/editFormTestPage?path&formId',
                templateUrl: 'editForm/editFormTestPage.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.editFormTestPage = { name: 'Edit Form Test Page' };

        })
        .controller('editFormTestPageController', function editFormTestPageController($scope, $stateParams, $location, titleService, spEntityService, $q, $timeout, spEditForm, spNavService) {

            titleService.setTitle('Edit');

            $scope.model = {
                id: $stateParams.eid || 0,
                formId: $stateParams.formId || 0
            };

            var getInstancesRequest = 'instancesOfType.{name,id}';
            var resourcesForFormRequest = 'console:typeToEditWithForm.instancesOfType.{name,id}';

//	refresh	the	form	data
            $scope.refresh = function() {
                console.log("$scope.refresh", "starting");
                if ($scope.selectedResource) {
                    $scope.error = '';
                    $scope.getFormData($scope.selectedResource).then(
                        function(data) {
                            console.log("$scope.refresh:", data);
                            $scope.formData = data;
                        },
                        function(error) {
                            console.error("$scope.refresh:	Error:", error);
                            $scope.error = error;
                        });
                }
            };

            //	Save	the	updates	to	the	resource
            $scope.save = function() {
                spEntityService.putEntity($scope.formData);
            };

            //	Save	the	form	updates
            $scope.saveForm = function() {
                console.log('$scope.saveForm:	', $scope.formControl);
                spEntityService.putEntity($scope.formControl);
            };

            /*	populateFormsList	*/
            $scope.getFormsList = function() {

                return spEntityService.getEntity('console:customEditForm', getInstancesRequest).then(function(requestResult) {
                    return requestResult.getInstancesOfType();
                });
            };
            
            /*	populateDefinitonsList	*/
            $scope.getDefinitionList = function () {

                return spEntityService.getEntity('core:definition', getInstancesRequest).then(function (requestResult) {
                    return requestResult.getInstancesOfType();
                });
            };

            /*	getFormControl	*/
            $scope.getFormControl = function(selectedForm) {

                return spEditForm.getFormDefinition(selectedForm.id()).then(function (data) {
                    console.log("$scope.getFormControl:", data);
                    return data;
                });
            };

            /*	populateResourcesList	*/
            $scope.getResourcesList = function(selectedForm) {
                return spEntityService.getEntity(selectedForm.id(), resourcesForFormRequest).then(function(data) {

                    console.log("$scope.getResourcesList:	", data);
                    var type = data.getTypeToEditWithForm();
                    $scope.selectedType = type;
                    //console.log("types:	",	types);
                    return type && type.getInstancesOfType();

                });

            };
            
            /*	populateDefResourcest	*/
            $scope.getDefResources = function (selectedDef) {
                return spEntityService.getEntity(selectedDef.id(), getInstancesRequest).then(function (data) {
                    $scope.defResources = data.getInstancesOfType();
                });

            };

            
            $scope.openModal = function () {
                $scope.modalInfo.formData = $scope.formData;
                $scope.modalInfo.formControl = $scope.formControl;
                $scope.modalInfo.isOpen=true;
            };
            
            $scope.closeModal = function () {
                $scope.formData = $scope.modalInfo.formData;
                
                $scope.modalInfo.isOpen = false;
                $scope.modalInfo.formControl = null;
                $scope.modalInfo.formData = null;
            };
            
            $scope.navigateToPage = function () {
                var params = {};
                if ($scope.selectedForm) {
                    params.formId = $scope.selectedForm.id();

                    spNavService.navigateToChildState('viewForm', $scope.selectedResource.id(), params);
                }

                //$scope.redirectPath = 'sddssd';
            };
            

            $scope.navigateToDefaultEditPage = function (forceGenerate) {
                var params = {};
                if ($scope.selectedDefResource) {
                    params.forceGenerate = forceGenerate;
                    spNavService.navigateToChildState('viewForm', $scope.selectedDefResource.id(), params);
                }

                //$scope.redirectPath = 'sddssd';
            };
            
            $scope.navigateToCreatePage = function () {
                var params = {};
                if ($scope.selectedForm) {
                    params.formId = $scope.selectedForm.id();
                    spNavService.navigateToChildState('createForm', $scope.selectedType.id(), params);
                }
            };
            
            $scope.navigateToDefaultCreatePage = function (forceGenerate) {
                var params = {};
                if ($scope.selectedDefinition) {
                    params.forceGenerate = forceGenerate;
                    spNavService.navigateToChildState('createForm', $scope.selectedDefinition.id(), params);
                }
            };
            
            $scope.navigateToDesigner = function () {
                var params;
                if ($scope.selectedForm) {
                    spNavService.navigateToChildState('editFormDesigner', $scope.selectedForm.id(), params);
                }
                //$scope.redirectPath = 'sddssd';
            };


            //	update	the	resources	list	when	the	form	changes	
            $scope.$watch('selectedForm', function() {

                if ($scope.selectedForm) {
                    $scope.isLoadingForm = true;
                    $scope.error = '';

                    var formControl = $scope.getFormControl($scope.selectedForm);
                    var formResources = $scope.getResourcesList($scope.selectedForm);

                    if (formResources) {
                        $q.all([formControl, formResources]).then(
                            function(resolvedPromises) {
                                $scope.formControl = resolvedPromises[0];
                                $scope.formResources = resolvedPromises[1];

                                if ($scope.formResources && $scope.formResources.length > 0) {
                                    $scope.selectedResource = $scope.formResources[0];
                                } else {
                                    $scope.selectedResource = null;
                                    $scope.formData = null;
                                }
                            },
                            function(error) {
                                $scope.error = error.data;
                                //$scope.isLoadingForm	=	false;
                            });
                    }
                }
            });

            //	update	the	form	data	when	the	selected	resource	changes.	(But	not	when	the	form	is	being	refreshed.)
            $scope.$watch('selectedResource', function() {
                if ($scope.selectedResource && !$scope.isLoadingForm) {

                    console.log('watch	on	selected	resource');
                    $scope.refresh();
                }

            });
            

            //	update	the	resources	list	when	the	form	changes	
            $scope.$watch('selectedDefinition', function () {

                if ($scope.selectedDefinition) {

                    $scope.getDefResources($scope.selectedDefinition);
                }
            });

                       

            
            //
            //	Init
            //
            $scope.formMode =  spEditForm.formModes.view;
            $scope.error = '';
            $scope.formControl = null;
            $scope.formData = null;            
            
            // modal
            $scope.modalInfo = {
                modalOpts: {
                    backdropFade: true,
                    dialogFade: true
                },
                isOpen: false
            };
            
             
            
            $scope.getFormsList().then(
                function (data) {
                    $scope.customForms = data;
                    $scope.selectedForm = $scope.customForms[0];
                },
                function (error) { $scope.error = error; });
            

            $scope.getDefinitionList().then(
                function (data) {
                    $scope.definitions = data;
                    $scope.selectedDefinition = $scope.definitions[0];
                },
                function (error) { $scope.error = error; });
        });

}());