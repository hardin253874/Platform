(ns rt.test.core
  (:import (java.io StringWriter))
  (:require [rt.setup :refer [get-settings get-default-fixtures running-in-teamcity?]]
            [rt.test.db :refer :all]
            [rt.lib.wd :refer [start-browser stop-browser get-browser-logs is-webdriver-initialised? get-browser]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.util :refer [mapcat-indexed datetime-str indices deep-merge ms->s csv-datetime-str
                                 write-csv-objects id->str timeit clean-filename
                                 format-tc-message left-with-elipses]]
            [rt.test.concurrent :refer [post-step-result post-test-event post-tc-event]]
            [clj-webdriver.taxi :refer [take-screenshot]]
            [clj-webdriver.element :refer [element-like?]]
            [clojure.java.io :as io]
            [clojure.string :as string]
            [clojure.set :as set]
            [clojure.test :refer [*testing-vars* testing-contexts-str]]
            [clojure.stacktrace :as stack]
            [clojure.pprint :refer [pprint pp print-table]]
            [clj-webdriver.taxi :as taxi]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import [java.io File PrintWriter StringWriter]
           [java.util Date]))

;; TODO - fix up this BFM... all the inconsistency with the handling of the references into the testrun
;; TODO - don't use (get-settings), or maybe to define (get-settings) to return the thread specific settings

(def ^:private current-test-run-id (atom nil))

(def ^:private testrun-running-state (atom {}))

(defn- get-testrun-driver [id]
  (get-in @testrun-running-state [id :driver]))

