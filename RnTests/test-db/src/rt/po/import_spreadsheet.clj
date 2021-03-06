(ns rt.po.import-spreadsheet
  (:require [rt.lib.wd :refer [right-click set-input-value wait-for-jq wait-until-displayed]]
            [rt.lib.wd-ng :refer [execute-script-on-element wait-for-angular]]
            [rt.lib.wd-rn :refer [drag-n-drop]]
            [rt.po.common :refer [exists-present? set-search-text-input-value click-modal-dialog-button-and-wait]]
            [rt.po.report-view :as rv]
            [clj-webdriver.taxi :as taxi]))

;;todo move to common location
(defn click [q]
  (taxi/wait-until #(empty? (taxi/elements ".busyIndicator-view")) 10000)
  (try
    (taxi/click q)
    (catch org.openqa.selenium.WebDriverException ex
      (println "WebDriver Exception when clicking on \"" q "\". Exception=" ex)
      (throw ex))))


;; navigation

(defn choose-next []
  (click ".importspreadsheet-view a[ng-click*='handleNext']"))

(defn choose-back []
  (click ".importspreadsheet-view a[ng-click*='handlePrevious']"))

(defn wait-for-complete
  ([] (wait-for-complete 30000))
  ([msecs] (taxi/wait-until #(taxi/present? ".showDetails:contains('action completed')") msecs 500)))


;;page 1 - upload

(defn choose-file-to-import
  [path]
  (rt.po.common/wait-until #(exists-present? ".importfileUpload input[type=file]") 5000)
  (taxi/send-keys ".importfileUpload input[type=file]" path)
  (when-not (rt.po.common/wait-until
              #(taxi/visible? ".importspreadsheet-view .importUploadState") 10000)
    (throw (Exception. (str "Failed to upload document:" path)))))

(defn no-heading-row? []
  (taxi/selected? (taxi/element "input#noHeadingRow")))

(defn set-no-heading-row [value]
  (when (not= (no-heading-row?) value)
    (taxi/click (taxi/element "input#noHeadingRow") )))

(defn heading-row []
  (taxi/attribute (taxi/element "input#hRowNo") "value"))

(defn set-heading-row [n]
  (rt.lib.wd/set-input-value "input#hRowNo" (str n)))

(defn data-start-row []
  (taxi/attribute (taxi/element "input#dRowNo") "value"))

(defn set-data-start-row [n]
  (rt.lib.wd/set-input-value "input#dRowNo" (str n)))

(defn data-last-row []
  (taxi/attribute (taxi/element "input#lastRowNo") "value"))

(defn set-data-last-row [n]
  (rt.lib.wd/set-input-value "input#lastRowNo" (str n)))

(defn separator []
  (taxi/text (first (taxi/selected-options "select#fileSep"))))

(defn set-separator [value]
  (taxi/select-option "select#fileSep" {:text value}))

(defn import-type []
  (taxi/text (first (taxi/selected-options "select#importType"))))

(defn set-import-type [value]
  (taxi/select-option "select#importType" {:text value}))


;;page 2 - select object

(defn choose-target-object [object-name]
  (let [q ".importspreadsheet-view button.inlineRelPicker-button"]
    ;; adding this to see if can get past an occasional error
    (wait-until-displayed q)
    (taxi/click q))
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" object-name)
  (rv/select-row-by-text object-name ".entityReportPickerDialog .dataGrid-view")
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))


;;page 3 - mapping

(defn find-mapping [column]
  (str ".ngRow:has(.col0:contains('" column "'))"))

(defn get-field-mapping [column]
  (taxi/text (first (taxi/selected-options (str (find-mapping column) " .col1 select")))))

(defn set-field-mapping [column value]
  (taxi/select-option (str (find-mapping column) " .col1 select") {:text value}))

(defn get-field-details [column]
  (taxi/text (str (find-mapping column) " .targetType")))

(defn field-options-visible? [column]
  (taxi/visible? (str (find-mapping column) " .targetOptions")))

(defn click-field-options [column]
  (taxi/click (str (find-mapping column) " .targetOptions")))

;;(defn set-all-mappings [col-field-map]
;;    (doall (map-indexed #(set-field-mapping %1 %2) set-all-mappings)))

(defn sample-1 [column]
  (clojure.string/trim (taxi/text (str (find-mapping column) " .col3"))))

(defn sample-2 [column]
  (clojure.string/trim (taxi/text (str (find-mapping column) " .col4"))))


;;page 3 - mapping options

(defn find-using []
  (taxi/text (first (taxi/selected-options " .findLookupUsing"))))

(defn set-find-using [value]
  (taxi/select-option " .findLookupUsing" {:text value}))

(defn click-ok []
  (taxi/click ".memberMappingOptions button:contains('OK')"))

(defn click-cancel []
  (taxi/click ".memberMappingOptions button:contains('Cancel')"))


;;page 4 - options

(defn config-name []
  (taxi/attribute (taxi/element "input#configName") "value" ))

(defn set-config-name [value]
  (rt.lib.wd/set-input-value "input#configName" value))

(defn test-import? []
  (taxi/selected? (taxi/element "input#testRun")))

(defn set-test-import [value]
  (when (not= (test-import?) value)
    (taxi/click (taxi/element "input#testRun"))))

(defn suppress-workflows? []
  (taxi/selected? (taxi/element "input#suppressWorkflows")))

(defn set-suppress-workflows [value]
  (when (not= (suppress-workflows?) value)
    (taxi/click (taxi/element "input#suppressWorkflows"))))

(defn save-without-importing []
  (taxi/click (taxi/element "a#saveConfig")))


