(ns rt.report
  (:require-macros [reagent.ratom :refer [reaction]]
                   [rt.lib.cljs-macros :refer [handler-fn]])
  (:require [rt.codemirror :refer [cm-input]]
            [rt.common :refer [entity-link run-link source-link]]
            [rt.store :as store]
            [rt.model :as model]
            [rt.util :refer [url-encode get-datetime-str id->str filter-records ensure-keyword]]
            [reagent.core :refer [atom]]
            [re-frame.core :refer [subscribe dispatch dispatch-sync register-sub register-handler]]
            [clojure.string :as string]
            [cljs.reader :refer [read-string]]
            [rt.util :as util]))

(defn- id->entity [id]
  (if (map? id) id {:id id}))

;; -------------------------
;; Queries

(register-sub
  :entity-list
  (fn [db _]
    (reaction (get-in @db [:entity-list]))))

(register-sub
  :selected-entity-list
  (fn [db _]
    (let [entity-type (reaction (get-in @db [:entity-list :selected-entity-type]))]
      (reaction (get-in @db [:entity-list @entity-type])))))

;; -------------------------
;; Event handlers

(register-handler
  :request-entity-list
  (fn [db [_ entity-type]]
    (when-not (get-in db [:entity-list entity-type])
      (store/request-entity-list entity-type #(dispatch [:update-entity-list entity-type %])))
    (assoc-in db [:entity-list :selected-entity-type] entity-type)))

(register-handler
  :refresh-entity-list
  (fn [db [_ entity-type]]
    (store/request-entity-list entity-type #(dispatch [:update-entity-list entity-type %]))
    db))

(register-handler
  :update-entity-list
  (fn [db [_ entity-type entity-list]]
    (println "update-entity-list" entity-type)
    (assoc-in db [:entity-list entity-type]
              (->> entity-list
                   (map (partial util/ensure-keyword-values [:id :type]))
                   ;; cache the full text rep for filtering on
                   ;; exluding the used-by member (for driverfns)
                   (map #(assoc % :pr-str (pr-str (dissoc % :used-by))))))))

(register-handler
  :reset-entity-lists
  (fn [db _]
    (assoc db :entity-list nil)))

(register-handler
  :reset-server
  (fn [db _]
    (store/reset #(do
                   (dispatch [:reset-entity-lists])
                   (dispatch [:request-entity-list (get-in db [:entity-list :selected-entity-type])])))
    db))

;; ---------------------------
;; Views

(defn- entity-table-row [{:keys [type id name doc tags explicit? line]}]
  (let [id (id->str id)]
    [:tr {:key id}
     [:td (when (#{:test :testsuite} type) (run-link "run" id))]
     [:td (if (#{:driverfn} type) (source-link id id line) (entity-link id id))]
     [:td {:title doc}
      [:span (str name " " (when explicit? " * "))]
      (when doc [:span {:dangerouslySetInnerHTML {:__html "&raquo;"}}])]
     [:td (apply str (interpose "," tags))]]))

(defn entity-table [entities]
  (println "render entity-table:" (some-> entities first keys))
  [:table.entity
   [:thead [:tr
            [:th (:style {:width "3rem"}) ""]
            [:th "Id"]
            [:th "Name"]
            [:th "Tags"]]]
   (apply vector :tbody
          (map #(vector entity-table-row %) entities))])

(defn set-report-filter [t q]
  (println "setting " t q)
  (util/set-location (str "/report/" t "?q=" q))
  (dispatch-sync [:update-in [:route :params :query-params :q] q]))

(defn entity-type-button []
  (let [entity-list (subscribe [:entity-list])
        selected-type (reaction (:selected-entity-type @entity-list))
        route (subscribe [:route])
        filter-text (reaction (get-in @route [:params :query-params :q]))]
    (fn [type-id & [label]]
      [:a.tab-label (merge {:href (str "#/report/" (id->str type-id) "?q=" @filter-text)}
                           (if (= @selected-type type-id) {:class "selected"}))
       (or label (id->str type-id))])))

(defn new-entity [entity-type-str]
  [:a.button.right {:href (str "#/edit?type=" entity-type-str)} (str "new " entity-type-str)])

(defn report-page [route]
  (let [entity-list (subscribe [:entity-list])
        filter-text (reaction (get-in @route [:params :query-params :q]))
        entity-type (reaction (or (some-> (get-in @route [:params :id]) ensure-keyword)
                                  (:selected-entity-type @entity-list)
                                  :testsuite))
        entities (reaction (filter-records @filter-text (get @entity-list @entity-type)))
        entity-count (reaction (count @entities))]
    (fn []
      (let [filter-text @filter-text
            entities @entities
            entity-type-str (id->str @entity-type)
            selected-type (:selected-entity-type @entity-list)]
        (println "render report-page" (pr-str @entity-type) "filter=" filter-text)
;        (dispatch [:update-in [:entity-list :filter-text] filter-text])
        (when (not= @entity-type selected-type)
          (dispatch [:request-entity-list @entity-type]))
        (dispatch [:app-status (str "Showing up to 500 of " @entity-count " " entity-type-str "s")])
        (if (and (zero? @entity-count) (not-empty filter-text))
          (dispatch [:app-alert "Warning - filter applied!"])
          (dispatch [:app-alert nil]))
        [:div.report-page
         [:div.report-header
          [:div.report-toolbar
           [entity-type-button :testsuite "suites"]
           [entity-type-button :test "tests"]
           [entity-type-button :testscript "scripts"]
           [entity-type-button :testfixture "fixtures"]
           [entity-type-button :driverfn "drivers"]
           [:input {:value       filter-text
                    :onChange    (handler-fn
                                   (set-report-filter entity-type-str (.. event -target -value)))
                    :placeholder "filter"}]
           (if (not= "driverfn" entity-type-str)
             [new-entity entity-type-str])]]
         [:div.report-content
          [entity-table (take 500 entities)]]]))))
