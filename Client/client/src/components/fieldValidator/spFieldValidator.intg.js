// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spFieldValidator_test */

describe('Entity Model|spFieldValidator|intg:', function() {
    "use strict";

    var $injector, $rootScope;

    var spEntityService, spFieldValidator;

    var decField, numberField, currencyField, singleLineField, multiLineField, nameField, descriptionField, dateField;


    beforeEach(module('sp.common.loginService'));
    beforeEach(module('sp.common.fieldValidator'));
    beforeEach(module('mod.common.spEntityService'));

    beforeEach(inject(function ($injector) {
        var done, errorMsg, testFields;

        $rootScope = $injector.get('$rootScope');
        spEntityService = $injector.get('spEntityService');
        spFieldValidator = $injector.get('spFieldValidator');

        TestSupport.setupIntgTests(this, $injector);
    }));

    beforeEach(inject(function () {
        var done, errorMsg, testFields;
        
        testFields = ['test:decimalDecimalField3192gen', 'test:numberIntFieldTestField', 'test:currencyCurrencyTestField', 'test:singleTextStringFieldTestField', 'test:multiTextStringFieldTestField', 'core:name', 'core:description', 'test:dateDateFieldTestField'];

        var getfields =  spEntityService.getEntities(testFields, spFieldValidator.getFieldQueryFragment());
        TestSupport.wait(getfields 
            .then(
                function(entities) {

                    decField = entities[0];
                    numberField = entities[1];
                    currencyField = entities[2];
                    singleLineField = entities[3];
                    multiLineField = entities[4];
                    nameField = entities[5];
                    descriptionField = entities[6];
                    dateField = entities[7];
                    done = true;

                },
                function(err) {
                    done = true;

                    errorMsg = err.ExceptionMessage || err.toString();
                    console.log('error', err, errorMsg);
                }));

        $rootScope.$apply();


        waitsFor(function() {
            return done;
        }, "get entities should return", 10000);

        runs(function() {
            expect('the http request failed with error: ' + errorMsg, errorMsg).toBeTruthy();
            expect(decField).toBeTruthy();
            expect(numberField).toBeTruthy();
            expect(currencyField).toBeTruthy();
            expect(singleLineField).toBeTruthy();
            expect(multiLineField).toBeTruthy();
            expect(nameField).toBeTruthy();
            expect(descriptionField).toBeTruthy();
            expect(dateField).toBeTruthy();

        });
        
        this.addMatchers(spFieldValidator_test.matchers);
    }));


    it("Validate number", function () {
        var validator = spFieldValidator.getValidator(numberField);
        
	    expect(validator('123')).toHaveNoErrors();
        expect(validator('-123')).toHaveNoErrors();
        expect(validator('1000000000')).toHaveNoErrors();
        expect(validator('1000000001')).toHaveErrorContaining('max');
        expect(validator('1000000001')).toHaveErrorContaining('1,000,000,000');
        expect(validator('-1000000000')).toHaveNoErrors();
        expect(validator('-1000000001')).toHaveErrorContaining('min');
        expect(validator('-1000000001')).toHaveErrorContaining('-1,000,000,000');
    });
    
    it("Validate number max and min values", function () {
        var validator;

        numberField.setMaxInt(7);
        numberField.setMinInt(-6);

        validator = spFieldValidator.getValidator(numberField);

        expect(validator('7')).toHaveNoErrors();
        expect(validator('8')).toHaveErrorContaining('between');
        expect(validator('-6')).toHaveNoErrors();
        expect(validator('-7')).toHaveErrorContaining('between');
    });

    it("Validate number zero max or min values", function () {
        numberField.setMinInt(0);

        var validator = spFieldValidator.getValidator(numberField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('-1')).toHaveErrorContaining('min');

        numberField.setMinInt(null);
        numberField.setMaxInt(0);

        validator = spFieldValidator.getValidator(numberField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('1')).toHaveErrorContaining('max');
    });

    it("Validate decimal", function () {
        var validator = spFieldValidator.getValidator(decField);

        expect(validator('123')).toHaveNoErrors();
        expect(validator('-123')).toHaveNoErrors();
        expect(validator('123.456')).toHaveNoErrors();
        expect(validator('999999999.99')).toHaveNoErrors();
        expect(validator('1000000000.01')).toHaveErrorContaining('max');
        expect(validator('1000000000.01')).toHaveErrorContaining('1,000,000,000');
        expect(validator('-999999999.99')).toHaveNoErrors();
        expect(validator('-1000000000.01')).toHaveErrorContaining('min');
        expect(validator('-1000000000.01')).toHaveErrorContaining('-1,000,000,000');
    });


    it("Validate decimal max and min values", function () {
        var validator;

        decField.setMaxDecimal(7.8);
        decField.setMinDecimal(-6.8);

        validator = spFieldValidator.getValidator(decField);

        expect(validator('7.8')).toHaveNoErrors();
        expect(validator('-6.8')).toHaveNoErrors();
        expect(validator('7.9')).toHaveErrorContaining('range');
        expect(validator('-6.9')).toHaveErrorContaining('range');
    });

    it("Validate decimal zero max or min values", function () {
        decField.setMinDecimal(0);

        var validator = spFieldValidator.getValidator(decField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('-0.5')).toHaveErrorContaining('min');

        decField.setMinDecimal(null);
        decField.setMaxDecimal(0);

        validator = spFieldValidator.getValidator(decField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('0.5')).toHaveErrorContaining('max');
    });
    
    it("Validate currency", function () {
        var validator = spFieldValidator.getValidator(currencyField);

        expect(validator('123')).toHaveNoErrors();
        expect(validator('-123')).toHaveNoErrors();
        expect(validator('123.456')).toHaveNoErrors();
        expect(validator('999999999.99')).toHaveNoErrors();
        expect(validator('1000000000.01')).toHaveErrorContaining('max');
        expect(validator('-999999999.99')).toHaveNoErrors();
        expect(validator('-1000000000.01')).toHaveErrorContaining('min');
    });

    it("Validate currency max and min values", function () {
        var validator;

        currencyField.setMaxDecimal(7.8);
        currencyField.setMinDecimal(-6.8);

        validator = spFieldValidator.getValidator(currencyField);

        expect(validator('7.8')).toHaveNoErrors();
        expect(validator('-6.8')).toHaveNoErrors();
        expect(validator('7.9')).toHaveErrorContaining('range');
        expect(validator('-6.9')).toHaveErrorContaining('range');
    });

    it("Validate currency zero max or min values", function () {
        currencyField.setMinDecimal(0);

        var validator = spFieldValidator.getValidator(currencyField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('-0.5')).toHaveErrorContaining('min');

        currencyField.setMinDecimal(null);
        currencyField.setMaxDecimal(0);

        validator = spFieldValidator.getValidator(currencyField);

        expect(validator('0')).toHaveNoErrors();
        expect(validator('0.5')).toHaveErrorContaining('max');
    });

    it('Validate single line stringField', function() {
        var validator = spFieldValidator.getValidator(singleLineField);
        expect(validator("a good string")).toHaveNoErrors();
    });
    

    it('Validate single line stringField max length', function () {
        var validator = spFieldValidator.getValidator(singleLineField);

        var maxString = (new Array(1000 + 1)).join("x");    // join creates n-1 characters ni the string 
        var overMaxString = (new Array(1001 + 1)).join("x");

        expect(validator(maxString)).toHaveNoErrors();
        expect(validator(overMaxString)).toHaveErrorContaining('max');
    });
    

    it('Validate multi line stringField max length', function () {
        var validator = spFieldValidator.getValidator(multiLineField);

        var maxString = (new Array(10000 + 1)).join("x");    // join creates n-1 characters ni the string 
        var overMaxString = (new Array(10001 + 1)).join("x");

        expect(validator(maxString)).toHaveNoErrors();
        expect(validator(overMaxString)).toHaveErrorContaining('max');
    });

    it('Validate stringField field min length', function () {
        singleLineField.setMinLength(4);
        
        var validator = spFieldValidator.getValidator(singleLineField);

        expect(validator("1234")).toHaveNoErrors();
        expect(validator("123")).toHaveErrorContaining('min');
    });
    

    it('Validate stringField field max length', function () {
        singleLineField.setMaxLength(4);

        var validator = spFieldValidator.getValidator(singleLineField);

        expect(validator("1234")).toHaveNoErrors();
        expect(validator("12345")).toHaveErrorContaining('max');
    });
    
    it('Validate stringField pattern', function () {
        var validator = spFieldValidator.getValidator(nameField);

        expect(validator("I am a good name")).toHaveNoErrors();
        expect(validator("I am a < bad name")).toHaveErrorContaining('must not contain angled brackets');
    });
    

    it('Validate name length <= 200', function () {
        var validator = spFieldValidator.getValidator(nameField);

        var maxString = (new Array(200 + 1)).join("x");    // join creates n-1 characters ni the string 
        var overMaxString = (new Array(201 + 1)).join("x");    // join creates n-1 characters ni the string 

        expect(validator(maxString)).toHaveNoErrors();
        expect(validator(overMaxString)).toHaveErrorContaining('max');
    });
    
    it("Validate date", function () {
        var validator = spFieldValidator.getValidator(dateField);

        expect(validator(new Date('1/1/2014'))).toHaveNoErrors();
    });

    it("Validate date max and min values", function () {
        var validator;

        dateField.setMaxDate(new Date('1/1/2000'));
        dateField.setMinDate(new Date('1/1/1999'));

        validator = spFieldValidator.getValidator(dateField);

        expect(validator(new Date('2/2/1999'))).toHaveNoErrors();
        expect(validator(new Date('1/1/1988'))).toHaveErrorContaining('between');
        expect(validator(new Date('1/1/2001'))).toHaveErrorContaining('between');
    });

    it("Validate date zero max or min values", function () {
        dateField.setMinDate(new Date('1/1/1999'));

        var validator = spFieldValidator.getValidator(dateField);

        expect(validator(new Date('1/1/2001'))).toHaveNoErrors();
        expect(validator(new Date('1/1/1988'))).toHaveErrorContaining('min');

        dateField.setMinDate(null);
        dateField.setMaxDate(new Date('1/1/2000'));

        validator = spFieldValidator.getValidator(dateField);

        expect(validator(new Date('1/1/1999'))).toHaveNoErrors();
        expect(validator(new Date('1/1/2001'))).toHaveErrorContaining('max');
    });
    

    it('Validate name regex does not fail if the name is null', function () {
        var validator = spFieldValidator.getValidator(nameField);

        expect(validator(null)).toHaveNoErrors();
    });
    
    it('Validate name regex does not fail if the name is empty string', function () {
        var validator = spFieldValidator.getValidator(nameField);

        expect(validator('')).toHaveNoErrors();
    });
    
    it('Validate number regex does not fail if the value is null or empty', function () {
        var validator = spFieldValidator.getValidator(numberField);

        expect(validator(null)).toHaveNoErrors();
        expect(validator('')).toHaveNoErrors();
        expect(validator(' ')).toHaveNoErrors();
    });
    
    it('Validate decimal regex does not fail if the value is null or empty or blank', function () {
        var validator = spFieldValidator.getValidator(decField);

        expect(validator(null)).toHaveNoErrors();
        expect(validator('')).toHaveNoErrors();
        expect(validator(' ')).toHaveNoErrors();
    });


    it('Validate currency regex does not fail if the value is null or empty or blank', function () {
        var validator = spFieldValidator.getValidator(currencyField);

        expect(validator(null)).toHaveNoErrors();
        expect(validator('')).toHaveNoErrors();
        expect(validator(' ')).toHaveNoErrors();
    });


    it('Validate description length <= 10000', function () {
        var validator = spFieldValidator.getValidator(descriptionField);

        var maxString = (new Array(10000 + 1)).join("x");    // join creates n-1 characters ni the string 
        var overMaxString = (new Array(10001 + 1)).join("x");    // join creates n-1 characters ni the string 

        expect(validator(maxString)).toHaveNoErrors();
        expect(validator(overMaxString)).toHaveErrorContaining('max');
    });
    

    it('sanitizer trims a name field to 200', function () {
        var trimmer = spFieldValidator.getSanitizer(nameField);

        var maxString = (new Array(200 + 1)).join("x");    // join creates n-1 characters ni the string 
        var overMaxString = (new Array(2001 + 1)).join("x");    // join creates n-1 characters ni the string 

        expect(trimmer(maxString)).toEqual(maxString);
        expect(trimmer(overMaxString).length).toEqual(200);
    });
    

    it('sanitizer doesnt do anything to a currency field', function () {
        var trimmer = spFieldValidator.getSanitizer(currencyField);

        var maxString = (new Array(2000 + 1)).join("1");    // join creates n-1 characters ni the string 

        expect(trimmer(maxString)).toEqual(maxString);
    });
    

    it('sanitizer to only allow [-0..9] in a number field', function () {
        var san = spFieldValidator.getSanitizer(numberField);

        expect(san('aa-123xx')).toEqual('-123');
    });
    
    it('sanitizer to only allow [-.0..9] in a decimal field', function () {
        var san = spFieldValidator.getSanitizer(decField);

        expect(san('aa-123.23xx')).toEqual('-123.23');
    });
    
    it('sanitizer to only allow [-.0..9] in a currency field', function () {
        var san = spFieldValidator.getSanitizer(currencyField);

        expect(san('aa-123.23xx')).toEqual('-123.23');
    });
    
    it('sanitizer doesnt do anything to an empty string in a decimal field', function () {
        var san = spFieldValidator.getSanitizer(decField);

        expect(san('')).toEqual('');
    });
    

    it('sanitizer doesnt do anything to a null in a decimal field', function () {
        var san = spFieldValidator.getSanitizer(decField);

        expect(san('')).toEqual('');
    });
    
    it("decimal field paser should parse values", function () {
        var parser = spFieldValidator.getParser(decField);
        expect(parser(null)).toBeUndefined();
        expect(parser("")).toBeUndefined();
        expect(parser("1.1")).toEqual(1.1);
        expect(parser("-1.1")).toEqual(-1.1);
    });

    it("number field paser should parse values", function () {
        var parser = spFieldValidator.getParser(numberField);
        expect(parser(null)).toBeUndefined();
        expect(parser("")).toBeUndefined();
        expect(parser("1")).toEqual(1);
        expect(parser("-1")).toEqual(-1);
    });
    
    it("currency field paser should parse values", function () {
        var parser = spFieldValidator.getParser(currencyField);
        expect(parser(null)).toBeUndefined();
        expect(parser("")).toBeUndefined();
        expect(parser("1.1")).toEqual(1.1);
        expect(parser("-1.1")).toEqual(-1.1);
    });
});



