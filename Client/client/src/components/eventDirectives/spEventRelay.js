// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a event relay for sending events between scope trees.    
    * This directive will listen for an eventRelayEvent and will broadcast
    * the event data to any child scopes.
    * The event data is expected to be an object with the following properties:
    *   - eventName {string} - The name of the relayed event. Mandatory.
    *   - eventData {object} - The data of the relayed event. Optional
    *   - allowPropagation {bool} - True to propagate the eventRelayEvent up the tree. Optional.
    */
    angular.module('mod.common.spEventRelay', [])
        .directive('spEventRelay', function() {
            return {
                restrict: 'A',
                link: function(scope, iElement, iAttrs) {
                    scope.$on('eventRelayEvent', function (event, data) {
                        if (!data || !data.allowPropagation) {
                            event.stopPropagation();
                        }                        

                        if (data &&
                            data.eventName &&
                            data.eventName !== 'eventRelayEvent') {
                            scope.$broadcast(data.eventName, data.eventData);
                        }
                    });
                }
            };
        });
}());