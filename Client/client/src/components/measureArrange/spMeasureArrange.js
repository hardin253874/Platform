// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function () {
    'use strict';

    /**
     * Directive implementing measure and arrange logic.
     * spMeasureArrange runs a multi-pass algorithm that first measures elements and then rearranges them.
     *
     * @module spMeasureArrange
     * @example

     Using the spMeasureArrange:

     &lt;div sp-measure-arrange="options" &gt;&lt;/div&gt

     where options is an object with the following properties:
     TBA
     */
    angular.module('mod.common.ui.spMeasureArrange', [
        'mod.app.formBuilder.services.spFormBuilderService'
    ]);

    angular.module('mod.common.ui.spMeasureArrange')
        .service('spMeasureArrangeService', spMeasureArrangeService)
        .directive('spMeasureArrange', spMeasureArrangeDirective);

    /* @ngInject */
    function spMeasureArrangeService() {
        /////
        // Service that coordinates multiple spMeasureArrange directives simultaneously.
        /////
        var exports = {};

        /////
        // Subscribers.
        /////
        exports.subscribers = {};

        /////
        // Perform layout for a particular subscriber.
        /////
        exports.performLayout = function (id) {
            exports.subscribers[id].revision++;
        };

        /////
        // Reset all subscribers.
        /////
        exports.reset = function () {
            exports.subscribers = {};
        };

        /////
        // Register subscriber.
        /////
        exports.register = function (id) {
            exports.subscribers[id] = {
                revision: 0
            };
        };

        /////
        // Get subscriber revision.
        /////
        exports.getRevision = function (id) {
            return exports.subscribers[id] && exports.subscribers[id].revision;
        };

        return exports;
    }

    /* @ngInject */
    function spMeasureArrangeDirective($rootScope, spMeasureArrangeService, spFormBuilderService) {
        return {
            restrict: 'A',
            transclude: false,
            replace: true,
            scope: false,
            link: link
        };

        function link(scope, element, attrs) {

            /////
            // Dimensions.
            /////
            var dimensions = {
                horizontal: 'horizontal',
                vertical: 'vertical',
                both: 'both'
            };

            /////
            // Resize modes.
            /////
            var resizeModes = {
                automatic: 'automatic',
                manual: 'manual',
                spring: 'spring'
            };

            /////
            // Display modes.
            /////
            var displayModes = {
                block: 'block',
                inlineBlock: 'inline-block'
            };

            var options = {};

            scope.spMeasureArrangeService = spMeasureArrangeService;

            attrs.$observe('spMeasureArrange', function (val) {
                if (val) {

                    /////
                    // Get the attributes from the element.
                    /////
                    options = scope.$eval(attrs.spMeasureArrange) || {};

                    if (options.id) {
                        spMeasureArrangeService.register(options.id);
                    }
                }
            });

            /////
            // When the revision for this subscriber changes, perform the measure arrange.
            /////
            scope.$watch(function () {
                return spMeasureArrangeService.getRevision(options.id);
            }, function (newRevision, oldRevision) {
                if (newRevision === oldRevision) {
                    return;
                }

                measureArrange();
            });

            /////
            // Measure and Arrange
            /////
            function measureArrange() {
                var structure;

                /////
                // Run phase 1
                /////
                structure = gather();

                if (structure) {
                    /////
                    // Run phase 2
                    /////
                    measure(structure);

                    /////
                    // Run phase 3
                    /////
                    arrange(structure);
                }

                complete();
            }

            /////
            // Gather the required information.
            /////
            function gather() {
                var structure = {
                    map: {},
                    children: []
                };

                /////
                // Is the specified control contained within an explicit parent. (affects padding)
                /////
                function isControlInExplicitContainer(parent) {
                    if (!parent) {
                        return false;
                    }

                    var controlInExplicitContainer = false;
                    var parentAlias;

                    while (parent) {
                        parentAlias = parent.type.getAlias();
                        if ((parent.name && parentAlias !== spFormBuilderService.containers.form && parentAlias !== spFormBuilderService.containers.screen) || (parentAlias === spFormBuilderService.containers.header)) {
                            controlInExplicitContainer = true;
                            break;
                        }

                        parent = structure.map[parent.id()].parentControl;
                    }

                    return controlInExplicitContainer;
                }

                /////
                // Get the desired display mode.
                /////
                function getDisplayMode(parentControl) {
                    if (!parentControl) {
                        return displayModes.block;
                    }

                    var parentAlias = parentControl.type.getAlias();

                    if (spFormBuilderService.isHorizontalContainer(parentAlias)) {
                        return displayModes.inlineBlock;
                    } else {
                        return displayModes.block;
                    }
                }

                /////
                // Get the resize mode based on alias.
                /////
                function getResizeMode(alias) {
                    switch (alias) {
                        case 'resizeHundred':
                        case 'resizeTwentyFive':
                        case 'resizeThirtyThree':
                        case 'resizeFifty':
                        case 'resizeSixtySix':
                        case 'resizeSeventyFive':
                        case 'resizeManual':
                            return resizeModes.manual;
                        case 'resizeSpring':
                            return resizeModes.spring;
                        default:
                            return resizeModes.automatic;
                    }
                }

                /////
                // The gather callback.
                /////
                function gatherCallback(control, parentControl, domElement, options) {
                    if (!control) {
                        return;
                    }

                    if (_.has(structure.map, control.id())) {
                        var ctrl = structure.map[control.id()];
                        if (ctrl && ctrl.parentControl === parentControl && ctrl.control === control) {
                            ctrl.repeatedElements.push(domElement);
                            return;
                        }
                    }

                    var controlAlias = control.type.getAlias();

                    /////
                    // Setup the structure control.
                    /////
                    var structureControl = {
                        control: control,
                        parentControl: parentControl,
                        domElement: domElement,
                        titleClass: options && options.titleClass,
                        contentClass: options && options.contentClass,
                        lineBreakClass: options && options.lineBreakClass,
                        children: [],
                        display: getDisplayMode(parentControl),
                        inlineElements: options && options.inlineElements,
                        repeatedElements: [],
                        isField: !!(control.fieldToRender || control.relationshipToRender),
                        isContainer: spFormBuilderService.isHorizontalContainer(controlAlias) || spFormBuilderService.isVerticalContainer(controlAlias),
                        isTabContainer: spFormBuilderService.isTabContainer(controlAlias),
                        isImplicitContainer: spFormBuilderService.isImplicitContainer(control),
                        isExplicitContainer: spFormBuilderService.isExplicitContainer(control),
                        isHorizontalContainer: spFormBuilderService.isHorizontalContainer(controlAlias),
                        isVerticalContainer: spFormBuilderService.isVerticalContainer(controlAlias),
                        size: {
                            height: undefined,
                            width: undefined
                        },
                        minSize: {
                            height: undefined,
                            width: undefined
                        },
                        resizeMode: {
                            vertical: (control && control.renderingVerticalResizeMode && control.renderingVerticalResizeMode._id && getResizeMode(control.renderingVerticalResizeMode._id.getAlias())) || (spFormBuilderService.isContainer(controlAlias) && resizeModes.automatic) || resizeModes.automatic,
                            horizontal: (control && control.renderingHorizontalResizeMode && control.renderingHorizontalResizeMode._id && getResizeMode(control.renderingHorizontalResizeMode._id.getAlias())) || (spFormBuilderService.isContainer(controlAlias) && resizeModes.spring) || resizeModes.automatic
                        }
                    };

                    //console.log('gather for ', control.debugString, 'resize', structureControl.resizeMode);

                    /////
                    // Attach the parent structure.
                    /////
                    if (parentControl) {
                        var parentStructureControl = structure.map[parentControl.id()];

                        if (parentStructureControl) {
                            parentStructureControl.children.push(structureControl);
                        } else {
                            console.error('Unknown parent entity');
                        }
                    } else {
                        structure.children.push(structureControl);
                    }

                    /////
                    // Implicit containers handle their display mode differently.
                    /////
                    if (structureControl.isImplicitContainer) {
                        if (structureControl.isHorizontalContainer) {
                            structureControl.display = displayModes.block;
                        } else {
                            structureControl.display = displayModes.inlineBlock;
                        }

                    }

                    /////
                    // Cache the content div so that the overflow can be disabled/enabled.
                    /////
                    if (structureControl.isContainer) {
                        if (structureControl.contentClass) {
                            var firstContentControl = structureControl.domElement.find(structureControl.contentClass).first();

                            if (firstContentControl) {
                                structureControl.firstContentControl = firstContentControl;
                            }
                        }
                    }

                    /////
                    // Ensure the control has a renderingVerticalResizeMode and it is set appropriately.
                    /////
                    if (!control.renderingVerticalResizeMode) {
                        control.registerLookup('console:renderingVerticalResizeMode');

                        control.setRenderingVerticalResizeMode('console:resizeAutomatic');
                    }

                    /////
                    // Ensure the control has a renderingHorizontalResizeMode and it is set appropriately.
                    /////
                    if (!control.renderingHorizontalResizeMode) {
                        control.registerLookup('console:renderingHorizontalResizeMode');

                        if (spFormBuilderService.isContainer(controlAlias)) {
                            control.setRenderingHorizontalResizeMode('console:resizeSpring');
                        } else {
                            control.setRenderingHorizontalResizeMode('console:resizeAutomatic');
                        }
                    }

                    /////
                    // Add the control to the fast lookup map.
                    /////
                    structure.map[control.id()] = structureControl;
                }

                /////
                // Walk the entire structure gathering information for all scopes that subscribe to the 'gather' event.
                /////
                scope.$broadcast('gather', gatherCallback);

                return structure;
            }

            /////
            // Perform the measure.
            /////
            function measure(structure) {

                /////
                // Pass 1 calculates the minimum size of each control using post-fix traversal.
                /////
                function pass1(structureControl) {
                    if (!structureControl) {
                        return;
                    }

                    /////
                    // Get the structure controls minimum viable size.
                    /////
                    function getMinSize(dimension) {
                        if (!structureControl || !dimension) {
                            return;
                        }

                        var cssMinHeight;
                        var cssMinWidth;

                        if (dimension === dimensions.horizontal) {
                            cssMinWidth = structureControl.domElement.css('min-width');

                            if (!structureControl.domElement[0].originalMinWidth) {
                                structureControl.domElement[0].originalMinWidth = cssMinWidth;
                            }
                        }

                        if (dimension === dimensions.vertical) {
                            cssMinHeight = structureControl.domElement.css('min-height');

                            if (!structureControl.domElement[0].originalMinHeight) {
                                structureControl.domElement[0].originalMinHeight = cssMinHeight;
                            }
                        }

                        var min;
                        var childIndex = 0;
                        var child;
                        var aggregateMin = 0;
                        var parentPadding = 0;

                        if (dimension === dimensions.vertical) {
                            min = parseFloat(structureControl.domElement[0].originalMinHeight) || 0;

                            if (structureControl.children && structureControl.children.length) {

                                parentPadding = structureControl.domElement.outerHeight() - structureControl.domElement.height();

                                if (structureControl.lineBreakClass) {
                                    var lineBreak = structureControl.domElement.find(structureControl.lineBreakClass).first();

                                    if (lineBreak && lineBreak.length) {
                                        parentPadding += 17;
                                    }
                                }

                                var alias = structureControl.control.type.getAlias();

                                if (structureControl.isExplicitContainer && !structureControl.control.hideLabel && alias !== spFormBuilderService.containers.screen && alias !== spFormBuilderService.containers.form) {
                                    if (structureControl.titleClass) {
                                        var label = structureControl.domElement.find(structureControl.titleClass).first();

                                        if (label && label.length) {
                                            parentPadding += label.outerHeight();
                                        }
                                    }
                                }

                                if (structureControl.isHorizontalContainer || structureControl.isTabContainer) {

                                    for (childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                                        child = structureControl.children[childIndex];

                                        if (child.minSize.height + parentPadding > min) {
                                            min = child.minSize.height + parentPadding;
                                        }
                                    }
                                } else {
                                    for (childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                                        child = structureControl.children[childIndex];

                                        aggregateMin += child.minSize.height;
                                    }

                                    if (aggregateMin + parentPadding > min) {
                                        min = aggregateMin + parentPadding;
                                    }
                                }
                            }

                            return min;
                        } else {
                            min = parseFloat(structureControl.domElement[0].originalMinWidth) || 0;

                            if (structureControl.children && structureControl.children.length) {

                                parentPadding = structureControl.domElement.outerWidth() - structureControl.domElement.width();

                                for (childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                                    child = structureControl.children[childIndex];

                                    if (child.minSize.width + parentPadding > min) {
                                        min = child.minSize.width + parentPadding;
                                    }
                                }
                            }

                            return min;
                        }
                    }

                    /////
                    // Temporarily disable overflow. (prefix)
                    /////
                    if (structureControl.isContainer) {

                        structureControl.domElement.css('overflow-x', 'hidden');

                        if (structureControl.firstContentControl) {
                            structureControl.firstContentControl.css('overflow-x', 'hidden');
                        }
                    }

                    /////
                    // Recursive.
                    /////
                    if (structureControl.children && structureControl.children.length) {
                        for (var childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                            var child = structureControl.children[childIndex];

                            pass1(child);
                        }
                    }

                    /////
                    // Calculate the controls minimum size. (postfix)
                    /////
                    structureControl.minSize.height = getMinSize(dimensions.vertical) || 0;
                    structureControl.minSize.width = getMinSize(dimensions.horizontal) || 0;

                    if (structureControl.isField) {
                        structureControl.minSize.height += (structureControl.domElement.outerHeight() - structureControl.domElement.height());
                        structureControl.minSize.width += (structureControl.domElement.outerWidth() - structureControl.domElement.width());
                    }
                }

                /////
                // Pass 2 calculates the height and width of non-spring controls.
                /////
                function pass2(structureControl, dimension) {
                    if (!structureControl || !dimension) {
                        return;
                    }

                    /////
                    // Ensure the rendering height/width is set correctly. (based on the alias)
                    /////
                    function updateRenderDimension(dimension) {
                        if (!structureControl || !dimension) {
                            return;
                        }

                        var alias;

                        if (dimension === dimensions.horizontal && structureControl.control.renderingHorizontalResizeMode) {
                            alias = structureControl.control.renderingHorizontalResizeMode._id.getAlias();
                        } else if (dimension === dimensions.vertical && structureControl.control.renderingVerticalResizeMode) {
                            alias = structureControl.control.renderingVerticalResizeMode._id.getAlias();
                        }

                        var value;

                        if (alias) {
                            switch (alias) {
                                case 'resizeHundred':
                                    value = 100;
                                    break;
                                case 'resizeTwentyFive':
                                    value = 25;
                                    break;
                                case 'resizeThirtyThree':
                                    value = 33;
                                    break;
                                case 'resizeFifty':
                                    value = 50;
                                    break;
                                case 'resizeSixtySix':
                                    value = 66;
                                    break;
                                case 'resizeSeventyFive':
                                    value = 75;
                                    break;
                            }
                        }

                        if (value) {
                            if (dimension === dimensions.horizontal) {
                                structureControl.control.renderingWidth = value;
                            } else {
                                structureControl.control.renderingHeight = value;
                            }
                        }
                    }

                    updateRenderDimension(dimension);

                    /////
                    // Depth first layout.
                    /////
                    if (structureControl.children && structureControl.children.length) {
                        for (var childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                            var child = structureControl.children[childIndex];

                            pass2(child, dimension);
                        }
                    }

                    var resizeMode = structureControl.resizeMode[dimension];
                    var padding = 0;

                    switch (resizeMode) {
                        case resizeModes.automatic:
                            if (dimension === dimensions.horizontal) {
                                structureControl.size.width = undefined;
                            } else {
                                structureControl.size.height = undefined;
                            }

                            break;
                        case resizeModes.manual:
                            if (dimension === dimensions.horizontal) {

                                padding = structureControl.domElement.outerWidth(true) - structureControl.domElement.outerWidth();

                                if (structureControl.parentControl && spFormBuilderService.isHorizontalContainer(structureControl.parentControl.type.getAlias())) {
                                    padding += 3;
                                }

                                if (padding) {
                                    structureControl.size.width = 'calc(' + structureControl.control.renderingWidth.toString() + '% - ' + padding.toString() + 'px)';
                                } else {
                                    structureControl.size.width = structureControl.control.renderingWidth.toString() + '%';
                                }
                            } else {
                                padding = structureControl.domElement.outerHeight(true) - structureControl.domElement.outerHeight();

                                if (padding) {
                                    structureControl.size.height = 'calc(' + structureControl.control.renderingHeight.toString() + '% - ' + padding.toString() + 'px)';
                                } else {
                                    structureControl.size.height = structureControl.control.renderingHeight.toString() + '%';
                                }
                            }

                            break;
                    }
                }

                /////
                // Pass 3 calculates the height and width of spring controls.
                /////
                function pass3(structureControl, dimension) {
                    if (!structureControl || !dimension) {
                        return;
                    }

                    /////
                    // Determine the maximum size of the control. (structureControl and dimension are captured in the closure).
                    /////
                    function determineMaxSize() {
                        if (!structureControl) {
                            return;
                        }

                        var result = 'calc(100%';

                        var margin = 0;

                        if (dimension === dimensions.vertical) {
                            margin += parseFloat(structureControl.domElement.css('margin-top')) + parseFloat(structureControl.domElement.css('margin-bottom'));
                        } else {
                            margin += 3;

                            margin += parseFloat(structureControl.domElement.css('margin-left')) + parseFloat(structureControl.domElement.css('margin-right'));
                        }

                        if (margin) {
                            result += ' - ' + margin.toString() + 'px';
                        }

                        result += ')';

                        return result;
                    }

                    /////
                    // Determine the spring size of the control. (structureControl and dimension are captured in the closure).
                    /////
                    function determineSpringSize(autoControls, manualControls, springControls) {
                        if (!dimension || !autoControls || !manualControls || !springControls) {
                            return;
                        }

                        var manualSiblings = 0;
                        var autoSiblings = 0;
                        var springSize = 0;

                        var margin = 0;

                        for (var manualControlIndex = 0; manualControlIndex < manualControls.length; manualControlIndex++) {
                            var manualControl = manualControls[manualControlIndex];

                            manualSiblings += dimension === dimensions.horizontal ? manualControl.control.renderingWidth : manualControl.control.renderingHeight;

                            if (dimension === dimensions.vertical) {
                                margin += parseFloat(manualControl.domElement.css('margin-top')) + parseFloat(manualControl.domElement.css('margin-bottom'));
                            } else {
                                margin += parseFloat(manualControl.domElement.css('margin-left')) + parseFloat(manualControl.domElement.css('margin-right'));
                            }
                        }

                        for (var autoControlIndex = 0; autoControlIndex < autoControls.length; autoControlIndex++) {
                            var autoControl = autoControls[autoControlIndex];

                            autoSiblings += dimension === dimensions.horizontal ? autoControl.minSize.width : autoControl.minSize.height;

                            if (dimension === dimensions.vertical) {
                                margin += parseFloat(autoControl.domElement.css('margin-top')) + parseFloat(autoControl.domElement.css('margin-bottom'));
                            } else {
                                margin += parseFloat(autoControl.domElement.css('margin-left')) + parseFloat(autoControl.domElement.css('margin-right'));
                            }
                        }

                        var parentPadding = 0;

                        if (structureControl.parentControl) {
                            var parentStructureControl = structure.map[structureControl.parentControl.id()];

                            if (parentStructureControl) {

                                var alias = parentStructureControl.control.type.getAlias();

                                if (parentStructureControl.isExplicitContainer && !parentStructureControl.control.hideLabel && alias !== spFormBuilderService.containers.screen && alias !== spFormBuilderService.containers.form) {
                                    if (parentStructureControl.titleClass) {
                                        var label = parentStructureControl.domElement.find(parentStructureControl.titleClass).first();

                                        if (label && label.length) {
                                            parentPadding = label.outerHeight();
                                        }
                                    }
                                }
                            }
                        }


                        if (springControls.length) {
                            springSize = Math.floor(100 / springControls.length);
                        }

                        var result = 'calc(' + springSize.toString() + '%';

                        if (manualSiblings) {
                            result += ' - ' + Math.ceil(manualSiblings / springControls.length).toString() + '%';
                        }

                        if (autoSiblings) {
                            result += ' - ' + Math.ceil(autoSiblings / springControls.length).toString() + 'px';
                        }

                        if (dimension === dimensions.vertical) {
                            margin += parseFloat(structureControl.domElement.css('margin-top')) + parseFloat(structureControl.domElement.css('margin-bottom'));
                        } else {
                            margin += 3;

                            margin += parseFloat(structureControl.domElement.css('margin-left')) + parseFloat(structureControl.domElement.css('margin-right'));
                        }

                        if (margin) {
                            result += ' - ' + margin.toString() + 'px';
                        }

                        if (parentPadding) {
                            result += ' - ' + parentPadding.toString() + 'px';
                        }

                        result += ')';

                        return result;
                    }

                    if (structureControl.children && structureControl.children.length) {
                        for (var childIndex = 0; childIndex < structureControl.children.length; childIndex++) {
                            var child = structureControl.children[childIndex];

                            pass3(child, dimension);
                        }
                    }

                    var resizeMode = structureControl.resizeMode[dimension];

                    switch (resizeMode) {
                        case resizeModes.spring:
                            var siblings = [];                            
                            
                            if (structureControl.parentControl &&
                                !spFormBuilderService.isTabContainer(structureControl.parentControl.type.getAlias())) {                                
                                siblings = _.without(structure.map[structureControl.parentControl.id()].children, structureControl);
                            }

                            var autoControls = [];
                            var manualControls = [];
                            var springControls = [structureControl];

                            for (var siblingIndex = 0; siblingIndex < siblings.length; siblingIndex++) {
                                var sibling = siblings[siblingIndex];

                                var siblingResizeMode = sibling.resizeMode[dimension];

                                switch (siblingResizeMode) {
                                    case resizeModes.automatic:
                                        autoControls.push(sibling);
                                        break;
                                    case resizeModes.manual:
                                        manualControls.push(sibling);
                                        break;
                                    case resizeModes.spring:
                                        springControls.push(sibling);
                                        break;
                                }
                            }

                            var springSize;

                            if (dimension === dimensions.horizontal) {
                                if (structureControl.parentControl && spFormBuilderService.isHorizontalContainer(structureControl.parentControl.type.getAlias())) {
                                    springSize = determineSpringSize(autoControls, manualControls, springControls);
                                } else {
                                    springSize = determineMaxSize();
                                }

                                structureControl.size.width = springSize;
                            } else {
                                if (!structureControl.parentControl || spFormBuilderService.isVerticalContainer(structureControl.parentControl.type.getAlias())) {
                                    springSize = determineSpringSize(autoControls, manualControls, springControls);
                                } else {
                                    springSize = determineMaxSize();
                                }

                                structureControl.size.height = springSize;
                            }

                            break;
                    }
                }

                /////
                // Run a specific pass.
                /////
                function runPass(pass, dimension) {
                    var rootIndex;

                    for (rootIndex = 0; rootIndex < structure.children.length; rootIndex++) {
                        switch (dimension) {
                            case dimensions.horizontal:
                                pass(structure.children[rootIndex], dimensions.horizontal);
                                break;
                            case dimensions.vertical:
                                pass(structure.children[rootIndex], dimensions.vertical);
                                break;
                            case dimensions.both:
                                pass(structure.children[rootIndex], dimensions.horizontal);
                                pass(structure.children[rootIndex], dimensions.vertical);
                                break;
                            default:
                                pass(structure.children[rootIndex]);
                        }
                    }
                }

                runPass(pass1);
                runPass(pass2, dimensions.both);
                runPass(pass3, dimensions.both);

                return structure;
            }

            /////
            // Perform the arrangement.
            /////
            function arrange(structure) {

                /////
                // Set a style on the actual element for the control and any repeated elements.
                /////
                function setStyle(ctrl, name, value) {

                    if (!ctrl || !ctrl.domElement) {
                        return;
                    }

                    ctrl.domElement.css(name, value);

                    if (ctrl.repeatedElements && ctrl.repeatedElements.length) {
                        _.each(ctrl.repeatedElements, function (el) {

                            el.css(name, value);
                        });
                    }
                }

                if (!structure) {
                    return;
                }

                var mapKey;
                var structureControl;
                var inHorizontalContainer;
                var childIndex = 0;
                var child;
                var alias;

                for (mapKey in structure.map) {
                    structureControl = structure.map[mapKey];

                    inHorizontalContainer = structureControl.parentControl && spFormBuilderService.isHorizontalContainer(structureControl.parentControl.type.getAlias());

                    if (structureControl.minSize.height) {
                        setStyle(structureControl, 'min-height', structureControl.minSize.height + 'px');
                    }

                    if (structureControl.minSize.width) {
                        // console.log('arrange:', inHorizontalContainer, structureControl.control.debugString, structureControl);
                        if (inHorizontalContainer) {
                            setStyle(structureControl, 'min-width', (structureControl.minSize.width - 5) + 'px');
                        } else {
                            setStyle(structureControl, 'min-width', structureControl.minSize.width + 'px');
                        }
                    }

                    if (structureControl.display) {
                        if (structureControl.isImplicitContainer) {
                            setStyle(structureControl, 'display', 'inline');

                            if (structureControl.inlineElements) {
                                var childElement = structureControl.domElement;
                                var elements = structureControl.inlineElements;

                                while (elements) {
                                    childElement = childElement.children(elements.selector);

                                    if (childElement && childElement.length) {
                                        childElement.css('display', 'inline');
                                    }

                                    elements = elements.inlineElements;
                                }
                            }
                        } else {
                            setStyle(structureControl, 'display', structureControl.display);

                        }
                    }

                    if (structureControl.size.height) {
                        setStyle(structureControl, 'height', structureControl.size.height);
                    } else {
                        alias = structureControl.control.type.getAlias();

                        if (alias === spFormBuilderService.containers.form || alias === spFormBuilderService.containers.screen) {
                            setStyle(structureControl, 'height', '100%');
                        } else {
                            if (!structureControl.isImplicitContainer) {
                                setStyle(structureControl, 'height', structureControl.minSize.height + 'px');
                            }
                        }
                    }

                    if (structureControl.size.width) {
                        setStyle(structureControl, 'width', structureControl.size.width);
                    } else {
                        alias = structureControl.control.type.getAlias();

                        if (alias === spFormBuilderService.containers.form || alias === spFormBuilderService.containers.screen) {
                            setStyle(structureControl, 'width', '100%');
                        } else {
                            if (inHorizontalContainer) {
                                setStyle(structureControl, 'width', (structureControl.minSize.width - 5) + 'px');
                            } else {
                                setStyle(structureControl, 'width', structureControl.minSize.width + 'px');
                            }
                        }
                    }
                }
            }

            /////
            // Fire the 'measureArrangeComplete' message.
            /////
            function complete() {
                $rootScope.$broadcast('measureArrangeComplete', {source: scope});
            }
        }
    }
}());