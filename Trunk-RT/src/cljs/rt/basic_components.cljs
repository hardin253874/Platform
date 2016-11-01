(ns rt.basic-components
  (:require-macros [reagent.ratom :refer [reaction]]
                   [rt.lib.cljs-macros :refer [handler-fn]])
  (:require [rt.util :refer []]
            [reagent.core :as r]))

(defn simple-props-table
  "html table of name value pairs for the given list of key and value pairs"
  [pairs]
  [:table.zebra
   (apply vector :tbody
          (map #(vector :tr
                        [:td (str (first %) ":")]
                        [:td nil (second %)])
               (partition 2 pairs)))])

(defn- tab-title [selected-tab-label [label]]
  [(if (= @selected-tab-label label) :li.selected :li)
   {:key     label
    :onClick #(reset! selected-tab-label label)}
   label])

(defn- tab [_ comp & args]
  [:div
   (when comp (apply vector comp args))])

(def get-tab-label first)

;; todo - the tabs should be an atom
(defn tabs [& tabs]
  (let [selected-tab-label (r/atom (get-tab-label (first tabs)))
        expanded? (r/atom false)
        get-selected-tab (fn [tabs] (first (filter #(= @selected-tab-label (get-tab-label %)) tabs)))]
    (fn [& tabs]
      (when-not (get-selected-tab tabs)
        (reset! selected-tab-label (get-tab-label (first tabs))))

      (if-not @expanded?

        [:div.tabs
         [:div.tab-header
          [:span.right {:onClick (handler-fn (swap! expanded? not))} "Expand tabs v"]
          (apply vector :ul (map (partial tab-title selected-tab-label) tabs))]
         [:div.tab-content {:key @selected-tab-label}
          (apply tab (get-selected-tab tabs))]]

        [:div.tabs
         [:div.tab-header
          [:span.right {:onClick (handler-fn (swap! expanded? not))} "Collapse tabs ^"]]
         (apply vector :div (map #(vector :label (get-tab-label %)
                                          [:div.tab-content (apply tab %)]) tabs))]))))



