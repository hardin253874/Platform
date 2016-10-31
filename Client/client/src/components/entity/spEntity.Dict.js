// Copyright 2011-2016 Global Software Innovation Pty Ltd

var spEntity;

var EdcEntity;  // legacy

(function (spEntity) {

    /**
     * Dict constructor.
     * @private
     * @class
     * @name spEntity.Dict
     *
     * @classdesc
     * A dictionary that can use IDs, Aliases, Entities, and EntityRefs as keys. Entries store against both ID and Alias, and can be retrieved by either.
     * Can also be effectively used as a Set of entities by not passing any value argument to the adder.
     */
    var Dict = (function () {
        function Dict() {
            this.entities = {};
        }

        /**
         * Adds the entity to the set, both by ID and alias.
         * If the value is undefined, then the key is stored - so it can be used as a set.
         *
         * @param {object} key An Entity, ID, alias, or EntityRef.
         * @param {object} value Whatever you like.
         *
         * @function
         * @name spEntity.Dict.add
         */
        Dict.prototype.add = function (key, value) {
            if (key) {
                var val = _.isUndefined(value) ? key : value;
                var er = spEntity.asEntityRef(key);
                var id = er.getId();
                var alias = er.getNsAlias();
                if (id)
                    this.entities[id] = val;
                if (alias)
                    this.entities[alias] = val;
            }
        };

        /**
         * Retrieves a value from the dictionary.
         *
         * @param {object} key An Entity, ID, alias, or EntityRef.
         * @returns {object} The value.
         *
         * @function
         * @name spEntity.Dict#get
         */
        Dict.prototype.get = function (key) {
            if (!key)
                return key;
            var er = spEntity.asEntityRef(key);
            var find = er.getNsAliasOrId();
            var res = this.entities[find];
            return res;
        };

        /**
         * Determines whether a value is in the dictionary.
         *
         * @param {object} key An Entity, ID, alias, or EntityRef.
         * @returns {object} The value.
         *
         * @function
         * @name spEntity.Dict#contains
         */
        Dict.prototype.contains = function (key) {
            return !_.isUndefined(this.get(key));
        };

        /**
         * Removes a key/value from the dictionary.
         *
         * @param {object} key An Entity, ID, alias, or EntityRef.
         * @returns {object} The value.
         *
         * @function
         * @name spEntity.Dict#remove
         */
        Dict.prototype.remove = function (key) {
            if (key) {
                var er = spEntity.asEntityRef(key);
                var id = er.getId();
                var alias = er.getNsAlias();
                if (id)
                    this.entities[id] = undefined;
                if (alias)
                    this.entities[alias] = undefined;
            }
        };

        /**
         * Returns all values.
         *
         * @returns {Array} An array of values.
         *
         * @function
         * @name spEntity.Dict#values
         */
        Dict.prototype.values = function () {
            return _.values(this.entities);
        };
        
        return Dict;
    })();
    spEntity.Dict = Dict;

})(spEntity || (spEntity = EdcEntity = {}));

