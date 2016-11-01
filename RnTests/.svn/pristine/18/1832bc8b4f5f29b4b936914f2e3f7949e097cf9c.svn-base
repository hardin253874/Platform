(ns rt.lib.wd-rn
  (:require [rt.lib.wd :refer :all]
            [rt.lib.wd-ng :refer :all]
            [clj-webdriver.taxi :as taxi]
            [clojure.string :as string]
            [clojure.data.csv :as csv]
            [clojure.data.json :as json]
            [clojure.java.io :as io]
            [clojure.data.codec.base64 :refer [encode]]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import org.openqa.selenium.Keys
           java.text.SimpleDateFormat))

(defn set-click-to-edit-value [selector value]
  (let [view-query (str selector " div[ng-click]")
        input-query (str selector " input")]

    (wait-until-displayed view-query)
    (taxi/click view-query)

    (wait-until-displayed input-query)
    (when (not (taxi/exists? input-query))
      ;; should just fail out here...
      (error "Failed to activate click to edit input"))

    (set-input-value input-query value)
    (taxi/send-keys input-query Keys/ENTER)))

(defn navitem-isdirty? []
  (clj-webdriver.taxi/execute-script
    "var svc = angular.element('body').injector().get('spState');
     return _.result(svc.navItem, 'isDirty')"))

(defn execute-action
  "Perform a registered action function. These are client-side javascript functions in the app
  that have been made available to allow testing of functions that we cannot reliably test via webdriver
  based actions against the app."
  [action opts]
  (execute-async-script
    "console.log(arguments);document.spAppKeyhole.executeAction(arguments[0], arguments[1], arguments[2]);"
    [action opts]))

(defn- fmt-name [name] (str "\"" name "\""))

(defn drag-n-drop
  "Perform a drag and drop operation using the drop drop simulation servce that exists
  in the app itself.

  The _from_ and _to_ arguments can be selector strings or web elements."
  [from to & [opts]]
  (wait-for-angular)
  (try
    (debug "Performing drag and drop: from" (fmt-name from) (type from) "to" (fmt-name to) (type to))
    (taxi/execute-script
      "
      var from = arguments[0],
          to = arguments[1],
          opts = arguments[2],
          done = arguments[3];
      var spDragDropSimService = angular.element('body').injector().get('spDragDropSimService');
      opts = opts && JSON.parse(opts);
      console.log('simDnD:', from, to, opts, arguments);
      if (!from) throw Error('Dnd - from is undefined');
      if (!to) throw Error('Dnd - to is undefined');
      from = angular.element(from);
      to = angular.element(to);
      if (!from.length) throw Error('Dnd - no element found for from');
      if (!to.length) throw Error('Dnd - no element found for to');
      console.log('Calling spDragDropSimService.dragAndDrop...');
      var result = spDragDropSimService.dragAndDrop(from, to, opts);
      console.log('Done calling spDragDropSimService.dragAndDrop.');
      if (done) done(result);
      "
      [(prepare-script-arg from) (prepare-script-arg to) (json/write-str opts)])
    (catch Exception e
      (error "Exception in drag and drop: from" (fmt-name from) "to" (fmt-name to) ", ex" e)
      (throw (Exception. "drag and drop failed due to client exception")))))

(defn drag-n-drop-in-report-builder
  "Perform a drag and drop in report builder operation using the reportBuilderService that exists
 in the app itself.

 The _from_ and _to_ arguments can be selector strings or web elements.
 The action can be the string of drag n drop action type"
  [from to action]
  (wait-for-angular)
  (try
    (debug "Performing drag and drop in report builder: from" (fmt-name from) "to" (fmt-name to) " by " action)
    (taxi/execute-script
      (str "
      console.log('Start Drag n Drop in report builder');
      var  from = angular.element(\"" from "\");
      var  to = angular.element(\"" to "\");
      var reportBuilderService = angular.element('body').injector().get('reportBuilderService');

      console.log('reportBuilderDnD:', from, to);
      if (!from) throw Error('reportBuilderDnD - from is undefined');
      if (!to) throw Error('reportBuilderDnD - to is undefined');

      console.log('reportBuilderDnD from:', from);
      console.log('reportBuilderDnD to:', to);

      var dropScope = to.closest('.ng-scope,.ng-isolate-scope').data().$scope;
      var dragData = from.closest('.ng-scope,.ng-isolate-scope').data().$scope.$eval(from[0].getAttribute(\"sp-draggable-data\"))
      var dropData = dropScope.$eval(to[0].getAttribute(\"sp-droppable-data\"))

      if (!dragData) throw Error('reportBuilderDnD - no dragData found for from');
      if (!dropData) throw Error('reportBuilderDnD - no dropData found for to');

      reportBuilderService.setActionFromReport('" action "', dragData, dropData);
      dropScope.$apply();
      "))
    (catch Exception e
      (error "Exception in drag and drop in report builder: from" (fmt-name from) "to" (fmt-name to) " by " action ", ex" e)
      (throw (Exception. (str "drag and drop in report builder failed due to client exception Ex: " e))))))



(defn drag-n-drop-reorder-analyzer
  "Perform a drag and drop in report builder to reorder the analyzer operation using the reportBuilderService that exists
 in the app itself.

 The _from_ and _to_ arguments can be selector strings or web elements.
 The action can be the string of drag n drop action type"
  [from to]
  (wait-for-angular)
  (try
    (debug "Performing drag and drop reorder analyzer: from" (fmt-name from) "to" (fmt-name to))
    (taxi/execute-script
      (str "
      console.log('Start Drag n Drop in report builder');
      var  from = angular.element(\"" from "\");
      var  to = angular.element(\"" to "\");
      var reportBuilderService = angular.element('body').injector().get('reportBuilderService');

      console.log('reportBuilderDnD:', from, to);
      if (!from) throw Error('reportBuilderDnD - from is undefined');
      if (!to) throw Error('reportBuilderDnD - to is undefined');

      console.log('reportBuilderDnD from:', from);
      console.log('reportBuilderDnD to:', to);

      var dropScope = to.closest('.ng-scope,.ng-isolate-scope').data().$scope;
      var dragData = from.closest('.ng-scope,.ng-isolate-scope').data().$scope.$eval(from[0].getAttribute(\"sp-droppable-data\"))
      var dropData = dropScope.$eval(to[0].getAttribute(\"sp-droppable-data\"))

      if (!dragData) throw Error('reportBuilderDnD - no dragData found for from');
      if (!dropData) throw Error('reportBuilderDnD - no dropData found for to');

      reportBuilderService.setActionFromReport('reOrderAnalyzerByDragDrop', dragData, dropData);
      dropScope.$apply();
      "))
    (catch Exception e
      (error "Exception in drag and drop in report builder: from" (fmt-name from) "to" (fmt-name to) ", ex" e)
      (throw (Exception. (str "drag and drop in report builder failed due to client exception Ex: " e))))))


(defn drag-n-drop-remove-column
  [from]
  (wait-for-angular)
  (try
    (debug "Performing drag and drop remove column:" (fmt-name from))
    (taxi/execute-script
      (str "
      console.log('Start Drag n Drop remove column');
      var  from = angular.element(\"" from "\");

      var reportBuilderService = angular.element('body').injector().get('reportBuilderService');

      console.log('reportBuilderDnD:', from);
      if (!from) throw Error('reportBuilderDnD - from is undefined');


      console.log('reportBuilderDnD from:', from);

      var dragScope = from.closest('.ng-scope,.ng-isolate-scope').data().$scope;
      var dragData = dragScope.$eval(from[0].getAttribute(\"sp-draggable-data\"))

      if (!dragData) throw Error('reportBuilderDnD - no dragData found for from');

      reportBuilderService.setActionFromReport('removeColumn', dragData.colDef.spColumnDefinition, null);
      dragScope.$apply();
      "))
    (catch Exception e
      (error "Exception in drag and drop remove column" (fmt-name from) ", ex" e)
      (throw (Exception. (str "drag and drop remove column failed due to client exception Ex: " e))))
    )
  )



(defn drag-n-drop-remove-analyzer
  [from]
  (wait-for-angular)
  (try
    (debug "Performing drag and drop remove analyzer:" (fmt-name from))
    (taxi/execute-script
      (str "
      console.log('Start Drag n Drop remove analyzer');
      var  from = angular.element(\"" from "\");

      var reportBuilderService = angular.element('body').injector().get('reportBuilderService');

      console.log('reportBuilderDnD:', from);
      if (!from) throw Error('reportBuilderDnD - from is undefined');


      console.log('reportBuilderDnD from:', from);

      var dragScope = from.closest('.ng-scope,.ng-isolate-scope').data().$scope;
      var dragData = dragScope.$eval(from[0].getAttribute(\"sp-droppable-data\"))

      if (!dragData) throw Error('reportBuilderDnD - no dragData found for from');

      console.log('reportBuilderDnD condition id:', dragData.tag.id);

      reportBuilderService.setActionFromReport('removeAnalyzer', dragData, null);
      dragScope.$apply();
      "))
    (catch Exception e
      (error "Exception in drag and drop remove analyzer" (fmt-name from) ", ex" e)
      (throw (Exception. (str "drag and drop remove analyzer failed due to client exception Ex: " e))))
    )
  )

(defn do-client-ready-check []
  (taxi/execute-script
    "
    var $browser = angular.element('body').injector().get('$browser');
    console.time('outstandingreqcheck');
    $browser.notifyWhenNoOutstandingRequests(function () {
      console.timeEnd('outstandingreqcheck');
    });
    "))

(def replacer-js
  "
    var seen = [];
    function replacer(key, val) {
      //console.log('stringify entity: replacer |%o|=|%o|', key, val);
      if (!!val && typeof val == 'object') {
        if (key[0] === '_' && !((key === '_id' || key === '_alias' || key === '_ns') && !!val))
          return undefined; // hide internal
        if (key === 'extendedProperties')
          return undefined; // we don't care... is clutter
        if (seen.indexOf(val) >= 0) {
          //console.log('stringify entity: skipping already seen ', key, val);
          if (val._id) {
            if (typeof val._id == 'object')
              return '~visited:id:' + val._id._id;
            return '~visited:id:' + val._id;
          }
          return '~visited:obj:' + Object.prototype.toString.call(val);
        }
        seen.push(val);
      }
      return val;
    }
  ")

(defn get-entity [id request]
  (let [results (execute-async-script
                  (str replacer-js
                       "
                       var done = arguments[arguments.length -1];
                       var spEntityService = angular.element('body').injector().get('spEntityService');
                       spEntityService.getEntity(arguments[0], arguments[1]).then(function (e) {
                         done(JSON.stringify({result: e}, replacer));
                       }, function (err) {
                         done(JSON.stringify({error: err}));
                       });
                       ")
                  [id request])]
    (json/read-json results)))

(comment

  (rt.po.app/logout)
  (execute-async-script
    (str replacer-js
         "
         var done = arguments[arguments.length -1];
         var spLoginService = angular.element('body').injector().get('spLoginService');
         spLoginService.readiNowLogin(arguments[0], arguments[1], arguments[2], true, false).then(function (e) {
           done(JSON.stringify({result: e}, replacer));
         }, function (err) {
           done(JSON.stringify({error: err}));
         });
         ")
    ["EDC" "BIAUser" "Readi911"])
  )

(defn get-entities-of-type [type-id request options]
  ;; e.g.
  ;; (get-entities-of-type "test:employee" "id,name" {:filter "Name like 'Ste%'"})
  (let [results (execute-async-script
                  (str replacer-js
                       "
                       var done = arguments[arguments.length -1];
                       var spEntityService = angular.element('body').injector().get('spEntityService');
                       spEntityService.getEntitiesOfType(arguments[0], arguments[1], arguments[2]).then(function (results) {
                         try {
                           results = _.map(results, function (e) { e.id = e.idP; e.alias = e.nsAlias; return e; });
                           done(JSON.stringify(results, replacer));
                         } catch (e) {
                           done(JSON.stringify({error: e}))
                         }
                       }, function (err) {
                         done(JSON.stringify({error: err}));
                       });
                       ")
                  [type-id request (clojure.walk/stringify-keys options)])]
    (json/read-json results)))

(defn put-entity [entity-json]
  (let [results (execute-async-script
                  "
                  var done = arguments[arguments.length -1];
                  var json = arguments[0];
                  console.log('json=', json);
                  var e = spEntity.fromJSON(json);
                  var spEntityService = angular.element('body').injector().get('spEntityService');
                  spEntityService.putEntity(e).then(function (id) {
                    done(JSON.stringify({id: id}));
                  }, function (err) {
                    done(JSON.stringify({error: err}));
                  });
                  "
                  [entity-json])]
    (json/read-json results)))

(defn delete-entity [id]
  (let [results (execute-async-script
                  "
                  var done = arguments[arguments.length -1];
                  var id = arguments[0];
                  var spEntityService = angular.element('body').injector().get('spEntityService');
                  spEntityService.deleteEntity(id).then(function () {
                    done(JSON.stringify({}));
                  }, function (err) {
                    done(JSON.stringify({error: err}));
                  });
                  "
                  [id])]
    (json/read-json results)))

(defn notify-loggedin []
  "Raise a signedin event to cause tree and internal caches to be cleared."
  (try
    (taxi/execute-script
      "        
        var spLoginService = angular.element('body').injector().get('spLoginService');                    
        if (spLoginService) {
          // Clear caches and nav tree
          spLoginService.notifyLoggedIn();
        }
      "
      ) (catch Exception e
          (throw (Exception. "notifyLoggedIn failed due to client exception")))))

(defn run-report [id options]
  (let [results (execute-async-script
                  (str replacer-js
                       "
                       var done = arguments[arguments.length -1];
                       var spReportService = angular.element('body').injector().get('spReportService');
                       console.log('running report ', arguments[1]);
                       spReportService.getReportData(arguments[0], arguments[1]).then(function (e) {
                         done(JSON.stringify({result: e}, replacer));
                       }, function (err) {
                         done(JSON.stringify({error: err}));
                       });
                       ")
                  [id (clojure.walk/stringify-keys options)])]
    (json/read-json results)))


(defn run-query
  "To do - write some doc for this.

  ...

  The oper is the string representation of the ConditionType used in the query engine
  and has values like: Equal, NotEqual, StartsWith, IsTrue, Contains...
  "
  [query]
  (let [results (execute-async-script
                  "
                  var done = arguments[arguments.length -1];
                  var query = JSON.parse(arguments[0]);
                  var spReportService = angular.element('body').injector().get('spReportService');
                  console.log('runQuery', query);
                  spReportService.runQuery(query).then(function (result) {
                    //debugging why dates coming back as a string....
                    //console.log('runQuery=>', result);
                    done(JSON.stringify({result: result}));
                  }, function (err) {
                    done(JSON.stringify({error: err}));
                  });
                  "
                  [(json/write-str query)])]
    (json/read-json results)))

(defn report-grid-scroll-to-top []
  "Perform a scroll down operation using the jquery scrollTop simulation that exists
  in the app itself.

  "
  (try
    (taxi/execute-script
      "
        var reportControl = angular.element(document.getElementsByClassName('ngViewport'))[0];

        reportControl.scrollTop = 0;
      "
      ) (catch Exception e
          (throw (Exception. "report grid scroll to top failed due to client exception")))))


(defn report-grid-scroll-down []
  "Perform a scroll down operation using the jquery scrollTop simulation that exists
  in the app itself.

  "
  (try
    (taxi/execute-script
      "
        var reportControl = angular.element(document.getElementsByClassName('ngViewport'))[0];
        var scrollPostion = reportControl.scrollTop;
        scrollPostion += 300;
        var newScrollPostion = reportControl.scrollHeight >= scrollPostion ? scrollPostion : reportControl.scrollHeight;

        reportControl.scrollTop = newScrollPostion;
      "
      )
    (catch Exception e
      (throw (Exception. "report grid scroll down failed due to client exception")))))

(defn page-scroll-down []
  "Perform a scroll down operation using the jquery scrollTop simulation that exists
  in the app itself.

  "
  (try
    (taxi/execute-script
      "
        var contentPage = angular.element(document.getElementsByClassName('client-view-content'))[0];
        var scrollPostion = contentPage.scrollTop;
        scrollPostion += 300;
        var newScrollPostion = contentPage.scrollHeight >= scrollPostion ? scrollPostion : contentPage.scrollHeight;

        contentPage.scrollTop = newScrollPostion;
      "
      )
    (catch Exception e
      (throw (Exception. "content page scroll down failed due to client exception")))))

(defn report-grid-scroll-up []
  "Perform a scroll up operation using the jquery scrollTop simulation that exists
  in the app itself.

  "
  (try
    (taxi/execute-script
      "
        var reportControl = angular.element(document.getElementsByClassName('ngViewport'))[0];
        var scrollPostion = reportControl.scrollTop;
        scrollPostion -= 300;
        var newScrollPostion = scrollPostion >= 0 ? scrollPostion : 0;

        reportControl.scrollTop = newScrollPostion;
      "
      )
    (catch Exception e
      (throw (Exception. "report grid scroll up failed due to client exception")))))


(defn get-css-color-from-name [color-name]
  (let [named-color (taxi/execute-script
                      (str "
                      var namedColors = angular.element('body').injector().get('namedColors');

                      var namedColor = _.filter(namedColors, function(nc) { return nc.name === '" color-name "'; });

                      if (namedColor && namedColor.length > 0) {
                          var color = namedColor[0].value;
                          var a = 1;
                          if (color.a >= 0) {
                              a = color.a / 255;
                          }

                          var result = 'rgba(' + color.r + ',' + color.g + ',' + color.b + ',' + a + ')';
                          return result;
                      }

                      return '';

                      ")
                      )]
    named-color))

(defn get-color-name-from-rgba-css [color-css]
  (let [color-name (taxi/execute-script
                     (str "
                      var namedColors = angular.element('body').injector().get('namedColors');

                      var namedColor = _.filter(namedColors, function(nc) {
                       var colorValue = nc.value;
                       var a = 1;
                       if (colorValue.a >= 0){
                            a = colorValue.a / 255;
                       }

                       var colorCss = 'rgba(' + colorValue.r + ',' + colorValue.g + ',' + colorValue.b + ',' + a + ')';

                       return colorCss === '" color-css "';
                       });

                      if (namedColor && namedColor.length > 0) {
                          var colorName = namedColor[0].name;
                          return colorName;
                      }

                      return '';

                      ")
                     )]
    color-name))

(defn get-color-name-from-rgb-css [color-css]
  (let [color-name (taxi/execute-script
                     (str "
                      var namedColors = angular.element('body').injector().get('namedColors');

                      var namedColor = _.filter(namedColors, function(nc) {
                       var colorValue = nc.value;

                       var colorCss = 'rgb(' + colorValue.r + ', ' + colorValue.g + ', ' + colorValue.b + ')';

                       return colorCss === '" color-css "';
                       });

                      if (namedColor && namedColor.length > 0) {
                          var colorName = namedColor[0].name;
                          return colorName;
                      }

                      return '';

                      ")
                     )]
    color-name))





(defn get-grouped-agg-rows-by-value [group-row]
  (try (let [agg-rows (taxi/execute-script
                              (str "
                                  var groupByRow = angular.element(\"" group-row "\").data().$scope.row;

                                  if (groupByRow && groupByRow.children && groupByRow.aggChildren.length > 0){
                                    return groupByRow.aggChildren;
                                  }
                                  return [];

                               ")
                              )]
         agg-rows)
       (catch Exception e
         (error "Exception in get-grouped-rows-by-value, ex:" e)
         (throw (Exception. "get-grouped-rows-by-value failed due to client exception")))))

(defn get-grouped-rows-by-value [group-row]
  (try (let [row-index-list (taxi/execute-script
                              (str "
                            var groupByRow = angular.element(\"" group-row "\").data().$scope.row;
                            if (groupByRow && groupByRow.children && groupByRow.children.length > 0){
                              var rowIndexList = [];
                              for (var i=0;i<groupByRow.children.length;i++){
                                rowIndexList.push(groupByRow.children[i].rowIndex);
                              }
                              return rowIndexList;
                            }
                            return [];

                         ")
                              )]
         row-index-list)
       (catch Exception e
         (error "Exception in get-grouped-rows-by-value, ex:" e)
         (throw (Exception. "get-grouped-rows-by-value failed due to client exception")))))


(defn query-results-as-objects [results]
  (let [cols (get-in results [:result :cols])
        cols (map :title cols)
        rows (get-in results [:result :data])
        rows (map (fn [x] (map :value (:item x))) rows)
        keys (map #(keyword (string/trim %)) cols)]
    (reduce (fn [result data]
              (conj result (zipmap keys (map #(string/trim %) data))))
            []
            rows)))

(defn nudge-top-dialog
  "If a dialog is showing, nudge it a little."
  []
  (if (taxi/exists? ".modal-header:last")
    (do
      (taxi/drag-and-drop-by ".modal-header:last" {:x 30 :y 30})
      true)
    false))

(defn test-id-css
  "Get the test-id CSS seelector fragment for the given name."
  [name]
  (str "[test-id=\"" (string/replace name #"[\s\']" "_") "\"]"))

(defn get-entities-by-type-alias [type-alias & conds]
  ;; a cond is like {:expr {:field "name"} :oper "startsWith" :val name-filter}
  (-> {:root    {:id type-alias :related []}
       :selects [{:field "name"}]
       :conds   conds}
      (run-query)
      (query-results-as-objects)))

(defn get-authenticated-identity
  "Return the current authenticated identity, if one."
  []
  (wait-for-angular)
  (try
    (-> (taxi/execute-script
          "
          var done = arguments[0];

          if (!window.angular) return 'null';

          var spLoginService = angular.element('body').injector().get('spLoginService');

          if (!spLoginService) return 'null';

          var result = spLoginService.getAuthenticatedIdentity();

          result = JSON.stringify(result || {});

          console.log('RT:get-authenticated-identity: \"' + result +'\"');

          if (done) done(result);
          return result;
          "
          [])
        (json/read-json))
    (catch Exception e
      (error "Exception in get-authenticated-identity, ex:" e)
      (throw (Exception. "get-authenticated-identity failed due to client exception")))))
