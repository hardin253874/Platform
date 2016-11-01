(ns rt.scripts
  "The scripts namespace contains handwritten scripts for use in test cases,
  and in some cases the definition of tests based on the scripts."
  (:require [rt.test.core :refer [*test-context* *tc* *testrun-cursor* resolve-entity
                                  get-script-parent merge-tc]]
            [rt.test.expects :refer [expect expect-equals expect-not
                                     expect-contains expect-match expect-contains-match
                                     expect-max-time]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser]]
            [rt.setup :refer [get-settings set-test-data setonce-test-data get-test-data get-random-test-data]]
            [rt.lib.util :refer :all]
            [clj-webdriver.taxi :as taxi]
            [clj-time.core]
            [clj-time.coerce]
            [clojure.set :refer [subset? intersection]]))

(defn has-tags? [tags test]
  (let [tags (map #(if (string? %) (keyword %) %) tags)]
    (subset? (set tags) (set (:tags test)))))

(defn has-some-tags? [tags test]
  (let [tags (map #(if (string? %) (keyword %) %) tags)]
    (not (empty? (intersection (set tags) (set (:tags test)))))))

(defn has-matching-step? [re test]
  (->> test :steps (map #(get-in % [:script])) (some #(some->> % (re-find (re-pattern re))))))

(defn matching-step-count [re test]
  (->> test :steps (map #(get-in % [:script]))
       (filter #(some->> % (re-find (re-pattern re))))
       count))

(defn has-index-in-range? [from to {:keys [index]}]
  (let [from (or from 0)
        to (or to -1)]
    (or (not index)
        (and (>= index from) (or (< to 0) (<= index to))))))

(defn get-random-tc-item
  "Given a key or key sequence to a list of items in the test context *tc*
  return a random item from that list."
  [ks]
  (->> (if (vector? ks) ks [ks])
       (get-in *tc*)
       rand-nth))

(defn make-local-date [y m d]
  (-> (clj-time.core/date-time y m d)
      clj-time.coerce/to-local-date-time
      clj-time.coerce/to-date))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Some functions to help coordinate multiple RT test runs
;; - bit of a hack to get some concurrency tests going quickly
;; - impl to be revisted
;; Assumes a single instance of RT running as "server"

(defn- get-server-context-url []
  (str (or (:server (get-settings)) "http://localhost:3000") "/api/context"))

(defn- sync-disabled? []
  (:disable-step-sync (get-settings)))

(defn- set-stage [n]
  (when-not (sync-disabled?)
    (try
      (clj-http.client/post (get-server-context-url)
                            {:form-params  {:stage-number n}
                             :content-type :json})
      (catch Exception _
        ;; ignore
        ))))

(defn- get-stage []
  (when-not (sync-disabled?)
    (try
      (-> (clj-http.client/get (get-server-context-url))
          :body
          clojure.data.json/read-json
          :stage-number
          (or 0))
      (catch Exception ex
        ;; ignore
        (println "exception in get-stage" (get-server-context-url) "ex=" ex)
        -1))))

(defn- wait-stage [n & [{:keys [timeout] :or {timeout 5}}]]
  (when-not (sync-disabled?)
    (loop [time-remaining (* timeout 1000)]
      (let [stage (get-stage)]
        (when (and (< stage n) (>= stage 0) (> time-remaining 0))
          (Thread/sleep 100)
          (recur (- time-remaining 100)))))
    (let [stage (get-stage)]
      (when-not (>= stage n)
        (println "Auto-advancing stage from" stage "=>" n)
        (set-stage n))))
  (get-stage))

(defn- get-step-index []
  (if-let [{:keys [testrun path]} *testrun-cursor*]
    (:index (rt.test.core/resolve-entity [testrun path]))
    0))

(defn think
  "Pause a little to represent \"think time\".
  Call with number of seconds to pause or a range for some randomness.
  Examples:
  (think 5) ;; think for 5 seconds
  (think 30 60) ;; think somewhere between 30 and 60 seconds"
  ([t] (when-not (:no-thinking (get-settings))
         (Thread/sleep (* 1000 t))))
  ([t1 t2] (think (/ (+ (* t1 1000) (rand-int (* (- t2 t1) 1000))) 1000))))

(defn rendezvous
  "A basic mechanism to rendezvous with other sessions running scripts.
  TODO - be able to wait on a more interesting key rather than the step index
  which only really works if all are running the same script."
  [& [{:keys [timeout] :or {timeout 30}}]]
  (wait-stage (get-step-index) {:timeout timeout}))

(defn reset-rendezvous "see rendezvous"
  []
  (set-stage 0))

