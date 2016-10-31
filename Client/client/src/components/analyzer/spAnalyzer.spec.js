// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Reports|Analyzer', function () {
    'use strict';

    // Setup
    beforeEach(module('mod.common.ui.spAnalyzer'));

    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));

    beforeEach(module('app.editFormModules'));

    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        spEntityService.mockGetEntityJSON([{
                    id: 'core:tenantGeneralSettingsInstance',
                    tenantCurrencySymbol: '$',
                    finYearStartMonth:
                        { id: 111 },
                    tenantConsoleThemeSettings:
                        { id: 222 }
                }
            ]
        );
    }));


    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.analyzerPopup-view').remove();        
    }));


    function browserTrigger(element, eventType) {
        if (document.createEvent) {
            var event = document.createEvent('MouseEvents');
            event.initMouseEvent(eventType, true, true, window, 0, 0, 0, 0, 0, false, false,
                false, false, 0, element);
            element.dispatchEvent(event);
        } else {
            element.fireEvent('on' + eventType);
        }
    }


    function changeInputValueTo(el, value, $sniffer, scope) {
        el.val(value);
        el.trigger($sniffer.hasEvent('input') ? 'input' : 'change');
        scope.$digest();
    }

    describe('spAnalyzer|spec:|spAnalyzer directive', function () {                        

        it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $templateCache) {
            var scope = $rootScope,
                element;

            // Setup the analyzer options        
            scope.analyzerOptions =
            {
                isEditMode: false,
                analyzerFields: []
            };

            element = angular.element('<sp-analyzer options="analyzerOptions"></sp-analyzer>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzer-view')).toBe(true);
        }));


        it('apply click raises spAnalyzerEventApplyConditions event', inject(function ($rootScope, $compile, $templateCache) {
            var scope = $rootScope,
                element,
                applyButton,
                spAnalyzerEventApplyConditionsRaised = false,
                operators = [
                    {
                        id: 'Equals',
                        name: 'Equals'
                    }
                ];

            // Setup the analyzer options        
            scope.analyzerOptions =
            {
                isEditMode: false,
                analyzerFields: [
                    {
                        name: 'Name',
                        operators: operators,
                        operator: operators[0].id,
                        type: 'String'
                    }
                ]
            };

            scope.$on('spAnalyzerEventApplyConditions', function (event, fields) {
                spAnalyzerEventApplyConditionsRaised = true;
                expect(fields.length).toBe(1);
                expect(fields[0].operator).toBe('Equals');
                expect(fields[0].value).toBeUndefined();
            });

            element = angular.element('<sp-analyzer options="analyzerOptions"></sp-analyzer>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzer-view')).toBe(true);

            applyButton = element.find(':button:contains(\'Apply\')');
            applyButton.click();

            scope.$digest();

            expect(spAnalyzerEventApplyConditionsRaised).toBe(true);
        }));


        it('reset click raises spAnalyzerEventApplyConditions event', inject(function ($rootScope, $compile, $templateCache) {
            var scope = $rootScope,
                element,
                applyButton,
                spAnalyzerEventApplyConditionsRaised = false,
                operators = [
                    {
                        id: 'Equals',
                        name: 'Equals'
                    }
                ];

            // Setup the analyzer options        
            scope.analyzerOptions =
            {
                isEditMode: false,
                analyzerFields: [
                    {
                        name: 'Name',
                        operators: operators,
                        operator: operators[0].id,
                        type: 'String'
                    }
                ]
            };

            scope.$on('spAnalyzerEventApplyConditions', function (event, fields) {
                spAnalyzerEventApplyConditionsRaised = true;
                expect(fields.length).toBe(1);
                expect(fields[0].operator).toBe('Unspecified');
                expect(fields[0].value).toBe(null);
            });

            element = angular.element('<sp-analyzer options="analyzerOptions"></sp-analyzer>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzer-view')).toBe(true);

            applyButton = element.find(':button:contains(\'Reset\')');
            applyButton.click();

            scope.$digest();

            expect(spAnalyzerEventApplyConditionsRaised).toBe(true);
        }));

        // DISABLED: this test no longer works under jasmine 1.31
        xit('apply click with changes raises spAnalyzerEventApplyConditions event', inject(function ($rootScope, $compile, $templateCache, $sniffer) {
            var scope = $rootScope,
                element,
                applyButton,
                operatorSelects,
                valueInputs,
                spAnalyzerEventApplyConditionsRaised = false,
                operators = [
                    {
                        id: 'Unspecified',
                        name: '[Select]'
                    },
                    {
                        id: 'Equals',
                        name: '=',
                        argCount: 1
                    },
                    {
                        id: 'NotEquals',
                        name: '<>',
                        argCount: 1
                    },
                    {
                        id: 'Contains',
                        name: 'Contains',
                        argCount: 1
                    }
                ];

            // Setup the analyzer options        
            scope.analyzerOptions =
            {
                isEditMode: false,
                analyzerFields: [
                    {
                        name: 'Name',
                        operators: operators,
                        operator: operators[1].id,
                        type: 'String',
                        value: 'Name'
                    },
                    {
                        name: 'Description',
                        operators: operators,
                        operator: operators[1].id,
                        type: 'String',
                        value: 'Description'
                    }
                ]
            };

            scope.$on('spAnalyzerEventApplyConditions', function (event, fields) {
                spAnalyzerEventApplyConditionsRaised = true;
                expect(fields.length).toBe(2);

                // Verify that field 1 has different values
                expect(fields[0].operator).toBe('NotEquals');
                expect(fields[0].value).toBe('NameChanged');

                // Verify that field 2 is unchanged
                expect(fields[1].operator).toBe('Equals');
                expect(fields[1].value).toBe('Description');
            });

            element = angular.element('<sp-analyzer options="analyzerOptions"></sp-analyzer>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzer-view')).toBe(true);

            // Change the operator and value of analyser field 1
            applyButton = element.find(':button:contains(\'Apply\')');

            operatorSelects = element.find('select.operatorSelect');
            valueInputs = element.find('span.valueEditorStyle input');

            // Change field 1 operator to not equals
            operatorSelects[0].value = '2';
            browserTrigger(operatorSelects[0], 'change');
            // Change field 1 value to 'NameChanged'
            changeInputValueTo($(valueInputs[0]), 'NameChanged', $sniffer, scope);

            scope.$digest();

            applyButton.click();

            scope.$digest();

            expect(spAnalyzerEventApplyConditionsRaised).toBe(true);
        }));


        it('changing operator with different argument sets values to null', inject(function ($rootScope, $compile, $templateCache, $sniffer) {
            var scope = $rootScope,
                element,
                applyButton,
                operatorSelects,                
                spAnalyzerEventApplyConditionsRaised = false,
                operators = [
                    {
                        id: 'Unspecified',
                        name: '[Select]'
                    },
                    {
                        id: 'Equals',
                        name: '=',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        id: 'NotEquals',
                        name: '<>',
                        type: 'String',
                        argCount: 1
                    },
                    {
                        id: 'Contains',
                        name: 'Contains',
                        argCount: 1
                    }
                ];

            // Setup the analyzer options        
            scope.analyzerOptions =
            {
                isEditMode: false,
                analyzerFields: [
                    {
                        name: 'Name',
                        operators: operators,
                        operator: operators[1].id,
                        type: 'String',
                        value: 'Name'
                    }                    
                ]
            };

            scope.$on('spAnalyzerEventApplyConditions', function (event, fields) {
                spAnalyzerEventApplyConditionsRaised = true;
                expect(fields.length).toBe(1);

                // Verify that field 1 has a null value
                expect(fields[0].operator).toBe('NotEquals');
                expect(fields[0].value).toBe(null);                
            });

            element = angular.element('<sp-analyzer options="analyzerOptions"></sp-analyzer>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzer-view')).toBe(true);

            // Change the operator and value of analyser field 1
            applyButton = element.find(':button:contains(\'Apply\')');

            operatorSelects = element.find('select.operatorSelect');           

            // Change field 1 operator to not equals
            // This will set the value to be null
            // as the notequal operator is configured with a different type to the equal operator
            // Note - pre Angular 1.4 used the array index as the SELECT option value, 1.4+ uses
            // the $$hashKey if you don't have an explicit "track by".. which we have now added
            operatorSelects[0].value = 'NotEquals';
            browserTrigger(operatorSelects[0], 'change');

            scope.$digest();

            applyButton.click();

            scope.$digest();

            expect(spAnalyzerEventApplyConditionsRaised).toBe(true);
        }));
    });

    describe('spAnalyzerButton|spec:|spAnalyzerButton directive', function () {

        it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $templateCache) {
            var scope = $rootScope,
                element,
                analyzerButton,
                analyzerIsOpen,
                analyzerButtonClicked = false,
                operators = [
                    {
                        id: 'Unspecified',
                        name: '[Select]'
                    },
                    {
                        id: 'Equals',
                        name: '=',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        id: 'NotEquals',
                        name: '<>',
                        type: 'String',
                        argCount: 1
                    },
                    {
                        id: 'Contains',
                        name: 'Contains',
                        argCount: 1
                    }
                ];

            scope.options = {
                // Setup the analyzer options                        
                isEditMode: false,
                analyzerFields: [
                    {
                        name: 'Name',
                        operators: operators,
                        operator: operators[1].id,
                        type: 'String',
                        value: 'Name'
                    }
                ]
            };

            scope.onAnalyzerButtonClicked = function (isOpen) {
                analyzerButtonClicked = true;
                analyzerIsOpen = isOpen;
            };

            element = angular.element('<sp-analyzer-button options="options" on-button-clicked="onAnalyzerButtonClicked(isOpen)"></sp-analyzer-button>');
            $compile(element)(scope);
            scope.$digest();

            // Verify that the html element has been replaced        
            expect(element.prop('localName')).toBe('div');
            expect(element.hasClass('analyzerButton-view')).toBe(true);

            scope.$digest();

            // Click the button to show the analyser
            // after changed the spAnalyzerButton html, remove the class name in button
            analyzerButton = element.find(':button');

            analyzerButton.click();

            scope.$digest();
            
            expect(scope.$$childHead.model.areAnalyzerFiltersActive).toBe(true);
            expect(analyzerButtonClicked).toBe(true);
            expect(analyzerIsOpen).toBe(true);
        }));

    });

    describe('spAnalyzerPopup|spec:|spAnalyzerPopup directive', function () {

        it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $templateCache, $timeout, $document) {
            var scope = $rootScope,
                element,
                body = $document.find('body');

            scope.options = {
                isOpen: false,
                // Setup the analyzer options        
                analyzerOptions:
                {
                    isEditMode: false,
                    analyzerFields: []
                }
            };            

            element = angular.element('<div sp-analyzer-popup="options"></div>');
            $compile(element)(scope);
            scope.$digest();
                        
            expect(body.find('div.analyzerPopup-view').length).toBe(0);

            // Show the popup
            scope.options.isOpen = true;            
            scope.$digest();
            $timeout.flush();

            expect(body.find('div.analyzerPopup-view').length).toBe(1);
        }));

    });
});