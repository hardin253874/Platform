// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Pickers|spEntityCompositePicker|spec:|spEntityCompositePicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spEntityCompositePicker'));
    beforeEach(module('entityPickers/entityCompositePicker/spEntityCompositePicker.tpl.html'));
    beforeEach(module('entityPickers/entityCompositePicker/spEntityCompositePickerNode.tpl.html'));
    beforeEach(module('entityPickers/entityCompositePicker/spEntityCompositePickerModal.tpl.html'));
    beforeEach(module('entityPickers/entityCompositePicker/spEntityCompositePickerMobileNode.tpl.html'));
    
    beforeEach(module('mod.common.spNgUtils'));
    beforeEach(module('mod.common.spMobile'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        var hierarchy, locationObject, locationRecusiveRel, locationHierarchy;
        // some report
        var report = spEntity.fromJSON({
            id: 4001,
            name: 'Some report',
            isOfType: [{ id: 4000, ns: 'core', alias: 'report' }]
        });
        spEntityService.mockGetEntity(report);

        // structureView type
        hierarchy = spEntity.fromJSON({
            id: 1111,
            alias: 'structureView',
            name: 'Hierarchy'
        });
        spEntityService.mockGetEntity(hierarchy);

        // Location object
        locationObject = spEntity.fromJSON({
            id: { id: 1112, ns: 'core', alias: 'location' },
            name: 'Location'
        });
        spEntityService.mockGetEntity(locationObject);

        // recursive relationship of Location object
        locationRecusiveRel = spEntity.fromJSON({
            id: { id: 1113, ns: 'core', alias: 'locationHasLocation' },
            name: 'Location has Location',
            fromType: locationObject,
            toType: locationObject,
            relType: 'relExclusiveCollection'
        });
        spEntityService.mockGetEntity(locationRecusiveRel);
      
        // Location hierarchy
        locationHierarchy = spEntity.fromJSON({
            id: { id: 1114, ns: 'core', alias: 'locationHierarchy' },
            name: 'Location Hierarchy',
            //types: [{ id: 1111, ns: 'core', alias: 'structureView' }],
            //isOfType: [{ id: 1111, ns: 'core', alias: 'structureView' }],
            isOfType: [hierarchy],
            'k:reportUsesDefinition': locationObject,
            'structureHierarchyRelationship': locationRecusiveRel
        });
        spEntityService.mockGetEntity(locationHierarchy);

        // test locations data
        var australia = spEntity.fromJSON({
            id: 11101,
            name: 'Australia',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(australia);

        var nsw = spEntity.fromJSON({
            id: 11102,
            name: 'NSW',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(nsw);

        var sydney = spEntity.fromJSON({
            id: 11103,
            name: 'Sydney',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(sydney);

        var dubbo = spEntity.fromJSON({
            id: 11104,
            name: 'Dubbo',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(dubbo);

        var parramatta = spEntity.fromJSON({
            id: 11105,
            name: 'Parramatta',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(parramatta);

        var baulkhamHills = spEntity.fromJSON({
            id: 11106,
            name: 'Baulkham Hills',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(baulkhamHills);

        var vic = spEntity.fromJSON({
            id: 11107,
            name: 'Victoria',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(vic);

        var melb = spEntity.fromJSON({
            id: 11108,
            name: 'Melbourne',
            isOfType: [locationObject]
        });
        spEntityService.mockGetEntity(melb);

        // set fwd and rev relationships

        // aus
        australia.registerRelationship({ id: 1113, isReverse: false });
        australia.setRelationship({ id: 1113, isReverse: false }, [nsw]);

        // nsw
        nsw.registerRelationship({ id: 1113, isReverse: false });
        nsw.setRelationship({ id: 1113, isReverse: false }, [sydney, dubbo]);
        nsw.registerLookup({ id: 1113, isReverse: true });
        nsw.setLookup({ id: 1113, isReverse: true }, australia);

        // dubbo
        dubbo.registerLookup({ id: 1113, isReverse: true });
        dubbo.setLookup({ id: 1113, isReverse: true }, nsw);

        // sydney
        sydney.registerRelationship({ id: 1113, isReverse: false });
        sydney.setRelationship({ id: 1113, isReverse: false }, [parramatta, baulkhamHills]);
        sydney.registerLookup({ id: 1113, isReverse: true });
        sydney.setLookup({ id: 1113, isReverse: true }, nsw);

        // parramatta
        parramatta.registerLookup({ id: 1113, isReverse: true });
        parramatta.setLookup({ id: 1113, isReverse: true }, sydney);

        // baulkhamHills
        baulkhamHills.registerLookup({ id: 1113, isReverse: true });
        baulkhamHills.setLookup({ id: 1113, isReverse: true }, sydney);

        // vic
        vic.registerRelationship({ id: 1113, isReverse: false });
        vic.setRelationship({ id: 1113, isReverse: false }, [melb]);
        vic.registerLookup({ id: 1113, isReverse: true });
        vic.setLookup({ id: 1113, isReverse: true }, australia);

        // melb
        melb.registerLookup({ id: 1113, isReverse: true });
        melb.setLookup({ id: 1113, isReverse: true }, vic);

        var testTreeData = [australia];

       
        spEntityService.mockGetEntitiesOfType(1112, testTreeData);
    }));

    it('should show a report control if the type is a report', inject(function ($rootScope, $compile) {
        var scope = $rootScope;

        scope.options = {
            reportId: 4001
        };

        var element = angular.element('<sp-entity-Composite-picker options="options"></sp-entity-Composite-picker>');
        $compile(element)(scope);
        scope.$digest();

        var ctrlScope = scope.$$childTail;
        var model = scope.$$childTail.model;

        expect(model).toBeTruthy();
        expect(ctrlScope.isReport).toBe(true);
        expect(ctrlScope.isStructureView).toBe(false);
    }));

    it('should show a hierarchy if the type is a structure view', inject(function ($rootScope, $compile) {
        var scope = $rootScope;

        scope.options = {
            reportId: 1114
        };

        var element = angular.element('<sp-entity-Composite-picker options="options"></sp-entity-Composite-picker>');
        $compile(element)(scope);
        scope.$digest();

        var ctrlScope = scope.$$childTail;
        var model = scope.$$childTail.model;

        expect(model).toBeTruthy();
        expect(ctrlScope.isReport).toBe(false);
        expect(ctrlScope.isStructureView).toBe(true);
    }));

    it('should show desktop version of control', inject(function ($rootScope, $compile) {
        var scope = $rootScope;

        scope.options = {
            reportId: 1114
        };

        var element = angular.element('<sp-entity-Composite-picker options="options"></sp-entity-Composite-picker>');
        $compile(element)(scope);
        scope.$digest();

        var ctrlScope = scope.$$childTail;

        expect(ctrlScope.isStructureView).toBe(true);
        expect(ctrlScope.isMobile).toBe(false);

        // get desktop div
        var desktopDiv = element.find('div[ng-if="isStructureView && !isMobile"]');		// desktop
        expect(desktopDiv).toBeTruthy();
    }));

    it('should show mobile version of control', inject(function ($rootScope, $compile, spMobileContext) {
        spMobileContext.isMobile = true;
        var scope = $rootScope;

        scope.options = {
            reportId: 1114
        };

        var element = angular.element('<sp-entity-Composite-picker options="options"></sp-entity-Composite-picker>');
        $compile(element)(scope);
        scope.$digest();

        // reset mobile context bfore expects. incase something fails
        spMobileContext.isMobile = false;

        var ctrlScope = scope.$$childTail;

        expect(ctrlScope.isStructureView).toBe(true);
        expect(ctrlScope.isMobile).toBe(true);

        // get desktop div
        var desktopDiv = element.find('div[ng-if="isStructureView && !isMobile"]');		// desktop
        expect(desktopDiv.length).toBe(0);

        // get mobile div
        var mobileDiv = element.find('div[ng-if="isStructureView && isMobile"]');		// mobile
        expect(mobileDiv.length).toBe(1);
    }));

    it('should have one root node', inject(function ($rootScope, $compile) {
        var scope = $rootScope;

        scope.options = {
            reportId: 1114
        };

        var element = angular.element('<sp-entity-Composite-picker options="options"></sp-entity-Composite-picker>');
        $compile(element)(scope);
        scope.$digest();

        var model = scope.$$childTail.model;

        expect(model).toBeTruthy();
        expect(model.root.length).toBe(1);
    }));
});