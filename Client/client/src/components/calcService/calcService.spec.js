// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|spCalcEngineService|spec:', function () {

    var testTypeId = 123;

    beforeEach(module('sp.common.spCalcEngineService'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('sp.app.settings'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    beforeEach(inject(function (spEntityService) {

        var manyToOne = spEntity.fromId('core:manyToOne');
        var json = {
            id: testTypeId,
            typeId: 'core:type',
            fields: [
                { name: 'FieldName1' },
                { name: 'FieldName2', fieldScriptName: 'FieldScriptName2' },
            ],
            relationships: [
                { name: 'Rel1', toName: 'ToName1', toScriptName: 'ToScriptName1', cardinality: manyToOne },
                { name: 'Rel2', toName: 'ToName2', cardinality: manyToOne },
                { name: 'Rel3', cardinality: manyToOne }
            ],
            reverseRelationships: [
                { name: 'Rel4', fromName: 'FromName4', fromScriptName: 'FromScriptName4', cardinality: manyToOne },
                { name: 'Rel5', fromName: 'FromName5', cardinality: manyToOne },
                { name: 'Rel6', cardinality: manyToOne }
            ]
        };

        spEntityService.mockGetEntityJSON(json);
    }));


    //
    // Tests start here
    //

    it('exists', inject(function (spCalcEngineService) {
        expect(spCalcEngineService).toBeTruthy();
    }));

    describe('getEntityTypeHints', function () {

        it('returns script names in required order', inject(function (spCalcEngineService, spEntityService) {

            spyOn(spEntityService, 'getEntity').andCallThrough();

            TestSupport.wait(
                spCalcEngineService.getEntityTypeHints(testTypeId)
                .then(function (scriptNames) {
                    // Check result
                    expect(spEntityService.getEntity).toHaveBeenCalled();
                    expect(scriptNames).toBeTruthy();
                    expect(scriptNames).toBeArray();
                    var expected = 'FieldName1,FieldScriptName2,FromName5,FromScriptName4,Rel3,Rel6,ToName2,ToScriptName1';
                    expect(scriptNames.join()).toBe(expected);

                    // Check request
                    var rq = spEntityService.getEntity.mostRecentCall.args[1];
                    expect(rq.indexOf('fieldScriptName') > 0).toBeTruthy();
                    expect(rq.indexOf('fromScriptName') > 0).toBeTruthy();
                    expect(rq.indexOf('toScriptName') > 0).toBeTruthy();
                }));
        }));
    });

});
