(ns rt.repl
  "The default namespace for the in app REPL."
  (:require [rt.lib.util :refer [seq->table left-with-elipses]]
            rt.setup
            rt.app
            [rt.test.db :refer [get-test-list get-test-run
                                get-test-runs-db get-test-run-list get-test-run-scripts
                                get-last-test-run]]
            [rt.test.core :refer [run-all-tests start-test-run start-partial-test-run
                                  run-next-step rerun-last-step run-next-step-with-expr
                                  get-script-parent teardown?
                                  write-run-suite-summary-csv get-test-refs get-test-scripts
                                  get-run-summary get-fixture-refs resolve-entity get-fixture-scripts]]
            [rt.test.junit-report :refer [write-junit-report]]
            rt.scripts
            [clojure.main :refer [repl]]
            clojure.repl
            clojure.pprint
            clojure.java.shell
            [clojure.string :as string]))

(defn save-to-repl-state!
  "Save the given map of kvs to the repl state (in app state).
  Returns the new value of the repl state."
  [new-values]
  (rt.setup/update-app-state! (fn [{:keys [repl] :as state} & args]
                                (assoc state :repl (merge repl new-values)))))

(defn save-test-list!
  "Save the test list to the repl state and return the test list."
  [test-list]
  (save-to-repl-state! {:test-list test-list})
  test-list)

(defn save-runs-list!
  "Save the test runs list to the repl state. And return it."
  [runs-list]
  (save-to-repl-state! {:runs-list runs-list})
  runs-list)

(defn save-test-run-cursor!
  [cursor]
  (save-to-repl-state! {:test-run-cursor cursor})
  cursor)

(defn get-last-list []
  (get-in (rt.setup/get-app-state) [:repl :test-list]))

(defn get-last-runs-list []
  (get-in (rt.setup/get-app-state) [:repl :runs-list]))

(defn get-test-run-cursor []
  (get-in (rt.setup/get-app-state) [:repl :test-run-cursor]))

