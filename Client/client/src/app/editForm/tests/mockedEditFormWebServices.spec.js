// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mockedEditFormWebServices', ['ng']);

angular.module('mockedEditFormWebServices').factory('editFormWebServices', function ($q, $timeout, $rootScope) {
    /**
     *  A set of client side services working against the edit form webapi service.
     *  @module spEditForm
     */
    var exports = {};

    exports.isMockService = true;
    exports.haveCalled = false;

    exports.getFormForInstance = function () { throw new Error('getFormForInstance has not been mocked.'); };
    exports.getFormForDefinition = function () { throw new Error('getFormForDefinition has not been mocked.'); };
    exports.getIdForRequest = function () { throw new Error('getIdForRequest has not been mocked.'); };

    var getFormDefinitionMock = {};
    var getFormForDefinitionMock = {};

    /* Create a mock and register it with the injector. */
    exports.register = function () {
        module(function ($provide) {
            $provide.value('editFormWebServices', this);
        });
    };

    exports.mockGetFormDefinition = function (entity) {
        var arr = _.isArray(entity) ? entity : [entity];

        for (var i = 0; i < arr.length; i++) {
            var e = arr[i];
            var data = { entity: e };
            getFormDefinitionMock[e.eid().getId()] = data;
            getFormDefinitionMock[e.eid().getNsAlias()] = data;
            if (e.eid().getNamespace() === 'core') {
                getFormDefinitionMock[e.eid().getAlias()] = data;
            }
        }
        return this;
    };

    exports.mockGetFormForDefinition = function (def, entity) {
        getFormForDefinitionMock[def] = entity;
        
        return this;
    };

    /* Mocked implementation of the getFormDefinition method */
    exports.getFormDefinition = function (selectedFormIdOrAlias, isInDesignMode) {
        var data = getFormDefinitionMock[selectedFormIdOrAlias];
        if (!data)
            throw new Error('No mock data was provided for ' + selectedFormIdOrAlias);

        exports.haveCalled = true;
        return $q.when(data.entity);
    };

    /* Mocked implementation of the getFormForDefinition method */
    exports.getFormForDefinition = function (selectedFormIdOrAlias, isInDesignMode) {
        var data = getFormForDefinitionMock[selectedFormIdOrAlias];
        if (!data)
            throw new Error('No mock data was provided for ' + selectedFormIdOrAlias);

        exports.haveCalled = true;
        return $q.when(data);
    };
    return exports;
});

