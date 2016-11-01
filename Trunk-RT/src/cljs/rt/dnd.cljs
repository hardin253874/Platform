(ns rt.dnd
  (:require [reagent.core :as r]
            [re-frame.core :refer [subscribe dispatch dispatch-sync register-sub register-handler]]
            [clojure.string :as string]))

(def state (r/atom {}))

(defn remove-html-class [e className]
  (set! (.-className e)
        (-> (.-className e)
            (string/replace (re-pattern (str "(?:^|\\s)" className "(?!\\S)")) ""))))

(defn add-html-class [e className]
  (remove-html-class e className)
  (set! (.-className e) (str (.-className e) " " className)))

(defn on-drag-start [data e]
  (let [el (.-currentTarget e)]
    (println "drag-start" data e (.-className el) (.getAttribute el "data-reactid"))
    (set! (.-effectAllowed (.-dataTransfer e)) "copy")
    (.setData (.-dataTransfer e) "text/plain" (pr-str data))
    (swap! state merge {:source el :data data}))
  js/undefined)

;; With dnd we get an enter when moving over visually nested elements
;; and then a leave on the original and then we we move off the 'child'
;; we get a leave on the child but not on the parent.
;; We typically don't want to remove the css class we add on the original in this case.
;; So we ignore the leave events unless they match the target of the most recent enter.

(defn on-drag-enter [e]
  (let [el (.-currentTarget e)]
    (swap! state #(if (not= (:source %) el)
                   (do
                     (add-html-class el "dragging-over")
                     (assoc-in % [:over el] (inc (get-in % [:over el]))))
                   %))
    (println "drag-enter" e (.-className el) (.getAttribute el "data-reactid")
             "ignoring" (= (:source @state) el)))
  js/undefined)

(defn on-drag-over [e]
  ;(println "drag-over" e)
  (when (.-preventDefault e) (.preventDefault e))
  (set! (.-dropEffect (.-dataTransfer e)) "copy")
  ;; could do things like coords
  (swap! state merge {})
  js/undefined)

(defn on-drag-leave [e]
  (let [el (.-currentTarget e)]
    (println "drag-leave" e (.-className el) (.getAttribute el "data-reactid")
             "ref cnt" (get-in @state [:over el]))
    (swap! state
           #(let [ref-cnt (dec (get-in % [:over el]))
                  state (assoc-in % [:over el] ref-cnt)
                  state (if (= 0 ref-cnt)
                          (do (remove-html-class el "dragging-over")
                              (assoc state :over (dissoc (:over state) el)))
                          state)]
             state)))
  js/undefined)

(defn on-drop [e]
  (let [el (.-currentTarget e)]
    (println "drop" e)
    (when (.-stopPropagation e) (.stopPropagation e))
    (remove-html-class el "dragging-over")
    (swap! state merge {:target el})
    (.getData (.-dataTransfer e) "text/plain")))

(defn on-drag-end [e]
  (println "drag-end" e)
  (reset! state {})
  js/undefined)

(defn drag-source-attrs [get-data-fn]
  {:draggable   "true"
   :onDragStart #(on-drag-start (get-data-fn %) %)
   :onDragEnd   on-drag-end})

(defn drag-target-attrs [on-drop-fn]
  {:onDrop      #(on-drop-fn (:data @state) (on-drop %))
   :onDragEnter on-drag-enter
   :onDragOver  on-drag-over
   :onDragLeave on-drag-leave})

#_(register-handler
  :on-drag-start
  (fn [db [_ data target]]
    (assoc-in db [:entity key] value)))

