// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals xdescribe */

describe('editFormService', function() {
    "use strict";

    var _spEditForm, stringField, decField, numField, currField;
    

    beforeEach(module('mod.app.editFormServices'));

    beforeEach(inject(function (spEditForm) {
        expect(spEditForm).toBeTruthy();
        _spEditForm = spEditForm;

        stringField = spEntity.fromJSON(
        {
            id: 'myStringfield',
            typeId: 'stringField'
        });
        
        decField = spEntity.fromJSON(
        {
            id: 'myDecfield',
            typeId:'decimalField'
        });
        

        currField = spEntity.fromJSON(
        {
            id: 'myCurrfield',
            typeId: 'currencyField'
        });
        
        numField = spEntity.fromJSON(
        {
            id: 'myNumberfield',
            typeId: 'intField'
        });

      
    }));

    describe('formatDecimalForDisplay', function () {
        
        it('will display 1234 as "1,234.000" using the default number of decimal places', function () {
            expect(_spEditForm.formatNumberForDisplay(decField, 1234)).toEqual('1,234.000');
        });
        

        it('will display 1234 with 4 decimal places as "1234.0000"', function () {
            decField.setField('decimalPlaces', 4, spEntity.DataType.Int32);

            expect(_spEditForm.formatNumberForDisplay(decField, 1234)).toEqual('1,234.0000');
        });
        

        it('will display 0 with four decimal places as "0.0000"', function () {
            decField.setField('decimalPlaces', 4, spEntity.DataType.Int32);

            expect(_spEditForm.formatNumberForDisplay(decField, 0)).toEqual('0.0000');
        });
    });
    

    describe('formatCurrencyForDisplay', function () {

        it('will display 1234 as "1,234.00" using the default number of decimal places', function () {
            expect(_spEditForm.formatNumberForDisplay(currField, 1234)).toEqual('1,234.00');
        });


        it('will display 1234 with 4 decimal places as "1234.0000"', function () {
            currField.setField('decimalPlaces', 4, spEntity.DataType.Int32);

            expect(_spEditForm.formatNumberForDisplay(currField, 1234)).toEqual('1,234.0000');
        });


        it('will display 0 with four decimal places as "0.0000"', function () {
            currField.setField('decimalPlaces', 4, spEntity.DataType.Int32);

            expect(_spEditForm.formatNumberForDisplay(currField, 0)).toEqual('0.0000');
        });
        
        it('will display a currency symbol', function () {
            expect(_spEditForm.formatNumberForDisplay(currField, 0, '$')).toEqual('$0.00');
        });
    });
    
    describe('formatNumberForDisplay', function () {

        it('will display 1234 as "1,234"', function () {
            expect(_spEditForm.formatNumberForDisplay(numField, 1234)).toEqual('1,234');
        });


        it('will display 0 as "0"', function () {
            expect(_spEditForm.formatNumberForDisplay(numField, 0)).toEqual('0');
        });

        it('will display null as empty string', function () {
            expect(_spEditForm.formatNumberForDisplay(numField, null)).toEqual('');
        });
        
        it('will display empty string as empty string', function () {
            expect(_spEditForm.formatNumberForDisplay(numField, '')).toEqual('');
        });
    });

    xdescribe('preSaveValidate', function() { // these tests no longer work because the validation mechanism has changed and required the form control code to set up validators
        var baseJson;
        
        beforeEach(function () {
            baseJson = {
                id: 9990,
                'console:containedControlsOnForm': [
                    {
                        id: 9991,
                        name: jsonString(),
                        'console:mandatoryControl': false,
                        'console:fieldToRender':
                        {
                            id: 9992,
                            'name': 'nameField',
                            isRequired: false,
                            allowMultiLines: false,
                            maxLength: jsonInt(),
                            minLength: jsonInt(),
                            pattern: jsonLookup(),
                            isOfType: [{
                                id: 'stringField'
                            }] 
                        }
                    },
                     {
                         id: 9971,
                         name: jsonString(),
                         'console:mandatoryControl': false,
                         'console:showControlHelpText': false,
                         'console:relationshipToRender':
                         {
                             id: 9972,
                             'name': 'nameField'
                            
                         }
                     }]
            };
             

        });
                    
        it('should not generate an error on not updating a non manditory field.', function () {
            var empty = spEntity.createEntityOfType(999);
            
            var formWithNonManditoryNameField = spEntity.fromJSON(baseJson);

            var isValid = _spEditForm.validateForm(formWithNonManditoryNameField, empty);
            expect(isValid).toBeTruthy();
        });
        
        it('should produce an error message when a mandatary field is left unfilled.', function () {
            var empty = spEntity.createEntityOfType(999);

            var formWithManditoryNameField = spEntity.fromJSON(baseJson);
            formWithManditoryNameField.getContainedControlsOnForm()[0].setMandatoryControl(true);
            
            var isValid = _spEditForm.validateForm(formWithManditoryNameField, empty);
            expect(isValid).toBeFalsy();
        });
        
        it('should produce an error message when a mandatary relationship is left unfilled.', function () {
            var empty = spEntity.createEntityOfType(999);

            var formWithManditoryNameField = spEntity.fromJSON(baseJson);
            formWithManditoryNameField.getContainedControlsOnForm()[1].setMandatoryControl(true);

            var isValid = _spEditForm.validateForm(formWithManditoryNameField, empty);
            expect(isValid).toBeFalsy();
        });
        

        it('should produce an error message when a field is less than the minimum length.', function () {
            var empty = spEntity.createEntityOfType(999); 
            empty.setField(9992, 'a', 'String');

            var form = spEntity.fromJSON(baseJson);
            form.getContainedControlsOnForm()[0].getFieldToRender().setField('minLength', 3, 'Integer'); 

            var isValid = _spEditForm.validateForm(form, empty);
            expect(isValid).toBeFalsy();
        });
    });    


    describe('httpserizeUrl', function() {
        it('it should turn a naked URL into one with a http:', function () {

            expect(_spEditForm.httperizeUrl('www.google.com')).toEqual('http://www.google.com');
        });

        it('it should do nothing to one with a http:', function () {

            expect(_spEditForm.httperizeUrl('http://www.google.com')).toEqual('http://www.google.com');
        });
        
        it('it should do nothing to one with a https:', function () {

            expect(_spEditForm.httperizeUrl('https://www.google.com')).toEqual('https://www.google.com');
        });
        

        it('it should do nothing to one with a ftp:', function () {

            expect(_spEditForm.httperizeUrl('ftp://www.google.com')).toEqual('ftp://www.google.com');
        });
    });

    
    describe('createEntityWithDefaults', function () {

        it('it should create an empty entity with no defaults from an empty form:', function () {
            var simpleForm = spEntity.fromJSON(
                {
                    id: 'simpleForm',
                    typeId: 'console:customEditForm',

                    'console:containedControlsOnForm': {
                        id: 'myForm',
                        typeid: 'singleLineTextControl',
                        'console:fieldToRender': {
                            id: 9991,
                            isOfType: [{
                                alias: 'intField'
                            
                        }]
                        }
                    }
                });
            
            _spEditForm.createEntityWithDefaults('myType', simpleForm).then(function (entity) {
                expect(entity.getField(9991)).toBeFalsy();
            });
        });

        it('it should create an entity with a default value in a field', function () {
            var simpleFormWithDefault = spEntity.fromJSON(
                {
                    id: 'simpleForm',
                    typeId: 'console:customEditForm',

                    'console:containedControlsOnForm': { 
                        id: 'myForm',
                        typeid: 'singleLineTextControl',
                        'console:fieldToRender': {
                            id: 9992,
                            defaultValue: 999,
                            isOfType: [{
                                alias: 'intField'
                                
                            }]
                        }
                    }
                });
            
            _spEditForm.createEntityWithDefaults('myType', simpleFormWithDefault).then(function (entity) {
                expect(entity.getField(9992)).toEqual(999);
            });
        });
        
        it('it should create an entity with a default value in a forward relationship', function () {
            var form = spEntity.fromJSON(
                {
                    id: 'simpleForm',
                    typeId: 'console:customEditForm',

                    'console:containedControlsOnForm': [{
                        id: 'myForm',
                        typeid: 'choiceRelationshipRenderControl',
                        isReversed: false,
                        'console:relationshipToRender': {
                            id: 9992,
                            toTypeDefaultValue: { id: 999, typeId: 'myToType' }
                            
                        }
                    }]
                });

            _spEditForm.createEntityWithDefaults('myType', form).then(function (entity) {
                expect(entity.getLookup(9992).id()).toEqual(999);
            });
        });
        
        it('it should create an entity with a default value in a reverse relationship', function () {
            var form = spEntity.fromJSON(
                {
                    id: 'simpleForm',
                    typeId: 'console:customEditForm',

                    'console:containedControlsOnForm': [{
                        id: 'myForm',
                        typeid: 'choiceRelationshipRenderControl',
                        isReversed: true,
                        'console:relationshipToRender': {
                            id: 9992,
                            fromTypeDefaultValue: { id: 999, typeId: 'myToType' }

                        }
                    }]
                });

            _spEditForm.createEntityWithDefaults('myType', form).then(function (entity) {
                expect(entity.getLookup(9992).id()).toEqual(999);
            });
        });
    });

    


});

