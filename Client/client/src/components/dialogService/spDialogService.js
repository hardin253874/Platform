// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
* Module implementing the spDialogService.
* spDialogService displays both modal and non-modal dialogs as well as message boxes in a consistent manner.
*
* @module spDialogService
* @example

Using spDialogService:

1. Inject the module into module:
    angular.module('yourModule', ['mod.common.ui.spDialogService'])
2. Pass the spDialogService to your directive/controller/etc
    .directive('yourDirective', function (spDialogService) {
3. To show a modal dialog, call the following (defaults and options defined below):
    spDialogService.showModalDialog(defaults, options);
4. To show a dialog, call the following (defaults and options defined below):
    spDialogService.showDialog(defaults, options);
5. To show a message box, call the following (title, message and buttons):
    spDialogService.showMessage(title, message, buttons).then(function(result) {});

where the 'defaults' object can contain the following:

Properties:
    - backdrop                      - (optional) Whether the background becomes grayed out or not.  'static' shows a background that will not close the dialog when clicked on.
        default: true
    - keyboard {boolean}            - (optional) True if the keyboard can close the dialog (esc) or not.
        default: true
    - templateUrl {string}          - (optional) The template url to be hosed in the dialog.
        default: 'dialogService/spDialogService.tpl.html'
    - controller {object}           - (optional) A controller object to be used. If undefined, a default controller will be used.
        default: null
    - title {string}                - The message box's title.
        default: null
    - message {string}              - The message to display.
        default: null
    - buttons {array}               - Array of buttons to show in the message box.
        default: null
    - windowClass             
        default: null               - (optional) A string containing a css class name to be added to the modal container
    - backdropClass 
        default: null               - (optional) A string containing a css class name to be added to the backdrop 



The 'options' object can contain the following:

Properties:
    - callback {function}           - (optional) A function that is called when the dialog is closed.

The 'buttons' array contains objects with the following:

Properties:
    - result                        - Passed to the promise upon click.
    - label                         - The buttons label.
    - cssClass                      - The class applied to the button.

To define additional properties for use in binding on the template, add them to the 'options' object. These properties can then be referenced in the dialog through {{dialogOptions.propertyName}}
    
*/

angular.module('mod.common.ui.spDialogService', ['ui.bootstrap'])
    .controller('spDialogServiceShowMessageController', ['$scope', '$uibModalInstance', 'options', function ($scope, $uibModalInstance, options) {

        if (options) {
            $scope.title = options.title;
            $scope.message = options.message;
            $scope.buttons = options.buttons;
        }

        $scope.close = function (result) {
            $uibModalInstance.close(result);
        };

        $scope.$on('signedout', function() {
            $scope.close();
        });
    }])
    .service('spDialogService', ['$uibModal', '$rootScope', function($uibModal, $rootScope) {
        'use strict';

        /////
        // Dialog defaults.
        /////
        var dialogDefaults = {
            backdrop: 'static',
            keyboard: true,
            modalFade: true,
            templateUrl: 'dialogService/spDialogService.tpl.html'
        };

        /////
        // Dialog options.
        /////
        var dialogOptions = {
            closeButtonText: 'Close',
            actionButtonText: 'OK',
            headerText: 'Proceed?',
            bodyText: 'Perform this action?'
        };

        /////
        // Method used to show a modal dialog.
        /////
        this.showModal = function(customDialogDefaults, customDialogOptions) {
            return this.showModalDialog(customDialogDefaults, customDialogOptions);
        };

        /////
        // Method used to show a modal dialog.
        /////
        this.showModalDialog = function(customDialogDefaults, customDialogOptions) {
            if (!customDialogDefaults)
                customDialogDefaults = {};

            customDialogDefaults.backdropClick = false;

            return this.showDialog(customDialogDefaults, customDialogOptions);
        };

        /////
        // Method used to show a non-modal dialog.
        /////
        this.show = function(customDialogDefaults, customDialogOptions) {
            return this.showDialog(customDialogDefaults, customDialogOptions);
        };

        /////
        // Method used to show a non-modal dialog.
        // Returns a promise
        /////
        this.showDialog = function(customDialogDefaults, customDialogOptions) {
            var tempDialogDefaults = {};
            var tempDialogOptions = {};

            angular.extend(tempDialogDefaults, dialogDefaults, customDialogDefaults);

            angular.extend(tempDialogOptions, dialogOptions, customDialogOptions);

            if (!tempDialogDefaults.controller) {

                tempDialogDefaults.controller = function($scope, $uibModalInstance) {

                    $scope.dialogOptions = tempDialogOptions;

                    $scope.dialogOptions.close = function(result) {

                        $uibModalInstance.dismiss(result);
                    };
                    $scope.dialogOptions.callback = function(result) {

                        $uibModalInstance.close(result);
                    };
                };
            }

            if (!tempDialogDefaults.scope) {
                tempDialogDefaults.scope = (tempDialogDefaults.scope || $rootScope).$new();
            }

            /////
            // Watch for 'signedout' messages and close the dialog.
            // Note* The $uibModal service will generate a $new inherited scope based on tempDialogDefaults.scope and
            // will add the $close() method to that so there is no way to easily close the dialog from here. For the
            // time being, look at $$childHead and see if it has the $close method and if so, call it. :(
            /////
            tempDialogDefaults.scope.$on('signedout', function () {
                if (tempDialogDefaults.scope.$$childHead && tempDialogDefaults.scope.$$childHead.$close) {
                    tempDialogDefaults.scope.$$childHead.$close();
                }
            });

            return $uibModal.open(tempDialogDefaults).result.then(function(result) {
                if (tempDialogOptions.callback) {
                    tempDialogOptions.callback(result);
                }
                return result;
            });
        };

        /////
        // Method used to show a message box.
        /////
        this.showMessageBox = function (title, message, buttons) {
            return this.showMessage(title, message, buttons);
        };

        /////
        // Method used to show a message box.
        /////
        this.showMessage = function (title, message, buttons) {

            if (!buttons) {
                buttons = [
                    { result: 'cancel', label: 'Cancel' },
                    { result: 'ok', label: 'Ok', cssClass: 'btn-primary' }
                ];
            }

            /////
            // Setup the options object.
            /////
            var options = {
                title: title,
                message: message,
                buttons: buttons
            };

            return $uibModal.open({
                templateUrl: 'template/dialog/message.html',
                controller: 'spDialogServiceShowMessageController',
                resolve: {
                    options: function () {
                        return options;
                    }
                }
            }).result;
        };

    }]).run(["$templateCache", function (e) {
        e.put("template/dialog/message.html", '<div class="modal-header">   <h6 style="margin-top: 0; margin-bottom: 0">{{ title }}</h6></div><div class="modal-body">  <p>{{ message }}</p></div><div class="modal-footer">    <button ng-repeat="btn in buttons" ng-click="close(btn.result)" class=btn ng-class="btn.cssClass">{{ btn.label }}</button></div>');
    }]);
    