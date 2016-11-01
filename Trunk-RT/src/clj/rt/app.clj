;; This file is to be cleaned up. It is mostly the result of a lot of playing around.
(ns rt.app
  "The main namespace for the ReadiTest app."
  (:require rt.lib.wd
            [rt.lib.util :refer [get-fqdn write-csv-objects ms->s datetime-str trim-leading timeit]]
            [rt.setup :refer :all]
            rt.test.core
            rt.test.db
            [clojure.string :as string]
            [clojure.java.io :as io]
            [clojure.walk :as walk]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import [java.io File FileNotFoundException FileOutputStream]
           [java.util Date]
           (java.util.zip ZipFile)))

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; working with a zip file

;(defn entries [zip-stream]
;  (take-while #(not (nil? %))
;              (repeatedly #(.getNextEntry zip-stream))))
;
;(defn copy-file [zip-stream filename]
;  (with-open [out-file (io/output-stream filename)]
;    (let [buff-size 4096
;          buffer (byte-array buff-size)]
;      (loop [len (.read zip-stream buffer)]
;        (when (> len 0)
;          (.write out-file buffer 0 len)
;          (recur (.read zip-stream buffer)))))))
;
;(defn extract-stream [zip-stream to-folder]
;  (let [extract-entry (fn [zip-entry]
;                        (when (not (.isDirectory zip-entry))
;                          (let [to-file (io/file to-folder (.getName zip-entry))
;                                parent-file (io/file (.getParent to-file))]
;                            (.mkdirs parent-file)
;                            (copy-file zip-stream to-file))))]
;    (->> zip-stream
;         entries
;         (map extract-entry)
;         dorun)))

(defn- map-zip [map-fn file-name]
  (with-open [zf (ZipFile. file-name)]
    (->> (enumeration-seq (.entries zf))
         (map (partial map-fn zf))
         (into []))))

(defn- extract-zipentry [zf ze dir]
  (let [to-file (io/file dir (.getName ze))]
    (io/make-parents to-file)
    (with-open [in (.getInputStream zf ze)
                out (io/output-stream to-file)]
      (io/copy in out))
    (.getName ze)))

(defn- safe-delete [file-path]
  (if (.exists (clojure.java.io/file file-path))
    (try
      (clojure.java.io/delete-file file-path)
      (catch Exception e (str "exception: " (.getMessage e))))
    false))

(defn- delete-directory [directory-path]
  (let [directory-contents (file-seq (clojure.java.io/file directory-path))
        files-to-delete (filter #(.isFile %) directory-contents)]
    (doseq [file files-to-delete]
      (safe-delete (.getPath file)))
    (safe-delete directory-path)))

(defn- extract-zipfile [zip-name dir-name]
  ;;do not clear ... causes problems when running multiple sessions
  ;; leave to something else to clear this
  ;;(delete-directory dir-name)
  (map-zip #(extract-zipentry %1 %2 dir-name) zip-name))

(comment
  (extract-zipfile "RnTests.zip" "RnTestsUnzipped")
  (count (enumeration-seq (.entries (ZipFile. "//spdevnas01.sp.local/Development/Shared/RnTests/RnTests.zip"))))

  )

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(defn get-app-drivers []
  (let [rt-namespace? (fn [ns] (or (= 0 (.indexOf (str ns) "rt.po"))
                                   (= 0 (.indexOf (str ns) "rt.scripts"))
                                   (= 0 (.indexOf (str ns) "rn."))))
        get-fn-meta (fn [ns] (map #(meta (ns-resolve ns (key %))) (ns-publics ns)))]
    ;; get all the driver (and script) functions extracting from code
    ;; and then override with any docs (that we have collected in the test-db)
    ;; then do some stuff to help with csv export...
    (->> (all-ns)
         (filter rt-namespace?)
         (mapcat get-fn-meta))))

(defn get-app-driver-list
  "Get the app driver function list."
  []
  (let [exprs (fn [e] (->> (mapcat #(% e) [:steps :setup :teardown])
                           (map :script)))
        entity-pr-strs (->> (vals @rt.test.db/test-db)
                            (filter #(#{:testscript :test :testfixture} (:type %)))
                            (map #(hash-map :id (:id %) :text (pr-str (exprs %)))))
        used-by (fn [n] (->> entity-pr-strs
                             (filter #(some-> (:text %) (.indexOf n) (>= 0)))
                             (map :id)))
        fndocs (->> (vals (rt.test.db/get-test-db))
                    (filter #(#{:driverfn} (:type %))))
        apply-updated-doc (fn [{:keys [ns name] :as m}]
                            (let [fndoc (first (filter #(and (= ns (:ns %)) (= name (:name %))) fndocs))]
                              (if fndoc
                                (assoc m :doc (:doc fndoc))
                                m)))]
    ;; get all the driver (and script) functions extracting from code
    ;; and then override with any docs (that we have collected in the test-db)
    ;; then do some stuff to help with csv export...
    (->> (get-app-drivers)
         ;; ensure all fields are in all objects for the write csv function
         (map #(merge {:rt-action false :rt-query false :rt-completed nil :rt-tags nil} %))
         ;; the conj bit is needed so we get the keys in the expected order
         (map #(conj {} (select-keys % [:ns :name :doc :arglists :line
                                        :rt-action :rt-query :rt-completed :rt-tags])))
         (map #(let [ns (str (:ns %)) name (str (:name %)) id (str ns "/" name)]
                (assoc % :id id :ns ns :name name :used-by (used-by id))))
         (map apply-updated-doc)
         (sort-by :id))))

(defn write-app-driver-list-csv
  "Write the app driver function list to a csv."
  []
  (->> (get-app-driver-list)
       (write-csv-objects "app-driver-functions.csv")
       #_(clojure.pprint/print-table)))

(comment
  (let [drivers (get-app-driver-list)]
    (println "count=" (count drivers))

    (->> drivers
         (map #(println (get-in % [:used-by]) "."))
         doall)

    (println (keys (first drivers)))
    (println "done"))

  (let [exprs (fn [e] (->> (mapcat #(% e) [:steps :setup :teardown])
                           (map :script)))
        entity-pr-strs (->> (vals @rt.test.db/test-db)
                            (filter #(#{:testscript :test :testfixture} (:type %)))
                            (map #(hash-map :id (:id %) :text (pr-str (exprs %)))))]
    (println (first entity-pr-strs)))

  )

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; the following are used during application startup

(defn load-tests-from-dir [dir]
  (let [d (io/file dir)
        files (file-seq d)
        files (filter #(re-find #".*\.edn$" (.getName %)) files)]
    (doseq [f files]
      (when (.exists f)
        ;(debug "Loading tests from" (.getName f))
        (->> (-> f slurp read-string)
             (map #(assoc % :source dir))
             (rt.test.db/add-tests))))))

(defn load-file-from-dir
  "load file and return nil if it succeeds otherwise return the error message"
  [f]
  ;(debug "Loading src from" (.getPath f))
  (try
    (-> f slurp load-string)
    nil
    (catch Exception e
      ;(error "exception loading" f e)
      (if-let [m (second (re-find #"not locate (.*)__init.class" (.getMessage e)))]
        (str ">>Warning: Failed to load source: " (.getName f)
             " - cannot find referenced file: " m " - will retry...")
        (str ">>Warning: Exception loading source: " (.getName f) " - " e)))))

(defn load-src-from-dir [dir]
  ;; load the files multiple times as a quick and dirty way to deal with
  ;; dependency issues with load order
  (let [files (file-seq (io/file dir))
        files (filter #(re-find #".*\.clj$" (.getName %)) files)
        ;; ... and with one minor help of doing files with common or core first
        files (concat (filter #(re-find #"(common)|(core)" (.getName %)) files)
                      (remove #(re-find #"(common)|(core)" (.getName %)) files))
        ;; ... and scratch last
        files (concat (remove #(re-find #"scratch" (.getPath %)) files)
                      (filter #(re-find #"scratch" (.getPath %)) files))]
    (let [failed-files (loop [files files n 10]
                         (infof "Loading source files, count=%d, retries remaining=%d" (count files) n)
                         (let [failed-files (reduce #(if-let [m (load-file-from-dir %2)]
                                                      (conj %1 {:f %2 :m m}) ; load returned an error message
                                                      %1    ; load returned nil to indicate success
                                                      )
                                                    [] files)]
                           (if (and (not-empty failed-files) (pos? n))
                             (recur (map :f failed-files) (dec n))
                             failed-files)))]
      (when (not-empty failed-files)
        (throw (Exception. (apply str "Failed to load driver function source files: " \newline
                                  (interpose \newline (map pr-str failed-files)))))))))

;; NOT COMPLETE
;(defn load-file-from-zip
;  "load file and return nil if it succeeds otherwise return the error message"
;  [zf f]
;  (println "Loading src from zip" (.getName zf) f)
;  (try
;    (with-open [in (.getInputStream zf (.getEntry zf f))
;                out (io/output-stream (byte-array []))]
;      (io/copy in out)
;      (-> out load-reader))
;    nil
;    (catch Exception e
;      ;(println "exception loading" f e)
;      (if-let [m (second (re-find #"not locate (.*)__init.class" (.getMessage e)))]
;        (str ">>Warning: Failed to load source: " f
;             " - cannot find referenced file: " m " - will retry...")
;        (str ">>Warning: Exception loading source: " f " - " e)))))
;
;(defn load-src-from-zip [zf-name]
;  (with-open [zf (ZipFile. zf-name)]
;    ;; load the files multiple times as a quick and dirty way to deal with
;    ;; dependency issues with load order
;    (let [files (map #(.getName %) (enumeration-seq (.entries zf)))
;          files (filter #(re-find #".*\.clj$" %) files)
;          ;; ... and with one minor help of doing files with common or core first
;          files (concat (filter #(re-find #"(common)|(core)" %) files)
;                        (remove #(re-find #"(common)|(core)" %) files))
;          ;; ... and scratch last
;          files (concat (remove #(re-find #"scratch" %) files)
;                        (filter #(re-find #"scratch" %) files))]
;      (let [failed-files (loop [files (take 2 files) n 2]
;                           (println "Loading source files, count=" (count files) ", retries remaining=" n)
;                           (let [failed-files (reduce #(if-let [m (load-file-from-zip zf %2)]
;                                                        (conj %1 {:f %2 :m m}) ; load returned an error message
;                                                        %1  ; load returned nil to indicate success
;                                                        )
;                                                      [] files)]
;                             (if (and (not-empty failed-files) (pos? n))
;                               (recur (map :f failed-files) (dec n))
;                               failed-files)))]
;        (when (not-empty failed-files)
;          (throw (Exception. (apply str "Failed to load driver function source files: " \newline
;                                    (interpose \newline (map pr-str failed-files))))))))))

(comment
  (load-src-from-zip "RnTests.zip")
  )

(defn load-tests-from-dir-save-back [dir]
  (let [d (io/file dir)
        files (file-seq d)
        files (filter #(re-find #".*\.edn$" (.getName %)) files)]
    (doseq [f files]
      (when (.exists f)
        ;(debug "Loading tests from" (.getName f))
        (doall (->> (-> f slurp read-string)
                    (map #(assoc % :source dir))
                    (rt.test.db/add-tests)
                    (map #(rt.test.db/save-entity %))))))))

(def running-jar
  "Resolves the path to the current running jar file."
  (-> :keyword class (.. getProtectionDomain getCodeSource getLocation getPath)))

(defn list-jar-resources [path]
  (let [jar (java.util.jar.JarFile. path)
        entries (.entries jar)]
    (loop [result []]
      (if (.hasMoreElements entries)
        (recur (conj result (.. entries nextElement getName)))
        result))))

(defn load-tests-from-resources []
  (let [test-db-files (list-jar-resources running-jar)
        test-file? #(and (.startsWith % "test-db/") (.endsWith % ".edn"))]
    (doseq [f (filter test-file? test-db-files)]
      (->> (-> f io/resource slurp read-string)
           (map #(assoc % :source :jar))
           (rt.test.db/add-tests)))))

(defn load-tests-from-data-dir []
  (when-let [db-dir (:test-db-dir (get-settings))]
    ;; use the save-back version if wanting to re-write the tests after loading
    (load-tests-from-dir db-dir)))

(defn load-src-from-data-dir []
  (when-let [db-dir (:test-db-dir (get-settings))]
    (load-src-from-dir db-dir)))

(defn load-test-entities [text load-fn & args]
  (let [t0 (.getTime (Date.))
        n0 (count @rt.test.db/test-db)
        result (apply load-fn args)]
    (infof "Loaded %d entities from %s in %.1f secs" (- (count @rt.test.db/test-db) n0) text (ms->s (- (.getTime (Date.)) t0)))
    result))

(defn load-src [text load-fn & args]
  (let [t0 (.getTime (Date.))
        n0 (count (get-app-drivers))
        result (apply load-fn args)
        n1 (count (get-app-drivers))]
    (infof "Loaded %d driver functions from %s in %.1f secs" (- n1 n0) text (ms->s (- (.getTime (Date.)) t0)))
    result))

(defn make-driverfn-id [{:keys [ns name]}]
  (keyword (str ns "/" name)))

(defn load-driver-functions-as-entities []
  (->> (get-app-driver-list)
       (map #(assoc % :type :driverfn :id (make-driverfn-id %)))
       rt.test.db/add-tests))

(defn get-shared-test-db-dirs []
  (let [dirs (:shared-db-dirs (get-settings))
        dirs (if (sequential? dirs) dirs [dirs])
        ;; remove the primary as we deal with it separately
        dirs (remove #(= % (:test-db-dir (get-settings))) dirs)
        dirs (remove empty? dirs)]
    dirs))

(defn- make-pkg-folder-name [pkg]
  (-> (io/file pkg)
      .getName
      (str ".unzipped")))

(defn- update-settings! [options]
  (let [
        ;; init tenants with tenant if needed... backward compat reasons
        ;; and ensure the tenant reflects the head of the tenants setting
        tenant (or (:tenant options) (:tenant (get-settings)))
        tenants (:tenants options)
        tenants (if (not-empty tenants) tenants [tenant])
        options (assoc options :tenants (into [] (remove empty tenants))
                               :tenant (first tenants))]

    (info "Running with options:" options)

    (update-settings options)

    ;; note - the following only updates the app-url in the config file
    (update-settings-file "config.edn")

    ;; fixup app-url
    (update-settings {:app-url (string/replace (get-app-url) "localhost" (get-fqdn))})))

(defn setup-environment
  "Perform all setup required to prepare to run tests. This includes reading configuration,
  extracting browser drivers and loading test definitions and previous test run entities."
  [& [options]]

  ;; Extract/copy various setup files to the file system
  (extract-setup-resource "config.edn" ".")
  (extract-setup-resource "state.edn" ".")
  (extract-setup-resource "chromedriver.exe" "./bin")
  (extract-setup-resource "chromedriver" "./bin")
  (extract-setup-resource "IEDriverServer.exe" "./bin")
  (extract-setup-resource "phantomjs.exe" "./bin")

  (load-settings "config.edn")
  (load-state "state.edn")

  (update-settings! (walk/keywordize-keys options))

  (info "Running with settings:" (get-settings))

  #_(println "a: with used-by" (->> (vals (rt.test.db/get-test-db))
                                    (filter #(< 0 (count (:used-by %))))
                                    count))

  (.mkdir (File. (get-test-runs-dir)))
  (.mkdir (File. (get-test-db-dir)))
  (.mkdir (File. (get-test-files-dir)))

  (doseq [pkg (:test-pkgs (get-settings))]
    (timeit "Extracting zip" (extract-zipfile pkg (make-pkg-folder-name pkg))))

  (rt.test.db/init-test-db)
  (rt.test.db/init-test-runs-db)

  (let [test-pkgs (:test-pkgs (get-settings))]
    (if (not-empty test-pkgs)
      (doseq [pkg test-pkgs]
        (load-src (format "test pkg: \"%s\"" pkg) load-src-from-dir (str (make-pkg-folder-name pkg) "/test-db")))
      ;;else
      (doseq [dir (get-shared-test-db-dirs)]
        (load-src (format "shared db folder: \"%s\"" dir) load-src-from-dir dir))))

  (load-src (format "db folder: \"%s\"" (get-test-db-dir)) load-src-from-data-dir)

  (load-test-entities "driver functions in code" load-driver-functions-as-entities)

  #_(println "b: with used-by" (->> (vals (rt.test.db/get-test-db))
                                    (filter #(< 0 (count (:used-by %))))
                                    count))

  (load-test-entities "jar file" load-tests-from-resources)
  (load-test-entities "resources folder" load-tests-from-dir "resources/test-db")

  (let [test-pkgs (:test-pkgs (get-settings))]
    (if (not-empty test-pkgs)
      (doseq [pkg test-pkgs]
        (load-test-entities (format "test pkg: \"%s\"" pkg) load-tests-from-dir (str (make-pkg-folder-name pkg) "/test-db")))
      ;;else
      (doseq [dir (get-shared-test-db-dirs)]
        (load-test-entities (format "shared db folder: \"%s\"" dir) load-tests-from-dir dir))))

  (load-test-entities (format "db folder: \"%s\"" (get-test-db-dir)) load-tests-from-data-dir)

  ;; doing it again to get the usage counts... urg
  (load-test-entities "driver functions in code" load-driver-functions-as-entities)

  #_(println "c: with used-by" (->> (vals (rt.test.db/get-test-db))
                                    (filter #(< 0 (count (:used-by %))))
                                    count))

  true)

(comment

  (setup-environment {:test-pkgs ["./RnTests.zip"]})
  )

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; more tinkering with stuff here....

;; todo fix this up .. tying to do a single call to setup where subsequent will block
(def setup-future-atom (atom nil))
(def setup-future-atom-lock (atom 0))
(defn wait-for-setup [& [options]]
  (when (= 1 (swap! setup-future-atom-lock inc))
    (reset! setup-future-atom (future (rt.app/setup-environment options))))
  ;; yuck, but just getting around an issue at the moment
  (Thread/sleep (+ 100 (rand-int 100)))
  @@setup-future-atom)

(comment
  (do (reset! setup-future-atom nil)
      (reset! setup-future-atom-lock 0))
  )

(defn setup-environment-with-wait [options]
  (wait-for-setup options))

(defn get-tests []
  (wait-for-setup)
  (rt.test.db/get-test-list))

(defn get-entities [type]
  (let [s (str "get-entities(" type ")")]
    (timeit (str s ":setup") (wait-for-setup))
    (timeit (str s ":get") (rt.test.db/get-test-list type))))

(defn get-entity [id]
  (info "get-entity for id" id)
  (wait-for-setup)
  (let [entity (rt.test.db/get-test-entity id)
        ;; fix up things like steps... should be doing when loading
        ;; AND this should be recursive, but since only here for loading old data formats
        entity (reduce (fn [entity step-key]
                         (if (step-key entity)
                           (assoc entity step-key (mapv #(if (string? %) {:script %} %) (step-key entity)))
                           entity))
                       entity [:steps :setup :teardown :after-fail :after-error :after-pass :before-step :after-step])]
    (info "get-entity for id" id "=>" entity)
    entity))

(defn get-entity-graph [id]
  (let [entity (get-entity id)]
    (rt.test.core/get-test-record entity)))

(defn update-entity
  "update the entity, knowing it came from a json based webservice and needing some cleaning up"
  [entity]
  (info "updating entity:" entity)
  (when entity
    ;; fix up the incoming entity converting strings to keywords where needed
    (let [{:keys [id type] :as entity} (walk/keywordize-keys entity)
          entity (assoc entity :id (keyword id) :type (keyword type))]
      (rt.test.db/save-entity entity)))
  "ok")

(defn get-browser-logs []
  (rt.lib.wd/get-browser-logs))

(defn save-driverfn-doc [entity]
  (info "save driver function doc:" entity)
  (when entity
    ;; fix up the incoming entity converting strings to keywords where needed
    (let [entity (walk/keywordize-keys entity)
          id (make-driverfn-id entity)
          type "driverfn"
          entity (assoc entity :id (keyword id) :type (keyword type))]
      (rt.test.db/save-entity entity)))
  "ok")
