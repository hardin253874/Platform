// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Actions|spec:', function () {
    'use strict';

    var response = {
        "actions": [
            {
                "eid": 3298,
                "name": "View 'Legal'",
                "Class": "EDC.ReadiNow.Silverlight.Windows.Controls.Actions.NavigationAction, EDC.ReadiNow.Silverlight.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36d66e51a95be77c",
                "ControlId": 3271,
                "state": "Edit",
                "icon": "/EDC.ReadiNow.Silverlight.Console;component/Assets/Images/16x16/View.png",
                "isenabled": true,
                "url": null,
                "method": "navigate",
                "data": {}
            },
            {
                "eid": 3339,
                "name": "Delete 'Legal'",
                "Class": "EDC.ReadiNow.Silverlight.Windows.Controls.Actions.DeleteAction, EDC.ReadiNow.Silverlight.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36d66e51a95be77c",
                "ControlId": 3601,
                "state": null,
                "icon": "/EDC.ReadiNow.Silverlight.Console;component/Assets/Images/16x16/Delete.png",
                "isenabled": true,
                "url": null,
                "method": "delete",
                "data": {}
            }
        ]
    };

    var q;
    var mockNavService, mockEntityService, mockDialogService, mockDeleteService, mockWorkflowRunService, mockUserTaskService, mockPromiseService;
    var navWasCalled = false, deleteWasCalled = false;
    beforeEach(module('ng'));
    beforeEach(module('mod.common.ui.spActionsService'));
    beforeEach(function () {
        mockNavService = {
            reset: function () { },
            initialise: function () { },
            navigateToChildState: jasmine.createSpy('navigateToChildState').andCallFake(function () {
                navWasCalled = true;
            })
        };

        module(function ($provide) {
            $provide.value('spNavService', mockNavService);
        });

        mockEntityService = {
            deleteEntities: jasmine.createSpy('deleteEntities').andCallFake(function() {
                deleteWasCalled = true;
                return q.defer().promise;
            })
        };

        module(function($provide) {
            $provide.value('spEntityService', mockEntityService);
        });

        mockDialogService = {
            showMessageBox: function () { return q.when().then(function() { return true; }); }
        };

        mockDeleteService = {
            showMessageBox: function () { return q.when().then(function () { return true; }); },
            showDialog: function () { return q.when().then(function() { return true; });}
        };
        
        module(function ($provide) {
            $provide.value('spDialogService', mockDialogService);
        });

        module(function ($provide) {
            $provide.value('spDeleteService', mockDeleteService);
        });

        mockUserTaskService = {
            waitToNavigateToFollowOnTasks: function () { }
        };

        module(function($provide) {
            $provide.value('spUserTask', mockUserTaskService);
        });

        mockWorkflowRunService = {
        };
        
        module(function ($provide) {
            $provide.value('spWorkflowRunService', mockWorkflowRunService);
        });

        mockPromiseService = {
        };

        module(function ($provide) {
            $provide.value('spPromiseService', mockPromiseService);
        });
    });
    
    beforeEach(inject(function ($httpBackend, $q) {
        this.$httpBackend = $httpBackend;
        q = $q;
    }));

    afterEach(function () {
        this.$httpBackend.verifyNoOutstandingRequest();
        this.$httpBackend.verifyNoOutstandingExpectation();
    });
    
    function getContext() {
        return {
            isEditMode: false,
            scope: {}
        };
    }
    
    describe('spActionsService', function () {

        it('should load', inject(function (spActionsService) {
            expect(spActionsService).toBeTruthy();
        }));

        it('has getConsoleActions', inject(function (spActionsService) {
            expect(spActionsService.getConsoleActions).toBeTruthy();
        }));

        it('has executeItem', inject(function (spActionsService) {
            expect(spActionsService.executeItem).toBeTruthy();
        }));

        it('has setActions', inject(function(spActionsService) {
            expect(spActionsService.setActions).toBeTruthy();
        }));

    });
    
    describe('spActionsService.getConsoleActions', function () {

        it('should return undefined for an invalid request', inject(function (spActionsService) {
            var promise = spActionsService.getConsoleActions();
            expect(promise).toBeUndefined();
        }));

        it('should post request to server', inject(function($rootScope, spActionsService) {
            this.$httpBackend.expectPOST('/spapi/data/v1/actions').respond(200, { show: true, actions: [] });
            
            var request = {
                ids: [5011, 5021, 5031],
                lastId: 4099
            };

            var promise = spActionsService.getConsoleActions(request);

            var r = {};
            TestSupport.waitCheckReturn($rootScope, promise, r, this.$httpBackend.flush());

            expect(r).toBeDefined();
            expect(r.value).toBeDefined();
            expect(r.value.show).toBe(true);
            expect(r.value.actions.length).toBe(0);
        }));

    });

    describe('spActionsService.executeItem', function () {

        it('handles unrecognized action', inject(function (spActionsService) {
            spActionsService.setActions(response.actions);
            spActionsService.executeItem(1, getContext);
        }));

        it('handles view action', inject(function ($rootScope, spActionsService) {
            spActionsService.setActions(response.actions);
            spActionsService.executeItem(0, getContext);

            $rootScope.$apply();

            expect(mockNavService.navigateToChildState).toHaveBeenCalled();
            expect(navWasCalled).toBeTruthy();
        }));

        it('handles delete action', inject(function ($rootScope, spActionsService) {
            spActionsService.setActions(response.actions);
            spActionsService.executeItem(1, getContext);

            $rootScope.$apply();

            expect(mockEntityService.deleteEntities).toHaveBeenCalled();
            expect(deleteWasCalled).toBeTruthy();
        }));
        
    });

});