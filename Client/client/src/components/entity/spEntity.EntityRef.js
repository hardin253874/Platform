// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, sp, spUtils */

var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {
    'use strict';

    var EntityRef = (function() {

        /**
         * Construct an EntityRef given an argument that can be a numeric id, a string alias, or ns:alias, or an object
         * either the raw entity ref object we get on the wire, or an existing EntityRef object to copy.
         * @private
         * @class
         * @name spEntity.EntityRef
         *
         * @classdesc
         * The EntityRef object is the "identifier" for an entity and can be a int64 id or a ns + alias combo, or both.
         */
        function EntityRef(id, fast) {
            var parts;

            if (fast) {
                // ID is numeric, nsAlias is either absent or fully qualified
                this._id = id.id;
                this._serId = id.id; // see notes at end
                var nsAlias = id.nsAlias;
                if (nsAlias) {
                    var i = nsAlias.indexOf(':');
                    this._ns = nsAlias.slice(0, i);
                    this._alias = nsAlias.slice(i + 1);
                }
                return;
            }

            if (id === null || id === undefined) {
                // accept null
                this._ns = null;
                this._alias = null;
            } else if (_.isFinite(id)) {
                // accept an ID number
                this._id = id;
                this._ns = null;
                this._alias = null;
            } else if (typeof id === 'string') {
                // accept alias or ns:alias or ID as string
                if (spUtils.stringIsNumber(id)) {
                    // accept an ID number
                    this._id = parseInt(id, 10);
                    this._ns = null;
                    this._alias = null;
                } else {
                    parts = id.split(':');
                    if (parts.length > 1) {
                        this._ns = parts[0];
                        this._alias = parts[1];
                    } else {
                        this._alias = id;
                    }
                }
            } else if (id._id || id._alias) {
                // accept another EntityRef as an argument
                this._id = id._id;
                this._ns = id._ns;
                this._alias = id._alias;
            } else if (id.nsAlias) {
                this._id = id.id;
                parts = id.nsAlias.split(':');
                if (parts.length === 1) {
                    this._alias = id.nsAlias;
                } else {
                    this._ns = parts[0];
                    this._alias = parts[1];
                }
            }
            else if (id.id || id.alias) {
                // accept a set of ID/NS/Alias as an argument
                this._id = id.id;
                this._ns = id.ns;
                this._alias = id.alias;
            } else {
                throw new Error('Invalid ID or alias.');
            }

            if (this._alias && !this._ns) {
                this._ns = 'core';
            }
            if (!this._id) {
                this._id = 0;
            }
            // See notes in _getIdOrDummyId()
            this._serId = this._id;

            // Map namespace aliases
            if (this._ns === 'k') this._ns = 'console';
            if (this._ns === 's') this._ns = 'shared';
            if (this._ns === 't') this._ns = 'test';
        }


        /**
         * The ID number of the entity.
         *
         * @returns {number} The ID of the entity, or zero. ID should not be null.
         *
         * @function
         * @name spEntity.EntityRef#getId
         */
        EntityRef.prototype.getId = function () {
            return this._id;
        };

        Object.defineProperty(EntityRef.prototype, 'idP', {
            get: function () { return this.getId(); },
            enumerable: true
        });


        /**
         * Returns the ID number of the entity. Or if the ID number is zero, generates a unique number and returns that instead (without changing ID).
         * The serialization protocol requires that every EntityRef has an ID, but to the client API we want to expose some entities as having just an
         * alias with a zero ID.
         *
         * @returns {number} The ID of the entity, or a dummy number.
         *
         * @function
         * @name spEntity.EntityRef#_getIdOrDummyId
         */
        EntityRef.prototype._getIdOrDummyId = function () {

            if (!_.isFinite(this._id)) {
                this._id = 0;

                if (!_.isFinite(this._serId)) {
                    this._serId = 0;
                }
            }

            if (this._id === 0) {
                if (this._serId === 0) {
                    this._serId = spEntity._getNextId();
                }
                return this._serId;
            }
            return this._id;
        };


        /**
         * The alias of the entity. Does not include the namespace.
         *
         * @returns {string} The alias if defined, otherwise null.
         *
         * @function
         * @name spEntity.EntityRef#getAlias
         */
        EntityRef.prototype.getAlias = function () {
            return this._alias;
        };


        /**
         * The namespace of the alias. Namespace is initialized to 'core' if only an alias was provided.
         * If no alias was provided, then the namespace will be null.
         *
         * @returns {string} The namespace (or 'core').
         *
         * @function
         * @name spEntity.EntityRef#getNamespace
         */
        EntityRef.prototype.getNamespace = function () {
            return this._ns;
        };


        /**
         * The namespace-alias combination. Namespace and alias will be separated by a colon.
         * Namespace is initialized to 'core' if only an alias was provided.
         * If no alias was provided, then this accessor will return null.
         *
         * @returns {string} The namespace (or 'core').
         *
         * @function
         * @name spEntity.EntityRef#getNsAlias
         */
        EntityRef.prototype.getNsAlias = function () {
            return !this._alias ? null : (this._ns + ':' + this._alias);
        };

        Object.defineProperty(EntityRef.prototype, 'nsAlias', {
            get: function () { return this.getNsAlias(); },
            enumerable: true
        });

        /**
         * Sets the namespace/alias on this entity. Use when this information isn't available at the time the entityRef was constructed.
         *
         * @params {string} nsAlias The namespace:alias.
         *
         * @function
         * @name spEntity.EntityRef#setNsAlias
         */
        EntityRef.prototype.setNsAlias = function (nsAlias) {
            var parts = nsAlias.split(':');

            if (parts.length === 2) {
                this._ns = parts[0];
                this._alias = parts[1];
            } else {
                this._ns = 'core';
                this._alias = parts[0];
            }

            // Map namespace aliases
            if (this._ns === 'k') this._ns = 'console';
            if (this._ns === 's') this._ns = 'shared';
            if (this._ns === 't') this._ns = 'test';
        };



        /**
         * The namespace-alias combination, or if not available, the ID.
         *
         * @returns {string} The namespace (or 'core').
         *
         * @function
         * @name spEntity.EntityRef#getNsAliasOrId
         */
        EntityRef.prototype.getNsAliasOrId = function () {
            return !this._alias ? this._id : (this._ns + ':' + this._alias);
        };

        Object.defineProperty(EntityRef.prototype, 'nsAliasOrId', {
            get: function () { return this.getNsAliasOrId(); },
            enumerable: true
        });

        /* legacy */
        EntityRef.prototype.id = EntityRef.prototype.getId;
        /* legacy */
        EntityRef.prototype.alias = EntityRef.prototype.getNsAlias;
        

        /**
         * Compares this EntityRef to anything that can be converted to a valid EntityRef.
         * If ID is specified on both, then both must match.
         * If alias is specified on both, then both must match.
         * At least one of ID or or alias must match.
         *
         * @returns {boolean} True if this matches the value.
         *
         * @function
         * @name spEntity.EntityRef#matches
         */
        EntityRef.prototype.matches = function (value) {
            if (!value) {
                return false;
            }

            var idMatch = false;
            var aliasMatch = false;

            var other = new spEntity.EntityRef(value);

            if (this._id !== 0 && other._id !== 0) {
                idMatch = this._id === other._id;
                if (!idMatch)
                    return false;
            }

            if (this._alias && other._alias) {
                aliasMatch = this.getNsAlias() === other.getNsAlias();
                if (!aliasMatch)
                    return false;
            }

            return idMatch || aliasMatch;
        };


        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        EntityRef.prototype._asRawEntityRef = function () {
            return {
                id: this._getIdOrDummyId(),
                ns: this._ns,
                alias: this._alias
            };
        };
        
        /** Return a "fully qualified" alias given an alias string that may or may not have a namespace prefix */
        EntityRef.alias = function alias(aliasStr) {
            return aliasStr.indexOf(':') < 0 ? 'core:' + aliasStr : aliasStr;
        };

        /** Merge an array of entityRefs into an array of raw entity refs */
        EntityRef.mergeEntityRefs = function mergeEntityRefs(rawEntityRefs, entityRefs) {
            entityRefs.forEach(function (entityRef) {
                EntityRef.mergeEntityRef(rawEntityRefs, entityRef);
            });
        };
        
        /** Merge a single entityRef into an array of raw entity refs */
        EntityRef.mergeEntityRef = function mergeEntityRef(rawEntityRefs, entityRef) {
            var rawEntityRef = _.find(rawEntityRefs, function(rawEr) {
                return entityRef._getIdOrDummyId() === rawEr.id;
            });
            if (!rawEntityRef) {
                //console.log('mergeEntityRefs: creating', entityRef.id(), entityRef.alias());
                rawEntityRef = {
                    id: entityRef._getIdOrDummyId()
                };
                rawEntityRefs.push(rawEntityRef);
            } else {
                //console.log('mergeEntityRefs: merging', entityRef.id(), entityRef.alias());
            }
            if (!rawEntityRef.ns) {
                rawEntityRef.ns = entityRef._ns;
            }
            if (!rawEntityRef.alias) {
                rawEntityRef.alias = entityRef._alias;
            }
        };
        return EntityRef;
    })();
    spEntity.EntityRef = EntityRef;



    /**
     * Converts a value to an EntityRef (using the same rules as the constructor).
     * Or passes value along if it is already an entityRef.
     *
     * @param {*} value An EntityRef, ID, or alias.
     * @returns {EntityRef} An EntityRef.
     *
     * @function
     * @name spEntity.asEntityRef
     */
    var asEntityRef = function (value) {
        if (!value)
            return value;
        else if (value instanceof EntityRef)
            return value;
        else if (value.eid) // is Entity
            return value.eid();
        else
            return new spEntity.EntityRef(value);
    };
    spEntity.asEntityRef = asEntityRef;



    /**
     * Accepts some sort of input that might represent an entity ID and returns an alias string or ID number.
     * Throws an exception if invalid.
     *
     * @params {*} id An alias, ID, or anything else that can be converted to an entity ref.
     * @returns {string|number} An alias or ID.
     *
     * @function
     * @name spEntity.asAliasOrId
     */
    var asAliasOrId = function (id) {
        var ref = new EntityRef(id);
        var res = ref.getNsAliasOrId();
        return res;
    };
    spEntity.asAliasOrId = asAliasOrId;
    
})(spEntity || (spEntity = EdcEntity = {}));

