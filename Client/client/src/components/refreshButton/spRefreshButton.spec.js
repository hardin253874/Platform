// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport,
 sp, spEntity, jsonString, jsonBool, jsonLookup,
 rnBoardTestData */

describe('Console|Controls|spRefreshButton|spec:|spRefreshButton directive', function () {
    'use strict';

    // Load the modules        
    beforeEach(function () {
        // Stub out the nav service        
        var navServiceStub = {};

        module('mod.common.ui.spRefreshButton', function ($provide) {
            $provide.value('spNavService', navServiceStub);
        });
    });

    beforeEach(module('refreshButton/spRefreshButton.tpl.html'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function (refreshMenuItemConfig) {
        // Set the first item timeout to 3 seconds
        refreshMenuItemConfig.menuItemConfig[0].timeoutMin = 0.05;
    }));


    afterEach(inject(function (refreshMenuItemConfig) {
        // Reset the menu item timeout back to 1 minute
        refreshMenuItemConfig.menuItemConfig[0].timeoutMin = 1;
    }));


    it('should create HTML element with appropriate content', inject(function ($rootScope, $compile) {
        var scope = $rootScope, element;

        // Setup the options        
        scope.options = {
            refreshCallback: function () {
            }, autoRefreshTimeoutMin: 0
        };

        element = angular.element('<sp-refresh-button options="options"></sp-refresh-button>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.prop('localName')).toBe('sp-refresh-button');

        var rootNode = angular.element(element[0].firstElementChild);
        expect(rootNode.prop('localName')).toBe('div');

        expect(rootNode.hasClass('refreshbutton-view')).toBe(true);
    }));


    it('should call refresh callback - manual refresh', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            applyButton,
            element,
            manualRefreshClicked;

        // Setup the options        
        scope.options =
        {
            refreshCallback: function () {
                manualRefreshClicked = true;
            },
            autoRefreshTimeoutMin: 0
        };

        element = angular.element('<sp-refresh-button options="options"></sp-refresh-button>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been created
        expect(element).toBeTruthy();

        var rootNode = angular.element(element[0].firstElementChild);
        expect(rootNode.prop('localName')).toBe('div');
        expect(rootNode.hasClass('refreshbutton-view')).toBe(true);

        applyButton = rootNode.find(':button.refreshButton').first();
        expect(applyButton.is(':disabled')).toBeFalsy();
        applyButton.click();

        scope.$digest();

        expect(manualRefreshClicked).toBe(true);
    }));

    it('should replace call refresh callback - auto refresh', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            counter = 0;

        // Setup the options        
        scope.options =
        {
            refreshCallback: function () {
                counter++;
            },
            autoRefreshTimeoutMin: 0.05
        };

        element = angular.element('<sp-refresh-button options="options"></sp-refresh-button>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been created
        expect(element).toBeTruthy();

        var rootNode = angular.element(element[0].firstElementChild);
        expect(rootNode.prop('localName')).toBe('div');
        expect(rootNode.hasClass('refreshbutton-view')).toBe(true);

        scope.$digest();
        $timeout.flush();

        scope.$digest();
        $timeout.flush();

        expect(counter).toBeGreaterThan(1);
    }));


    it('should have disabled buttons if disabled option is true', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope;

        // Setup the options
        scope.options = {
            disabled: true,
            autoRefreshTimeoutMin: 0
        };

        var element = angular.element('<sp-refresh-button options="options"></sp-refresh-button>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been created
        expect(element).toBeTruthy();

        var rootNode = angular.element(element[0].firstElementChild);
        expect(rootNode.prop('localName')).toBe('div');

        var applyButton = rootNode.find(':button.refreshButton').first();
        expect(applyButton.is(':disabled')).toBeTruthy();
    }));

});