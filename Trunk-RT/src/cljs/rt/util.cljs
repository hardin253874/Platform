(ns rt.util
  (:require-macros [cljs.core.async.macros :refer [go alt! go-loop]])
  (:require [clojure.string :as string]
            [cljs.core.async :refer [put! <! >! chan timeout close! alts!]]))

(defn id->str [id]
  (let [s (str id)]
    (if (= 0 (.indexOf s ":")) (subs s 1) s)))

(defn ensure-valid-id [id]
  #_(println "validating" id "=>" (string/replace id #"[^a-zA-Z0-9\-/]" ""))
  (string/replace id #"[^a-zA-Z0-9\-/\.]" "-"))

(defn ensure-keyword [v]
  (when v
    (keyword (ensure-valid-id (id->str v)))))

(defn ensure-keyword-values [keys entity]
  {:pre [entity (not-empty entity)]}
  (reduce #(assoc %1 %2 (keyword (%2 %1))) entity keys))

(defn get-datetime-str
  ([] (get-datetime-str (js/Date.)))
  ([d] (str (.getFullYear d) (.getMonth d) (.getDay d) (.getHours d) (.getMinutes d) (.getSeconds d))))

(defn url-encode
  [s]
  (some-> s str (js/encodeURIComponent) (.replace "+" "%20")))

(defn set-location [uri]
  (set! (.-href (.-location js/window)) (str "#" uri)))

(defn throttle
  ([c ms] (throttle (chan) c ms))
  ([c' c ms]
    (go
      (loop [v (<! c) last nil waiting? false]
        (if-not waiting?
          (do
            (>! c' v)
            (<! (timeout ms))
            ;; not sure why >! doesn't work for the following
            (put! c :timeout)
            (recur (<! c) nil true))
          (if (= :timeout v)
            (recur (or last (<! c)) nil false)
            (recur (<! c) v true)))))
    c'))

(defn throttle2
  ([c ms] (throttle2 (chan) c ms))
  ([c' c ms]
    (go
      (loop [t nil v (<! c) prev nil]
        (if (nil? t)
          (do
            (>! c' v)
            (recur (js/Date.) nil nil))
          (let [[v _] (alts! [c (timeout ms)])]
            (if v
              (if (>= (- (js/Date.) t) ms)
                (recur nil v nil)
                (recur (js/Date.) nil v))
              (recur nil (or prev (<! c)) nil))))))
    c'))

(defn filter-records [q items]
  (let [re (re-pattern (str "(?i)" (or q "")))
        items (filter #(re-find re (or (:pr-str %) (pr-str %))) items)]
    items))

