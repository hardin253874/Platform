// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function() {
    'use	strict';

    angular.module('mod.app.editFormDesigner', ['ui.router', 'ui.bootstrap', 'titleService', 'mod.common.spEntityService', 'ngGrid', 'mod.app.editFormCache', 'mod.app.editFormWebServices',
                   'mod.app.editForm.designerDirectives', 'sp.navService', 'mod.common.alerts'])
        .config(function($stateProvider){
            $stateProvider.state('editFormDesigner', {
                url: '/{tenant}/{eid}/editFormDesigner?path',
                templateUrl: 'editForm/editFormDesigner.tpl.html'
            });
        })
        .controller('editFormDesignerController', function editFormDesignerController($scope, $stateParams, titleService, spEntityService, $q, $timeout, editFormCache, editFormWebServices, spNavService, spAlertsService) {

            titleService.setTitle('Edit Form Designer');


            function alert(msg, severity) {
                spAlertsService.addAlert(msg, { severity: severity || 'error' });
            }


            //	Save	the	updates	to	the	resource
            $scope.save = function () {
                spEntityService.putEntity($scope.formControl).then(
                    function (result) {
                        editFormCache.remove($scope.formControl.id());      // flush the local form cache

                        if (spNavService.canNavigateToParent()) {
                            spNavService.navigateToParent();
                        } else {
                            alert('Saved', 'info');
                        }
                    },
                    function (error) {
                        alert('An error occurred saving the form: ' + sp.result(error, 'data.Message'));
                });
            };

            $scope.cancel = function () {
                if (spNavService.canNavigateToParent()) {
                    spNavService.navigateToParent();
                } else {
                    alert('Cancelled', 'info');
                }
            };


            $scope.closeAlert = function(index) {
                $scope.alerts.splice(index, 1);
            };

            /* getFormDefinition promise */

            function getFormDefinition(selectedFormId) {
                return editFormWebServices.getFormDefinition(selectedFormId, true); // we are not using the cache as we are getting the designer version.
            }

            //
            //	Init
            //
            $scope.model = {
                formId: $stateParams.eid ? parseInt($stateParams.eid, 10) : 0,
                dirty: false
            };

            $scope.isDirty = function () {
                if (!$scope.formControl || !$scope.formControl.hasChangesRecursive) {
                    return false;
                }

                $scope.model.dirty = $scope.formControl.hasChangesRecursive();

                return $scope.model.dirty;
            };

            $scope.removeControl = function () {
                $scope.formControl.setContainedControlsOnForm([]);
                $scope.$broadcast('onControlsOnFormChanged');
            };

            $scope.fieldValidationMessages = [];
            $scope.alerts = [];


            $scope.isInDesigner = true;
            $scope.formControl = null;
            $scope.formData = null;
            
            /*	getFormControl	*/
            if ($scope.model.formId !== 0) {
                getFormDefinition($scope.model.formId).then(
                    function(formControl) {
                        $scope.alerts.push({ type: 'success', msg: 'Got formControl' });

                        $scope.formControl = formControl;
                    },
                    function (error) {
                        console.log('Error while trying to get Form:', error);
                        $scope.alerts.push({ type: 'error', msg: 'Error while trying to get form:' + error.data.ErrorMessage }); // NOTE: Need better error message
                    });
            } else {
                $scope.alerts.push({ type: 'error', msg: 'No form Id specified.' }); // NOTE, this should probably be changed to look for the default form.
            }

        });

}());