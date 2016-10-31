// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|spTypeOperatorService|spec:', function () {
    'use strict';

    beforeEach(module('mod.common.ui.spTypeOperatorService'));
    
    it('ensure sounds like operator is removed', inject(function (spTypeOperatorService) {
        expect(_.map(spTypeOperatorService.getApplicableOperators('String'),'oper')).not.toContain('Soundex');
        expect(_.map(spTypeOperatorService.getApplicableOperators('UserString'),'oper')).not.toContain('Soundex');
        expect(_.map(spTypeOperatorService.getApplicableOperators('UserInlineRelationship'),'oper')).not.toContain('Soundex');
        expect(_.map(spTypeOperatorService.getApplicableOperators('InlineRelationship'),'oper')).not.toContain('Soundex');
        expect(_.map(spTypeOperatorService.getApplicableOperators('RelatedResource'),'oper')).not.toContain('Soundex');        
    }));
});