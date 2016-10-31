// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('mod.app.formBuilder.factories.FieldContainer', [

])
    /**
    * Module implementing the form builder field container.
    *
    * @module FieldContainer
    */
    .factory('FieldContainer', function () {
        'use strict';

        /////
        // Field Container type.
        /////
        function FieldContainer(id, value, type) {

            if (!(value instanceof spResource.Field) && !(value instanceof spResource.Relationship)) {
                throw 'invalid value specified';
            }

            this.value = value;
            this.type = type;
            this.id = id;
        }

        FieldContainer.prototype = {

            isField: function () {
                /////
                // FieldContainer.isField method.
                /////
                return this.type === FieldContainer.containerType.field;
            },
            isRelationship: function () {
                /////
                // FieldContainer.isRelationship method.
                /////
                return this.type === FieldContainer.containerType.relationship;
            },
            isLookup: function () {
                /////
                // FieldContainer.isLookup method.
                /////
                return this.type === FieldContainer.containerType.relationship && this.value.isLookup();
            },
            isChoiceField: function () {
                /////
                // FieldContainer.isChoiceField method.
                /////
                return this.type === FieldContainer.containerType.relationship && this.value.isChoiceField();
            },
            getEntity: function () {
                /////
                // FieldContainer.getEntity method.
                /////
                return this.value.getEntity();
            },
            getValue: function () {
                /////
                // FieldContainer.getValue method.
                /////
                return this.value;
            },
            getFieldGroupEntity: function () {
                /////
                // FieldContainer.getFieldGroupEntity method.
                /////
                if (this.isField()) {
                    return this.value.getFieldGroupEntity();
                } else if (this.isRelationship()) {
                    if (!this.value.isReverse()) {
                        return this.value.getEntity().relationshipInFromTypeGroup;
                    } else {
                        return this.value.getEntity().relationshipInToTypeGroup;
                    }
                }

                return undefined;
            },
            getTypeAlias: function () {
                /////
                // FieldContainer.getTypeAlias method.
                /////
                var types;
                var type;

                if (this.isField()) {
                    if (this.value.getTypes) {
                        types = this.value.getTypes();

                        if (types && types.length > 0) {
                            type = types[0];

                            if (type && type.getAlias) {
                                return type.getAlias();
                            }
                        }
                    }

                    return undefined;
                } else if (this.isRelationship()) {
                    return 'core:relationship';
                }

                return undefined;
            }
        };


        /**
         * FieldContainer.name property.
         */
        Object.defineProperty(FieldContainer.prototype, 'name', {
            get: function () {
                return this.value.getName() || this.value.getEntity().name;
            },
            set: function (value) {
                if (this.type === FieldContainer.containerType.relationship) {
                    if (this.value.isReverse() && this.getEntity().hasField('fromName')) {
                        this.getEntity().fromName = value;
                    } else if (!this.value.isReverse() && this.getEntity().hasField('toName')) {
                        this.getEntity().toName = value;
                    } else {
                        this.getEntity().name = value;
                    }
                } else {
                    this.getEntity().name = value;
                }

                /////
                // Hack: Delete the cached value!
                /////
                if (this.value.hasOwnProperty('_val_name')) {
                    delete this.value._val_name;
                }
            },
            enumerable: true,
            configurable: true
        });

        /**
         * FieldContainer.description property.
         */
        Object.defineProperty(FieldContainer.prototype, 'description', {
            get: function () {
                return this.value.getEntity().description;
            },
            set: function (value) {
                this.value.getEntity().description = value;
            },
            enumerable: true,
            configurable: true
        });

        FieldContainer.containerType = {
            /**
             * Field Container entity types
             */
            field: 'field',
            relationship: 'relationship'
        };

        return (FieldContainer);
    });