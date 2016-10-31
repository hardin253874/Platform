// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Form Builder Service', function() {

    var _$state;

    //beforeEach(module(function ($stateProvider) { $stateProvider.state('state-two', { url: '/' }); }));

    /////
    // Load the 'spFormBuilderService' module.
    /////
    beforeEach(module('mod.app.formBuilder.services.spFormBuilderService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedEditFormService'));

    /////
    // Inject the required dependencies.
    /////
    beforeEach(function() {

        /////
        // Mock out the $state provider.
        /////
        _$state = {
            params: {
                eid: 456, //formId
                mode: 'createDefinition',
                definitionId: 123
            },
            current: {
                name: 'formBuilder'
            }
        };

        module(function($provide) {
            $provide.value('$state', _$state);
        });
    });

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    /////
    // Setup the mocked entity service.
    /////
    beforeEach(inject(function(spEntityService, spEditForm) {

        /////
        // Mock a definition.
        /////
        var json = {
            id: 123,
            typeId: 'core:definition',
            name: 'test Definition',
            description: 'My Test Definition.',
            inherits: [
                {
                    id: 'core:userResource',
                    inherits: [
                        {
                            id: 'core:resource',
                            fields: [
                                {
                                    id: 123111,
                                    name: 'resource Field 1'
                                },
                                {
                                    id: 123112,
                                    name: 'resource Field 2'
                                }
                            ]
                        }
                    ],
                    fields: [
                        {
                            id: 12311,
                            name: 'userResource Field 1'
                        },
                        {
                            id: 12312,
                            name: 'userResource Field 2'
                        }
                    ]
                }
            ],
            fields: [
                {
                    id: 1231,
                    name: 'field 1',
                    fieldInGroup: jsonLookup(1233)
                },
                {
                    id: 1232,
                    name: 'field 2',
                    fieldInGroup: jsonLookup(1233)
                }
            ],
            fieldGroups: [
                {
                    id: 1233,
                    name: 'field Group 1'
                }
            ]
        };

        spEntityService.mockGetEntityJSON(json).getEntity(123).then(function(entity) {

        });

        /////
        // Mock a form.
        /////
        json = {
            id: 456,
            typeId: 'console:customEditForm',
            name: 'test Form',
            description: 'My Test Form.',
            typeToEditWithForm: jsonLookup()
        };

        spEntityService.mockGetEntityJSON(json);
        spEditForm.mockGetFormDefinition(spEntity.fromJSON(json));


        /////
        // Mock form instance.
        /////
        json = {
            id: 'console:customEditForm',
            typeId: 'core:type',
            instancesOfType: [
                {
                    name: 'form instance 1'
                },
                {
                    name: 'form instance 2'
                }
            ]
        };

        spEntityService.mockGetEntityJSON(json);

        /////
        // Mock screens.
        /////
        json = {
            id: 'console:screen',
            typeId: 'core:type',
            instancesOfType: [
                {
                    name: 'screen instance 1'
                },
                {
                    name: 'screen instance 2'
                }
            ]
        };

        spEntityService.mockGetEntityJSON(json);

        /////
        // Mock the definition type.
        /////
        json = {
            id: 'core:definition',
            typeId: 'core:type',
            name: 'Definition',
            description: 'Definition type.',
            fields: [
                {
                    id: 111,
                    name: 'dummy Field 1'
                },
                {
                    id: 222,
                    name: 'dummy Field 2'
                }
            ],
            instancesOfType: [
                {
                    name: 'definition instance 1'
                },
                {
                    name: 'definition instance 2'
                }
            ]
        };

        spEntityService.mockGetEntityJSON(json);

        /////
        // Mock the definition type.
        /////
        json = {
            id: 'core:fieldType',
            typeId: 'core:type',
            name: 'Field Type',
            instancesOfType: [
                {
                    id: 880,
                    alias: 'dummyType1',
                    name: 'dummy Type 1',
                    'console:defaultRenderingControls': [
                        {
                            id: 8801,
                            'console:context': {
                                alias: 'console:uiContextHtml'
                            }
                        }
                    ],
                    'console:renderingControl': [
                        {
                            id: 8802,
                            'console:context': {
                                alias: 'console:uiContextHtml'
                            }
                        }
                    ]
                },
                {
                    id: 881,
                    alias: 'dummyType2',
                    name: 'dummy Type 2',
                    'console:renderingControl': [
                        {
                            id: 8803,
                            'console:context': {
                                alias: 'console:uiContextHtml'
                            }
                        }
        ]
                }
            ]
        };

        spEntityService.mockGetEntityJSON(json);
    }));

    it('should expose 5 quadrants', inject(function (spFormBuilderService) {
        var quadrants = spFormBuilderService.quadrants;

        expect(quadrants).not.toBeUndefined();
        expect(quadrants.left).toEqual('left');
        expect(quadrants.top).toEqual('top');
        expect(quadrants.right).toEqual('right');
        expect(quadrants.bottom).toEqual('bottom');
        expect(quadrants.center).toEqual('center');
    }));

    it('should expose 4 containers', inject(function (spFormBuilderService) {
        var containers = spFormBuilderService.containers;

        expect(containers).not.toBeUndefined();
        expect(containers.horizontal).toEqual('horizontalStackContainerControl');
        expect(containers.vertical).toEqual('verticalStackContainerControl');
        expect(containers.form).toEqual('customEditForm');
        expect(containers.header).toEqual('headerColumnContainerControl');
    }));

    it('should expose 2 builders', inject(function (spFormBuilderService) {
        var builders = spFormBuilderService.builders;

        expect(builders).not.toBeUndefined();
        expect(builders.form).toEqual('formBuilder');
        expect(builders.screen).toEqual('screenBuilder');
    }));

    it('should be able to create and destroy an insert indicator', inject(function (spFormBuilderService, $rootScope) {
        var insertIndicator;

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).toBeNull();

        spFormBuilderService.createInsertIndicator($rootScope);

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).not.toBeNull();

        spFormBuilderService.destroyInsertIndicator();

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).toBeNull();
    }));

    it('should be able to hide and show the insert indicator', inject(function (spFormBuilderService, $rootScope) {
        var insertIndicator;
        var jInsertIndicator;

        spFormBuilderService.createInsertIndicator($rootScope);
        spFormBuilderService.positionInsertIndicator(0, 0, 100, 100, $rootScope);

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).not.toBeNull();

        jInsertIndicator = $(insertIndicator);

        expect(jInsertIndicator.css('display')).toEqual('block');

        spFormBuilderService.hideInsertIndicator();

        expect(jInsertIndicator.css('display')).toEqual('none');

        spFormBuilderService.showInsertIndicator();

        expect(jInsertIndicator.css('display')).toEqual('block');

        spFormBuilderService.destroyInsertIndicator();

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).toBeNull();
    }));

    it('should be able to position the insert indicator', inject(function (spFormBuilderService, $rootScope) {
        var insertIndicator;
        var jInsertIndicator;

        spFormBuilderService.createInsertIndicator($rootScope);
        spFormBuilderService.positionInsertIndicator(0, 0, 100, 100, $rootScope);

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).not.toBeNull();

        jInsertIndicator = $(insertIndicator);

        expect(jInsertIndicator.css('top')).toEqual('0px');
        expect(jInsertIndicator.css('left')).toEqual('0px');
        expect(jInsertIndicator.css('height')).toEqual('100px');
        expect(jInsertIndicator.css('width')).toEqual('100px');

        spFormBuilderService.positionInsertIndicator(100, 100, 200, 200, $rootScope);

        expect(jInsertIndicator.css('top')).toEqual('100px');
        expect(jInsertIndicator.css('left')).toEqual('100px');
        expect(jInsertIndicator.css('height')).toEqual('200px');
        expect(jInsertIndicator.css('width')).toEqual('200px');

        spFormBuilderService.destroyInsertIndicator();

        insertIndicator = document.getElementById('insertIndicator');
        expect(insertIndicator).toBeNull();
    }));

    it('should be able to create a temporary definition', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        _$state.params.definitionId = 0;

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.initializeDefinition().then(function(definition) {
            return definition;
        }), result);

        runs(function() {
            expect(result.value).not.toBeUndefined();
            expect(result.value.name).toEqual('New Definition');
            expect(result.value.description).toEqual('A new user-created definition.');
        });
    }));

    it('should be able to get the current builder', inject(function (spFormBuilderService) {
        var builder;

        builder = spFormBuilderService.getBuilder();

        expect(builder).not.toBeUndefined();
        expect(builder).toEqual(spFormBuilderService.builders.form);
    }));

    it('should have a form id by default', inject(function(spFormBuilderService) {
        var formId = spFormBuilderService.getFormId();

        expect(formId).not.toBeUndefined();
        expect(formId).toEqual(456);
    }));

    it('should have a definition id by default', inject(function (spFormBuilderService, $rootScope) {
        var result = {};
        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId(), result);

        runs(function() {
            expect(result.value).not.toBeUndefined();
            expect(result.value).toEqual(123);
        });
    }));

    it('should be able to get the definition type', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        _$state.params.definitionId = 0;

        spFormBuilderService.createTemporaryForm();

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.initializeDefinition().then(function (definition) {
            return definition;
        }), result);

        runs(function () {
            var definitionType = spFormBuilderService.getDefinitionType();

            expect(definitionType).not.toBeUndefined();
            expect(definitionType instanceof spResource.Type).toBeTruthy();
        });
    }));

    it('should be able to get the definition', inject(function(spFormBuilderService, $rootScope) {
        var result = {};
        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
            .then(function(definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function(error) {
            console.log(error);
            throw error;
        }), result);

        runs(function() {
            expect(result.value).not.toBeUndefined();
            expect(result.value.id()).toBe(123);
            expect(result.value.name).toBe('test Definition');
        });
    }));

    it('should be able to get the local definition fields', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        spFormBuilderService.createTemporaryForm();

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function (error) {
                console.log(error);
                throw error;
            }), result);

        runs(function () {
            var fields = spFormBuilderService.getDefinitionFields(true);

            expect(fields).not.toBeUndefined();
            expect(fields.length).toEqual(2);
            expect(fields[0].name).toEqual('field 1');
            expect(fields[1].name).toEqual('field 2');
        });
    }));

    it('should be able to get the inherited definition fields', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        spFormBuilderService.createTemporaryForm();

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function (error) {
                console.log(error);
                throw error;
            }), result);

        runs(function () {
            var fields = spFormBuilderService.getDefinitionFields(false);

            expect(fields).not.toBeUndefined();
            expect(fields.length).toEqual(6);
            expect(fields[0].getEntity().name).toEqual('field 1');
            expect(fields[1].getEntity().name).toEqual('field 2');
            expect(fields[2].getEntity().name).toEqual('resource Field 1');
            expect(fields[3].getEntity().name).toEqual('resource Field 2');
            expect(fields[4].getEntity().name).toEqual('userResource Field 1');
            expect(fields[5].getEntity().name).toEqual('userResource Field 2');
        });
    }));

    it('should be able to get the local definition field groups', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        spFormBuilderService.createTemporaryForm();

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function (error) {
                console.log(error);
                throw error;
            }), result);

        runs(function () {
            var fieldGroups = spFormBuilderService.getDefinitionFieldGroups(true);

            expect(fieldGroups).not.toBeUndefined();
            expect(fieldGroups.length).toEqual(1);
            expect(fieldGroups[0].name).toEqual('field Group 1');
        });
    }));

    it('should be able to get the inherited definition field groups', inject(function (spFormBuilderService, $rootScope) {
        var result = {};

        spFormBuilderService.createTemporaryForm();

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function (error) {
                console.log(error);
                throw error;
            }), result);

        runs(function () {
            var fieldGroups = spFormBuilderService.getDefinitionFieldGroups(false);

            expect(fieldGroups).not.toBeUndefined();
            expect(fieldGroups.length).toEqual(2);
            expect(fieldGroups[0].getEntity().name).toEqual('Default');
            expect(fieldGroups[1].getEntity().name).toEqual('field Group 1');
        });
    }));

    it('should be able to reload the definition', inject(function (spFormBuilderService, $rootScope, $q) {
        var result = {};
        TestSupport.waitCheckReturn($rootScope, $q.when(spFormBuilderService.getFormId())
            .then(function(formId) {
                return spFormBuilderService.getForm(formId);
            })
            .then(function(form) {
                return spFormBuilderService.getDefinitionId();
            })
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            }, function (error) {
                console.log(error);
                throw error;
            })
            .then(function () {
                spFormBuilderService.form.typeToEditWithForm = spFormBuilderService.definition;

                spFormBuilderService.setInitialFormBookmark();
            }), result);

        runs(function () {

            expect(spFormBuilderService.unsavedChanges()).toBeFalsy();

            spFormBuilderService.definition.name = 'blah';

            expect(spFormBuilderService.unsavedChanges()).toBeTruthy();

            TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getDefinitionId()
                .then(function (definitionId) {
                return spFormBuilderService.reloadDefinition(definitionId);
            }, function(error) {
                console.log(error);
                throw error;
            })
            .then(function () {
                spFormBuilderService.setInitialFormBookmark();
            }), result);

            runs(function() {
                expect(spFormBuilderService.unsavedChanges()).toBeFalsy();
            });
        });
    }));

    it('should be able to cache the field render controls', inject(function (spFormBuilderService, $rootScope) {
        TestSupport.wait(
            spFormBuilderService.cacheFieldRenderControls()
            .then(function () {
                expect(spFormBuilderService.fieldRenderControls).not.toBeUndefined();
                expect(spFormBuilderService.fieldRenderControls['core:dummyType1']).not.toBeUndefined();
                expect(spFormBuilderService.fieldRenderControls['core:dummyType2']).not.toBeUndefined();
                expect(spFormBuilderService.fieldRenderControls['core:dummyType1']._id).toEqual(8801);
                expect(spFormBuilderService.fieldRenderControls['core:dummyType2']._id).toEqual(8803);
            }));
    }));

    it('should be able to get field render control alias', inject(function (spFormBuilderService, $rootScope) {
        TestSupport.wait(
            spFormBuilderService.cacheFieldRenderControls()
            .then(function () {
                var alias = spFormBuilderService.getFieldRenderControlAlias('core:dummyType1');
                expect(alias).not.toBeUndefined();
                expect(alias._id).toEqual(8801);
            }));
    }));

    it('should be able to get field render control instance', inject(function (spFormBuilderService, $rootScope) {
        TestSupport.wait(
            spFormBuilderService.cacheFieldRenderControls()
            .then(function () {
                var json = {
                    id: 4444,
                    name: 'blah',
                    isOfType: 'stringField'
                };

                var field = spEntity.fromJSON(json);
                var instance = spFormBuilderService.getFieldRenderControlInstance('core:dummyType1', field);
                expect(instance).not.toBeUndefined();
                expect(instance.type._id).toEqual(8801);
                expect(instance.fieldToRender).toEqual(field);
            }));
    }));

    it('should be able to get the new field instance name', inject(function (spFormBuilderService) {
        var name = spFormBuilderService.getNewFieldInstanceName('blah');

        expect(name).not.toBeUndefined();
        expect(name).toEqual('blah');
    }));

    it('should be able to get the new field instance description', inject(function (spFormBuilderService) {
        var name = spFormBuilderService.getNewFieldInstanceDescription('blah');

        expect(name).not.toBeUndefined();
        expect(name).toEqual('User created \'blah\' field.');
    }));

    it('should be able to create a field', inject(function (spFormBuilderService) {
        var field = spFormBuilderService.createField('stringField', 'blah');

        expect(field).not.toBeUndefined();
        expect(field.name).toEqual('blah');
    }));

    it('should be able to get the fields belonging to a field group', inject(function (spFormBuilderService, $rootScope) {        
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function () {
                var fields = spFormBuilderService.getFieldsBelongingToFieldGroup(1233);

                expect(fields).not.toBeUndefined();
                expect(fields.length).toEqual(2);
                expect(fields[0].getEntity().name).toEqual('field 1');
                expect(fields[1].getEntity().name).toEqual('field 2');
            }));
    }));

    it('should be able to determine direct fields', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function () {
                expect(spFormBuilderService.isDirectField(1231)).toBeTruthy();
                expect(spFormBuilderService.isDirectField(1232)).toBeTruthy();
                expect(spFormBuilderService.isDirectField(12311)).toBeFalsy();
                expect(spFormBuilderService.isDirectField(12312)).toBeFalsy();
            }));
    }));

    it('should be able to determine direct field groups', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function () {
                expect(spFormBuilderService.isDirectFieldGroup(1233)).toBeTruthy();
            }));
    }));

    it('should be able to determine whether a field group has fields', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function () {
                var fg = spEntity.fromJSON({ id: 1233 });
                expect(spFormBuilderService.fieldGroupHasFields(fg)).toBeTruthy();
            }));
    }));

    it('should be able to remove a field group', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function (defn) {
                spFormBuilderService.removeFieldGroup(defn.fieldGroups[0]);
                expect(defn.fieldGroups.length).toEqual(0);
            }));
    }));

    it('should be able to remove a field group from the definition', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function (defn) {
                spFormBuilderService.removeFieldGroupFromDefinition(defn.fieldGroups[0]);
                expect(defn.fieldGroups.length).toEqual(0);
            }));
    }));

    it('should be able to remove a field', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function (defn) {
                expect(_.filter(defn.fields, function (f) { return f.getDataState() !== spEntity.DataStateEnum.Delete; }).length).toEqual(2);
                spFormBuilderService.removeField(defn.fields[0]);
                expect(_.filter(defn.fields, function (f) { return f.getDataState() !== spEntity.DataStateEnum.Delete; }).length).toEqual(1);
            }));
    }));

    it('should be able to remove a field from the definition', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function (defn) {
                expect(_.filter(defn.fields, function (f) { return f.getDataState() !== spEntity.DataStateEnum.Delete; }).length).toEqual(2);
                spFormBuilderService.removeFieldFromDefinition(defn.fields[0]);
                expect(_.filter(defn.fields, function (f) { return f.getDataState() !== spEntity.DataStateEnum.Delete; }).length).toEqual(1);
            }));
    }));

    it('should be able to walk an object graph', inject(function (spFormBuilderService, $rootScope) {
        spFormBuilderService.createTemporaryForm();

        TestSupport.wait(
            spFormBuilderService.getDefinitionId()
            .then(function (definitionId) {
                return spFormBuilderService.getDefinition(definitionId);
            })
            .then(function (defn) {
                var fields = [];

                spFormBuilderService.walkGraph(defn, function (node) {
                    return node.inherits;
                }, function(node) {
                    _.forEach(node.fields, function(field) {
                        fields.push(field);
                    });
                });

                expect(fields).not.toBeUndefined();
                expect(fields.length).toEqual(6);
            }));
    }));

    it('should be able to create a definition', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        expect(definition).not.toBeUndefined();
        expect(definition.id()).toBeGreaterThan(0);
        expect(definition.name).toEqual('blah');
        expect(definition.description).toEqual('blah blah');
    }));

    it('should be able to create a definition with a specific id', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah', 12345);

        expect(definition).not.toBeUndefined();
        expect(definition.id()).toEqual(12345);
        expect(definition.name).toEqual('blah');
        expect(definition.description).toEqual('blah blah');
    }));

    it('should cache the definition after creating it', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        spFormBuilderService.createDefinition('blah', 'blah blah', 12345);

        expect(spFormBuilderService.definition).not.toBeUndefined();
        expect(spFormBuilderService.definition.id()).toEqual(12345);
        expect(spFormBuilderService.definition.name).toEqual('blah');
        expect(spFormBuilderService.definition.description).toEqual('blah blah');
    }));

    it('should be able to create a form', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');
        var form = spFormBuilderService.createForm(definition, 'blah', 'blah blah');

        expect(form).not.toBeUndefined();
        expect(form.id()).toBeGreaterThan(0);
        expect(form.name).toEqual('blah');
        expect(form.description).toEqual('blah blah');
    }));

    it('should be able to create a screen', inject(function (spFormBuilderService) {
        var screen = spFormBuilderService.createScreen('blah', 'blah blah');

        expect(screen).not.toBeUndefined();
        expect(screen.id()).toBeGreaterThan(0);
        expect(screen.name).toEqual('blah');
        expect(screen.description).toEqual('blah blah');
    }));

    it('should be able to create a form with a specific id', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');
        var form = spFormBuilderService.createForm(definition, 'blah', 'blah blah', 12345);

        expect(form).not.toBeUndefined();
        expect(form.id()).toEqual(12345);
        expect(form.name).toEqual('blah');
        expect(form.description).toEqual('blah blah');
    }));

    it('should cache the form after creating it', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');
        spFormBuilderService.createForm(definition, 'blah', 'blah blah', 12345);

        expect(spFormBuilderService.form).not.toBeUndefined();
        expect(spFormBuilderService.form.id()).toEqual(12345);
        expect(spFormBuilderService.form.name).toEqual('blah');
        expect(spFormBuilderService.form.description).toEqual('blah blah');
    }));

    it('should prepare relevant relationships', inject(function (spFormBuilderService) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');
        var form = spFormBuilderService.createForm(definition, 'blah', 'blah blah', 12345);
        
        expect(definition.inherits).toBeArray();
        expect(definition.fieldGroups).toBeArray();
        expect(definition.inSolution).not.toBeArray();

        expect(form.typeToEditWithForm.idP).toBe(definition.idP);
        expect(form.containedControlsOnForm).toBeArray();
        expect(form.inSolution).not.toBeArray();
    }));

    it('should be able to load type information', inject(function (spFormBuilderService, $rootScope) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');
        var result = {};

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.getType(definition).then(function (entity) {
            return entity;
        }, function (error) {
            console.log(error);
            throw error;
        }), result);

        runs(function () {
            expect(result.value).not.toBeUndefined();
            expect(result.value.id()).toBe(0);
            expect(result.value.alias()).toBe('core:definition');
            expect(result.value.name).toBe('Definition');
        });
    }));

    it('should be able to load lookup information', inject(function (spFormBuilderService, $rootScope) {
        var definition = spFormBuilderService.createDefinition('blah', 'blah blah', 123);
        var result = {};

        TestSupport.waitCheckReturn($rootScope, spFormBuilderService.cacheLookupFields(definition).then(function (entity) {
            return entity;
        }, function (error) {
            console.log(error);
            throw error;
        }), result);

        runs(function () {
            expect(result.value).not.toBeUndefined();
            expect(result.value.id()).toBe(123);
            expect(result.value.fields).not.toBeUndefined();
            expect(result.value.fields.length).toEqual(2);
            expect(result.value.fields[0].id()).toEqual(111);
            expect(result.value.fields[0].name).toEqual('dummy Field 1');
        });
    }));

    it('should support simultaneous lookup requests', inject(function (spFormBuilderService, $rootScope, $q) {
        var requests = [];

        var definition1 = spFormBuilderService.createDefinition('blah', 'blah blah', 1234);
        var definition2 = spFormBuilderService.createDefinition('blah', 'blah blah', 5678);

        var result = {};

        requests.push(spFormBuilderService.cacheLookupFields(definition1));
        requests.push(spFormBuilderService.cacheLookupFields(definition2));

        TestSupport.waitCheckReturn($rootScope, $q.all(requests).then(function (results) {
            return results;
        }, function (error) {
            console.log(error);
            throw error;
        }), result);

        runs(function () {
            expect(result.value).not.toBeUndefined();
            expect(result.value.length).toBe(2);
            expect(result.value[0].id()).toEqual(1234);
            expect(result.value[1].id()).toEqual(5678);
            expect(result.value[0].fields).not.toBeUndefined();
            expect(result.value[0].fields.length).toEqual(2);
            expect(result.value[0].fields[0].id()).toEqual(111);
            expect(result.value[0].fields[0].name).toEqual('dummy Field 1');
        });
    }));

    it('should queue lookup requests', inject(function (spFormBuilderService, $rootScope, $q) {
        var requests = [];

        var definition1 = spFormBuilderService.createDefinition('blah', 'blah blah', 1234);

        var result = {};

        requests.push(spFormBuilderService.cacheLookupFields(definition1));
        requests.push(spFormBuilderService.cacheLookupFields(definition1));

        TestSupport.waitCheckReturn($rootScope, $q.all(requests).then(function (results) {
            return results;
        }, function (error) {
            console.log(error);
            throw error;
        }), result);

        runs(function () {
            expect(result.value).not.toBeUndefined();
            expect(result.value.length).toBe(2);
            expect(result.value[0].id()).toEqual(1234);
            expect(result.value[1].id()).toEqual(1234);
            expect(result.value[0].fields).not.toBeUndefined();
            expect(result.value[0].fields.length).toEqual(2);
            expect(result.value[0].fields[0].id()).toEqual(111);
            expect(result.value[0].fields[0].name).toEqual('dummy Field 1');
        });
    }));

    it('should be able to reset', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        expect(spFormBuilderService.definition).not.toBeUndefined();
        expect(spFormBuilderService.definition.id()).toBeGreaterThan(0);
        expect(spFormBuilderService.definition.name).toEqual('blah');
        expect(spFormBuilderService.definition.description).toEqual('blah blah');

        spFormBuilderService.reset();

        expect(spFormBuilderService.definition).toBeUndefined();
        expect(spFormBuilderService.definitionRevision).toEqual(0);
    }));

    it('should be able to create relationships', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        var rel = spFormBuilderService.createRelationship('core:resource', undefined, 'core:oneToMany', undefined, undefined);

        expect(rel).not.toBeUndefined();
        expect(rel.name).toBeNull();
        expect(rel._typeIds[0].nsAlias).toEqual('core:relationship');
    }));

    it('should be able to create choice fields', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        var rel = spFormBuilderService.createRelationship('core:enumType', undefined, 'core:manyToOne', undefined, undefined);

        expect(rel).not.toBeUndefined();
        expect(rel.name).toBeNull();
        expect(rel._typeIds[0].nsAlias).toEqual('core:relationship');
    }));

    it('should be able to create lookups', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        var rel = spFormBuilderService.createRelationship('core:resource', undefined, 'core:manyToOne', undefined, undefined);

        expect(rel).not.toBeUndefined();
        expect(rel.name).toBeNull();
        expect(rel._typeIds[0].nsAlias).toEqual('core:relationship');
    }));

    it('should be able to create images', inject(function (spFormBuilderService) {

        spFormBuilderService.createTemporaryForm();

        var definition = spFormBuilderService.createDefinition('blah', 'blah blah');

        var rel = spFormBuilderService.createRelationship('core:photoFileType', 'test', 'core:manyToOne', undefined, undefined);

        expect(rel).not.toBeUndefined();
        expect(rel.name).toEqual('test');
        expect(rel._typeIds[0].nsAlias).toEqual('core:relationship');
    }));
});