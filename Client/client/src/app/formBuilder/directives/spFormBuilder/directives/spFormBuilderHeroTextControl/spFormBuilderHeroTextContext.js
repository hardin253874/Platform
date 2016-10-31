// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing context menu for hero text.
    * spFormBuilderEmbeddedForm renders a child form that has been placed on the form during design time.
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderHeroTextContext', [
        'mod.app.heroText.spHeroTextProperties',
        'mod.app.formBuilder.directives.spFormBuilderAssignParent',
        'mod.common.ui.spDialogService',
        'mod.app.configureDialog.Controller'
    ])
        .directive('spFormBuilderHeroTextContext', function (spHeroTextProperties, spFormBuilderAssignParentService, controlConfigureDialogFactory,
                                                             spDialogService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: true,
                transclude: false,
                scope: {
                    control: '=?' // the heroTextControl
                },
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderGenericControl/spFormBuilderGenericContext.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.model = {
                        showContextMenu: false,
                        contextMenu: {
                            menuItems: [
                                {
                                    text: 'Hero Text Properties',
                                    icon: 'assets/images/16x16/propertiesChart.png',
                                    type: 'click',
                                    click: 'onHeroTextPropsClick()'
                                },
                                {
                                    text: 'Container Properties',
                                    icon: 'assets/images/16x16/propertiesContainer.png',
                                    type: 'click',
                                    click: 'onContainerPropertiesClick()'
                                },
                                {
                                    text: 'Assign Parent',
                                    icon: 'assets/images/16x16/assign.png',
                                    type: 'click',
                                    click: 'onAssignDataClick()',
                                    disableForScreens: true
                                }
                            ]
                        }
                    };

                    // Handle configure click
                    scope.onConfigureClick = function () {
                        scope.model.showContextMenu = true;
                    };

                    /////
                    // Show the chart properties.
                    scope.onHeroTextPropsClick = _.partial(spHeroTextProperties.showDialog, {
                        heroTextControl: scope.control
                    });

                    scope.onContainerPropertiesClick = function () {
                        var options = {
                            formControl: scope.control,
                            isFieldControl: true,
                            isFormControl: true,
                            relationshipType: controlConfigureDialogFactory.getRelationshipType(scope.control),
                            isReverseRelationship: scope.control.isReversed
                        };

                        controlConfigureDialogFactory.createDialog(options);
                    };


                    // Handle 'Assign Data' click
                    scope.onAssignDataClick = _.partial(spFormBuilderAssignParentService.showDialog, {
                        control: scope.control
                    });

                }
            };
        });
}());