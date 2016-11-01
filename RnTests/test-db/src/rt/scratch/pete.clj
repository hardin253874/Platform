(comment

  (:require [rt.lib.wd :refer [right-click debug-click set-input-value double-click cancel-editable-edit-mode
                               wait-for-jq find-element-with-text wait-until-displayed get-repeated-elements
                               has-class]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [drag-n-drop test-id-css set-click-to-edit-value navitem-isdirty?]]
            [rt.lib.util :refer [timeit]]
            [rt.po.common :refer [exists-present? click-modal-dialog-button-and-wait]]
            [rt.po.edit-form :as ef]
            [rt.po.report-view :as rv]
            [rt.po.common :refer [safe-text exists-present? set-search-text-input-value wait-until]]
            [clj-webdriver.taxi :refer [text attribute send-keys elements element exists? displayed? find-element-under *driver* value input-text clear]]
            [clj-webdriver.core :refer [->actions move-to-element]]
            [clojure.string :as string]
            [rt.po.app :as app]
            [clj-webdriver.taxi :as taxi]
            [clj-webdriver.taxi :as taxi :refer [to text exists? click value selected? find-element select-by-text selected-options select-option find-element-under find-elements-under element elements element attribute visible? *driver*]]            
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]])
  
  (do
    ;; set some namespaces
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all])
    (require '[clojure.set :refer [subset?]])
    (require '[clj-webdriver.taxi :as taxi])
    (require '[rt.po.import-spreadsheet :as is])
    (require '[rt.po.report-builder])
    
    ;; set the default test context 
    (rt.test.core/merge-tc {:target   :chrome
                            :tenant   "EDC"
                            :username "Administrator" :password "tacoT0wn"})

    
    )

  (do
    ;; write test script here 
    (rn.common/start-app-and-login)
    (rt.po.app/navigate-to "CRM")
    (rt.po.form-builder-config/get-calculation-error)
    (rt.po.form-builder-config/set-calculation "[Name]+")
    (rt.po.form-builder-config/ok-disabled?)
    (rt.po.form-builder/add-from-field-menu-to-container "Calculation" 0)
    
    (rt.po.report-builder/save-as "Test123")
    )

  (defn save-as [name]
  (click ".report-Builder-ToolBar button[ng-click*=saveAsReportEntity]")
  (set-input-value ".reportSaveAsDialog-view [ng-model*=reportName]" name)
  (taxi/click "button:contains(OK)")
  (wait-for-angular)
  (when (rt.po.common/exists-present? ".reportSaveAsDialog-view input")
    (throw (Exception. "Did not expect dialog to be present."))))
  
   (defn save-as [name]
  (taxi/click ".report-Builder-ToolBar button[ng-click*=saveAsReportEntity]")
  (set-input-value ".reportSaveAsDialog-view input" name)
  (app/choose-modal-ok)
  (wait-for-angular)
  (when (rt.po.common/exists-present? ".reportSaveAsDialog-view input")
    (throw (Exception. "Did not expect dialog to be present."))))
  
  )




