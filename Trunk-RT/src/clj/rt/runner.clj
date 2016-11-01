(ns rt.runner
  (require [rt.session :as session]
           [rt.test.db :as db]
           [rt.lib.util :refer [id->str]]
           [clojure.pprint :refer [pp pprint print-table]]))

(defn pred [p v]
  (let [s (read-string (str \# p))]
    (binding [*ns* (find-ns 'rt.user)]
      (eval `(apply ~s ~v)))))

(defn get-suite-tests [{:keys [tests test-filter]}]
  ;; Grab both the static defined tests and any based on the optional filter.
  (let [test-ids (set (map :id tests))]
    (map db/get-test-entity
         (concat tests
                 (when test-filter
                   (->> (db/get-test-list :test)
                        (filter #(pred test-filter [%]))
                        (map :id)
                        (remove #(test-ids %))
                        (sort)))))))

(defn run-test [{session-id :id :as session} test & [context]]
  {:pre [(map? session) session-id test]}
  (session/set-session-test! session-id test context)
  (session/raise-event! session-id {:type :beforeTest :test test :context context})
  (session/print-session-steps (session/get-session session-id))
  @(session/submit! session-id session/run-test!)
  (session/raise-event! session-id {:type :afterTest :test test :context context}))

(defn fixup-suite [suite]
  ;; temp while hooking this all up
  (-> suite
      (assoc :fixtures (map db/get-test-entity (concat (:once-fixtures suite)
                                                       (:each-fixtures suite))))
      (dissoc :once-fixtures :each-fixtures)))

(defn run-suite [{session-id :id :as session} suite]
  (let [suite (fixup-suite suite)
        suite-id (id->str (:id suite))]
    ;; The session knows how to invoke event handlers in the right context
    ;; so configure it for this suite and raise the suite events.
    (session/set-session-test! session-id nil {:suites [suite]})
    (session/raise-event! session-id {:type :beforeSuite :suite suite})
    (doseq [test (get-suite-tests suite)]
      (run-test session test {:suites [suite]}))
    (session/raise-event! session-id {:type :afterSuite :suite suite})))

;; testing

(defn do-test [options]
  (do
    (db/init-test-db)
    (db/add-tests [session/sample-test])
    (db/add-tests session/sample-fixtures)
    (db/add-tests session/sample-scripts)
    (db/add-tests [{:id          :suite-1
                    :type        :testsuite
                    :fixtures    session/sample-fixtures
                    :tests       [:my-test-1]
                    :test-filter (pr-str '(has-tags? [:prod] %))}])
    (map :id (vals (db/get-test-db))))

  (run-suite (session/get-recent-session) (db/get-test-entity :suite-1)))

(comment
  (.printStackTrace *e)

  (do-test {})
  (session/print-session-steps (session/get-recent-session))

  (do
    (db/init-test-db)
    (db/add-tests [session/sample-test])
    (db/add-tests session/sample-fixtures)
    (db/add-tests session/sample-scripts)
    (db/add-tests [{:id          :suite-1
                    :type        :testsuite
                    :fixtures    session/sample-fixtures
                    :tests       [:my-test-1]
                    :test-filter (pr-str '(has-tags? [:prod] %))}])
    (map :id (vals (db/get-test-db))))

  (run-test (session/get-recent-session) session/sample-test {:fixtures session/sample-fixtures})
  (session/print-session-steps (session/get-recent-session))

  (run-suite (session/get-recent-session) (db/get-test-entity :suite-1))
  (session/print-session-steps (session/get-recent-session))

  (rt.repl/reset)
  (rt.repl/list-tests "steve")
  (rt.repl/init-test-run :steve)

  (fixup-suite (db/get-test-entity :steve))
  (get-suite-tests (db/get-test-entity :steve))
  (session/print-session-steps (session/get-recent-session))
  (run-suite (session/get-recent-session) (db/get-test-entity :steve))

  (pp)
  )
