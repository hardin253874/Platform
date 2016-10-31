// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, sp */


/**
 * @ngdoc module
 * @name editForm.service:resourceScopeService
 *
 *  @description Service used to mediate bindings between resourceScope seners and receivers
 */

(function () {
    'use strict';

    angular.module('mod.app.resourceScopeService', ['ng', 'mod.common.spEntityService']);


    /**
     * @ngdoc service
     * @name mod_app.editFormServices:editFormServices
     *
     *  @description This service is used by any Page that used the custom edit forms.
     */

    angular.module('mod.app.resourceScopeService').factory('spResourceScope', function ($rootScope) {


        var exports = {};
        var scopeForMessenging = $rootScope.$new();

        function createChannelId(senderControl) {
            return '' + senderControl.eid().getNsAliasOrId();
        }

        /*
         * Get the channel Id to use for a relationship Resource
         * @param {object} senderControl The sending formControl.
         * @param {object} reEntity The relationship entity holding the reference the relationshop.
         */
        exports.getChannelIdFromSender = function (senderControl) {
            return createChannelId(senderControl);
        };

        /*
         * Get the send Id to use for a relationship Resource
         * @param {object} senderControl The sending formControl.
         * @param {object} reEntity The relationship entity holding the reference the relationshop.
         */
        exports.getChannelIdFromReceiver = function (receiverControl) {
            var senderControl = receiverControl.receiveContextFrom;
            return createChannelId(senderControl);
        };

        /*
         * Send an update scope message.
         * @param {object} formControl The formControl sending the update.
         * @param {object} context (optional) The context that the message is sent in. For a relationship this would be the relationship id.
         * @param {object} id The id of the new context.
         */
        exports.sendScopeUpdate = function (channelId, id) {
            if (!channelId) {
                throw new Error('spResourceScope: sendScopeUpdate: argument error, no channelId provided.');
            }

            scopeForMessenging.$broadcast(channelId, id);
        };


        function getIdFromReciever(receiver) {

        }

        function getIdFromSenderRelationship(relationship) {

        }

        /**
         * Register an interest on a scope changing.
         *
         * @param {object} formControl The form control interested in the scope changes.
         * @param {function} The function to be called when the scope changes. The functions one parameter is the id of the new scope item.
         * @returns {Entity} The created entity
         */
        exports.onScopeUpdate = function (channelId, onChangeFunction) {

            if (!channelId) {
                throw new Error('spResourceScope: onScopeUpdate:  argument error, no channelId provided.');
            }


            return scopeForMessenging.$on(channelId, function (event, id) {
                onChangeFunction(id);
            });
        };


        return exports;
    });

}());