// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing an edit form.
    * spEditForm provides the base edit form.
    *
    * @module spEditForm
    * @example

    Using the spEditForm:

    &lt;sp-edit-form&gt;&lt;/sp-edit-form&gt

    */
    angular.module('mod.app.editForm.designerDirectives.spEditForm', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.common.alerts',
        'mod.common.spCachingCompile'
    ])
        .directive('spEditForm', function (spFormBuilderService, spAlertsService, $q, spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},
                link: function (scope, element) {
                    /////
                    // Ensure the model is initialized.
                    /////
                    scope.model = scope.model || {};

                    scope.spFormBuilderService = spFormBuilderService;

                    /////
                    // Stops certain characters from being entered into the editable labels.
                    /////
                    scope.validateinput = function (evt) {
                        var e = evt || event;

                        if (e.shiftKey) {
                            switch (e.which) {
                                case 188: // <
                                case 190: // >
                                    e.stopPropagation();
                                    e.preventDefault();
                                    return false;
                            }
                        }

                        return true;
                    };

                    /////
                    // Change validate.
                    /////
                    scope.changeValidate = function (value) {

                        if (value) {
                            return value.replace(/[<>]+/g, '');
                        }
                        return value;
                    };

                    /////
                    // Gets whether this name is valid.
                    /////
                    scope.isValidName = function (newName, oldName) {

                        if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                            return true;
                        }

                        if (!newName) {
                           spAlertsService.addAlert('Invalid form name specified.', { severity: spAlertsService.sev.Warning, expires: true });
                           return false;
                        }

                        return true;
                    };

                    scope.showHelp = function () {
                        return scope.spFormBuilderService &&
                               scope.spFormBuilderService.form &&
                               scope.spFormBuilderService.form.showFormHelpText &&
                               scope.spFormBuilderService.form.description;
                    };

                    scope.measureArrangeOptions = {
                        id: 'formBuilder'
                    };

                    scope.$on('calcTabsLayout', function (event, callback) {
                        event.stopPropagation();
                        callback(scope.measureArrangeOptions.id);
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spEditForm/spEditForm.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());