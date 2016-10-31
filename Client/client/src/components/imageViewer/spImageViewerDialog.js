// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an image viewer dialog.    
    * 
    * @module spImageViewerDialog   
    * @example
        
    Using the spImageViewerDialog:
    
    spImageViewerDialog.showModalDialog(options).then(function(result) {
    });
    
    where options is an object with the following properties:
        - imageUrl - {string}. The url of the image to show.            
    */
    angular.module('mod.common.ui.spImageViewerDialog', [
        'ui.bootstrap',
        'mod.common.ui.spDialogService',
        'mod.common.spXsrf'
    ])
        .controller('spImageViewerDialogController', function ($scope, $uibModalInstance, $document, options, spXsrf) {
            var image = new Image(),
                body = $document.find('body'),
                imagePadding = 10,                
                containerPadding = 40,
                defaultDialogSize = 200 - containerPadding;


            // Setup the dialog model
            $scope.model = {                
                imageDisplayMode: 'none',
                imageWidth: null,
                imageHeight: null,
                imageUrl: null,
                containerMaxWidth: (body.width() * 0.8) - (containerPadding + imagePadding),
                containerMaxHeight: body.height() * 0.7,                
                containerAspectRatio: 0,
                imageDisplayWidth: 'auto',
                imageDisplayHeight: 'auto'
            };

            
            $scope.model.containerAspectRatio = $scope.model.containerMaxWidth / $scope.model.containerMaxHeight;


            // Returns the image style
            $scope.getImageStyle = function () {
                var style = {};

                switch ($scope.model.imageDisplayMode) {
                case 'natural':
                    style.height = 'auto';
                    style.width = 'auto';
                    break;
                case 'scaled':
                    style.height = $scope.model.imageDisplayHeight + 'px';
                    style.width = $scope.model.imageDisplayWidth + 'px';                    
                    break;
                }

                if (options.backgroundColor) {
                    style['background'] = spUtils.getCssColorFromARGBString(options.backgroundColor);
                    style['background-color'] = spUtils.getCssColorFromARGBString(options.backgroundColor);
                }

                return style;
            };


            // Returns the image container style
            $scope.getImageContainerStyle = function () {
                var style = {};

                switch ($scope.model.imageDisplayMode) {
                case 'natural':
                    style.height = 'auto';
                    style.width = 'auto';
                    style.overflow = 'hidden';
                    break;
                case 'scaled':                                       
                    style.height = ($scope.model.imageDisplayHeight + imagePadding) + 'px';
                    style.width = ($scope.model.imageDisplayWidth + imagePadding) + 'px';
                    style.overflow = 'hidden';                                                              
                    break;
                }

                return style;
            };
            

            // Initialise the controller
            function initialise() {
                var scale;

                if (!image.complete) {
                    return;
                }

                $scope.model.imageWidth = image.naturalWidth;
                $scope.model.imageHeight = image.naturalHeight;
                $scope.model.imageUrl = options.imageUrl;

                // Calculate the aspect ratio of the image
                $scope.model.imageAspectRatio = $scope.model.imageWidth / $scope.model.imageHeight;

                if ($scope.model.imageWidth <= defaultDialogSize &&
                    $scope.model.imageHeight <= defaultDialogSize) {
                    // The image is smaller than the dialog size, so show the image at it
                    // natural size. 
                    $scope.model.imageDisplayWidth = 'auto';
                    $scope.model.imageDisplayHeight = 'auto';
                    $scope.model.imageDisplayMode = 'natural';
                    $scope.model.dialogSize = null;
                } else if (($scope.model.imageWidth > defaultDialogSize ||
                        $scope.model.imageHeight > defaultDialogSize) &&
                    $scope.model.imageWidth < $scope.model.containerMaxWidth &&
                    $scope.model.imageHeight < $scope.model.containerMaxHeight) {
                    // It is bigger than the default dialog size but smaller
                    // than the maximum container size
                    // Resize the dialog but show it at its natural size
                    $scope.model.imageDisplayWidth = 'auto';
                    $scope.model.imageDisplayHeight = 'auto';
                    $scope.model.imageDisplayMode = 'natural';
                    $scope.model.dialogSize = {
                        width: $scope.model.imageWidth + containerPadding + imagePadding
                    };
                } else {
                    // The image is bigger than the container 
                    // Scale it down.
                    // Need to scale the image                        
                    $scope.model.imageDisplayMode = 'scaled';

                    if ($scope.model.imageAspectRatio < $scope.model.containerAspectRatio) {
                        // Scale by height
                        scale = $scope.model.containerMaxHeight / $scope.model.imageHeight;
                    } else {
                        // Scale by width
                        scale = $scope.model.containerMaxWidth / $scope.model.imageWidth;
                    }

                    $scope.model.imageDisplayWidth = Math.round((scale * $scope.model.imageWidth));
                    $scope.model.imageDisplayHeight = Math.round((scale * $scope.model.imageHeight));

                    $scope.model.dialogSize = {
                        width: $scope.model.imageDisplayWidth + containerPadding + imagePadding
                    };
                }                
            }
            

            image.onload = function () {
                $scope.$apply(function () {
                    initialise();
                });
            };


            image.src = spXsrf.addXsrfTokenAsQueryString(options.imageUrl);


            // Ok click handler
            $scope.ok = function () {
                $uibModalInstance.close(true);
            };           
        })
        .directive('spImageViewerDialogDomHelper', function () {
            return {
                restrict: 'AC',
                link: function (scope, element, iAttrs) {
                    var iModalDialogElement = element.parents('.modal-dialog').first();

                    scope.$watch('model.dialogSize', function (dialogSize) {
                        if (dialogSize &&
                            dialogSize.width &&
                            dialogSize.width !== iModalDialogElement.width()) {
                            iModalDialogElement.width(dialogSize.width);                            
                        }
                    });
                }
            };
        })
        .factory('spImageViewerDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        backdrop: true,
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'imageViewer/spImageViewerDialog.tpl.html',
                        controller: 'spImageViewerDialogController',
                        windowClass: 'spImageViewerDialog',
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