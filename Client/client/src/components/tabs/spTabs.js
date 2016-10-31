// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, sp, spEntity */

(function () {
    'use strict';

    angular.module('mod.common.ui.spTabs', [
        'mod.common.spUuidService', 'mod.app.formBuilder.services.spFormBuilderService', 'mod.common.ui.spMeasureArrange', 'sp.themeService', 'mod.common.ui.spDialogService', 'mod.common.ui.rnInfoButton'
    ]);

    angular.module('mod.common.ui.spTabs')
        .directive('spTabs', spTabs);

    /* @ngInject */
    function spTabs($compile, $document, $window, $timeout, spUuidService, spFormBuilderService, spMeasureArrangeService, spThemeService, spDialogService, spEditForm) {
        return {
            restrict: 'E',
            templateUrl: 'tabs/spTabs.tpl.html',
            scope: {
                items: '=',
                parentControl: '=',
                isInDesign: '=?'
            },
            link: link
        };

        function link(scope) {

            scope.visitedTabs = [];
            var currentDropTarget = null,
                body = $document.find('body'),
                currentInsertionIndicatorPosition = null,
                insertIndicatorElement,
                insertIndicatorScope,
                insertionIndicatorPosition = {
                    before: 'before',
                    after: 'after'
                };

            // Set the drop data.
            scope.insertIndicatorDropData = {};

            /////
            // Watch the items collection.
            /////
            scope.$watch('items', function () {
                if (scope.items) {

                    /////
                    // create an unique id for each tab
                    /////
                    _.forEach(scope.items, function (item) {
                        if (!item.id) {
                            item.id = spUuidService.create();
                        }
                    });
                }
            });

            /////
            // Selection changed event handler.
            /////
            scope.selectionChanged = function (tab) {
                if (tab && tab.id) {

                    if (!_.includes(scope.visitedTabs, tab.id)) {

                        // do layout calc here only once per tab

                        // do not run layout calc the very first time as it would be done by the root element be editForm or editFormBuilderContainer etc..
                        // run it once for every subsequent clicked tab. (incase of formbuilder, if there is any change in controls inside an already visited tab, editFormBuilderContainer runs layout calc.
                        if (scope.visitedTabs.length > 0) {
                            scope.$emit('calcTabsLayout', calcLayoutCallback);
                        }

                        scope.visitedTabs.push(tab.id);
                    } else {
                        // force refresh the grid on selected tab. *only if this tab has already been visited. if visiting the first time, grid will get refreshed on measureArrangeComplete event
                        if (scope.visitedTabs.length > 0) {
                            var controlAlias = sp.result(tab, 'model.formControl.containedControlsOnForm[0].type.nsAlias');
                            if (controlAlias === 'console:subjectRecordAccessEditor') {
                                forceRebuildRecordAccess();
                            } else {
                                forceRebuildGrid();
                            }
                        }
                    }

                    if (tab.selectionChanged) {     // if a selection changed handler is defined on 'tab' item then invoke it
                        tab.selectionChanged(tab);
                    }
                }
            };

            /////
            // Tab has been visited.
            /////
            scope.tabVisited = function (tab) {

                if (tab && tab.id) {
                    if (_.includes(scope.visitedTabs, tab.id)) {
                        return true;
                    }
                }
                return false;
            };

            ///
            // Callback to calculate tab layout.
            ///
            function calcLayoutCallback(measureArrangeId) {
                if (measureArrangeId) {
                    doLayout(measureArrangeId);
                }
            }

            ///
            // Calculate tab layout
            ///
            function doLayout(measureArrangeId) {
                $timeout(function () {
                    spMeasureArrangeService.performLayout(measureArrangeId);
                });
            }

            ///
            // Force refresh the grid(s) on active tab(s)
            ///
            function forceRebuildGrid() {
                $timeout(function () {
                    scope.$broadcast('forceRebuildGrid');
                });
            }

            function forceRebuildRecordAccess(){
                $timeout(function () {
                    scope.$broadcast('forceRebuildRecordAccess');
                });
            }

            /////
            // Event handler for removing a tab.
            /////
            scope.removeTab = function (tab) {
                if (tab.model.removeCallback) {
                    tab.model.removeCallback(tab);
                }
            };

            // Filter the tabs by data state.
            scope.filterByDataState = function (tab) {
                if (!tab || !tab.model || !tab.model.formControl) {
                    return true;
                }

                return tab.model.formControl.getDataState() !== spEntity.DataStateEnum.Delete;
            };

            /////
            // Event handler for configuring a tab.
            /////
            scope.configureTab = function (tab) {
                if (tab.model.configureCallback) {

                    var options = {
                        isTab: true
                    };

                    tab.model.configureCallback(tab.model.formControl, options);
                }
            };

            scope.showHelp = function (tab) {
                var showHelp = false;
                if (!scope.isInDesign &&
                    tab &&
                    tab.model &&
                    tab.model.formControl &&
                    tab.model.formControl.showControlHelpText &&
                    tab.model.formControl.description) {
                    showHelp = true;
                }
                return showHelp;                                
            };

            scope.openDetail = openDetail;

            function openDetail(formControl, templateUrl) {
                var titleModel = spEditForm.createTitleModel(formControl, scope.isInDesign);
                var defaults = {
                    templateUrl: templateUrl,
                    controller: ['$scope', '$uibModalInstance', 'fieldTitle', 'fieldValue', modalInstanceCtrl],
                    resolve: {
                        fieldTitle: function () {
                            return titleModel ? titleModel.name : '';
                        },
                        fieldValue: function () {
                            return formControl ? formControl.description : '';
                        }
                    }
                };

                var options = {};

                spDialogService.showDialog(defaults, options);
            }

            function modalInstanceCtrl(scope, $uibModalInstance, fieldTitle, fieldValue) {
                scope.model = {};
                scope.model.fieldTitle = fieldTitle;
                scope.model.fieldValue = fieldValue;

                scope.clearFieldValue = function () {
                    scope.model.fieldValue = '';
                };

                scope.closeDetail = function () {
                    $uibModalInstance.close(scope.model);
                };
            }

            ////
            // Event handler for adding a tab.
            ////
            scope.addTab = function () {
                if (scope.parentControl && scope.parentControl.containedControlsOnForm) {
                    var newStackContainer = spFormBuilderService.createHiddenStackContainer(spFormBuilderService.containers.vertical);
                    newStackContainer.setRenderingVerticalResizeMode('console:resizeSpring');
                    newStackContainer.renderingBackgroundColor = 'transparent';
                    newStackContainer.name = 'New Tab';
                    scope.parentControl.containedControlsOnForm.add(newStackContainer);
                }
            };

            //TAB Theme style
            scope.getTabStyle = function (tab) {
                var tabStyle = {};
                if (spThemeService && spThemeService.getConsoleTheme()) {
                    if (document.getElementById('spTabSet') && document.getElementById('spTabSet').getElementsByTagName('ul')[0]) {
                        var headingLineColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaContainerHeadingLineColor;
                        if (headingLineColor) {
                            document.getElementById('spTabSet').getElementsByTagName('ul')[0].style.borderBottomColor = sp.getCssColorFromARGBString(headingLineColor);
                        }
                    }
                }

                return tabStyle;
            };

            
            //TAB Heading Theme style
            scope.getTabHeadingStyle = function (tab) {
                var tabHeadingStyle = {};
                if (spThemeService && spThemeService.getConsoleTheme()) {
                    if (tab.isActive) {
                        var selectedTabFontColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaSelectedTabFontColor;
                        if (selectedTabFontColor) {
                            tabHeadingStyle['color'] = sp.getCssColorFromARGBString(selectedTabFontColor);
                        }

                        var selectedTabColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaSelectedTabColor;
                        if (selectedTabColor) {
                            tabHeadingStyle['border-color'] = sp.getCssColorFromARGBString(selectedTabColor);

                        }
                    } else {
                        var unSelectedTabFontColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaUnselectedTabFontColor;
                        if (unSelectedTabFontColor) {
                            tabHeadingStyle['color'] = sp.getCssColorFromARGBString(unSelectedTabFontColor);
                        }

                        var unSelectedTabColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaUnselectedTabColor;
                        if (unSelectedTabColor) {
                            tabHeadingStyle['border-color'] = sp.getCssColorFromARGBString(unSelectedTabColor);

                        }
                    }

                }
                return tabHeadingStyle;
            };

            scope.getTabStyleClass = function (tab) {

                if (spThemeService && spThemeService.getConsoleTheme()) {
                    var selectedTabColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaSelectedTabColor;
                    var unSelectedTabColor = spThemeService.getConsoleTheme().consoleGeneralContentAreaUnselectedTabColor;
                    if (selectedTabColor || unSelectedTabColor) {
                        return '';
                    } else {
                        return 'tabHeading';
                    }
                } else {
                    return 'tabHeading';
                }
            };


            ///
            // Drag options.
            ///
            scope.dragOptions = {
                onDragEnd: function (event, data) {
                    dragEnd(event, data);
                },
                onDragStart: function (event, data) {
                    dragStart(event, data);
                }
            };

            ///
            // Drag End handler.
            ///
            function dragEnd() {
                destroyInsertIndicator();
            }

            ///
            // Drag Start handler.
            ///
            function dragStart() {
                // todo
            }

            ///
            // Drop options.
            ///
            scope.dropOptions = {
                simpleEventsOnly: true,
                propagateDragEnter: scope.fieldContainer,
                propagateDragLeave: scope.fieldContainer,
                propagateDrop: scope.fieldContainer,
                propagateDragOver: false,
                onAllowDrop: function (source, target, dragData, dropData) {

                    return allowDrop(source, target, dragData, dropData);
                },
                onDrop: function (event, source, target, dragData, dropData) {
                    return drop(event, source, target, dragData, dropData, scope.parentControl);
                },
                onDragOver: function (event, source, target, dragData, dropData) {
                    return dragOver(event, source, target, dragData, dropData);
                }
            };

            ///
            // Whether this control can be dragged onto the container
            ///
            function allowDrop(source, target, dragData, dropData) {
                return canDropItem(dragData, dropData);
            }

            ///
            // Drop function.
            ///
            function drop(event, source, target, dragData, dropData, tabContainer, control) {
                var dragDataRenderOrdinal;
                var dropDataRenderOrdinal;

                if (tabContainer) {
                    if (dragData && dropData) {

                        // sanity checks
                        if (dragData === dropData)
                            return control;

                        if (!currentInsertionIndicatorPosition) {
                            return control;
                        }

                        // if it reorder of tabs or inserting new item?
                        if (isExistingControlInContainer(tabContainer, dragData)) {
                            reorder(tabContainer, dragData, dropData);
                        }
                        else {
                            addNewControlToContainedControlsOnForm(tabContainer, dragData, dropData);
                        }
                    }
                }
                dropCleanup();
                return tabContainer;
            }

            function isExistingControlInContainer(tabContainer, dragData) {
                var formControl = dragData;

                if (dragData.isTabItem) {
                    formControl = dragData.model.formControl;
                }

                var foundCtrl = _.find(tabContainer.getContainedControlsOnForm(), function (control) {
                    return control.idP === formControl.idP;
                });

                return !!foundCtrl;
            }

            function reorder(tabContainer, dragData, dropData) {
                var dragDataRenderOrdinal = sp.result(dragData, 'model.formControl.renderingOrdinal');
                var dropDataRenderOrdinal = sp.result(dropData, 'model.formControl.renderingOrdinal');

                if (!(dragDataRenderOrdinal >= 0 && dropDataRenderOrdinal >= 0)) return;

                // check if there has been an actual change
                var itemInMiddle;
                var hasChanges = false;

                // moving ordinal up
                if (dragDataRenderOrdinal < dropDataRenderOrdinal) {
                    if (currentInsertionIndicatorPosition === insertionIndicatorPosition.after) {
                        hasChanges = true;
                    }
                    else {
                        itemInMiddle = _.find(tabContainer.getContainedControlsOnForm(), function (ctrl) {
                            return ctrl.renderingOrdinal > dragDataRenderOrdinal && ctrl.renderingOrdinal < dropDataRenderOrdinal;
                        });
                        hasChanges = itemInMiddle ? true : false;
                    }
                }
                else if (dragDataRenderOrdinal > dropDataRenderOrdinal) {   // moving ordinal down
                    if (currentInsertionIndicatorPosition === insertionIndicatorPosition.before) {
                        hasChanges = true;
                    }
                    else {
                        itemInMiddle = _.find(tabContainer.getContainedControlsOnForm(), function (ctrl) {
                            return ctrl.renderingOrdinal < dragDataRenderOrdinal && ctrl.renderingOrdinal > dropDataRenderOrdinal;
                        });

                        hasChanges = itemInMiddle ? true : false;
                    }
                }

                if (hasChanges) {
                    tabContainer.getContainedControlsOnForm().forEach(function (ctrl) {

                        if (ctrl.idP === dragData.model.formControl.idP) {
                            if (currentInsertionIndicatorPosition === insertionIndicatorPosition.before) {
                                ctrl.renderingOrdinal = dropDataRenderOrdinal;
                            }
                            else if (currentInsertionIndicatorPosition === insertionIndicatorPosition.after) {
                                ctrl.renderingOrdinal = dropDataRenderOrdinal + 1;
                            }
                        }
                        else { // is not dragData

                            // this may create non-consecutive numbers
                            if (currentInsertionIndicatorPosition === insertionIndicatorPosition.before && ctrl.renderingOrdinal >= dropDataRenderOrdinal || (currentInsertionIndicatorPosition === insertionIndicatorPosition.after && ctrl.renderingOrdinal > dropDataRenderOrdinal)) {
                                ctrl.renderingOrdinal++;
                            }
                        }
                    });

                    // we may endup with non-consecutive ordinal numbers. so set them in order
                    var controlsOnForm = tabContainer.getContainedControlsOnForm()
                        .sort(
                            function (a, b) {
                                return (a.renderingOrdinal - b.renderingOrdinal);   // numeric sort
                            });

                    updateRenderingOrdinals(controlsOnForm);

                    scope.$emit('spTabsReordered');
                }
            }

            /////
            // Inserts a new control in the collection of contained controls of the form.
            /////
            function insertAtContainedControlsOnForm(container, index, control) {
                var controls;

                controls = container.containedControlsOnForm.slice(0);
                controls.splice(index, 0, control);
                container.containedControlsOnForm = controls;
            }

            function addNewControlToContainedControlsOnForm(tabContainer, dragData, dropData) {
                var controls, index;

                controls = tabContainer.containedControlsOnForm.slice(0);
                index = controls.indexOf(dropData);

                insertAtContainedControlsOnForm(tabContainer, 1, dragData);

                scope.$emit('spTabsUpdated');
            }

            ///
            // Refresh the rendering ordinals of the controls in the array.
            ///
            function updateRenderingOrdinals(controlArray) {
                if (!controlArray) {
                    return;
                }

                for (var i = 0; i < controlArray.length; i++) {
                    setRenderingOrdinal(controlArray[i], i);
                }
            }

            ///
            // Sets the rendering ordinal of a control.
            ///
            function setRenderingOrdinal(control, ordinal) {

                if (!control) {
                    return;
                }

                if (!control.hasOwnProperty('renderingOrdinal')) {
                    control.registerField('console:renderingOrdinal', spEntity.DataType.Int32);
                }

                control.renderingOrdinal = ordinal;
            }

            ///
            // Perform any drop cleanup.
            ///
            function dropCleanup() {
                hideInsertIndicator();
            }

            ///
            // Drag Over.
            ///
            function dragOver(event, source, target, dragData, dropData) {
                if (canDropItem(dragData, dropData)) {
                    showInsertionIndicator(event, target, dragData, dropData);
                } else {
                    hideInsertIndicator();
                }
            }

            // Show the insertion indicator.
            function showInsertionIndicator(event, target, dragData, dropData) {
                if (dropData.isTabItem) {
                    var clientRect = target.getBoundingClientRect(),
                        iiPosition = getInsertionIndicatorPositionTabItem(event, target, dragData, dropData);

                    scope.insertIndicatorDropData.data = dropData;
                    scope.insertIndicatorDropData.target = target;

                    if (currentDropTarget === dropData &&
                        currentInsertionIndicatorPosition === iiPosition) {
                        return;
                    }

                    switch (iiPosition) {
                        case insertionIndicatorPosition.before:
                            positionInsertIndicator(clientRect.top, clientRect.left, target.clientHeight, 4);
                            break;
                        case insertionIndicatorPosition.after:
                            positionInsertIndicator(clientRect.top, clientRect.right - 4, target.clientHeight, 4);
                            break;
                    }

                    currentDropTarget = dropData;
                    currentInsertionIndicatorPosition = iiPosition;
                }
            }

            // Returns true if the drag data can be dropped onto the drop data, false otherwise.
            function canDropItem(dragData, dropData) {

                if (!dragData || !dropData ||
                    dragData === dropData ||
                    (dragData && !dragData.isTabItem)) {
                    return false;
                }

                return true;
            }

            // Gets the insertion indicator position for a tab item.
            function getInsertionIndicatorPositionTabItem(event, target, dragData, dropData) {
                var clientRect = target.getBoundingClientRect(),
                    clientX = event.originalEvent.clientX;

                if (clientX <= clientRect.left + Math.floor(clientRect.width / 2)) {
                    return insertionIndicatorPosition.before;
                }
                else {
                    return insertionIndicatorPosition.after;
                }
            }

            // Positions the insert indicator.
            function positionInsertIndicator(top, left, height, width) {
                var scrollX, scrollY;

                if (!_.isNumber(top) || !_.isNumber(left) || !_.isNumber(height) || !_.isNumber(width)) {
                    return;
                }

                if (!insertIndicatorElement) {
                    createInsertIndicator();
                }

                scrollX = $window.scrollX || 0;
                scrollY = $window.scrollY || 0;

                insertIndicatorElement.css({
                    width: width,
                    height: height,
                    top: top + scrollY,
                    left: left + scrollX
                }).show();
            }

            // Create the insert indicator.
            function createInsertIndicator() {
                if (insertIndicatorElement) {
                    return;
                }

                insertIndicatorScope = scope.$new();

                scope.$apply(function () {
                    insertIndicatorElement = $compile('<div class="tabInsertionIndicatorLine" />')(insertIndicatorScope);
                    if (insertIndicatorElement) {
                        body.append(insertIndicatorElement);
                        insertIndicatorElement.hide();

                        currentDropTarget = null;
                        currentInsertionIndicatorPosition = null;
                    }
                });
            }

            // Hides the insert indicator.
            function hideInsertIndicator() {
                currentDropTarget = null;
                currentInsertionIndicatorPosition = null;

                if (!insertIndicatorElement) {
                    return;
                }

                insertIndicatorElement.hide();
            }

            // Destroy the insert indicator.
            function destroyInsertIndicator() {
                currentDropTarget = null;
                currentInsertionIndicatorPosition = null;

                if (insertIndicatorElement) {
                    insertIndicatorElement.remove();
                    insertIndicatorElement = null;
                }

                if (insertIndicatorScope) {
                    insertIndicatorScope.$destroy();
                    insertIndicatorScope = null;
                }
            }
        }
    }
}());