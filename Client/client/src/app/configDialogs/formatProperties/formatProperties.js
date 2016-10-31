// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mod.app.configureDialog.formControlFormat', ['ui.bootstrap', 'mod.app.editForm', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService', 'sp.common.fieldValidator', 'mod.app.configureDialog.fieldPropertiesHelper', 'mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerUtils'])
    .directive('formControlFormat', function () {
        return {
            restrict: 'E',
            transclude: false,
            replace: true,
            scope: {
                formControl: '=?',
                resizeOptions: '='
            },
            templateUrl: 'configDialogs/formatProperties/views/formatProperties.tpl.html',
            controller: 'formatController'
        };
    })
    .controller('formatController', function ($scope, spEditForm, namedColors, spColorPickerUtils) {
        if (!$scope.formControl)
            return;
        if (!$scope.resizeOptions)
            return;
        $scope.model = {
            popupColorOptions: {
                color: {
                    r: 255,
                    g: 255,
                    b: 255,
                    a: 255
                },
                isOpen: false,
                colorName: 'White',
                selectedHorizontalMode: null,
                selectedVerticalMode: null
            }
        };

        // Returns the background color as a css string
        $scope.getCssBackgroundColor = function (color) {
            var style = {};

            if (color) {
                style['background-color'] = spUtils.getCssColorFromRgb(color);
            }

            return style;
        };

        //
        //Get control backgroung olor.
        //

        var bgColor = spEditForm.getControlBackgroundColor($scope.formControl);
        if (bgColor) {
            var jColor = jQuery.Color(bgColor);

            if (jColor) {
                $scope.model.popupColorOptions.color = {
                    r: jColor._rgba[0],
                    g: jColor._rgba[1],
                    b: jColor._rgba[2],
                    a: jColor._rgba[3] * 255
                };
            }
        }

        //set horizontal and vertical resize modes.
        $scope.model.selectedHorizontalMode = $scope.resizeOptions.resizeModes[1];
        if ($scope.formControl.renderingHorizontalResizeMode) {
            var currentHorizontalMode = _.find($scope.resizeOptions.resizeModes, function (mode) {
                return mode.alias() === $scope.formControl.renderingHorizontalResizeMode.alias();
            });
            if (currentHorizontalMode)
                $scope.model.selectedHorizontalMode = currentHorizontalMode;
        }

        $scope.model.selectedVerticalMode = $scope.resizeOptions.resizeModes[1];
        if ($scope.formControl.renderingVerticalResizeMode) {
            var currentVerticalMode = _.find($scope.resizeOptions.resizeModes, function (mode) {
                return mode.alias() === $scope.formControl.renderingVerticalResizeMode.alias();
            });
            if (currentVerticalMode)
                $scope.model.selectedVerticalMode = currentVerticalMode;
        }

        $scope.$watch('formControl.renderingHorizontalResizeMode', function (resizeMode) {

            if (resizeMode && resizeMode.nsAlias !== $scope.model.selectedHorizontalMode.nsAlias) {
                $scope.model.selectedHorizontalMode = getResizeMode(resizeMode.nsAlias);
            }
        });

        $scope.$watch('formControl.renderingVerticalResizeMode', function (resizeMode) {

            if (resizeMode && resizeMode.nsAlias !== $scope.model.selectedVerticalMode.nsAlias) {
                $scope.model.selectedVerticalMode = getResizeMode(resizeMode.nsAlias);
            }
        });

        $scope.$watch('model.selectedHorizontalMode', function (resizeMode) {
            if (resizeMode && resizeMode.nsAlias !== $scope.formControl.renderingHorizontalResizeMode.nsAlias) {
                $scope.formControl.setRenderingHorizontalResizeMode(resizeMode);
            }
        });

        $scope.$watch('model.selectedVerticalMode', function (resizeMode) {
            if (resizeMode && resizeMode.nsAlias !== $scope.formControl.renderingVerticalResizeMode.nsAlias) {
                $scope.formControl.setRenderingVerticalResizeMode(resizeMode);
            }
        });

        $scope.openColorPicker = function () {
            $scope.model.popupColorOptions.isOpen = true;
        };
        $scope.$watch('model.popupColorOptions.color', function () {
            setColorName($scope.model.popupColorOptions.color);
            // Set control backgroung olor.
            spEditForm.setControlBackgroundColor($scope.formControl, spUtils.getCssColorFromRgb($scope.model.popupColorOptions.color));

        }, true);

        // Sets the name of the specified color
        function setColorName(color) {
            var foundColor = _.find(namedColors, function (nc) {
                return spColorPickerUtils.areColorsEqual(nc.value, color);
            });

            if (foundColor) {
                $scope.model.popupColorOptions.colorName = foundColor.name;
            } else {
                $scope.model.popupColorOptions.colorName = 'Custom';
            }
        }

        function getResizeMode(strModeAlias) {
            if (!$scope.resizeOptions.resizeModes || !strModeAlias) {
                return undefined;
            }

            var mode = _.find($scope.resizeOptions.resizeModes, function (mode) {
                return mode.nsAlias === strModeAlias;
            });

            return mode;
        }

    });