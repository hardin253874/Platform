(ns rt.source
  (:require-macros [reagent.ratom :refer [reaction]])
  (:require [reagent.core :as r]
            [re-frame.core :refer [subscribe dispatch register-sub register-handler dispatch-sync]]
            [rt.codemirror :refer [cm-input]]
            [rt.util :refer [filter-records set-location id->str]]
            [rt.common :refer [source-link driverfn-list-link]]
            [rt.store :as store]
            [clojure.string :as string]))

;; -------------------------
;; Queries

(register-sub
  :source-file
  (fn [db _]
    (reaction (get-in @db [:source-file]))))

(register-sub
  :source-file-list
  (fn [db _]
    (reaction (get-in @db [:source-file-list]))))

;; -------------------------
;; Event handlers

(register-handler
  :request-source-list
  (fn [db [_]]
    (store/get-source-list #(dispatch [:update-source-list %]))
    (assoc db :source-file-list {:items [] :state :loading})))

(register-handler
  :update-source-list
  (fn [db [_ items]]
    (-> db
        (assoc-in [:source-file-list :items] items)
        (assoc-in [:source-file-list :loading] false))))

(register-handler
  :request-source-file
  (fn [db [_ name]]
    (store/get-source-file name #(dispatch [:update-source-file %]))
    (assoc db :source-file {:name name :content "" :state :loading})))

(register-handler
  :update-source-file
  (fn [db [_ code]]
    ;;todo check name matches
    (-> db
        (assoc-in [:source-file :content] code)
        (assoc-in [:source-file :loading] false))))

(register-handler
  :update-source-file-selection
  (fn [db [_ code]]
    (assoc-in db [:source-file :selection] code)))

(register-handler
  :evaluate
  (fn [db [_ selection]]
    (let [code (or selection (get-in db [:source-file :content]))]
      (store/eval-expr code #(dispatch [:update-eval-result %2 %3])))
    (-> db
        (assoc-in [:source-file :eval-result] nil)
        (assoc-in [:source-file :eval-output] (str "running "
                                                   (or selection "whole file") "...")))))

(register-handler
  :update-eval-result
  (fn [db [_ result output]]
    (println "eval results" result output)
    (-> db
        (assoc-in [:source-file :eval-result] result)
        (assoc-in [:source-file :eval-output] output))))

;; -------------------------
;; Views

(defn source-edit-page [route]
  (let [source-file (subscribe [:source-file])
        content (reaction (:content @source-file))
        selection (reaction (:selection @source-file))
        ;; note - need to use dispatch-sync for "controlled" components like
        ;; textarea. but don't use it within other dispatch handlers - google it
        update-content #(dispatch-sync [:update-source-file %])
        update-selection #(dispatch [:update-source-file-selection %])
        eval-selection #(dispatch [:evaluate @selection])
        save-file #(store/put-source-file (:name @source-file) (:content @source-file))
        show-help #(dispatch [:show-help :source-edit])
        initial-line (reaction (js/parseInt (str (get-in @route [:params :query-params :line]))))]
    (dispatch [:request-source-file (get-in @route [:params :ns])])
    (fn []
      [:div.source-editor
       [:div.toolbar
        ;; next line for when testing with a textarea
        #_[:textarea {:value @content :rows 4 :onChange #(update-content (.. % -target -value))}]
        [:div (take 100 @selection) (when (> (count @selection) 100) "...")]
        [:button {:onClick show-help} "?"]
        [:button {:onClick save-file} "Save"]
        [:button {:onClick eval-selection} "Eval"]]
       [:div.editor-container
        [:div.editor
         [cm-input {:value              content
                    :onChange           update-content
                    :onSelectionChanged update-selection
                    :onCtrlEnter        eval-selection
                    :initialLine        @initial-line}]]
        [:div.eval-results
         [:div.result [:pre.wrap (or (:eval-result @source-file) "no result")]]
         [:div.output [:pre.wrap (or (:eval-output @source-file) "no output")]]]]])))

;; todo in the listing
;; filter by file name
;; filter by driver fn ... will need backend support

(defn set-source-filter [q]
  (set-location (str "/source?q=" q))
  (dispatch-sync [:update-in [:route :params :query-params :q] q]))

(defn- source-file-table-row [{:keys [id doc tags publics]}]
  (let [id (id->str id)]
    [:tr {:key id}
     [:td (source-link id id)]
     [:td doc]
     [:td (when (not-empty publics) (driverfn-list-link id (count publics)))]
     [:td tags]]))

(defn source-file-table [entities]
  [:table.entity
   [:thead [:tr
            [:th "Namespace"]
            [:th "Doc"]
            [:th "#Functions"]
            [:th "Tags"]]]
   (apply vector :tbody
          (map #(vector source-file-table-row %) entities))])

(defn source-list-page []
  (let [source-file-list (subscribe [:source-file-list])]
    (dispatch [:request-source-list])
    (fn [route]
      (if-let [items (:items @source-file-list)]
        (let [q (get-in @route [:params :query-params :q])
              items (filter-records q items)]
          [:div.source-list
           [:div.toolbar
            [:input.filter {:value q :placeholder "filter by file or function" :onChange #(set-source-filter (.. % -target -value))}]
            #_[:select [:option {:value "rt.po.app"} "rt.po.app"]]]
           [:div.content
            [source-file-table items]]])
        ;; else
        [:div "loading"]))))

