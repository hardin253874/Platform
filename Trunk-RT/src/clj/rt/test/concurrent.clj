(ns rt.test.concurrent
  (:require [rt.setup :refer [get-settings]]
            [rt.lib.util :refer [format-tc-message]]
            clj-http.client
            clojure.data.json
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(defn- get-server-url []
  (:server (get-settings)))

(defn host-name []
  (System/getenv "COMPUTERNAME"))

(defn request-test-from-server [& [n]]
  (infof "Getting test from server %s iter %s" (get-server-url) n)
  (when-let [server (get-server-url)]
    (try
      ;;todo add some kind of identifier for this session/runner
      (-> (clj-http.client/get (str server (str "/api/context?iter=" n)))
          :body
          clojure.data.json/read-json
          :tests)
      (catch Exception ex
        ;; ignore
        (error "exception in /api/context" ex)
        nil))))

(defn request-options-from-server
  "Get server options and clear out any options that we don't want
  leaked across from the server"
  [& [server]]
  (when-let [server (or server (get-server-url))]
    (try
      ;;todo add some kind of identifier for this session/runner
      (-> (clj-http.client/get (str server "/api/context?iter=0"))
          :body
          clojure.data.json/read-json
          (dissoc :shared-db-dirs :test-db-dir :data-dir :sessions))
      (catch Exception _
        ;; ignore
        nil))))

(defn post-step-result [step]
  (when-let [server (get-server-url)]
    (try
      (clj-http.client/post (str server "/api/step-result")
                            {:form-params  {:step step
                                            :host (host-name)
                                            :session-id (:session-id (get-settings))}
                             :content-type :json})
      (catch Exception _
        ;; ignore
        ))))

(defn post-test-event [event]
  (when-let [server (get-server-url)]
    (try
      (clj-http.client/post (str server "/api/test-event")
                            {:form-params  {:event event
                                            :host (host-name)
                                            :session-id (:session-id (get-settings))}
                             :content-type :json})
      (catch Exception _
        ;; ignore
        ))))

(defn- ensure-str-values [m]
  (reduce (fn [m [k v]] (assoc m k (cond (keyword? v) (str v)
                                         (map? v) (ensure-str-values v)
                                         :else v))) {}  m))

(defn post-tc-event [event]
  (if-let [server (get-server-url)]
    (try
      (clj-http.client/post (str server "/api/tc-event")
                            {:form-params  {:event      (ensure-str-values event)
                                            :host       (host-name)
                                            :session-id (:session-id (get-settings))}
                             :content-type :json})
      (catch Exception _
        ;; ignore
        ))
    ;; no server so print right now
    (println (format-tc-message (:type event) (:values event)))))
