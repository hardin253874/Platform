(ns rt.test.runner
  (:import (java.util.concurrent ConcurrentSkipListSet)))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; playing around with some new test entity model concepts

;; Concepts

;; ReadiTest Entity Model
;; There exists a ReadiTest entity model of testsuite, testfixture, test, testscript
;; Suites have Fixtures and Tests
;; Tests have Fixtures and Tests
;; Fixtures are data and Scripts and of type "once" or "each"
;; Scripts are a clojure expression plus meta

;; ReadiTest script and app driver functions
;; Plain clojure functions with certain metadata.

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; helper functions

;; placeholder
(defn expect-equals [a b])

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; app driver functions
;; - plain clojure code
;; - do things and get things
;; - not test framework aware
;; - throw Exceptions when things go wrong (as opposed to test expectations)
;; - may operate on the external environment but do not modify
;; the state of ReadiTest itself
;; - typically exist in own namespaces, or better in a clojure lib that can
;; be versioned along side the target application it is "driving"

(defn action-1 []
  (println "do action-1"))

(defn action-2 [value]
  (println "do action-2 with value" value))

(defn query-1 []
  "Some value")

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; script functions
;; - plain clojure code
;; - may be annotated with same attributes as the testscript entities
;; - is test framework aware so can use the expectations library and
;; access the test context

(defn script-1 []
  (action-1)
  (expect-equals "Some value" (query-1)))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; ReadiTest Types

;; script-call 'type'
;; - used in various entities to define a call to a script and attributes
;; relating to that call.
;; - the script-call is either clojure code, or a list with the
;; function name plus scalar argument values to pass, or a keyword
;; that is the id of a script entity
;; - additional attributes are doc strings, and profiling related options
;; such as target times and whether to wait

;; todo - add a core.typed type for this

;;; ReadiTest Entity Model

;; testscript entities
;; - scripts allow QA to package reusable sequences of actions and queries
;; for use in tests, or other scripts.
;; - the steps list contains script-calls
;; - at present these scripts don't take parameters, but instead
;; should use data defined in the test context *tc* which can be
;; populated by any script called as part of a test "setup".
;; - to call any driver or script "function" (or any clojure function)
;; you just call it: e.g. (my-function arg1 arg2)
;; - to call script defined as a testscript entity you call it via the
;; test runner: e.g. (call-script :my-script-id), or just provide the script id

(def sample-testscripts
  [{:id    :samples/script-1
    :type  :script
    :steps ["(println \"this is a plain code step\")"
            {:script "(println \"this is a plain code step\")" :doc "step 2"}
            {:script "(action-1)"}]}

   {:id    :samples/script-2
    :type  :script
    :steps [:samples/script-1
            "(println \"this test is done\")"]}])


;; testfixture entities
;; - to talk about these....

(def sample-testfixtures
  [{:id       :samples/chrome
    :type     :fixture
    :data     {:target :chrome}
    :setup    []
    :teardown []}

   {:id       :samples/default-login
    :type     :fixture
    :data     {:tenant "EDC"}
    :setup    ["(identity {:username \"Administrator\" :password \"tacoT0wn\"})"
               "(rn/start-app-and-login"]
    :teardown []}])

;; tests
;; - Use these for test cases and test scenarios, the distinction being in the
;; intent, scope and granularity of what is being tested. This is discussed elsewhere.

(def sample-tests
  [{:id      :samples/test-1
    :type    :test
    :fixture [:samples/chrome]}])

;; suites
;; list of fixtures
;; list of tests or suites to be recursively included
;; tests are performed in their suite groupings
;; tests can be perfomred in any order within their suite
;; (and we should randomise this to avoid accidental order dependencies)

(def sample-testsuites
  [{:id    :samples/suite-1
    :type  :suite
    :once  [:samples/chrome
            :samples/default-login]
    :tests [:samples/test-1]}])


;; our database is an atom of a map keyed by the entity ids.

(def test-db (atom {}))

(defn get-test-tb [] @test-db)

(defn upsert-entity [{:keys [id] :as e}]
  {:pre [(keyword? id)]}
  (swap! test-db (fn [test-db] (assoc test-db id e)))
  e)


;; need crud functions on our entities

(defn delete-entity [id]
  (throw (Exception. "not implemented")))