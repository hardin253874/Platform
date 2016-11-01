(ns rt.po.document-library
  (:require [rt.lib.util :refer [throw-not-implemented]]
            [rt.lib.wd :refer [right-click double-click wait-for-jq set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [get-entity]]
            [rt.test.expects :refer [expect expect-equals]]
            [clojure.string :as string]
            [rt.po.app :as app :refer [make-app-url enable-config-mode choose-context-menu choose-modal-ok]]
            [clj-webdriver.taxi :refer [to element elements find-elements-under find-element-under attribute clear input-text value
                                        text click select-option selected? displayed? exists?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clj-webdriver.taxi :as taxi]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import (org.openqa.selenium StaleElementReferenceException)))

(defn add-new-document []
  (click (last (elements "button[title=New]")))
  (wait-for-angular))

(defn upload-document [path]
  (try
    (taxi/send-keys ".fileUpload input[type=file]" path)
    (await-for 10000)
    (catch StaleElementReferenceException ex
      (warn "Ignoring StaleElementException" ex))))

;;Download document and wait for the document to download.
(defn download-document [file-name]
  ;;Delete file with the same name exist in the download folder.
  (let [filepath (rt.po.common/get-download-file-path file-name)]
    (if (rt.po.common/file-exist? filepath)
      (rt.po.common/delete-file filepath))
    (click "sp-file-name-upload-control .edit-form-value a")
    (rt.po.common/wait-until #(rt.po.common/file-exist? filepath) 50000)))

(defn compare-two-files [file-path1 file-path2]
  (let [f1 (slurp file-path1)
        f2 (slurp file-path2)]
    (= f1 f2)))

(defn doc-library-header-selector [report]
  (str ".docLibrary-view .docLibrary-header:contains('" report "')"))

(defn doc-library-report-visible? [report]
  (exists? (doc-library-header-selector report)))

