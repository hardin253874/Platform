// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport,
 sp, spEntity, jsonString, jsonBool, jsonLookup,
 rnBoardTestData */

describe('rnFeatureSwitch|spec:', function () {
    'use strict';

    beforeEach(module('mod.featureSwitch'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('returns a service object', inject(function (rnFeatureSwitch) {
        expect(rnFeatureSwitch).toBeTruthy();
    }));

    it('the service has all expected functions', inject(function (rnFeatureSwitch) {
        let fns = [
            'isFeatureOn'
        ];
        fns.map(f => expect(_.isFunction(rnFeatureSwitch[f])).toBeTruthy());
    }));

    it('the isFeatureOn function works', inject(function (spAppSettings, rnFeatureSwitch) {
        spAppSettings.initialSettings = {
            featureSwitches: 'testOnFeature'
        };
        let {isFeatureOn} = rnFeatureSwitch;
        expect(isFeatureOn('testOffFeature')).toBeFalsy();
        expect(isFeatureOn('testOnFeature')).toBeTruthy();
        expect(isFeatureOn('TESTONFeatURE')).toBeTruthy();
    }));
});

