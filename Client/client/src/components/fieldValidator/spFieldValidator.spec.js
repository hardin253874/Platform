// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spFieldValidator_test */

describe('Entity Model|spFieldValidator|spec:', function() {
    "use strict";
    var _spFieldValidator;

    var stringFieldWithPatternJson = {
        id: 9991,
        'name': 'nameField',
        isRequired: false,
        allowMultiLines: false,
        maxLength: jsonInt(),
        minLength: jsonInt(),
        pattern: {
            id: 9992,
            regex: '^[^@]+[@]([a-zA-Z][a-zA-Z0-9-]*)(\\.[a-zA-Z][a-zA-Z0-9-]*)*$',
            regexDescription: 'Blah'
        },
        isOfType: [{
            id: 'stringField'
        }]
    };
    
    var decimalFieldJson = {
        id: 9993,
        'name': 'decField',
        isRequired: false,
       
        isOfType: [{
            id: 'decimalField'
        }]
    };

    beforeEach(module('sp.common.fieldValidator'));

    beforeEach(inject(function(spFieldValidator) {

        _spFieldValidator = spFieldValidator;

        var m = spFieldValidator_test.matchers;
        
        this.addMatchers(spFieldValidator_test.matchers);
    }));


    it("that an empty string cannot fail a pattern match", function () {
        var field = spEntity.fromJSON(stringFieldWithPatternJson);

        var validator = _spFieldValidator.getValidator(field);
        
        expect(validator('')).toHaveNoErrors();

    });
    
    it("that a null string cannot fail a pattern match", function () {
        var field = spEntity.fromJSON(stringFieldWithPatternJson);

        var validator = _spFieldValidator.getValidator(field);

        expect(validator(null)).toHaveNoErrors();

    });
    

    it("that a null decimal is valid", function () {
        var field = spEntity.fromJSON(decimalFieldJson);

        var validator = _spFieldValidator.getValidator(field);

        expect(validator(null)).toHaveNoErrors();

    });
    
    it("date field does not allow alpha characters", function () {
        var inputString = 'foo';
        
        var sanitizedString = inputString.replace(_spFieldValidator.sanitizeDateRegex, '');
        expect(sanitizedString).toEqual('');
    });
    
});



