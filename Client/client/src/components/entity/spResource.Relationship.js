// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spResource;

(function (spResource) {

    /**
     * Relationship constructor. Do not use.
     * @private
     * @class
     * @name spResource.Relationship
     *
     * @classdesc
     * TODO
     */
    var Relationship = (function () {

        function Relationship(relationshipEntity, typeEntity, isReverse) {
            if (!relationshipEntity)
                throw new Error('relationshipEntity is required');
            this._relEntity = relationshipEntity;   // the relationship definition entity
            this._typeEntity = typeEntity;          // the type entity from which this relationship is extending
            this._isReverse = isReverse;
            try {
                this._card = relationshipEntity.getLookup('cardinality').eid().getNsAlias();
            }
            catch (e) { }
            if (!this._card) {
                throw new Error('Cardinality must be loaded.');
            }
            this._toOne = spResource.Relationship.toOne(this._card, isReverse);
            this._fromOne = spResource.Relationship.fromOne(this._card, isReverse);
        }


        /**
         * Returns the visible name of the field.
         *
         * @returns {string} The effective name of this field.
         *
         * @function
         * @name spResource.Relationship#getName
         */
        Relationship.prototype.getName = function () {
            if (!this._name) {
                var dirName = this._isReverse ? this._relEntity.getField('fromName') : this._relEntity.getField('toName');
                this._name = dirName || this._relEntity.getName();
            }
            return this._name;
        };

        /**
         * Returns the effective description of the relationship.
         *
         * @returns {string} The effective description of this relationship.
         *
         * @function
         * @name spResource.Relationship#getDescription
         */
        Relationship.prototype.getDescription = function () {
            var res = this._relEntity.getField('description');
            return res;
        };

        /**
         * Returns the indentifier name to refer to this relationship in scripts.
         *
         * @returns {string} The effective script name of this relationship.
         *
         * @function
         * @name spResource.Relationship#getScriptName
         */
        Relationship.prototype.getScriptName = function () {
            if (!this._scriptName) {
                var dirName = this._relEntity[this._isReverse ? 'fromScriptName' : 'toScriptName'];
                this._scriptName = dirName || this.getName();
            }
            return this._scriptName;
        };

        /**
         * Returns the entity for this field.
         *
         * @function
         * @name spResource.Relationship#getEntity
         */
        Relationship.prototype.getEntity = function () {
            return this._relEntity;
        };


        /**
         * Returns the entity that represents the field group for this relationship.
         *
         * @returns {spEntity.Entity} The field-group entity for this relationship on this end type.
         *
         * @function
         * @name spResource.Relationship#getFieldGroupEntity
         */
        Relationship.prototype.getFieldGroupEntity = function () {
            var relAlias = this._isReverse ? 'relationshipInToTypeGroup' : 'relationshipInFromTypeGroup';
            var fg = this._relEntity.getLookup(relAlias);
            return fg || null;
        };


        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if this has a 'to-one' cardinality relative to the current type.
         *
         * @function
         * @name spResource.Relationship#isToOne
         */
        Relationship.prototype.isToOne = function () {
            return this._toOne;
        };

        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if this has a 'to-many' cardinality relative to the current type.
         *
         * @function
         * @name spResource.Relationship#isToMany
         */
        Relationship.prototype.isToMany = function () {
            return !this._toOne;
        };

        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if this has a 'from-one' cardinality relative to the current type.
         *
         * @function
         * @name spResource.Relationship#isFromOne
         */
        Relationship.prototype.isFromOne = function () {
            return this._fromOne;
        };


        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if this has a 'from-many' cardinality relative to the current type.
         *
         * @function
         * @name spResource.Relationship#isToMany
         */
        Relationship.prototype.isFromMany = function () {
            return !this._fromOne;
        };


        /**
         * Returns true if this member is a field.
         *
         * @returns {bool} True if this is a field.
         *
         * @function
         * @name spResource.Relationship#isField
         */
        Relationship.prototype.isField = function () {
            return false;
        };


        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if this is a lookup.
         *
         * @function
         * @name spResource.Relationship#isLookup
         */
        Relationship.prototype.isLookup = function () {
            return this.isToOne() && !this.isChoiceField();
        };


        /**
         * Returns true if this is a 'relationship' in the UI sense. That is, to-many, and not a choice field.
         *
         * @returns {bool} True if this is a relationship.
         *
         * @function
         * @name spResource.Relationship#isRelationship
         */
        Relationship.prototype.isRelationship = function () {
            return this.isToMany() && !this.isChoiceField();
        };


        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @returns {bool} True if the relationship goes into this type.
         *
         * @function
         * @name spResource.Relationship#isReverse
         */
        Relationship.prototype.isReverse = function () {
            return this._isReverse;
        };


        /**
        * Returns true if the relationship is required.
        *
        * @returns {bool} True if the relationship is required. Defaults to false if not required.
        *
        * @function
        * @name spResource.Relationship#isRequired
        */
        Relationship.prototype.isRequired = function () {
            var required = this._isReverse ? this._relEntity.revRelationshipIsMandatory : this._relEntity.relationshipIsMandatory;
            return required || false;
        };


        /**
        * Returns true if the show relationship text help.
        *
        * @returns {bool} True if show relationship text help. Defaults to false if not.
        *
        * @function
        * @name spResource.Relationship#showTextHelp
        */
        Relationship.prototype.showTextHelp = function () {
            var showTextHelp = this._relEntity.showRelationshipHelpText;
            return showTextHelp || false;
        };


        /**
        * Returns true if the relationship is readonly.
        *
        * @returns {bool} True if the relationship is readonly. Defaults to false if not required.
        *
        * @function
        * @name spResource.Relationship#isReadOnly
        */
        Relationship.prototype.isReadOnly = function () {
            return false;
        };


        /**
         * Returns true if the relationship is hidden (relative to the parent type).
         *
         * @returns {bool} True if the relationship is hidden relative to the current type.
         *
         * @function
         * @name spResource.Relationship#isHidden
         */
        Relationship.prototype.isHidden = function () {
            var hField = this._isReverse ? 'hideOnToType' : 'hideOnFromType';
            var hidden = this._relEntity.getField(hField);
            return hidden || false;
        };


        /**
         * Returns true if this relationship is reverse, otherwise false.
         *
         * @function
         * @name spResource.Relationship#isChoiceField
         */
        Relationship.prototype.isChoiceField = function () {
            if (this._isReverse)
                return false;
            if (!this._isChoiceLoaded) {
                try {
                    var toType = this._relEntity.getToType();
                    var inh = toType && toType.getInherits ? toType.getInherits()[0] : null;
                    if (inh) {
                        this._isChoiceField = inh.eid().getNsAlias() === 'core:enumValue';
                    } else {
                        this._isChoiceField = this._relEntity.getRelType() ? (this._relEntity.getRelType().eid().getNsAlias() === 'core:relChoiceField' || this._relEntity.getRelType().eid().getNsAlias() === 'core:relMultiChoiceField') : false;
                    }
                }
                catch (e)
                {
                    this._isChoiceField = false;
                }
                this._isChoiceLoaded = true;
            }
            return this._isChoiceField;
        };



        /**
         * Returns true if this relationship is StructureLevel, otherwise false.
         *
         * @function
         * @name spResource.Relationship#isStructureLevel
         */
        Relationship.prototype.isStructureLevel = function () {
            
            if (this._isReverse)
                return false;
          
            try {                
                var toType = this._relEntity.getToType();
                this._isStructureLevel = toType ? toType.eid().getNsAlias() === 'core:structureLevel' : false;
            }
            catch (e) {
                this._isStructureLevel = false;
            }            
            
            return this._isStructureLevel;
        };

        /**
         * Returns a description of the member type
         */
        Relationship.prototype.memberTypeDesc = function () {
            return this.isLookup() ? 'Lookup'
                : this.isChoiceField() ? (this.isToMany() ? 'Multi Choice' : 'Choice Field')
                : 'Relationship';
        };


        /**
          * Returns the solutions array of the relationship.
          *
          * @returns [{object}] The solutions array of this relationship.
          *
          * @function
          * @name spResource.Relationship#getSolutions
          */
        Relationship.prototype.getSolutions = function() {
            return this._relEntity.getInSolution();
        };


        Relationship.prototype._isVisible = function (options) {
            if (options && options.showHidden) {
                return true;
            }
            if (options && options.hideNonWritable) {                
                var alias = this._relEntity.eid().getNsAlias();
                if (!this._isReverse && (alias === 'core:createdBy' || alias === 'core:lastModifiedBy' || alias === 'core:isOfType')) {
                    return false;
                }
                if (sp.result(this._relEntity, 'toType.nsAlias') === 'core:photoFileType') {
                    return false;
                }
                return true;
            }

            // hide choice field relationship if viewed in reverse direction in form builder.
            // #28082:Form builder: getting weird message when checking choice field details in form builder.
            let relTypeAlias = sp.result(this._relEntity, 'relType.nsAlias');
            if (this._isReverse &&
                    (relTypeAlias === 'core:relMultiChoiceField' || relTypeAlias === 'core:relChoiceField')) {
                return false;
            }

            return !this.isHidden();
        };


        return Relationship;
    })();

    spResource.Relationship = Relationship;

    // Public static helper for resolving cardinality
    var toOne = function (cardinality, isReverse) {
        var toOne = isReverse ?
            (cardinality === 'core:oneToMany' || cardinality === 'core:oneToOne')
            : (cardinality === 'core:manyToOne' || cardinality === 'core:oneToOne');
        return toOne;
    };

    spResource.Relationship.toOne = toOne;

    // Public static helper for resolving cardinality
    spResource.Relationship.fromOne = function (cardinality, isReverse) {
        var fromOne = spResource.Relationship.toOne(cardinality, !isReverse);
        return fromOne;
    };

})(spResource || (spResource = {}));
