(ns rt.test.db
  "The test database"
  (:require [rt.lib.util :refer [datetime-str timeit trim-leading]]
            [rt.setup :refer [get-test-db-dir get-test-runs-dir get-settings]]
            [clojure.java.io :as io]
            [clojure.string :as string]
            [clojure.edn :as edn]
            [clojure.set :as set]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import [java.io File PrintWriter]
           [java.util Date]))

;;print here so I know when this is getting cleared on me ... REPL stuff
(println "Resetting test-db")

(def test-db (atom {}))
(def test-runs-db (atom {}))

(defn get-test-db [] @test-db)
(defn get-test-runs-db [] @test-runs-db)

(defn- fixup-test-type [entity]
  (if (#{:testcase :testscenario} (:type entity))
    (assoc entity :type :test)
    entity))

(defn- fixup-test-tags [entity]
  (let [tags (set (:tags entity))]
    (if (and (#{:testsuite :test :testscript} (:type entity))
             (empty? (set/intersection tags #{:desktop :mobile :tablet})))
      (assoc entity :tags (conj tags :desktop))
      entity)))

(defn- fixup-step [step]
  (cond
    (keyword? step) {:script-id step}
    (string? step) {:script step}
    :default step))

(defn- fixup-steps [steps-key entity]
  (if-let [steps (steps-key entity)]
    (assoc entity steps-key (mapv #(assoc (fixup-step %1) :index %2) steps (range)))
    entity))

(defn add-tests
  "Add tests to the test database.
  Tests can be any entity: suites, tests, fixtures or scripts.
  Each entity should include :id and :type"
  [tests]
  {:pre [(or (seq? tests) (vector? tests))]}
  #_(debug "Loading" (count tests) "tests")
  (let [tests (->> tests
                   ;; fixup the types
                   (map fixup-test-type)
                   (map fixup-test-tags)
                   ;; add step indexes where applicable
                   (map (partial fixup-steps :steps))
                   (map (partial fixup-steps :setup))
                   (map (partial fixup-steps :teardown)))]
    (swap! test-db (fn [existing]
                     (->> tests
                          ;; map into key value pairs
                          (map #(vector (:id %) %))
                          (into existing))))
    tests))

(def entity-keys-in-listing [:type :id :name :doc :tags :source :ns :new-doc :used-by])

(defn get-test-list
  "Return a listing of all tests including descriptive properties."
  ([]
   (->> (vals (get-test-db))
        (filter #(#{:testsuite :test} (:type %)))
        (map #(select-keys % entity-keys-in-listing))))
  ([entity-type]
   {:pre [(keyword? entity-type)]}
   (if (= :driverfn entity-type)
     (->> (vals (get-test-db))
          (filter #(#{entity-type} (:type %))))
     (->> (vals (get-test-db))
          (filter #(#{entity-type} (:type %)))
          (map #(select-keys % entity-keys-in-listing))))))

(defn get-entity-list
  "Return a listing of all entities including all properties."
  ([]
   (->> (vals (get-test-db))
        (filter #(#{:testsuite :test} (:type %)))))
  ([entity-type]
   {:pre [(keyword? entity-type)]}
   (->> (vals (get-test-db))
           (filter #(#{entity-type} (:type %))))))

(defn get-test-entity
  "If the given test-id is already a record then return it. If it is an id then
  return the corresponding record in the database."
  [e]
  (if-let [entity (cond
                    (string? e) (get @test-db (keyword e))
                    (keyword? e) (get @test-db e)
                    :else e)]
    entity
    (do (error "Warning - no entity for " e)
        (throw (Exception. (format "Cannot find test entity %s" (pr-str e)))))))

(defn- clean-name [s]
  (-> s
      (string/replace #"[^a-zA-Z0-9\-\.]" "_")
      (string/replace #"^_" "")))

(defn- mark-modified [entity]
  (merge entity
         {:modified    (datetime-str)
          :modified-by "not-implemented"}))

(defn- fixup-entity-list-property [entity list-key]
  (if (list-key entity)
    (assoc entity list-key
                  (->> (list-key entity)
                       (filter #(or (keyword? %) (not-empty %)))
                       ; hack! removing the index.. we don't want to save this
                       (map #(if (map? %) (dissoc % :index :check-expr-result) %))
                       ; hack! convert script-id back to plain keyword
                       (map #(if (and (map? %) (:script-id %)) (keyword (:script-id %)) %))
                       ;; convert strings to keywords
                       (mapv #(if (string? %) (keyword (trim-leading \: %)) %))))
    entity))

(defn save-entity [{:keys [id type] :as entity}]
  {:pre [(keyword? id) (keyword? type)]}

  (let [entity (reduce fixup-entity-list-property entity
                       [:steps :setup :teardown :after-fail :after-error :after-pass
                        :before-step :after-step
                        :tests
                        :fixtures :once-fixtures :each-fixtures
                        :tags])
        db-dir (str (:test-db-dir (get-settings)) "/" (clean-name type))
        file-name (str db-dir "/" (clean-name id) ".edn")]

    ;; ensure the destination folder exists
    (.mkdir (File. db-dir))

    ;; write out to file... format nicely
    (info "Writing entity file:" file-name)
    (spit file-name (with-out-str (clojure.pprint/pprint [(mark-modified entity)])))

    ;; update the in-memory db
    (add-tests [entity])

    ;; return it
    entity))

(defn get-src-dir []
  (str (:test-db-dir (get-settings)) "/src"))

(defn ns->path [ns]
  (-> ns
      (string/replace #"[\.\\]" "/")
      (string/replace #"-" "_")))

(defn ns->dir [ns]
  (let [path (ns->path ns)
        index (.lastIndexOf (ns->path ns) "/")]
    (if (>= index 0) (subs path 0 index) "")))

(defn ns->file-name [ns]
  (str (last (string/split (ns->path ns) #"/")))
  (let [path (ns->path ns)
        index (.lastIndexOf (ns->path ns) "/")]
    (if (>= index 0) (subs path (inc index)) path)))

(defn path->ns [src-dir path]
  (let [path (if (= (.indexOf path src-dir) 0) (subs path (inc (count src-dir))) path)
        path (string/replace path #"\.clj$" "")
        path (string/replace path #"_" "-")]
    (string/replace path #"[\\/,]" ".")))

(defn get-app-driver-file-list []
  (let [src-dir (io/file (get-src-dir))]
    (->> (file-seq src-dir)
         (filter #(re-find #".*\.clj$" (.getName %)))
         (map #(path->ns (.getPath src-dir) (.getPath %))))))

(defn get-app-driver-file-list-info []
  (let [publics (fn [ns] (->> (try (ns-publics ns)
                                   (catch Exception e []))
                              (map #(meta (ns-resolve ns (key %))))
                              (map #(hash-map :ns (str (:ns %))
                                             :name (str (:name %))
                                              :line (:line %)))))]
    (->> (get-app-driver-file-list)
         (map #(hash-map :id % :publics (publics (symbol %)))))))

(defn get-app-driver-file [ns]
  (let [path (str (get-src-dir) "/" (ns->path ns) ".clj")]
    (info "Reading source file:" ns path)
    (slurp path)))

(defn save-app-driver-file [ns code]
  (let [src-dir (str (get-src-dir) "/" (ns->dir ns))
        path (str src-dir "/" (ns->file-name ns) ".clj")]
    (.mkdir (File. src-dir))
    (info "Writing source file:" ns src-dir path)
    (spit path code)
    ;; and reload it
    (load-string code)))

(defn get-test-run-list
  "Return a listing of all tests including descriptive properties,
  ordered by created date."
  ([] (->> (vals @test-runs-db)
           (sort-by :created)
           (map #(select-keys % [:id :created])))))

(defn get-test-run
  "Get the test run record for the given test run id."
  [test-run-id]
  (get @rt.test.db/test-runs-db test-run-id))

(defn get-last-test-run-id []
  (-> (get-test-run-list) last :id))

(defn get-last-test-run []
  (get-test-run (get-last-test-run-id)))

(defn get-test-script
  "Get the test script record for the given id."
  [id]
  {:post [#(= (:type %) :testscript)]}
  (get @rt.test.db/test-db id))

(defn get-test-run-scripts
  "Get a list of all the scripts of the test run, each with :test-id and :index to
  identify the script in the overall test run."
  [id]
  (let [run (get-test-run id)
        script-mapper #(merge %1 {:index %2} (select-keys %3 [:script :status :time :result]) (:counters %3))
        test-mapper #(map-indexed (partial script-mapper {:test-id (-> % :test :id)}) (:scripts %))
        results (mapcat test-mapper (:tests run))]
    results))

(defn add-test-run
  "Add the given test run to the test runs database and return the test run.
  Replaces any existing test run with the same :id"
  [test-run]
  (swap! test-runs-db (fn [test-runs]
                        (->> [test-run]
                             (map #(vector (:id %) %))
                             (into test-runs))))
  test-run)


(defn create-new-test-run-dir [id-str]
  (let [base-dir (-> (get-test-runs-dir) (or ".") (str "/" id-str))]
    (loop [n 0]
      (let [out-dir (str base-dir (if (pos? n) (str "-" n) ""))]
        (info "Creating output folder:" out-dir)
        ;; mkdir returns true if we created the folder
        ;; so if we didn't create the folder then tweak the name and try again
        ;; giving up after so many (no error, just use it)
        (if (or (.mkdir (File. out-dir)) (>= n 9))
          out-dir
          (recur (inc n)))))))

(defn create-test-run
  "Create a new test run and add it to the test run database.
  The output folder for the test run artifacts is created."
  []
  (let [date (Date.)
        id-str (str (datetime-str date) "-" (System/getenv "COMPUTERNAME"))
        out-dir (create-new-test-run-dir id-str)
        test-run {:id (keyword id-str) :type :testrun :dir-name out-dir :created date}]
    (add-test-run test-run)))

(defn update-test-run! [id f & args]
  (-> (swap! test-runs-db (fn [m] (let [tr (get m id)
                                        tr (apply f tr args)]
                                    (assoc m id tr))))
      (get id)))

(defn add-test-run-event [id event]
  (update-test-run! id #(assoc % :events (vec (conj (:events %) event)))))

(defn load-test-run-file [id]
  (let [dir-name (str (get-test-runs-dir) "/" (name id))
        f (io/file (str dir-name "/test-run.edn"))]
    (when (.exists f)
      #_(debug "Loading test run from" (.getName f))
      (try
        (-> f slurp read-string)
        (catch Exception e
          (error "Warning: exception reading testrun file for" dir-name))))))

(defn write-test-run-file [{:keys [dir-name] :as run}]
  (let [file-name (str dir-name "/test-run.edn")
        run (dissoc run :session-driver)]
    (timeit "write testrun file" (spit file-name (pr-str run)))
    run))

(defn write-pprint-test-run-file [{:keys [dir-name] :as run}]
  (let [file-name (str dir-name "/test-run-pprint.edn")]
    (timeit "write pretty printed testrun file" (spit file-name (with-out-str (clojure.pprint/pprint run))))
    run))

(defn complete-test-run [id]
  (let [tr (update-test-run! id assoc :status :finished)]
    (write-test-run-file tr)))

(defn add-tests-for-scripts-in-ns
  "Add tests for each of the suitably annotated 'script' functions in the given namespace.
  A suite is created for each of the tests.
  The suite name is the namespace name.
  The test names are based on the namespace and function names.
  Merges the given templates to define/override aspects of the created tests and suite."
  [ns test-template suite-template]
  (let [suite-name (name ns)
        suite-alias (if (re-find #"^rt\.scripts\." suite-name)
                      (subs suite-name 11)
                      suite-name)
        tests (->> (ns-publics ns)
                   (filter #(:testcase (meta (second %))))
                   (map #(merge {:id    (keyword (str suite-alias "/" (first %)))
                                 :type  :test
                                 :name  (:doc (meta (second %)))
                                 :steps [(str "(" suite-alias "/" (first %) ")")]}
                                test-template)))
        suite (merge {:id    (keyword suite-name)
                      :type  :testsuite
                      :name  suite-name
                      :tests (mapv :id tests)}
                     suite-template)]
    (info "Adding" (count tests) "tests for" ns)
    (add-tests tests)
    (add-tests [suite])))

(defn init-test-db []
  (reset! test-db {}))

(defn get-test-runs-ids-on-fs []
  (->> (.listFiles (java.io.File. "./test-runs"))
       (filter #(.isDirectory %))
       (sort-by #(.lastModified %))
       (map #(keyword (.getName %)))))

(defn init-test-runs-db []
  (let [t0 (.getTime (Date.))
        ;; 0 as don't want any past test runs now...
        test-run-ids (take 0 (reverse (get-test-runs-ids-on-fs)))]
    (reset! test-runs-db {})
    (doseq [id test-run-ids]
      (try
        (when-let [run (load-test-run-file id)]
          (add-test-run run))
        (catch Exception ex
          (error "Exception occurred loading test run record" id ", exception:" ex))))
    (info "Loaded" (count test-run-ids) "test runs in" (-> (Date.) .getTime (- t0) (/ 1000) float) "secs")))

;; thinking on some stuff here... to be tidied up

(defn get-rt-actions [ns]
  (->> (ns-publics (symbol ns))
       (filter #(:rt-action (meta (val %))))
       (map #(first %))))

(defn get-rt-queries [ns]
  (->> (ns-publics (symbol ns))
       (filter #(:rt-query (meta (val %))))
       (map #(first %))))

(defn get-rt-registry []
  (->> (filter #(re-matches #"rt\..*" (name (ns-name %))) (all-ns))
       (map #(hash-map :ns %
                       :actions (vec (get-rt-actions (str %)))
                       :queries (vec (get-rt-queries (str %)))))
       (remove #(and (empty? (:actions %)) (empty? (:queries %))))))


