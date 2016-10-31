// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntity */

describe('Report Entity Model|spReportEntity|spec:', function () {
    'use strict';

    it('cloneTypesSet contains data', function() {
        expect(spReportEntity.cloneTypesSet['reportColumn']).toBeTruthy();
        expect(spReportEntity.cloneTypesSet['whatever']).toBeFalsy();
    });

    it('cloneRelationship contains data', function() {
        expect(spReportEntity.cloneRelationshipSet['reportColumns']).toBeTruthy();
        expect(spReportEntity.cloneRelationshipSet['whatever']).toBeFalsy();
    });

});