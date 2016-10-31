// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('app.editForm.mapControl', ['mod.app.editForm'])
    .controller('mapControl',

function ($scope) {
    'use strict';
    
    function toQueryFragment(fieldName) {
        var val = $scope.formData.getField(fieldName);
        if (val) {
            return '+' + val;
        } else {
            return '';
        }

    }    

    function getDebounceWait () {
        if ($scope.formMode === 'view')
            return 0;
        else {
            return 2000;
        }
    }
    
    function buildAddressString() {
        if ($scope.formData) {
            console.log('formData is being changed');
            //HACK! There will be a better way of doing this
            $scope.addressString = toQueryFragment('shared:addressLine1') +
                toQueryFragment('shared:addressLine2') +
                toQueryFragment('shared:addressLine3') +
                toQueryFragment('shared:city') +
                toQueryFragment('shared:region') +
                toQueryFragment('shared:country') +
                toQueryFragment('shared:code');
        } else {
            $scope.addressString = '';
        }
    }
 
    $scope.addressString = '';

    
    $scope.$watch('formData',
        function () {
            buildAddressString();
        });
    
    $scope.$watch('formData.getField(\'shared:addressLine1\')', _.debounce(
        function () {
            buildAddressString();
        }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:addressLine2\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:addressLine3\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:city\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:region\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:country\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
    $scope.$watch('formData.getField(\'shared:code\')', _.debounce(
     function () {
         buildAddressString();
     }, getDebounceWait()));
})
.directive('mapFrame', function () {
    return {
        restrict: 'E',
        replace: true,
        transclude: true,
        template: '<iframe class="map-iframe"></iframe>',
        link: function (scope, element, attrs) {
            attrs.$observe('address', function () {
                console.log('location address is being changed');
                var address = attrs.address;
                console.log(' type:', address);
                var iframesrc = 'https://maps.google.com.au/maps?key=AIzaSyB-JuC7SCa84rVrYmr1ODUl_Dstkuqjs3U&q=' + address + '&output=embed';
                element.attr('src', iframesrc);
            });
            
        }
    };
});