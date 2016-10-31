// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals xdescribe */

xdescribe('Directive: OpenId', function () {
    var elm;
    var scope;
    var directiveScope;
    var httpBackend;

    /////
    // Load the 'mod.app.login.directive' module.
    /////
    beforeEach(module('mod.app.login.directive'));

    /////
    // Populate the template cache.
    /////
    beforeEach(module('login/directives/openId.tpl.html'));

    beforeEach(inject(function ($rootScope, $compile, $injector) {
        /////
        // Mock the HTTP service
        /////
        httpBackend = $injector.get('$httpBackend');
        elm = angular.element('<open-id></open-id>');
        scope = $rootScope.$new();

        /////
        // Compile the element
        /////
        $compile(elm)(scope);
        scope.$digest();
    }));

    it('should be able to perform a ReadiNow login', function () {
        performLogin(function () {
            /////
            // POST the ReadiNow signin.
            /////
            httpBackend.expectPOST('/spapi/data/v1/login/spsignin', {
                username: directiveScope.model.username,
                password: directiveScope.model.password,
                tenant: directiveScope.model.tenant
            }).respond({});
        }, function () {
            directiveScope.model.username = 'Administrator';
            directiveScope.model.password = 'Password';
            directiveScope.model.tenant = 'EDC';
        }, function () {
            directiveScope.readiNowLoginClick();
        });
    });    

    it('should be able to perform a custom OpenId login', function () {
       
        performLogin(function () {
            httpBackend.expectGET('/spapi/data/v1/login/signin?openid_identifier=http://CustomProvider.com&provider=OpenID').respond({

            });
        }, function () {
            /////
            // Set OpenId as the selected provider.
            /////
            directiveScope.model.provider = directiveScope.model.allProviders[5];

            /////
            // Change the OpenId url.
            /////
            expect(directiveScope.model.openIdProvider).toEqual('http://OpenIdProvider.com');
            directiveScope.model.openIdProvider = 'http://CustomProvider.com';
            expect(directiveScope.model.openIdProvider).toEqual('http://CustomProvider.com');
        }, function () {
            /////
            // Perform the OpenId login.
            /////
            directiveScope.openIdLoginClick();
        });
    });

    it('should be able to perform a Google login', function() {

        performLogin(function() {
            httpBackend.expectGET('/spapi/data/v1/login/signin?openid_identifier=https://www.google.com/accounts/o8/id&provider=Google').respond({
                
            });
        }, null, function() {
            /////
            // Set Google as the selected provider.
            /////
            directiveScope.providerClick(directiveScope.model.allProviders[1]);
        });
    });    

    it('should be able to single-click signin with Google', function () {
        var signedIn = false;
        var divs;
        var i;

        expect(elm).toBeTruthy();

        /////
        // Get the directives isolated scope
        /////
        directiveScope = scope.$$childHead;

        /////
        // Wait for the 'signedin' broadcast.
        /////
        scope.$watch('signedin', function () {
            signedIn = true;
        });

        httpBackend.expectGET('/spapi/data/v1/login/signin?openid_identifier=https://www.google.com/accounts/o8/id&provider=Google').respond({

        });

        /////
        // Locate all the anchors.
        /////
        divs = elm.find('div');

        expect(divs).toBeTruthy();

        /////
        // Locate and click the 'Google' button on the login screen.
        /////
        for (i = 0; i < divs.length; i++) {
            if (divs[i].innerText.replace(/^\s\s*/, '').replace(/\s\s*$/, '') === 'Google') {
                $(divs[i]).trigger('click');
                break;
            }
        }

        httpBackend.flush();

        /////
        // Ensure that the watch was hit.
        /////
        expect(signedIn).toEqual(true);
    });

    function performLogin(httpMock, preScope, postScope) {
        var signedIn = false;

        expect(elm).toBeTruthy();
        expect(scope).toBeTruthy();

        /////
        // Get the directives isolated scope
        /////
        directiveScope = scope.$$childHead;

        expect(directiveScope).toBeTruthy();

        /////
        // Wait for the 'signedin' broadcast.
        /////
        scope.$watch('signedin', function () {
            signedIn = true;
        });

        if (preScope) {
            preScope();
        }

        if (httpMock) {
            httpMock();
        }

        if (postScope) {
            postScope();
        }

        httpBackend.flush();

        /////
        // Ensure that the watch was hit.
        /////
        expect(signedIn).toEqual(true);
    }
});