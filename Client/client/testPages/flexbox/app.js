/*global angular, _, sp*/

(function () {
    'use strict';

    angular.module('app', ['ngAnimate', 'testPagesAppHelper', 'mod.common.spLocalStorage'])
        .run(function (testPagesAppHelper) {
            // add your app and component templates here...
            // this is needed to get the paths correct since we aren't using the build system
            //for example:
            //testPagesAppHelper.appTemplate('src/app/controls/spSearchControl/spSearchControl.tpl.html');
        })
        .factory('appState', function () {
            return {};
        })
        .directive('appLayout', appLayoutDirective);

    // .directive('rnLayoutContainer', rnLayoutContainer);

    // function rnLayoutContainer() {
    //     return {
    //         restrict: 'A',
    //         link
    //     };
    //     function link(scope, element, attrs) {
    //
    //         scope.$watch('item.style', () => {
    //             let style = sp.result(scope, 'item.style');
    //             let styles = style && style.replace(/\n/g, '').split(';');
    //             _.forEach(styles, s => {
    //                 let [k,v] = _.map(s.split(':'), s => _.trim(s));
    //                 element.css(k, v);
    //             });
    //         });
    //     }
    // }

    function appLayoutDirective(appState, spLocalStorage) {
        return {
            restrict: 'E',
            transclude: true,
            replace: true,
            template: '<div class="app-layout" ng-transclude></div>',
            link: link
        };

        function link(scope) {
            scope.onClick = function () {
                console.log('hello', appState);
            };
            scope.toggleNav = function () {
                appState.showNav = !appState.showNav;
            };
            scope.appState = appState;
            appState.showNav = true;

            scope.itemTemplates = ['chart', 'report', 'form', 'hero', 'hero2', 'action'];
            scope.layoutItems = [];

            scope.getTemplate = item => `templates/${item.template}.tpl.html`;
            scope.clear = () => {
                scope.item = initRootItem();
                scope.selectedItem = scope.item;
                saveState();
            };
            scope.addItem = t => {
                let item = {
                    name: getNewName(t),
                    style: getDefaultStyle(t),
                    depth: 0,
                    items: [],
                    template: t
                };
                scope.selectedItem = scope.selectedItem || scope.item;
                let container = scope.selectedItem.template === 'container' ?
                    scope.selectedItem :
                    getItemContainer(scope.selectedItem);
                container.items = container.items.concat([item]);
                item.depth = container.depth + 1;
                scope.selectedItem = item;
                saveState();
            };
            scope.selectItem = item => {
                scope.selectedItem = item;
            };
            scope.setStyle = (item, s) => {
                item.style = _.without(item.style.split('\n'), s).concat([s]).join(';\n').replace(/;;/g, ';');
            };
            scope.getStyle = item => {
                let styles = item.style && item.style.replace(/\n/g, '').split(';');
                var styleJson = {};
                _.forEach(styles, s => {
                    let [k,v] = _.map(s.split(':'), s => _.trim(s));
                    styleJson[k] = v;
                });
                return styleJson;
            };
            scope.selectContainer = item => {
                scope.selectedItem = getItemContainer(scope.selectedItem);
            };
            scope.getFlattenedItems = flattenItems;
            scope.deleteItem = item => {
                let container = getItemContainer(item);
                if (container) {
                    container.items = _.without(container.items, item);
                    scope.selectedItem = container;
                    saveState();
                }
            };

            restoreState();

            //-----------------------------------------------------------------

            function getItemContainer(item) {
                let items = flattenItems();
                return _.find(items, _.partial(isImmediateContainerOf, item));
            }

            function isImmediateContainerOf(item, container) {
                return _.indexOf(container.items, item) >= 0;
            }

            function getNewName(t) {
                let items = flattenItems();
                let existing = _.filter(items, x => x.template === t || !x.template && t === 'container');
                return `${t} - ${existing.length}`;
            }

            function flattenItems(item = scope.item) {
                return _.flatten([item].concat(_.map(item.items, flattenItems)));
            }

            function getDefaultStyle(t) {
                let styles = [];
                let colours = ['#E85D11', '#B71C1C', '#E91E63', '#861D9C', '#3F51B5', '#2196F3', '#00B289', '#1B5E20', '#12BB2E', '#607D8B'];
                let colour = colours[Math.floor((Math.random() * 1000)) % colours.length];
                //styles.push(`background-color: ${colour}`);
                if (t === 'container') {
                    styles.push('display: flex');
                    styles.push('flex-direction: row');
                    styles.push('flex-wrap: nowrap');
                    styles.push('justify-content: center');
                    styles.push('flex-grow: 1');
                    styles.push('flex-shrink: 1');
                    styles.push('flex-basis: 0');
                    styles.push('min-width: 100px');
                } else {
                    styles.push('flex-grow: 1');
                    styles.push('flex-shrink: 1');
                    styles.push('flex-basis: auto');
                }
                return styles.join(';\n').replace(/;;/g, ';');
            }

            function saveState() {
                let item = angular.toJson(scope.item); // angular function strips its special props like $$hashKey
                spLocalStorage.setItem("item", item);
            }

            function restoreState() {
                let savedItemJson = spLocalStorage.getItem("item");
                scope.item = savedItemJson && JSON.parse(savedItemJson) || initRootItem();
            }

            function initRootItem() {
                return {
                    name: 'root',
                    template: 'container',
                    items: [],
                    style: getDefaultStyle('container'),
                    depth: 0
                };
            }

        }
    }

}());