// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, $, sp, spResource, spEntity */

(function () {
    'use strict';

    /**
     * Module implementing a form builder toolbox field viewer control.
     * spFormBuilderToolboxFields provides the toolbox for interacting fields on the canvas.
     *
     * @module spFormBuilderToolboxFields
     * @example

     Using the spFormBuilderToolboxFields:

     &lt;sp-form-builder-toolbox-fields&gt;&lt;/sp-form-builder-toolbox-fields&gt

     */
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxFields', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.configureDialog.Controller',
        'mod.common.alerts',
        'sp.common.spDialog',
        'mod.common.ui.spFocus',
        'mod.app.formBuilder.factories.FieldContainer',
        'mod.common.spCachingCompile'
    ])
        .directive('spFormBuilderToolboxFields', ['$q', '$rootScope', 'spFormBuilderService', 'controlConfigureDialogFactory', 'spAlertsService', 'spDialog', 'focus', 'FieldContainer', 'spCachingCompile', function ($q, $rootScope, spFormBuilderService, controlConfigureDialogFactory, spAlertsService, spDialog, focus, FieldContainer, spCachingCompile) {

            function logFieldGroups(message, fieldGroups) {
                if (fieldGroups) {
                    console.log((message || '') + ' - fieldGroups: ' + _.map(fieldGroups, function (fg) {
                        var e = fg.getEntity() || {};
                        return ('id=' + e.idP + ',name=' + e.name);
                    }));
                }
            }

            /**
             * Directive structure.
             */
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},
                link: function (scope, element) {

                    /**
                     * Ensure a model exists.
                     */
                    scope.model = scope.model || {};
                    scope.model.search = {
                        value: null,
                        id: 'searchFields'
                    };

                    scope.spFormBuilderService = spFormBuilderService;
                    scope.getTargetType = getTargetType;

                    scope.popover = {
                        isOpen: false
                    };

                    scope.model.additionalFieldGroups = [
                        {
                            name: 'Unallocated',
                            id: -1
                        }
                    ];

                    /**
                     * Creates a new backing field and render control instance.
                     */

                    function getRenderControlInstance(state) {
                        var newEntity;
                        var fields;
                        var fieldGroups;
                        var defaultFieldGroup;
                        var defaultFieldGroupId = -1;
                        var onCreatePromise;
                        var onCreateRenderControlPromise = $q.defer();
                        var promises = [];

                        if (!state) {
                            console.error('Invalid state specified.');
                            return undefined;
                        }

                        fieldGroups = spFormBuilderService.getDefinitionFieldGroups(false);
                        logFieldGroups('getRenderControlInstance', fieldGroups);

                        if (fieldGroups) {
                            defaultFieldGroup = _.find(fieldGroups, function (fieldGroup) {
                                if (_.isNull(fieldGroup.getEntity().name)) {
                                    console.error('spFormBuilderToolboxFields.getRenderControlInstance: entity on fieldGroup has null name. (state=' + JSON.stringify(state) + ')');
                                }
                                return fieldGroup.getEntity().alias() === 'core:default';
                            });

                            if (!defaultFieldGroup) {
                                defaultFieldGroup = createNewFieldGroup();
                            }

                            if (defaultFieldGroup) {
                                if (defaultFieldGroup instanceof spResource.FieldGroup) {
                                    defaultFieldGroupId = defaultFieldGroup.getEntity().id();
                                } else if (defaultFieldGroup instanceof spEntity._Entity) {
                                    defaultFieldGroupId = defaultFieldGroup.id();
                                }
                            }
                        }

                        if (state.isRelationship) {
                            newEntity = spFormBuilderService.createRelationship(state.type, state.assignName ? state.name : undefined, state.cardinality, state.additionalData, defaultFieldGroupId);

                            if (newEntity) {

                                if (state.onCreateCallback) {
                                    onCreatePromise = state.onCreateCallback(newEntity, spFormBuilderService.createSource.form);
                                } else {
                                    onCreatePromise = $q.when(newEntity);
                                }

                                promises.push(onCreatePromise);
                                promises.push(onCreateRenderControlPromise.promise);

                                onCreatePromise.then(function (relationship) {

                                    if (relationship) {

                                        var renderControl = spFormBuilderService.getRelationshipRenderControlInstance(state.type, relationship, state.omitControlName);

                                        if (renderControl) {

                                            var args = {
                                                renderControl: renderControl,
                                                relationship: relationship
                                            };

                                            var promise;

                                            if (state.onCreateRenderControlCallback) {
                                                promise = state.onCreateRenderControlCallback(args, spFormBuilderService.createSource.form);
                                            } else {
                                                promise = $q.when(args);
                                            }

                                            promise.then(function (data) {

                                                spFormBuilderService.addRelationshipToDefinition(data.relationship);

                                                onCreateRenderControlPromise.resolve(data.renderControl);
                                            }, function (error) {
                                                onCreateRenderControlPromise.reject(error);
                                            });
                                        }
                                    }

                                    return relationship;
                                });

                                return $q.all(promises).then(function (results) {
                                    return results[1];
                                });
                            }
                        } else {
                            newEntity = spFormBuilderService.createField(state.type, state.name, state.additionalData, defaultFieldGroupId);

                            if (newEntity) {

                                if (state.onCreateCallback) {
                                    onCreatePromise = state.onCreateCallback(newEntity, spFormBuilderService.createSource.form);
                                } else {
                                    onCreatePromise = $q.when(newEntity);
                                }

                                return onCreatePromise
                                    .then(function (field) {
                                        if (field) {

                                            fields = spFormBuilderService.getDefinitionFields(true);

                                            if (fields) {
                                                fields.add(newEntity);
                                            }
                                        }

                                        return field;
                                    })
                                    .then(function (field) {
                                        // note: field is an entity, but unmanagedControlLabel is a plain json string tagging along
                                        return spFormBuilderService.getFieldRenderControlInstance(field.type.nsAlias, field, field.unmanagedControlLabel);
                                    });

                            }
                        }

                        return $q.reject();
                    }


                    function launchConfigureDialog(entity, relType, isFormControl, isInitialCreate, source) {
                        var deferred = $q.defer();
                        //Build option variable
                        var options = {
                            formControl: isFormControl ? entity.renderControl : entity,
                            source: source,  // form or definition (currently only set for calculated fields, because isFormControl will be false in either case for the initial drop)
                            isFieldControl: isFormControl,
                            relationshipType: relType,
                            relationship: isFormControl ? null : entity,
                            isReverseRelationship: (relType === 'lookup' || relType === 'relationship') ? false : undefined,   // when createing a new lookup/relationship, its fwd direction
                            isFormControl: isFormControl,
                            definition: spFormBuilderService.definition,
                            isInitialCreate: isInitialCreate
                        };
                        controlConfigureDialogFactory.createDialog(options).then(function (result) {
                            if (result) {
                                if (isFormControl)
                                    entity.renderControl = result;
                                else
                                    entity = result;
                                deferred.resolve(entity);
                            } else {
                                deferred.reject();
                            }
                        });
                        return deferred.promise;
                    }


                    /**
                     * Callback method for when a new choice field is created.
                     */
                    function onCreateChoiceField(instance, source) {
                        if (source === 'definition')
                            return launchConfigureDialog(instance, 'choice', false, true);
                        else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the field, reject to cancel.
                            /////
                            deferred.resolve(instance);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new choice render control is created.
                     */
                    function onCreateChoiceRenderControl(args, source) {
                        if (source === 'form')
                            return launchConfigureDialog(args, 'choice', true, true);
                        else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the args, reject to cancel.
                            /////
                            deferred.resolve(args);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new image field is created.
                     */
                    function onCreateImageField(instance) {
                        var deferred = $q.defer();

                        /////
                        // Resolve the deferred promise to add the field, reject to cancel.
                        /////
                        deferred.resolve(instance);

                        return deferred.promise;
                    }

                    /**
                     * Callback method for when a new image render control is created.
                     */
                    function onCreateImageRenderControl(args) {
                        var deferred = $q.defer();

                        /////
                        // Resolve the deferred promise to add the args, reject to cancel.
                        /////
                        deferred.resolve(args);

                        return deferred.promise;
                    }

                    /**
                     * Callback method for when a new lookup field is created.
                     */
                    function onCreateLookupField(instance, source) {
                        if (source === 'definition') {
                            runCustomActionsOnCreateRelationship(instance);
                            return launchConfigureDialog(instance, 'lookup', false, true);
                        } else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the field, reject to cancel.
                            /////
                            deferred.resolve(instance);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new lookup render control is created.
                     */
                    function onCreateLookupRenderControl(args, source) {
                        if (source === 'form') {
                            runCustomActionsOnCreateRelationshipRenderControl(args.renderControl);
                            runCustomActionsOnCreateRelationship(args.renderControl.relationshipToRender);
                            return launchConfigureDialog(args, 'lookup', true, true);
                        } else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the field, reject to cancel.
                            /////
                            deferred.resolve(args);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new relationship field is created.
                     */
                    function onCreateRelationshipField(instance, source) {
                        if (source === 'definition') {
                            runCustomActionsOnCreateRelationship(instance);
                            return launchConfigureDialog(instance, 'relationship', false, true);
                        } else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the field, reject to cancel.
                            /////
                            deferred.resolve(instance);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new relationship render control is created.
                     */
                    function onCreateRelationshipRenderControl(args, source) {
                        if (source === 'form') {
                            runCustomActionsOnCreateRelationshipRenderControl(args.renderControl);
                            runCustomActionsOnCreateRelationship(args.renderControl.relationshipToRender);
                            return launchConfigureDialog(args, 'relationship', true);
                        } else {
                            var deferred = $q.defer();

                            /////
                            // Resolve the deferred promise to add the field, reject to cancel.
                            /////
                            deferred.resolve(args);

                            return deferred.promise;
                        }
                    }

                    /**
                     * Callback method for when a new calculated field is created.
                     */
                    function onCreateCalculatedField(instance, source) {
                        return launchConfigureDialog(instance, 'field', false, true, source);
                    }

                    /**
                     * Method to perform custom actions on creating a new lookup/relationship. to be done before the relationship properties dialog is launched.
                     */
                    function runCustomActionsOnCreateRelationship(relationship) {
                        // overwrite the name with empty string
                        relationship.name = '';

                        // set the fromType name to the name of definition being created
                        relationship.fromType.name = spFormBuilderService.getDefinitionType().getName();

                        // if toType is set to 'resource' then set it to null
                        if (relationship.toType.idP === 0 && relationship.toType.alias() === 'core:resource')
                            relationship.toType = null;
                    }

                    /**
                     * Method to perform custom actions on creating a new lookup/relationship render control. to be done before the relationship properties dialog is launched.
                     */
                    function runCustomActionsOnCreateRelationshipRenderControl(renderControl) {
                        // overwrite the name with empty string
                        renderControl.name = '';
                    }

                    /**
                     * Popover groups.
                     */
                    scope.model.popoverGroups = [
                        {
                            name: 'Text Fields',
                            fields: [
                                {
                                    name: 'Text',
                                    icon: 'assets/images/itemicon/StringField.png',
                                    type: 'core:stringField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Multiline Text',
                                    icon: 'assets/images/itemicon/MultilineString.png',
                                    type: 'core:stringField',
                                    additionalData: {
                                        allowMultiLines: true
                                    },
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                }
                            ],
                            isOpen: true
                        },
                        {
                            name: 'Numeric Fields',
                            fields: [
                                {
                                    name: 'Number',
                                    icon: 'assets/images/itemicon/IntField.png',
                                    type: 'core:intField',                                    
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'AutoNumber',
                                    icon: 'assets/images/itemicon/AutoNumberField.png',
                                    type: 'core:autoNumberField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Decimal',
                                    icon: 'assets/images/itemicon/DecimalField.png',
                                    type: 'core:decimalField',                                    
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Currency',
                                    icon: 'assets/images/itemicon/CurrencyField.png',
                                    type: 'core:currencyField',
                                    additionalData: {
                                        decimalPlaces: 2
                                    },
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                }
                            ],
                            isOpen: false
                        },
                        {
                            name: 'Date & Time Fields',
                            fields: [
                                {
                                    name: 'Date and Time',
                                    icon: 'assets/images/itemicon/DateTimeField.png',
                                    type: 'core:dateTimeField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Date',
                                    icon: 'assets/images/itemicon/DateField.png',
                                    type: 'core:dateField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Time',
                                    icon: 'assets/images/itemicon/TimeField.png',
                                    type: 'core:timeField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                }
                            ],
                            isOpen: false
                        },
                        {
                            name: 'Other',
                            fields: [
                                {
                                    name: 'Yes/No',
                                    icon: 'assets/images/itemicon/BooleanField.png',
                                    type: 'core:boolField',
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    }
                                },
                                {
                                    name: 'Image',
                                    icon: 'assets/images/itemicon/Image.png',
                                    type: 'core:photoFileType',
                                    source: 'new',
                                    isRelationship: true,
                                    isImage: true,
                                    assignName: true,
                                    omitControlName: true,
                                    cardinality: 'core:manyToOne',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    },
                                    onCreateRenderControlCallback: function (args, source) {
                                        return onCreateImageRenderControl(args, source);
                                    },
                                    onCreateCallback: function (instance, source) {
                                        return onCreateImageField(instance, source);
                                    }
                                },
                                {
                                    name: 'Choice',
                                    icon: 'assets/images/16x16/ChoiceField.png',
                                    type: 'core:enumType',
                                    source: 'new',
                                    isRelationship: true,
                                    isChoice: true,
                                    assignName: false,
                                    cardinality: 'core:manyToOne',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    },
                                    onCreateRenderControlCallback: function (args, source) {
                                        return onCreateChoiceRenderControl(args, source);
                                    },
                                    onCreateCallback: function (instance, source) {
                                        return onCreateChoiceField(instance, source);
                                    },
                                    additionalData: {
                                        toType: {
                                            name: 'New Choice Field',
                                            typeId: 'core:enumType',
                                            inherits: [
                                                {
                                                    id: 'core:enumValue'
                                                }
                                            ]
                                        }
                                    }
                                },
                                {
                                    name: 'Lookup',
                                    icon: 'assets/images/itemicon/LookupField.png',
                                    type: 'core:resource',
                                    source: 'new',
                                    isRelationship: true,
                                    isLookup: true,
                                    assignName: false,
                                    cardinality: 'core:manyToOne',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    },
                                    onCreateRenderControlCallback: function (args, source) {
                                        return onCreateLookupRenderControl(args, source);
                                    },
                                    onCreateCallback: function (instance, source) {
                                        return onCreateLookupField(instance, source);
                                    }
                                },
                                {
                                    name: 'Relationship',
                                    icon: 'assets/images/itemicon/relationship.png',
                                    type: 'core:resource',
                                    source: 'new',
                                    isRelationship: true,
                                    assignName: false,
                                    cardinality: 'core:oneToMany',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    },
                                    onCreateRenderControlCallback: function (args, source) {
                                        return onCreateRelationshipRenderControl(args, source);
                                    },
                                    onCreateCallback: function (instance, source) {
                                        return onCreateRelationshipField(instance, source);
                                    }
                                },
                                {
                                    name: 'Calculation',
                                    icon: 'assets/images/itemicon/Calculation.png',
                                    type: 'core:stringField',   // too many things check this to risk not having a concrete type. so it gets changed in the property dialog instead
                                    source: 'new',
                                    getRenderControlInstance: function () {
                                        return getRenderControlInstance(this);
                                    },
                                    onCreateCallback: function (instance, source) {
                                        return onCreateCalculatedField(instance, source);
                                    },
                                    additionalData: {
                                        isCalculatedField: true,
                                        isFieldReadOnly: true
                                    }
                                }
                            ],
                            isOpen: false
                        }
                    ];

                    /**
                     * Get the lookups target type.
                     */

                    function getTargetType(lookup) {
                        var targetType;
                        var entity;

                        if (lookup && lookup.getEntity && lookup.isReverse) {
                            entity = lookup.getEntity();

                            if (entity) {
                                if (!lookup.isReverse()) {
                                    targetType = entity.getToType();
                                } else {
                                    targetType = entity.getFromType();
                                }

                                return targetType;
                            }
                        }

                        return null;
                    }

                    /**
                     * Gets the selected type.
                     */

                    function getSelectedType() {
                        var type;
                        var lookup;
                        var targetType;

                        lookup = spFormBuilderService.selectedLookup;

                        if (lookup) {
                            targetType = getTargetType(lookup);
                        } else {
                            targetType = spFormBuilderService.definition;
                        }

                        type = new spResource.Type(targetType);

                        return type;
                    }

                    /**
                     * Refresh the field groups.
                     */

                    function refreshFieldGroups() {
                        var type;
                        var fieldGroups;
                        var localFieldGroups = [];
                        var sortedLocalFieldGroups;
                        var localFieldGroupIds;
                        var inheritedFieldGroups;
                        var sortedInheritedFieldGroups;
                        var defaultIndex;

                        type = getSelectedType();

                        if (type && type.getFieldGroups) {
                            inheritedFieldGroups = _.uniqBy(type.getFieldGroups(), function (fieldGroup) {
                                return fieldGroup.getEntity().id();
                            });

                            /////
                            // Determine the local field groups.
                            /////
                            localFieldGroupIds = _.map(_.uniqBy(spFormBuilderService.getDefinitionFieldGroups(true), function (fieldGroup) {
                                return fieldGroup.id();
                            }), function (fg) {
                                return fg.id();
                            });

                            /////
                            // Split the field groups into local and inherited.
                            /////
                            _.forEach(localFieldGroupIds, function (fg) {
                                var index = _.findIndex(inheritedFieldGroups, function (ifg) {
                                    return ifg.getEntity().id() === fg;
                                });

                                if (index >= 0) {
                                    localFieldGroups.splice(0, 0, inheritedFieldGroups.splice(index, 1)[0]);
                                }
                            });

                            // logFieldGroups('refreshFieldGroups local', localFieldGroups);
                            // logFieldGroups('refreshFieldGroups inherited', inheritedFieldGroups);

                            /////
                            // Sort the local field groups.
                            /////
                            sortedLocalFieldGroups = sp.naturalSort(localFieldGroups, function (e) {
                                if (_.isNull(e.getEntity().name)) {
                                    console.error('spFormBuilderToolboxFields.refreshFieldGroups: entity on localFieldGroups has null name');
                                }
                                return sp.result(e.getEntity(), 'name');
                            });

                            /////
                            // Sort the inherited field groups.
                            /////
                            sortedInheritedFieldGroups = sp.naturalSort(inheritedFieldGroups, function (e) {
                                if (_.isNull(e.getEntity().name)) {
                                    console.error('spFormBuilderToolboxFields.refreshFieldGroups: entity on inheritedFieldGroups has null name');
                                }
                                return sp.result(e.getEntity(), 'name');
                            });

                            fieldGroups = sortedInheritedFieldGroups.concat(sortedLocalFieldGroups);

                            /////
                            // Ensure 'Default' is at the front. Yuck!
                            /////
                            defaultIndex = _.findIndex(fieldGroups, function (fg) {
                                if (_.isNull(fg.getEntity().name)) {
                                    console.error('spFormBuilderToolboxFields.refreshFieldGroups: entity on fieldGroup has null name');
                                }
                                return sp.result(fg.getEntity(), 'name.toLowerCase');
                            });

                            if (defaultIndex > 0) {
                                fieldGroups.splice(0, 0, fieldGroups.splice(defaultIndex, 1)[0]);
                            }

                            scope.model.fieldGroups = fieldGroups;
                        }
                    }

                    /**
                     * Refresh the fields.
                     */

                    function refreshFields() {
                        var type;
                        var fields;
                        var relationships;
                        var fieldContainers = [];
                        var sortedContainers;
                        var defaultFieldGroup;
                        var defaultFieldGroupEntity = null;
                        var defaultFieldGroupId = -1;
                        type = getSelectedType();

                        if (scope.model.fieldGroups) {
                            defaultFieldGroup = _.find(scope.model.fieldGroups, function (fieldGroup) { return fieldGroup && fieldGroup.getName() === 'Default'; });


                            if (defaultFieldGroup && defaultFieldGroup.getEntity()) {
                                defaultFieldGroupEntity = defaultFieldGroup.getEntity();
                                defaultFieldGroupId =  defaultFieldGroup.getEntity().id();
                            }
                        }

                        if (type && type.getFields) {
                            fields = _.uniqBy(type.getFields(), function (field) {
                                return field.getEntity().id();
                            });

                            _.forEach(fields, function (field) {
                                fieldContainers.push(new FieldContainer(field._fieldEntity.idP, field, FieldContainer.containerType.field));
                            });
                        }

                        /////
                        // The 'getRelationships' method filters out hidden relationships.
                        /////
                        if (type && type.getAllRelationships) {
                            relationships = _.uniqBy(type.getAllRelationships(), function (relationship) {
                                /////
                                // Uniqueness needs to include direction.
                                /////
                                return relationship.getEntity().id() + relationship.isReverse().toString();
                            });

                            _.forEach(relationships, function (relationship) {
                                var isGroupChanged = false;
                                var initialDataState;

                                //bug 25668, set the relationsihp field default field type group to 'Default'
                                if (defaultFieldGroupEntity) {                                                                        
                                    if (!relationship.isReverse()) {
                                        if (!relationship.getEntity().relationshipInFromTypeGroup) {                                  
                                            initialDataState = relationship.getEntity().getDataState();          
                                            relationship.getEntity().relationshipInFromTypeGroup = defaultFieldGroupEntity;
                                            isGroupChanged = true;
                                        }
                                    } else {
                                        if (!relationship.getEntity().relationshipInToTypeGroup) {
                                            initialDataState = relationship.getEntity().getDataState();
                                            relationship.getEntity().relationshipInToTypeGroup = defaultFieldGroupEntity;
                                            isGroupChanged = true;
                                        }
                                    }

                                    if (isGroupChanged &&
                                        initialDataState === spEntity.DataStateEnum.Unchanged) {
                                        // Group has changed but initial data state was unchanged.
                                        // Set the data state to unchanged to avoid a save and potentially unexpected security errors
                                        relationship.getEntity().setDataState(spEntity.DataStateEnum.Unchanged);
                                        scope.spFormBuilderService.setInitialFormBookmark();
                                    }
                                }

                                fieldContainers.push(new FieldContainer(relationship._relEntity.idP, relationship, FieldContainer.containerType.relationship));
                            });
                        }

                        sortedContainers = sp.naturalSort(fieldContainers, 'name');

                        scope.model.fields = sortedContainers;

                        scope.model.additionalFieldGroups[0].visible = _.some(scope.model.fields, function (container) {
                            var fieldGroupEntity = container.getFieldGroupEntity();

                            return !fieldGroupEntity || !_.find(scope.model.fieldGroups, function (fg) {
                                return fg.getEntity().id() === fieldGroupEntity.id();
                            });
                        });
                    }

                    /**
                     * Determines whether the current element supports drop.
                     */
                    function allowDropImpl(entities, dragData, dropData, fieldGroups, defaultFieldGroup, getEntityId) {
                        var result;
                        var sourceFieldGroupId = -1;
                        var targetFieldGroupId = -1;
                        var sourceFieldGroup;

                        if (!_.some(entities, function (entity) {
                            return getEntityId(entity) === dragData.getEntity().id();
                        })) {
                            /////
                            // Field being dragged does not belong to this definition.
                            /////
                            result = false;
                        } else {

                            /////
                            // Get the source field group id.
                            /////
                            if (dragData.getFieldGroupEntity) {
                                sourceFieldGroup = dragData.getFieldGroupEntity();

                                if (sourceFieldGroup) {
                                    sourceFieldGroupId = sourceFieldGroup.id();
                                }
                            }

                            if (sourceFieldGroupId !== -1 && (defaultFieldGroup && defaultFieldGroup.id() !== sourceFieldGroupId) && !_.some(fieldGroups, function (fieldGroup) {
                                return fieldGroup.id() === sourceFieldGroupId;
                            })) {
                                /////
                                // The source field group is allocated and does not belong to this definition.
                                /////
                                result = false;
                            } else {

                                /////
                                // Get the target field group id.
                                /////
                                if (dropData.fieldGroup && dropData.fieldGroup.getEntity) {
                                    targetFieldGroupId = dropData.fieldGroup.getEntity().id();
                                }

                                if (sourceFieldGroupId === targetFieldGroupId) {
                                    result = false;
                                } else {

                                    /////
                                    // Target field group is unallocated or belongs to this definition.
                                    /////
                                    result = targetFieldGroupId === -1 || (defaultFieldGroup && defaultFieldGroup.id() === targetFieldGroupId) || _.some(fieldGroups, function (fieldGroup) {
                                        return fieldGroup.id() === targetFieldGroupId;
                                    });
                                }
                            }
                        }

                        return result;
                    }

                    /**
                     * Determines whether the current element supports drop.
                     */

                    function allowDrop(source, target, dragData, dropData) {
                        var fields;
                        var relationships;
                        var defaultFieldGroup = null;
                        var fieldGroups;
                        var inheritedFieldGroups;
                        var result = false;
                        var targetFieldGroupId;

                        if (dragData) {

                            /////
                            // Get the field groups that belong to this definition.
                            /////
                            fieldGroups = spFormBuilderService.getDefinitionFieldGroups(true);

                            if (fieldGroups && fieldGroups.length) {
                                defaultFieldGroup = _.find(fieldGroups, function(fg) {
                                    return fg.alias() === 'core:default';
                                });
                            }

                            if (!defaultFieldGroup) {
                                inheritedFieldGroups = spFormBuilderService.getDefinitionFieldGroups(false);

                                if (inheritedFieldGroups && inheritedFieldGroups.length > 0) {
                                    defaultFieldGroup = _.find(inheritedFieldGroups, function (fg) {
                                        return fg.getEntity().alias() === 'core:default';
                                    });

                                    if (defaultFieldGroup) {
                                        defaultFieldGroup = defaultFieldGroup.getEntity();
                                    }
                                }
                            }

                            if (dragData.source === 'new') {
                                /////
                                // Dragging a new field onto a field group.
                                /////

                                if (dropData.fieldGroup && dropData.fieldGroup.id && dropData.fieldGroup.id === -1) {
                                    /////
                                    // Allow drop on the 'Unallocated' field group.
                                    /////
                                    result = true;
                                } else {
                                    targetFieldGroupId = dropData.fieldGroup.getEntity().id();

                                    result = defaultFieldGroup && defaultFieldGroup.id() === targetFieldGroupId;

                                    /////
                                    // Determine if the target field group belongs to this definition.
                                    /////
                                    result = result || _.some(fieldGroups, function (fieldGroup) {
                                        return fieldGroup.id() === targetFieldGroupId;
                                    });
                                }

                            } else {
                                /////
                                // Dragging fields between field groups.
                                /////

                                if (dragData.isField && dragData.isField()) {

                                    /////
                                    // Get the fields that belong to this definition.
                                    /////
                                    fields = spFormBuilderService.getDefinitionFields(true);

                                    result = allowDropImpl(fields, dragData, dropData, fieldGroups, defaultFieldGroup, function getEntityId(e) { return e.id(); });
                                } else if (dragData.isRelationship && dragData.isRelationship()) {

                                    /////
                                    // Get the relationships that belong to this definition.
                                    /////
                                    relationships = spFormBuilderService.getDefinitionRelationships(true);

                                    result = allowDropImpl(relationships, dragData, dropData, fieldGroups, defaultFieldGroup, function getEntityId(e) { return e.getEntity().id(); });
                                }
                            }
                        }

                        if (!result) {
                            spFormBuilderService.hideInsertIndicator();
                        }

                        return result;
                    }

                    function dropImp(entities, dragData, dropData, targetFieldGroupId, getEntityId, setRel) {
                        var defaultFieldGroup = null;
                        var inheritedFieldGroups;
                        var fieldGroups;

                        if (!_.some(entities, function (entity) {
	                        return getEntityId(entity) === dragData.getEntity().id();
                        })) {
                            /////
                            // Field does not belong to this definition.
                            /////
                            return;
                        }

                        fieldGroups = spFormBuilderService.getDefinitionFieldGroups(true);

                        if (fieldGroups && fieldGroups.length) {
                            defaultFieldGroup = _.find(fieldGroups, function (fg) {
                                return fg.alias() === 'core:default';
                            });
                        }

                        if (!defaultFieldGroup) {
                            inheritedFieldGroups = spFormBuilderService.getDefinitionFieldGroups(false);

                            if (inheritedFieldGroups && inheritedFieldGroups.length > 0) {
                                defaultFieldGroup = _.find(inheritedFieldGroups, function (fg) {
                                    return fg.getEntity().alias() === 'core:default';
                                });

                                if (defaultFieldGroup) {
                                    defaultFieldGroup = defaultFieldGroup.getEntity();
                                }
                            }
                        }

                        /////
                        // If there is a target field group...
                        /////
                        if (targetFieldGroupId !== -1) {
                            /////
                            // and the field group doesn't belong to this definition...
                            /////
                            if ((defaultFieldGroup && defaultFieldGroup.id() !== targetFieldGroupId) && !_.some(fieldGroups, function (fieldGroup) {
                                return fieldGroup.id() === dropData.fieldGroup.getEntity().id();
                            })) {
                                /////
                                // Target field group does not belong to this definition.
                                /////
                                return;
                            } else {
                                setRel();
                            }
                        } else {
                            dragData.getEntity().setFieldInGroup(null);
                        }
                    }

                    /**
                     * A drop operation has occurred.
                     */

                    function drop(event, source, target, dragData, dropData) {
                        var newField;
                        var newRelationship;
                        var fields;
                        var relationships;
                        var targetFieldGroupId = -1;
                        var onCreatePromise;

                        $('.fb-hilight').removeClass('fb-hilight');

                        if (dragData && dropData && dropData.fieldGroup) {

                            if (dropData.fieldGroup && dropData.fieldGroup.getEntity) {
                                targetFieldGroupId = dropData.fieldGroup.getEntity().id();
                            }

                            fields = spFormBuilderService.getDefinitionFields(true);

                            if (dragData.source === 'new') {

                                /////
                                // Drag from controls panel.
                                /////
                                if (!dragData.isRelationship) {

                                    newField = spFormBuilderService.createField(dragData.type, dragData.name, dragData.additionalData, targetFieldGroupId);

                                    if (dragData.onCreateCallback) {
                                        onCreatePromise = dragData.onCreateCallback(newField, spFormBuilderService.createSource.definition);
                                    } else {
                                        onCreatePromise = $q.when(newField);
                                    }

                                    onCreatePromise.then(function (field) {
                                        if (field) {
                                            fields.add(field);

                                            refreshFields();
                                        }
                                    });


                                } else {
                                    newRelationship = spFormBuilderService.createRelationship(dragData.type, dragData.assignName ? dragData.name : undefined, dragData.cardinality, dragData.additionalData, targetFieldGroupId);

                                    if (dragData.onCreateCallback) {
                                        onCreatePromise = dragData.onCreateCallback(newRelationship, spFormBuilderService.createSource.definition);
                                    } else {
                                        onCreatePromise = $q.when(newRelationship);
                                    }

                                    onCreatePromise.then(function (relationship) {
                                        if (relationship) {
                                            spFormBuilderService.addRelationshipToDefinition(relationship);

                                            refreshFields();
                                        }
                                    });
                                }


                            } else {
                                /////
                                // Drag between field groups.
                                /////
                                if (dragData.isField && dragData.isField()) {
                                    dropImp(fields, dragData, dropData, targetFieldGroupId, function getEntityId(e) { return e.id(); }, function setRel() { dragData.getEntity().setFieldInGroup(dropData.fieldGroup.getEntity()); });
                                } else if (dragData.isRelationship && dragData.isRelationship()) {
                                    relationships = spFormBuilderService.getDefinitionRelationships(true);

                                    dropImp(relationships, dragData, dropData, targetFieldGroupId, function getEntityId(e) { return e.getEntity().id(); }, function setRel() {
                                        if (!dragData.value.isReverse()) {
                                            dragData.getEntity().setRelationshipInFromTypeGroup(dropData.fieldGroup.getEntity());
                                        } else {
                                            dragData.getEntity().setRelationshipInToTypeGroup(dropData.fieldGroup.getEntity());
                                        }
                                    });
                                }

                                refreshFieldGroups();

                                refreshFields();

                                scope.$apply();
                            }
                        }
                    }

                    /**
                     * Drag over event handler.
                     */

                    function dragOver(event, source, target, dragData, dropData) {
                        /////
                        // If the element supports drop, add a hilight to it.
                        /////
                        if (allowDrop(source, target, dragData, dropData)) {
                            $(event.currentTarget).addClass('fb-hilight');
                        }
                    }

                    /**
                     * Drag Leave event handler.
                     */

                    function dragLeave(event) {
                        $(event.currentTarget).removeClass('fb-hilight');
                    }

                    /**
                     * Recursive function for finding the owner type of a field group.
                     */

                    function findFieldGroupOwner(type, fieldGroupId) {
                        var types;

                        if (type && type.fieldGroups) {

                            if (_.some(type.fieldGroups, function (fg) {
                                return fg.id() === fieldGroupId;
                            })) {
                                return type;
                            }

                            types = type.inherits;

                            for (var i = 0; i < types.length; i++) {
                                var childType;

                                childType = findFieldGroupOwner(types[i], fieldGroupId);

                                if (childType) {
                                    return childType;
                                }
                            }
                        }

                        return undefined;
                    }

                    /**
                     * Load routine.
                     */

                    function load() {
                        spFormBuilderService.initializationComplete().then(function () {
                            spFormBuilderService.cacheFieldRenderControls();
                        });

                        focus('searchFields');
                    }

                    /**
                     * Drag End handler.
                     */

                    function dragEnd() {
                        /////
                        // Perform cleanup
                        /////
                        spFormBuilderService.isDragging = false;
                        spFormBuilderService.destroyInsertIndicator();
                    }

                    /**
                     * Drag Start handler.
                     */

                    function dragStart() {
                        spFormBuilderService.isDragging = true;

                        /////
                        // Remove these in drag start rather then drag end so that the drop handler has access to them.
                        /////
                        spFormBuilderService.currentDropTarget = undefined;
                        spFormBuilderService.currentDropTargetIsField = undefined;
                        spFormBuilderService.currentDropTargetQuadrant = undefined;
                    }

                    /**
                     * Check whether the fields are still loading.
                     */
                    function checkFieldsLoading() {
                        if (spFormBuilderService.selectedLookup) {
                            var id;

                            if (spFormBuilderService.selectedLookup.id) {
                                id = spFormBuilderService.selectedLookup.id();
                            } else if (spFormBuilderService.selectedLookup.getEntity) {
                                var entity = spFormBuilderService.selectedLookup.getEntity();

                                if (entity && spFormBuilderService.selectedLookup.isReverse) {
                                    if (!spFormBuilderService.selectedLookup.isReverse() && entity.toType && entity.toType.id) {
                                        id = entity.toType.id();
                                    } else if (spFormBuilderService.selectedLookup.isReverse() && entity.fromType && entity.fromType.id) {
                                        id = entity.fromType.id();
                                    }
                                }
                            }

                            if (id) {
                                var cachedEntity = spFormBuilderService.lookupCache[id];

                                if (cachedEntity && !cachedEntity.requestActive) {
                                    spFormBuilderService.definitionFieldsLoading = false;
                                }
                            }
                        }
                    }


                    /**
                     * Watch for changes in the selected lookup.
                     */
                    scope.$watch('spFormBuilderService.selectedLookup', function (newVal, oldVal) {

                        if (newVal === oldVal || !spFormBuilderService.definition) {
                            return;
                        }

                        checkFieldsLoading();

                        refreshFieldGroups();
                        refreshFields();
                    });

                    /**
                     * Watch for changes in the service revision.
                     */
                    scope.$watch('spFormBuilderService.serviceRevision', function (newVal, oldVal) {

                        if (newVal === oldVal || !spFormBuilderService.definition) {
                            return;
                        }

                        checkFieldsLoading();

                        refreshFieldGroups();
                        refreshFields();
                    });

                    /**
                     * Watch for changes in the definition revision.
                     */
                    scope.$watch('spFormBuilderService.definitionRevision', function (newVal, oldVal) {

                        if (newVal === oldVal || !spFormBuilderService.definition) {
                            return;
                        }

                        checkFieldsLoading();

                        refreshFieldGroups();
                        refreshFields();
                    });

                    /**
                     * Whether the field group can be modified.
                     */
                    scope.canModifyFieldGroup = function () {
                        var fieldGroupEntity;

                        if (this.fieldGroup) {

                            fieldGroupEntity = this.fieldGroup.getEntity();

                            if (fieldGroupEntity) {
                                return spFormBuilderService.isDirectFieldGroup(fieldGroupEntity.id());
                            }
                        }

                        return false;
                    };

                    /**
                     * Whether the field can be modified.
                     */
                    scope.canModifyField = function () {
                        var entity;

                        if (this.field) {

                            entity = this.field.getEntity();

                            if (entity) {
                                if (this.field.isField()) {
                                    return spFormBuilderService.isDirectField(entity.id());
                                } else if (this.field.isRelationship()) {
                                    return spFormBuilderService.isDirectRelationship(entity.id());
                                }
                            }
                        }

                        return false;
                    };

                    /**
                     * Create a new field group.
                     */
                    function createNewFieldGroup() {
                        var fieldGroup;
                        var fieldGroups;
                        var json;

                        if (spFormBuilderService.definition) {

                            json = {
                                typeId: 'core:fieldGroup',
                                name: 'New Field Group',
                                description: 'A user-created field group'
                            };

                            /////
                            // Create the new field group.
                            /////
                            fieldGroup = spEntity.fromJSON(json);

                            /////
                            // Get the field groups from the definition.
                            /////
                            fieldGroups = spFormBuilderService.getDefinitionFieldGroups(true);

                            if (fieldGroups) {
                                /////
                                // Add the new field group.
                                /////
                                fieldGroups.add(fieldGroup);
                            }

                            return fieldGroup;
                        }

                        return undefined;
                    }

                    /**
                     * Create a new field group.
                     */
                    scope.newFieldGroupClick = function () {

                        if (spFormBuilderService.definition) {

                            /////
                            // Create the new field group.
                            /////
                            createNewFieldGroup();

                            /////
                            // Refresh the field groups.
                            /////
                            refreshFieldGroups();
                        }
                    };

                    /**
                     * Remove the selected entity.
                     */
                    scope.removeClick = function () {

                        if (spFormBuilderService.definition && this.field) {
                            var that = this;

                            spDialog.confirmDialog('Confirm delete', 'Are you sure you want to delete this field and its data?').then(function (result) {
                                if (result) {
                                    if (that.field.type === FieldContainer.containerType.field) {
                                        spFormBuilderService.removeField(that.field);
                                    } else if (that.field.type === FieldContainer.containerType.relationship) {
                                        spFormBuilderService.removeRelationship(that.field);
                                    }

                                    refreshFields();
                                }
                            });
                        }
                    };

                    /**
                     * Configure the selected field.
                     */
                    scope.configureField = function () {
                        var field = this.field;
                        var relationshipType = getRelationshipType(field);
                        //Build option variable
                        var options = {
                            formControl: this.field.getEntity(),
                            isFieldControl: false,
                            relationshipType: relationshipType,
                            relationship: field.type === 'relationship' ? field.getEntity() : null,
                            isReverseRelationship: (field.type === 'lookup' || field.type === 'relationship') ? sp.result(field, 'value.isReverse') : undefined,
                            definition: spFormBuilderService.definition
                        };
                        controlConfigureDialogFactory.createDialog(options).then(function (result) {
                            if (result) {

                                if (options && options.relationshipType === 'choice' && options.relationship.dataState === 'create') {
                                    // if required, update the render control type based on single select / multi select
                                    spFormBuilderService.updateChoiceFieldControlType(result);
                                }

                                refreshFields();
                                $rootScope.$broadcast('formControlUpdated');
                            }
                        });
                    };

                    /**
                     * Get the relationship type
                     */

                    function getRelationshipType(field) {
                        if (field.type !== 'field') {
                            if (field.isChoiceField())
                                return 'choice';
                            if (field.isLookup()) {
                                var toType = field.getEntity().toType;

                                if (toType && toType.alias) {
                                    var typeAlias = toType.alias();
                                    if (typeAlias === 'core:photoFileType')
                                        return 'image';
                                    else if (typeAlias === 'core:enumValue')
                                        return 'choice';
                                }
                                if (toType && toType.getInherits) {
                                    var inh = toType.getInherits()[0];
                                    if (inh && inh.alias) {
                                        if (inh.alias() === 'core:enumValue')
                                            return 'choice';
                                    }
                                }
                                return 'lookup';
                            }
                            if (field.isRelationship())
                                return 'relationship';
                        }
                        return 'field';
                    }

                    /**
                     * Removes the specified field group.
                     */

                    function removeFieldGroup(fieldGroup) {
                        if (!fieldGroup) {
                            return;
                        }

                        spFormBuilderService.removeFieldGroup(fieldGroup);

                        /////
                        // Refresh
                        /////
                        refreshFields();
                        refreshFieldGroups();
                    }

                    /**
                     * Remove the selected field group.
                     */
                    scope.removeFieldGroup = function () {

                        if (spFormBuilderService.definition && this.fieldGroup) {

                            if (spFormBuilderService.fieldGroupHasFields(this.fieldGroup)) {
                                var that = this;

                                spDialog.confirmDialog('Confirm delete', 'Are you sure you want to delete this field group and its contained fields?').then(function (result) {
                                    if (result) {
                                        removeFieldGroup(that.fieldGroup);
                                    }
                                });

                                return;
                            }

                            removeFieldGroup(this.fieldGroup);
                        }
                    };

                    /**
                     * Whether this field is mandatory.
                     */
                    scope.isMandatory = function (member) {
                        return member.isRequired();
                    };

                    /**
                     * Whether this field is already on the form.
                     */
                    scope.isOnForm = function (field) {

                        if (!field) {
                            return false;
                        }

                        if (field.isField && field.isField()) {
                            return spFormBuilderService.isFieldOnForm(field);
                        } else if (field.isRelationship && field.isRelationship()) {
                            return spFormBuilderService.isRelationshipOnForm(field);
                        } else {
                            return false;
                        }
                    };


                    /**
                     * Get the field type icon.
                     */
                    scope.getFieldTypeIcon = function () {
                        var typeAlias;
                        var toType;
                        var basePath = 'assets/images/itemicon/';
                        var path = 'StringField.png';

                        if (this.field && this.field.getTypeAlias) {

                            typeAlias = this.field.getTypeAlias();

                            switch (typeAlias) {
                                case 'core:stringField':
                                    path = this.field.getEntity().allowMultiLines ? 'MultilineString.png' : 'StringField.png';
                                    break;
                                case 'core:intField':
                                    path = 'IntField.png';
                                    break;
                                case 'core:decimalField':
                                    path = 'DecimalField.png';
                                    break;
                                case 'core:currencyField':
                                    path = 'CurrencyField.png';
                                    break;
                                case 'core:dateTimeField':
                                    path = 'DateTimeField.png';
                                    break;
                                case 'core:dateField':
                                    path = 'DateField.png';
                                    break;
                                case 'core:timeField':
                                    path = 'TimeField.png';
                                    break;
                                case 'core:boolField':
                                    path = 'BooleanField.png';
                                    break;
                                case 'core:choice':
                                    path = 'ChoiceField.png';
                                    break;
                                case 'core:lookup':
                                    path = 'LookupField.png';
                                    break;
                                case 'core:autoNumberField':
                                    path = 'AutoNumberField.png';
                                    break;
                                case 'core:relationship':
                                    if (this.field.isLookup()) {

                                        toType = this.field.getEntity().toType;

                                        if (toType && toType.alias) {
                                            typeAlias = toType.alias();

                                            switch (typeAlias) {
                                                case 'core:photoFileType':
                                                    path = 'Image.png';
                                                    break;
                                                default:
                                                    path = 'LookupField.png';
                                                    break;
                                            }
                                        }
                                    } else if (this.field.isChoiceField()) {
                                        path = 'ChoiceField.png';
                                    } else {
                                        path = 'relationship.png';
                                    }
                                    break;
                            }
                        }

                        return basePath + path;
                    };

                    /**
                     * Label tooltip function.
                     */
                    scope.getLabelTooltip = function () {
                        if (spFormBuilderService.selectedLookup && spFormBuilderService.selectedLookup.getName) {
                            return 'Following the \'' + spFormBuilderService.selectedLookup.getName() + '\' relationship.';
                        }

                        return undefined;
                    };

                    /**
                     * Filters the list of available fields to only those containing the search value.
                     */
                    scope.filterBySearchBox = function (field) {

                        var search;

                        if (!scope.model.search.value || !field || !field.name)
                            return true;

                        search = scope.model.search.value.toLowerCase();

                        return field.name.toLowerCase().indexOf(search) >= 0;
                    };

                    scope.filterByDataState = function (field) {

                        if (!field || !field.getEntity) {
                            return true;
                        }

                        return field.getEntity().getDataState() !== spEntity.DataStateEnum.Delete;
                    };

                    /**
                     * Drop options.
                     */
                    scope.dropOptions = {
                        onAllowDrop: function (source, target, dragData, dropData) {

                            return allowDrop(source, target, dragData, dropData);
                        },
                        onDrop: function (event, source, target, dragData, dropData) {

                            drop(event, source, target, dragData, dropData);
                        },
                        onDragOver: function (event, source, target, dragData, dropData) {

                            dragOver(event, source, target, dragData, dropData);
                        },
                        onDragLeave: function (event, source, target, dragData, dropData) {

                            dragLeave(event, source, target, dragData, dropData);
                        }
                    };

                    /**
                     * Drag options.
                     */
                    scope.dragOptions = {
                        onDragEnd: function (event, data) {
                            dragEnd(event, data);
                        },
                        onDragStart: function (event, data) {
                            dragStart(event, data);
                        }
                    };

                    /**
                     * Gets whether this definition is editable.
                     */
                    scope.isEditable = function () {

                        return !spFormBuilderService.selectedLookup;
                    };

                    /**
                     * Filters the unallocated fields.
                     */
                    scope.filterFieldByUnallocated = function (field) {

                        if (!field) {
                            return false;
                        }

                        var fieldGroupEntity;

                        fieldGroupEntity = field.getFieldGroupEntity();

                        return !fieldGroupEntity || !_.find(scope.model.fieldGroups, function (fg) {
                            return fg.getEntity().id() === fieldGroupEntity.id();
                        });
                    };

                    /**
                     * A field group has been renamed.
                     */
                    scope.fieldGroupRenamed = function () {
                        refreshFieldGroups();
                    };

                    //function updateChoiceFieldType(relationship) {
                    //    var relationshipEntity;

                    //    if (!relationship) {
                    //        return;
                    //    }

                    //    if (relationship instanceof spEntity._Entity) {
                    //        relationshipEntity = relationship;
                    //    } else {
                    //        relationshipEntity = relationship.getEntity();
                    //    }

                    //    exports.walkGraph(exports.form, function (node) {
                    //        if (node) {
                    //            return node.containedControlsOnForm;
                    //        }

                    //        return null;
                    //    }, function (node, parent) {
                    //        if (node && node.relationshipToRender) {
                    //            if (node.relationshipToRender.id() === relationshipEntity.id()) {
                    //                if (parent && parent.containedControlsOnForm) {

                    //                    if (node.getDataState() === spEntity.DataStateEnum.Create) {
                    //                        parent.containedControlsOnForm.remove(node);
                    //                    }

                    //                    node.dataState = spEntity.DataStateEnum.Delete;

                    //                    return true;
                    //                }
                    //            }
                    //        } else if (node && node.containedControlsOnForm) {
                    //            //remove tab, bug 24861 delete a relationship from the LH pane, thautomatically removed the tab from the RH pane
                    //            //removeTabFromFormByRelationship(node, relationshipEntity.id());
                    //        }

                    //        return false;
                    //    });
                    //}

                    function updateFieldScriptNameIfRequired(newName, oldName, additionalData) {
                        var field = sp.result(additionalData, 'value._fieldEntity');
                        var fieldScriptName = sp.result(field, 'fieldScriptName');
                        if (!field || !fieldScriptName) {
                            console.error('Invalid field info provided');
                            return;
                        }

                        if (field.dataState === spEntity.DataStateEnum.Create && oldName.toLowerCase() === fieldScriptName.toLowerCase()) {
                            field.fieldScriptName = newName;
                        }
                    }
                   

                    /**
                     * A field has been renamed.
                     */
                    scope.fieldRenamed = function (newName, oldName, additionalData) {
                        updateFieldScriptNameIfRequired(newName, oldName, additionalData);

                        refreshFields();

                        $rootScope.$broadcast('formControlUpdated');
                    };

                    /**
                     * Determines whether the specified field name and field's script name is valid.
                     */
                    scope.isValidFieldName = function (newName, oldName, additionalData) {

                        try {
                            var field = sp.result(additionalData, 'value._fieldEntity');
                            // validate field name
                            var result = spFormBuilderService.validateFieldName(newName, field.idP);
                            if (result.hasError) {
                                spAlertsService.addAlert(result.message, { severity: spAlertsService.sev.Warning, expires: true });
                                return false;
                            }

                            // validate field script name
                            var existingFieldScriptName = sp.result(field, 'fieldScriptName');
                            var scriptNameValueToValidate = existingFieldScriptName;
                            if (field.dataState === spEntity.DataStateEnum.Create && oldName.toLowerCase().trim() === existingFieldScriptName.toLowerCase().trim()) {
                                scriptNameValueToValidate = newName;
                            }

                            result = spFormBuilderService.validateFieldName(scriptNameValueToValidate, field.idP);
                            if (result.hasError) {
                                spAlertsService.addAlert(result.message, { severity: spAlertsService.sev.Warning, expires: true });
                                return false;
                            }

                            return true;
                            
                        } catch (e) {
                            console.error(e.message);
                        }

                        return true;
                    };

                    /**
                     * Gets the Type script name
                     */
                    function getTypeScriptName() {
                        var targetType = spFormBuilderService.definition;

                        if (targetType && targetType.typeScriptName) {
                            return targetType.typeScriptName;
                        }

                        return null;
                    }

                    /**
                     * Gets whether this field group name is valid.
                     */
                    scope.isValidFieldGroupName = function (newName, oldName) {

                        if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                            return true;
                        }

                        if (!newName) {
                            spAlertsService.addAlert('Invalid field group name specified.', { severity: spAlertsService.sev.Warning, expires: true });
                            return false;
                        }

                        return true;
                    };

                    /**
                     * Get the label style.
                     */
                    scope.getLabelStyle = function (canModify) {
                        var style = {};

                        if (!canModify) {
                            style['font-style'] = 'italic';
                        }

                        return style;
                    };

                    /**
                     * Gets the field group tooltip
                     */
                    scope.getFieldGroupTooltip = function () {
                        var types;
                        var fieldGroupId;

                        fieldGroupId = this.fieldGroup.getEntity().id();

                        if (spFormBuilderService.definition) {
                            types = spFormBuilderService.definition.inherits;

                            if (types) {
                                for (var i = 0; i < types.length; i++) {
                                    var childType;

                                    childType = findFieldGroupOwner(types[i], fieldGroupId);

                                    if (childType) {
                                        return 'Inherited from ' + childType.name;
                                    }
                                }
                            }
                        }

                        return undefined;
                    };

                    /**
                     * Stops certain characters from being entered into the editable labels.
                     */
                    scope.validateinput = function (evt) {
                        var e = evt || event;

                        if (e.shiftKey) {
                            switch (e.which) {
                                case 188: // <
                                case 190: // >
                                    e.stopPropagation();
                                    e.preventDefault();
                                    return false;
                            }
                        }

                        return true;
                    };

                    /**
                     * Change validation.
                     */
                    scope.changeValidate = function (value) {

                        if (value) {
                            return value.replace(/[<>]+/g, '');
                        }
                        return value;
                    };

                    load();

                    var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/directives/spFormBuilderToolboxFields/spFormBuilderToolboxFields.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        }])
        .filter('filterFieldByFieldGroup', function () {
            /**
             * Filter the fields that belong to the specified field group.
             */
            return function (fields, fieldGroup) {

                var fieldsInFieldGroup = [];
                var entity;
                var entityId;

                if (!fields || !fieldGroup) {
                    return false;
                }

                entity = fieldGroup.getEntity();

                if (entity) {

                    entityId = entity.id();

                    fieldsInFieldGroup = _.filter(fields, function (field) {
                        var fieldGroupEntity;

                        /////
                        // Get the field group that this field belongs to.
                        /////
                        fieldGroupEntity = field.getFieldGroupEntity();

                        /////
                        // Determine whether this 
                        /////
                        if (fieldGroupEntity) {
                            return fieldGroupEntity.id() === entityId;
                        } else {
                            return false;
                        }
                    });
                }

                return fieldsInFieldGroup;
            };
        });
}());