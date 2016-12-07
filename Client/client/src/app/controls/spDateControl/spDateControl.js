// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

// WORK IN PROGRESS

(function() {
    'use strict';

    /**
    * Module implementing a date control.
    * spDateControl displays and modifies date values.
    *
    * @module spDateControl
    * @example

    Using the spDateControl:

    &lt;sp-date-control model="myModel"&gt;&lt;/sp-date-control&gt

    where myModel is available on the scope with the following members:

    Properties:
        - value {date}              - The current value.
            default: 0
        - prefix {string}           - (optional) A string placed before the value in both read-only and modify state.
            default: ''
        - suffix {string}           - (optional) A string placed after the value in both the read-only and modify states.
            default: ''
        - isReadOnly {boolean}      - (optional) A boolean indicating whether this control is rendered as read-only or not.
            default: false
        - isRequired {boolean}      - (optional) True if this control requires a value; false otherwise.
            default: false
        - isInTestMode {boolean}    - (optional) A boolean indicating whether this control is currently in test mode or not.
            default: false
        - minimumValue {date}       - (optional) The lower-bound of the value.
            default: null
        - maximumValue {date}       - (optional) The lower-bound of the value.
            default: null
        
    Note: Dates do not respect timezones, and are not intended to.
    */
    angular.module('app.controls.spDateControl', ['ngLocale', 'mod.common.ui.spDialogService', 'sp.common.fieldValidator', 'mod.common.spCachingCompile', 'ui.bootstrap.position'])
        .directive('spDateControl', function (spDialogService, spControlProvider, $locale, spCachingCompile, $document, $uibPosition) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    model: '=?'
                },                
                link: function (scope, element) {
                    var body = $document.find('body')[0],
                        previousElementPos = {},
                        previousHeight = -1;

                    body.addEventListener('click', documentClick, true);

                    scope.internalModel = {
                        value: undefined
                    };                                        

                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        //typeParser: defaultDateParser//spParseDate//spUtils.parseDate
                    };


                    /////
                    // Open the dialog.
                    /////
                    function open($event) {

                        $event.preventDefault();                                                
                        
                        scope.model.opened = !scope.model.opened;
                    }

                    if (scope.model) {
                        scope.model.dateOptions = {
                            'year-format': "'yy'",
                            'starting-day': 1,
                            'spDateControlPopup': ''
                        };

                        scope.model.open = open;
                        scope.model.format = Globalize.culture().calendars.standard.patterns.d;
                    }

                    /////
                    // Invoke the provider.
                    /////
                    spControlProvider(scope, options);
                    

                    scope.$watch('model.value', updateInternalModelValue);

                    scope.$watch('internalModel.value', function (newVal, oldVal) {

                        if (_.isDate(newVal) && _.isDate(oldVal) && newVal.getTime() === oldVal.getTime())
                            return;

                        if (_.isDate(scope.internalModel.value) && _.isDate(scope.model.value) && scope.internalModel.value.getTime() === scope.model.value.getTime())
                            return;

                        if (scope.internalModel.value === scope.model.value)
                            return;
                        
                        // if update from the view and not valid value then skip updating model.value
                        if (scope.validationModel.getIsValidValue() === false)
                            return;

                        scope.model.value = scope.internalModel.value;

                    });
                    
                    // this is to cover edge case where an invalid date value is typed in and after that the field is cleared.
                    // when the invalid value is entered then the 'internalModel.Value' is set to null and 'model.value' is not updated and it holds old valid value.
                    // if then the date value is cleared, the watch on 'internalModel.Value' is not fired 'cos it is already null and that means the 'model.value' is not updated to null.
                    // this watch updates the 'model.value' to null.
                    scope.$watch('validationModel.getIsValidValue()', function (newVal, oldVal) {
                        if (newVal === oldVal)
                            return;
                        
                        if(newVal && scope.model.value && !scope.internalModel.value) {
                            scope.model.value = scope.internalModel.value;
                        }
                    });

                    scope.$on('$destroy', function () {
                        scope.dateTimePickerPopupElement = null;
                        body.removeEventListener('click', documentClick, true);
                    });
                     
                    scope.$on('spDateControlPopup', function (event, popup) {
                        scope.dateTimePickerPopupElement = popup;

                        event.stopPropagation();

                        updatePopupPosition();

                        _.delay(updatePositionTimerCallback, 100);
                    });

                    scope.onDateInputClick = function () {
                        if (!scope.model.opened) return;

                        scope.model.opened = false;
                    };

                    if (sp.result(scope, 'model.isInlineEditing')) {
                        scope.$watch('model.isReadOnly', function(isReadOnly) {
                            if (isReadOnly) {
                                // Reset internal value to model value when switching control to view mode
                                updateInternalModelValue(scope.model.value);                                                                        
                            }
                        });
                    }

                    function updateInternalModelValue(modelValue) {
                        if (_.isDate(scope.internalModel.value) && _.isDate(modelValue) && scope.internalModel.value.getTime() === modelValue.getTime())
                            return;

                        if (scope.internalModel.value === modelValue)
                            return;

                        scope.internalModel.value = modelValue;
                    }

                    function updatePopupPosition() {
                        var btnCalender = element.find('button.btn-calendar'),
                            popupElement = scope.dateTimePickerPopupElement;

                        if (!popupElement || !popupElement.length || !btnCalender || !btnCalender.length) {
                            return;
                        }

                        popupElement.css({ top: 0, left: 0, display: 'block', opacity: 0 });

                        // Calculate the position for the popup
                        var position = $uibPosition.offset(btnCalender);
                        previousElementPos = position;
                        
                        var puWidth = popupElement.prop('offsetWidth');                        
                        var puHeight = popupElement.prop('offsetHeight');
                        // Hack to prevent flicker of popup as it is moved to final position                        
                        // while inner element transclusions make it change size
                        var opacity = puWidth < 250 ? 0 : 1;    // style change might have broken the hack. 
                        
                        previousHeight = puHeight;

                        var puStyle = {
                            top: position.top + position.height,
                            left: (position.left + position.width) - puWidth
                        };             
                           
                        // adjust the top and left if the popup is going off the screen
                        if (puHeight + puStyle.top > $(window).height()) {
                            puStyle.top = $(window).height() - puHeight - 5;
                        }

                        if (puWidth + puStyle.left > $(window).width()) {
                            puStyle.left = $(window).width() - puWidth - 5;
                        }

                        if (puStyle.left < 0) {
                            puStyle.left = 5;
                        }
                        
                        puStyle.top += 'px';
                        puStyle.left += 'px';
                        puStyle.display = 'block';
                        puStyle.position = 'absolute';
                        puStyle.opacity = opacity;
                        
                        // Now set the calculated positioning.
                        popupElement.css(puStyle);
                    }

                    // Update position timer callback
                    // Used to move the popup when the parent control moves
                    function updatePositionTimerCallback() {
                        if (!scope.model.opened) {
                            return;
                        }

                        var btnCalender = element.find('button.btn-calendar');
                        var popupElement = scope.dateTimePickerPopupElement;

                        if (!popupElement || !popupElement.length || !btnCalender || !btnCalender.length) {
                            return;
                        }

                        var currentElementPos = $uibPosition.offset(btnCalender);
                        var currentHeight = popupElement.prop('offsetHeight');                            
                        
                        if (currentElementPos.width !== previousElementPos.width ||
                            currentElementPos.height !== previousElementPos.height ||
                            currentElementPos.top !== previousElementPos.top ||
                            currentElementPos.left !== previousElementPos.left ||
                            currentHeight !== previousHeight) {
                            scope.$apply(function () {
                                updatePopupPosition();
                            });
                        }
                        
                        _.delay(updatePositionTimerCallback, 100);
                    }

                    // Handle document click events.
                    // This is raised before the local document click handler
                    // is fired. This is necessary because the regular click handler is triggered too late
                    // and ends up getting called after the destroy handler of the bootstrap datePicker.
                    function documentClick(event) {
                        if (!event || !scope.model.opened) return;                                                                        

                        // Get the element that was clicked
                        var target = angular.element(event.target);
                        // Get the scope on the element that was clicked
                        var targetScope = target.scope();
                        var clickedOutsidePopup = true;
                        
                        // Check if target scope is a child of the current scope
                        // This way we can determine if the user clicked inside
                        // the popup.
                        while (targetScope) {
                            if (targetScope === scope) {
                                clickedOutsidePopup = false;
                                break;
                            }

                            targetScope = targetScope.$parent;
                        }

                        if (clickedOutsidePopup) {
                            // Have clicked outside date control popup
                            // so close it
                            scope.model.opened = false;
                        }                        
                    }

                    var cachedLinkFunc = spCachingCompile.compile('controls/spDateControl/spDateControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        })
        .directive('spDatePickerKeyHandler', function() {
            return {
                restrict: 'A',                
                priority: 5000, // Ensure we run last
                link: function (scope, elm) {
                    // If we are inline editing remove all the keydown bindings on the input element.
                    // This way we remove the one added by the bootstrap date picker which displays the popup
                    // instead of navigating to the next row
                    if (sp.result(scope, 'model.isInlineEditing')) {
                        elm.unbind('keydown');   
                    }                    
                }
            };
        })
        .directive('spDateControlPopup', function() {            
            return {
                restrict: 'A',                
                link: function(scope, elm) {
                    var datePickerPopup = elm.parents('ul.dropdown-menu[uib-datepicker-popup-wrap]').first();
                    if (datePickerPopup && datePickerPopup.length) {
                        scope.$emit('spDateControlPopup', datePickerPopup);    
                    }                    
                }
            };
        })
        .directive('spDateControlValidation', function ($parse, spControlValidationProvider, spFieldValidator) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'A',
                replace: false,
                transclude: false,
                scope: false,
                require: 'ngModel',
                priority: 100,     // note: priority is set higher so that this directive runs last and we can inject our parser as the first element of '$parsers' array. (to make our parser run first). 
                link: function(scope, elm, attrs, ctrl) {

                    scope.validationModel = $parse(attrs.spDateControlValidation)(scope);
                    
                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        directiveName: 'spDateControlValidation',
                        typeParser: spFieldValidator.customDateParser,
                        absoluteMinimum: new Date('1900-01-01'),
                        absoluteMaximum: new Date('2100-01-01')
                        //onValidate: onValidate
                    };
                    
                    /////
                    // Run the provider.
                    /////
                    spControlValidationProvider(scope, ctrl, options);
                }
            };
        })
           
        .filter('spDateControlFilter', function() {
            /////
            // Format the value.
            /////
            return function(value) {

                return Globalize.format(value, 'd');
            };
        })

        // #23707:Angular 1.3.0 rc0 causes datepicker to display initial date as unformatted string https://github.com/angular-ui/bootstrap/issues/2659
        // http://plnkr.co/edit/L9u12BeeBgPC2z0nlrLZ?p=preview
        // this is the important bit:
        .directive('datepickerPopup', function () {
        return {
            restrict: 'EAC',
            require: 'ngModel',
            link: function(scope, element, attr, controller) {
                //remove the default formatter from the input directive to prevent conflict
                controller.$formatters.shift();
            }
        };
    });
   
    
}());