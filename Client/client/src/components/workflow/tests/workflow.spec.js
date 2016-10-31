// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, spEntity, spWorkflow */

describe('Console|Workflow|spec:', function () {
    'use strict';

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    describe('the spWorkflow js module', function () {

        it('getExtendedProperties and setExtendedProperties functions', inject(function () {
            var entity = spEntity.fromId(12345);

            var extProps = _.partial(spWorkflow.getExtendedProperties, entity);

            expect(extProps()).toBeTruthy();
            expect(_.isEmpty(extProps())).toBeTruthy();

            spWorkflow.setExtendedProperties(entity, { x: 3, y: 99 });

            expect(extProps()).toBeTruthy();
            expect(extProps().y).toBe(99);
        }));

        it('mergeExtendedProperties function', inject(function () {
            var entity = spEntity.fromId(12345);

            var extProps = _.partial(spWorkflow.getExtendedProperties, entity);

            spWorkflow.setExtendedProperties(entity, { x: 3, y: 99, o: {} });

            expect(extProps()).toBeTruthy();
            expect(extProps().y).toBe(99);
            expect(extProps().o.id).toBeUndefined();

            spWorkflow.mergeExtendedProperties(entity, { o: { id: 666 } });

            expect(extProps().o.id).toBe(666);
        }));
    });
});
