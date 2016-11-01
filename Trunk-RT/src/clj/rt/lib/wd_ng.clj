(ns rt.lib.wd-ng
  (:require [rt.lib.wd :refer [execute-async-script prepare-script-arg]]
            [clj-webdriver.taxi :refer [execute-script *driver*]]
            [clj-webdriver.driver :refer [init-driver driver?]]
            [clj-webdriver.element :refer [element-like? init-elements]]
            [clj-webdriver.core :refer [find-elements]]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import java.util.Date
           (java.text SimpleDateFormat)))

(defn wait-for-angular
  "Wait for AngularJS to indicate things have settled... this pretty much
  means no outstanding $http operations or $timeouts."
  []
  (let [t0 (.getTime (java.util.Date.))]
    ;(print "waiting for angular...")
    (try
      (execute-async-script
        "
        var callback = arguments[0] || function () {};
        try {
          //console.time('e2e waiting for angular');
          angular.element('body').injector().get('$browser').
              notifyWhenNoOutstandingRequests(function () {
                //console.timeEnd('e2e waiting for angular');
                callback(null) });
        } catch (e) {

          callback(e);
        }
        ")
      (catch Exception ex
        (error "Exception caught waiting for angular: " ex)))
    ;(println "wait ng took " (- (.getTime (java.util.Date.)) t0))
    ))

(defn add-angular-wait-to-finder
  "Replace the given finder function with another that first waits for Angular."
  [f]
  (fn [& args]
    (wait-for-angular)
    (apply f args)))

(def find-by-jq-wait-ng-js "
  var callback = arguments[arguments.length-1] || function () {};
  var selector = arguments[0];
  var b =  angular.element('body').injector().get('$browser');
  console.time('find-by-jq-wait-ng');
  try {
    b.notifyWhenNoOutstandingRequests(function () {
      console.log('find-by-jq arguments:', arguments.length, arguments);
      var result = $(selector);
      result = _.map(result, _.identity); // firefox hangs if we don't do this
      console.log('find-by-jq (after waiting on ng) ' + arguments.length + ' ' + arguments[0] + ', returned ' + result.length);
      callback(result);
    });
  } catch (e) {
    callback(null);
  } finally {
    console.timeEnd('find-by-jq-wait-ng');
  }
  ")

(defn find-by-jq-wait-ng
  ([q] (find-by-jq-wait-ng *driver* q))
  ([driver q]
   (try
     (let [elements (execute-async-script driver find-by-jq-wait-ng-js [q])]
       ;;todo - check that we aren't creating WebElements on nil webelement
       (init-elements elements))
     (catch Exception e
       (error (str "Exception finding \"" q "\". Exception=" (.getMessage e)))
       (throw e)))))

(defn jq-finder-wait-ng
  "default to find-by-jq, but can pass map to say whether css, xpath or jq"
  ([q] (jq-finder-wait-ng *driver* q))
  ([driver q]
   (if q
     (let [t0 (.getTime (Date.))
           elements (cond
                      (element-like? q) (vector q)
                      (map? q) (if (= (first (keys q)) :jq)
                                 (find-by-jq-wait-ng driver (first (vals q)))
                                 (find-elements driver q))
                      :else (find-by-jq-wait-ng driver q))
           t (- (.getTime (Date.)) t0)]
       (debug (format "%s: jq-finder-wait-ng %s => found %d elements, took %d msecs"
                        (.format (SimpleDateFormat. "HH:mm:ss.SSS") (Date.)) (pr-str q) (count elements) t))
       elements)
     (do (error "jq-finder-wait-ng called on falsy query:" q)
         (throw (Exception. "jq-finder-wait-ng called on falsy query"))))))

(defn evaluate-angular-expression
  "Evaluate the given expression in the AngularJS scope of the given element."
  [element expression]
  (wait-for-angular)
  (execute-script
    "
    var element = arguments[0], expression = arguments[1];
    console.log('evaluating', expression, ' on ', element);
    var scope = angular.element(element).scope();
    return scope.$eval(expression);
    "
    [(prepare-script-arg element) expression]))

(defn execute-script-on-element
  "Evaluate the given script with the given element and additional args
  where the element is passes as arguments[0] with any args after that."
  [script element & args]
  (wait-for-angular)
  (execute-script script (apply vector (prepare-script-arg element) args)))

(defn apply-angular-expression
  "Evaluate the given expression in the AngularJS scope of the given element."
  [element expression]
  (wait-for-angular)
  (execute-script
    "
    var element = arguments[0], expression = arguments[1];
    console.log('evaluating', expression, ' on ', element);
    var scope = angular.element(element).scope();
    var result = scope.$eval(expression);
    scope.$apply();
    return result;
    "
    [(prepare-script-arg element) expression]))
