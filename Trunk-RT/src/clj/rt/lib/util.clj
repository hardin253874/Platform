(ns rt.lib.util
  (:require [clojure.string :as string]
            [clojure.data.csv :as csv]
            [clojure.java.io :as io]
            [clojure.walk :as walk]
            [clojure.data.codec.base64 :refer [encode]]
            [clj-time.core :as t]
            [clj-time.format :as tf]
            [clj-time.local :as tl]
            [clj-time.coerce :as tc]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import org.openqa.selenium.Keys
           java.text.SimpleDateFormat
           [javax.imageio ImageIO]
           java.util.Date
           (java.awt Toolkit)))

(defn load-csv-data [path]
  (with-open [in-file (if (.exists (io/file path))
                        (io/reader path)
                        (io/reader (io/resource path)))]
    (doall
      (csv/read-csv in-file))))

(defn load-csv-objects [path]
  (let [csv (load-csv-data path)
        keys (map #(keyword (string/trim %)) (first csv))]
    (reduce (fn [result data]
              (conj result (zipmap keys (map #(string/trim %) data))))
            []
            (rest csv))))

(defn seq->table
  "Convert a sequence of Clojure maps (all same keys) to a table like format that is a vector of vectors
  with the map keys as the first vector and the values for each map as the subsequent vectors."
  [xs]
  {:pre [(or (vector? xs) (seq? xs))]}
  (let [ks (keys (first xs))]
    (into (vector (map name ks)) (mapv #(map % ks) xs))))

(defn write-csv-objects [file-name xs]
  {:pre [(or (vector? xs) (seq? xs))]}
  (with-open [out-file (clojure.java.io/writer file-name)]
    (clojure.data.csv/write-csv out-file (seq->table xs))))

(defn datetime-str
  ([] (datetime-str (Date.)))
  ([date] (.format (SimpleDateFormat. "yyyyMMdd-HHmmss") date)))

(defn csv-datetime-str
  ([] (datetime-str (Date.)))
  ([date] (.format (SimpleDateFormat. "yyyy/MM/dd HH:mm:ss") date)))

(defn make-test-name
  "Make a suitable record name for use in tests... _fwiw it adds a timestamp_
  plus a random bit at the end."
  [prefix]
  (str prefix "-" (datetime-str) "-" (format "%x" (rand-int 100000))))

(defn str->base64 [original]
  (String. (encode (.getBytes original)) "UTF-8"))

(defmacro timeit
  "A macro to print the time an expression takes to run."
  [label expr]
  `(let [t0# (.getTime (java.util.Date.))
         result# ~expr]
     (debug ~label "time:" (- (.getTime (java.util.Date.)) t0#))
     result#))

(defn time-check [t f & args]
  (let [t0 (.getTime (java.util.Date.))
        _ (apply f args)
        t1 (.getTime (java.util.Date.))]
    (< (- t1 t0) t)))

(defn get-time [f & args]
  (let [t0 (.getTime (java.util.Date.))
        _ (apply f args)
        t1 (.getTime (java.util.Date.))]
    (- t1 t0)))

(defn indices
  "Return a lazy sequence of the indexes of the collection
  elements that the given predicate return non-nil."
  [pred coll]
  (keep-indexed #(when (pred %2) %1) coll))

(defn deep-merge
  "Recursively merges maps. If vals are not maps, the last value wins."
  [& vals]
  (if (every? map? vals)
    (apply merge-with deep-merge vals)
    (last vals)))

(defn mapcat-indexed
  "Returns the result of applying concat to the result of applying map-indexed
  to f and colls.  Thus function f should return a collection."
  [f & colls]
  (apply concat (apply map-indexed f colls)))

(defn get-fqdn
  "Get the fqdn of the local host in our environment... beware that it makes assumptions
  about computer names and the domains certain prefixes imply."
  []
  (if-let [host (System/getenv "COMPUTERNAME")]
    (cond
      (.startsWith host "SYD1") (str host ".entdata.local")
      (.startsWith host "RN") (str host ".readinow.net")
      :else (str host ".sp.local"))
    ;; it'll be nil on OS X... to investigate how to do this there
    "localhost"))

(defn ms->s [t]
  "Convert msec to secs"
  (if (nil? t) 0 (-> t (/ 1000) float)))

(defn left-with-elipses [s n]
  (if s
    (let [n (min n (count s))]
      (str (subs s 0 n) (when (> (count s) n) "...")))
    ""))

(defn wrap-string [s col]
  (->> (re-seq (re-pattern (str ".{0," col "}")) s)
       (interpose \newline)
       (apply str)))

(defn id->str [id]
  (let [s (str id)]
    (if (.startsWith s ":") (subs s 1) s)))

(defn clean-filename [filename]
  (string/replace filename #"[\\/:\"]+" "_"))

(defn csv->list [s]
  (some-> s (string/split #"[\s,]+")))

(defn ensure-valid-id [id]
  #_(println "validating" id "=>" (string/replace id #"[^a-zA-Z0-9\-/]" ""))
  (string/replace id #"[^a-zA-Z0-9\-/\.]" "-"))

(defn ensure-keyword [v]
  (when v
    (keyword (ensure-valid-id (rt.lib.util/id->str v)))))

(defn checked-csv->list [x]
  (cond
    (string? x) (csv->list x)
    (not (vector? x)) [x]
    :default x))

(defn ->ids [s]
  (mapv ensure-keyword (checked-csv->list s)))

(defn throw-not-implemented []
  (throw (Exception. "not yet implemented")))

(defn trim-leading [ch s]
  (if (= ch (first s))
    (subs s 1)
    s))

;; team city messages

(defn tc-escape [text]
  (some-> text
          (string/replace #"([|'\[\]])" "|$0")
          (string/replace #"\r" "|r")
          (string/replace #"\n" "|n")))

(defn format-tc-message [name attrs]
  (trace "format-tc-message" (str "'" name "','" attrs "'"))
  (->> attrs
       (map #(str (id->str (first %)) "='" (tc-escape (second %)) "'"))
       (interpose " ")
       (apply str)
       (#(str "##teamcity[" (id->str name) " " % "]"))))

(defn read-image
  "Reads a BufferedImage from a file.

  Example:
  =========

  (read-image \"C:\\image.png\")"
  [fileName]
  (ImageIO/read (io/file fileName)))

(defn get-image-rgba
  "Gets the RGBA values of the specified image at the specified coordinates.
  If the coordinates are not specified the colour at the centre of the image is returned.

  Example:
  =========

  (get-image-rgba (read-image \"C:\\image.png\") 1 1)"
  ([image x y]
   (let [rgb (.getRGB image x y)]
     (let [a (bit-and (bit-shift-right rgb 24) 0xff)
           r (bit-and (bit-shift-right rgb 16) 0xff)
           g (bit-and (bit-shift-right rgb 8) 0xff)
           b (bit-and rgb 0xff)]
       {:r r :g g :b b :a a})))
  ([image]
   (get-image-rgba
     image
     (int (/ (.getWidth image) 2))
     (int (/ (.getHeight image) 2)))))

(defn contains-match [a b]
  (not (nil? (some #(re-find (re-pattern a) %) b))))

(defn match [a b]
  (not (nil? (re-find (re-pattern a) b))))

(defn style-string->map [s]
  (->> (string/split s #";")
       (map #(string/split % #":" 2))
       (map #(mapv string/trim %))
       (into {})
       (walk/keywordize-keys)))

(defn ->int [value]
  (if (string? value) (Integer/parseInt value) value))

(defn get-browser-position [& [index]]
  (let [index (or index 0)
        ;; either based on index
        ;top (* (mod index 10) 20)
        ;left (* index 10)

        ;; or random within screen
        screen-dimensions (.getScreenSize (Toolkit/getDefaultToolkit))
        ;; the max bounds bit here is to handle it being wrong on retina displays
        width (min 1920 (.getWidth screen-dimensions))
        height (min 1080 (.getHeight screen-dimensions))
        left (rand-int (/ width 3))
        top (rand-int (/ height 3))]
    (debug "DEBUG: screen size" width height)
    {:left left :top top}))

