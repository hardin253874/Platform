// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|directives|spec', function () {
    'use strict';
    
    beforeEach(module('sp.common.directives'));
    var $rootScope, $compile;

    beforeEach(inject(['$compile','$rootScope',function($c, $r) {
        $compile = $c;
        $rootScope = $r;
        
        $rootScope.TestCallback = function (istruncated) {
            if (istruncated)
                console.log('testing ellipsis callback', istruncated);
        };
        spyOn($rootScope, 'TestCallback').andCallThrough();
    }]));

    it('The element text should set', function () {
        var element;
        element = angular.element(' <span style="display: inline-block;width: 206px;height: 1.5em"><span class="singleLineTextField-text-read" ellipsis="{{displayString}}" callback="TestCallback" name="input"></span></span>');
        $compile(element)($rootScope);
        $rootScope.displayString = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit';
        $rootScope.$digest();
        console.log('text: ',element.text());
        expect(element.text()).toEqual($rootScope.displayString);
    });

    it('The callback function has been called', function() {
        var element;
        element = angular.element(' <span style="display: inline-block;width: 206px;height: 1.5em"><span class="singleLineTextField-text-read" ellipsis="{{displayString}}" callback="TestCallback" name="input"></span></span>');
        $compile(element)($rootScope);
        $rootScope.displayString = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit';
        $rootScope.$digest();
        expect($rootScope.TestCallback).toHaveBeenCalled();
    });
});