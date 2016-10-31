/*global angular, _, sp*/

(function () {
    'use strict';

    angular.module('app')
        .controller('SurvivorsController', function ($scope, $interval) {
            this.number = 50000;
            $interval(() => {
                this.number -= random(10);
            }, random(30000));
        })
    .controller('DaysSince', function ($scope, $timeout, $interval) {
        this.number = 147;
        $timeout(() => {
            this.number = 0;
            $interval(() => {
                this.number += 1;
            }, 30000);
        }, random(120000));
    });

    function random(max) {
        return Math.floor(Math.random() * max + 1);
    }

}());