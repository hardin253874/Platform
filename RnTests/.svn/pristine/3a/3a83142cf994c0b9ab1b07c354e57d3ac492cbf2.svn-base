(ns rn.lib.wd
  )


(comment
  (->> (get-entities-of-type "report" "name" {:filter "Name='User Accounts'"})
       first :_id :_id)

  (let [report-name "User Accounts"
        id (->> (get-entities-of-type "report" "name" {:filter (format "Name='%s'" report-name)})
                first :_id :_id)
        report (run-report id {:metadata "full" :startIndex 0 :pageSize 0})
        cols (->> report :result :meta :anlcols
                  (map #(assoc (second %) :cid (first %))))
        username-col (->> cols
                          (filter #(= "Username" (:title %)))
                          first :cid name)
        results (run-report id {:metadata "full" :startIndex 0 :pageSize 100
                                :conds    [{:expid username-col
                                            :type  "String"
                                            :oper  "StartsWith"
                                            :value "BIA"}]})]
    (->> results :result :gdata count))

  (def results (run-report 4145 {:metadata "full" :startIndex 0 :pageSize 2000
                                 :conds    [{:expid "8016"
                                             :type  "String"
                                             :oper  "StartsWith"
                                             :value "BIA"}]}))

  (->> results clojure.pprint/pprint)

  ;; cols
  (->> results :result :meta :rcols vals (sort-by :ord) clojure.pprint/pprint)
  (->> results :result :meta :anlcols vals (sort-by :ord) clojure.pprint/pprint)
  (->> results :result :gdata count)


  ;;{"conds":[{"expid":"7661","type":"String","oper":"Contains","value":"abegum"}],"isreset":false}

  )
