// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* jshint bitwise:false */

angular.module('mod.common.spUuidService', [])
    .factory('spUuidService', function () {
        "use strict";

        var svc = {
            /////
            // Create a new UUID.
            /////
            create: function() {

                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
                    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            },

            /////
            // Empty UUID.
            /////
            empty: function() {
                return '00000000-0000-0000-0000-000000000000';
            }
        };

        return svc;
    });