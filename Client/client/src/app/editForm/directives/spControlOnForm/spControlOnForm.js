// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spControlOnForm module determines the correct directive, builds a
    // template representing it, compiles it against the scope and injects the
    // resulting element into the DOM. If a directive does not exist, the legacy
    // edit-form control is used.
    /////
    angular.module('mod.app.editForm.designerDirectives.spControlOnForm', [
        'mod.common.spMobile', 'mod.common.spCachingCompile', 'mod.app.spFormControlVisibilityService'
    ]);

    angular.module('mod.app.editForm.designerDirectives.spControlOnForm')
        .directive('spControlOnForm', spControlOnForm);

    function spControlOnForm($injector, spMobileContext, spCachingCompile, spFormControlVisibilityService) {

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            scope: {
                formControl: '=',
                parentControl: '=?',
                formData: '=',
                formTheme: '=?',
                formMode: '=?',
                isInTestMode: '=?',
                isReadOnly: '=?',
                isDisabled: '=?',
                isInDesign: '=?',
                isEmbedded: '=?',
                structureDepth: '=?',
                isInlineEditing: '=?'
            },
            link: link
        };

        function link(scope, element) {

            /////
            // Compile the 'control on form'
            /////

            scope.isMobile = spMobileContext.isMobile;

            if (!(scope.formControl && scope.formControl.getType)) {
                return;
            }

            var template;
            var elementName;
            var style;
            var styleVal = {};
            var haveStyle;
            var templateKey;

            var type = scope.formControl.getType();
            var alias = type && type.getAlias();
            var field = scope.formControl.fieldToRender;
            var tablet = spMobileContext.isTablet;
            var mobile = spMobileContext.isMobile;
            var cachedIsControlVisible;

            if (!type || !alias) {
                return;
            }

            /////
            // Form the directive name.
            /////
            var directiveName = 'sp' + capitaliseFirstLetter(alias);

            /////
            // Determine whether the directive exists or not.
            /////
            var directiveExists = $injector.has(directiveName + 'Directive');

            /////
            // If the directive exists, generate a template for it. If not, fallback to the legacy edit-form control.
            /////
            if (directiveExists) {
                elementName = getElementName(directiveName);

                style = {width: 360, height: 30, display: 'block'};

                var controlType = _.find(scope.formControl.isOfType, {idP: type.idP});
                if (controlType) {
                    style.width = mobile ? controlType.minWidthMobile : tablet ? controlType.minWidthTablet : controlType.minWidth;
                    style.height = mobile ? controlType.minHeightMobile : tablet ? controlType.minHeightTablet : controlType.minHeight;
                }

                // this control is 'special' due to 'allowMultiLines' on the related field dictating allowed height
                if (alias === 'singleLineTextControl') {
                    if (field && field.allowMultiLines) {
                        style.height = mobile || tablet ? 110 : 60;
                    }
                    style.overflow = 'hidden';
                }

                // Bug 28769: Workflow Buttons are not actionable when logged in as User. (Client: Origin)
                if (alias === 'tabRelationshipRenderControl') {
                    delete style.display;
                }

                if (style.height && style.width) {
                    styleVal.height = style.height.toString() + 'px';
                    styleVal.width = style.width.toString() + 'px';
                    styleVal['min-height'] = style.height.toString() + 'px';
                    styleVal['min-width'] = style.width.toString() + 'px';
                    haveStyle = true;
                }

                if (style.display) {
                    styleVal.display = style.display;
                    haveStyle = true;
                }

                if (scope.formControl.renderingBackgroundColor) {
                    styleVal['background-color'] = scope.formControl.renderingBackgroundColor;
                    haveStyle = true;
                }

                if (style.background) {
                    styleVal.background = style.background;
                    haveStyle = true;
                }

                if (style.overflow) {
                    styleVal.overflow = style.overflow;
                    haveStyle = true;
                }               

                // when showing in the grid, just let it be plain and fill the cell
                // and writing this way to have minimal impact on existing code
                if (scope.isInlineEditing) {
                    style = {};
                    styleVal = {};
                    haveStyle = true;
                }

                templateKey = 'spControlOnForm-' + elementName;
            } else {
                /////
                // TODO: Remove this once we have all new isolate directives created.
                /////
                templateKey = 'spControlOnForm-spReplace';
            }

            if (!scope.isInlineEditing && !scope.isInDesign && scope.formControl && scope.formControl.visibilityCalculation) {
                spFormControlVisibilityService.registerControlVisibilityHandler(scope, scope.formControl.id(), controlVisibilityHandler);
            }

            var cachedLinkFunc = spCachingCompile.get(templateKey);
            if (!cachedLinkFunc) {
                if (directiveExists) {
                    template = '<' + elementName + ' form-data="formData" form-control="formControl" parent-control="parentControl" ' +
                        'form-theme="formTheme" form-mode="formMode" is-read-only="isReadOnly" structure-depth="structureDepth" ' +
                        'is-in-test-mode="isInTestMode" is-embedded="isEmbedded" is-disabled="isDisabled" ' +
                        'is-in-design="isInDesign" is-inline-editing="isInlineEditing"></' + elementName + '>';                                               
                } else {
                    template = '<sp-replace source="getStructureControlFile(formControl, formMode, isReadOnly,isDisabled)" ' +
                        'ng-controller="structureControlOnFormController"></sp-replace>';
                }
                //
                cachedLinkFunc = spCachingCompile.compile(templateKey, template);
                
            }
            
            cachedLinkFunc(scope, function (clone) {
                if (haveStyle) {
                    clone.css(styleVal);
                }
                element.append(clone);
            });

            function controlVisibilityHandler(formControlId, isControlVisible) {
                if (!formControlId || !element || scope.formControl.id() !== formControlId) {
                    return;
                }

                if (cachedIsControlVisible === isControlVisible) {
                    return;
                }

                spFormControlVisibilityService.showHideElement(element, isControlVisible);

                cachedIsControlVisible = isControlVisible;
            }
        }

        /////
        // Convert a directive name into its equivalent element name.
        /////
        function getElementName(str) {
            return str.replace(/(.*?)([A-Z])/g, '$1-$2').toLowerCase();
        }

        /////
        // Capitalizes the first letter of a string.
        /////
        function capitaliseFirstLetter(string) {
            return string.charAt(0).toUpperCase() + string.slice(1);
        }        
    }
}());