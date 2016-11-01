(ns rt.session
  "A test run session"
  (require [rt.lib.util :refer [clean-filename id->str format-tc-message]]
           [rt.test.db :as db]
           [rt.test.core :as old]
           [clojure.core.async :as a :refer [chan go <!! >!! <! >! thread]]
           [clojure.pprint :refer [pprint print-table pp]]
           [clj-time.core :as t]
           [clojure.string :as string])
  (:import (java.util Date)
           (java.io File StringWriter)))

;; TODO - when to take session-id versus session as an argument - clean this up

(def test-sessions (atom {}))
(def ^:dynamic *session-id* nil)
(def ^:dynamic *tc* nil)
(def ^:dynamic *ignore-events* false)

(declare raise-event!)

(defn get-session
  ([] (get-session *session-id*))
  ([session-id] (get @test-sessions session-id)))

(defn get-new-session-id []
  (count @test-sessions))

(defn update-session!
  "Update the given session or create a new one with the given id."
  [session-id update-fn & args]
  (swap! test-sessions (fn [test-sessions]
                         (let [session (or (get test-sessions session-id)
                                           {:id session-id})]
                           (assoc test-sessions session-id (apply update-fn session args)))))
  (get-session session-id))

(defn read-expressions
  "Read one or more forms and return as a list of parsed forms ready to eval."
  [text]
  (binding [*read-eval* false]
    ;; Note - we wrap in \( and \) otherwise read-string only reads
    ;; the first form it sees.
    (read-string (str \( text \newline \)))))

