// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */

describe('Internal|spTenantSettings library|spec:', function () {
    'use strict';

    var entityService, tenantSettings;
    
    beforeEach(module('mod.common.spTenantSettings'));
    beforeEach(module('mockedEntityService'));
    

    // Load the module that contains the directive with a mocked entity service
    beforeEach(inject(function (spEntityService, spTenantSettings) {
        spEntityService.mockGetEntityJSON(
            [
                {
                    id: 'core:tenantGeneralSettingsInstance',
                    tenantCurrencySymbol: '@',
                    finYearStartMonth:
                        { id: 111 },
                    tenantConsoleThemeSettings:
                        { id: 222 }
                }
            ]
            );
        spEntityService.mockGetEntityJSON(
            [
                {
                    id: 'console:tenantConsoleTheme'
                },
                {
                    id: 'core:fullConfigButton'
                },
                {
                    id: 'core:selfServiceConfigButton'
                },
                {
                    id: 'console:adminToolboxStaticPage'
                },
                {
                    id: 'core:name'
                }
            ]
            );
        spEntityService.mockGetInstancesOfTypeRawData('core:solution', entityTestData.thumbnailSizesTestData);
        entityService = spEntityService;
        tenantSettings = spTenantSettings;
    }));
    

   

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });
    
    it('should get a financial year', inject(function ($rootScope) {
        
        $rootScope.$broadcast('signedin');

        var fyPromise = tenantSettings.getFinYearStartMonth();

        var fy = {};

        TestSupport.waitCheckReturn($rootScope, fyPromise, fy);

        runs(function() {
            expect(fy).toBeTruthy();
            expect(fy.value.id()).toEqual(111);
        });
    }));
    
    it('should get a currency symbol of @', inject(function ($rootScope) {
        
        $rootScope.$broadcast('signedin');

        var currPromise = tenantSettings.getCurrencySymbol();

        var curr = {};

        TestSupport.waitCheckReturn($rootScope, currPromise, curr);

        runs(function () {
            expect(curr).toBeTruthy();
            expect(curr.value).toEqual('@');
        });
    }));
    
    it('should get console theme', inject(function ($rootScope) {
        
        $rootScope.$broadcast('signedin');

        var themePromise = tenantSettings.getTenantTheme();

        var theme = {};

        TestSupport.waitCheckReturn($rootScope, themePromise, theme);

        runs(function () {
            expect(theme).toBeTruthy();
            expect(theme.value.id()).toEqual(222);
        });
    }));

    it('should deal with a sign out and a sign in', inject(function ($rootScope) {
        var curr1 = {};
        var curr2 = {};
        var curr3 = {};        
        var done1, done2, done3;    

        runs(function() {
            $rootScope.$broadcast('signedin');        

            tenantSettings.getCurrencySymbol().then(function(result1) {            
                curr1 = result1;
                done1 = true;                        
            });

            $rootScope.$apply();
        });

        waitsFor(function() {
            return done1;
        });    

        runs(function () {
            $rootScope.$broadcast('signedout');

            tenantSettings.getCurrencySymbol().then(function(result2) {                
            }, function (err) {
                curr2 = null;
                done2 = true;                
            });        

            $rootScope.$apply();
        });

        waitsFor(function() {
            return done2;
        });    

        runs(function () {
            $rootScope.$broadcast('signedin');        

            tenantSettings.getCurrencySymbol().then(function(result3) { 
                curr3 = result3;
                done3 = true;
            });

            $rootScope.$apply();
        });

        waitsFor(function() {
            return done3;
        }); 

        runs(function() {
            expect(curr1).toEqual('@');
            expect(curr2).toBeFalsy();
            expect(curr3).toEqual('@');
        });
    }));
});