(defn get-test-id-by-index
  "Get the id of the test given its index in the last test listing."
  [index]
  (->> (get-last-list)
       (filter #(= index (:index %)))
       first
       :id))

(defn get-test-run-id-by-index
  "Get the id of the test run given its index in the last test run listing."
  [index]
  (->> (get-last-runs-list)
       (filter #(= index (:index %)))
       first
       :id))

(defn find-tests
  "Return the tests with matching id or name"
  [s] (let [re (re-pattern s)]
        (->> (get-test-list)
             (filter #(or (re-find re (str (:id %)))
                          (re-find re (or (:name %) ""))))
             (map :id)
             (sort))))

(defn get-tests
  ([] (get-tests ""))
  ([search-str] (->> (get-test-list)
                     (filter #((set (find-tests search-str)) (:id %)))
                     (sort-by :id)
                     (sort-by :type)
                     (map-indexed #(assoc %2 :index %1))
                     vec)))

;;; rt.REPL commands

(defn last-list
  "Show the lest test listing."
  {:repl-command true}
  []
  (clojure.pprint/print-table [:index :id :name :type]
                              (get-last-list)))

(defn list-tests
  "Print a list of all or matching tests and suites"
  {:repl-command true}
  [& [search-str]]
  (->> (get-tests (or search-str ""))
       (save-test-list!))
  (last-list))

(defn list-runs
  "List test runs"
  {:repl-command true}
  []
  ;; todo - support options to filter results like n most recent
  (let [runs (vals (get-test-runs-db))
        runs (map-indexed #(into {:index %1}
                                 (select-keys %2 [:id :created :status])) runs)]
    (save-runs-list! runs)
    (clojure.pprint/print-table [:index :id :created :status] runs)))

(comment

  (let [id (-> (get-test-run-list) last :id)
        run (get-test-run id)
        script-mapper #(merge %1 {:index %2} (select-keys %3 [:script :status :time :result]) (:counters %3))
        test-mapper #(map-indexed (partial script-mapper {:test-id (-> % :test :id)}) (:scripts %))
        results (mapcat test-mapper (:tests run))]
    results)

  (reset)
  (get-run-summary (get-last-test-run))

  (clojure.pprint/pp)

  )

(defn get-script-summary [{:keys [run test-index script-index]}]
  (when script-index
    (select-keys (get-in run [:tests test-index :scripts script-index])
                 [:script :type :status :result :time :counters])))

(defn print-run
  "Print a summary of the current test run."
  {:repl-command true}
  ([] (print-run (get-in (get-test-run-cursor) [:testrun :id])))
  ([id] (let [id (if (number? id) (get-test-run-id-by-index id) id)
              {:keys [id created dir-name] :as testrun} (get-test-run id)
              test-refs (get-test-refs testrun)
              tests (map #(get-in testrun (:path %)) test-refs)
              tests-summary (map #(hash-map :id (get-in % [:id])
                                            :status (:status %)
                                            :steps (count (mapcat :steps (:steps %))))
                                 tests)
              separator (apply str (repeat 8 "=========="))]
          (when testrun
            (println separator)
            (println " Test run:" id)
            (println "  created:" created)
            (println "   output:" dir-name)
            (println separator)
            (print "Has tests:")
            (clojure.pprint/print-table [:id :status :steps] tests-summary)
            (doseq [ref test-refs]
              (let [test-path (:path ref)
                    test (get-in testrun test-path)
                    scripts (get-test-scripts (get-in testrun test-path) test-path)
                    scripts (map #(assoc % :step (-> (:script %)
                                                     str    ; in case it is a keyword script id
                                                     (left-with-elipses 50)
                                                     (string/replace #"\r\n" " "))
                                           :owner (:owner %)
                                           :index (:index %)
                                           :status (if (:result %) :done :not-done)
                                           :events (select-keys (->> (:events %) (map :type) frequencies)
                                                                [:pass :fail :error]))
                                 scripts)]
                (println separator)
                (print "Steps for" (:id test))
                (clojure.pprint/print-table [:step :owner :index :status :time] scripts)))))))

(defn- get-test-steps-summary [testrun test-ref]
  (let [test-path (:path test-ref)
        test (get-in testrun test-path)
        parent-fixture-paths (map second (get-fixture-refs [testrun (drop-last test-path)]))
        scripts (get-test-scripts (get-in testrun test-path) test-path)
        setup-scripts (mapcat #(get-fixture-scripts (get-in testrun %) % :setup) parent-fixture-paths)
        teardown-scripts (mapcat #(get-fixture-scripts (get-in testrun %) % :teardown) parent-fixture-paths)
        ;; don't include once style setup and teardown ... we are obsoleting it anyway
        ;scripts (concat setup-scripts scripts teardown-scripts)
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
                         :script (:script %)
                         :owner (:owner %)
                         :status status
                         :events event-summary))
                     scripts)]
    (map #(merge {:test-id (:id test)} %) scripts)))

(defn get-testrun-steps-summary []
  (let [id (get-in (get-test-run-cursor) [:testrun :id])
        id (if (number? id) (get-test-run-id-by-index id) id)]
    (when-let [{:keys [id created] :as testrun} (get-test-run id)]
      (let [test-refs (get-test-refs testrun)
            results []]
        {:id      id
         :created created
         ;; need to include the tests, if only their ids, for the UI to know if this
         ;; is the correct testrun for the context.... yep, yuck
         :tests   (mapv #(hash-map :id (:id %)) (:tests testrun))
         :steps   (vec (mapcat (partial get-test-steps-summary testrun) test-refs))}))))

(defn run-tests
  "Create a \"test run\" for the given given tests and run them all."
  {:repl-command true}
  ([& tests] (let [tests (map #(if (number? %) (get-test-id-by-index %) %) tests)
                   run-id (start-test-run tests)]
               (if (empty? (-> run-id get-test-run :tests))
                 (do
                   (println "No tests to run. Please check the given test ids."))
                 (do
                   (run-all-tests run-id)
                   (save-test-run-cursor! {:testrun (get-test-run run-id)})
                   (write-junit-report run-id)
                   ;; write a version to the test runs folder
                   (let [base-file-name (str (-> run-id get-test-run :dir-name) "/suitemetrics")]
                     (write-run-suite-summary-csv base-file-name run-id))
                   ;; and a version to the current folder (where the jar runs)
                   (write-run-suite-summary-csv "./suitemetrics" run-id)
                   ;; return the following
                   (get-run-summary (get-test-run run-id))))))
  ([] (println "Missing test id or index")))

(defn init-test-run
  "Start a test run with the given tests (ids or indexes into last listing)"
  {:repl-command true}
  ([& tests] (let [tests (map #(if (number? %) (get-test-id-by-index %) %) tests)
                   run-id (start-test-run tests)]
               (save-test-run-cursor! {:testrun (get-test-run run-id)})
               (print-run)
               run-id))
  ([] (println "Missing test id or index")))


(defn init-partial-test-run
  "Start a test run with the given for the test in the given suite"
  [suite test]
  (when-let [run-id (start-partial-test-run suite test)]
    (save-test-run-cursor! {:testrun (get-test-run run-id)})
    (print-run)
    run-id))


(defn run-next
  "Run the next step of the current test run"
  {:repl-command true}
  []
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (run-next-step run-id)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn run-next-with-expr
  "Run the next step of the current test run but using the given expr"
  {:repl-command true}
  [expr save?]
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (run-next-step-with-expr run-id expr save?)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn rerun-last
  "Run the last run step of the current test run"
  {:repl-command true}
  []
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (rerun-last-step run-id)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn reset-last
  "Reset the last run step of the current test run"
  {:repl-command true}
  []
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (rt.test.core/reset-last run-id)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn run-to-end
  "Run the to end of the current test run"
  {:repl-command true}
  []
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (do (run-all-tests run-id)
        (get-script-summary {:testrun (last (get-test-run-list))}))
    (println "No tests have been started.")))

(defn run-to-error
  "Run the to either an error or the end of the current test run"
  {:repl-command true}
  []
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (rt.test.core/run-until-error run-id)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn run-to-step
  "Run the to given step index of the current test run"
  {:repl-command true}
  [path]
  (if-let [{{run-id :id} :testrun} (get-test-run-cursor)]
    (let [cursor (rt.test.core/run-to-step run-id path)]
      (when cursor
        (save-test-run-cursor! cursor))
      (get-script-summary (or cursor {:testrun (last (get-test-run-list))})))
    (println "No tests have been started.")))

(defn reset
  "Reset rt. Reloads settings, tests and run data."
  {:repl-command true}
  [& [options]]
  (rt.app/setup-environment options)
  (when-let [testrun (get-last-test-run)]
    (save-test-run-cursor! {:testrun testrun}))
  nil)

(defn start-server
	"Start the RT webserver and launches a browser on http://localhost:3000."
  {:repl-command true}
	[]
	((resolve 'rt.server/start-server))
	(clojure.java.shell/sh "cmd" "/c start http://localhost:3000"))

(defn help []
  (println "Any Clojure expression can be used: e.g. (println \"Hello World\")")
  (println "The following are the typical commands/functions you'll use at this prompt.")
  (println "At present you need to wrap the commands in braces: e.g. (start-test :mytest)")
  (println "A good starter command is (list-tests \"suites\")")

  (println)
  (doseq [{:keys [name doc]} (->> (ns-publics *ns*)
                                  (filter #(:repl-command (meta (val %))))
                                  (map #(select-keys (meta (second %)) [:name :doc])))]
    (println (format "%-20s %s" name doc)))

  (println)
  (println (format "%-20s %s" "Ctrl+Z ENTER" "Quit the rt.REPL")))

(defn do-repl-init []
  (println "Welcome to the rt.app command line (REPL).")
  (println "(help)                for usage help")
  (in-ns 'rt.repl)
  (refer 'clojure.pprint))

(defn do-repl-read [request-prompt request-exit]
  ;; Look for a limited set of convenience commands
  ;; TODO - fill this out
  (let [form (clojure.main/repl-read request-prompt request-exit)]
    (cond
      (= 'dir form) (list 'list-tests)
      (= 'help form) (list 'help)
      (= '? form) (list 'help)
      (= 'exit form) request-exit
      (= '(exit) form) request-exit
      :default form)))

(defn run-repl [& [options]]
  (rt.app/setup-environment options)
  #_(clojure.main/repl :init init-repl)
  (clojure.main/repl
    :init do-repl-init
    ;; commented out as isn't working reliably
    ;;:read  do-repl-read
    ))

