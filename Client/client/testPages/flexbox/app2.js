/*global angular, _*/

(function () {
    'use strict';

    angular.module('app', []);

    angular.module('app')
        .component('myFlexApp', {
            templateUrl: 'app2.tpl.html',
            controller: function () {
            }
        })
        .component('app2Content', {
            templateUrl: 'app2-content.tpl.html',
            controller: function () {
            }
        })
        .component('app2Screen1', {
            templateUrl: 'app2-screen1.tpl.html',
            controller: function () {
            }
        })
        .component('app2Screen2', {
            templateUrl: 'app2-screen2.tpl.html',
            controller: function () {
            }
        })
        .component('app2Report', {
            bindings: { rowCount: '<'},
            templateUrl: 'app2-report.tpl.html',
            controller: function ($element, reportService) {
                this.data = reportService.getReportData(this.rowCount || 999);
                this.headers = _.keys(_.first(this.data));
                this.style = {
                    'width': '100%',
                    'min-width': (this.headers.length * 5) + 'em',
                    'min-height': Math.min(this.data.length + 4, 12) + 'em',
                    'max-height': '12em',
                    'overflow': 'auto'
                };
                $element.css(this.style);
            }
        })
        .component('app2Form1', {
            templateUrl: 'app2-form1.tpl.html',
            controller: function () {
            }
        })
        .component('app2Form2', {
            templateUrl: 'app2-form2.tpl.html',
            controller: function () {
            }
        })
        .component('field1', {
            templateUrl: 'field1.tpl.html',
            controller: function () {
                this.tempId = _.uniqueId('field1-');
                this.label = 'some label';
                this.value = 'some value';
            }
        })
        .component('field2', {
            templateUrl: 'field2.tpl.html',
            controller: function () {
                this.tempId = _.uniqueId('field2-');
                this.label = 'some label';
                this.value = 'some value';
            }
        })
        .component('vertContainer', {
            templateUrl: 'vert-container.tpl.html',
            controller: function () {
            }
        })
        .component('horzContainer', {
            templateUrl: 'horz-container.tpl.html',
            controller: function () {
            }
        })
        .component('hero', {
            templateUrl: 'hero.tpl.html',
            controller: function () {
            }
        })
        .component('app2Nav', {
            templateUrl: 'app2-nav.tpl.html',
            controller: function () {
                this.links = _.map(_.range(100), x => 'link ' + x);
                this.style = {
                    'background-color': getRandomColour()
                };
            }
        })
        .component('app2Aside', {
            templateUrl: 'app2-aside.tpl.html',
            controller: function () {
            }
        })
        .service('reportService', reportService)
        .directive('randomBgc', function () {
            return {
                controller: function ($element) {
                    $element.css('background-color', getRandomColour());
                }
            };
        })
        .directive('watchDimensions', function () {
            return {
                controller: function ($scope, $element, $attrs) {
                    // this has problems... needs digests to be run to work
                    // to come back to
                    $scope.$watch(() => {
                        let width = $element[0].offsetWidth;
                        console.log('watch width:', width);
                        return width;
                    }, width => {
                        console.log('width changed:', width);
                    });
                }
            };
        });

    function reportService() {
        return {
            getReportData: n => {
                let data = [
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 10, dob: '2006-Jan-10'},
                    {name: 'lil', age: 7, dob: '2008-Sep-30'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'},
                    {name: 'joe', age: 20, dob: '2006-Jan-10'}
                ];
                return _.take(data, n);
            }
        };
    }

    function getRandomColour() {
        return '#' + rc(192) + rc(192) + rc(192);

        // returns a random colour between the given bounds, inclusive
        function rc(f = 0, t = 255) {
            return ('0' + Math.floor(Math.random() * (t - f + 1) + f).toString(16)).substr(-2);
        }
    }

}());
