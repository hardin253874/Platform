(ns rt.po.workflow-user-input
  (:require [clojure.test :refer :all]
			[clj-webdriver.taxi :as taxi]
			[clj-webdriver.core :refer [->actions move-to-element]]
            [rt.lib.wd :as wd]
            [rt.lib.wd-ng :as wd-ng]
            [rt.po.app :as app]
            [rt.po.common :as common]
			[clj-time.core :as t]
			[clj-time.coerce :as tc]
            [clj-time.format :as tf]))

; gets the container of the input with the given label
(defn get-input-container [label]
  (let [el (taxi/element {:xpath (str "//span[text()=\"" label "\"]/ancestor::*[contains(@class, \"workflow-input-row\")]")})] el))

; gets the value of the input associated with the label
(defn get-workflow-input-text [label]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [i (taxi/find-element-under row {:tag :input})]
      (let [v (taxi/attribute i "value")]
        (println v) v))))

; sets the text "value" in to the input with the given label
(defn set-workflow-input-text [label value]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [i (taxi/find-element-under row {:tag :input})]
      (rt.lib.wd/set-input-value i (str value)))))

; sets the first record that matches the search text "value" in to the input with the given label
(defn set-workflow-input-record [label value]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [btn (taxi/find-element-under row {:tag :button})]
      (taxi/click btn)
      (rt.lib.wd-ng/wait-for-angular)))
  (rt.po.edit-form/choose-in-entity-picker-dialog value))

; gets if the checkbox associated with the input label is checked or not
(defn get-workflow-input-checked? [label]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
        (let [i (taxi/find-element-under row {:tag :input})]
          (let [c (taxi/selected? i)] c))))

; ensures that the boolean input associated with the given label is checked or unchecked according to the true/false value of "check"
(defn set-workflow-input-checked [label check]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [i (taxi/find-element-under row {:tag :input})]
      (when-not (= (taxi/selected? i) check) (taxi/click i)))))

; sets the text "value" in to the date input with the given label
(defn set-workflow-input-date [label value]
  (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [d (taxi/find-element-under row {:css "input.date-input"})]
      (taxi/click d)
      (rt.lib.wd/set-input-value d (str value)))))

; gets the value in the time input given by label, as a concatenated string
(defn get-workflow-input-time [label]
   (let [row (rt.po.workflow-user-input/get-input-container label)]
    (let [i (taxi/find-element-under row {:css "input.time-input"})]
      (let [v (taxi/attribute i "value")]
        (println v) v))))

; sets the text value for "hours" and "minutes" into the time input with the given label
(defn set-workflow-input-time [label hours minutes meridian]  
  (rt.po.common/set-time-control-value-via-popup (str "div.workflow-input-row:has(div.workflow-input-label:contains('" label "'))") hours minutes meridian))

; waits until the expected prompt message has appeared
(defn wait-for-message [message]
  (taxi/wait-until #(taxi/exists? { :xpath (str "//h2[span[text()=\"" message "\"]]") }))
  (rt.lib.wd-ng/wait-for-angular))

; click the done button, submitting the form
(defn done []
  (let [b (taxi/element {:css ".workflow-input-accept-button"})]
        (taxi/click b)))
