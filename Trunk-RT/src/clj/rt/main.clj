(ns rt.main
  "The main namespace for ReadiTest. Handles the CLI interface"
  (:require rt.lib.wd
            [rt.lib.util :refer [->int get-browser-position timeit
                                 checked-csv->list ->ids csv->list ensure-keyword]]
            [rt.setup :refer [get-settings get-target get-app-url
                              get-version running-in-teamcity? init-logging]]
            [rt.test.db :refer [get-test-list]]
            rt.test.core
            rt.test.junit-report
            [rt.app :refer [setup-environment setup-environment-with-wait]]
            rt.repl
    ;; need rt.server to cause it to load, even though we don't refer to it here
            rt.server
            rt.user
            rt.agent
            [rt.test.concurrent :refer [request-test-from-server request-options-from-server]]
            [clojure.tools.cli :as cli]
            [clojure.string :as string]
            [clojure.core.async :as async :refer [chan go go-loop <!! >!! <! >! thread]]
            clojure.main
            clojure.repl
    ;; ref clj-http to have it load, even if not used here
            clj-http.client
            [clojure.java.io :as io]
            [clojure.tools.nrepl.server :as nrepl-server]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]
    ;; temp
            rt.runner)
  (:gen-class)
  (:import (java.net BindException ServerSocket)
           (java.awt Toolkit)
           (java.util Date)))

;; only here to be a thing to set in the REPL to skip the process exit when debugging
;; - do not change this value here... too easy to leave in when commiting
(def ^:dynamic *disable-process-quit* false)
;; call this to disable
(defn- disable-process-quit! []
  (alter-var-root (var *disable-process-quit*) (constantly true)))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; run-concurrent-tests

;; this a big mess begging for some cleanup
;; but for now some notes...

;; this is used for a couple of different purposes and that is making understanding
;; the various options difficult (writing this comment many months after working on this).

;; - the original purpose was to run multiple sessions of the same tests against
;; the same or different tenants, where test may be a list of tests or suites

;; - then we added the ability to run a different subset of the given test 'suite'
;; in each session using from index and max count, and typically doing that against
;; different tenants... the handling of these only makes sense if a single 'suite'
;; - then we added batch size to reduce session size to help prevent a series of longer
;; running tests cause a session that is much longer to run than others.. yep clear as mud

;; - always remember that the 'test' is typically a suite of tests and only when
;; it is a suite that things like from index and max count make sense
;; - this will spawn a number of sessions
;; - each session will test against the next tenant from a list of tenants
;; with that list created from the input list extended by repeating to match
;; the session count.
;; e.g. sessions = 3, tenants ABC => [ABC,ABC,ABC]
;; e.g. sessions = 3, tenants ABC,EDC => [ABC,EDC,ABC]

;; - if a from index is given and we have more than one tenant then we
;; generate from index for each of the subsequent tenants

(defn- get-current-time []
  (.getTime (Date.)))

(defn- get-test-count [suite]
  (let [n (count (rt.test.core/get-suite-tests (rt.test.db/get-test-entity suite)))]
    (infof "Get test count for %s => %d tests" suite n)
    n))

(defn- run-test-batch [index tenant test from count {:keys [server] :as options}]
  (let [options (merge (dissoc options :sessions :tenants :tests)
                       {:test test :mode :shell :session-id index}
                       (when tenant {:tenant tenant})
                       (when from {:from-test-index from :max-test-count count})
                       (get-browser-position index)
                       ;; add localhost if server not specified as we'll have started one in this process
                       (when-not server {:server "http://localhost:3000"}))]
    (info "Running with options: " (with-out-str (clojure.pprint/pprint options)))
    (rt.agent/run-test-session "test" options)))

(defn- apply-pace
  "Sleep for the difference between the pace setting and the test time, both in seconds."
  [pace secs]
  (when-let [delay (and pace (> pace secs) (- pace secs))]
    (infof "Padding out iteration due to pace setting. Pausing for %d secs" (int delay))
    (Thread/sleep (* delay 1000))))

(defn- apply-ramp-up-delay [index sessions {:keys [ramp-up]}]
  ;; If there is a ramp up then spread the start up of the sessions over the
  ;; ramp up time (seconds). In this mode we'd expect sessions equals batch count.
  ;; Do this by sleeping an amount based on the session index.
  (when-let [delay (and ramp-up (> sessions 1) (/ ramp-up (dec sessions)))]
    (infof "Waiting %d due to ramp up time %d" (int (* index delay)) ramp-up)
    (Thread/sleep (* index delay 1000))))

