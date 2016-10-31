export default class EntityService {
    constructor($q) {
        this.$q = $q;
    }

    /**
     *
     * @param {number|string} id
     * @param {string} request
     */
    getEntity(id, request) {
        return this.$q.when({id});
    }
}

