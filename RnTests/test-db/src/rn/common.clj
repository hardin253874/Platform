(ns rn.common
  (require rt.scripts.common
           [rn.services.entity :refer [get-entity-id-for-name]]
           [rn.services.report :refer [run-report]]
           clj-http.client
           [rt.lib.wd-rn :refer [get-entities-of-type]]
           [rt.lib.util :refer [timeit]]
           [rt.setup :refer [get-settings]]
           [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

;; place for common things

(defn log [s]
  (info s))

;; some redirects to the rt.scripts based files

(def start-app-and-login rt.scripts.common/start-app-and-login)
(def start-app-and-login2 rt.scripts.common/start-app-and-login2)
(def clear-all-alerts rt.scripts.common/clear-all-alerts)
(def wait-until rt.po.common/wait-until)
(def wait-until-settled rt.scripts.common/wait-until-settled)
(def sleep rt.scripts.common/sleep)
(def take-screenshot rt.scripts.common/take-screenshot)
(def capture-browser-logs rt.scripts.common/capture-browser-logs)
(def clear-browser-logs rt.scripts.common/clear-browser-logs)
(def safe-wait-for-angular rt.scripts.common/safe-wait-for-angular)

(defn log-free-java-mem []
  (let [x (float (/ (.freeMemory (java.lang.Runtime/getRuntime)) 1024))]
    (debug (str "Free java memory = " x "kb"))))

;; this "-old" version of the function is faster when it works,
;; but suffers from being useless if there are more than 500 records - you get none.
(defn get-record-names-for-type-old
  ([type-name] (get-record-names-for-type-old "definition" type-name {}))
  ([type-name opts] (get-record-names-for-type-old "definition" type-name opts))
  ([base-name type-name {:keys [app-name]}]
   (let [filter (format "Name = '%s'" type-name)
         filter (if app-name
                  (str filter (format " and [Resource in application].Name = '%s'" app-name))
                  filter)]
     (->> (get-entities-of-type base-name "name,instancesOfType.name"
                                {:filter filter})
          first
          :instancesOfType
          (map :name)
          (remove empty?)))))

(defn get-record-names-for-type
  ([type-name] (get-record-names-for-type "definition" type-name {}))
  ([type-name opts] (get-record-names-for-type "definition" type-name opts))
  ([base-name type-name {:keys [app-name]}]
   (let [filter (format "Name = '%s'" type-name)
         filter (if app-name
                  (str filter (format " and [Resource in application].Name = '%s'" app-name))
                  filter)
         type-id (->> (get-entities-of-type base-name "name" {:filter filter})
                         first :_id :_id)]
     (->> (get-entities-of-type type-id "name" {:filter "true"})
             (map :name)
             (remove empty?)))))

(defn get-record-names-for-type-via-report
  [type-name]
  ;; the slow way apparantly
  #_(->> (run-report "Resource" {:conds [{:title "Type" :oper "Equal" :value type-name}]})
         (map #(get % "Name")))
  ;; a faster way
  (let [type-id (get-entity-id-for-name type-name "definition")]
    (->> (run-report "Resource" {:entity-type-id type-id})
         (map #(get % "Name")))))

(def get-choice-values (partial get-record-names-for-type "enumType"))

(comment

  (let [type-name "Business Unit"]
    (timeit "" (->> (get-record-names-for-type type-name) count println))
    (timeit "" (->> (get-record-names-for-type-old type-name {:app-name "ReadiBCM"}) count println))
    (timeit "" (->> (get-record-names-for-type-via-report type-name) count println)))

  (get-record-names-for-type-via-report "Employee")
  (get-record-names-for-type-via-report "Mitigating Control")
  (get-record-names-for-type "Employee")

  (get-record-names-for-type-via-report "Building")
  (get-record-names-for-type "Building")

  (get-choice-values "Level of Risk" {:app-name "ReadiBCM"})
  (get-choice-values "Level of Risk")

  (->> (rt.lib.wd-rn/get-entities-of-type "enumType" "name,instancesOfType.name"
                                          {:filter "Name='Level of Risk'"})
       first keys)

  )