(defn- run-test-batches
  "Start a session for each of the given tenants and run the test batches
  on the sessions. Sessions are handed a batch only when ready for one."
  [batches tenants options]

  (let [batch-count (count batches)
        batches-in (chan batch-count)
        batches-out (chan batch-count)]

    ;; kick off a go process for each tenant in the tenant list... these represent our sessions
    (doseq [tenant tenants]
      (go-loop [batch (<! batches-in)]
        (when batch
          (let [[index test from size] batch
                ;; apply-ramp-up-delay may sleep for a bit
                _ (apply-ramp-up-delay index (count tenants) options)
                ;; this next expression runs and waits for the test run
                r (<! (thread (run-test-batch (inc index) tenant test from size options)))
                r (or r "nil")]
            (>! batches-out r)
            (recur (<! batches-in))))))

    ;; put all batches onto the session input channel
    (doseq [b batches]
      (>!! batches-in b))

    ;; read a result for each batch off the results channel
    (doseq [_ batches]
      (let [r (<!! batches-out)
            ;; drop the test std out ... most will have been logged to file
            r (if (map? r) (dissoc r :out) r)]
        (info "Test run done=> " r)))

    ;; close channels to ensure the go processes complete
    (async/close! batches-in)
    (async/close! batches-out)))

(defn- build-indexes
  "Build a list of n indexes, starting with the given index and increasing by size."
  [from n size]
  (reduce #(conj %1 ((fnil + 0) %2 (last %1))) [from] (repeat (dec n) size)))

(defn- run-parallel-tests
  "Run the given test suite over multiple sessions, distrbuting the suites tests
  in batches of a given size."
  [{:keys [tests sessions tenants batch-size max-test-count from-test-index server setup-scripts]
    :or   {sessions 1 from-test-index 0 max-test-count -1}
    :as   options}]
  {:pre [(= (count tests) 1) (> (count tenants) 0) (> sessions 0)]}

  (let [suite (first tests)

        ;; match tenants list to session count
        tenants (take sessions (apply concat (repeat sessions tenants)))

        max-test-count (if (> max-test-count 0) max-test-count
                                                (- (get-test-count suite) from-test-index))

        batch-size (or batch-size (int (Math/ceil (/ max-test-count sessions))))

        batch-count (int (Math/ceil (/ max-test-count batch-size)))

        from-indexes (build-indexes from-test-index batch-count batch-size)

        ;; build a list of vectors, each with an index, test, from, count
        batches (map #(vector %1 suite %2 batch-size) (range batch-count) from-indexes)]

    (infof "Parallel running %s batches size %s using %d sessions" batch-count batch-size sessions)

    (try
      ;; only start a server if we aren't given an existing to use
      (when-not server
        (rt.server/start-server))

      (when (not-empty setup-scripts)
        (apply rt.repl/run-tests setup-scripts))

      (do (print "running batches: ")
          (clojure.pprint/pprint {:tests tests :tenants tenants :batches batches :options options}))
      (run-test-batches batches tenants options)

      (info "All test runs complete")

      (catch BindException _
        (error "Failed to run concurrently - need access to port 3000 ... is RT already running?"))))

  ;; return no results
  nil)

(defn- run-concurrent-tests
  "Run multiple sessions of the given tests."
  [{:keys [tests sessions tenants ramp-up server setup-scripts]
    :or   {sessions 1 ramp-up 20}
    :as   options}]
  {:pre [(> (count tests) 0) (> (count tenants) 0) (> sessions 0)]}

  (let [;; match tenants list to session count
        tenants (take sessions (apply concat (repeat sessions tenants)))

        ;; fill out tests list to match batch count, matters if less tests than sessions
        tests (take sessions (apply concat (repeat sessions tests)))

        batches (map #(vector %1 %2 nil nil) (range sessions) tests)]

    (infof "Concurrent running %d sessions" sessions)

    (try
      ;; only start a server if we aren't given an existing to use
      (when-not server
        (rt.server/start-server))

      (when (not-empty setup-scripts)
        (apply rt.repl/run-tests setup-scripts))

      (do (print "running batches: ")
          (clojure.pprint/pprint {:tests tests :tenants tenants :batches batches :options options}))
      (run-test-batches batches tenants options)

      (info "All test runs complete")

      (catch BindException _
        (error "Failed to run concurrently - need access to port 3000 ... is RT already running?"))))

  ;; return no results
  nil)

(defn- reset-browser []
  (rt.lib.wd/stop-browser)
  (rt.test.core/quit-all-drivers))

(defn- run-serial-tests
  "Run tests either once or repeatedly.
  The tests are either given or received from a server."
  [{:keys [tests server no-repeat pace test-repeat stop-on-error]
    :or {test-repeat 0}}]

  (loop [test-repeat test-repeat]
    (let [tests (if (not-empty tests) tests (request-test-from-server nil))
          t0 (get-current-time)
          pace (->int pace)]

      (info "Running tests" tests "reporting to rt server" server)

      (when-not (empty? tests)

        ;; Notes - the core test runner gets the "global" settings using (get-settings)
        ;; and looks for :from-test-index and :max-test-count. Not ideal but the way it is.

        ;; run and print results
        (let [result (apply rt.repl/run-tests tests)]
          (info "Test run done result: " result)

          (let [s (with-out-str (rt.repl/print-run))]
            (debugf "Print run: %s" s))

          ;; maybe repeat
          (if (and (not no-repeat) (:repeat (request-options-from-server server)))
            (do (info "Re-running tests as server has :repeat on")
                (reset-browser)
                (apply-pace pace (/ (- (get-current-time) t0) 1000))
                (recur test-repeat))
            ;; else if test repeat is on and not stop on errors and we had an error
            (if (and (> test-repeat 0) (not (and stop-on-error (> (:errors result) 0))))
              (do (infof "Re-running tests due to test-repeat %d" test-repeat)
                  (reset-browser)
                  (recur (dec test-repeat)))
              ;; else
              result)))))))

(defn- setup-environment! [{:keys [server] :as options}]
  (info "Options as given:" options)
  (let [options (merge (request-options-from-server server) options)]
    (info "Options after merge with server options:" options)
    (setup-environment options)
    (get-settings)))

(defn- run-tests [options]
  (let [options (setup-environment! options)]

    (info "Running with options:" (with-out-str (clojure.pprint/pprint options)))

    (let [{:keys [tests sessions concurrent] :or {tests [] sessions 0}} options]
      (cond
        (and (= (count tests) 1) (> sessions 0)) (run-parallel-tests options)
        concurrent (run-concurrent-tests options)
        (> sessions 0) (warn "More than one test and sessions given, did you mean to add --concurrent?")
        :default (run-serial-tests options)))))

(defn- start-server [& [options]]
  (setup-environment-with-wait (or options {}))
  (rt.server/start-server))

;; notes on the cli options
;;
;; tests is a vector of the tests and suites specified using any of the test, suite, or tests arguments
;; - they all end up in tests which will default to an empty vector
;;    can either use such as --tests "t1,t2"
;;    or --test t1 --test t2
;;

(defn- append-keyed-vector [k m _ v]
  (println "append" k m v)
  (update-in m [k] #(into [] (remove empty (concat (or %1 []) %2))) v))

(def cli-options
  [[nil "--tests TESTS" "Test or suite ids" :parse-fn ->ids]
   ["-t" "--test TEST" "Test or test suite id"
    :parse-fn ->ids :assoc-fn (partial append-keyed-vector :tests)]
   [nil "--suite SUITE" "Test or test suite id"
    :parse-fn ->ids :assoc-fn (partial append-keyed-vector :tests)]

   ["-a" "--app-url URL" "The target client application root URL"]

   [nil "--tenants TENANTs" "Comma separated list of tenants - for use when have a list of tests and multiple sessions"
    :parse-fn checked-csv->list]
   [nil "--tenant TENANT" "Default tenant when no other is defined in fixtures"
    :parse-fn checked-csv->list :assoc-fn (partial append-keyed-vector :tenants)]

   [nil "--username USER" "Default username when no other is defined in fixtures"]
   [nil "--password PWD" "Default password when no other is defined in fixtures"]

   [nil "--target BROWSER" "Default browser no other is defined in fixtures. Any of chrome, firefox, ie, phantomjs."
    :parse-fn #(ensure-keyword %1)]
   [nil "--browser BROWSER" "Default browser no other is defined in fixtures. Any of chrome, firefox, ie, phantomjs."
    :parse-fn #(ensure-keyword %1)
    :assoc-fn (fn [m k v] (assoc m :target v))]
   [nil "--left x" "Default browser location offset"
    :parse-fn #(Integer/parseInt %)]
   [nil "--top y" "Default browser location offset"
    :parse-fn #(Integer/parseInt %)]

   [nil "--remote HOST" "optional remote selenium grid host"]

   [nil "--test-pkg PKG" "A zip file containing tests and drivers. Will be unzipped and used instead of shared-db-dirs"]
   [nil "--shared-db-dir DIR" "path to a shared test-db folder."]

   [nil "--setup-scripts SCRIPTS" "The script or scripts to run before running sessions."
    :parse-fn ->ids]
   [nil "--setup-script SCRIPT" "The script or scripts to run before running sessions."
    :parse-fn ->ids
    :assoc-fn (partial append-keyed-vector :setup-scripts)]

   [nil "--sessions N" "The number of sessions to use. If concurrent then each given test is
   given to a session, or the same test is run on each session. If not concurrent then we are
   running 'parallel' mode and the given test is assumed to be a suite and is distributed over
   the sessions. Need to explain this better :)"
    :parse-fn #(Integer/parseInt %)]

   [nil "--server URL" "URL for a coordinating RT server, something like http://myhost:3000"]
   [nil "--session-id ID" "The id of the session"]

   [nil "--concurrent" "Run the given tests concurrently. Use sessions to say how many."]
   [nil "--ramp-up SEC" "time over which to start multiple sessions"
    :parse-fn #(Integer/parseInt %)]
   [nil "--pace SEC" "the pace time is the minimum time a session takes per iteration"
    :parse-fn #(Integer/parseInt %)]

   [nil "--from-test-index n" "Start from the given test index for each suite, assuming the default sort order"
    :parse-fn #(Integer/parseInt %)]
   [nil "--max-test-count n" "Limit the number of tests run in each suite."
    :parse-fn #(Integer/parseInt %)]
   [nil "--batch-size n" "Test batch size when using multiple sessions to run a suite"
    :parse-fn #(Integer/parseInt %)]

   [nil "--repeat" "Turn on repeating. Only meaninful when a runner is controlled by a server (this is temp)"]
   [nil "--no-repeat" "Disable repeating, regardless of any server supplied value"]
   [nil "--no-thinking" "Disable repeating, regardless of any server supplied value"]

   [nil "--stop-on-error" "If true then stop a test run after the first error or fail"]
   [nil "--test-retry-limit N" "If a positive number then failed tests in a suite will be retried up to that many times."
    :parse-fn #(Integer/parseInt %)]

   [nil "--test-repeat N" "The number of times to run each test"
    :parse-fn #(Integer/parseInt %)]

   [nil "--verbose" "More output"]
   [nil "--dry-run" "Print instead of executing the test steps"]
   [nil "--teamcity" "Turn on teamcity mode meaning we always exit 0 and (to be done) emit TC progress messages."]
   [nil "--disable-auto-ng-wait" "Disables the automatic wait for angular on every element locator."]])

(comment

  (let [args ["test" "--tests" "t1,:t2" "-t" ":rn/suites/t1,a/b"
              "--target=chrome"]
        result (cli/parse-opts args cli-options)]
    (clojure.pprint/pprint (dissoc result :summary)))

  (let [args ["test" "--test=perf/tests/t0,perf/tests/t0" "--server=x" "--disable-auto-ng-wait" "--left=220"
              "--test=t9" "--test t10" "--app-url=a" "--suite s1" "-t t2"]
        result (cli/parse-opts args cli-options)]
    (clojure.pprint/pprint (dissoc result :summary)))

  )

(defn- usage [options-summary]
  (->> ["Run the ReadiNow Automated Tests."
        ""
        "Usage: program-name [options] action"
        ""
        "Options:"
        options-summary
        ""
        "Actions:"
        "    server         Start the web server (localhost:3000)"
        "    test           Run the specified tests or test suites"
        "    list           List the available suites and tests"
        "    repl           Start an interactive REPL"]
       (string/join \newline)))

(defn- error-msg [errors]
  (str "The following errors occurred while parsing your command:\n\n"
       (string/join \newline errors)))

(defn- exit [status msg]
  (info "Exit message:" msg)
  (rt.lib.wd/stop-browser)
  (rt.test.core/quit-all-drivers)
  (when-not *disable-process-quit*                          ; for debugging, see var def comments
    (System/exit status)))

(defn- list-tests [& [options]]
  (setup-environment-with-wait options)
  (rt.app/write-app-driver-list-csv)
  (exit 0 (rt.repl/list-tests (:test options))))

(defn- run-test [options]
  (let [results (run-tests options)
        {:keys [errors fails]} results
        issues ((fnil + 0 0) errors fails)
        exit-code (if (or (zero? issues)
                          (running-in-teamcity? options))
                    0 1)]
    (exit exit-code (str results))))

(defn listen [{:keys [port]}]
  (with-open [sock (.accept (ServerSocket. port))]
    (doseq [msg-in (line-seq (io/reader sock))]
      (println msg-in))))

(defn -main
  "The main function - what you get when you run this (e.g. lein run)."
  [& args]
  (let [{:keys [options arguments errors summary]} (cli/parse-opts args cli-options)
        options (cond-> options
                        (:test-pkg options) (assoc :test-pkgs (remove empty? [(:test-pkg options)]))
                        (:shared-db-dir options) (assoc :shared-db-dirs (remove empty? [(:shared-db-dir options)]))
                        ;; keep these as we pass them to spawned sessions
                        ;true (dissoc :test-pkg :shared-db-dir)
                        )]

    (info "Running RT" (get-version 'rt) "with options:" arguments options)

    ;; Handle help and error conditions
    (cond
      (:help options) (exit 0 (usage summary))
      (not= (count arguments) 1) (exit 1 (usage summary))
      errors (exit 1 (error-msg errors)))

    (when-not errors

      (init-logging options)

      ;; Execute program with options
      (try
        (case (first arguments)
          "list" (list-tests options)
          "test" (run-test options)
          "server" (start-server options)
          "repl" (rt.repl/run-repl options)
          "nrepl" (nrepl-server/start-server :port 7888)
          "agent" (rt.agent/run-agent options)
          "listen" (listen options)
          (exit 1 (usage summary)))

        (catch Exception ex
          (exit 1 (pr-str ex)))))))

(comment

  (do
    (disable-process-quit!)
    (reset)
    (apply -main (string/split "test --tests rn/suites/regression-mobile --from-test-index 2 --max-test-count 1" #"\s+")))

  (do
    (disable-process-quit!)
    (apply -main (string/split "test --test rn/suites/regression-mobile --from-test-index 2
     --sessions 2 --tenants t1,t2 --ramp-up 10" #"\s+")))

  (do
    (disable-process-quit!)
    (apply -main (string/split "test --test rn/suites/regression-mobile --from-test-index 2
     --sessions 2 --tenants t1,t2 --batch-size 2" #"\s+")))

  (do
    (disable-process-quit!)
    (rt.repl/reset)
    (apply -main (string/split "test --test rn/suites/regression-chrome --dry-run --teamcity
    --tenants EDC,T001,T002 --sessions 0 --from-test-index 0 --max-test-count 100" #"\s+")))

  (do
    (disable-process-quit!)
    (rt.repl/reset)
    (apply -main (string/split "test --test rn/suites/regression-chrome --dry-run --teamcity
    --tenants EDC,T001,T002 --sessions 3" #"\s+")))

  (apply -main (string/split "test --test rn/suites/regression-chrome --tenants EDC,T001
  --remote spappliance.sp.local --sessions 2 --max-test-count 1 --ramp-up 0" #"\s+"))

  (apply -main (string/split "test --test rn/suites/regression-chrome --tenants EDC,DEV
  --sessions 2 --max-test-count 1 --ramp-up 0" #"\s+"))

  (apply -main (string/split "test --test rn/suites/regression-chrome --tenants EDC,DEV
  --from-test-index 0 --max-test-count -1 --ramp-up 0" #"\s+"))

  (apply -main ["help"])

  (apply -main (string/split "test --test rn/suites/regression-chrome-1 --from-test-index 0 --max-test-count 1" #"\s+"))

  (apply -main (string/split "test --server http://localhost:3000 --sessions=1" #"\s+"))
  (apply -main (string/split "test --server http://localhost:3000 --no-repeat --test perf/tests/t0
        --setup-script grc/tests/set-test-data --sessions 1" #"\s+"))

  (apply -main (string/split "test --test samples/suite --test-pkg RnTests.zip --shared-db-dir=" #"\s+"))
  (apply -main (string/split "test --test samples/suite --test-pkg //spdevnas01.sp.local/Development/Shared/RnTests/RnTests.zip" #"\s+"))
  (apply -main (string/split "test --shared-db-dir //spdevnas01.sp.local/Development/Shared/RnTests/test-db" #"\s+"))
  (apply -main (string/split "test --shared-db-dir //spdevnas01.sp.local/Development/Shared/RnTests/test-db --test-pkg=" #"\s+"))

  (init-logging)
  (listen {:port 2003})

  )