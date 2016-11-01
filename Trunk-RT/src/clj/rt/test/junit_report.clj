(ns rt.test.junit-report
  "Product a junit format report of a testrun"
  (:require [rt.lib.util :refer [datetime-str indices deep-merge ms->s csv-datetime-str write-csv-objects
                                 mapcat-indexed]]
            [rt.test.db :refer [get-test-run get-last-test-run-id get-last-test-run]]
            [rt.test.core :refer [get-entity-map get-testrun-entities get-test-context-for-ref get-test-scripts get-test-refs
                                  get-run-summary]]
            [clojure.data.xml :as xml]))

;; todo test scripts within scripts

(defn id->str [id]
  (let [s (str id)]
    (if (.startsWith s ":") (subs s 1) s)))

(defn get-tests [test]
  (cons test (mapcat get-tests (:tests test))))

(defn get-test-cursors
  ([test] (get-test-cursors test []))
  ([test path]
   (cons path (mapcat-indexed #(get-test-cursors %2 (conj path :tests %1)) (:tests test)))))

(defn suites-in-run [run]
  (filter #(#{:testsuite} (:type %)) (get-tests run)))

(defn scripts-with-counter [counter {:keys [scripts]}]
  (filter #(counter (:counters %)) scripts))

(defn has-script-with-counter [counter {:keys [scripts]}]
  (some #(counter (:counters %)) scripts))

(defn failed-fixture-steps [test]
  (->> (:fixtures test)
       (mapcat #(concat (:setup %) (:teardown %)))
       (mapcat :steps)
       (mapcat :events)
       (filter #(#{:fail :error} (:type %)))))

(defn failed-steps [test]
  (->> (:steps test)
       (mapcat :steps)
       (mapcat :events)
       (filter #(= :fail (:type %)))))

(defn errored-steps [test]
  (->> (:steps test)
       (mapcat :steps)
       (mapcat :events)
       (filter #(= :error (:type %)))))

(defn has-failed-fixture? [test]
  (first (failed-fixture-steps test)))

(defn has-failed-step? [test]
  (first (failed-steps test)))

(defn has-errored-step? [test]
  (first (errored-steps test)))

(defn test-failed? [test]
  (or (has-failed-step? test)))

(defn test-errored? [test]
  (or (has-failed-fixture? test) (has-errored-step? test)))

(defn total-scripts-time [scripts]
  (reduce #((fnil + 0 0) %1 (:time %2)) 0 scripts))

(defn test-time [{:keys [steps]}]
  (->> (mapcat :steps steps)
       total-scripts-time))

(defn total-tests-time [tests]
  (reduce #((fnil + 0 0) %1 (test-time %2)) 0 tests))

(defn script-xml [{:keys [script time doc target-msec owner events metric-id index]}]
  (xml/element :script (merge {:script    script
                               ;:doc    (or doc script)
                               :time      (ms->s time)
                               :source    (id->str owner)
                               :metric-id metric-id
                               :index     index}
                              (select-keys (->> events (map :type) frequencies)
                                           [:pass :fail :error])
                              (if target-msec {:target-time (ms->s target-msec)} {}))))

(defn scripts-summary [testrun test-ref]
  (let [path (:path test-ref)
        scripts (get-test-scripts (get-in testrun path) path)]
    (apply xml/element :scripts {} (map script-xml scripts))))

(defn get-test-context [testrun test-ref]
  (get-test-context-for-ref testrun test-ref))

(defn test-xml [testrun test-ref]
  {:pre [(= (:type testrun) :testrun)
         (:path test-ref)]}
  (let [{test-id :id :as test} (get-in testrun (:path test-ref))]
    (xml/element :testcase (merge {:id        test-id
                                   :name      test-id
                                   :classname ""            ; (:name test)
                                   :time      (ms->s (test-time test))}
                                  (when (:retries test) {:retries (:retries test)}))
                 (when (test-errored? test)
                   (xml/element :error {:message (-> (vec (errored-steps test))
                                                     (concat (failed-fixture-steps test))
                                                     first
                                                     pr-str)}))
                 (when (test-failed? test)
                   (xml/element :failure {:message (-> (failed-steps test)
                                                       first
                                                       pr-str)}))
                 (xml/element :system-out {}
                              (xml/element :context {} (pr-str (get-test-context testrun test-ref)))
                              (scripts-summary testrun test-ref))
                 ;; might put this node back in at some point, when have something to put in there
                 #_(xml/element :system-err {}))))

(defn suite-xml [testrun suite-id-str]
  {:pre [(= (:type testrun) :testrun)]}
  (let [test-refs (->> (get-test-refs testrun)
                       (filter #(= suite-id-str (:suite-id-str %))))
        tests (map #(get-in testrun (:path %)) test-refs)]
    (apply xml/element :testsuite
           {:id        suite-id-str
            :name      suite-id-str
            :timestamp (:created testrun)
            :tests     (count tests)
            :errors    (count (filter test-errored? tests))
            :failures  (count (filter test-failed? tests))
            :time      (ms->s (total-tests-time tests))}
           (conj (mapv (partial test-xml testrun) test-refs)
                 (xml/element :system-out {}
                              (str "see " (:dir-name testrun) " folder on test machine"))
                 (xml/element :system-err {})))))


(defn suites-xml [testrun]
  {:pre [(= (:type testrun) :testrun)]}
  (let [run-summary (get-run-summary testrun)
        test-refs (get-test-refs testrun)
        suites (distinct (map :suite-id-str test-refs))
        tests (map #(get-in testrun (:path %)) test-refs)]
    (xml/indent-str
      (apply xml/element :testsuites
             {:name     ""
              :tests    (count test-refs)
              :disabled 0
              :errors   (count (filter test-errored? tests))
              :failures (count (filter test-failed? tests))
              :time     (ms->s (:time run-summary))}
             (map (partial suite-xml testrun) suites)))))


(defn write-junit-report [id]
  (when-let [{:keys [dir-name] :as testrun} (get-test-run id)]
    (let [xml-str (suites-xml testrun)]
      ;; write the file to the testrun folder
      (spit (str dir-name "/junit-results.xml") xml-str)
      ;; writing the file to the current folder for convenient access to the latest results
      (spit "junit-results.xml" xml-str))))

(comment
  (rt.app/setup-environment)
  (clojure.pprint/pp)

  (suites-xml (get-last-test-run))

  (distinct (map :suite-id (get-test-refs (get-last-test-run))))

  (get-run-summary (get-last-test-run))
  (map :id (suites-in-run (get-last-test-run)))

  (spit "junit.xml" (suites-xml (get-last-test-run)))

  (let [testrun (get-last-test-run)
        testrun-map (get-entity-map testrun [])]
    (->> testrun-map
         (map #(hash-map :path %1 :entity (get-in testrun %1)))
         (filter #(#{:test} (-> % :entity :type)))
         (map #(-> % :entity :id))))

  (let []
    (->> (get-last-test-run-id)
         get-test-run
         get-tests
         (map #(select-keys % [:id :type]))))

  (let [id (get-last-test-run-id)
        r (get-test-run id)]
    (->> (get-test-cursors r)
         (map #(assoc {:path %} :test (get-in r (conj % :id))))))

  )
