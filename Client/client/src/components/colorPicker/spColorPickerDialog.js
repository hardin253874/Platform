// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     * Module implementing a color picker dialog.        
     *
     * @module spColorPickerDialog    
     * @example            
        
     Using the spColorPickerDialog:
    
     spColorPickerDialog.showModalDialog(options).then(function(result) {
      });
    
     where options is an object with the following properties:
        - color - {object}. The foreground color.
            - a {number} - The alpha channel 0-255
            - r {number} - The red channel 0-255
            - g {number} - The green channel 0-255
            - b {number} - The blue channel 0-255
      

     where result is:
        - false, if cancel is clicked
        - selected color - {object} if ok is clicked
     */
    angular.module('mod.common.ui.spColorPickerDialog', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerUtils', 'mod.common.ui.spDialogService'])
        .controller('spColorPickerDialogController', function ($scope, $uibModalInstance, options, namedColors, spColorPickerUtils) {

            // Setup the dialog model
            $scope.model = {
                color: _.clone(options.color)
            };



            // Ok click handler
            $scope.ok = function () {
                var result = {
                    color: {}
                };
                if ($scope.model.color) {
                    spColorPickerUtils.copyColor($scope.model.color, result.color);
                } else {
                    spColorPickerUtils.copyColor(options.color, result.color);
                }

                $uibModalInstance.close(result);
            };


            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };

           
        })
        .factory('spColorPickerDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        keyboard: true,
                        backdropClick: false,
                        windowClass: 'spColorPickerDialog',
                        templateUrl: 'colorPicker/spColorPickerDialog.tpl.html',
                        controller: 'spColorPickerDialogController',
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