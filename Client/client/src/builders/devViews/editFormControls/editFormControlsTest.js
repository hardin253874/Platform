// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /////
    // EditForm controls test controller.
    /////
    angular.module('app.editFormControlsTest', ['mod.common.ui.spDialogService'])
        .controller('editFormControlsTestController', function($scope, spDialogService) {

            /////
            // Refresh the entire form control to cause the edit form to refresh (rather than watching each child property)
            /////

            function refreshFormControl(model) {
                model.formControl = spEntity.fromJSON(model.formControlJSON);
            }

            /////
            // Refresh the form data to cause the edit form to refresh (rather than watching each child property)
            /////

            function refreshFormData(model) {
                model.formData = jQuery.extend(true, {}, model.formData);
            }

            /////
            // Scroll using contents
            /////
            function initializeContents() {

                var links = $(".editFormControlContents > li > a.scroll-link");

                if (links && links.length > 0) {
                    _.forEach(links, function (link) {
                        $(link).click(function () {
                            var url = $(this).attr("href");
                            var content = $('.layout-content');

                            content.animate({
                                scrollTop: $(url).offset().top - content.offset().top + 32
                            }, 1000);
                            return false;
                        });
                    });
                }

                links = $(".home-link");

                if (links && links.length > 0) {
                    _.forEach(links, function (link) {
                        $(link).click(function () {
                            var content = $('.layout-content');

                            content.animate({
                                scrollTop: 0
                            }, 500);

                            return false;
                        });
                    });
                }
            }           

            /////
            // spNumberControl model
            /////
            $scope.spNumberControlModel = {
                value: 123,
                minimumValue: 100,
                maximumValue: 1000,
                isReadOnly: false,
                isRequired: true,
                prefix: '',
                suffix: '',
                isInTestMode: true
            };

            /////
            // spNumericKFieldRenderControl model
            /////
            ///
            $scope.spNumericKFieldRenderControlModel = {
                formControlJSON:
                    {
                        name: 'Numeric',
                        description: 'numeric description',
                        isMandatoryForForm: false,

                            mandatoryControl: false,
                            fieldToRender: {
                                id: 0,
                                minInt: 100,
                                maxInt: 1000,
                                isRequired: true,
                                isOfType: [{
                                    id: 'intField'
                                }]
                            }
                        },
                formData: {
                    _field: 123,
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };


            /////
            // spCurrencyControl model
            /////
            $scope.spCurrencyControlModel = {
                value: 123.4567,
                minimumValue: 100.100,
                maximumValue: 1000.100,
                decimalPlaces: 4,
                isReadOnly: false,
                isRequired: true,
                prefix: '',
                suffix: '',
                isInTestMode: true
            };

            /////
            // spCurrencyKFieldRenderControl model
            /////
            $scope.spCurrencyKFieldRenderControlModel = {
                formControlJSON:
                        {
                            name: 'Currency',
                            description: 'Currency description',
                            isMandatoryForForm: false,
                            mandatoryControl: false,
                            fieldToRender: {
                                id: 0,
                                decimalPlaces: 4,
                                minDecimal: 100,
                                maxDecimal: 1000,
                                isRequired: true,
                                isOfType: [{
                                    id: 'currencyField'
                                }]
                            }
                        },

                formData: {
                    _field: 123.456,
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };

            /////
            // spDecimalControl model
            /////
            $scope.spDecimalControlModel = {
                value: 123.456789,
                minimumValue: 100.100,
                maximumValue: 1000.100,
                decimalPlaces: 4,
                isReadOnly: false,
                isRequired: true,
                prefix: '',
                suffix: '',
                isInTestMode: true
            };

            /////
            // spDecimalKFieldRenderControl model
            /////
            $scope.spDecimalKFieldRenderControlModel = {
                formControlJSON:
                        {
                            name: 'Decimal',
                            description: 'Decimal description',
                            isMandatoryForForm: false,
                            mandatoryControl: false,
                            fieldToRender: {
                                id: 0,
                                decimalPlaces: 4,
                                minDecimal:  100.100,
                                maxDecimal: 1000.100,
                                isRequired: true,
                                isOfType: [{
                                    id: 'decimalField'
                                }]
                            }
                        },
                
                formData: {
                    _field: 123.456,
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };

            /////
            // spSingleLineTextControl model
            /////
            $scope.spSingleLineTextControlModel = {
                formControlJSON:
                        {
                            name: 'Single Line',
                            description: 'Single Line description',
                            isMandatoryForForm: false,
                            mandatoryControl: false,
                            readOnlyControl: false,
                            pattern: [{id:20, regex: ''}],
                            fieldToRender: {
                                id: 0,
                                maximumLength: 10,
                                minimumLength: 1,
                                isRequired: true,
                                allowMultiLines: false,
                                isOfType: [{
                                    id: 'stringField'
                                }]
                                
                                
                            }
                        },
                
                formData: {
                    _field: 'The quick brown fox jumped over the lazy dog.',
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };
              
            
            /////
            // spAutoNumberControl model
            /////
            $scope.spAutoNumberControlModel = {
                value: 123,
                prefix: '',
                suffix: '',
                isInTestMode: true,
                isReadOnly: true
            };

            /////
            // spAutoNumberKFieldRenderControl model
            /////
            $scope.spAutoNumberKFieldRenderControlModel = {
                formControlJSON:
                       {
                           name: 'Autonumber',
                           description: 'Autonumber description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               isRequired: true,
                               autoNumberDisplayPattern: '',
                               isOfType: [{
                                   id: 'autoNumberField'
                               }]
                           }
                       },
                
               formData: {
                    _field: 123,
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isInTestMode: true
            };

            /////
            // spCheckboxControl model
            /////
            $scope.spCheckboxControlModel = {
                value: false,
                label: 'This is a test',
                prefix: '',
                suffix: '',
                isInTestMode: true,
                isReadOnly: false
            };

            /////
            // spCheckboxKFieldRenderControl model
            /////
            $scope.spCheckboxKFieldRenderControlModel = {
                formControlJSON:
                       {
                           name: 'Checkbox',
                           description: 'Checkbox description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               isRequired: true,
                               isOfType: [{
                                   id: 'boolField'
                               }]
                           }
                       },
                
                formData: {
                    _field: false,
                    getField: function () {
                        return this._field;
                    },
                    setField: function (id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };

            /////
            // spDateControl model
            /////
            $scope.spDateControlModel = {
                value: new Date('2013-12-15'),
                minimumValue: new Date('2013-12-01'),
                maximumValue: new Date('2013-12-31'),
                prefix: '',
                suffix: '',
                isRequired: true,
                isMandatory: true,
                isInTestMode: true,
                isReadOnly: false
            };

            /////
            // spDateKFieldRenderControl model
            /////
            $scope.spDateKFieldRenderControlModel = {
                formControlJSON:
                       {
                           name: 'Date',
                           description: 'Date description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               decimalPlaces: 4,
                               minDate: jsonDate(new Date('2013-12-01')),
                               maxDate: jsonDate(new Date('2013-12-31')),
                               isRequired: true,
                               isOfType: [{
                                   id: 'dateField'
                               }]
                           }
                       },
                
                 formData: {
                    _field: new Date('2013-12-15'),
                    getField: function() {
                        return this._field;
                    },
                    setField: function(id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };
            
            ///
            // spDateConfigControl model
            ///
            $scope.spDateConfigControlModel = {
                formControlJSON:
                       {
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               decimalPlaces: 4,
                               minDate: jsonDate(new Date('2013-12-01')),
                               maxDate: jsonDate(new Date('2013-12-31')),
                               isRequired: true,
                               isOfType: [{
                                   id: 'stringField'    // ** 'stringField' not 'dateField'
                               }]
                           }
                       },

                formData: {
                    _field: new Date('2013-12-25').toISOString(), //null,//
                    getField: function () {
                        return this._field;
                    },
                    setField: function (id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
                //dateValue: new Date('2013-12-15'),
                //value: new Date('2013-12-15'),
                //minimumValue: new Date('2013-12-01'),
                //maximumValue: new Date('2013-12-31'),
                //prefix: '',
                //suffix: '',
                //isRequired: true,
                //isMandatory: true,
                //isInTestMode: true,
                //isReadOnly: false,
                //useToday: false,
                //disableControl: false,
                //updateInputValue: function () {
                //    if ($scope.spDateConfigControlModel.useToday) {
                //        $scope.spDateConfigControlModel.useToday = false;
                //    }
                    
                //    $scope.spDateConfigControlModel.value = spUtils.parseDate($scope.spDateConfigControlModel.dateValue);
                //},
                //setToday: function () {
                //    if ($scope.spDateConfigControlModel.useToday) {
                //        $scope.spDateConfigControlModel.value = 'TODAY';
                //    }
                //    else {
                //        $scope.spDateConfigControlModel.value = spUtils.parseDate($scope.spDateConfigControlModel.dateValue);
                //    }
                //}
            };
            //$scope.spDateConfigControlInputModel = {
            //    value: $scope.spDateConfigControlModel.dateValue
            //};
            
            ///
            // spTimeControl model
            ///
            $scope.spTimeControlModel = {
                value: new Date(1973, 0, 1, 9, 15, 0, 0),
                minimumValue: new Date(1973, 0, 1, 9, 0, 0, 0),
                maximumValue: new Date(1973, 0, 1, 17, 0, 0, 0),
                prefix: '',
                suffix: '',
                isRequired: false,
                isMandatory: true,
                isInTestMode: true,
                isReadOnly: false
            };
            
            /////
            // spTimeKFieldRenderControl model
            /////
            $scope.spTimeKFieldRenderControlModel = {
                formControlJSON:
                       {
                           name: 'Time',
                           description: 'Time description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               minTime: jsonTime(spUtils.translateToServerStorageDateTime(new Date(1973, 0, 1, 9, 0, 0, 0))),
                               maxTime: jsonTime(spUtils.translateToServerStorageDateTime(new Date(1973, 0, 1, 17, 0, 0, 0))),
                               isRequired: true,
                               isOfType: [{
                                   id: 'timeField'
                               }]
                           }
                       },

                formData: {
                    _field: spUtils.translateToServerStorageDateTime(new Date(1973, 0, 1, 9, 15, 0, 0)),
                    getField: function () {
                        return this._field;
                    },
                    setField: function (id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };
            
            ///
            // spDateAndTimeControl model
            ///
            $scope.spDateAndTimeControlModel = {
                value: new Date(1973, 0, 1, 9, 15, 0, 0),
                minimumValue: new Date(1973, 0, 1, 9, 0, 0, 0),
                maximumValue: new Date(1973, 1, 1, 17, 0, 0, 0),
                prefix: '',
                suffix: '',
                isRequired: true,
                isMandatory: true,
                isInTestMode: true,
                isReadOnly: false
            };
            
            /////
            // spDateAndTimeKFieldRenderControl model
            /////
            $scope.spDateAndTimeKFieldRenderControlModel = {
                formControlJSON:
                       {
                           name: 'Date and Time',
                           description: 'Date and Time description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               minDateTime: jsonDateTime(new Date(1960, 0, 1, 9, 0, 0, 0)),
                               maxDateTime: jsonDateTime(new Date(1960, 1, 1, 17, 0, 0, 0)),
                               isRequired: true,
                               isOfType: [{
                                   id: 'dateTimeField'
                               }]
                           }
                       },

                formData: {
                    _field: new Date(1960, 0, 1, 9, 15, 0, 0),
                    getField: function () {
                        return this._field;
                    },
                    setField: function (id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
            };
            
            ///
            // spDateAndTimeConfigControl model
            ///
            $scope.spDateAndTimeConfigControlModel = {
                formControlJSON:
                       {
                           name: 'Date and Time config',
                           description: 'Date and Time description',
                           isMandatoryForForm: false,
                           mandatoryControl: false,
                           fieldToRender: {
                               id: 0,
                               minDateTime: jsonDateTime(new Date(1950, 0, 1, 9, 0, 0, 0)),
                               maxDateTime: jsonDateTime(new Date(1950, 1, 1, 17, 0, 0, 0)),
                               isRequired: true,
                               isOfType: [{
                                   id: 'stringField'    // ** 'stringField' not 'dateTimeField'
                               }]
                           }
                       },

                formData: {
                    _field: new Date(1950, 0, 1, 9, 15, 0, 0).toISOString(),
                    getField: function () {
                        return this._field;
                    },
                    setField: function (id, value) {
                        this._field = value;
                    }
                },
                isReadOnly: false,
                isInTestMode: true
                //dateValue: new Date(1960, 0, 1, 9, 0, 0, 0),
                //value: new Date(1965, 0, 1, 9, 15, 0, 0),
                //minimumValue: jsonDateTime(new Date(1960, 0, 1, 9, 0, 0, 0)),
                //maximumValue: jsonDateTime(new Date(1960, 1, 1, 17, 0, 0, 0)),
                //prefix: '',
                //suffix: '',
                //isRequired: true,
                //isMandatory: true,
                //isInTestMode: true,
                //isReadOnly: false,
                //useNow: false,
                //updateInputValue: function () {
                //    if ($scope.spDateAndTimeConfigControlModel.useNow) {
                //        $scope.spDateAndTimeConfigControlModel.useNow = false;
                //    }

                //    $scope.spDateAndTimeConfigControlModel.value = spUtils.parseDate($scope.spDateAndTimeConfigControlModel.dateValue);
                //},
                //setNow: function () {
                //    if ($scope.spDateAndTimeConfigControlModel.useNow) {
                //        $scope.spDateAndTimeConfigControlModel.value = 'NOW';
                //    }
                //    else {
                //        $scope.spDateAndTimeConfigControlModel.value = spUtils.parseDate($scope.spDateAndTimeConfigControlModel.dateValue);
                //    }
                //}
            };

            /////
            // Convert the date to a string.
            /////
            $scope.convertDateToString = function (value) {
                if (!_.isDefined(value) || _.isNull(value)) {
                    return '';
                }

                return Globalize.format(value, 'd');
            };

            // Refresh the form control when the JSON changes
            $scope.$watch('spNumericKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spNumericKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spCurrencyKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spCurrencyKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDecimalKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDecimalKFieldRenderControlModel);
            }, true);

            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spAutoNumberKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spAutoNumberKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spCheckboxKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spCheckboxKFieldRenderControlModel);
            }, true);

            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDateKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDateKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDateConfigControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDateConfigControlModel);
            }, true);
            
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spTimeKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spTimeKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDateAndTimeControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDateAndTimeControlModel);
            }, true);

           
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDateAndTimeKFieldRenderControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDateAndTimeKFieldRenderControlModel);
            }, true);
            
            // Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spDateAndTimeConfigControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spDateAndTimeConfigControlModel);
            }, true);


			// Refresh the form control when the isMandatory checkbox is changed.
            $scope.$watch('spSingleLineTextControlModel.formControlJSON', function (newVal, oldVal) {
                refreshFormControl($scope.spSingleLineTextControlModel);
            }, true);


            initializeContents();
        }).directive('formattedDate', function() {
            return {
                require: '^ngModel',
                restrict: 'A',
                link: function (scope, elm, attrs, ctrl) {

                    function isUndefinedOrNull(value) {
                        return _.isUndefined(value) || _.isNull(value);
                    }

                    function formatDate(date) {
                        var d = new Date(date || Date.now()),
                            month = '' + (d.getMonth() + 1),
                            day = '' + d.getDate(),
                            year = d.getFullYear();

                        if (month.length < 2) month = '0' + month;
                        if (day.length < 2) day = '0' + day;

                        return [year, month, day].join('-');
                    }

                    ///////
                    //// Add a formatter.
                    ///////
                    //ctrl.$formatters.unshift(function (modelValue) {
                        
                    //    if (isUndefinedOrNull(modelValue))
                    //        return '';

                    //    var retVal = formatDate(modelValue);
                    //    return retVal;
                    //});

                    ///////
                    //// Add a parser.
                    ///////
                    //ctrl.$parsers.unshift(function (viewValue) {
                        
                    //    var date = new Date(viewValue);
                    //    return date;
                    //});
                }
            };
        });
}());