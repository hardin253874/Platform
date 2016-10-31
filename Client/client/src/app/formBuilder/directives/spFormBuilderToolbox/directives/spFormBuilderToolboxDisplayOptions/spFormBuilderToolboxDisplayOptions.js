// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing a form builder toolbox display options control.
    * spFormBuilderToolboxDisplayOptions provides the toolbox for interacting the miscellaneous display options on the canvas.
    *
    * @module spFormBuilderToolboxDisplayOptions
    * @example

    Using the spFormBuilderToolboxDisplayOptions:

    &lt;sp-form-builder-toolbox-display-options&gt;&lt;/sp-form-builder-toolbox-display-options&gt

    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxDisplayOptions', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.common.spCachingCompile'
    ])
        .directive('spFormBuilderToolboxDisplayOptions', function($q, spFormBuilderService, $timeout, spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    group: '=?'
                },                
                link: function(scope, element) {

                    /////
                    // Create a model.
                    /////
                    scope.model = {};

                    /////
                    // Gets a new container control.
                    /////
                    function getNewControlJson(controlTypeAlias) {
                        return {
                            typeId: controlTypeAlias,
                            name: jsonString(),
                            'console:renderingOrdinal': jsonInt(),
                            'console:renderingWidth': jsonInt(100),
                            'console:renderingHeight': jsonInt(100),
                            'console:renderingBackgroundColor': 'white',
                            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
                            'console:hideLabel': jsonBool(false),
                            'console:containedControlsOnForm': [
                            ]
                        };
                    }

                    function getContainer() {
                        var json = getNewControlJson('k:verticalStackContainerControl');
                        json.name = 'New Container';
                        return $q.when(spEntity.fromJSON(json));
                    }

                    function getTabbedContainer() {
                        var json = getNewControlJson('k:tabContainerControl');
                        json.name = spFormBuilderService.form.name;
                        json['console:renderingWidth'] = jsonInt();
                        json['console:renderingHeight'] = jsonInt();
                        return $q.when(spEntity.fromJSON(json));
                    }

                    function getHeroText() {
                        var json = getNewControlJson('heroTextControl');
                        json.name = 'Title';
                        json['console:renderingVerticalResizeMode'] = jsonLookup('console:resizeTwentyFive');
                        return $q.when(spEntity.fromJSON(json));
                    }

                    scope.$watch('group.isOpen', function(newVal, oldVal) {
                        if (newVal === oldVal) {
                            return;
                        }

                        if (newVal) {
                            var divScroll = element.closest('.layout-content');

                            if (divScroll && divScroll.length) {
                                $timeout(function() {
                                    //divScroll[0].scrollTop = divScroll[0].scrollHeight;
                                    divScroll.animate({ scrollTop: divScroll[0].scrollHeight });
                                }, 250);
                            }
                        }
                    });

                    /////
                    // Drag start event.
                    /////
                    function dragStart() {
                        spFormBuilderService.isDragging = true;
                    }

                    /////
                    // Drag end event.
                    /////
                    function dragEnd() {
                        spFormBuilderService.isDragging = false;
                        spFormBuilderService.destroyInsertIndicator();
                    }

                    var defaultDragOptions = {
                        onDragStart: function() {
                            dragStart();
                        },
                        onDragEnd: function() {
                            dragEnd();
                        }
                    };

                    /////
                    // Setup the groups and child controls within the display options pane.
                    /////
                    var displayOptionGroups = scope.displayOptionGroups || [
                        {
                            name: 'General Controls',
                            hidden: false,
                            displayOptions: [
                                {
                                    name: 'Hero Text',
                                    description: 'Display big summary figures.',
                                    icon: 'assets/images/16x16/herotext.png',
                                    dragOptions: defaultDragOptions,
                                    getRenderControlInstance: getHeroText,
                                    remove: scope.group.mode !== 'screen'
                                }
                            ]
                        },
                        {
                            name: 'Layout Controls',
                            hidden: false,
                            displayOptions: [
                                {
                                    name: 'Container',
                                    description: 'Group objects together so they can be treated like a single object.',
                                    icon: 'assets/images/16x16/fieldsgroup.png',
                                    dragOptions: defaultDragOptions,
                                    getRenderControlInstance: getContainer
                                },
                                {
                                    name: 'Tabbed Container',
                                    description: 'Group objects together within tabs so they can be treated like a single object.',
                                    icon: 'assets/images/16x16/tabs.png',
                                    dragOptions: defaultDragOptions,
                                    getRenderControlInstance: getTabbedContainer
                                }
                            ]
                        }
                    ];

                    // Remove items that should not be shown, and empty groups
                    _.forEach(displayOptionGroups, function (group) {
                        group.displayOptions = _.filter(group.displayOptions, function(opt) { return !opt.remove; });
                    });
                    displayOptionGroups = _.filter(displayOptionGroups, function(group) { return group.displayOptions.length; });
                    scope.model.displayOptionGroups = displayOptionGroups;

                    // Link
                    var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/directives/spFormBuilderToolboxDisplayOptions/spFormBuilderToolboxDisplayOptions.tpl.html');
                    cachedLinkFunc(scope, function (clone) {                        
                        element.append(clone);
                    });
                }
            };
        });
}());