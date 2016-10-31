// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global $, console, _, angular, sp */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spVerticalStackContainerControl', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'sp.themeService',
        'mod.common.spMobile',
        'mod.common.spCachingCompile',
        'mod.common.ui.spDialogService'
    ]);

    angular.module('mod.app.editForm.designerDirectives.spVerticalStackContainerControl')
        .service('spVerticalStackContainerService', spVerticalStackContainerService)
        .directive('spVerticalStackContainerControl', spVerticalStackContainerControl);

    /* @ngInject */
    function spVerticalStackContainerService() {
        var exports = {};
        var currentTitleStyle = {};
        var currentHeadingStyle = {};

        exports.setStyles = function (titleStyle, headingStyle) {
            if (titleStyle) {
                currentTitleStyle = titleStyle;
            }
            if (headingStyle) {
                currentHeadingStyle = headingStyle;
            }

        };

        exports.getTitleStyle = function () {
            return currentTitleStyle;
        };

        exports.getHeadingStyle = function () {
            return currentHeadingStyle;
        };

        return exports;

    }

    /* @ngInject */
    function spVerticalStackContainerControl(spEditForm, editCoordinator, spFormBuilderService,
                                             spVerticalStackContainerService, spThemeService, spMobileContext,
                                             spCachingCompile, spAlertsService, spDialogService) {

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                formControl: '=',
                parentControl: '=?',
                formData: '=',
                formTheme: '=?',
                formMode: '=?',
                actionPanelFile: '=?',
                actionPanelOptions: '=?',
                isInTestMode: '=?',
                isInDesign: '=?',
                isReadOnly: '=?',
                isEmbedded: '=?',
                structureDepth: '=?'
            },
            link: link
        };

        function link(scope, element) {
            /////
            // Set the control class.
            /////
            scope.controlClass = scope.formMode === 'design' ? 'vertical-stack-container-control-design' : 'vertical-stack-container-control';

            scope.titleStyle = {};
            scope.headingStyle = {};
            scope.isMobileDevice = spMobileContext.isMobile;
            scope.structureDepth = !scope.structureDepth ? 1 : scope.structureDepth + 1;
            scope.titleClass = 'title' + scope.structureDepth;

            /////
            // Sorting of child controls.
            /////
            scope.sortableOptions = {
                disabled: scope.formMode !== spEditForm.formModes.design,
                revert: true,
                onSortComplete: function (event, ui, fromIndex, toIndex) {
                    var startIndex;
                    var endIndex;
                    var renderingOrdinal;
                    var originalRenderingOrdinal;

                    if (toIndex === fromIndex) {
                        return;
                    }

                    /////
                    // Reorder the controlsOnForm array.
                    /////
                    scope.controlsOnForm.splice(toIndex, 0, scope.controlsOnForm.splice(fromIndex, 1)[0]);

                    if (toIndex < fromIndex) {
                        startIndex = toIndex;
                        endIndex = fromIndex;
                    } else {
                        startIndex = fromIndex;
                        endIndex = toIndex;
                    }

                    originalRenderingOrdinal = scope.controlsOnForm[startIndex].renderingOrdinal;

                    /////
                    // Update the rendering ordinals for all controls between the 'start' and 'end' indexes.
                    /////
                    for (var i = startIndex; i <= endIndex; i++) {

                        if (i + 1 <= endIndex) {
                            renderingOrdinal = scope.controlsOnForm[i + 1].renderingOrdinal;
                        } else {
                            renderingOrdinal = originalRenderingOrdinal;
                        }

                        scope.controlsOnForm[i].renderingOrdinal = renderingOrdinal;
                    }
                }
            };

            /////
            // Get the controls contained within this control directly referencing the entity model.
            /////
            scope.getFormControls = function () {

                if (scope.formControl && scope.formControl.getContainedControlsOnForm) {
                    return _.sortBy(scope.formControl.getContainedControlsOnForm(), scope.renderingOrdinal);
                }


                return null;
            };

            /////
            // Filter used by the repeater to order the controls.
            /////
            scope.renderingOrdinal = function (formControl) {
                return sp.result(formControl, 'renderingOrdinal') || 0;
            };

            scope.onEditClick = function () {
                scope.formMode = spEditForm.formModes.edit;
                editCoordinator.setDisabled(true);
            };

            scope.onSaveClick = function () {

                spEditForm.saveFormData(scope.formData).then(
                    function () {
                        editCoordinator.setDisabled(false);
                        scope.formMode = spEditForm.formModes.view;
                    },
                    function (error) {
                        showAlert(error);
                    });
            };

            scope.onCancelClick = function () {
                editCoordinator.setDisabled(false);
                scope.formMode = spEditForm.formModes.view;
            };

            var editButton = {
                text: 'Edit',
                click: scope.onEditClick,
                order: 0
            };

            var saveButton = {
                text: 'Save',
                click: scope.onSaveClick,
                order: 1
            };

            var cancelButton = {
                text: 'Cancel',
                click: scope.onCancelClick,
                order: 2
            };

            var editSaveButtons = [
                editButton,
                saveButton,
                cancelButton
            ];

            scope.editSaveButtons = editSaveButtons;

            scope.getContentsClass = function (formControl) {
                if (scope.isMobileDevice && !scope.parentControl) {
                    return 'contents';
                } else {
                    if (formControl && spFormBuilderService.isImplicitContainer(formControl)) {
                        return 'contents-implicit';
                    }
                }

                return 'contents';
            };

            scope.getControlStyle = function (formControl) {
                var style = {};

                if (formControl) {
                    if (formControl.renderingBackgroundColor) {
                        style['background-color'] = formControl.renderingBackgroundColor;
                    }
                }

                return style;
            };

            scope.getTitleStyle = function (structureDepth) {
                var fc = scope.formControl;
                if (!fc || !fc.name || $.trim(fc.name).length === 0) {
                    return {display: 'none'};
                }
                if (structureDepth === 1) {
                    return spThemeService.getTitleStyle();
                } else {
                    return spThemeService.getHeadingStyle();
                }
            };

            var controlsNotToShowHelpOn = ['console:chartRenderControl', 'console:reportRenderControl', 'console:tabRelationshipRenderControl', 'console:tabContainerControl'];

            scope.showHelp = function (formControl) {
                var typeAlias = spUtils.result(formControl, 'type.nsAlias');

                if (_.find(controlsNotToShowHelpOn, function (t) { return t == typeAlias; }))
                    return false;

                if (formControl && formControl.showControlHelpText && formControl.description) {
                    return true;
                }

                return false;
            };

            scope.getChildControlTitle = function (formControl) {
                var titleModel = spEditForm.createTitleModel(formControl, scope.isInDesign);
                return titleModel.name;
            };

            scope.$watch('formMode', function (mode) {
                editButton.hidden = mode !== spEditForm.formModes.view;
                cancelButton.hidden = mode === spEditForm.formModes.view;
                saveButton.hidden = cancelButton.hidden;
            });

            scope.$watch('formTheme', function () {
                if (scope.formTheme) {
                    scope.titleStyle = {};
                    scope.headingStyle = {};
                    var titleFontColor = scope.formTheme.consoleGeneralContentAreaTitleFontColor;
                    if (titleFontColor) {
                        scope.titleStyle['color'] = sp.getCssColorFromARGBString(titleFontColor);
                    }

                    var headingFontColor = scope.formTheme.consoleGeneralContentAreaContainerHeadingFontColor;
                    if (headingFontColor) {
                        scope.headingStyle['color'] = sp.getCssColorFromARGBString(headingFontColor);
                    }

                    var headingLineColor = scope.formTheme.consoleGeneralContentAreaContainerHeadingLineColor;
                    if (headingLineColor) {
                        //$scope.titleStyle['border-bottom-color'] = sp.getCssColorFromARGBString(headingLineColor);
                        scope.headingStyle['border-bottom-color'] = sp.getCssColorFromARGBString(headingLineColor);
                    }

                    spVerticalStackContainerService.setStyles(scope.titleStyle, scope.headingStyle);
                }
            });

            scope.$watch('isDisabled || !formData', function (isDisabled) {

                editButton.disabled = isDisabled || !scope.formData;
            });

            scope.$on('$destroy', function () {
                if (editCoordinator) {
                    editCoordinator.unregisterIsDirty(isDirty);
                    editCoordinator.unregisterSetDisabled(setDisabled);
                }
            });

            scope.$on('gather', function (event, callback) {
                var options = {};

                var contentClass = scope.getContentsClass(scope.formControl);
                if (contentClass) {
                    options.contentClass = '.' + contentClass;
                }

                if (scope.titleClass) {
                    options.titleClass = '.' + scope.titleClass;
                }

                callback(scope.formControl, scope.parentControl, element, options);
            });

            scope.$on('measureArrangeComplete', function (event) {

                if (scope && scope.formControl) {

                    if (element) {

                        var height = element.outerHeight();
                        if (height && height > 0) {

                            // 50px min height
                            if (height < 50) {
                                height = 50;
                            }

                            var contentElement = element.find('> div');
                            if (contentElement) {

                                var childElements = contentElement.children();
                                if (childElements.length > 1) {

                                    // subtract the title height
                                    var titleHeight = childElements.first().outerHeight(true);
                                    if (titleHeight > 0 && height > titleHeight) {

                                        childElements.last().css('height', 'calc(100% - ' + titleHeight + 'px)');
                                    }
                                }
                            }
                        }
                    }
                }
            });

            editCoordinator.registerIsDirty(isDirty);
            editCoordinator.registerSetDisabled(setDisabled);

            var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spVerticalStackContainerControl/verticalStackContainerControl.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });

            function setDisabled(value) {
                if (scope.formMode !== spEditForm.formModes.edit) {
                    scope.isDisabled = value;
                }
            }

            function isDirty() {
                if (!scope.formMode || scope.formMode !== 'edit' || !scope.formData) {
                    return false;
                }

                return scope.formData.hasChangesRecursive();
            }

            function showAlert(msg, severity) {
                spAlertsService.addAlert(msg, {severity: severity || 'error'});
            }
        }
    }
}());