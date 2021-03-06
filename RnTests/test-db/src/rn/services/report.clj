(ns rn.services.report
  (:require [rt.lib.wd-rn :as rn]
            [rn.services.entity :refer [get-entity-id-for-name]]))

(defn get-report-id [report-name & [filter-opts]]
  (get-entity-id-for-name report-name "report" filter-opts))

(defn run-report
  "Conds are like {:title \"Name\" :oper \"StartsWith\" :value \"Joe\" }."
  ([report-name] (run-report report-name {}))
  ([report-name {:keys [conds start-index page-size entity-type-id]
                 :or   {conds [] start-index 0 page-size 1000}}]
   {:pre [report-name]}
   (let [report-id (get-report-id report-name)
         report (rn/run-report report-id
                               {:metadata     "full" :startIndex 0 :pageSize 0
                                :entityTypeId entity-type-id})
         cols (->> report :result :meta :rcols
                   ; need to "name" the cid as our interop has turned the numeric id into a keyword
                   (map #(assoc (second %) :cid (name (first %)))))
         anlcols (->> report :result :meta :anlcols
                      ; need to "name" the cid as our interop has turned the numeric id into a keyword
                      (map #(assoc (second %) :cid (name (first %)))))
         make-cond (fn [cols {:keys [title oper value]}]
                     (if-let [col (first (filter #(= title (:title %)) cols))]
                       {:expid (str (:cid col))
                        :type  (:type col)
                        :oper  (or oper (:doper col))
                        :value (str value)}
                       (throw (Exception. (format "Failed to find column %s" title)))))
         conds (mapv (partial make-cond anlcols) conds)
         results (rn/run-report report-id {:metadata "full" :startIndex start-index
                                           :pageSize page-size :entityTypeId entity-type-id
                                           :conds    conds})
         get-col-title (fn [ord] (first (filter #(= ord (:ord %)) cols)))
         make-rel-objs (fn [vals] (mapv #(hash-map :id (name (first %)) :name (second %)) vals))
         make-row (fn [{:keys [eid values]}]
                    ;; note - get col title works on using the number of elements already in the obj
                    ;; as the col ord.... take care with that!
                    (-> (reduce #(let [{:keys [title]} (get-col-title (count %1))]
                                  (assoc %1 title (cond
                                                    (:val %2) (:val %2)
                                                    (:vals %2) (make-rel-objs (:vals %2))
                                                    :default nil)))
                                {} values)
                        (assoc :id (str eid))))
         data (->> results :result :gdata
                   (map make-row))]
     ;(clojure.pprint/pprint (->> report :result))
     data)))

(defn get-entities-from-report-run
     [report-name & [filter-opts report-opts]]
  (let [report-id (get-report-id report-name filter-opts)]
    (map :eid (:gdata (:result (rn/run-report report-id report-opts))))))

(comment

  (rt.lib.util/timeit
    "" (->> (run-report "User Accounts" {:conds [{:title "Username" :value "a"}]})))

  (rt.lib.util/timeit
    "" (->> (run-report "User Accounts") (count)))

  (rt.lib.util/timeit
    "" (let [type-name "Business Unit"
             options {:conds [{:title "Type"
                               :oper  "Equal"
                               :value type-name}]}]
         (->> (run-report "Resource" options)
              count)))

  (rt.lib.util/timeit
    "" (let [type-name "Business Unit"
             type-id (get-entity-id-for-name type-name "definition")
             options {:entity-type-id type-id}]
         (->> (run-report "Resource" options)
              count)))

  (rt.lib.util/timeit
    "" (rt.setup/set-test-data :accounts run-report "User Accounts"))

  (rt.lib.util/timeit
    "" (rt.setup/get-test-data :accounts))

  (rt.lib.util/timeit
    "" (rt.setup/setonce-test-data :accounts run-report "User Accounts"))


  (rt.lib.util/timeit
    "" (->> (run-report "Employee") (map #(get % "Employee"))))

  (def results (run-report "User Accounts" {:conds [{:title "Username" :value "abeg"}]}))

  (->> (rn/get-entities-of-type "report" "name" {:filter "Name='User Accounts'"})
       first :_id :_id)

  (let [report-name "User Accounts"
        id (->> (rn/get-entities-of-type "report" "name" {:filter (format "Name='%s'" report-name)})
                first :_id :_id)
        report (rn/run-report id {:metadata "full" :startIndex 0 :pageSize 0})
        cols (->> report :result :meta :anlcols
                  (map #(assoc (second %) :cid (first %))))
        username-col (->> cols
                          (filter #(= "Username" (:title %)))
                          first :cid name)
        results (rn/run-report id {:metadata "full" :startIndex 0 :pageSize 100
                                   :conds    [{:expid username-col
                                               :type  "String"
                                               :oper  "StartsWith"
                                               :value "BIA"}]})]
    (->> results :result :gdata count))

  (->> results clojure.pprint/pprint)

  ;; cols
  (->> results :result :meta :rcols vals (sort-by :ord) clojure.pprint/pprint)
  (->> results :result :meta :anlcols vals (sort-by :ord) clojure.pprint/pprint)
  (->> results :result :gdata count)


  ;;{"conds":[{"expid":"7661","type":"String","oper":"Contains","value":"abegum"}],"isreset":false}

  )
