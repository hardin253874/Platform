// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing a form builder toolbox control.
    * spFormBuilderToolbox provides the toolbox for interacting with the canvas.
    *
    * @module spFormBuilderToolbox
    * @example

    Using the spFormBuilderToolbox:

    &lt;sp-form-builder-toolbox&gt;&lt;/sp-form-builder-toolbox&gt
        
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolbox', ['mod.common.spCachingCompile'])
        .directive('spFormBuilderToolbox', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    mode: '@mode'
                },                
                link: function(scope, element) {

                    /////
                    // Set the top level groups in the toolbox
                    /////
                    scope.model = {
                        groups: [
                            {
                                name: 'Relationship Viewer',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/relationshipViewer.tpl.html',
                                modes: ['form'],
                                hidden: true
                            },
                            {
                                name: 'Fields and Related Objects',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/fieldViewer.tpl.html',
                                isOpen: true,
                                modes: ['form']
                            },
                            {
                                name: 'Objects',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/objectViewer.tpl.html',
                                isOpen: true,
                                modes: ['adminToolbox']
                            },
                            {
                                name: 'Choice Fields',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/enumTypeViewer.tpl.html',
                                isOpen: false,
                                modes: ['adminToolbox']
                            },
                            {
                                name: 'Objects',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/objectViewer.tpl.html',
                                isOpen: true,
                                modes: ['screen']
                            },/*
                            {
                                name: 'Workflows',
                                mode: 'all'
                            },*/
                            {
                                name: 'Display Options',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/displayOptions.tpl.html',
                                isOpen: true,
                                modes: ['form', 'screen'],
                                mode: scope.mode
                            },
                            {
                                name: 'Custom',
                                template: 'formBuilder/directives/spFormBuilderToolbox/templates/customControlsViewer.tpl.html',
                                isOpen: false,
                                modes: ['form'],
                                hidden: true
                            }
                        ]
                    };                    

                    /////
                    // Filter to applicable options
                    /////
                    scope.model.groups = _.filter(scope.model.groups, function(g) {
                        return g.modes.indexOf(scope.mode) !== -1;
                    });

                    /////
                    // Set the toolbox title
                    /////
                    scope.toolboxTitle = scope.mode === 'screen' ? 'Screen' : (scope.mode === 'adminToolbox' ? 'Application Toolbox' : 'Form');

                    /////
                    // Ability to hide tabs but still have them run.
                    /////
                    scope.getGroupStyle = function(group) {
                        var style = {};

                        if (group && group.hidden) {

                            style.visibility = 'hidden';
                            style.display = 'none';
                        }

                        return style;
                    };

                    scope.$on('toolboxGroupVisibilityChanged', function (event, info) {
                        event.stopPropagation();
                        var group = _.find(scope.model.groups, { name: info.groupName });
                        if (group) {
                            group.hidden = info.hidden;
                        }
                    });
                    
                    var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/spFormBuilderToolbox.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());