(ns rt.codemirror
  (:require [reagent.core :as r]))

(defn- inc-cursor [c]
  #js {:ch (inc (.-ch c)) :line (.-line c)})

(defn- get-closest-form-range [cm]
  (let [sc (.getSearchCursor cm #"[\(\)]" (.getCursor cm))
        c (if (.findPrevious sc) (.to sc) (.getCursor cm))]
    (let [r (.findMatchingBracket cm c)]
      (when (and r (.-match r))
        (let [from (if (.-forward r) (.-from r) (.-to r))
              to (if (.-forward r) (.-to r) (.-from r))]
          [from (inc-cursor to)])))))

(defn- get-closest-form [cm]
  (when-let [[from to] (get-closest-form-range cm)]
    (.getRange cm from to)))

(defn- select-closest-form [cm]
  (when-let [[from to] (get-closest-form-range cm)]
    (.setSelection cm from to)))

(defn cm-input [{:keys [value initialLine onChange onSelectionChanged onCtrlEnter] :as opts}]
  (let [cm (r/atom nil)
        cm-opts #js {:matchBrackets true :autoCloseBrackets true
                     :showHint      true
                     :lineNumbers   true
                     :extraKeys     #js {"Ctrl-Space"     "autocomplete"
                                         "Ctrl-Enter"     (or onCtrlEnter #())
                                         "Ctrl-Alt-Enter" #(select-closest-form @cm)}}
        initial-line (r/atom initialLine)
        goto-line (fn [label]
                    (when (and @initial-line (not-empty @value))
                      ;(println label "goto" @initial-line @value)
                      (let [line @initial-line]
                        (.setTimeout js/window #(.setCursor @cm line) 300))
                      (reset! initial-line nil)))]
    (r/create-class
      {:reagent-render
       (fn [{:keys [value]}]
         ;; Even though this text area is hidden by CodeMirror and there isn't a real
         ;; need to be setting or updating its value, we do use its initial value
         ;; and also we want to deref the value atom so this component is updated when needed.
         [:textarea {:defaultValue @value}])
       :component-did-mount
       (fn [component]
         ;; note - initial value comes from the textarea
         (reset! cm (.fromTextArea js/CodeMirror (r/dom-node component) cm-opts))
         (when onChange
           (.on @cm "change" #(onChange (.getValue @cm))))
         (when onSelectionChanged
           (.on @cm "cursorActivity"
                #(let [text (.getSelection @cm)
                       text (if (not-empty text) text
                                                 (get-closest-form @cm))]
                  (onSelectionChanged text))))
         (goto-line "did mount"))
       :component-did-update
       (fn [_ _ _]
         (if (not= (.getValue @cm) @value)
           (.setValue @cm @value))
         (goto-line "did update"))
       :component-did-unmount
       #()
       :display-name
       "cm-input"})))

