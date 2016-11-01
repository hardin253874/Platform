(ns rt.user
  "The default namespace when running test step expressions."
  (:require [rt.expects :refer :all]
            [rt.session :refer [*tc* make-artifact-filename get-test-id get-test-duration get-suite-id
                                get-post-test-messages]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser]]
            [rt.lib.util :refer :all]
            [clj-webdriver.taxi :as taxi]
            [clojure.set :refer [subset? intersection]]))

(defn has-tags? [tags test]
  (let [tags (map #(if (string? %) (keyword %) %) tags)]
    (subset? (set tags) (set (:tags test)))))

(defn has-some-tags? [tags test]
  (let [tags (map #(if (string? %) (keyword %) %) tags)]
    (not (empty? (intersection (set tags) (set (:tags test)))))))

(defn has-matching-step? [re test]
  (->> test :steps (map #(get-in % [:script])) (some #(some->> % (re-find (re-pattern re))))))

(defn has-index-in-range? [from to {:keys [index]}]
  (let [from (or from 0)
        to (or to -1)]
    (or (not index)
        (and (>= index from) (or (< to 0) (<= index to))))))




