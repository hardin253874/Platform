(ns rt.po.access-rules-new
  (:require [rt.lib.util :refer [throw-not-implemented]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.po.common :refer [click-modal-dialog-button-and-wait]]
            [rt.po.edit-form :as ef]
            [rt.po.app :as app :refer [make-app-url enable-config-mode choose-context-menu choose-modal-ok]]
            [clj-webdriver.taxi :refer [to element elements find-elements-under find-element-under attribute clear input-text value
                                        text click select-option selected? displayed? exists? select-by-text selected-options]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

(defn new-dialog-visible? []
  (exists? ".sp-new-type-dialog div:contains(New Access Rule)"))

(defn set-role [role-name]
 (select-by-text ".sp-new-type-dialog select[id=roles]" role-name) )

(defn get-role []
  (text (first (selected-options ".sp-new-type-dialog select[id=roles]"))) )

(defn get-include-user []
  (selected? ".sp-new-type-dialog input[id=includeUsers]"))

(defn set-include-user [value]
  (when (not= (get-include-user) value)
    (click ".sp-new-type-dialog input[id=includeUsers]")))

(defn set-object [object-name]
  (select-by-text ".sp-new-type-dialog select[id=objects]" object-name))

(defn get-object []
  (text (first (selected-options ".sp-new-type-dialog select[id=objects]"))))

(defn click-ok []
  (choose-modal-ok))

(defn click-cancel []
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(Cancel)"))

(defn set-include-users-value [value]
  (ef/set-bool-field-value "Include Users"  value))

(defn get-include-users-value []
  (ef/bool-field-value "Include Users"))

(defn set-user-or-role [text]
  (select-by-text "#roles" text))

(defn user-or-role-option-exists? [text]
  (exists? (str "#roles" " option[label='" text "']")))

(defn set-object [text]
  (select-by-text "#objects" text))

(defn object-option-exists? [text]
  (exists? (str "#objects" " option[label='" text "']")))