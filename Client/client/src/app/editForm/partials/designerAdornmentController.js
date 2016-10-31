// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use	strict';

    /**
    * Module implementing the 'display options' controller.
    * Controls the layout containers used by the form builder toolbox.
    */
    angular.module('app.editForm.designerAdornmentController', ['mod.app.editForm'])
        .controller('designerAdornmentController', function ($scope, spEditForm) {

            /**
            * Converts an RGBA color to its CSS equivalent.
            */
            function getCssColorFromRgb(color) {
                var r = 0, g = 0, b = 0, a = 1;

                if (!color) {
                    return null;
                }

                if (color.r) {
                    r = color.r;
                }

                if (color.g) {
                    g = color.g;
                }

                if (color.b) {
                    b = color.b;
                }

                if (color.a >= 0) {
                    a = color.a / 255;
                }

                return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
            }
           
            /**
            * The resize options.
            */
            $scope.resizableOptions = {
                disabled: $scope.formMode !== spEditForm.formModes.design,
                onResizeStop: function (event, source, data) {
                    $scope.onResizeStop(event, source, data);
                }
            };

            /**
            * Function that handles the 'Resize Stop' event.
            */
            $scope.onResizeStop = function (event, source, data) {

                spEditForm.resizeControl(data, source.size.width, source.size.height);

                $scope.$apply();
            };

            /**
            * The drop options.
            */
            $scope.dropOptions = {

                onDragLeave: function (event, source, target, dragData) {

                    $scope.onDragLeave(event, source, target, dragData);
                },
                onDragOver: function (event, source, target, dragData) {

                    $scope.onDragOver(event, source, target, dragData);
                },
                onDrop: function (event, source, target, dragData) {

                    $scope.onDrop(event, source, target, dragData);
                }
            };

            /**
            * Function that handles the 'Drag Over' event.
            */
            $scope.onDragOver = function (event, source, target) {

                var jTarget = $(target).closest('.adornment');

                if (jTarget && jTarget.length > 0) {
                    jTarget.css('background-color', 'whiteSmoke');
                }
            };

            /**
            * Function that handles the 'Drag Leave' event.
            */
            $scope.onDragLeave = function (event, source, target) {

                var jTarget = $(target).closest('.adornment');

                if (jTarget && jTarget.length > 0) {
                    jTarget.css('background-color', '');
                }
            };

            /**
            * Function that handles the 'Drop' event.
            */
            $scope.onDrop = function (event, source, target, dragData) {
                var id;

                var jTarget = $(target).closest('.adornment');

                if (jTarget && jTarget.length > 0) {
                    jTarget.css('background-color', '');
                }

                if (!dragData) {
                    return;
                }

                if (dragData._fieldEntity) {
                    id = dragData._fieldEntity._id._id;
                } else if (dragData._relEntity) {
                    id = dragData._relEntity._id._id;
                } else if (dragData.id) {
                    var entity = spEntity.createEntityOfType(dragData.id);

                    entity.registerField('core:name', spEntity.DataType.Bool);
                    entity.registerField('core:description', spEntity.DataType.Bool);
                    entity.registerField('core:isRequired', spEntity.DataType.Bool);
                    entity.registerField('console:renderingOrdinal', spEntity.DataType.Int32);
                    entity.registerField('console:renderingHeight', spEntity.DataType.Int32);
                    entity.registerField('console:renderingWidth', spEntity.DataType.Int32);
                    entity.registerField('console:renderingBackgroundColor', spEntity.DataType.String);
                    entity.registerField('console:mandatoryControl', spEntity.DataType.Bool);
                    entity.registerField('console:readOnlyControl', spEntity.DataType.Bool);                    
                    entity.registerRelationship('console:fieldToRender', spEntity.DataType.Bool);
                    entity.registerRelationship('console:containedControlsOnForm', spEntity.DataType.Bool);

                    entity.setName('');
                    entity.setDescription('');
                    entity.setIsRequired(false);
                    entity.setReadOnlyControl(false);                    
                    entity.setMandatoryControl(false);                    
                    entity.setRenderingHeight(100);
                    entity.setRenderingWidth(200);

                    $scope.addControl(entity, $scope);

                    $scope.$apply();
                } else {
                    console.log('Unable to determine drop item');
                    return;
                }
            };

            /**
            * Function that handles the configure buttons 'Click' event.
            */
            $scope.configure_click = function () {

                var color = spEditForm.getControlBackgroundColor($scope.formControl);

                var existingColor = {
                    r: 0,
                    g: 0,
                    b: 0,
                    a: 255
                };

                if (color) {
                    var jColor = jQuery.Color(color);

                    if (jColor) {
                        existingColor = {
                            r: jColor._rgba[0],
                            g: jColor._rgba[1],
                            b: jColor._rgba[2],
                            a: jColor._rgba[3]
                        };
                    }
                }

                window.alert('show configure dialog here');
            };

            /**
            * Function that handles the remove buttons 'Click' event.
            */
            $scope.remove_click = function () {

                $scope.removeControl($scope.formControl);
            };

        });
}());