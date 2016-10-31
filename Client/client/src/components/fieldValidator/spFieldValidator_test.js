// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spFieldValidator_test;

(function (spFieldValidator_test) {

    spFieldValidator_test.matchers = {        
        toHaveNoErrors: function(expectedValue) {
            var validationErrors = this.actual;
            this.message = function() {
                if (validationErrors.length > 0) {
                    return 'Validation errors encountered: ' + validationErrors.join('; ');
                } else {
                    return 'OK';
                }
            };

            return validationErrors.length === 0;
        },

        toHaveErrorContaining: function(expectedValue) {
            var validationErrors = this.actual;
            this.message = function() {
                if (validationErrors.length === 0) {
                    return 'Expected "' + expectedValue + '" but didn\'t get an error.';
                } else {
                    return 'Expected "' + expectedValue + '" but got "' + this.actual + '".';
                }
            };

            return validationErrors.length > 0 && validationErrors[0].indexOf(expectedValue) !== -1;
        }            
    };

})(spFieldValidator_test || (spFieldValidator_test = {}));