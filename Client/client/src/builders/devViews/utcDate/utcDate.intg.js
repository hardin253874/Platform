// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Internal|UTC|intg:|utcDummyServiceTest', function () {
    'use strict';

    var $injector, $http, $rootScope, $q,
        spWebService, utcDummyService;

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('spApps.utcDummyService'));
    beforeEach(module('sp.common.loginService'));

    beforeEach(inject(function ($injector) {

        // load a DI table with the modules needed
        $http = $injector.get('$http');
        $rootScope = $injector.get('$rootScope');
        $q = $injector.get('$q');
        spWebService = $injector.get('spWebService');
        utcDummyService = $injector.get('utcDummyService');

        TestSupport.setupIntgTests(this, $injector);

        utcDummyService.setWebApiRoot(spWebService.getWebApiRoot());

    }));

    describe('getTzNameFromCustomHeader', function () {

        it('should get correct Windows tz name when passing Olson tz name in custom header', function () {
            var sydneyOlsonTzName = 'Australia/Sydney';
            var expectedSydneyTzText = 'MS TimeZoneId : AUS Eastern Standard Time; System Timezone Display name : (UTC+10:00) Canberra, Melbourne, Sydney';

            // get headers
            var headers = utcDummyService.getHeaders();

            // make sure 'TZ' custom header is present
            expect(headers['Tz']).toBeTruthy();

            // set custom header to Sydney
            headers['Tz'] = sydneyOlsonTzName;


            var result = {};
            TestSupport.waitCheckReturn($rootScope,  utcDummyService.getMsTzFromHeader(), result);

            runs(function () {
                expect(result.value.data).toEqual(expectedSydneyTzText);
            });
        });
    });

    xdescribe('getMsTzFromUrl', function () {

        it('should get correct Windows tz name when passing Olson tz name in query string', function () {
            var sydneyOlsonTzName = 'Australia/Sydney';
            var expectedSydneyTzText = '"MS TimeZoneId : AUS Eastern Standard Time; System Timezone Display name : (UTC+10:00) Canberra, Melbourne, Sydney"';

            var result = {};
            TestSupport.waitCheckReturn($rootScope, utcDummyService.getMsTzFromUrl(sydneyOlsonTzName), result);

            runs(function () {
                expect(result.value.data).toEqual(expectedSydneyTzText);
            });
        });
    });
});