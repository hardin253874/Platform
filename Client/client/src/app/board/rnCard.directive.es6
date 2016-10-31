// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, rnBoard, showdown */

(function () {
    'use strict';
        

    angular.module('mod.app.board').directive('rnBoardCard', rnCardDirective);

    // todo - move to somewhere more common
    angular.module('mod.app.board').filter('toTrustedHtml', ['$sce', function ($sce) {
        return function (text) {
            return $sce.trustAsHtml(text);
        };
    }]);

    /* @ngInject */
    function rnCardDirective($compile,$filter) {

        // Notes
        // We use "card" in the templates and any angular expressions for the controllers' this
        // due to the controllerAs syntax.
        // We use "vm" in code for the controller object's this due to the simple assignment.
        // We use different names to highlight they are different references even if to the same object.

        return {
            scope: {
                board: '=',
                item: '='
            },
            link: link,
            controller: CardViewController,
            controllerAs: 'card',
            bindToController: true,
            // the following is the default template IF NOT DEFINED on the board - see the link function below
            template: `<div>
            <div class="card-id" style="background-color: {{card.colour}}"><a href="" ng-click="card.executeAction(card.item)">{{card.id}}</a></div>
            <div class="card-title" title="{{card.descr}}">{{card.title}}</div>
            <div class="card-field" ng-repeat="f in card.fields">
              <label>{{f.title}}</label>
              <div>{{f.value}}</div>
              <img ng-src="{{f.imageUri}}" ng-show="f.imageUri">
            </div>
            </div>`
        };

        function link(scope, elem) {
            var tmpl = sp.result(scope, 'card.board.cardTemplate');
            if (tmpl) {
                var html;
                try {
                    html = $compile('<div>' + tmpl + '</div>')(scope);
                } catch (e) {
                    html = '<div>template error</div>';
                }
                elem.html(html);
            }
        }

        /* @ngInject */
        function CardViewController($scope, spWebService, rnBoardService) {

            let vm = this;
            let {getItemValue, getItemValueId} = rnBoardService;

            vm.editHref = vm.board.viewRecordHref(vm.item.eid);
            //vm.debugString = JSON.stringify(_.map(_.range(vm.board.model.cols.length), _.partial(getItemValue, vm.item)));

            // template helpers
            vm.getInitials = s => s && s.replace(/[^A-Z]/g, '') || '??';
            vm.leftWithElipses = (s, n) => s.length < n ? s : s.substring(0, n) + '...';
            vm.drilldown = item => vm.board && vm.board.drilldown(item);
            vm.executeAction = item => vm.board && vm.board.executeAction(item);
            vm.getLegendStyle = getLegendStyle;

            $scope.$watch('card.board.model.meta', metaChanged);
            $scope.$watch('card.board.model.cols', columnsChanged);
            $scope.$watch('card.board.model.data', dataChanged);
            $scope.$watch('card.board.model.styleDimension', updateStyle);
            $scope.$watch('card.board.model.styleDimension.values', updateStyle);

            function metaChanged(meta) {
                //console.log('meta', meta);
            }


            function columnsChanged(cols) {
                updateCardFields();
                updateChildItems();
            }

            function dataChanged(data) {
                updateCardFields();
            }

            function updateChildItems() {
                var childResults = sp.result(vm.board, 'model.childReportResults');
                var childReportCols = sp.result(vm.board, 'model.childReportCols');
                var parentCol = sp.result(vm.board, 'model.childReportParentCol');

                if (!childResults || !parentCol) return;

                vm.childCards = _(childResults.gdata)
                    .filter(r => getItemValueId(r, parentCol.ord) === vm.item.eid)
                    .map(item => {
                        var fields = colsToFields(item, childReportCols);
                        return {
                            item: item,
                            fields: fields,
                            values: _.zipObject(
                                _.map(_.map(fields, 'title'), s => s.toLowerCase()),
                                _.map(fields, 'value')
                            ),
                            imageUris: _.zipObject(
                                _.map(_.map(fields, 'title'), s => s.toLowerCase()),
                                _.map(fields, 'imageUri')
                            )
                        };
                    })
                    .value();

                // todo - work out how to remove this hard coded knowledge ...
                // this is a collection of the first card we find per distinct assignee
                vm.childCardPerUniqAssignee = _(vm.childCards)
                    .reject(c => !c.values['assigned'])
                    .groupBy(c => c.values['assigned'])
                    .map(a => _.first(a))
                    .value();
            }

            //todo - should cache what we can of this at the board level
            function updateCardFields() {
                var f, cols = vm.board.model.cols;

                //console.log('cols', cols);

                // examine the cols for fields we want to display

                vm.fields = colsToFields(vm.item, cols);

                // find suitable title and description fields
                vm.title = 'name to go here';
                vm.descr = '';

                //don't default to the internal entity id
                //vm.id = vm.item.eid;

                // title
                f = findField([
                    _.partial(_.find, vm.fields, function (f) {
                        return (/(^id.*$)|(number$)/ig).test(f.col.title);
                    })
                ]);
                if (f) {
                    vm.id = getItemValue(vm.item, f.col.ord);
                    f.hideOnCard = true;
                }
                f = findField([
                    _.partial(_.find, vm.fields, function (f) {
                        return f.col.entityname;
                    }),
                    _.partial(_.find, vm.fields, function (f) {
                        return (/(^name$)|(^title$)|(^subject$)|(^summary$)/ig).test(f.col.title);
                    })
                ]);
                if (!f) {
                    f = _.first(vm.fields);
                }
                if (f) {
                    vm.title = getItemValue(vm.item, f.col.ord);
                    f.hideOnCard = true;
                }

                // description
                f = findField([
                    _.partial(_.find, vm.fields, function (f) {
                        return f.col.title === 'Description';
                    })
                ]);
                if (f) {
                    vm.descr = getItemValue(vm.item, f.col.ord);
                    f.hideOnCard = true;
                }

                // hide fields that the board's row and col dimension are based on
                _.forEach(vm.fields, function (f) {
                    if (vm.board.model.colDimension && vm.board.model.colDimension.col === f.col ||
                        vm.board.model.rowDimension && vm.board.model.rowDimension.col === f.col ||
                        vm.board.model.styleDimension && vm.board.model.styleDimension.col === f.col) {
                        f.hideOnCard = true;
                    }
                });

                // make a map of the fields, keyed by name
                // and do it before we dump the hidden fields in the fields array
                vm.values = _.zipObject(
                    _.map(_.map(vm.fields, 'title'), s => s.toLowerCase()),
                    _.map(vm.fields, 'value')
                );

                // discard those we don't want to see
                vm.fields = _.reject(vm.fields, 'hideOnCard');

//                console.log('fields', vm.fields);

                // not sure this is a great thing to do but push back the type field if one
                // onto the item
                vm.item.type = vm.values.type;

                // convert the first couple of lines of the description using markdown
                var text = _.take((vm.descr || '').split('\n'), 2).join('\n');
                vm.descrHtml = vm.descr && new showdown.Converter().makeHtml(text);

                updateStyle();
            }

            function getLegendStyle(card){
                var style = {};

                if (!card) {
                    return style;
                }

                if (card){
                    style['border-left'] = '5px solid ' + card.colour;                    
                }
               
                return style;
            }

            function colsToFields(item, cols) {
                return _(cols)
                    .map(function (col) {
                        var value = getItemValue(item, col.ord);
                        var valueId = getItemValueId(item, col.ord);
                        if (col.type === 'DateTime') {
                            //bug 27731 if the col value is empty, don't set to value to .Now()
                            if (!_.isEmpty(value))
                                value = formatDateTime(value, $filter);                           
                        }
                        if (col.type === 'Date') {                            
                            if (!_.isEmpty(value))
                                value = formatDate(value, $filter);                           
                        }
                        if (col.type === 'Time') {                            
                            if (!_.isEmpty(value))
                                value = formatTime(value, $filter);                           
                        }

                        //todo - lookup for val rules in the report meta....
                        // but for now....
                        if (col.type === 'Decimal') {
                            var f = parseFloat(value);
                            value = !_.isNaN(f) ? f.toFixed(0) : 0;
                        }
                        if (col.type === 'StructureLevels') {
                            value = rnBoardService.structureViewToString(value);
                        }
                        return {
                            col: col,
                            title: col.title,
                            //bug 27698 Board View: Extension for image name SHOULD NOT be displayed in the Board View
                            value: col.type === 'Image' && valueId ? removeExtension(value) : value,

                            imageUri: col.type === 'Image' && valueId ?
                                spWebService.getImageApiSmallUrl(valueId) :
                                undefined
                        };
                    })
                    .filter(f => f.value || f.imageUri)
                    .value();
            }

            function updateStyle() {
                var d = vm.board.model.styleDimension;
                if (d && d.values && vm.item) {
                    var itemValue = getItemValue(vm.item, d.ord);
                    var itemId = getItemValueId(vm.item, d.ord);
                    var val = _.find(d.values, v => v.rid === itemId || v.value === itemValue);
                    vm.colour = val && val.colour || 'white';
                }
            }
        }
    }

    //get the file name without extensiion
    function removeExtension(fileStr)
    {
        if (!fileStr)
            return undefined;

        return fileStr.split('.')[0];
    }

    /**
     * Returns the first field that passes any of the given filter functions.
     */
    function findField(finderFns) {
        var field = null;
        _.some(finderFns, function (fn) {
            field = fn();
            return !!field;
        });
        return field;
    }

    // quick copy paste...
    function formatDate(dateStr, filter) {
        var spDate = filter('spDate');
        return spDate(dateStr);
    }

    function formatDateTime(dateStr, filter){
       var spDateTime = filter('spDateTime');
       return spDateTime(dateStr);
    }

    function formatTime(dateStr, filter){       
        var spTime = filter('spTime');
        return spTime(dateStr); 
    }
}());
