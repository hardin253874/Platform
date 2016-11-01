// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spTabContainerControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spTabContainerControl', [
        'mod.app.editForm',
        'mod.common.spMobile',
        'mod.common.spCachingCompile',
        'sp.app.settings',
        'mod.app.spFormControlVisibilityService'
    ]).directive('spTabContainerControl', function ($rootScope, spEditForm, spNavService, spMobileContext, spCachingCompile, spAppSettings, spFormControlVisibilityService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=?',
                    parentControl: '=?',
                    formData: '=?',
                    formTheme: '=?',
                    formMode: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?',
                    isInDesign: '=?',
                    isEmbedded: '=?',
                    designData: '=?',
                    isInlineEditing: '=?'
                },
                link: function ($scope, element) {
                    var templateUrl = spMobileContext.isMobile ?
                        'editForm/directives/spTabContainerControl/spTabContainerControlMobile.tpl.html' :
                        'editForm/directives/spTabContainerControl/spTabContainerControl.tpl.html';                                        

                    var updateSelectedTabInfo = true;

                    $scope.$on('$destroy', function () {
                        updateSelectedTabInfo = false;
                    });
                    
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    $scope.model = {
                        isReadOnly: $scope.isReadOnly,
                        isInTestMode: $scope.isInTestMode,
                        isEmbedded: $scope.isEmbedded                        
                    };

                    function configureFormControl() {
                        if ($scope.formControl) {

                            //$scope.getFormControlFile = spEditForm.getFormControlFile;
                            //$scope.getControlTitle = spEditForm.getControlTitle;
                            checkAndUpdateRenderingOrdinal();

                            var controlsOnForm = $scope.formControl.getContainedControlsOnForm()
                                    .sort(
                                        function (a, b) {
                                            return (a.getName() > b.getName());
                                        });

                            $scope.controlsOnForm = filterControls(controlsOnForm);

                            setTabItems($scope.controlsOnForm, $scope);
                            
                            /////
                            // When the form data changes, update the model.
                            /////
                            $scope.$watch("formData", function () {
                                if ($scope.formData) {

                                    if ($scope.model.tabItems) {
                                        _.forEach($scope.model.tabItems, function (item) {
                                            item.model.formData = $scope.formData;
                                        });
                                    }
                                }
                            });
                        }
                    }

                    function filterControls(controls) {
                        // Feature switch for tab controls
                        // Exclude controls with the following aliases
                        var exclude = {
//                            'core:objectSummaryControl': !spAppSettings.isFeatureOn('securitySummary')
                        };
                        var filtered = _.filter(controls, function (ctrl) {
                            return !exclude[ctrl.nsAlias];
                        });
                        return filtered;
                    }
                    
                    ///
                    // Check and set if any control does not have a renderingOrdinal set.
                    ///
                    function checkAndUpdateRenderingOrdinal() {
                        if ($scope.formControl) {
                            var maxExistingOrdinal = 0;
                            var hasUnassignedOrdinalControl = false;
                            
                            // check if there is any element that doesn't have rendering Ordinal set. Also find out the max ordinal number used if any
                            $scope.formControl.getContainedControlsOnForm().forEach(function (control) {
                                
                                if (!control.hasOwnProperty('renderingOrdinal')) {
                                    control.registerField('console:renderingOrdinal', spEntity.DataType.Int32);
                                    hasUnassignedOrdinalControl = true;
                                }
                                else if (!_.isNumber(control.renderingOrdinal)) {
                                    hasUnassignedOrdinalControl = true;
                                }
                                else if (control.renderingOrdinal > maxExistingOrdinal) {
                                    maxExistingOrdinal = control.renderingOrdinal;
                                }
                            });
                            
                            if(hasUnassignedOrdinalControl) {
                                $scope.formControl.getContainedControlsOnForm().forEach(function (control) {

                                    if (!_.isNumber(control.renderingOrdinal)) {
                                        control.renderingOrdinal = ++maxExistingOrdinal;
                                    }
                                });
                            }
                        }
                    }
                    
                    
                    ///
                    // Update the tabItems ordinals on spTabsReordered event.
                    ///
                    $scope.$on("spTabsReordered", function (event) {
                        event.stopPropagation();

                        var controls = $scope.formControl.getContainedControlsOnForm();

                        if ($scope.model.tabItems && controls) {
                            _.forEach($scope.model.tabItems, function (item) {
                                
                                var foundControl = _.find(controls, function (ctrl) { return ctrl.idP === sp.result(item, 'model.formControl.idP'); });
                                
                                if(foundControl) {
                                    item.ordinal = foundControl.renderingOrdinal;
                                }
                            });
                        }
                    });
                    
                    ///
                    // Configure the form control on 'formControlUpdated' event.
                    ///
                    $scope.$on('formControlUpdated', configureFormControl);
                    
                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", configureFormControl);

                    /////
                    // Watch for changes to the contained controls.
                    /////
                    $scope.$watch("formControl.containedControlsOnForm.length", function (newVal, oldVal) {

                        if (newVal === oldVal) {
                            return;
                        }

                        configureFormControl();
                    });

                    /////
                    // When the is-read-only value changes, update the model.
                    /////
                    $scope.$watch("isReadOnly", function (value) {
                        $scope.model.isReadOnly = value;
                    });

                    /////
                    // When the is-embedded value changes, update the model.
                    /////
                    $scope.$watch("isEmbedded", function (value) {
                        $scope.model.isEmbedded = value;
                    });

                    // watch formMode
                    $scope.$watch("formMode", function () {
                        var tabItems = sp.result($scope, 'model.tabItems');
                        _.forEach(tabItems, function (item) {
                            item.model.formMode = $scope.formMode;
                        });
                    });

                    
                    /////
                    // The tab selection has changed.
                    /////
                    $scope.selectionChanged = function (tab) {
                        // add selected tab id to the nav item
                        if (updateSelectedTabInfo && tab) {

                            var tabContainerId = sp.result(tab, 'model.parentControl.idP');
                            var selectedTabId = sp.result(tab, 'model.formControl.idP');

                            if (tabContainerId && selectedTabId) {
                                var navItem = spNavService.getCurrentItem();
                                navItem.data = navItem.data || {};
                                navItem.data.selectedTabInFormInfo = navItem.data.selectedTabInFormInfo || {};

                                // add/override the form key
                                navItem.data.selectedTabInFormInfo[tabContainerId] = selectedTabId;
                            }
                        }
                    };

                    /////
                    // Callback that is fird when a tab is removed.
                    /////
                    function removeCallback(tab) {

                        if (!tab || !tab.model || !tab.model.formControl) {
                            return;
                        }


                        if (tab.model.formControl.getDataState() === spEntity.DataStateEnum.Create) {
                            $scope.formControl.getContainedControlsOnForm().remove(tab.model.formControl);
                        }

                        tab.model.formControl.dataState = spEntity.DataStateEnum.Delete;

                        configureFormControl();
                    }

                    /////
                    // Configure the form control.
                    /////
                    function configureCallback(formControl, options) {
                        if ($scope.designData.configureCallback) {
                            $scope.designData.configureCallback(formControl, options);
                        }
                    }
                    
                    /////
                    // Configures the tab items.
                    /////
                    function setTabItems(controlsOnForm, scope) {
                        var navItem = spNavService.getCurrentItem(),
                            selectedTabId = -1;

                        if (navItem && sp.result(navItem, 'data.selectedTabInFormInfo')) {
                            var foundItem = navItem.data.selectedTabInFormInfo[scope.formControl.idP];

                            if (foundItem) {
                                selectedTabId = foundItem;
                            }
                        }
                        
                        var existingTabs = $scope.model.tabItems;
                        var tabItems = [];
                        
                        if (controlsOnForm && _.isArray(controlsOnForm)) {
                            _.forEach(controlsOnForm, function (control) {
                                var tabItem = createNewTabItem(scope, control, selectedTabId);
                                tabItems.push(tabItem);
                            });
                        }

                        var filteredTabs = _.filter(tabItems,
                            function(t) {
                                if (t && t.model && t.model.formControl && t.model.formControl.getDataState() !== spEntity.DataStateEnum.Delete) {
                                    return true;
                                }

                                return false;
                            });

                        /////
                        // Add a dummy tab whilst in design mode.
                        /////
                        if (filteredTabs.length === 0 && $scope.isInDesign) {
                            if ($scope.designData && $scope.designData.tabs) {
                                tabItems = _.union(tabItems, $scope.designData.tabs);
                            }
                        }

                        if (selectedTabId === -1 && tabItems.length > 0) {
                            // make the first tab active
                            _.first(_.sortBy(tabItems, 'ordinal')).isActive = true;
                        }

                        $scope.model.tabItems = tabItems;
                    }

                    function controlVisibilityHandler(controlId, isControlVisible) {
                        if (!$scope.model || !$scope.model.tabItems || !controlId) {
                            return;
                        }

                        var tabItem = _.find($scope.model.tabItems, function(ti) {
                            return ti.model && ti.model.formControl && ti.model.formControl.id() === controlId;
                        });

                        if (tabItem &&
                            tabItem.isHidden === isControlVisible) {
                            tabItem.isHidden = !isControlVisible;
                        }
                    }

                    function createNewTabItem(scope, control, selectedTabId) {
                        if (!scope.isInDesign && !$scope.isInlineEditing && control && control.visibilityCalculation) {
                            spFormControlVisibilityService.registerControlVisibilityHandler(scope, control.id(), controlVisibilityHandler);    
                        }                        

                        return {
                            name: spEditForm.getControlTitle(control),
                            isTabItem: true,
                            isActive: control.id() === selectedTabId,
                            isHidden: false,
                            tooltip: spEditForm.getControlDescription(control),
                            selectionChanged: scope.selectionChanged,
                            ordinal: control.hasField('console:renderingOrdinal') ? control.renderingOrdinal : null,
                            url: scope.isInDesign ? 'formBuilder/directives/spFormBuilder/templates/tabRenderControl.tpl.html' : 'editForm/partials/shared/tabControl.tpl.html',
                            model: {
                                formControl: control,
                                formData: scope.formData,
                                formTheme: scope.formTheme,
                                formMode: scope.formMode,
                                isReadOnly: scope.isReadOnly,
                                isInTestMode: scope.isInTestMode,
                                isInDesign: scope.isInDesign,
                                isEmbedded: scope.isEmbedded,
                                removeCallback: removeCallback,
                                configureCallback: configureCallback,
                                parentControl: scope.formControl
                            },
                            parentModel: scope.model    // todo: move stuff like isInTestMode, isInDesign etc  from model into parentModel **also fix in editFoem/partials/tabContainerControl (its still been used!)
                        };
                    }

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    $scope.$on('measureArrangeComplete', function (event) {

                        if ($scope && $scope.formControl) {

                            if (element) {

                                var height = element.outerHeight();
                                if (height && height > 0) {

                                    // 200px min height 
                                    if (height < 200) {
                                        height = 200;
                                    }

                                    var tabContentElement = element.find('> sp-tabs > #spTabSet > .tab-content');
                                    if (tabContentElement) {

                                        // set the background color on the contents
                                        if ($scope.formControl.renderingBackgroundColor) {
                                            tabContentElement.css('background-color', $scope.formControl.renderingBackgroundColor);
                                        }

                                        var tabsHeight = 0;

                                        // subtract the tabs height (may wrap on to multiple lines)
                                        var tabsElement = element.find('> sp-tabs > div > ul');
                                        if (tabsElement) {

                                            tabsHeight = tabsElement.outerHeight(true);
                                        }

                                        if (tabsHeight > 0 && height > tabsHeight) {

                                            tabContentElement.css('height', 'calc(100% - ' + tabsHeight + 'px)');
                                        }
                                    }
                                }
                            }
                        }
                    });

                    var cachedLinkFunc = spCachingCompile.compile(templateUrl);
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());