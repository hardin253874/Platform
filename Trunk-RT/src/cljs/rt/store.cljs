(ns rt.store
  (:require-macros [cljs.core.async.macros :refer [go alt!]])
  (:require [cljs-http.client :as http]
            [cljs.core.async :refer [put! <! >! chan timeout close!]]
            [cljs.reader :as reader]
            [rt.util :as util]
            [clojure.string :as string])
  (:import goog.net.XhrIo))

;; this shouldn't be "store", more like "rt-server"

(defn get-app-drivers [cb]
  (go (let [{drivers :body} (<! (http/get (str "/api/entities?type=driverfn")))
            drivers (->> drivers
                         (map (fn [{:keys [ns name] :as d}]
                                (assoc d :ns-name (str ns "/" name))))
                         (sort-by :ns-name)
                         (reduce #(assoc %1 (:ns-name %2) %2) {}))]
        (println "received driver list:" (count drivers))
        (cb drivers))))

(defn get-app-settings [cb]
  (go (let [{settings :body} (<! (http/get "/api/settings"))]
        (cb settings))))

(defn put-app-settings [settings]
  (http/post "/api/settings" {:json-params {:settings settings}}))

(defn reset [& [cb]]
  (go (<! (http/post "/api/reset"))
      (if cb (cb))))

(defn get-source-list [cb]
  (go (let [{list :body} (<! (http/get "/api/sourcefiles"))]
        (cb list))))

(defn get-source-file [name cb]
  (go (let [{source :body} (<! (http/get (str "/api/sourcefile?name=" name)))]
        (cb source))))

(defn put-source-file [name source]
  {:pre [name source]}
  (http/post "/api/sourcefile" {:json-params {:name name :src source}}))

(defn get-testrun [cb]
  (go (let [{testrun :body} (<! (http/get "/api/testrun"))]
        (println "get-testrun=>" (:id testrun))
        (cb testrun))))

(defn get-testrun-report [test-id cb]
  (go (let [{testrun :body} (<! (http/get (str "/api/testrun-report?test-id=" test-id)))]
        ;(println "get-testrun-report=>" testrun)
        (cb testrun))))

(defn get-step-summary [cb]
  (go (let [{data :body} (<! (http/get (str "/api/step-summary")))]
        (cb data))))

(defn get-session-summary [cb]
  (go (let [{data :body} (<! (http/get (str "/api/session-summary")))]
        (cb data))))

(defn get-source-code [ns name cb]
  (go (let [{code :body} (<! (http/get (str "/api/source?ns=" ns "&name=" name)))]
        ;(println "get-source-code=>" code)
        (cb code))))

(defn initiate-test-run
  ([suite-id] (initiate-test-run suite-id nil))
  ([suite-id include-only-test-id]
   (println "init test run" (pr-str suite-id) (pr-str include-only-test-id))
   (let [suite-id (util/id->str suite-id)
         include-only-test-id (util/id->str include-only-test-id)]
     (go (let [response (<! (http/put "/api/testrun"
                                      {:json-params {:suite suite-id :test include-only-test-id}}))]
           #_(println "create-test-run returned:" response))))))

(defn request-entity-list [entity-type cb]
  (go (let [{entities :body} (<! (http/get (str "/api/entities?type=" (name (or entity-type :test)))))]
        (println "GET entities returned:" (count entities))
        (let [entities (->> entities (sort-by :id) (sort-by :type))]
          (cb entities)))))

(defn filter-entity [filter-text entity]
  (try
    (re-find (re-pattern (str "(?i)" filter-text)) (pr-str entity))
    (catch js/Error e
      ;; try block to ignore temp errors if filtering by partial re expressions
      )))

(defn filter-entity-on-keys [keys filter-text entity]
  (try
    (re-find (re-pattern (str "(?i)" filter-text)) (pr-str (select-keys entity keys)))
    (catch js/Error e
      ;; try block to ignore temp errors if filtering by partial re expressions
      )))

(defn request-entity [id cb]
  (when id
    (println "request-entity" id)
    (go (let [{entity :body} (<! (http/get (str "/api/entity?id=" (util/id->str id))))]
          ;(println "GET entity returned:" entity)
          (cb (util/ensure-keyword-values [:id :type] entity))))))

(defn request-entity-graph [id cb]
  (when id
    (go (let [{entity :body} (<! (http/get (str "/api/entity-graph?id=" (util/id->str id))))]
          ;(println "GET entity graph returned:" entity)
          (cb (util/ensure-keyword-values [:id :type] entity))))))

(defn get-suite-tests [id cb]
  (when id
    (println "get-suite-tests" id)
    (go (let [{results :body} (<! (http/get (str "/api/suite-tests?id=" (util/id->str id))))]
          ;(println "GET suite-tests" (pr-str results))
          (cb (map (partial util/ensure-keyword-values [:id :type]) results))))))

;; todo - consolidate the steps lists to type mapping between here and model ns

(def step-lists-by-type {:test        [:setup :steps :teardown]
                         :testscript  [:steps]
                         :testfixture [:setup :teardown]
                         :testsuite   [:setup :teardown]})

(def rel-ids-by-type {:test        [:fixtures]
                      :testscript  []
                      :testfixture []
                      :testsuite   [:tests :each-fixtures :once-fixtures]})

(defn clean-entity-steps [entity steps-key]
  (assoc entity steps-key (->> (steps-key entity)
                               ;; remove empty expressions
                               (remove #(and (:script %) (empty? (:script %))))
                               ;; remove the :__index key
                               (map #(if (:__index %) (dissoc % :__index) %))
                               ;; swap out any expressions that are a keyword
                               ;; with the keyword itself (as a string until I can fix the json issue)
                               (map #(if (= \: (first (:script %)))
                                      ;(keyword (subs (:script %) 1))
                                      (:script %)
                                      %))
                               vec)))

(defn- ensure-entity-id [entity-or-id]
  (util/id->str (if (map? entity-or-id) (:id entity-or-id) entity-or-id)))

(defn clean-rel-list [entity rel-key]
  (assoc entity rel-key (->> (rel-key entity) (map ensure-entity-id))))

(defn- clean-entity [entity]
  (let [entity-type (keyword (:type entity))

        ;; the json webapi doesn't handle my (mistaken) use of a keyword with
        ;; namespace and name and multiple segments... so convert to a string
        ;; Ditto test lists etc
        entity (assoc entity :id (util/id->str (:id entity)))
        entity (reduce clean-rel-list entity (get rel-ids-by-type entity-type))

        ;; clean up the various step lists, removing placeholders etc
        entity (reduce clean-entity-steps entity (get step-lists-by-type entity-type))]
    entity))

(defn save-entity
  ([entity] (save-entity entity nil))
  ([entity cb]
   (println "posting" entity)
   (let [entity (clean-entity entity)]

     (println "posting cleaned" (type (:id entity)) entity)

     ;;something been playing with as alternative to http/post
     #_(xhr-save-entity entity)

     (go (<! (http/post "/api/entity" {:json-params {:entity entity}}))
         (if cb (cb))))))

(defn post-driver [driver]
  (http/post "/api/driver-doc" {:json-params driver}))

(defn check-expr [expr cb]
  (go (if-not (string/blank? expr)
        (let [{result :body} (<! (http/post "/api/check-expr" {:json-params {:expr expr :ns "rt.scripts"}}))]
          ;(println "check result" (type result) result (string/blank? result))
          (cb expr (if (string/blank? result) nil result)))
        (cb expr nil))))

(defn eval-expr [expr cb]
  (go (if-not (string/blank? expr)
        (let [{[result out] :body} (<! (http/post "/api/eval-expr" {:json-params {:expr expr :ns "rt.scripts"}}))]
          ;(println "eval result" (pr-str result) (pr-str out))
          (cb expr result (if (string/blank? out) "" out)))
        (cb expr nil ""))))

(defn rerun-with-expr [expr save?]
  (when (not-empty expr)
    (go (<! (http/post "/api/reset-step" {}))
        (<! (http/post "/api/step" {:json-params {:expr expr :save? save?}})))))

(comment

  (defn get-logs [url]
    (let [c (chan)]
      (go (let [{logs :body :as response} (<! (http/get url))]
            (.log js/console "logs get response:" response (clj->js response))
            (.log js/console "logs after get:" (clj->js logs))
            (>! c (vec logs))))
      c))

  (defn load-outfile-list [cb]
    (go (let [response (<! (http/get "/api/outfiles"))]
          (println "outfiles:" response)
          (cb (:body response)))))

  ;; something I was playing around with... to come back to

  ;; xhr helpers

  (def ^:private meths
    {:get    "GET"
     :put    "PUT"
     :post   "POST"
     :delete "DELETE"})

  (defn edn-xhr [{:keys [method url data on-complete]}]
    (let [xhr (XhrIo.)]
      (events/listen xhr goog.net.EventType.COMPLETE
                     (fn [e]
                       (on-complete (reader/read-string (.getResponseText xhr)))))
      (. xhr
         (send url (meths method) (when data (pr-str data))
               #js {"Content-Type" "application/edn"}))))

  (defn xhr-save-entity [entity]
    (edn-xhr
      {:method :post
       :url    (str "updateEntity")
       :data   {:entity entity}
       :on-complete
               (fn [res]
                 (println "server response:" res))}))

  )