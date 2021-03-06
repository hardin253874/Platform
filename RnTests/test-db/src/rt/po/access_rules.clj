(ns rt.po.access-rules
  (:require [rt.lib.util :refer [throw-not-implemented]]
            [rt.lib.wd :refer [right-click double-click wait-for-jq set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [get-entity]]
            [rt.test.expects :refer [expect expect-equals]]
            [clojure.string :as string]
            [rt.po.app :as app :refer [make-app-url enable-config-mode choose-context-menu choose-modal-ok]]
            [rt.po.report-view :as report-view]
            [rt.po.common :as common :refer [exists-present? set-search-text-input-value click-modal-dialog-button-and-wait wait-until]]
            [clj-webdriver.taxi :refer [to element elements find-elements-under find-element-under attribute clear input-text value
                                        text click select-option selected? displayed? exists? select-by-text toggle selected-options]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

;;Driver functions related to access rules toolbar
(defn edit-button-visible? []
  (exists? "button[title*=Edit]"))
(defn save-button-visible? []
  (exists? "button[title*=Save]"))
(defn close-button-visible? []
  (exists? "button[title*=Cancel]"))

;; note - removed some click-xxxx functions as they were redefined later in this file

(defn add-new-access-rule []
  (click "button[title*='New Access Rule']")
  (wait-until #(exists-present? ".sp-new-type-dialog .modal-header:contains('New Access Rule')")))

(defn get-row-index-from-row-element [element]
  (let [n (read-string (attribute element "rowindex"))]
    (if (number? n) n nil)))

(defn get-row-element [rule-name object-name query-name]
  (report-view/set-search-text (or rule-name object-name))
  (wait-for-angular)
  (let [elements (elements ".ngRow")
        element (first (filter #(and (.contains (text %) (str object-name "\n"))
                                     (.contains (text %) (str query-name "\nEdit")))
                               elements))]
    element)
  )

(defn get-row-element-in-view-mode [rule-name object-name query-name]
  (report-view/set-search-text rule-name)
  (wait-for-angular)
  (let [elements (elements ".ngRow")
        element (first (filter #(and (.contains (text %) (str object-name "\n")) (.contains (text %) query-name)) elements))]
    element)
  )

(defn access-rule-enabled? [rule-name object-name query-name]
  (let [rowindex (get-row-index-from-row-element (get-row-element-in-view-mode rule-name object-name query-name))]
    (let [checkbox (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col0 input"))]
      (selected? checkbox))))

(defn enable-access-rule [rule-name object-name query-name]
  (if (save-button-visible?)
    (let [rowindex (get-row-index-from-row-element (get-row-element rule-name object-name query-name))]
      (let [checkbox (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col0 input"))]
        (when (not (selected? checkbox)) (click checkbox))))))

(defn disable-access-rule [rule-name object-name query-name]
  (if (save-button-visible?)
    (let [rowindex (get-row-index-from-row-element (get-row-element rule-name object-name query-name))]
      (let [checkbox (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col0 input"))]
        (when (selected? checkbox) (click checkbox))))))

(defn select-access-rule-operation [rule-name object-name query-name operation-name]
  (if (save-button-visible?)
    (let [rowindex (get-row-index-from-row-element (get-row-element rule-name object-name query-name))]
      (let [combobox (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col3 select"))]
        (select-by-text combobox operation-name)))))

(defn edit-access-rule [rule-name object-name query-name]
  (if (save-button-visible?)
    (let [rowindex (get-row-index-from-row-element (get-row-element rule-name object-name query-name))]
      (let [button (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col5 button[id*=edit]"))]
        (click button)))))

(defn delete-access-rule [rule-name object-name query-name]
  (if (save-button-visible?)
    (let [rowindex (get-row-index-from-row-element (get-row-element rule-name object-name query-name))]
      (let [button (element (str ".ngRow[rowindex=" rowindex "] .ngCellText.col5 button[id*=delete]"))]
        (click button)
        (choose-modal-ok)))))

(defn get-row-count []
  (count (elements ".ngRow")))

;; Driver functions related to navigation access custom UI page

(defn set-user-or-role [text]
  (select-by-text "#navAccessRoles" text))

(defn user-or-role-option-exists? [text]
  (exists? (str "#navAccessRoles" " option[label='" text "']")))

(defn set-application [text]
  (select-by-text "#navAccessApp" text))

(defn application-option-exists? [text]
  (exists? (str "#navAccessApp" " option[label='" text "']")))

(defn set-include-users-value
  "Use this for navigation access custom UI pages."
  [value]
  (let [e (str "input[type='checkbox'][ng-change='onIncludeUsersChanged()']:visible")]
    (when-not (= (selected? e) value) (click e))))

(defn get-include-users-value []
  (selected? (str "input[type='checkbox'][ng-change='onIncludeUsersChanged()']:visible")))

(defn find-button [title]
  (element (str "[title=\"" title "\"]")))

(defn click-edit-button []
  (click (find-button "Edit"))
  (wait-for-angular))

(defn click-save-button []
  (click (find-button "Save"))
  (wait-for-angular))

(defn click-close-button []
  (click (find-button "Cancel"))
  (wait-for-angular))

;; intended to work with unique named nodes in nav tree
(defn get-node-in-nav-tree [text]
  (str ".ngCellText span:contains(" text ")"))

(defn node-in-nav-tree-exists? [text]
  (exists-present? (get-node-in-nav-tree text)))

(defn get-node-path [text]
  (attribute (get-node-in-nav-tree text) :uib-tooltip))

(defn get-node-checkbox [text]
  (str ".ngCellText:contains(" text ") input[type=checkbox] "))

(defn node-checked? [text]
  (selected? (get-node-checkbox text)))

(defn set-node-value [text value]
  (let [e (get-node-checkbox text)]
    (when-not (= (selected? e) value)
      (click e))))

; added specifically for the transition to form tab. don't use in any other context.
(defn- get-row-info [el]
  (let [e (:webelement el),
        idx (.getAttribute e "rowindex"),
        obj (clojure.string/trim (text (element (str ".ngRow[rowindex=" idx "] .col2")))),
        pe (element (str ".ngRow[rowindex=" idx "] .col3 select")),
        pv (element (str ".ngRow[rowindex=" idx "] .col3")),
        p (clojure.string/trim (text (if (nil? pe) pv (first (selected-options (str ".ngRow[rowindex=" idx "] .col3 select")))))),
        q (clojure.string/trim (text (element (str ".ngRow[rowindex=" idx "] .col4"))))]
    {:index idx, :object obj :permissions p :query q}))

(defn- get-rows [object permissions query]
  (for [el (elements (str ".ngRow:has(.col2:contains('" object "')):has(.col3:contains('" permissions "')):has(.col4:contains('" query "'))"))] (get-row-info el)))

(defn- get-exact-rows [rows object permissions query]
  (filter #(and (= permissions (:permissions %)) (and (= query (:query %)) (= object (:object %)))) rows))

(defn get-row [object permissions query]
  (let [rows (rt.po.access-rules/get-rows object permissions query),
        exact (get-exact-rows rows object permissions query)]
    (if (= (count exact) 0) (first rows) (first exact))))

(defn get-last-row [object permissions query]
  (let [rows (rt.po.access-rules/get-rows object permissions query),
        exact (get-exact-rows rows object permissions query)]
    (if (= (count exact) 0) (last rows) (last exact))))

(defn get-exact-row [object permissions query]
  (let [rows (rt.po.access-rules/get-rows object permissions query)]
    (first (get-exact-rows rows object permissions query))))

(defn get-last-exact-row [object permissions query]
  (let [rows (rt.po.access-rules/get-rows object permissions query)]
    (last (get-exact-rows rows object permissions query))))

(defn row-exists? [object permissions query]
  (not (nil? (get-row object permissions query))))

(defn exact-row-exists? [object permissions query]
  (not (nil? (get-exact-row object permissions query))))

(defn row-enabled? [row]
  (selected? (str ".ngRow[rowindex=" (:index row) "] .ngCellText.col0 input[type='checkbox']")))

(defn set-row-enabled [row check]
  (let [q (str ".ngRow[rowindex=" (:index row) "] .ngCellText.col0 input[type='checkbox']")]
    (when-not (= (selected? q) check) (toggle q))))

(defn set-row-operation [row operation]
  (let [el (element (str ".ngRow[rowindex=" (:index row) "] .ngCellText.col3 select"))]
    (select-by-text el operation)))

	
(defn set-access-summary-search [value]
	(set-input-value ".sp-subject-record-access-summary input[ng-model*=value]" value)
)	
	
(defn right-click-row [row]
  (when-let [el (element (str ".ngRow[rowindex=" (:index row) "]"))]
    (rt.lib.wd/right-click el)))

(defn double-click-row [row]
  (when-let [el (element (str ".ngRow[rowindex=" (:index row) "]"))]
    (rt.lib.wd/double-click el)))

