// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     * Module implementing a foreground background color picker dialog.        
     *
     * @module spColorPickerFgBgDialog    
     * @example            
        
     Using the spColorPickerFgBgDialog:
    
     spColorPickerFgBgDialog.showModalDialog(options).then(function(result) {
      });
    
     where options is an object with the following properties:
        - foregroundColor - {object}. The foreground color.
            - a {number} - The alpha channel 0-255
            - r {number} - The red channel 0-255
            - g {number} - The green channel 0-255
            - b {number} - The blue channel 0-255
            
        - backgroundColor - {object}. The background color.            
            - a {number} - The alpha channel 0-255
            - r {number} - The red channel 0-255
            - g {number} - The green channel 0-255
            - b {number} - The blue channel 0-255

     where result is:
        - false, if cancel is clicked
        - selected foreground and background color - {object} if ok is clicked
     */
    angular.module('mod.common.ui.spColorPickerFgBgDialog', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerUtils', 'mod.common.ui.spDialogService'])
        .controller('spColorPickerFgBgDialogController', function ($scope, $uibModalInstance, options, namedColors, spColorPickerUtils) {

            // Setup the dialog model
            $scope.model = {
                fgColorPopupOptions: {
                    isOpen: false,
                    color: _.clone(options.foregroundColor)
                },
                bgColorPopupOptions: {
                    isOpen: false,
                    color: _.clone(options.backgroundColor)
                }
            };

            // Returns the background color as a css string
            $scope.getCssBackgroundColor = function (color) {
                var style = {};

                if (color) {
                    style['background-color'] = spColorPickerUtils.getCssColorFromRgba(color);
                }

                return style;
            };


            // Returns the foreground color as a css string
            $scope.getCssForegroundColor = function (color) {
                var style = {};

                if (color) {
                    style['color'] = spColorPickerUtils.getCssColorFromRgba(color);
                }

                return style;
            };

           
            // Foreground color drop down click handler
            $scope.fgColorClick = function () {
                $scope.model.fgColorPopupOptions.isOpen = !$scope.model.fgColorPopupOptions.isOpen;
            };


            // Background color drop down click handler
            $scope.bgColorClick = function () {
                $scope.model.bgColorPopupOptions.isOpen = !$scope.model.bgColorPopupOptions.isOpen;
            };


            // Watches for changes in the selected foreground color and updates it's name
            $scope.$watch('model.fgColorPopupOptions.color', function () {
                setColorName($scope.model.fgColorPopupOptions.color);
            }, true);


            // Watches for changes in the selected background color and updates it's name
            $scope.$watch('model.bgColorPopupOptions.color', function () {
                setColorName($scope.model.bgColorPopupOptions.color);
            }, true);


            // Ok click handler
            $scope.ok = function () {
                var result = {
                    foregroundColor: {},
                    backgroundColor: {}
                };

                spColorPickerUtils.copyColor($scope.model.fgColorPopupOptions.color, result.foregroundColor);
                spColorPickerUtils.copyColor($scope.model.bgColorPopupOptions.color, result.backgroundColor);                

                $uibModalInstance.close(result);
            };


            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };


            // Sets the name of the specified color
            function setColorName(color) {
                var foundColor = _.find(namedColors, function (nc) {
                    return spColorPickerUtils.areColorsEqual(nc.value, color.a);
                });

                if (foundColor) {
                    color.name = foundColor.name;
                } else {
                    color.name = 'Custom';
                }
            }            
        })
        .factory('spColorPickerFgBgDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        keyboard: true,
                        backdropClick: false,
                        windowClass: 'spColorPickerFgBgDialog',
                        templateUrl: 'colorPicker/spColorPickerFgBgDialog.tpl.html',
                        controller: 'spColorPickerFgBgDialogController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };

            return exports;
        });
}());