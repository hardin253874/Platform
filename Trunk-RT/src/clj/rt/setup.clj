(ns rt.setup
  "Environment and settings."
  (:require [clojure.java.io :as io]
            [clojure.string :as string]
            clojure.pprint
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]
            [taoensso.timbre :as timbre]
            [taoensso.timbre.appenders.core :as appenders])
  (:import [java.io File PrintWriter PushbackReader FileNotFoundException]
           (java.util Properties)))

(def ^:private app-settings (atom {}))
(def ^:private app-state (atom {}))

(comment
  (reset! app-settings {})
  )

(defn get-settings [] @app-settings)
(defn get-test-runs-dir [] (or (:test-runs-dir @app-settings) "./test-runs"))
(defn get-test-db-dir [] (-> (:test-db-dir @app-settings) (or "./test-db")))
(defn get-test-files-dir [] (-> (:test-files-dir @app-settings) (or "./test-files")))
(defn get-target [] (:target @app-settings))
(defn get-app-url [] (:app-url @app-settings))
(defn get-tenant [] (:tenant @app-settings))
(defn get-default-fixtures [] (:default-fixtures @app-settings))

(defn get-app-state [] @app-state)

(defn update-app-settings! [f & args]
  (swap! app-settings f args))

(defn update-app-state! [f & args]
  (swap! app-state f args))

(defn update-settings
  "Merge the given options into the application settings."
  [options]
  (swap! app-settings (fn [m] (merge m options)))
  @app-settings)

(defn update-app-state
  "Merge the given map into the application state."
  [options]
  (swap! app-state (fn [m] (merge m options)))
  @app-state)

(defn load-settings-file
  "Read and return the given configuration file."
  [filename]
  (with-open [r (io/reader filename)]
    (read (PushbackReader. r))))

(defn update-settings-file
  "Update any settings in the given file with values from the current app settings."
  [filename]
  (let [settings @app-settings
        settings-in-file (load-settings-file filename)
        ;; At the moment only saving the app-url as then we don't have
        ;; to worry about all the string/keyword issues we have when data is coming from
        ;; a webapi call.
        ;new-settings (merge settings-in-file settings)
        new-settings (assoc settings-in-file :app-url (or (:app-url settings)
                                                          (:app-url settings-in-file)))]
    (spit filename (with-out-str (clojure.pprint/pprint new-settings)))))

(defn load-settings
  "Read the settings file the given file and merge into the application settings."
  [filename]
  (update-settings (load-settings-file filename)))

(defn load-state
  "Read the settings file and merge into the application state."
  [filename]
  (update-app-state (load-settings-file filename)))

(defn- fmt-name [name] (str "\"" name "\""))

(defn extract-setup-resource
  "Copy a setup related resource to the file system.

  Looks in the resources/setup folder for the resource,
  either on the file system (as in a dev environment)
  or in the appliction JAR file.

  Copies to the given destination folder."
  [name dest-dir]
  (let [dest-name (str dest-dir "/" name)]
    (if-not (.exists (io/file dest-name))
      (when-let [f (io/resource (str "setup/" name))]
        (.mkdir (File. dest-dir))
        (debug "Extracting resource:" (fmt-name f) " to " (fmt-name dest-name))
        (with-open [in (io/input-stream f)
                    out (io/output-stream dest-name)]
          (io/copy in out)))
      ;; else
      (debug "Extract resource:" (fmt-name name) "already exists in" (fmt-name dest-dir)))))

(defn extract-data-resource
  "Copy a data resource to the ./data folder on the file system.

  Looks in the resources/data folder for the resource,
  either on the file system (as in a dev environment)
  or in the appliction JAR file."
  [name]
  (let [dest-dir "data"
        dest-name (str dest-dir "/" name)]
    (if-not (.exists (io/file dest-name))
      (when-let [f (io/resource (str "data/" name))]
        (.mkdir (File. dest-dir))
        (debug "Extracting resource:" (fmt-name f) " to " (fmt-name dest-name))
        (with-open [in (io/input-stream f)
                    out (io/output-stream dest-name)]
          (io/copy in out)))
      ;; else
      (debug "Extract resource:" (fmt-name name) "already exists in" (fmt-name dest-dir)))))

(defn running-in-teamcity? [& [options]]
  (or (System/getenv "TEAMCITY_VERSION") (:teamcity options)))

(defn get-version [dep]
  (let [path (str "META-INF/maven/" (or (namespace dep) (name dep))
                  "/" (name dep) "/pom.properties")
        props (io/resource path)]
    (when props
      (with-open [stream (io/input-stream props)]
        (let [props (doto (Properties.) (.load stream))]
          (.getProperty props "version"))))))


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; setting and getting test data

;; get
;; (get-test-data :bia-users)

;; set
;; (set-test-data :bia-users [...])
;; (set-test-data :bia-users ensure-test-accounts ...)

;; set only if not already set, use a function to return the value
;; (setonce-test-data :bia-users ensure-test-accounts ...)

;; todo - look for at the server if we have a server defined

;; nicked from clojure.test
(defn- get-possibly-unbound-var
  [v]
  (try (var-get v)
       (catch IllegalStateException e
         nil)))
;; nicked from clojure.test
(defn- function?
  [x]
  (if (symbol? x)
    (when-let [v (resolve x)]
      (when-let [value (get-possibly-unbound-var v)]
        (and (fn? value)
             (not (:macro (meta v))))))
    (fn? x)))

(defn- data-file-name [k]
  (str (get-test-files-dir) "/" (name k) ".edn"))

(defn get-test-data [k & [quiet]]
  (let [v (try (read-string (slurp (data-file-name k)))
                   (catch FileNotFoundException _ nil))]
    (when (and (not quiet) (empty? v))
      (throw (Exception. (format "No test data for key %s" k))))
    v))

(defn get-random-test-data [k]
  (rand-nth (get-test-data k)))

(defmulti set-test-data (fn [_ v & _] (function? v)))

(defmethod set-test-data true [k f & args]
  (set-test-data k (pr-str (apply f args)))
  nil)

(defmethod set-test-data false [k v]
  ;(println "setting test data" k (type v) (pr-str v))
  (when (empty? v)
    (throw (Exception. (format "No test data for key %s" k))))
  (let [fname (data-file-name k)]
    (io/make-parents fname)
    (spit fname v))
  nil)

(defn setonce-test-data [k f & args]
  (try
    (get-test-data k)
    (catch Exception _
      (apply set-test-data k f args)))
  nil)

(defn init-logging [& [options]]
  ;; logging levels can be any of:
  ;;  :trace :debug :info :warn :error :fatal :report
  ;; our default logging is info to console and debug to file
  ;; and the verbose switch reduces each to debug and trace resp.

  (let [n (or (:session-id options) (:from-test-index options) 0) ;; attempt to make something uniquish...
        fname (format "./logs/readitest-%s-%x.log" n (rand-int 100000))]
    (io/delete-file fname true)
    (timbre/merge-config! {:appenders {:spit (appenders/spit-appender {:fname fname})}}))
  (timbre/merge-config! {:appenders {:spit {:min-level (if (:verbose options) :trace :debug)}}})

  (timbre/merge-config! {:appenders {:println {:min-level (if (:verbose options) :debug :info)}}}))



(comment

  (get-test-data :people)
  (read-string (get-test-data :bcp-manager-creds))
  (set-test-data :people1 {:name "bob"})
  (set-test-data :people1 identity {:name "bobby"})
  (setonce-test-data :people2 identity {:name "joely"})
  (setonce-test-data :people2 identity {:name "lily"})

  )