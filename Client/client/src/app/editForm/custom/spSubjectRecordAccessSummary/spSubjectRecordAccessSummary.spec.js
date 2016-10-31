// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Custom Form Controls|spec:|spSubjectRecordAccessSummary', function () {
    var element;
    var scope;

    beforeEach(module('mod.app.editForm.customDirectives.spSubjectRecordAccessSummary'));
    beforeEach(module('editForm/custom/spSubjectRecordAccessSummary/spSubjectRecordAccessSummary.tpl.html'));
    beforeEach(module('test.spBusyIndicator'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    beforeEach(function () {
        element = angular.element('<sp-subject-record-access-summary form-control="formControl" form-data="formData" />');
        inject(function ($rootScope, $compile) {
            scope = $rootScope.$new();

            scope.formControl = null;
            scope.formData = null;

            $compile(element)(scope);
            scope.$digest();
        });
    });

    function setup(scope) {
        scope.formControl = spEntity.fromJSON({
            id: 1111111,
        });
        scope.$digest();
    }

    function sampleResponse()  {
        return [
        {
            subname: 'Everyone',
            typename: 'Resources',
            scope: 'AllInstances',
            perms: 'Read, Write',
            reason: 'Test reason'
        }];
    }

    it('should load as empty', inject(function () {
        expect(element).toBeTruthy();

        var innerScope = scope.$$childTail;

        var model = innerScope.model;
        expect(model).toBeTruthy();
        expect(innerScope.model).toBeTruthy();
    }));

    it('should load summary for subject', inject(function ($httpBackend) {
        var response = sampleResponse();
        $httpBackend.expectGET('/spapi/data/v1/accesscontrol/typeaccessreport/1234').respond(response);

        setup(scope);

        var subject = spEntity.fromJSON({ id: 1234 });
        scope.formData = spEntity.fromJSON(subject);
        scope.$digest();

        var innerScope = scope.$$childTail;
        expect(innerScope.model).toBeTruthy();
        expect(innerScope.loadPromise).toBeTruthy();
        var model = innerScope.model;
        expect(model.busyIndicator.isBusy).toBe(true);

        TestSupport.wait(innerScope.loadPromise.then(function () {
            expect(model.fullData).toBeArray(1);
            expect(model.objectSummaryData).toBeArray(1);
            expect(model.busyIndicator.isBusy).toBe(false);
            expect(model.gridLoaded).toBe(true);
            var record = model.fullData[0];
            expect(record.subname).toBe('Everyone');
            expect(record.typename).toBe('Resources');
            expect(record.perms).toBe('Read, Write');
            expect(record.scope).toBe('All records');
            expect(record.reason).toBe('Test reason');
        }), { customFlush: $httpBackend.flush });
    }));

    it('refresh should reload summary', inject(function ($httpBackend) {
        var response = sampleResponse();
        $httpBackend.expectGET('/spapi/data/v1/accesscontrol/typeaccessreport/1234').respond(response);

        setup(scope);

        var subject = spEntity.fromJSON({ id: 1234 });
        scope.formData = spEntity.fromJSON(subject);
        scope.$digest();

        var innerScope = scope.$$childTail;
        expect(innerScope.model).toBeTruthy();
        expect(innerScope.loadPromise).toBeTruthy();
        var model = innerScope.model;
        expect(model.busyIndicator.isBusy).toBe(true);

        TestSupport.wait(innerScope.loadPromise.then(function () {
            expect(model.fullData).toBeArray(1);
            expect(model.objectSummaryData).toBeArray(1);

            var resp2 = sampleResponse();
            resp2[1] = resp2[0];
            $httpBackend.expectGET('/spapi/data/v1/accesscontrol/typeaccessreport/1234').respond(resp2);
            model.refreshOptions.refreshCallback();
            return innerScope.loadPromise;
        }).then(function () {
            expect(model.fullData).toBeArray(2);
            expect(model.objectSummaryData).toBeArray(2);
        }), { customFlush: $httpBackend.flush });
    }));

    it('search works', inject(function ($timeout, $httpBackend) {
        var response = sampleResponse();
        response[1] = sampleResponse()[0];
        response[2] = sampleResponse()[0];
        response[1].subname = 'Fred';
        response[2].typename = 'Fred';

        $httpBackend.expectGET('/spapi/data/v1/accesscontrol/typeaccessreport/1234').respond(response);

        setup(scope);

        var subject = spEntity.fromJSON({ id: 1234 });
        scope.formData = spEntity.fromJSON(subject);
        scope.$digest();

        var innerScope = scope.$$childTail;
        expect(innerScope.model).toBeTruthy();
        expect(innerScope.loadPromise).toBeTruthy();
        var model = innerScope.model;
        expect(model.busyIndicator.isBusy).toBe(true);

        $httpBackend.flush();
        expect(model.fullData).toBeArray(3);
        expect(model.objectSummaryData).toBeArray(3);
        expect(model.busyIndicator.isBusy).toBe(false);

        model.search = { value: 'rEd' }; // case insensitive
        scope.$digest();
        expect(model.fullData).toBeArray(3);
        expect(model.objectSummaryData).toBeArray(2);

        model.search = { value: null }; // cancel search
        scope.$digest();
        expect(model.fullData).toBeArray(3);
        expect(model.objectSummaryData).toBeArray(3);
    }));
});