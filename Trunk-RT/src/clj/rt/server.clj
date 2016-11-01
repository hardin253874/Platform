(ns rt.server
  "A web server to serve up a HTML UI to the test app plus any web services it requires."
  (:require [rt.app :refer [get-tests get-entities get-entity update-entity
                            get-app-driver-list get-browser-logs
                            get-entity-graph]]
            [rt.lib.util :refer [ms->s id->str ->int timeit format-tc-message]]
            rt.repl
            [rt.load :refer [register-agent register-session report-event]]
            [compojure.handler :as handler]
            [compojure.route :as route]
            [compojure.core :refer [GET POST PUT defroutes]]
            [ring.middleware.json :as middleware]
            [ring.middleware.format :refer [wrap-restful-format]]
            [ring.adapter.jetty :as jetty]
            [ring.util.response :refer [response resource-response]]
            [clojure.core.async :as async :refer [chan go <!! >!! <! >!]]
            [clojure.walk :as walk]
            [rt.test.db :as db])
  (:import java.io.File
           (java.util Date)
           (java.io StringWriter)))

(def channels (atom {}))

(defn get-thread-id []
  (.getId (Thread/currentThread)))

(defn start-test-runner
  "Start the test runner and return the channel that it listens to."
  []
  (let [c (chan (async/buffer 100))]
    (go (loop [d (<! c)]
          (when-let [msg-type (:type d)]
            (println (format "%d: received test runner request: %s" (get-thread-id) d))
            (try
              (condp = msg-type
                :run-next (if (:expr d)
                            (rt.repl/run-next-with-expr (:expr d) (:save? d))
                            (rt.repl/run-next))
                :run-to-end (rt.repl/run-to-end)
                :run-to-error (rt.repl/run-to-error)
                :run-to-step (rt.repl/run-to-step (:path d))
                :rerun (rt.repl/rerun-last)
                :reset-step (rt.repl/reset-last)
                :reset (rt.repl/reset))

              (catch Exception e
                (println "Exception:" e)))
            (recur (<! c)))))
    c))

