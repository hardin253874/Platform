// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mockedAngular', ['ng']);

// Mock $stateParams
angular.module('mockedAngular').factory('$stateParams', function () {
    var exports = {};

    return exports;
});


// Mock $state
angular.module('mockedAngular').factory('$state', function () {
    var exports = {};

    return exports;
});
