(ns rt.po.report-sort-option
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.test.core :refer [*test-context*]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.common :as common :refer [exists-present? wait-until]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? find-elements select-by-text selected-options select-option element elements element attribute visible?]]))


(defn get-sort-lists []
  (elements "[ng-repeat*='si in model.sortInfo']")
  )

(defn get-sort-list [index]
   (nth (get-sort-lists) index)
  )

(defn click-add-sorting []
  (click "button:contains(Add Sorting)"))


(defn set-sorting [index colname dir]
  (select-by-text (nth (elements "[ng-model*='si.column']") index) colname)
  (select-by-text (nth (elements "[ng-model*='si.sortDirection']") index) dir)

  )

(defn set-sortings [sort-info]
  (wait-until #(expect-equals (count (get-sort-lists)) (count sort-info) ))

  (doall (map-indexed (fn [index item]
                        (set-sorting index (str (get item :colname)) (str (get item :dir)))

                        )
                      sort-info))

  )


(comment
  #_(def sort-info [{:colname "AA_All Fields"  :dir "Ascending"} {:colname "DateTime" :dir "Descending"}])

  #_(map-indexed (fn [index item]
                 (println "insert" item "at index" index))
               sort-info)

  #_(set-sortings sort-info)


  )
;; BUTTONS

(defn click-ok []
  ;(wait-until #(expect (not= (text (first (selected-options (last (elements "[ng-model*='si.column']"))))) "[Select]")))

  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))