// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing an editable label control.
    * spEditableLabel provides the label that can be edited in-place.
    *
    * @module spEditableLabel
    * @example

    Using the spEditableLabel:

    &lt;sp-editable-label value="myValue"&gt;&lt;/sp-editable-label&gt

    where value is the scope value to display/edit.
        
    */
    angular.module('mod.common.ui.spEditableLabel', ['mod.common.spCachingCompile'])
        .directive('spEditableLabel', function (focus, $timeout, spCachingCompile) {
            return {
                restrict: 'E',
                transclude: false,                                
                scope: {
                    value: '=',
                    readonly: '=?',
                    placeholder: '=?',
                    preUpdateCallback: '=?',
                    updateCallback: '=?',
                    keyDownCallback: '=?',
                    changeCallback: '=?',
                    suffix: '=?',
                    additionalData: '=?',
                    showHelpText: '=?',
                    helpText: '=?'
                },
                link: function (scope, element) {
                    var originalValue;

                    scope.editing = false;
                    scope.readonly = !!scope.readonly;
                    scope.labelValue = scope.value;
                    
                    /////
                    // Value has been updated.
                    /////
                    function update() {

                        if (!scope.editing) {
                            return;
                        }

                        if (scope.labelValue) {
                            scope.labelValue = scope.labelValue.trim();
                        }

                        if (scope.preUpdateCallback && originalValue !== null) {
                            if (scope.preUpdateCallback(scope.labelValue, originalValue, scope.additionalData) === false) {
                                revert();
                                return;
                            }
                        }

                        scope.editing = false;
                        scope.value = scope.labelValue;

                        $timeout(function() {
                            if (scope.updateCallback) {
                                scope.updateCallback(scope.value, originalValue, scope.additionalData);
                            }

                            originalValue = null;
                        });
                    }

                    /////
                    // Value has been reverted.
                    /////
                    function revert() {
                        scope.labelValue = originalValue;

                        originalValue = null;
                        scope.editing = false;
                    }

                    /////
                    // When clicking on the control, switch to edit mode.
                    /////
                    scope.onClick = function () {
                        if (!scope.readonly && !this.editing) {

                            originalValue = this.value;

                            this.editing = true;

                            focus('editing');
                        }
                    };

                    /////
                    // Select the contents of the input box when focused.
                    /////
                    scope.onFocus = function (evt) {
                        var e = evt || event;

                        $(e.target).select();
                    };

                    /////
                    // Perform special processing when enter/escape are pressed.
                    /////
                    scope.onKeyDown = function (event) {

                        if (scope.keyDownCallback) {
                            if (scope.keyDownCallback(event) === false) {
                                return false;
                            }
                        }

                        if (!scope.readonly && this.editing) {

                            if (event.which === 13) {
                                /////
                                // Enter pressed
                                /////
                                update();
                            } else if (event.which === 27) {
                                /////
                                // Escape pressed
                                /////
                                revert();
                            }
                            
                        }

                        return true;
                    };

                    /////
                    // On change handler.
                    /////
                    scope.onChange = function() {
                        if (scope.changeCallback) {
                            scope.labelValue = scope.changeCallback(scope.labelValue);
                        }
                    };

                    /////
                    // Watch the two-way value and update the label value when it changes.
                    /////
                    scope.$watch('value', function(newVal) {

                        if (newVal === scope.labelValue) {
                            return;
                        }

                        scope.labelValue = newVal;
                    });

                    /////
                    // Blur event.
                    /////
                    scope.onBlur = function () {
                        update();
                    };

                    var cachedLinkFunc = spCachingCompile.compile('editableLabel/spEditableLabel.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());