(defn eval-expression!
  "Evaluate the expression with the given context binding and in the
  rt.scripts namespace."
  [session-id expr]
  ;(println "evaluating:" (pr-str expr))
  ;(println "in ns:" (find-ns 'rt.user))
  (binding [*session-id* session-id
            *ns* (find-ns 'rt.user)
            *tc* (:tc (get-session session-id))
            old/*tc* *tc*]
    (try
      (eval expr)
      (catch Exception ex
        (raise-event! session-id {:type :error :message (str ex)})
        (clojure.stacktrace/print-cause-trace ex)
        ex))))

(defn eval-expression-with-out-str!
  "Run the given expr capturing any output to *out* in a string
  and return a pair of the actual result and the out string."
  [session-id expr]
  (let [s (StringWriter.)]
    (binding [*out* s]
      [(eval-expression! session-id expr) (str s)])))

(defn create-session [{session-id :id}]
  ;; ignore the given session properties other than the id
  (let [request-chan (chan)
        session {:id                session-id
                 :created           (Date.)
                 :request-chan      request-chan
                 :next-file-counter 0}]

    ;; process all requests until the channel is closed
    (thread (loop []
              (when-let [e (<!! request-chan)]
                (println "session" session-id "running the received function")
                (try
                  (e session-id)
                  (catch Exception ex
                    (println "Exception (in loop) caught running given function in session:" ex)
                    (clojure.stacktrace/print-cause-trace ex)))
                (println "session" session-id "DONE running the received function")
                (recur))))

    session))

(defn close-session [{:keys [request-chan] :as session}]
  (a/close! request-chan)
  session)

(defn create-session!
  [] (update-session! (get-new-session-id) create-session))

(defn close-session!
  [session-id] (update-session! session-id close-session))

(defn submit!
  "Queue to run a function on the session thread and return a promise
  that is resolved with the function return value.
  The function is passed the session-id as the first argument along with
  any other arguments passed to run-on-session!"
  [session-id some-fn & args]
  (let [p (promise)
        f (fn [session-id] (deliver p (try (apply some-fn session-id args)
                                           (catch Exception ex
                                             (println "Exception caught running given function in session:" ex)
                                             (clojure.stacktrace/print-cause-trace ex)
                                             ex))))
        f (fn [session-id] (deliver p "done"))]
    (>!! (:request-chan (get-session session-id)) f)
    p))

(defn- run-fixture-handlers [session event]
  ;; run all fixture defined handlers for this event type
  ;; note - reverse order for any "after" type events
  (let [fixtures (or (and (.startsWith (str :afterTest) ":after")
                          (reverse (:fixtures session)))
                     (:fixtures session))
        handler-steps (mapcat (:type event) fixtures)]
    (binding [*ignore-events* true]
      (doseq [expr (map :expr handler-steps)]
        ;(println "running fixture handler:" expr)
        (eval-expression! (:id session) (first (read-expressions expr)))))))

(defn- add-session-event [{step-id :running-step-id :as session} event]
  (assoc session :events (conj (vec (:events session)) (merge (assoc event :created (t/now))
                                                              (when step-id {:step-id step-id})))))

(defn raise-event!
  ([event] (raise-event! *session-id* event))
  ([session-id event]
   (when-not *ignore-events*
     (let [session (update-session! session-id add-session-event event)]
       (run-fixture-handlers session event)
       session-id))))

(defn- step-execution-event? [{event-type :type}]
  (#{:error :fail :pass :beforeStep :afterStep} event-type))

(defn- step-modification-event? [{event-type :type}]
  (#{:addStep :removeStep} event-type))

(defn- related-exec-events [{:keys [events]} {:keys [step-id]}]
  (filter #(and (= step-id (:step-id %)) (step-execution-event? %)) events))

(defn- get-step-execution-summary [{:keys [events] :as session} {:keys [step-id] :as step}]
  (let [events (->> (related-exec-events session step) (sort-by :created))
        last-before-event (last (filter #(= :beforeStep (:type %)) events))
        last-after-event (last (filter #(= :afterStep (:type %)) events))
        last-exec-events (drop-while #(t/before? (:created %) (:created last-before-event)) events)
        type-counts (select-keys (frequencies (map :type last-exec-events)) [:error :fail :pass])]
    (when-not (empty? last-exec-events)
      (merge
        {:events  (count last-exec-events)
         :msecs   (t/in-millis (t/interval (:created (first last-exec-events)) (:created (last last-exec-events))))
         :counts  type-counts
         :status  (cond
                    (not last-after-event) :running
                    (:error type-counts) :error
                    (:fail type-counts) :fail
                    (:pass type-counts) :pass
                    :default :done)
         :started (:created last-before-event)}
        (when last-after-event
          {:result   (:result last-after-event)
           :out      (:out last-after-event)
           :finished (:created last-after-event)})))))

(defn fixup-step [step]
  ;(println "before fixup:" step)
  (let [step (cond
               (string? step) {:expr step}
               (keyword? step) {:script step}
               (and (map? step) (:script step)) {:expr (:script step)}
               :default step)]
    ;(println "after fixup:" step)
    (dissoc step :wait-ng :target-msecs)))

(defn get-steps [session]
  ;; todo - add recursive resolving of scripts in scripts
  ;; todo - need to add any scripts used to the session so they do not change between calls to get-steps..
  (let [get-entity-steps (fn [step-type owner]
                           (mapcat (fn [step index]
                                     (let [step (fixup-step step)]
                                       (if (:script step)
                                         (let [script (db/get-test-entity (:script step))]
                                           ;(println "getting script" script)
                                           (map #(assoc (fixup-step %1) :id (:script step) :type step-type :index %2)
                                                (:steps script) (range)))
                                         [(assoc step :id (:id owner) :type step-type :index index)])))
                                   (step-type owner) (range)))
        original-steps (->> (concat (mapcat (partial get-entity-steps :setup) (:fixtures session))
                                    (get-entity-steps :steps (:test session))
                                    (mapcat (partial get-entity-steps :teardown) (reverse (:fixtures session))))
                            (map-indexed #(assoc %2 :step-id %1)))
        make-step-id (partial + (count original-steps))
        edit-events (->> (filter step-modification-event? (:events session))
                         (map-indexed #(assoc-in %2 [:step :step-id] (make-step-id %1))))
        apply-edit-event (fn [steps event]
                           (condp = (:type event)
                             :addStep (let [[a b] (split-with #(not= (:before-id event) (:step-id %)) steps)]
                                        (concat a [(:step event)] b))
                             :removeStep (remove #(= (:step-id event) (:step-id %)) steps)))]
    (->> (reduce apply-edit-event original-steps edit-events)
         (map #(merge % (get-step-execution-summary session %))))))

(defn get-events [session]
  (:events session))

(defn set-session-test
  ([session test] (set-session-test session test nil))
  ([session test {:keys [suites fixtures]}]
    ;; the test itself may define fixtures and any suites may have fixtures so include them all
    ;; and do it in the order of suite then test then those in the given context such that the
   (let [fixtures (concat [] (mapcat :fixtures suites) (:fixtures test) fixtures)]
     (assoc session :test test
                    :fixtures fixtures
                    :tc (reduce #(merge %1 (:data %2)) (:tc session) fixtures)
                    :events []))))

(defn set-session-test! [session-id test & [context]]
  (update-session! session-id set-session-test test context))

(defn get-step-by-id [session step-id]
  (first (filter #(= step-id (:step-id %)) (get-steps session))))

(defn get-first-step [session]
  (first (get-steps session)))

(defn get-next-step [session {:keys [step-id]}]
  (fnext (drop-while #(not= step-id (:step-id %)) (get-steps session))))

(defn get-last-executed-step [session]
  ;; find the step from the current active steps that was most recently executed
  ;; (remembering that test steps may exist in the test, but not be active due to :removeStep events)
  (let [step-ids (into #{} (filter identity (map :step-id (get-steps session))))]
    (some->> (:events session)
             (filter step-execution-event?)
             (reverse)
             (map :step-id)
             (some #(step-ids %))
             (get-step-by-id session))))

(defn get-step [session which]
  (cond
    (number? which) (get-step-by-id session which)
    (= :next which) (if-let [step (get-last-executed-step session)]
                      (get-next-step session step)
                      (get-first-step session))
    (= :last which) (get-last-executed-step session)
    (= :first which) (get-first-step session)
    ;; assume already a step
    :default which))

(defn add-step [session step before-step-id]
  ;; clear step id in case the given step is an existing step being added with modifications
  (let [step (dissoc step :step-id)]
    (add-session-event session {:type :addStep :step step :before-id before-step-id})))

(defn remove-step [session step-id]
  (add-session-event session {:type :removeStep :step-id step-id}))

(defn add-step!
  ([session-id step] (add-step! session-id step nil))
  ([session-id step before-step-id] (update-session! session-id add-step step before-step-id)))

(defn remove-step! [session-id step-id]
  (update-session! session-id remove-step step-id))

(defn update-step! [session-id step new-step]
  (let [{:keys [step-id]} step
        {before-step-id :step-id} (get-next-step (get-session session-id) step)]
    (add-step! session-id new-step before-step-id)
    (remove-step! session-id step-id)))

(defn get-test-status [session]
  (let [steps (get-steps session)]
    (cond
      (some #(#{:error} (:status %)) steps) :error
      (some #(#{:fail} (:status %)) steps) :fail
      (not-any? #(nil? (:status %)) steps) :done
      :default :not-done)))

(defn- do-before-step! [session-id {:keys [step-id]}]
  (update-session! session-id #(assoc % :running-step-id step-id))
  (raise-event! session-id {:type :beforeStep})
  session-id)

(defn- do-after-step! [session-id {:keys [step-id]} event-details]
  (raise-event! session-id (merge {:type :afterStep} event-details))
  (update-session! session-id #(dissoc % :running-step-id))
  session-id)

(defn run-step! [session-id step]
  (when-let [step (get-step (get-session session-id) step)]
    (let [{:keys [expr step-id]} step
          form (first (read-expressions expr))
          temp (do-before-step! session-id step)
          [result out] (eval-expression-with-out-str! session-id form)
          temp (do-after-step! session-id step {:result result
                                                :out    out
                                                :expr   (pr-str form)})]
      [step result out])))

(defn run-test! [session-id]
  (doseq [step (get-steps (get-session session-id))]
    (let [status (get-test-status (get-session session-id))]
      (when (or (not= :error status)
                (= :teardown (:type step)))
        (let [[step result out] (run-step! session-id step)]
          (println "Just ran step " step "=>" result "=out>" out)))))
  (get-test-status (get-session session-id)))

(defn make-artifact-filename
  ;;todo complete this
  ([suffix] (make-artifact-filename *session-id* suffix))
  ([session-id suffix] (let [session (get-session session-id)
                             ;; base name on test and current step index
                             {:keys [test running-step-id]} session
                             step (get-step-by-id session running-step-id)
                             filename (if step
                                        (str (id->str (:id step)) "-" (id->str (:type step)) "-" (:index step))
                                        (str (id->str (:id test))))]

                         (update-session! (:id session)
                                          #(assoc % :next-file-counter (inc (:next-file-counter session))))
                         (clean-filename (str filename "-" (:next-file-counter session) suffix)))))

(defn get-test-id
  ([] (get-test-id *session-id*))
  ([session-id] (let [session (get-session session-id)]
                  (id->str (:id (:test session))))))

(defn get-suite-id
  ([] (get-test-id *session-id*))
  ([session-id] (let [session (get-session session-id)]
                  (id->str (:id (first (:suites session)))))))

(defn get-test-duration
  ([] (get-test-duration *session-id*))
  ([session-id] (let [session (get-session session-id)
                      steps (get-steps session)]
                  (reduce + 0 (filter identity (map :msecs steps))))))

(defn get-post-test-messages
  ([] (get-post-test-messages *session-id*))
  ([session-id] (let [session (get-session session-id)
                      test-id (get-test-id session-id)
                      output (->> (get-events session)
                                  (filter :out)
                                  (map :out)
                                  (apply str))]
                  (concat
                    ;; did we fail
                    (when-let [{:keys [expected actual]} (->> (get-events session)
                                                              (filter #(#{:error :fail} (:type %)))
                                                              (sort-by :created)
                                                              first)]
                      [(format-tc-message :testFailed {:name     test-id
                                                       :message  "FAIL" :details ""
                                                       :expected expected :actual actual})])
                    ;; grab all output
                    [(format-tc-message :testStdOut {:name test-id :out output})
                     (format-tc-message :testFinished {:name test-id :duration (get-test-duration session-id)})]))))

;;-----------------------------------------------------------------------------------------
;;; helpers for tests

(defn- make-step [form]
  {:expr (pr-str form)})

(def sample-scripts [{:id    :script-1 :type :testscript
                      :steps [(make-step '(println "step 1 from script 1"))
                              (make-step '(println "step 2 from script 1"))]}])

(def sample-test {:id    :my-test-1 :type :test
                  :steps [(make-step '(println "step 1"))
                          (make-step '(Thread/sleep 500))
                          {:script :script-1}
                          (make-step '(expect-equals 2 3 "cats and dogs!"))
                          (make-step '(/ 9 0))
                          (make-step '(println "all done"))]})

(def sample-fixtures [{:id       :fixture-1 :type :testfixture
                       :data     {:target :chrome :username "Administrator"}
                       :setup    [(make-step '(println "logging in...." (:username *tc*)))]
                       :teardown [(make-step '(println "teardown 1"))]}
                      {:id         :fixture-2 :type :testfixture
                       :beforeStep [(make-step '(println "...before step..."))]
                       :afterStep  [(make-step '(println "...after step..."))]
                       :beforeTest [(make-step '(println "...before test..."))]
                       :afterTest  [(make-step '(println "...after test..."))]
                       :error      [(make-step '(println "ERROR"))]
                       :fail       [(make-step '(println "FAIL - writing to file:" (make-artifact-filename ".png")))]
                       :pass       [(make-step '(println "PASS"))]}
                      {:id        :fixture-3 :type :testfixture
                       :afterStep [(make-step '(println "...after step (fixture 2)..."))]}
                      {:id          :teamcity-messages :type :testfixture
                       :beforeSuite [(make-step '(println (format-tc-message :testSuiteStarted {:name (get-suite-id)})))]
                       :afterSuite  [(make-step '(println (format-tc-message :testSuiteFinished {:name (get-suite-id)})))]
                       :beforeTest  [(make-step '(println (format-tc-message :testStarted {:name (get-test-id)})))]
                       ;; todo - work out way to emit single testFailed, testStdOut, testStdErr messages
                       :afterTest   [(make-step '(doall (map println (get-post-test-messages))))]}])

(defn get-recent-session []
  (or (get-session (last (sort (keys @test-sessions))))
      (create-session!)))

(defn get-recent-session-id []
  (:id (get-recent-session)))

(defn print-session-steps [session]
  (print-table [:type :id :index :step-id :expr :status :result :msecs :events :finished]
               (get-steps session)))

(defn print-step-output [session step-ref]
  (when-let [out (:out (get-step session step-ref))]
    (pprint (clojure.string/split out #"\r\n"))))

(defn create-sample-session []
  (let [{session-id :id} (get-recent-session)]
    (db/add-tests sample-scripts)

    ;; test and explicit fixtures
    (set-session-test! session-id sample-test {:fixtures sample-fixtures})
    ;; test that includes fixtures
    #_(set-session-test! session-id (merge test {:fixtures fixtures}))
    ;; fixtures in the suites
    #_(set-session-test! session-id test {:suites [{:id :suite1 :fixtures fixtures}]})))

(comment

  ;; my todo list

  ;- add ability to add/remove/update steps - DONE
  ;- expose test ids and paths (suites) to the handlers so can build file names etc - DONE
  ;- counter for file naming - DONE

  ;- run tests .. run to end, run to error
  ;- handler for generating start/finish suite and test messages
  ;- run a test (to error or end) but allow stopping
  ;- setup steps return values to merge into test context
  ;- run scripts
  ;- run suites
  ;- save step mods back to the test
  ;- use some existing tests etc from the existing test.db entities (don't worry about datomic now)

  )

(comment

  (pprint (vals @test-sessions))
  (pprint (keys @test-sessions))

  (pp)
  (print-table *1)
  (.printStackTrace *e)

  ;; minimal eval of an expression on a session
  ;; - only events are captured

  (let [{session-id :id} (get-recent-session)
        forms (read-expressions "(expect-equals 2 3 \"cats and dogs!\")")]
    @(submit! session-id #(eval-expression! % (first forms))))

  (get-events (get-recent-session))

  ;; run a step on a session
  ;; - a test context and event handlers need to be established

  (create-sample-session)

  (get-steps (get-recent-session))
  (print-session-steps (get-recent-session))

  (get-events (get-recent-session))
  (pprint (get-events (get-recent-session)))
  (print-table (get-events (get-recent-session)))

  ;; run using keyword
  (run-step! (get-recent-session-id) :next)
  (submit! (get-recent-session-id) run-step! :next)
  (submit! (get-recent-session-id) run-step! :first)
  (submit! (get-recent-session-id) run-step! :last)
  ;; run using index
  (submit! (get-recent-session-id) run-step! 0)
  (submit! (get-recent-session-id) run-step! 3)

  ;; print the output of a step
  (print-step-output (get-recent-session) 3)

  ;; run using step
  (submit! (get-recent-session-id) run-step! (make-step '(println *tc*)))

  ;; add and remove steps
  (add-step! (get-recent-session-id) (make-step '(println "a new step")) nil)
  (submit! (get-recent-session-id) run-step! 5)
  (remove-step! (get-recent-session-id) 4)

  ;; update existing step

  (let [step (get-step-by-id (get-recent-session) 3)
        step' (assoc step :expr (pr-str '(println "an edited step")))]
    (update-step! (get-recent-session-id) step step'))

  (get-last-executed-step (get-recent-session))

  ;; run a test

  (do (create-sample-session)
      (run-test! (get-recent-session-id))
      (print-session-steps (get-recent-session)))

  (do (create-sample-session)
      @(submit! (get-recent-session-id) run-test!)
      (print-session-steps (get-recent-session)))

  (get-post-test-messages (get-recent-session-id))


  )

(comment

  ;; playing with things

  (->> (read-expressions
         "(do
             (println \"help\")
             (* 5 (+ 5 6)))

         ;; and this is the next step

         (println \"bye\")
         ")
       (map #(pr-str %)))

  (->> (read-expressions "(println \"help\") 3 ;; what do you think of that
  (+ 3 5) (comment sfasfdasf)")
       (map #(do (println %) %))
       (map #(eval-expression! 0 %)))

  (pprint @test-sessions)

  (create-session!)
  (doseq [session-id (keys @test-sessions)]
    (update-session! session-id close-session))
  (get-session 0)

  (raise-event! 0 {:error 99})

  ;; minimal eval of an expression on a session
  ;; only events are captured
  (let [{session-id :id} (get-recent-session)]
    (submit! session-id
             #(eval-expression!
               session-id
               (first (read-expressions "(expect-equals 2 3 \"cats and dogs!\")")))))

  (let [{session-id :id} (get-recent-session)]
    (submit! session-id
             #(eval-expression!
               session-id
               (first (read-expressions "(println \"cats and dogs!\")")))))

  (let [{session-id :id} (get-recent-session)]
    (submit! session-id
             #(run-step! session-id
                         {:expr "(println 999999)"})))

  ;; playing around with a local atom

  (let [events (atom [])
        add-event (fn [e] (swap! events #(conj % e)))]
    (binding [*tc* {:onError #(add-event {:error %})}]
      (println "tc=" *tc*)
      ((:onError *tc*) 999)
      ((:onError *tc*) 995))
    (println "events=" @events))

  ;; playing around with channels

  (do
    (let [session {:request-chan  (chan)
                   :response-chan (chan)}
          init (fn [{:keys [request-chan response-chan] :as session}]
                 (go (loop []
                       (when-let [e (<! response-chan)]
                         (println "got response" e)
                         (recur))))

                 (thread (loop []
                           (when-let [e (<!! request-chan)]
                             (println "got request" e)
                             (Thread/sleep 3000)
                             (println "send response" e)
                             (>!! response-chan e)
                             (recur))))
                 (assoc session :something-else "lkjhasdfklashklfh"))
          session (init session)]

      (println session)

      (>!! (:request-chan session) "hello")

      (Thread/sleep 5000)
      (println "closing")
      (a/close! (:request-chan session))))

  )
