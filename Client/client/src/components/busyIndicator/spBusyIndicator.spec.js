// Copyright 2011-2016 Global Software Innovation Pty Ltd

// This module may be used in any spec tests to ensure the busy indicator is loaded.
// e.g. beforeEach(module('test.spBusyIndicator'));
angular.module('test.spBusyIndicator', [
    'mod.common.ui.spBusyIndicator',
    'busyIndicator/spBusyIndicator.tpl.html',
    'busyIndicator/spBusyIndicatorPopup.tpl.html',
    'busyIndicator/spBusyIndicatorPopupBackdrop.tpl.html',
    'busyIndicator/spBusyIndicatorProgressBar.tpl.html',
    'busyIndicator/spBusyIndicatorSpinnerTextBottom.tpl.html'
]);

describe('Console|Controls|spBusyIndicator|spec:|spBusyIndicator directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('test.spBusyIndicator'));
    
    it('should display inline progress bar', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element,
            busyIndicator;

        scope.options = {
            type: 'progressBar',
            text: 'Loading...',
            isBusy: false
        };

        element = angular.element('<sp-busy-indicator options="options"></sp-busy-indicator>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('busyIndicator-view')).toBe(true);
        expect(element.hasClass('ng-hide')).toBe(true);

        busyIndicator = element.find('div.progress');
        expect(busyIndicator.length).toBe(1);

        // Show the control
        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();

        expect(element.hasClass('ng-hide')).toBe(false);

        // Hide the control
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        expect(element.hasClass('ng-hide')).toBe(true);
    }));


    it('should display inline spinner', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element,
            busyIndicator;

        scope.options = {
            type: 'spinner',
            text: 'Loading...',
            isBusy: false
        };

        element = angular.element('<sp-busy-indicator options="options"></sp-busy-indicator>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('busyIndicator-view')).toBe(true);
        expect(element.hasClass('ng-hide')).toBe(true);

        // Show the control
        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();

        expect(element.hasClass('ng-hide')).toBe(false);

        // Hide the control
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        expect(element.hasClass('ng-hide')).toBe(true);
    }));


    it('should display element popup progress bar', inject(function ($rootScope, $compile, $document, $timeout) {
        var scope = $rootScope,
            element,            
            backdrop,
            popupup;

        scope.options = {
            type: 'progressBar',
            text: 'Loading...',
            placement: 'element',
            isBusy: false
        };

        element = angular.element('<div style="width:100px;height:100px" sp-busy-indicator-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');
     
        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);

        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();
        $timeout.flush();


        // Show the popup

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(1);
        expect(popupup.length).toBe(1);

        // Hide the popup
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);
    }));


    it('should display element popup spinner', inject(function ($rootScope, $compile, $document, $timeout) {
        var scope = $rootScope,
            element,
            backdrop,
            popupup;

        scope.options = {
            type: 'spinner',
            text: 'Loading...',
            placement: 'element',
            isBusy: false
        };

        element = angular.element('<div style="width:100px;height:100px" sp-busy-indicator-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);

        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();
        $timeout.flush();

        // Show the popup

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(1);
        expect(popupup.length).toBe(1);

        // Hide the popup
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        backdrop = element.find('div.busyIndicatorBackdropPlacement');
        popupup = element.find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);
    }));


    it('should display window popup progress bar', inject(function ($rootScope, $compile, $document, $timeout) {
        var scope = $rootScope,
            element,
            backdrop,
            popupup;

        scope.options = {
            type: 'progressBar',
            text: 'Loading...',
            placement: 'window',
            isBusy: false
        };

        element = angular.element('<div style="width:100px;height:100px" sp-busy-indicator-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);

        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();
        $timeout.flush();

        // Show the popup

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(1);
        expect(popupup.length).toBe(1);

        // Hide the popup
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);
    }));


    it('should display window popup spinner', inject(function ($rootScope, $compile, $document, $timeout) {
        var scope = $rootScope,
            element,
            backdrop,
            popupup;

        scope.options = {
            type: 'spinner',
            text: 'Loading...',
            placement: 'window',
            isBusy: false
        };

        element = angular.element('<div style="width:100px;height:100px" sp-busy-indicator-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);

        scope.$apply(function () {
            scope.options.isBusy = true;
        });

        scope.$digest();
        $timeout.flush();

        // Show the popup

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(1);
        expect(popupup.length).toBe(1);

        // Hide the popup
        scope.$apply(function () {
            scope.options.isBusy = false;
        });

        scope.$digest();

        backdrop = $document.find('body').find('div.busyIndicatorBackdropPlacement');
        popupup = $document.find('body').find('div.busyIndicatorPopupPlacement');

        expect(backdrop.length).toBe(0);
        expect(popupup.length).toBe(0);
    }));
});