(defn- set-testrun-driver [id value]
  (swap! testrun-running-state #(assoc-in % [id :driver] value)))

(defn quit-all-drivers []
  (try (doseq [d (map :driver (vals @testrun-running-state))]
         (taxi/quit d))
       (catch Exception _)))

(defn set-current-test-run-id! [id]
  (reset! current-test-run-id id))

(defn get-current-test-run-id []
  @current-test-run-id)

;; The current test context for when executing a script
;; *test-context* is the old name for it, only here until we switchover
;; *tc* is the nice short name for it
(def ^:dynamic *test-context* nil)
(def ^:dynamic *tc* nil)

;; to help with debugging sessions
(defn merge-tc [m]
  (alter-var-root (var *tc*) #(merge % m))
  (alter-var-root (var *test-context*) #(merge % m)))

(def ^:dynamic *in-handler?* nil)

;; The current test "cursor" with the testrun and the path to the
;; contained entity of interest, typically a script.
(def ^:dynamic *testrun-cursor* nil)

;; move this state to the test run
(def file-counter (atom 0))
(defn next-file-count []
  (swap! file-counter inc))

;; Many of the functions here deal with a cursor, or path based reference into the
;; testrun data structure.
;; This entity-ref is simply a vector of the object and then the path
;; and may be destructured as using similar to [obj path :as node-ref]
;; or in the  case of a testrun [testrun path :as entity-ref]
;; To get the actual entity within the obj, use (get-in obj path) or the following helper

(defn resolve-entity [[obj path]]
  (get-in obj path))

(defn read-expressions
  "Read one or more forms and return as a list of parsed forms ready to eval."
  [text]
  (binding [*read-eval* false]
    ;; Note - we wrap in \( and \) otherwise read-string only reads the first form it sees.
    (read-string (str \( text \newline \)))))

(defn read-script-string [script]
  (first (read-expressions script)))

(defn eval-script [script-str]
  ;; the script-str might actually be a function... but that's mainly in debug
  ;; and might not be supported much longer

  (let [{:keys [dry-run]} (get-settings)]
    (when-not dry-run
      ;(binding [*out* *err*] (debug "Running script-str" (type script-str) script-str))

      (if (instance? String script-str)
        (binding [*ns* (find-ns 'rt.scripts)]
          ;; if there are multiple expressions then we run all of them
          ;; and return the result of the last
          (reduce #(eval %2) nil (read-expressions script-str)))
        ;; else assume a function
        (script-str)))))

(declare do-report)

(defn eval-script-with-out-str
  "Run the given script capturing any output to *out* in a string
  and return a pair of the actual result and the out string."
  [script-str]
  (let [s (StringWriter.)]
    (binding [*out* s]
      (try
        ;; returning a vector of the result and stdout
        [(eval-script script-str) (str s)]

        (catch Throwable ex
          (do-report {:type :error :message (str ex)})
          (clojure.stacktrace/print-cause-trace ex)
          [ex (str s)])))))

;; TODO - update this function for the new testrun format
(defn get-run-metrics
  "Get a list of metered events, at the moment a script run, with times and the test they belong to."
  [id]
  (let [{:keys [tests id created] :as run} (get-test-run id)
        script-summary (fn [{{test-id :id} :test} {:keys [time doc counters]}]
                         (merge {:run-id   id
                                 :datetime (csv-datetime-str created)
                                 :test-id  test-id
                                 :script   doc
                                 :time     time}
                                counters))]
    (when run
      (let [row-objects (mapcat (fn [test] (map #(script-summary test %) (:scripts test))) tests)]
        row-objects))))

;; TODO - update this function for the new testrun format
(defn get-metrics
  "Get metrics for the given run ids.
  Either pass in run ids as mulitple arguments
  or pass :all to get all known runs
  or a number for the last n known runs."
  [n & more]
  (let [ids (cond
              (= :all n) (map :id (get-test-run-list))
              (number? n) (map :id (->> (get-test-run-list)
                                        (sort-by :created)
                                        reverse (take n)))
              :default (cons n more))]
    (mapcat get-run-metrics ids)))

;; TODO - update this function for the new testrun format
(defn write-run-metrics-csv
  ([file-name] (write-run-metrics-csv file-name :all))
  ([file-name id-or-n] (write-csv-objects file-name (get-metrics id-or-n))))

(defn ancestor-paths
  "produce a sequence of paths to each ancestor
  of the given path... so if path is [:a :b :c]
  then this produces ((:a) (:a :b) (:a :b :c))"
  [path]
  (map #(take (inc %) path) (range (count path))))

(defn get-entities-on-path [[testrun path]]
  {:pre [(= (:type testrun) :testrun)]}
  (->> (ancestor-paths path)
       (map #(let [{:keys [id type]} (get-in testrun %)]
              {:id id :type type :path (vec %)}))
       (filter :type)))

(defn get-script-object [script]
  ;; steps can be strings, or maps, or keyword ids
  ;; => turn them into "objects"
  ;; TODO - deal with scripts in scripts
  (cond
    (keyword? script) (get-test-entity script)
    (and (map? script) (keyword? (:script-id script))) (get-test-entity (:script-id script))
    (map? script) script
    :default {:script script}))

(defn get-script-entity [script]
  ;; steps can be strings, or maps, or keyword ids
  ;; => turn them into "entities"
  ;; TODO - deal with scripts in scripts
  (cond
    (keyword? script) (get-test-entity script)
    (and (map? script) (keyword? (:script-id script))) (get-test-entity (:script-id script))
    (and (map? script) (:type script)) script
    (map? script) {:type :testscript :steps [script]}
    :default {:type :testscript :steps [{:script script}]}))

;; Get a representation of the given entity type with all
;; referenced entities replaced with their test records
;; and any other preparation to create a record of the execution
;; of a test.
(defmulti get-test-record (fn [entity & args] (:type entity)))

(defmethod get-test-record :testscript
  [script]
  (let [script (assoc script :steps (mapv get-script-object (:steps script)))]
    script))

(defmethod get-test-record :testfixture
  [fixture]
  (let [
        get-script-record (comp get-test-record get-script-entity)]
    (->> [:setup :teardown :after-fail :after-pass :after-error :before-step :after-step]
         (reduce #(assoc %1 %2 (mapv get-script-record (%2 fixture))) fixture))))

(defmethod get-test-record :test
  ([test] (get-test-record test []))
  ([test fixtures & args]
    (try
      ;; the args bit above might be a filterfn that we don't care about here
      (let [get-script-record (comp get-test-record get-script-entity)
            get-fixture-record (comp get-test-record get-test-entity)

            ;; flatten all the script lists
            steps (mapv get-script-record (:steps test))
            setup (mapv get-script-record (:setup test))
            teardown (mapv get-script-record (:teardown test))

            ;; merge the given fixtures with the test's fixtures
            test-fixtures (mapv get-fixture-record (:fixtures test))
            fixtures (into (vec fixtures) test-fixtures)

            ;; move any data, setup and teardown into an anonymous fixture
            fixtures (conj fixtures
                           {:type :testfixture :setup setup :teardown teardown
                            :data (:data test)})

            ;; fixup the test
            test (dissoc test :setup :teardown :data)
            test (assoc test :fixtures fixtures :steps steps)]

        test)

      (catch Exception e
        (error (format "Exception building test for test %s" (or (:id test) (pr-str test))))
        (throw e)))))

(defn pred [p v]
  (let [s (read-string (str \# p))]
    (binding [*ns* (find-ns 'rt.scripts)]
      (eval `(apply ~s ~v)))))

(defn has-tags? [tags test]
  (let [tags (map #(if (string? %) (keyword %) %) tags)]
    (set/subset? (set tags) (set (:tags test)))))

(defn- sort-tests [tests]
  (->> tests
       ;; sort by id or randomise
       ;shuffle
       (sort-by :id)
       ;; default the grouping keys
       (map #(merge {:nomodify ((set (:tags %)) :nomodify) :priority 10} %))
       ;; group
       (group-by #(select-keys % [:nomodify :priority]))
       ;; sort by group key
       (into (sorted-map-by #(compare [(not (:nomodify %1)) (:priority %1)]
                                      [(not (:nomodify %2)) (:priority %2)])))
       ;; map back to a seq of tests
       vals
       (apply concat)))

(defn get-suite-tests [{:keys [tests test-filter]}]
  ;; Grab both the static defined tests and any based on the optional filter.
  (let [test-ids (set (map #(if (:id %) (:id %) %) tests))
        test-filter-fn (fn [t] (try (pred test-filter [t])
                                    (catch Exception e
                                      (error "Exception handling test filter expression" e)
                                      false)))
        ;; sort tests with nomodify tag first - todo: deprecate this and add a priority attr to test
        get-sort-key (fn [{:keys [id tags] :as t}]
                  (str (if ((set tags) :nomodify) 0 1) ":" id))]
    (map get-test-entity
         (concat tests
                 (when-not (empty? test-filter)
                   (->> (get-entity-list :test)

                        ;; filter first without :index and the test re index will be ignored
                        (filter test-filter-fn)

                        (sort-tests)
                        ;(sort-by get-sort-key)

                        ;; then index after the first pass of the filter and then filter again
                        ;; so we have indexes after other filtering has been applied
                        (map-indexed #(assoc %2 :index %1))
                        (filter test-filter-fn)

                        (remove #(test-ids (:id %)))
                        (map :id)))))))

(defn filter-tests [expr-form]
  (map :id (get-suite-tests {:tests [] :test-filter (pr-str expr-form)})))

(defn print-filtered-tests [expr-form]
  (pprint (filter-tests expr-form)))

(defn find-index-for-suite-test [suite-id test-id-re]
  (->> (get-test-entity suite-id)
       (get-suite-tests)
       (map #(hash-map :index %1 :id (:id %2)) (range))
       (filter #(re-find test-id-re (str (:id %))))))

(comment
  (pprint (map :tags (get-test-list :test)))

  (->> (get-suite-tests {:tests       []
                         :test-filter (pr-str '(do (some-> (get-in % [:steps 0 :script]) (re-find #"open"))))})
       first)

  (print-filtered-tests '(has-matching-step? #"comment" %))
  (print-filtered-tests '(->> % :steps (map :script) (some #(some->> % (re-find (re-pattern "sleep"))))))
  (print-filtered-tests '(do (println (keys %) false)))

  (print-filtered-tests '(has-tags? ["prod" "smoke-test"] %))
  (print-filtered-tests '(has-tags? ["prod" "regression"] %))
  (print-filtered-tests '(has-tags? ["prod" "regression" "nomodify"] %))
  (print-filtered-tests '(has-tags? ["prod" "known-issues"] %))

  (->> (filter-tests '(and (has-tags? ["regression" "prod"] %)
                           (has-index-in-range? 100 -1 %)))
       (map get-test-entity)
       (map #(select-keys % [:id :tags]))
       (map #(format "%60s %s" (:id %) (:tags %)))
       count)


  (timeit "" (->> (get-test-entity :rn/suites/regression-chrome)
                  (get-suite-tests)
                  count))

  (print-filtered-tests '(and (has-tags? ["regression"] %) (not (has-tags? ["prod"] %))))
  (print-filtered-tests '(and (has-tags? ["regression" "nomodify"] %) (not (has-tags? ["prod"] %))))
  (print-filtered-tests '(and (has-tags? ["regression" "nomodify"] %) (not (has-tags? ["prod"] %))))

  (print-filtered-tests '(and (has-tags? ["user/abida" :regression] %) (not (has-tags? ["prod"] %))))
  (print-filtered-tests '(and (has-tags? ["user/karen"] %) (not (has-tags? ["prod"] %))))
  (print-filtered-tests '(and (has-tags? ["user/shaofen"] %) (not (has-tags? ["prod"] %))))
  (print-filtered-tests '(and (has-tags? ["user/tina"] %) (not (has-tags? ["prod"] %))))

  (print-filtered-tests '(and (has-tags? ["user/anurag"] %) (not (has-tags? ["prod"] %))))

  (print-filtered-tests '(has-tags? ["area/report-builder"] %))
  (print-filtered-tests '(has-tags? ["area/chart-builder"] %))

  (let [suite (get-test-entity :rn/suites/regression-chrome)]
    (pprint (map #(str "index=" %2 ", id=" (:id %1)) (get-suite-tests suite) (range))))

  (find-index-for-suite-test :rn/suites/regression-chrome #"create-update")

  )

(defmethod get-test-record :testsuite
  ([suite] (get-test-record suite [] identity))
  ([suite fixtures filterfn]
    (let [get-fixture-record (comp get-test-record get-test-entity)
          ;; add the given fixtures in front of this suite's "once" fixtures
          once-fixtures (mapv get-fixture-record (concat fixtures (:once-fixtures suite)))
          ;; resolve the each fixture
          each-fixtures (mapv get-fixture-record (:each-fixtures suite))

          ;; Prepare tests as records of those tests (or suites), passing in our "each" fixtures
          tests (->> (get-suite-tests suite)
                     ;; ensure we are indexed in case the filter makes use of an index
                     (map-indexed #(assoc %2 :index %1))
                     (filter filterfn)
                     (mapv #(get-test-record % each-fixtures filterfn)))

          ;; a suite's :each-fixtures are passed to the contained tests and suites, and then removed
          ;; a suite's :once-fixtures are rekeyed as :fixtures
          suite (dissoc suite :once-fixtures :each-fixtures)
          suite (assoc suite :tests tests :fixtures once-fixtures)]
      suite)))

(comment

  (pprint (mapcat #(map (fn [t] (hash-map :id (:id t) :t (:id %))) (:tests %))
                  (:tests (get-test-record (get-test-entity :rn/suites/new-tests-in-progress)))))

  )

(defmethod get-test-record :default [e & args]
  (throw (Exception. (format "ERROR unknown entity type for entity: %s" (pr-str e)))))

(defn merge-default-fixtures [{:keys [type fixtures] :as test-record}]
  ;; we only merge default fixtures for tests added without a suite
  (debug "merging " (select-keys test-record [:id :type :fixtures]) "with" (get-default-fixtures))
  (if (and (= type :test))
    (assoc test-record :fixtures (concat (vec (get-default-fixtures)) fixtures))
    test-record))

;; test means test or suite
(defn add-to-testrun
  ([testrun test] (add-to-testrun testrun test identity))
  ([testrun test filter-fn]
   "The filter-fn means we only include the entity or any of its referenced entities
   if they pass the filter-fn."
   (let [test-entity (merge-default-fixtures (get-test-entity test))
         entity-record (get-test-record test-entity [] filter-fn)
         existing-records (vec (:tests testrun))]
     (if entity-record
       (assoc testrun :tests (conj existing-records entity-record))
       (assoc testrun :tests existing-records)))))

(defn create-testrun
  "Create a testrun from the given tests that are tests or suites, either entities or ids"
  [& tests]
  (let [{:keys [id]} (create-test-run)
        ;; horrid last minute hack (what's new) to look up some global settings
        ;; and build a filter on the tests
        {:keys [from-test-index max-test-count]
         :or   {from-test-index 0 max-test-count 99999}} (get-settings)
        max-test-count (max max-test-count 0)
        test-filter #(or (not= :test (:type %))
                         (not (:index %))
                         (and (>= (:index %) from-test-index)
                              (< (:index %) (+ from-test-index max-test-count))))
        testrun (update-test-run! id (fn [r] (reduce #(add-to-testrun %1 %2 test-filter) r tests)))]

    (reset! file-counter 0)

    (when-not (not-empty (:tests testrun))
      (warn "create-testrun.... no tests?" tests))

    ;; checking for empty tests list in case tests are not found
    (when (not-empty (:tests testrun))
      (write-test-run-file testrun)
      id)))

(defn create-partial-testrun
  "Create a testrun from the given suites that only includes the given tests."
  [suites tests-to-include]
  (let [tests-to-include (->> tests-to-include
                              (map (comp :id get-test-entity))
                              (filter identity)
                              set)
        temp (debug "tests to include" tests-to-include)
        test-filter #(or (not= :test (:type %))
                         (empty? tests-to-include)
                         (tests-to-include (:id %)))
        {:keys [id]} (create-test-run)
        testrun (update-test-run! id (fn [r] (reduce #(add-to-testrun %1 %2 test-filter) r suites)))]

    (reset! file-counter 0)

    (debug "create-partial-testrun...." suites tests-to-include (mapcat #(map :id (:tests %)) (:tests testrun)))

    (when-not (not-empty (:tests testrun))
      (warn "create-partial-testrun.... no tests?" suites tests-to-include testrun))

    ;; checking for empty tests list in case tests are not found
    (when (not-empty (:tests testrun))
      (write-test-run-file testrun)
      #_(write-pprint-test-run-file testrun)
      id)))

;; A little thinking on the way I've done this get scripts thing.
;; I should probably just return a list of the "cursors" rather than the entities
;; with the path stuffed in them. And then when asking for the scripts, say, I look them up.

(defmulti get-test-scripts :type)

(defmethod get-test-scripts :testscript
  [e path]
  (map-indexed #(assoc %2 :path (conj path :steps %1)
                          ;; default the owner of the script to be a test
                          :owner :test)
               (:steps e)))

;; for some reason I'm having trouble calling the fixture multimethod (something to do with arity?!),
;; hence the specifically named one here
(defn get-fixture-scripts
  [e path scripts-type]
  (->> (scripts-type e)
       (mapcat-indexed #(get-test-scripts %2 (conj path scripts-type %1)))
       (map #(assoc % :owner :fixture))))

(defmethod get-test-scripts :testfixture
  ([e path]
    (concat (get-test-scripts e path :setup)
            (get-test-scripts e path :teardown)))
  ([e path scripts-type]
    (get-fixture-scripts e path scripts-type)))

(defmethod get-test-scripts :test
  [e path]
  (concat
    (mapcat-indexed #(get-fixture-scripts %2 (conj path :fixtures %1) :setup) (:fixtures e))
    (mapcat-indexed #(get-test-scripts %2 (conj path :setup %1)) (:setup e))
    (mapcat-indexed #(get-test-scripts %2 (conj path :steps %1)) (:steps e))
    (mapcat-indexed #(get-test-scripts %2 (conj path :teardown %1)) (:teardown e))
    (apply concat (map #(get-fixture-scripts %2 (conj path :fixtures %1) :teardown)
                       (reverse (range (count (:fixtures e)))) (reverse (:fixtures e))))))

(defmethod get-test-scripts :testsuite
  [e path]
  (concat
    (mapcat-indexed #(get-fixture-scripts %2 (conj path :fixtures %1) :setup) (:fixtures e))
    (mapcat-indexed #(get-test-scripts %2 (conj path :tests %1)) (:tests e))
    (apply concat (map #(get-fixture-scripts %2 (conj path :fixtures %1) :teardown)
                       (reverse (range (count (:fixtures e)))) (reverse (:fixtures e))))))

(defmethod get-test-scripts :default
  [e path]
  (error "unexpected get-test-scripts on" e "with path" path)
  (throw (Exception. (format "Failed to get scripts for " (pr-str e)))))

(defn get-testrun-scripts
  "Returns the entire list of scripts for the test run (the 'current' testrun if one
  is not provided). Each script includes its path within the test run."
  ([] (get-testrun-scripts (get-last-test-run-id)))
  ([id] (mapcat-indexed #(get-test-scripts %2 [:tests %1]) (:tests (get-test-run id)))))

;; get a seq of the path to this entity plus the paths to its contained entities
(defmulti get-entity-map :type)

(defn get-testrun-entities [testrun]
  {:pre [(= :testrun (:type testrun))]}
  (->> (get-entity-map testrun [])
       (map #(let [{:keys [id type]} (get-in testrun %)]
              {:path % :id id :type type}))))

(defmethod get-entity-map :testscript
  [e path]
  ;(println path (dissoc e :steps))
  (concat
    [path]
    (map-indexed (fn [i _] (conj path :steps i)) (:steps e))))

(defmethod get-entity-map :testfixture
  [e path]
  ;(println path (:id e))
  (concat
    [path]
    (mapcat-indexed #(get-entity-map %2 (conj path :setup %1)) (:setup e))
    (mapcat-indexed #(get-entity-map %2 (conj path :teardown %1)) (:teardown e))))

(defmethod get-entity-map :test
  [e path]
  ;(println path (:id e))
  (concat
    [path]
    (mapcat-indexed #(get-entity-map %2 (conj path :fixtures %1)) (:fixtures e))
    (mapcat-indexed #(get-entity-map %2 (conj path :steps %1)) (:steps e))))

(defmethod get-entity-map :testsuite
  [e path]
  ;(println path (:id e))
  (concat
    [path]
    (mapcat-indexed #(get-entity-map %2 (conj path :fixtures %1)) (:fixtures e))
    (mapcat-indexed #(get-entity-map %2 (conj path :tests %1)) (:tests e))))

(defmethod get-entity-map :testrun
  [e path]
  ;(println path (:id e))
  (concat
    [path]
    (mapcat-indexed #(get-entity-map %2 (conj path :tests %1)) (:tests e))))

(defmethod get-entity-map :default
  [e path]
  (error "unexpected get-entity-map on" e "with path" path)
  (throw (Exception. (format "Failed to get-entity-map for " (pr-str e)))))

(defmulti get-test-context :type)

(defmethod get-test-context :testscript
  [e]
  (apply merge (map :result (:steps e))))

(defmethod get-test-context :testfixture
  [e]
  (apply merge (:data e) (map get-test-context (:setup e))))

(defmethod get-test-context :test
  [e]
  (apply merge (map get-test-context (:fixtures e))))

(defmethod get-test-context :testsuite
  [e]
  (apply merge (map get-test-context (:fixtures e))))

(defmethod get-test-context :default
  [e]
  {})

(defn make-test-context [r tc path-segment]
  {:pre [(= (:type r) :testrun)]}
  (let [path (conj (vec (:__path tc)) path-segment)
        part (get-in r path)
        tc (assoc tc :__path path)]
    (if (map? part)
      (do #_(debug "making test context from " path (:type part))
        (merge tc (get-test-context part)))
      tc)))

(defn get-default-tc []
  (select-keys (get-settings) [:target :tenant :username :password :elevate-user]))

;; todo - include in *tc* the result of the previous step of a test as the :last-result

(defn get-test-context-for-ref [testrun ref]
  {:pre [(= (:type testrun) :testrun)]}
  (-> (reduce (partial make-test-context testrun) (get-default-tc) (:path ref))
      ;; clean off the temp member (ugly!)
      (dissoc :__path)))

(defn update-testrun-record!
  [id path update-script]
  (let [update-testrun (fn [testrun] (update-in testrun path update-script))
        testrun (update-test-run! id update-testrun)]
    (get-in testrun path)))

(defn get-test-refs [testrun]
  {:pre [testrun
         (= (:type testrun) :testrun)]}
  (let [get-suite-id (fn [path]
                       ;; build suite-id-str based on possibly mult containing suites
                       (:suite-id-str (reduce #(let [path (conj (vec (:path %1)) %2)
                                                     {:keys [id type]} (get-in testrun path)
                                                     suite-id (when (= :testsuite type) id)]
                                                {:path         path
                                                 :suite-id-str (str (:suite-id-str %1) suite-id)})
                                              {} path)))]
    (->> (get-testrun-entities testrun)
         (filter #(#{:test} (:type %)))
         (map #(assoc % :suite-id-str (get-suite-id (:path %)))))))

(defn get-parent-entity
  [types testrun path]
  {:pre [(= (:type testrun) :testrun)]}
  ;; look back to find the most immediate entity of the given type, ...bit ugly...
  (let [types-set (set types)]
    (when-let [parent-path (loop [path (drop-last path)]
                             (cond
                               (empty? path) nil
                               (types-set (:type (get-in testrun path))) path
                               :else (recur (drop-last path))))]
      (assoc (get-in testrun parent-path) :path parent-path))))

(def get-script-parent (partial get-parent-entity #{:test :testfixture}))
(def get-script-test (partial get-parent-entity #{:test}))
(def get-script-suite (partial get-parent-entity #{:testsuite}))

(defn get-fixture-refs [[testrun :as entity-ref]]
  {:pre [(= (:type testrun) :testrun)]}
  ;; get the fixures that apply to the given script
  (->> (get-entities-on-path entity-ref)
       (mapcat (fn [{:keys [path]}]
                 (map #(vector testrun (conj path :fixtures %))
                      (->> path (get-in testrun) :fixtures count range))))))

(defn clean-result [result]
  (try
    ;; todo - replace objects with a (pr-str) of them
    ;; this is a rough version of that, a better routine is needed
    (if (and (map? result)
             (not (element-like? result))
             (not (= (class result) clj_webdriver.driver.Driver)))
      result
      {:value (pr-str result)})

    (catch Exception e
      {:value "Exception while checking result... often an invalid WebElement"})))

(defn run-script [testrun {:keys [path]}]
  {:pre [(= (:type testrun) :testrun)
         (not (nil? path))]}
  (let [script (assoc (get-in testrun path) :path path)
        {script-str :script path :path owner :owner} script
        test-step? (= :test (:type (get-script-parent testrun path)))
        tc (get-test-context-for-ref testrun script)]
    (binding [taxi/*driver* (get-testrun-driver (:id testrun))
              *test-context* tc
              *tc* tc
              ;; only set a new cursor if the first time. This means if we are running
              ;; a script as part of the running of an initial script then we keep the
              ;; context of the initial. This matters when runner 'handler' scripts for errors etc.
              *testrun-cursor* (or *testrun-cursor* {:testrun testrun :path path})]

      ;;debug
      ;(binding [*out* *err*] (debug "Running next step" (type script-str) script-str path))

      (when (and test-step? (zero? (:index script)))
        (info "Running:" script-str ". With test-context:" *tc*))
      (let [
            _ (when test-step? (do-report {:type :before-step}))
            _ (update-testrun-record! (:id testrun) path #(assoc % :running true :test-context *tc*))
            t0 (.getTime (Date.))
            [result out] (eval-script-with-out-str script-str)
            _ (when test-step? (do-report {:type :after-step}))
            t (- (.getTime (Date.)) t0)
            _ (update-testrun-record! (:id testrun) path #(dissoc % :running))
            result (clean-result result)]
        (set-testrun-driver (:id testrun) taxi/*driver*)
        (when-not (running-in-teamcity?)                    ; hack to remove logging in teamcity
          (when-not (some #(#{:after-step :before-step} %) path) ; hack to remove logging for handlers
            (debugf "Running%s: %s" (if test-step? "" " Fixture") script-str)
            (debugf "=> Took: %f secs, Result: %s" (ms->s t) result)
            (when (not-empty out) (debug "=> Output: " (string/replace out #"\r?\n$" "")))))
        ;(debug "Updating at path" (:path script) (get-in testrun path))
        (update-testrun-record! (:id testrun) path #(assoc % :time t :result result :out out))
        result))))

(defn run-fixture-scripts [testrun path event]
  {:pre [(:type event)
         (keyword? (:type event))]}
  (when-let [script-type ((:type event) {:pass        :after-pass
                                         :fail        :after-fail
                                         :error       :after-error
                                         :before-step :before-step
                                         :after-step  :after-step})]
    (let [fixture-refs (get-fixture-refs [testrun path])
          fixtures (map #(assoc (resolve-entity %) :path (second %)) fixture-refs)
          scripts (mapcat #(get-fixture-scripts % (:path %) script-type) fixtures)]
      (binding [*in-handler?* true]
        (doseq [script scripts]
          ;(print "Running fixture-script" script-type "script:") (clojure.pprint/pprint script)
          (run-script testrun script))))))

(defn do-report [m]
  (if-let [{:keys [testrun path]} *testrun-cursor*]
    (do
      (let [id (:id testrun)
            script (get-in testrun path)
            test-step? (= :test (:type (get-script-parent testrun path)))]
        ;; this print is NOT debug... so don't remove
        (when-not (#{:before-step :after-step} (:type m))
          (info "Script event:" m script))
        ;(timeit "do-report updating script" (update-testrun-script! id path #(assoc % :events (conj (vec (:events %)) m))))
        (update-testrun-record! id path #(assoc % :events (conj (vec (:events %)) m)))
        (when (not *in-handler?*)
          (run-fixture-scripts testrun path m))))
    ;; else
    (warn "Script event (missing script):" m)))

(defn get-test-ref-for-script [[testrun path :as script-ref]]
  (->> (get-entities-on-path script-ref)
       (filter #(#{:test :testsuite} (:type %)))
       last))

(defn update-test-status [[testrun path :as script-ref]]
  ;; Update the containing test's status based on the results of any performed scripts
  ;;
  (let [test-ref (get-test-ref-for-script script-ref)
        test-path (:path test-ref)
        test (get-in testrun test-path)
        scripts (get-test-scripts (get-in testrun test-path) test-path)
        scripts (map #(assoc %
                       :script (:script %)
                       :owner (:owner %)
                       :status (if (:result %) :done :not-done)
                       :events (select-keys (->> (:events %) (map :type) frequencies)
                                            [:pass :fail :error]))
                     scripts)
        in-error? (some #(or (get-in % [:events :error])
                             (and (= :fixture (:owner %))
                                  (get-in % [:events :fail])))
                        scripts)
        all-done? (= (count scripts) (count (filter :result scripts)))
        status (if in-error? :error (if all-done? :done :not-done))]

    ;(debug "Update test" (:id test) "with status" status all-done? in-error?)
    (update-testrun-record! (:id testrun) test-path #(assoc % :status status))

    ;; notify the coordinator, if one
    ;; todo - don't do all those calcs if no one to post to....
    (timeit "post-summary"
            (let [parent-id (:id (get-script-parent testrun path))
                  fixup-setup-step (fn [step]
                                     ;; if no parent id then is a setup step ... adjust the index for reporting purposes
                                     (if parent-id
                                       step
                                       (update-in step [:index] - 100)))
                  step (get-in testrun path)
                  result (-> step
                             (select-keys [:index :script :time :status :metric-id :target-msec])
                             (fixup-setup-step)
                             (assoc :test-id (:id test)))
                  result (merge result (select-keys (->> (:events step) (map :type) frequencies)
                                                    [:pass :fail :error]))]
              (post-step-result result)))

    ;; write the testrun to file after each test completes or errors
    ;; - not too often as it can get slow...
    (when (#{:done :error} status)
      ;;todo - double check this... just noticed that it might be writing a stale version of the testrun
      (write-test-run-file testrun))))

;; OMG this is ugly....

(defn teardown? [{:keys [path]}]
  (some #(= :teardown %) path))

(defn get-next-step [{:keys [id] :as testrun}]
  {:pre [id]}
  (let [scripts (get-testrun-scripts id)
        take-script (fn [script]
                      (let [test-ref (get-test-ref-for-script [testrun (:path script)])
                            test (get-in testrun (:path test-ref))]
                        (and (not (:done script))
                             (or (not (= :error (:status test)))
                                 (teardown? script)))))]
    (first (filter take-script scripts))))

(defn- update-entity [entity]
  (debug "updating entity" entity)
  (save-entity entity)
  entity)

(defn- update-step [parent-id index expr]
  (let [entity (get-test-entity parent-id)
        steps (:steps entity)]
    (when (< index (count steps))
      (when-let [existing-expr (get-in steps [index :script])]
        (debug "Updating" parent-id "step" (get steps index) "with expression" expr)
        (update-entity (assoc entity :steps (assoc-in steps [index :script] expr)))))))

(def teamcity-context (atom {}))

(defn test-error-message [test]
  (when ((resolve 'rt.test.junit-report/test-errored?) test)
    (-> (vec ((resolve 'rt.test.junit-report/errored-steps) test))
        (concat ((resolve 'rt.test.junit-report/failed-fixture-steps) test))
        first
        pr-str)))

(defn test-failed-message [test]
  (when ((resolve 'rt.test.junit-report/test-failed?) test)
    (-> ((resolve 'rt.test.junit-report/failed-steps) test)
        first
        pr-str)))

(defn- get-test-steps-summary [testrun test-ref]
  (let [make-step-string #(left-with-elipses % 73)
        test-path (:path test-ref)
        test (get-in testrun test-path)
        scripts (get-test-scripts (get-in testrun test-path) test-path)
        scripts (map #(let [event-summary (select-keys (->> (:events %) (map :type) frequencies)
                                                       [:pass :fail :error])
                            status (cond
                                     (:running %) :running
                                     (:error event-summary) :error
                                     (:fail event-summary) :fail
                                     (:pass event-summary) :pass
                                     (:result %) :done
                                     (and (= :error (:status test))
                                          (not (teardown? %))) :skipped
                                     :else :not-done)]
                       (assoc %
                         :step (make-step-string (:script %))
                         :owner (:owner %)
                         :status status
                         :events event-summary))
                     scripts)]
    (map #(merge {:test-id (:id test)} %) scripts)))

(defn get-test-std-out [testrun path]
  (let [test (resolve-entity [testrun path])
        scripts (get-test-scripts test path)
        script-out-strs (fn [{:keys [result out script]}]
                          (concat [\newline "Step: " script]
                                  (when (and (not-empty result) (not= result {:value "nil"})) [\newline "=> Result: " result])
                                  (when (not-empty out) [\newline "=> Output: " \newline out])))]
    (str
      \newline
      (with-out-str (clojure.pprint/print-table
                      [:step :owner :index :status :time]
                      (get-test-steps-summary testrun {:path path})))
      (->> scripts
           (mapcat script-out-strs)
           (apply str)))))

(defn post-tc-message [m data]
  ;(println (format-tc-message m data))
  (post-tc-event {:type m :values data}))

(defn teamcity-test-finished []
  (let [{:keys [path testrun-id]} @teamcity-context]
    (when testrun-id
      (let [testrun (get-test-run testrun-id)
            {id :id test-path :path :as test} (get-script-test testrun path)
            error-message (str (test-error-message test) \newline (test-failed-message test))
            test-name id]

        (when test-name

          (when-not (empty? (string/trim error-message))
            (post-tc-message :testFailed {:name    test-name
                                               :message "FAIL"
                                               :details error-message}))

          (let [std-out (get-test-std-out testrun test-path)]
            (if (running-in-teamcity?)
              (post-tc-message :testStdOut {:name test-name :out std-out})
              (info "Test:" test-name " - Summary:" std-out))))

        (let [steps (->> test :steps (remove #(some-> % :steps first :script (.startsWith "(think"))))
              duration ((resolve 'rt.test.junit-report/test-time) {:steps steps})]
          (post-tc-message :testFinished {:name test-name :duration duration})
          (post-test-event {:type :test-finished :name test-name :duration duration}))))))

(defn teamcity-suite-finished []
  (let [{:keys [path testrun-id]} @teamcity-context]
    (when testrun-id
      (let [testrun (get-test-run testrun-id)
            {suite-id :id} (get-script-suite testrun path)
            suite-name suite-id]
        (post-tc-message :testSuiteFinished {:name suite-name})))))

(defn teamcity-step [testrun path]
  (let [{last-path :path} @teamcity-context
        {last-test-id :id} (get-script-test testrun last-path)
        {last-suite-id :id} (get-script-suite testrun last-path)
        {id :id} (get-script-test testrun path)
        {suite-id :id} (get-script-suite testrun path)
        test-name id
        suite-name suite-id]

    (when test-name
      (when-not (= last-test-id id)
        (teamcity-test-finished)

        (when-not (= last-suite-id suite-id)
          (teamcity-suite-finished)
          (post-tc-message :testSuiteStarted {:name suite-name}))

        (post-tc-message :testStarted {:name test-name})
        (post-test-event {:type :test-started :name test-name})

        (swap! teamcity-context #(assoc % :testrun-id (:id testrun) :path path))))))

(defn teamcity-start []
  (reset! teamcity-context {}))

(defn teamcity-finish []
  (teamcity-test-finished)
  (teamcity-suite-finished)
  (reset! teamcity-context nil))

(defn do-next-step [id]
  (let [testrun (get-test-run id)
        next-script (get-next-step testrun)]

    (when-let [{:keys [path]} next-script]

      ;(print "Running: ") (pprint next-script)
      ;(debug "Running with app settings: " (rt.setup/get-settings))

      (let [result (run-script testrun next-script)
            script (update-testrun-record! id (:path next-script) #(assoc % :done true))]

        ;;(clojure.pprint/pprint script)
        (update-test-status [(get-test-run id) (:path next-script)])

        (teamcity-step (get-test-run id) path)

        ;; return the current value of the script, plus its path
        (assoc script :path (:path next-script))))))

(defn do-next-step-with-expr [id expr save?]
  (let [testrun (get-test-run id)
        next-script (get-next-step testrun)]

    (when-let [{:keys [path index]} next-script]

      (let [{parent-id :id parent-type :type} (get-script-parent testrun path)]

        (debug "do-next-step-with-expr: script parent" parent-id parent-type)
        (debug "do-next-step-with-expr: script index" index)

        (when (and save? parent-id)
          (update-step parent-id (:index next-script) expr)))

      (debug "do-next-step-with-expr: replacing expr in script" (:script next-script) "=>" expr)
      (update-testrun-record! id path #(assoc % :script expr))

      (let [testrun (get-test-run id)
            next-script (assoc (get-in testrun path) :path path)]

        (debug "Running with app settings" (rt.setup/get-settings))

        (let [result (timeit "run-script" (run-script testrun next-script))
              script (update-testrun-record! id path #(assoc % :done true))]

          #_(clojure.pprint/pprint script)
          (update-test-status [(get-test-run id) path])

          ;; return the current value of the script, plus its path
          (assoc script :path path))))))

(defn reset-last-step
  ([] (reset-last-step (get-last-test-run-id)))
  ([id]
   (let [scripts (get-testrun-scripts id)
         last-script (last (filter :done scripts))]
     (infof "Resetting last script: %s" (:script last-script))
     (when last-script
       (let [script (update-testrun-record! id (:path last-script)
                                            #(-> %
                                                 (assoc :done false)
                                                 (dissoc :result :out :time :events)))]
         (update-test-status [(get-test-run id) (:path last-script)])
         script)))))

(defn reset-last-test
  ([] (reset-last-test (get-last-test-run-id)))
  ([id]
   (let [scripts (get-testrun-scripts id)
         last-script (last (filter :done scripts))
         get-test-id #(:id (get-script-test (get-test-run id) (:path %)))
         test-id (get-test-id last-script)]
     (infof "Resetting all steps for test: %s" test-id)
     (when test-id
       (while (= test-id (get-test-id (reset-last-step id)))
         )))))

(defn script-has-error? [script]
  (some #(#{:error :fail} (:type %)) (:events script)))

(defn run-to-end
  ([] (run-to-end (get-last-test-run-id)))
  ([id]
   (debug "Running with app settings: " (get-settings))
   (let [test-retry-limit (or (:test-retry-limit (get-settings)) 0)
         stop-on-error (:stop-on-error (get-settings))]

     (loop [c (do-next-step id)]
       (when c
         (let [{:keys [retries status path] :or {retries 0}} (get-script-test (get-test-run id) (:path c))]
           ;; if we have an error and we have retries available then reset the test
           (when (and (= :error status) (< retries test-retry-limit))
             (reset-last-test id)
             (update-testrun-record! id path #(assoc % :state :not-done :retries (inc retries))))
           ;; run the next unless have an error and stop-on-error is on and we have no retries
           (when (not (and stop-on-error (= :error status) (>= retries test-retry-limit)))
             (recur (do-next-step id)))))))

   (write-test-run-file (get-test-run id))
    ;; not doing the pprint thing - takes time and space and no one has used them in the past year
    #_(write-pprint-test-run-file (get-test-run id))))

(defn run-to-error
  ([] (run-to-error (get-last-test-run-id)))
  ([id]
   (debug "Running with app settings: " (rt.setup/get-settings))
   (loop [c (do-next-step id)]
     (when (and c (not (script-has-error? c)))
       (recur (do-next-step id))))
   (timeit "write-testrun-file" (write-test-run-file (get-test-run id)))
    ;; don't do pprint version... takes too long
   (debug "NOT writing pretty printed output (slow). If you want that then do (run-to-end).")
    #_(timeit "write-pprint-testrun-file" (write-pprint-test-run-file (get-test-run id)))))

(defn run-to-step-impl
  ([path] (run-to-step-impl (get-last-test-run-id) path))
  ([id path]
   (debug "running to step:" path)
   (loop [c (do-next-step id)]
     (debug "just ran step:" (select-keys c [:script :path]) path (not= path (:path c)))
     (when (and c (not= path (:path c)))
       (recur (do-next-step id))))
   (timeit "write-testrun-file" (write-test-run-file (get-test-run id)))
    ;; don't do pprint version... takes too long
   (debug "NOT writing pretty printed output (slow). If you want that then do (run-to-end).")
    #_(timeit "write-pprint-testrun-file" (write-pprint-test-run-file (get-test-run id)))))

(defn get-scripts-summary [testrun]
  {:pre [(= (:type testrun) :testrun)]}
  (->> (get-testrun-scripts (:id testrun))
       (map (fn [s] (assoc {:script (:script s) :time (:time s)}
                      :error (count (filter #(= :error (:type %)) (:events s)))
                      :fail (count (filter #(= :fail (:type %)) (:events s)))
                      :pass (count (filter #(= :pass (:type %)) (:events s))))))))

(defn get-run-summary [testrun]
  {:pre [(= (:type testrun) :testrun)]}
  (let [id (:id testrun)
        scripts (get-scripts-summary testrun)
        tests (count (get-test-refs testrun))
        sum-counter (fn [get-counter-fn]
                      (reduce #((fnil + 0 0) %1 (get-counter-fn %2)) 0 scripts))
        errors (sum-counter :error)
        fails (sum-counter :fail)
        passes (sum-counter :pass)
        time (sum-counter :time)]
    {:id id :tests tests :errors errors :fails fails :passes passes :time time}))

(defn get-run-suite-summaries
  "Generate a map per suite with each map including the times for each test step, the
  times keyed by the step :metric-id. Also include important test context data."
  [id]
  (let [{:keys [:tests :created] :as testrun} (get-test-run id)
        test-refs (get-test-refs testrun)
        suites (distinct (map :suite-id-str test-refs))
        tests (map #(get-in testrun (:path %)) test-refs)
        app-context (->> test-refs
                         (map (partial get-test-context-for-ref testrun))
                         (filter #(-> % :app :client-version))
                         (map #(-> % :app))
                         first)
        get-script (fn [suite test script] (merge {:test-id (-> test :id)
                                                   :suites  #{suite}}
                                                  (select-keys script [:script :metric-id :time])))
        test-steps (fn [test-ref]
                     (let [test (get-in testrun (:path test-ref))]
                       (map #(get-script (:suite-id-str test-ref) test-ref %) (mapcat :steps (:steps test)))))
        steps (mapcat test-steps test-refs)
        ;; we can either skip those without an explicit :metric-id or we can generate one for them
        ;; - for the moment choosing the former
        steps (filter :metric-id steps)
        ;steps (map-indexed #(assoc %2 :metric-id (or (:metric-id %2) (str "step" %1))) steps)
        suite-summary (fn [suite]
                        (->> steps
                             (filter #((:suites %) suite))
                             (reduce #(assoc %1 (:metric-id %2) (:time %2))
                                     (merge {:run-id id :run-date (csv-datetime-str created) :suite suite} app-context))))]
    (map suite-summary suites)))

(defn write-run-suite-summary-csv
  ([file-name] (write-run-suite-summary-csv file-name (get-last-test-run-id)))
  ([file-name id] (doseq [summary (get-run-suite-summaries id)]
                    (let [suite-id (-> summary :suite (string/replace #"[\:\/\\]+" "-"))]
                      (write-csv-objects (str file-name "-" suite-id ".csv") [summary])))))

(defn get-test-id-str []
  (when-let [{:keys [testrun path]} *testrun-cursor*]
    (id->str (:id (get-script-parent testrun path)))))

(defn make-artifact-file-path []
  (if-let [{:keys [testrun path]} *testrun-cursor*]
    (let [{:keys [index]} (rt.test.core/resolve-entity [testrun path])
          test-id (id->str (:id (get-script-test testrun path)))
          name (clean-filename (str (format "%03d" (next-file-count)) "-" test-id "-step_" index))
          path (str (:dir-name testrun) "/" name)]
      path)
    ;; else
    (str (next-file-count) "-unknown")))

(defn get-test-steps-as-source [id & [steps-list-key]]
  (let [steps-list-key (or steps-list-key :steps)
        test (rt.test.core/get-test-record (rt.test.db/get-test-entity id))]
    (condp = steps-list-key
      :steps (->> test
                  :steps
                  (mapcat :steps)
                  (map :script)
                  (interpose \newline)
                  (apply str))
      :setup (->> test
                  :fixtures
                  (mapcat :setup)
                  (mapcat :steps)
                  (map :script)
                  (interpose \newline)
                  (apply str))
      :teardown (->> test
                  :fixtures
                  (mapcat :teardown)
                  (mapcat :steps)
                  (map :script)
                  (interpose \newline)
                  (apply str)))))

;; legacy

(defn prepare-test-run [tests]
  (apply create-testrun tests))

(defn start-test-run [tests]
  (set-current-test-run-id! (apply create-testrun tests)))

(defn start-partial-test-run [suite test]
  (when-let [id (create-partial-testrun [suite] (filter identity [test]))]
    (set-current-test-run-id! id)))

(defn run-all-tests [id]
  (teamcity-start)
  (run-to-end id)
  (teamcity-finish))

(defn run-next-step [id]
  (let [step (do-next-step id)]
    {:testrun (get-test-run id) :path (:path step)}))

(defn run-next-step-with-expr [id expr save?]
  (let [step (do-next-step-with-expr id expr save?)]
    {:testrun (get-test-run id) :path (:path step)}))

(defn rerun-last-step [id]
  (reset-last-step)
  (let [step (do-next-step id)]
    {:testrun (get-test-run id) :path (:path step)}))

(defn reset-last [id]
  (reset-last-step)
  (let [testrun (get-test-run id)
        {:keys [path]} (get-next-step testrun)]
    {:testrun testrun :path path}))

(defn run-until-error [id]
  (let [step (run-to-error id)]
    (when step
      {:testrun (get-test-run id) :path (:path step)})))

(defn run-to-step [id path]
  (let [step (run-to-step-impl id path)]
    (when step
      {:testrun (get-test-run id) :path (:path step)})))

(defn run-tests [tests]
  (let [id (create-testrun tests)]
    (run-all-tests id)))

(comment

  (.printStackTrace *e)

  (do
    (rt.repl/reset-last)
    (rt.repl/run-next-with-expr "(expect-equals 1 2)" false))


  (do
    (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
    (rt.app/setup-environment)
    (create-testrun :steve)
    (create-partial-testrun [:steve] [:steve/test/fb])

    (create-testrun :f)
    (run-next-step (get-last-test-run-id))
    (reset-last-step)

    (run-to-error)

    (do
      (rt.setup/update-settings {:from-test-index 10 :max-test-count 2})
      (create-testrun :rn/suites/regression-chrome)
      (map :id (-> (get-last-test-run) :tests first :tests)))

    (clojure.pprint/pprint (get-test-run (get-last-test-run-id)))

    (clojure.pprint/pp)
    (get-entities-on-path [(get-last-test-run) [:tests 0 :fixtures 0]])

    (map #(merge (select-keys % [:script :path])
                 {:parent (last (get-entities-on-path [(get-last-test-run)
                                                       (butlast (:path %))]))})
         (get-testrun-scripts))

    (clojure.pprint/print-table
      [:script :done :result]
      (map #(select-keys % [:script]) (get-testrun-scripts)))

    (clojure.pprint/pprint (count (get-testrun-scripts)))

    (get-entity-map (get-last-test-run) [])

    (let [testrun (get-last-test-run)
          testrun-map (get-entity-map testrun [])]
      (->> testrun-map
           (map #(hash-map :path %1 :entity (get-in testrun %1)))
           (filter #(#{:test} (-> % :entity :type)))
           (map #(-> % :entity :id))))

    (do-next-step)
    (dissoc (do-next-step) :out)
    (reset-last-step)
    (run-to-end)

    (clojure.pprint/pp)

    (do
      (rt.app/setup-environment)
      (prepare-test-run [:steve])
      (def test-run-id (get-last-test-run-id))
      (def run (get-test-run test-run-id))
      (clojure.pprint/pprint run)
      ))

  (do
    (def r1 (create-testrun :perf/tests/t0))
    (def r2 (create-testrun :perf/tests/t0))
    (clojure.pprint/pprint (get-test-run r1))
    (->> (get-test-run r1)
         (clojure.pprint/pprint))
    (-> (get-test-run r2)
        (clojure.pprint/pprint))
    (println (rt.test.junit-report/suites-xml (get-test-run r2)))
    (run-next-step r1)
    (run-next-step r2))

  (do
    ;(rt.repl/reset)
    (def test-run-id (get-last-test-run-id))
    (get-run-suite-summaries test-run-id)
    #_(write-run-suite-summary-csv "suitemetrics" test-run-id))

  (do
    (rt.app/setup-environment)
    (prepare-test-run [:samples/test-1])
    (def test-run-id (get-last-test-run-id))
    (def run (get-test-run test-run-id))
    (clojure.pprint/pprint run)

    (run-next-step test-run-id)
    )

  (do
    (rt.setup/update-settings {:default-fixtures []})
    (init-test-db)
    (init-test-runs-db)
    (add-tests [{:id    :script1
                 :type  :testscript
                 :steps ["(println \"hey from script1\")"]}
                {:id    :script2
                 :type  :testscript
                 :steps ["(println \"before calling script1\")"
                         :script1
                         "(println \"after calling script1\")"]}
                {:id    :fixture1
                 :type  :testfixture
                 :data  {:tenant "EDC"}
                 :setup ["(println \"fixture1 setup\")"
                         :script1
                         "(defn new-driver-fn []
                         (println \"hey from new driver\"))"]}
                {:id       :fixture2
                 :type     :testfixture
                 :teardown ["(println \"fixture2 teardown\")"]}
                {:id       :t1
                 :type     :test
                 :data     {:tenant "EDC"}
                 :setup    ["(println \"SETUP SCRIPT\")"
                            "(identity {:a 1 :b 2})"
                            ;; scripts in script not yet supported
                            ;:script2
                            ]
                 :teardown ["(println \"TEARDOWN STEP 1\")"
                            "(println \"TEARDOWN STEP 2\")"]
                 :steps    [{:script "(println \"STEP 1\")" :doc "first step in my test"}
                            :script1
                            "(expect-equals 1 (rt.scripts.samples/sample-test-query-1 \"STEP 4\"))"
                            "(expect-equals 1 (:last-result *test-context*))"
                            "(rt.scripts.samples/sample-script-that-throws)"
                            "(expect-equals 1 2)"]}
                {:id       :t2
                 :type     :test
                 :fixtures [:fixture1 :fixture2]
                 :steps    ["(println \"STEP a\")"
                            "(new-driver-fn)"
                            "(rt.scripts.samples/sample-test-script-1 \"STEP b\")"
                            "(rt.scripts.samples/sample-test-query-1 \"STEP c\")"
                            "(expect-equals 1 1)"]}
                {:id       :s1
                 :name     "suite1"
                 :type     :testsuite
                 :tests    [:t1 :t2]
                 :teardown []}]))

  (get-test-entity :t2)

  (do-next-step (get-last-test-run-id))

  (do
    (prepare-test-run [:s1])
    (run-all-tests (get-last-test-run-id)))

  (println (rt.test.junit-report/suites-xml (get-last-test-run)))

  (get-run-summary (get-test-run test-run-id))

  (write-run-metrics-csv "test-times.csv" test-run-id)


  (clojure.pprint/pp)
  (clojure.pprint/pprint @rt.test.db/test-runs-db)

  (spit "test-run-db.edn" (with-out-str (clojure.pprint/pprint @test-runs-db)))
  (spit "test-db.edn" (with-out-str (clojure.pprint/pprint @test-db)))

  )