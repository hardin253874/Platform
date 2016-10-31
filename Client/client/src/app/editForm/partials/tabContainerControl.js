// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('app.editForm.tabContainerController', ['mod.app.editForm'])
        .controller('tabContainerController', function ($scope, spEditForm) {

            $scope.controlsOnForm =
                $scope.formControl.getContainedControlsOnForm()
                    .sort(
                    function (a, b) {
                        return (a.getName() > b.getName());
                    });

            /////
            // Convert the current edit form scope values into a generic model with
            // no ties to edit form.
            /////
            $scope.model = {
                isReadOnly: $scope.isReadOnly,
                isInTestMode: $scope.isInTestMode
            };

            setTabItems($scope.controlsOnForm, $scope);

            $scope.getFormControlFile = spEditForm.getFormControlFile;
            $scope.getControlTitle = spEditForm.getControlTitle;

            $scope.$watch('formData', function () {
                if ($scope.formData) {

                    if ($scope.tabItems) {
                        _.forEach($scope.tabItems, function (item) {
                            item.model.formData = $scope.formData;
                        });
                    }
                }
            });

            ///
            // When the is-read-only value changes, update the model.
            ///
            $scope.$watch("isReadOnly", function (value) {

                $scope.model.isReadOnly = value;
            });

            function setTabItems(controlsOnForm, scope) {
                var tabItems = [];
                if (controlsOnForm && _.isArray(controlsOnForm)) {
                    _.forEach(controlsOnForm, function (control) {
                        var isRelationship = control.hasRelationship('console:relationshipToRender') && control.getRelationshipToRender() !== null;

                        var tabItem = {
                            name: spEditForm.getControlTitle(control),
                            ordinal: control.hasField('console:renderingOrdinal') ? control.renderingOrdinal : null,
                            url: 'editForm/partials/shared/tabControl.tpl.html',
                            model: {
                                formControl: control,
                                formData: scope.formData,
                                formMode: scope.formMode,
                                isReadOnly: scope.isReadOnly,
                                isInTestMode: scope.isInTestMode
                            },
                            parentModel: scope.model
                        };

                        tabItems.push(tabItem);

                    });
                }

                $scope.tabItems = tabItems;
            }
        });

}());