(ns rt.po.access-summary
  (:require [rt.lib.util :refer [throw-not-implemented]]
            [rt.lib.wd :refer [right-click double-click wait-for-jq set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [get-entity]]
            [rt.test.expects :refer [expect expect-equals]]
            [clojure.string :as string]
            [rt.po.app :as app :refer [make-app-url enable-config-mode choose-context-menu choose-modal-ok]]
            [rt.po.common :as common :refer [exists-present? set-search-text-input-value click-modal-dialog-button-and-wait]]
            [clj-webdriver.taxi :refer [to element elements find-elements-under find-element-under attribute clear input-text value
                                        text click select-option selected? displayed? exists? select-by-text]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

(defn click-refresh []
  (click (element ".sp-subject-record-access-summary button.refreshButton"))
  (wait-for-angular))

(defn set-search-text [text]
  (set-input-value (element ".sp-subject-record-access-summary .sp-search-control-input") text)
  (wait-for-angular))

(defn row-selector [object perms scope role]
  (str ".sp-subject-record-access-summary .ngGrid .ngRow:has(.col0:contains(" object ")):has(.col1.ngCellText[title='" perms "']):has(.col2:contains(" scope ")):has(.col3:contains(" role "))"))

(defn row-exists? [object perms scope role]
  (common/exists-present? (row-selector object perms scope role)))

(defn row-reason [object perms scope role]
  (text (str (row-selector object perms scope role) " .col4" )))
  