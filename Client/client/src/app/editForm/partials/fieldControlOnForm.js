// Copyright 2011-2016 Global Software Innovation Pty Ltd


angular.module('app.editForm.fieldControlOnFormController', ['mod.app.editForm', 'sp.common.directives'])
    .controller('fieldControlOnFormController',

        function ($scope, spFieldValidator, spEditForm) {
            "use strict";


            var fieldToRender;


            if ($scope.formControl) {
                fieldToRender = $scope.formControl.getFieldToRender();

                $scope.isMandatoryOnForm = $scope.formControl.getMandatoryControl();
                $scope.isRequired = $scope.isMandatoryOnForm || ($scope.fieldToRender && $scope.fieldToRender.isRequired);
                $scope.titleModel = spEditForm.createTitleModel($scope.formControl);

                spEditForm.commonFieldControlInit(fieldToRender);
                $scope.isReadOnly = $scope.formMode !== spEditForm.formModes.edit;
                $scope.showFieldHelpText = fieldToRender.showFieldHelpText;
                $scope.helpText = fieldToRender.description;
                /////
                // When the form data changes, update the model.
                /////
                var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                $scope.$watch(dataWatch, function (existingValue) {
                    $scope.fieldValue = existingValue;
                });


                $scope.$watch("fieldValue", function (value) {

                    if ($scope.formData) {
                        $scope.formData.setField(fieldToRender.id(), value, spEntity.DataType.String);
                    } else {
                    }

                });


            }

        }
    );

