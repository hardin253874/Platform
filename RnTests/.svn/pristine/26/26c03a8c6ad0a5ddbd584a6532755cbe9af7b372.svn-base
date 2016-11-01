(ns rn.services.entity
  (:require [rt.lib.wd-rn :as rn]))

;; todo move to different file, probably do when move rt.lib.wd-rn
(defn get-entity-ids-for-name
  ([entity-name] (get-entity-ids-for-name entity-name "resource"))
  ([entity-name type-id & [{:keys [app-name folder-name]}]]
   (let [filter (format "Name = '%s'" entity-name)
         filter (if app-name
                  (str filter (format " and [Resource in application].Name = '%s'" app-name))
                  filter)
         filter (if folder-name
                  (str filter (format " and [Resource in folder].Name = '%s'" folder-name))
                  filter)]
     (->> (rn/get-entities-of-type type-id "name" {:filter filter})
          (map #(get-in % [:_id :_id]))))))

(def get-entity-id-for-name (comp first get-entity-ids-for-name))

(defn delete-entity-for-name
  "Delete the entity for the given name and options type and filter opts.
  Only if one and one only entity is found. Returns the id of the deleted entity
  or nil otherwise."
  [entity-name type-id & [filter-opts]]
  (let [ids (get-entity-ids-for-name entity-name type-id filter-opts)]
    (if (= 1 (count ids))
      (do (rt.lib.wd-rn/delete-entity (first ids))
          (first ids))
      nil)))

(comment

  (rn/get-entities-of-type "resource" "name,isOfType.name" {:filter "Name = 'Navigation Access'"})

  (get-entity-id-for-name "All Resources" "report")
  (get-entity-id-for-name "Employee" "definition")
  (get-entity-id-for-name "SummariseCurrency" "report" {:app-name "Foster University"})

  ;; Resource in folder
  ;; Resource in application

  )
