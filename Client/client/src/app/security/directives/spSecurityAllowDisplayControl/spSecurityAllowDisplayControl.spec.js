// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity, spWorkflow, spWorkflowConfiguration, jsonString */

describe('Security|spSecurityAllowDisplayControl|spec:', function() {
    'use strict';

    var entityChanges = [];

    // Setup
    beforeEach(module('app.security.directives.spSecurityAllowDisplayControl'));

    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));

    beforeEach(module('mockedEntityService'));
    beforeEach(module('sp.app.navigation'));

    function browserTrigger(element, eventType) {
        if (document.createEvent) {
            var event = document.createEvent('MouseEvents');
            event.initMouseEvent(eventType, true, true, window, 0, 0, 0, 0, 0, false, false,
                false, false, 0, element);
            element.dispatchEvent(event);
        } else {
            element.fireEvent('on' + eventType);
        }
    }

    function createApp1MockData(spNavDataService) {
        var app1TopMenuNode = {},
            app1TopMenu;

        app1TopMenu = spEntity.fromJSON({
            id: 10001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 0,
            name: 'Application 1',
            'console:isTopMenuVisible': true
        });

        app1TopMenuNode.item = spNavDataService.navItemFromEntity(app1TopMenu);
        app1TopMenuNode.item.depth = 1;
        app1TopMenuNode.children = [];

        return app1TopMenuNode;
    }

    function createApp2MockData(spNavDataService) {
        var app2TopMenuNode = {},
            app2TopMenu;

        app2TopMenu = spEntity.fromJSON({
            id: 20001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 1,
            name: 'Application 2',
            'console:isTopMenuVisible': true
        });

        app2TopMenuNode.item = spNavDataService.navItemFromEntity(app2TopMenu);
        app2TopMenuNode.item.depth = 1;
        app2TopMenuNode.children = [];

        return app2TopMenuNode;
    }

    beforeEach(inject(function(spNavService, spNavDataService, $q) {
        var app1Node = createApp1MockData(spNavDataService),
            app2Node = createApp2MockData(spNavDataService);

        spNavService.getNavTree = function() {
            return {
                item: null,
                children: [app1Node, app2Node]
            };
        };

        spNavService.getCurrentApplicationId = function() {
            return 12345;
        };

        spNavService.makelink = function(navItem) {
            return '';
        };

        spNavService.requestInitialNavTree = function() {
            var deferred = $q.defer();
            deferred.resolve();

            return deferred.promise;
        };

        spNavService.refreshTreeBranch = function () {
            var deferred = $q.defer();
            deferred.resolve();

            return deferred.promise;
        };
    }));


    // Set the mocked data
    beforeEach(inject(function(spEntityService, $q) {
        var subjects = [];

        entityChanges = [];

        spEntityService.putEntity = function(entity) {
            var deferred = $q.defer();
            deferred.resolve(entity);

            entityChanges.push(entity);

            return deferred.promise;
        };

        subjects.push(spEntity.fromJSON({
            id: { id: 30001, ns: 'core', alias: 'tstRole1' },
            'name': 'Role1',
            'isOfType': [{ ns: 'core', alias: 'role' }]
        }));

        subjects.push(spEntity.fromJSON({
            id: { id: 30002, ns: 'core', alias: 'tstRole2' },
            'name': 'Role2',
            'isOfType': [{ ns: 'core', alias: 'role' }]
        }));

        subjects.push(spEntity.fromJSON({
            id: { id: 30003, ns: 'core', alias: 'tstAccount' },
            'name': 'Account',
            'isOfType': [{ ns: 'core', alias: 'userAccount' }]
        }));

        spEntityService.mockGetEntitiesOfType('core:subject', subjects);

        var app1TopMenu = spEntity.fromJSON({
            id: 10001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 0,
            name: 'Application 1',
            'core:hideOnDesktop': false,
            'core:hideOnTablet': false,
            'core:hideOnMobile': false,
            'core:allowedDisplayBy': [{ id: 30001 }],
            'console:folderContents': [
                {
                    id: 10002,
                    typeId: 'console:folder',
                    'console:consoleOrder': 2,
                    name: 'App1Folder1',
                    'core:allowedDisplayBy': [{ id: 30001 }]
                },
                {
                    id: 10003,
                    typeId: 'console:folder',
                    'console:consoleOrder': 2,
                    name: 'App1Folder2',
                    'core:allowedDisplayBy': [{ id: 30001 }]
                }
            ]
        });

        // Mock the top menu contents
        spEntityService.mockGetEntity(app1TopMenu);

        var app2TopMenu = spEntity.fromJSON({
            id: 20001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 0,
            name: 'Application 2',
            'core:hideOnDesktop': false,
            'core:hideOnTablet': false,
            'core:hideOnMobile': false,
            'console:folderContents': [
                {
                    id: 20002,
                    typeId: 'console:folder',
                    'console:consoleOrder': 2,
                    name: 'App2Folder1'
                },
                {
                    id: 20003,
                    typeId: 'console:folder',
                    'console:consoleOrder': 2,
                    name: 'App2Folder2'
                }
            ]
        });

        // Mock the top menu contents
        spEntityService.mockGetEntity(app2TopMenu);
    }));

    it('should replace HTML element with appropriate content', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'view'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);

        controlScope = scope.$$childHead;

        expect(controlScope.model.availableSubjects.length).toBe(3);
        expect(controlScope.model.filteredSubjects.length).toBe(2);
        expect(controlScope.model.includeUsers).toBe(false);
    }));

    it('includeUsers shows user accounts', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope,
            includeUsersCheckbox;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'view'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);

        includeUsersCheckbox = element.find('.includeUsersCheckbox');
        expect(includeUsersCheckbox.length).toBe(1);

        // Check the include users check box
        includeUsersCheckbox[0].click();

        scope.$digest();

        controlScope = scope.$$childHead;

        expect(controlScope.model.availableSubjects.length).toBe(3);
        expect(controlScope.model.filteredSubjects.length).toBe(3);
        expect(controlScope.model.includeUsers).toBe(true);
    }));

    it('select role and application populates grid', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope,
            selectControls;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'view'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);

        selectControls = element.find('select.securityOptionsDropdown');
        controlScope = scope.$$childHead;

        // Select application 1
        selectControls[0].value = controlScope.getMenuNodes()[0].$$hashKey;
        browserTrigger(selectControls[0], 'change');

        // Select role 1
        selectControls[1].value = controlScope.model.filteredSubjects[0].$$hashKey;
        browserTrigger(selectControls[1], 'change');

        scope.$digest();

        // Verify select controls have correct selections
        expect(controlScope.model.selectedTopMenuNode.item.id).toBe(10001);
        expect(controlScope.model.selectedSubject).toBe(controlScope.model.filteredSubjects[0]);

        // Verify grid is showing correct values
        expect(controlScope.model.navNodeData).toBe(controlScope.model.navNodeDataCache[10001]);

        expect(controlScope.model.navNodeData[0].item.id).toBe(10002);
        expect(controlScope.model.navNodeData[1].item.id).toBe(10003);
        expect(controlScope.model.navNodeData.length).toBe(2);

        // Verify nodes are selected
        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[0].allowedDisplaySubjects[30001]).toBe('unchanged');
        expect(controlScope.model.navNodeData[0].selected).toBe(true);
        expect(_.values(controlScope.model.navNodeData[1].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[1].allowedDisplaySubjects[30001]).toBe('unchanged');
        expect(controlScope.model.navNodeData[1].selected).toBe(true);

        // Verify topMenu is selected
        expect(_.values(controlScope.model.topMenusState).length).toBe(1);
        expect(_.values(controlScope.model.topMenusState[10001]).length).toBe(1);
        expect(controlScope.model.topMenusState[10001][30001].countSelections).toBe(2);
        expect(controlScope.model.topMenusState[10001][30001].allowDisplay).toBe(true);

        expect(controlScope.getNodePath(controlScope.model.navNodeData[0]), 'Application 1 > App1Folder1');
        expect(controlScope.getNodePath(controlScope.model.navNodeData[1]), 'Application 1 > App1Folder2');

        // Select application 2
        selectControls[0].value = controlScope.getMenuNodes()[1].$$hashKey;
        browserTrigger(selectControls[0], 'change');

        // Select role 2
        selectControls[1].value = controlScope.model.filteredSubjects[1].$$hashKey;
        browserTrigger(selectControls[1], 'change');

        scope.$digest();

        // Verify select controls have correct selections
        expect(controlScope.model.selectedTopMenuNode.item.id).toBe(20001);
        expect(controlScope.model.selectedSubject).toBe(controlScope.model.filteredSubjects[1]);

        // Verify grid is showing correct values
        expect(controlScope.model.navNodeData).toBe(controlScope.model.navNodeDataCache[20001]);

        expect(controlScope.model.navNodeData[0].item.id).toBe(20002);
        expect(controlScope.model.navNodeData[1].item.id).toBe(20003);
        expect(controlScope.model.navNodeData.length).toBe(2);

        // Verify nodes are not selected
        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(0);
        expect(controlScope.model.navNodeData[0].selected).toBe(false);
        expect(_.values(controlScope.model.navNodeData[1].allowedDisplaySubjects).length).toBe(0);
        expect(controlScope.model.navNodeData[1].selected).toBe(false);

        // Verify topMenu is not selected
        expect(_.values(controlScope.model.topMenusState).length).toBe(1);
        expect(controlScope.model.topMenusState[20001]).toBeFalsy();
    }));

    it('add item to role', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope,
            selectControls;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'edit'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        controlScope = scope.$$childHead;

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);
        expect(controlScope.canModifySelections()).toBeFalsy();

        selectControls = element.find('select.securityOptionsDropdown');

        // Select application 2
        selectControls[0].value = controlScope.getMenuNodes()[1].$$hashKey;
        browserTrigger(selectControls[0], 'change');

        scope.$digest();

        expect(controlScope.canModifySelections()).toBeFalsy();

        // Select role 2
        selectControls[1].value = controlScope.model.filteredSubjects[1].$$hashKey;
        browserTrigger(selectControls[1], 'change');

        scope.$digest();

        expect(controlScope.canModifySelections()).toBeTruthy();

        // Verify select controls have correct selections
        expect(controlScope.model.selectedTopMenuNode.item.id).toBe(20001);
        expect(controlScope.model.selectedSubject).toBe(controlScope.model.filteredSubjects[1]);

        // Verify grid is showing correct values
        expect(controlScope.model.navNodeData).toBe(controlScope.model.navNodeDataCache[20001]);

        expect(controlScope.model.navNodeData[0].item.id).toBe(20002);
        expect(controlScope.model.navNodeData[1].item.id).toBe(20003);
        expect(controlScope.model.navNodeData.length).toBe(2);

        // Verify nodes are not selected
        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(0);
        expect(controlScope.model.navNodeData[0].selected).toBe(false);
        expect(_.values(controlScope.model.navNodeData[1].allowedDisplaySubjects).length).toBe(0);
        expect(controlScope.model.navNodeData[1].selected).toBe(false);

        // Verify topMenu is not selected
        expect(_.values(controlScope.model.topMenusState).length).toBe(0);
        expect(controlScope.model.topMenusState[20001]).toBeFalsy();

        // Assign the first item to the current role
        controlScope.model.navNodeData[0].selected = true;

        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[0].allowedDisplaySubjects[30002]).toBe('create');

        // Verify topMenu is selected        
        expect(_.values(controlScope.model.topMenusState).length).toBe(1);
        expect(_.values(controlScope.model.topMenusState[20001]).length).toBe(1);
        expect(controlScope.model.topMenusState[20001][30002].countSelections).toBe(1);
        expect(controlScope.model.topMenusState[20001][30002].allowDisplay).toBeFalsy();

        // Trigger a save
        scope.$broadcast('allowDisplayAction', { action: 'save' });

        scope.$digest();

        expect(scope.allowDisplayOptions.mode).toBe('view');

        // There should be one save as only 1 subject was changed
        expect(entityChanges.length).toBe(1);
        // Root entity is the subject
        expect(entityChanges[0].id()).toBe(30002);
        // One entry for the item and one for the top menu
        expect(entityChanges[0].allowDisplay.length).toBe(2);
        var instances = entityChanges[0].allowDisplay.getInstances();
        expect(instances.length).toBe(2);

        // Top Menu
        expect(_.find(instances, function(i) { return i.entity.id() === 20001; }).dataState).toBe(spEntity.DataStateEnum.Create);
        // Items
        expect(_.find(instances, function(i) { return i.entity.id() === 20002; }).dataState).toBe(spEntity.DataStateEnum.Create);
    }));

    it('remove item from role', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope,
            selectControls;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'edit'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);

        selectControls = element.find('select.securityOptionsDropdown');
        controlScope = scope.$$childHead;

        // Select application 1
        selectControls[0].value = controlScope.getMenuNodes()[0].$$hashKey;
        browserTrigger(selectControls[0], 'change');

        // Select role 1
        selectControls[1].value = controlScope.model.filteredSubjects[0].$$hashKey;
        browserTrigger(selectControls[1], 'change');

        scope.$digest();

        // Verify select controls have correct selections
        expect(controlScope.model.selectedTopMenuNode.item.id).toBe(10001);
        expect(controlScope.model.selectedSubject).toBe(controlScope.model.filteredSubjects[0]);

        // Verify grid is showing correct values
        expect(controlScope.model.navNodeData).toBe(controlScope.model.navNodeDataCache[10001]);

        expect(controlScope.model.navNodeData[0].item.id).toBe(10002);
        expect(controlScope.model.navNodeData[1].item.id).toBe(10003);
        expect(controlScope.model.navNodeData.length).toBe(2);

        // Verify nodes are selected
        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[0].allowedDisplaySubjects[30001]).toBe('unchanged');
        expect(controlScope.model.navNodeData[0].selected).toBe(true);
        expect(_.values(controlScope.model.navNodeData[1].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[1].allowedDisplaySubjects[30001]).toBe('unchanged');
        expect(controlScope.model.navNodeData[1].selected).toBe(true);

        // Verify topMenu is selected
        expect(_.values(controlScope.model.topMenusState).length).toBe(1);
        expect(_.values(controlScope.model.topMenusState[10001]).length).toBe(1);
        expect(controlScope.model.topMenusState[10001][30001].countSelections).toBe(2);
        expect(controlScope.model.topMenusState[10001][30001].allowDisplay).toBe(true);

        // Remove all items from the current role
        controlScope.model.navNodeData[0].selected = false;
        controlScope.model.navNodeData[1].selected = false;

        expect(_.values(controlScope.model.navNodeData[0].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[0].allowedDisplaySubjects[30001]).toBe('delete');
        expect(_.values(controlScope.model.navNodeData[1].allowedDisplaySubjects).length).toBe(1);
        expect(controlScope.model.navNodeData[1].allowedDisplaySubjects[30001]).toBe('delete');

        // Verify topMenu is selected        
        expect(_.values(controlScope.model.topMenusState).length).toBe(1);
        expect(_.values(controlScope.model.topMenusState[10001]).length).toBe(1);
        expect(controlScope.model.topMenusState[10001][30001].countSelections).toBe(0);
        expect(controlScope.model.topMenusState[10001][30001].allowDisplay).toBeTruthy();

        // Trigger a save
        scope.$broadcast('allowDisplayAction', { action: 'save' });

        scope.$digest();

        expect(scope.allowDisplayOptions.mode).toBe('view');

        // There should be one save as only 1 subject was changed
        expect(entityChanges.length).toBe(1);
        // Root entity is the subject
        expect(entityChanges[0].id()).toBe(30001);
        // One entry for the item and one for the top menu
        expect(entityChanges[0].allowDisplay.length).toBe(0);

        var instances = entityChanges[0].allowDisplay.getInstances();
        expect(instances.length).toBe(3);

        // Top Menu
        expect(_.find(instances, function(i) { return i.entity.id() === 10001; }).dataState).toBe(spEntity.DataStateEnum.Delete);
        // Items
        expect(_.find(instances, function(i) { return i.entity.id() === 10002; }).dataState).toBe(spEntity.DataStateEnum.Delete);
        expect(_.find(instances, function(i) { return i.entity.id() === 10003; }).dataState).toBe(spEntity.DataStateEnum.Delete);
    }));

    it('should change mode', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'view'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        scope.$broadcast('allowDisplayAction', { action: 'edit' });

        scope.$digest();

        expect(scope.allowDisplayOptions.mode).toBe('edit');

        scope.$broadcast('allowDisplayAction', { action: 'cancel' });

        scope.$digest();

        expect(scope.allowDisplayOptions.mode).toBe('view');
    }));

    it('select unselect all', inject(function($rootScope, $compile) {
        var scope = $rootScope,
            element,
            controlScope,
            selectControls;

        // Setup the control options        
        scope.allowDisplayOptions =
        {
            mode: 'edit'
        };

        element = angular.element('<sp-security-allow-display-control options="allowDisplayOptions"></sp-security-allow-display-control>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spSecurityAllowDisplay-view')).toBe(true);

        selectControls = element.find('select.securityOptionsDropdown');
        controlScope = scope.$$childHead;

        // Select application 2
        selectControls[0].value = controlScope.getMenuNodes()[1].$$hashKey;
        browserTrigger(selectControls[0], 'change');

        // Select role 2
        selectControls[1].value = controlScope.model.filteredSubjects[1].$$hashKey;
        browserTrigger(selectControls[1], 'change');

        scope.$digest();

        // select all
        controlScope.onSelectAll(controlScope.model.navNodeData[0], true);
        controlScope.onSelectAll(controlScope.model.navNodeData[1], true);

        expect(controlScope.model.navNodeData[0].selected).toBe(true);
        expect(controlScope.model.navNodeData[1].selected).toBe(true);

        // deselect all
        controlScope.onSelectAll(controlScope.model.navNodeData[0], false);
        controlScope.onSelectAll(controlScope.model.navNodeData[1], false);

        expect(controlScope.model.navNodeData[0].selected).toBe(false);
        expect(controlScope.model.navNodeData[1].selected).toBe(false);
    }));
});