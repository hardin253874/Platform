(ns rt.driverfn-chooser
  (:require-macros [reagent.ratom :refer [reaction]])
  (:require [rt.util :refer [filter-records url-encode id->str]]
            [reagent.core :as r]
            [re-frame.core :refer [subscribe dispatch dispatch-sync register-sub register-handler]]
            [markdown.core :as md]
            [clojure.string :as string]
            [rt.model :as model]))

(defn- driverfn-table-row [{:keys [id tags]} selected? onClick]
  (let [id (id->str id)]
    [:tr (merge {:key id} (if selected? {:class "selected"} {}))
     #_[:td ""]
     [:td {:onClick onClick} id]
     [:td (apply str (interpose "," tags))]]))

(defn driverfn-table [entities selected-index]
  [:table.driverfns
   #_[:thead [:tr
            [:td {:width "3rem"} ""]
            [:th "Name"]
            [:th "Tags"]]]
   (apply vector :tbody
          (map #(vector driverfn-table-row %1 (= @selected-index %2)
                        (partial reset! selected-index %2)) @entities (range)))])

(defn- entity-link [id label]
  [:a {:href (str "#/edit/" (url-encode id)) :target "_blank"} label])

(defn copy-to-clipboard [data & [data-type]]
  (try
    (->> (js/ClipboardEvent. "copy" #js {:dataType (or data-type "text/plain") :data data})
         (.dispatchEvent js/document))
    (catch js/Error e
      (.warn js/console "Exception copying to clipboard:" e))))

(defn copy-button []
  [:span.copy-button {:dangerouslySetInnerHTML {:__html "&#x2600;"}}])

(defn copy-button-component [data]
  (let [on-click #(copy-to-clipboard data)]
    (r/create-class
      {:reagent-render
       (fn [_] [:span {:title "click to copy (not yet working)"} [copy-button]])
       :component-did-mount
       (fn [this] (.addEventListener (r/dom-node this) "click" on-click))
       :component-will-unmount
       (fn [this] (.removeEventListener (r/dom-node this) "click" on-click))})))

(defn driverfn-preview [entity]
  (let [{:keys [id used-by] :as e} @entity
        id (id->str id)
        doc (model/driverfn-doc e)
        call-syntax (model/driverfn-call-syntax e)]
    [:div.driverfn-preview
     [:h1 id]
     [:div [:input {:readOnly "readOnly" :value call-syntax}] [copy-button-component call-syntax]]
     [:div [:span {:dangerouslySetInnerHTML {:__html (md/mdToHtml (if doc (string/trim doc) ""))}}]]
     (when (not-empty used-by)
       [:div
        [:h2 "Used by:"]
        (apply vector :ul (map #(vector :li (entity-link % %)) used-by))])
     #_[:pre.wrap (pr-str (dissoc @entity :pr-str))]]))

(defn driverfn-chooser []
  (let [entity-list (subscribe [:entity-list])
        driver-list (reaction (:driverfn @entity-list))
        filter (r/atom "")
        driver-list (reaction (take 100 (filter-records @filter @driver-list)))
        selected-index (r/atom 0)
        selected (reaction (get (vec @driver-list) @selected-index))]
    (fn []
      [:div.driverfn-chooser
       [:div.driverfn-header
        #_[:span "Selecting from " (count @driver-list) " drivers"]
        [:input {:value @filter :onChange #(reset! filter (.. % -target -value)) :placeholder "filter"}]]
       [:div.driverfn-list
        [driverfn-table driver-list selected-index]]
       [driverfn-preview selected]])))
