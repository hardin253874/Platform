(ns rt.scripts.samples
  (:require rt.scripts.common
            [rt.test.expects :refer [expect expect-equals]]
            [rt.test.db :refer [add-tests add-tests-for-scripts-in-ns]]))

;; some dummy scripts and tests for debugging

;; the following shows loading scripts as tests cases based on function meta

(defn dummy-script-1
  "Ignore this."
  {:testcase true}
  []
  (expect-equals true true))

(defn dummy-script-2
  "Ignore this."
  {:testcase true}
  []
  (expect-equals true false))

;; the following are some scripts used by the test cases in tests/sample.edn

(defn sample-test-script-1 [& args]
  (Thread/sleep 200)
  (println "I know nothing" args))

(defn sample-test-query-1 [& [args]]
  (Thread/sleep 100)
  (println "This is what you say" args)
  args)

(defn sample-script-that-throws []
  (println "about to throw")
  (throw (Exception. "arrrr")))

(defn init []
  #_(add-tests-for-scripts-in-ns 'rt.scripts.samples {} {:id :dummy-suite}))

