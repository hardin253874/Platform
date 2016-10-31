// Copyright 2011-2015 Global Software Innovation Pty Ltd
/*global _, CodeMirror */

(function () {
    'use strict';

    angular.module('sp.common.ui.spExpressionSheet', ['mod.common.spCachingCompile', 'mod.common.spEntityService', 'sp.common.spCalcEngineService'])
        .directive('spExpressionSheet', function ($q, $parse, $window, $document, $timeout, spCachingCompile, spCalcEngineService) {
            return {
                restrict: 'E',
                replace: false,
                transclude: false,
                scope: {
                    options: "=?"
                },
                link: function (scope, element) {

                    var win;
                    var scroller;
                    var colheaderscroller;
                    var rowheaderscroller;
                    var minItemHeight = 30;
                    var minItemWidth = 100;
                    var vBuffer = 30;
                    var hBuffer = 20;
                    var vOffset = 0;
                    var hOffset = 0;
                    var rowStart = 0;
                    var colStart = 0;
                    var rowEnd = vBuffer;
                    var colEnd = hBuffer;
                    var rows = _.range(1, 300);
                    var columns = _.range(1, 200);
                    var height = rows.length * minItemHeight;
                    var width = columns.length * minItemWidth;
                    var az = ' ABCDEFGHIJKLMNOPQRSTUVWXYZ';
                    var codeMirror;
                    var hints = {};

                    scope.model = {
                        editorOptions: {
                            mode: 'text/x-spql',
                            lineNumbers: true,
                            lineWrapping: false,
                            matchBrackets: true,
                            autofocus: false,
                            readOnly: false,
                            dragDrop: false
                        },
                        busyIndicator: {
                            type: 'spinner',
                            text: 'Loading...',
                            placement: 'element',
                            isBusy: true
                        },
                        init: init,
                        getStyle: getStyle,
                        getColumnStyle: getColumnStyle,
                        getColumnText: getColumnText,
                        getRowStyle: getRowStyle,
                        visibleRows: rows.slice(rowStart, rowEnd),
                        visibleCols: columns.slice(colStart, colEnd),
                        parameters: [],
                        cells: {},
                        selectedCell: null,
                        select: select,
                        preselect: preselect,
                        dragOptions: {
                            dragImage: {
                                url: getDragImage(),
                                width: 100,
                                height: 30
                            },
                            onDragStart: onDragStart,
                            onDragEnd: onDragEnd
                        },
                        dropOptions: {
                            onAllowDrop: onAllowDrop,
                            onDrop: onDrop
                        }
                    };

                    var onSelectedExpressionChangedDebounce = function () {
                        calculate(scope.model.selectedCell);
                    };
                    var onSelectedExpressionChanged = _.debounce(onSelectedExpressionChangedDebounce, 100);
                    scope.$watch('model.selectedCell.expression', onSelectedExpressionChanged);
                    scope.$watch('options', load);

                    function load() {
                        scope.model.busyIndicator.isBusy = true;
                        scope.model.cells = {};
                        scope.model.parameters.length = 0;

                        if (!scope.options) {
                            scope.model.busyIndicator.isBusy = false;
                            return $q.when();
                        }

                        return $q.when().then(function () {

                            var cellA1 = {
                                row: 1,
                                col: 1,
                                expression: 'let y = abs(42) select y',
                                result: { value: null }
                            };

                            var cellB2 = {
                                row: 2,
                                col: 2,
                                expression: '[C:1].[Name]',
                                result: { value: null }
                            };

                            var cellC1 = {
                                row: 1,
                                col: 3,
                                expression: '[Application survey]',
                                result: { value: null }
                            };
                            
                            _.set(scope.model.cells, '1.1', cellA1);
                            _.set(scope.model.cells, '2.2', cellB2);
                            _.set(scope.model.cells, '1.3', cellC1);

                            scope.model.parameters = _.map(sp.result(scope.options, 'inputs'), function (e) {
                                return {
                                    name: e.name,
                                    type: 'Entity',
                                    typeName: 'Entity',
                                    isList: false,
                                    entityTypeId: '' + sp.result(e, 'isOfType.0.idP'),
                                    value: '' + e.idP
                                };
                            });

                        }).then(recalculateAll).finally(function () {

                            scope.model.busyIndicator.isBusy = false;
                            select(1, 1);

                        });
                    }

                    function getDragImage(str) {
                        //return 'assets/images/colorpicker.png';
                        var canvas = document.createElement('canvas');
                        canvas.width = 100;
                        canvas.height = 30;
                        var context = canvas.getContext('2d');
                        context.fillStyle = '#31a89f';
                        context.fillRect(0, 0, 100, 30);
                        if (str && str.length) {
                            context.font = '14px Arial';
                            context.textAlign = 'center';
                            context.textBaseline = 'middle';
                            context.fillStyle = '#ffffff';
                            context.fillText(str, 50, 15);
                        }
                        return canvas.toDataURL('image/png');
                    }

                    function getCells() {
                        return _.chain(scope.model.cells)
                            .map()
                            .flatten()
                            .compact()
                            .valueOf();
                    }

                    function getCell(row, col) {
                        var cell = _.get(scope.model.cells, row + '.' + col);

                        if (!cell) {
                            cell = {
                                row: row,
                                col: col,
                                expression: '',
                                error: '',
                                result: {
                                    resultType: 'String',
                                    value: ''
                                }
                            };

                            setCell(row, col, cell);
                        }

                        return cell;
                    }
                    
                    function setCell(row, col, cell) {
                        if (row, col, cell) {
                            _.set(scope.model.cells, row + '.' + col, cell);
                        }
                    }

                    function select(row, col) {
                        var selected = sp.result(scope.model, 'selectedCell');
                        if (selected) {
                            delete selected.selected;
                        }

                        var cell = getCell(row, col);
                        scope.model.selectedCell = cell;
                        scope.model.selectedCell.selected = true;
                    }

                    function preselect(row, col) {
                        var str = getColumnText(col) + ':' + row;
                        var dragImage = sp.result(scope.model, 'dragOptions.dragImage.element');
                        if (dragImage && dragImage.src) {
                            safeApply(scope, function () {
                                dragImage.src = getDragImage(str);
                            });
                        }
                    }
                    
                    var calculateTimeouts = [];
                    function calculate(cell) {
                        if (cell && cell.row && cell.col) {
                            removeCalculate(cell.row, cell.col, true);

                            var calculateCell = _.partial(calculateResult, cell);

                            calculateTimeouts.push({
                                row: cell.row,
                                col: cell.col,
                                fn: $timeout(calculateCell, 500)
                            });
                        }
                    }

                    function removeCalculate(row, col, cancel) {
                        if (calculateTimeouts && calculateTimeouts.length && row && col) {
                            var timeout = _.find(calculateTimeouts, { 'row': row, 'col': col });
                            if (timeout) {
                                if ((cancel === true) && timeout.fn) {
                                    $timeout.cancel(timeout.fn);
                                }
                                var idx = calculateTimeouts.indexOf(timeout);
                                if (idx > -1) {
                                    calculateTimeouts.splice(idx, 1);
                                }
                            }
                        }
                    }

                    function cancelCalculations() {
                        if (calculateTimeouts && calculateTimeouts.length) {
                            _.each(calculateTimeouts, function(timeout) {
                                if (timeout.fn) {
                                    $timeout.cancel(timeout.fn);
                                }
                            });
                            calculateTimeouts.length = 0;
                        }
                    }

                    function recalculateAll() {
                        // todo: execution order by dependencies and recursion guarding
                        return $q.all(_.map(getCells(), calculate));
                    }

                    function isExpression(str) {
                        return str.indexOf('[') === 0;
                    }

                    function isCellName(str) {
                        return str.indexOf(':') > 0;
                    }

                    function isParameterName(str) {
                        return _.some(scope.model.parameters, function(p) {
                            return p.name === str;
                        });
                    }

                    function getCellParameter(paramName) {
                        var relatedCell = getCellFromString(paramName);

                        if (relatedCell && relatedCell.result && relatedCell.result.resultType) {
                            var t = relatedCell.result.resultType;

                            if (relatedCell.result.error) {
                                throw new Error('Required cell ' + paramName + ' has an error.');
                            }
                            
                            return {
                                name: paramName,
                                type: t,
                                typeName: t,
                                isList: relatedCell.result.isList,
                                entityTypeId: '' + relatedCell.result.entityTypeId,
                                value: relatedCell.result.value || ''
                            };
                        }

                        return null;
                    }

                    function getParameters(cell, paramNames) {
                        var params = [];
                        
                        try {
                            _.each(paramNames, function (p) {
                                if (isCellName(p)) {
                                    // it's a cell parameter
                                    var cellParameter = getCellParameter(p);
                                    if (cellParameter) {
                                        params.push(cellParameter);
                                        return;
                                    }
                                }

                                // todo: make inputs and records and parameters distinct
                                if (isParameterName(p)) {
                                    // it's a parameter
                                    var param = _.find(scope.model.parameters, function (parameter) {
                                        return p === parameter.name;
                                    });

                                    if (param && param.entityTypeId) {
                                        params.push(param);
                                        return;
                                    }
                                }
                            });
                        } catch (e) {
                            cell.error = e;
                        }

                        return params;
                    }

                    function getParameterNamesInExpression(str) {
                        var paramNames = [];

                        if (str && str.length && (str.indexOf('[') > -1) && (str.indexOf(']') > -1)) {
                            paramNames = _.compact(str.split(/\][^\[\]]*|[^\[\]]*\[/));
                        }

                        return paramNames;
                    }

                    function getCellFromString(str) {
                        var cell = null;

                        if (str && str.length && (str.indexOf(':') === str.lastIndexOf(':'))) {
                            var lookup = _.compact(str.split(':'));
                            if (lookup && lookup.length === 2) {
                                var col = getColumnIndex(lookup[0]);
                                var row = parseInt(lookup[1]);
                                if (col && row) {
                                    cell = getCell(row, col);
                                }
                            }
                        }

                        return cell;
                    }

                    function calculateResult(cell) {
                        if (cell) {
                            delete cell.error;
                            delete cell.result;
                            
                            if (cell.expression && cell.expression.length) {
                                var context = (sp.result(scope.options, 'context') || 'core:definition');
                                var paramNames = getParameterNamesInExpression(cell.expression);
                                var params = getParameters(cell, paramNames);

                                if (!cell.error) {
                                    spCalcEngineService.evalExpression(cell.expression, context, null, params).then(function (result) {
                                        cell.result = result.data;
                                        removeCalculate(cell.row, cell.col);
                                    });
                                }
                            }
                        }
                    }
                    
                    function onDragStart(event, data) {
                        var evt = event.originalEvent || event;
                        if (evt && evt.currentTarget) {

                        }
                        evt.stopPropagation();
                    }

                    function onDragEnd(event, data) {
                        
                    }

                    function onAllowDrop(source, target, dragData, dropData) {
                        var sourceIsCell = isCellElement(source[0]);
                        var targetIsEditor = isEditorElement(target);

                        return sourceIsCell === true && targetIsEditor === true;

                        //return false;
                    }

                    function onDrop(event, source, target, dragData, dropData) {
                        var evt = event.originalEvent || event;

                        if (!onAllowDrop(source, target, dragData, dropData)) {
                            return true;
                        }

                        try {
                            var sourceIsCell = isCellElement(source[0]);
                            var targetIsEditor = isEditorElement(target);
                            if (targetIsEditor === true) {
                                var txt = '';
                                if (sourceIsCell === true) {
                                    txt = '[' + getColumnText(dragData[1]) + ':' + dragData[0] + ']';
                                }
                                if (codeMirror) {
                                    var pos = getEditorPosition(evt);
                                    var line = pos.line;
                                    var ch = pos.ch;
                                    _.each(codeMirror.listSelections(), function (s) {
                                        // replace any selection we are over
                                        if ((s.anchor !== s.head) &&
                                            ((s.anchor.line < pos.line && pos.line < s.head.line) ||
                                             (s.anchor.line === s.head.line && s.anchor.line === pos.line && s.anchor.ch <= pos.ch && s.head.ch >= pos.ch) ||
                                             (s.anchor.line < s.head.line && s.anchor.line === pos.line && s.anchor.ch <= pos.ch) ||
                                             (s.anchor.line < s.head.line && s.head.line === pos.line && s.head.ch >= pos.ch))) {
                                            codeMirror.setSelection(s.anchor, s.head);
                                            codeMirror.replaceSelection("", "end", "drag");
                                            line = s.anchor.line;
                                            ch = s.anchor.ch;
                                            return false;
                                        }
                                        return true;
                                    });
                                    codeMirror.setCursor(line, ch);
                                    codeMirror.replaceSelection(txt, "end", "paste");
                                    codeMirror.display.input.focus();
                                }
                            }
                        } finally {
                            evt.stopPropagation();
                            return false;
                        }
                    }
                    
                    function isCellElement(el) {
                        if (!el || !el.classList) {
                            return false;
                        }
                        return el.classList.contains('ex-cell-container');
                    }

                    function isEditorElement(el) {
                        if (!el || !el.classList) {
                            return false;
                        }
                        return el.classList.contains('ex-cell-editor');
                    }

                    function getEditorPosition(event) {
                        var coords = null;
                        if (codeMirror && event) {
                            var space = codeMirror.display.lineSpace.getBoundingClientRect();
                            var x = event.clientX - space.left;
                            var y = event.clientY - space.top;

                            coords = codeMirror.coordsChar({left: x, top: y}, "div");
                        }
                        return coords;
                    }

                    function getEntityTypeIdFromText(search) {
                        var entityTypeId = 0;

                        if (isExpression(search)) {
                            var obj = {};
                            var context = (sp.result(scope.options, 'context') || 'core:definition');
                            var paramNames = getParameterNamesInExpression(search);
                            var params = getParameters(obj, paramNames);

                            return spCalcEngineService.compile(search, { context: context, params: params }).then(function(result) {
                                return sp.result(result, 'entityTypeId');
                            });
                        }

                        if (isCellName(search)) {
                            var cell = getCellFromString(search);
                            entityTypeId = sp.result(cell, 'result.entityTypeId');
                        }

                        if (!entityTypeId && isParameterName(search)) {
                            var param = _.find(scope.model.parameters, function (p) {
                                return p.name === search;
                            });
                            entityTypeId = sp.result(param, 'entityTypeId');
                        }

                        return $q.when().then(function () { return entityTypeId; });
                    }

                    function getEntityTypeHints(entityTypeId) {
                        var cached = _.get(hints, entityTypeId);

                        if (entityTypeId && !cached) {
                            return spCalcEngineService.getEntityTypeHints(entityTypeId).then(function (results) {
                                _.set(hints, entityTypeId, results);
                                return results;
                            });
                        }

                        return $q.when().then(function() {
                            return cached;
                        });
                    }
                    
                    function getHints(cm, options) {
                        var filtered = _.filter(sp.result(options, 'found') || [], function(f) {
                            return f.toLowerCase().indexOf(options.filter) === 0;
                        });
                        var hintObj = {
                            from: options.from,
                            to: options.to,
                            list: _.map(filtered, function (f) {
                                return {
                                    text: '[' + f + ']',
                                    displayText: f
                                };
                            })
                        };
                        return hintObj;
                    }

                    function autoComplete(search, filter, from, to) {
                        console.log('autocomplete: ' + search + ' (' + filter + ')');

                        getEntityTypeIdFromText(search)
                            .then(getEntityTypeHints)
                            .then(function(foundHints) {
                                CodeMirror.showHint(codeMirror, getHints, {
                                    found: foundHints,
                                    filter: filter || '',
                                    from: from,
                                    to: to
                                });
                            });
                    }

                    function onChanged(cm, changes) {
                        if (cm !== codeMirror || !codeMirror) {
                            return;
                        }

                        if (codeMirror.hasFocus() && (changes.origin === '+input' || changes.origin === '+delete')) {
                            var pos = codeMirror.getCursor();
                            var token = codeMirror.getTokenAt(pos);
                            var line = codeMirror.getLine(pos.line);
                            var activeDot;
                            var precedingActiveDot = '';
                            var followingActiveDot = '';
                            var expr = '';
                            var filter = '';
                            
                            if (token.string === '.') {
                                activeDot = token.start;
                            } else {
                                activeDot = line.substring(0, pos.ch).lastIndexOf('.');
                            }

                            if (!activeDot) {
                                return;
                            }

                            // what comes before?
                            precedingActiveDot = line.substring(0, activeDot);
                            if (precedingActiveDot.length && _.endsWith(precedingActiveDot, ']')) {
                                expr = _.last(_.compact(precedingActiveDot.substring(0, precedingActiveDot.length - 1).split(/^\[|\s\[/)));
                                if (expr.search(/\]./) > 0) {
                                    expr = '[' + expr + ']';
                                }
                            }

                            if (!expr || !expr.length || expr === '[]') {
                                return;
                            }

                            // what comes after?
                            if (activeDot < line.length) {
                                followingActiveDot = line.substring(activeDot + 1, pos.ch);

                                if (followingActiveDot.length && followingActiveDot.search(/[^a-zA-Z0-9_\-]/) === 0) {
                                    return;
                                }

                                if (followingActiveDot.length)
                                {
                                    filter = followingActiveDot.substring(0, followingActiveDot.search(/[^a-zA-Z0-9_\-]|$/));
                                }
                            }

                            var start = { line: pos.line, ch: activeDot + 1 };
                            var end = { line: pos.line, ch: activeDot + 1 + filter.length };
                            autoComplete(expr, filter, start, end);
                        }
                    }

                    function safeApply(s, fn) {
                        if (!s.$root.$$phase) { s.$apply(fn); } else { fn(); }
                    }

                    function getStyle() {
                        var style = {};
                        style['width'] = width + 'px';
                        style['height'] = height + 'px';
                        style['padding-top'] = vOffset + 'px';
                        style['padding-left'] = hOffset + 'px';
                        return style;
                    }

                    function getColumnStyle(col, isHeader) {
                        var style = {};

                        if (col) {
                            style['width'] = minItemWidth + 'px';

                            if (scope.model.selectedCell && (col === scope.model.selectedCell.col)) {
                                style['font-weight'] = 'bolder';
                                style['color'] = '#31a89f';
                                style['border-bottom'] = 'solid 2px #31a89f';
                            }
                        }

                        return style;
                    }

                    function getColumnText(col) {
                        var str = '';

                        if (col) {
                            var j = col % 26;
                            var i = _.floor(col / 26);
                            if (!j) {
                                i--;
                                j = 26;
                            }
                            str = az[i] + az[j];
                        }
                        
                        return _.trim(str);
                    }

                    function getColumnIndex(str) {
                        var col = 0;

                        if (str && str.length && (str.length < 3)) {
                            var i = 0;
                            var j;

                            if (str.length > 1) {
                                j = az.indexOf(str[1]);
                                i = az.indexOf(str[0]);
                            } else {
                                j = az.indexOf(str[0]);
                            }

                            col = (i * 26) + j;
                        }

                        return col;
                    }

                    function getRowStyle(row) {
                        var style = {};

                        if (row) {
                            style['height'] = minItemHeight + 'px';

                            if (scope.model.selectedCell && (row === scope.model.selectedCell.row)) {
                                style['font-weight'] = 'bolder';
                                style['color'] = '#31a89f';
                                style['border-right'] = 'solid 2px #31a89f';
                            }
                        }

                        return style;
                    }

                    function setPositionY(pos) {
                        // how many items will definitely fill the window?
                        vBuffer = Math.round(scroller[0].clientHeight / minItemHeight) + 2;

                        vOffset = pos;

                        // relate the percentage scroll to the full list
                        var pV = vOffset / (scroller[0].scrollHeight - scroller[0].offsetHeight);

                        pV = pV > 1 ? 1 : pV < 0 ? 0 : pV;

                        rowStart = parseInt(pV * (rows.length - vBuffer));
                        rowEnd = rowStart + vBuffer;

                        if (rowEnd > rows.length) {
                            rowEnd = rows.length;
                            rowStart = rowEnd - vBuffer;
                        }

                        var visibleRowHeight = (rowEnd - rowStart) * minItemHeight;

                        var vOffsetMax = height - visibleRowHeight;

                        rowheaderscroller[0].scrollTop = Math.max(vOffset - vOffsetMax, 0);

                        vOffset = vOffset > vOffsetMax ? vOffsetMax : vOffset < 0 ? 0 : vOffset;
                    }

                    function setPositionX(pos) {
                        hBuffer = Math.round(scroller[0].clientWidth / minItemWidth) + 2;

                        hOffset = pos;

                        var pH = hOffset / (scroller[0].scrollWidth - scroller[0].offsetWidth);

                        pH = pH > 1 ? 1 : pH < 0 ? 0 : pH;

                        var colId = parseInt(pH * (columns.length - hBuffer));
                        colStart = colId;
                        colEnd = colId + hBuffer;

                        if (colEnd > columns.length) {
                            colEnd = columns.length;
                            colStart = colEnd - hBuffer;
                        }

                        var visibleColumnWidth = (colEnd - colStart) * minItemWidth;

                        var hOffsetMax = width - visibleColumnWidth;

                        colheaderscroller[0].scrollLeft = Math.max(hOffset - hOffsetMax, 0);

                        hOffset = hOffset > hOffsetMax ? hOffsetMax : hOffset < 0 ? 0 : hOffset;
                    }

                    function onScroll() {
                        if (!scroller || !scroller[0]) {
                            return;
                        }

                        var x = scroller[0].scrollLeft;
                        var y = scroller[0].scrollTop;
                        
                        setPositionX(x);
                        setPositionY(y);

                        safeApply(scope, function () {
                            scope.model.visibleRows = rows.slice(rowStart, rowEnd);
                            scope.model.visibleCols = columns.slice(colStart, colEnd);
                        });
                    }

                    function onResize() {
                        if (!scroller || !scroller[0]) {
                            return;
                        }

                        scroller[0].scrollLeft = 0;
                        scroller[0].scrollTop = 0;
                        setPositionX(0);
                        setPositionY(0);

                        safeApply(scope, function () {
                            scope.model.visibleRows = rows.slice(rowStart, rowEnd);
                            scope.model.visibleCols = columns.slice(colStart, colEnd);

                            scope.$broadcast('sp-rebuild-watch-limits');
                        });
                    }
                    var onResizeDebounced = _.debounce(onResize, 150);

                    function setupScrolling() {
                        if (!colheaderscroller) {
                            colheaderscroller = angular.element(element.find('.ex-col-hdr-container')[0]);
                        }

                        if (!rowheaderscroller) {
                            rowheaderscroller = angular.element(element.find('.ex-row-hdr-container')[0]);
                        }

                        if (!scroller) {
                            scroller = angular.element(element.find('.ex-cells')[0]);
                            scroller.bind('scroll', onScroll);
                        }

                        if (!win) {
                            win = angular.element($window);
                            win.bind('resize', onResizeDebounced);
                        }
                    }

                    function setupEditor() {
                        $timeout(function() {
                            if (!codeMirror) {
                                var cm = element.find('.CodeMirror')[0];
                                if (cm) {
                                    codeMirror = cm.CodeMirror;
                                }
                            }
                            if (!codeMirror) {
                                setupEditor(); // repeat until hooked in
                            } else {
                                //codeMirror.on("change", onEditorChanged);
                                codeMirror.on("change", onChanged);
                            }
                        }, 200);
                    }

                    function init() {
                        $timeout(function () {
                            scope.$broadcast('sp-rebuild-watch-limits');
                        }, 300);
                    }

                    scope.$on('$destroy', function () {
                        if (win) {
                            win.unbind('resize', onResizeDebounced);
                        }

                        // cancel any timers still calculating
                        cancelCalculations();
                    });

                    var cachedLinkFunc = spCachingCompile.compile('devViews/expressionSheet/spExpressionSheet.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                        setupScrolling();
                        setupEditor();
                    });
                }
            };
        });
}());