(defn get-output-files []
  (->> (clojure.java.io/file "./out")
       (file-seq)
       (filter #(.isFile %))
       (map #(.getPath %))))

(defn ensure-test-runner []
  ;;TODO fix this ... swap shold not have side-effects!
  (when-not (:test-runner @channels)
    (println "starting testrunner")
    (swap! channels #(assoc % :test-runner (start-test-runner)))))

(defn run-tests [id]
  (ensure-test-runner)
  (when-let [c (:test-runner @channels)]
    (>!! c {:type :run :id id}))
  ;; need to return a response or something that can be made into one
  ""
  #_(let [{test-run-id :id} (rt.repl/run-tests id)]
      (response (rt.test.db/get-test-run test-run-id))))

(defn get-suite-tests [id]
  (->> (rt.test.core/get-suite-tests (rt.test.db/get-test-entity id))
       (map #(select-keys % [:id :type :name :doc :tags]))
       (filter not-empty)))

(defn create-test-run [suite-id test-id]
  (println "creating testrun for suite" suite-id "and including test (empty or nil means all)" test-id)
  (let [test-id (if (empty? test-id) nil test-id)
        test-run-id (rt.repl/init-partial-test-run suite-id test-id)]
    (response (rt.test.db/get-test-run test-run-id))))

(defn get-test-run []
  (let [testrun-summary (rt.repl/get-testrun-steps-summary)]
    #_(spit "temp.edn" (with-out-str (clojure.pprint/pprint testrun-summary)))
    testrun-summary))

(defn get-test-run-report [test-id]
  (let [{:keys [steps] :as summary} (rt.repl/get-testrun-steps-summary)
        test-id (and (not-empty test-id) (keyword test-id))
        test-ids (distinct (map :test-id steps))
        test-ids (if test-id (filter #(= test-id %) test-ids) test-ids)
        steps-for-test (fn [id] (filter #(= id (:test-id %)) steps))
        test-status (fn [id] (let [steps (steps-for-test id)]
                               (cond
                                 (some #(= :error (:status %)) steps) :error
                                 (some #(= :fail (:status %)) steps) :fail
                                 (= :not-done (:status (first steps))) :not-started
                                 (some #(= :not-done (:status %)) steps) :running
                                 :default :done)))
        tests (mapv #(hash-map :id % :status (test-status %)) test-ids)]
    (assoc summary :test-records tests
                   :steps (steps-for-test test-id)
                   :filtered-on-test-id test-id)))

(defn run-next [params]
  (ensure-test-runner)
  (let [{:keys [expr save?]} (clojure.walk/keywordize-keys params)]
    (when-let [c (:test-runner @channels)]
      (>!! c {:type :run-next :expr expr :save? save?})))
  ;; need to return a response or something that can be made into one
  "")

(defn reset []
  (rt.repl/reset)
  #_(ensure-test-runner)
  #_(when-let [c (:test-runner @channels)]
      (>!! c {:type :reset}))
  ;; need to return a response or something that can be made into one
  "")

(defn run-to-end []
  (ensure-test-runner)
  (when-let [c (:test-runner @channels)]
    (>!! c {:type :run-to-end}))
  ;; need to return a response or something that can be made into one
  "")

(defn run-to-error []
  (ensure-test-runner)
  (when-let [c (:test-runner @channels)]
    (>!! c {:type :run-to-error}))
  ;; need to return a response or something that can be made into one
  "")

(defn run-to-step [params]
  (ensure-test-runner)
  (let [{:keys [id path]} (clojure.walk/keywordize-keys params)
        ;; need to convert strings in the path to keys too
        path (mapv #(if (string? %) (keyword %) %) path)]
    (when-let [c (:test-runner @channels)]
      (>!! c {:type :run-to-step :id id :path path})))
  ;; need to return a response or something that can be made into one
  "")

(defn rerun-step [params]
  (ensure-test-runner)
  (let [{:keys [testrun-id test-id step-index]} (clojure.walk/keywordize-keys params)]
    (when-let [c (:test-runner @channels)]
      (>!! c {:type :rerun :testrun-id testrun-id :test-id test-id :step-index step-index})))
  ;; need to return a response or something that can be made into one
  "")

(defn reset-step [params]
  (ensure-test-runner)
  (let [{:keys [testrun-id test-id step-index]} (clojure.walk/keywordize-keys params)]
    (when-let [c (:test-runner @channels)]
      (>!! c {:type :reset-step :testrun-id testrun-id :test-id test-id :step-index step-index})))
  ;; need to return a response or something that can be made into one
  "")

(defn check-expr
  "Return nil if ok, otherwise an error string."
  ([expr] (check-expr expr "rt.scripts"))
  ([expr ns]
   {:pre [ns expr]}
    ;(println "check-expr" (pr-str expr) (pr-str ns))
   (try
     (binding [*ns* (find-ns (symbol ns))]
       (binding [*read-eval* false]
         (load-string expr)))
     nil
     (catch Exception e
       (println e)
       (pr-str e)))))

(defn check-expr-with-out
  ([expr] (check-expr expr "rt.scripts"))
  ([expr ns]
   {:pre [ns expr]}
   (println "check-expr" (pr-str expr) (pr-str ns))
   (let [s (StringWriter.)
         out0 *out*]
     (binding [*out* s]
       (try
         (binding [*ns* (find-ns (symbol ns))]
           (binding [*read-eval* false]
             [(pr-str (load-string expr)) (str s)]))
         (catch Exception e
           (println e)
           (binding [*out* out0] (println "exception" (pr-str e) "out" (str s)))
           [(pr-str e) (str s)]))))))

(defn eval-script [script-str]
  (if-let [forms (rt.test.core/read-expressions script-str)]
    #_(reduce #(do (println "eval:" (pr-str %2) "in ns:" *ns*) (eval %2)) nil forms)
    (eval (first forms))
    (throw (Exception. (str "Failed to resolve script: \"" script-str "\"")))))

(defn eval-script-with-out-str
  [script-str]
  (println "eval-expr" (pr-str script-str))
  (let [s (StringWriter.)]
    (binding [*out* s]
      (try
        [(eval-script script-str) (str s)]
        (catch Exception ex
          (println "exception")
          (clojure.stacktrace/print-cause-trace ex)
          [ex (str s)])))))

(defn eval-expr-1
  ([expr ns] (let [[result out] (check-expr-with-out expr ns)]
               (println "eval-expr-1 returning:" result "out:" out)
               [(pr-str result) out])))

(defn eval-expr-2
  ([expr ns] [nil (with-out-str (check-expr expr ns))]))

(defn eval-expr-3
  "Returns [return-value std-out]"
  ([expr ns]
   {:pre [ns expr]}
   (println "eval-expr-3" (pr-str expr) (pr-str ns))
   (let [t0 (.getTime (Date.))
         [result out] (binding [*ns* (find-ns (symbol ns))]
                        (eval-script-with-out-str expr))
         t (- (.getTime (Date.)) t0)]
     (println "Eval" expr "took" (ms->s t) "secs")
     (println "Eval returned" result (str \newline "***Output***" \newline out "***End Output***"))
     [(pr-str result) out])))

;; the various eval-expr-1 2 and 3 here are trying out different ways to
;; eval code. i was hoping 3 would work but finding things hang somewhere
;; when evaluating some expressions - ones that cause exceptions.. I think...
;; I might come back to this....

(defn eval-expr
  ([expr] (eval-expr expr "rt.scripts"))
  ([expr ns] (try
               (eval-expr-1 expr ns)
               (catch Exception e
                 (println "Exception" e)
                 [nil "exception"]))))

(defn get-source [ns name]
  (clojure.repl/source-fn (symbol ns name)))

(defn update-source [ns name src]
  (println "To be implemented: update-source" ns name src))

(defn get-source-files []
  (rt.test.db/get-app-driver-file-list-info))

(defn get-source-file [name]
  (rt.test.db/get-app-driver-file name))

(defn update-source-file [name src]
  ;(println "update-source-file" name src)
  (if (and name src)
    (do
      (rt.test.db/save-app-driver-file name src)
      nil)
    false))

(def context (atom {}))

(defn get-context [& [iter]]
  ;(println "getting context=>" @context iter (type iter))
  ;; TODO - support some repeats, but for now only return if iter 0
  ;; doing this because this API is currently used to get the test to run
  (when (or (empty? iter) (< (->int iter) 1))
    (merge (rt.setup/get-settings) @context)))

(defn set-context [value]
  (println "setting context .. merge" value "into " @context)
  (swap! context merge value))

(def posted-step-summary (atom {}))

(defn weighted-avg [current new]
  (+ (* (or current new) 9/10) (* (or new 0) 1/10)))

(defn avg [{:keys [count total]}]
  (/ total count))

(defn clean-avg [{:keys [min max count total] :as stats}]
  ;; kinda arbitrary choosing of the 10 threshold
  (if (> count 10)
    (/ (- total min max) (- count 2))
    (avg stats)))

(defn rate [{:keys [values]}]
  (let [t0 (some-> values first :date .getTime)
        d0 (some-> values first :time)
        tn (some-> values last :date .getTime)
        n (-> values count)]
    ;; some thinking here ... rough as guts... account for the time the steps take, hence d0
    (if (and t0 tn (> tn t0))
      (/ n (/ (- tn (- t0 d0)) 60000))
      0)))

(defn trim-values [{:keys [values]}]
  ;; need to tune this... but at least we shouldn't fall over
  (if (> (count values) 150)
    (subvec values (- (count values) 100))
    values))

(defn update-step-stats [stats {:keys [time script test-id index metric-id target-msec error fail]}]
  (let [time (double (/ time 1000))
        d (Date.)
        stats (-> stats
                  (assoc :expr script :test-id test-id :index index
                         :metric-id metric-id :target-msec target-msec
                         :last-reported d)
                  (update-in [:values] #(-> (or % [])
                                            (conj {:date d :time time})))
                  (update-in [:count] (fnil inc 0))
                  (update-in [:total] (fnil + 0) time)
                  (update-in [:wavg] weighted-avg time)
                  (update-in [:min] (fnil min time) time)
                  (update-in [:max] (fnil max time) time))
        stats (assoc stats :avg (double (avg stats)))
        stats (assoc stats :cavg (double (clean-avg stats)))
        stats (assoc stats :rate (double (rate stats)))
        stats (assoc stats :values (trim-values stats))
        stats (if (or error fail) (update-in stats [:errors] (fnil inc 0)) stats)]
    stats))

(defn update-step-summary [posted-step-summary {:keys [test-id index metric-id] :as step}]
  (cond-> posted-step-summary
          true (update-in [:update-count] (fnil inc 0))
          ;metric-id (update-in [:metrics metric-id] update-step-stats step)
          test-id (update-in [:tests test-id index] update-step-stats step)))

(defn update-session-summary [session-summary host session-id {:keys [test-id index]}]
  (update-in session-summary [:sessions host session-id]
             #(-> % (assoc :host host :session-id session-id
                           :last-reported (Date.)
                           :last-test-id test-id
                           :last-step-index index))))

(defn update-session-test-summary [session-summary host session-id {:keys [name duration]}]
  (cond-> session-summary
          true (update-in [:sessions host session-id]
                          #(-> % (assoc :host host :session-id session-id
                                        :last-reported (Date.)
                                        :last-test-id name)))
          duration (update-in [:sessions host session-id]
                              #(-> % (assoc :last-test-duration duration)))))

(defn post-step-result [data]
  (let [{:keys [step host session-id]} (clojure.walk/keywordize-keys data)]
    ;(println "Received step result" host session-id step)
    (swap! posted-step-summary update-step-summary step)
    (swap! posted-step-summary update-session-summary host session-id step))
  "")

(defn post-test-event [data]
  (let [{:keys [event host session-id]} (clojure.walk/keywordize-keys data)]
    ;(println "Received test event" host session-id event)
    (swap! posted-step-summary update-session-test-summary host session-id event))
  "")

(defn handle-tc-event [data]
  (let [{:keys [event session-id]} (clojure.walk/keywordize-keys data)]
    ;(println "Received tc event" host session-id event)
    (let [{:keys [type values]} event
          values (assoc values :flowId (str session-id))]
      (println (format-tc-message type values))))
  "")

(defn write-step-summary-csv [& [fname]]
  (->> (vals (:tests @posted-step-summary))
       (mapcat vals)
       (map #(-> %
                 (dissoc :values)
                 (assoc :last-reported (rt.lib.util/csv-datetime-str (:last-reported %)))))
       (rt.lib.util/write-csv-objects (or fname "step-summary.csv")))
  "")

(defn take-step-stats-baseline
  ([]
   (println "taking baseline...")
   (swap! posted-step-summary take-step-stats-baseline))
  ([summary]
    ;; build list of test-id and index and then do an update-in with each
   (reduce (fn [summary {:keys [test-id index]}]
             (update-in summary [:tests test-id index] #(assoc % :baseline (:cavg %))))
           summary
           (->> (vals (:tests summary)) (mapcat vals)))
   ""))

(comment

  (get-context 0)
  (set-context {:test "crm/tests/new-lead-ct"})
  (reset! posted-step-summary {})
  (clojure.pprint/pprint @posted-step-summary)
  (do
    (post-step-result {"step" {"test-id" "t0" "index" 0 "time" (rand-int 5000)
                               "target-msec" 2000 "script" "hello"}})
    (post-step-result {"step" {"test-id" "t1" "index" 0 "time" (rand-int 5000) "script" "hello"}})
    (post-step-result {"step" {"test-id" "aa1" "index" 0 "time" (rand-int 5000) "metric-id" "m1" "script" "hello"}})
    (post-step-result {"step" {"test-id" "aa1" "index" (/ 1 100) "time" (rand-int 5000) "metric-id" "m1" "script" "hello"}})
    (post-step-result {"step" {"test-id" "aa1" "index" 2 "time" (rand-int 2000) "metric-id" "m1" "script" "hello"}})
    (post-step-result {"step" {"test-id"     "a/b/c/2" "index" 3 "time" (rand-int 2000)
                               "target-msec" 1000
                               "script"      "hello out there is there anyone to see what is happening on this place should we
                               stay or should we go now or will there be trouble"}}))

  (write-step-summary-csv)
  (take-step-stats-baseline)

  (do
    (reset! posted-step-summary {})
    (post-step-result {"step" {"test-id" "t1" "index" 1 "time" (rand-int 5000) "metric-id" "m1" "script" "hello"}})
    (clojure.pprint/pprint @posted-step-summary))

  )

(defroutes
  app-routes
  (GET "/" [] (resource-response "index.html" {:root "public"}))
  (GET "/api/registeragent" [host max] (response (register-agent host max)))
  (GET "/api/registersession" [host id] (response (register-session host id)))
  (POST "/api/reportevent" {value :body} (response (report-event value)))
  (GET "/api/context" [iter] (response (get-context iter)))
  (GET "/api/step-summary" [] (response @posted-step-summary))
  (POST "/api/context" {value :body} (response (set-context value)))
  (POST "/api/step-result" {value :body} (response (post-step-result value)))
  (POST "/api/test-event" {value :body} (response (post-test-event value)))
  (POST "/api/tc-event" {value :body} (response (handle-tc-event value)))
  (GET "/api/drivers" [] (response (get-entities :driverfn)))
  (GET "/api/settings" [] (response (rt.setup/get-settings)))
  (POST "/api/settings" {{settings "settings"} :body} (response (rt.app/setup-environment settings)))
  (GET "/api/tests" [] (response (get-tests)))
  (GET "/api/entities" [type] (response (get-entities (keyword type))))
  (GET "/api/entity" [id] (response (get-entity (keyword id))))
  (GET "/api/entity-graph" [id] (response (get-entity-graph (keyword id))))
  (POST "/api/entity" {{entity "entity"} :body} (update-entity entity))
  (GET "/api/suite-tests" [id] (response (get-suite-tests (keyword id))))
  (GET "/api/logs" [] (response (get-browser-logs)))
  (GET "/api/outfiles" [path] (response (vec (get-output-files))))
  (GET "/api/testrun" [] (response (get-test-run)))
  (GET "/api/testrun-report" [test-id] (response (get-test-run-report test-id)))
  (PUT "/api/testrun" {{test "test" suite "suite"} :body} (create-test-run suite test))
  (POST "/api/run" [] (run-to-end))
  (POST "/api/reset" [] (reset))
  (POST "/api/run-to-error" [] (run-to-error))
  (POST "/api/run-to-step" {params :body} (run-to-step params))
  (POST "/api/rerun-step" {params :body} (rerun-step params))
  (POST "/api/reset-step" {params :body} (reset-step params))
  (POST "/api/step" {params :body} (run-next params))
  (POST "/api/driver-doc" {driver :body} (response (rt.app/save-driverfn-doc driver)))
  (POST "/api/check-expr" {{ns "ns" expr "expr"} :body} (response (check-expr expr ns)))
  (POST "/api/eval-expr" {{ns "ns" expr "expr"} :body} (response (eval-expr expr ns)))
  (GET "/api/source" {{ns :ns name :name} :params} (response (get-source ns name)))
  (POST "/api/source" {{ns :ns name :name src :src} :body} (response (update-source ns name src)))
  (GET "/api/sourcefiles" {} (response (get-source-files)))
  (GET "/api/sourcefile" {{name :name} :params} (response (get-source-file name)))
  (POST "/api/sourcefile" {{name "name" src "src"} :body} (response (update-source-file name src)))
  (route/files "/out" {:root (str (System/getProperty "user.dir") "/out")})
  (route/files "/test-files" {:root (str (System/getProperty "user.dir") "/" (rt.setup/get-test-files-dir))})
  (route/resources "/")
  (route/not-found "Page not found!"))

(comment
  (rt.setup/get-settings)
  )

(def app
  (-> (handler/api app-routes)
      #_(wrap-restful-format)
      (middleware/wrap-json-body)
      (middleware/wrap-json-response)))

(def server (atom nil))

(defn start-server []
  (if @server
    (do
      (.stop @server)
      (.start @server))
    ;;else
    (reset! server (jetty/run-jetty app {:port 3000 :join? false}))))

(defn stop-server []
  (when @server
    (.stop @server)
    (reset! @server nil)))

(comment

  (reset! server nil)

  (def server (jetty/run-jetty #'app {:port 3000 :join? false}))
  (.stop server)
  (.start server)

  (start-server)
  )