// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|mapControl', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.mapControl'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyControlEntity;

            controller = $controller('mapControl', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
        it('address string should pass to the Iframe', inject(function ($rootScope, $compile) {
            $scope = $rootScope;
            $scope.addressString = '2 edgewood drive, Stanhope gardens';
            var element;
            element = angular.element('<map-frame address ="{{addressString}}"></map-frame>');
            $compile(element)($scope);
            $scope.$digest();
            console.log(element.attr('src'));
            expect(element.attr('src')).toEqual('https://maps.google.com.au/maps?key=AIzaSyB-JuC7SCa84rVrYmr1ODUl_Dstkuqjs3U&q=2 edgewood drive, Stanhope gardens&output=embed');
        }));
    });
});