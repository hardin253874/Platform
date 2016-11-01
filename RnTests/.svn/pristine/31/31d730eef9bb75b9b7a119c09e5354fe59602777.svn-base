(ns rt.po.form-builder
  (:import (org.openqa.selenium WebDriverException)
           org.openqa.selenium.Keys)
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
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

;; shadow taxi click with debug-click
(def click debug-click)

;;inspecting form builder rendering
(defn get-form-title []
  (text ".sp-Edit-Form-Heading"))

(defn get-form-description [] find-element-with-text
  (text ".sp-Edit-Form-Description"))

(defn get-toolbox-section [name]
  (text (str ".fb-toolbox-section:contains(" name ")")))

(defn has-toolbox-section? [name]
  (not (empty? (get-toolbox-section name))))

(defn get-field-group [name]
  (text (str ".fb-toolbox-group:contains(" name ")"))
  )

(defn has-field-group? [name]
  (not (empty? (get-field-group name))))

(defn has-toolbar? []
  (exists? ".sp-Form-Builder-ToolBar"))

(defn has-canvas? []
  (exists? ".sp-form-builder-container-content"))

(defn has-fields? []
  (exists? ".fb-toolbox-item"))

(defn has-form-controls? []
  (exists? ".sp-form-builder-form-control"))

;; Driver functions for field menu popup

(defn field-menu-item-by-type [type]
  ;; map type to menu item text
  ;; only needed if different
  (condp = type
    "DateTime" "Date and Time"
    "MultilineText" "Multiline Text"
    type))

(defn add-field-menu-selector []
  ".fb-toolbox-control[uib-popover-template*=newField] img[src*='add.png']")

(defn field-menu-opened? []
  ;;(evaluate-angular-expression ".fb-toolbox-control[popover-template*=newField]" "popover.isOpen")
  ;;the above no longer works with angular ui bootstrap, so we might need to find a better solution than that below
  (exists-present? ".fb-popover-fields"))

(defn open-add-field-menu []
  (when (not (field-menu-opened?))
    (click (add-field-menu-selector))
    (wait-until #(field-menu-opened?))))

(defn close-add-field-menu []
  (when (field-menu-opened?)
    (click (add-field-menu-selector))))

(defn- get-menu-item-element [name]
  (first (filter #(= (text %) name) (elements (str "tr.fb-field div:contains(" name ")")))))

;;todo: generalise for more levels
(defn get-accordian-item-element [top heading name]

  ;; I suspect we are in toggling UI hell here causing unreliable menu handling.
  ;; It seems the combination of css transitions and the toggle behavior of the menu sections
  ;; is making it difficult to test and open sections when needed.

  (let [open-section-query (str top " .panel-collapse.in div:contains(" name ")")]
    (when-not (exists? open-section-query)
      (println (str "get-accordian-element - clicking to open heading '" heading "' to see item '" name "'"))
      (click (str top " .panel-heading div:contains(" heading ")"))
      (try
        (wait-until #(exists? open-section-query) 2000 500)
        (catch Exception e
          (throw (Exception. (str "Failed to open accordian for heading " heading)))))))

  ;; the exposed elements have empty text for a short time after opening
  ;; so wait for them to appear

  (let [find-menu-item (fn [] (->> (elements (str top " tr.fb-field div:contains(" name ")"))
                                   ;; filtering for exact match
                                   (filter #(= (text %) name))
                                   first))]
    (timeit (str "waiting for accordian element '" name "' under heading '" heading "'")
            (try
              (wait-until find-menu-item 5000 500)
              (catch Exception e
                (throw (Exception. (str "Failed to get menu/accordian for " name " under heading " heading))))))

    (find-menu-item)))


(comment
  ;; sg - testing ... can remove after mar 22, 2015
  (rt.po.form-builder/add-from-field-menu-to-field-group "Date and Time" "Field Group 1")

  (do (open-add-field-menu)
      (get-accordian-item-element ".fb-popover-fields" "Other" "Relationship"))


  (->> (get-repeated-elements ".fb-popover-fields .panel"
                              {:heading-div ".panel-heading"
                               :content-div ".panel-collapse"})
       (map #(assoc % :heading (taxi/text (:heading-div %))
                      :open? (has-class (:content-div %) "in")))
       ;(map :heading)
       (filter #(= "Other" (:heading %)))
       first
       :heading)

  )

(defn get-field-menu-item-element [name]
  ;; TODO: make case insensitive
  (let [name (field-menu-item-by-type name)
        groups {"Text Fields"        ["Text" "Multiline Text"]
                "Numeric Fields"     ["Number" "AutoNumber" "Decimal" "Currency"]
                "Date & Time Fields" ["Date" "Time" "Date and Time"]}
        heading (select-keys groups (for [[k v] groups :when (< -1 (.indexOf v name))] k))
        heading (if (empty? heading) {"Other" []} heading)
        heading (key (first heading))]
    (get-accordian-item-element ".fb-popover-fields" heading name)))

;; Driver functions related to form builder toolbox

(defn toolbox-section-expanded? [section-name]
  (evaluate-angular-expression (str ".fb-toolbox-section:contains(" section-name ")") "group.isOpen"))

(defn expand-toolbox-section [section-name]
  (when (not (toolbox-section-expanded? section-name))
    (click (str ".fb-toolbox-section:contains(" section-name ") a[ng-click*=toggleOpen]"))))

(defn collapse-toolbox-section [section-name]
  (when (toolbox-section-expanded? section-name)
    (click (str ".fb-toolbox-section:contains(" section-name ") a[ng-click*=toggleOpen]"))))

(defn field-group-expanded? [group-name]
  (exists-present? (str ".fb-toolbox-group:contains(" group-name ") .fb-toolbox-toggle[style*=opened]")))

(defn expand-field-group [group-name]
  (when (not (field-group-expanded? group-name))
    (click (str ".fb-toolbox-group:contains(" group-name ") .fb-toolbox-toggle"))))

(defn collapse-field-group [group-name]
  (when (field-group-expanded? group-name)
    (click (str ".fb-toolbox-group:contains(" group-name ") .fb-toolbox-toggle"))))

(defn get-toolbox-field-container-element-str [field-name]
  (str ".fb-toolbox-item:contains(" field-name ")"))

(defn toolbox-field-exist? [field-name]
  (exists-present? (get-toolbox-field-container-element-str field-name)))

(defn- hover-over [e]
  (->actions *driver* (move-to-element e))
  e)

(defn system-field? [field-name]
  (hover-over (element (get-toolbox-field-container-element-str field-name)))
  (not (exists-present? (str (get-toolbox-field-container-element-str field-name) " .ui-icon-wrench"))))

(defn open-field-configure-dialog [field-name]
  (hover-over (element (get-toolbox-field-container-element-str field-name)))
  (click (str (get-toolbox-field-container-element-str field-name) " .ui-icon-wrench")))

(defn delete-field [field-name]
  (hover-over (element (get-toolbox-field-container-element-str field-name)))
  (click (str (get-toolbox-field-container-element-str field-name) " .ui-icon-close"))
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)"))

(defn get-field-group-element [group-name]
  (element (str ".fb-toolbox-group-drop:contains(\"" group-name "\")")))

(defn toolbox-field-group-exist? [group-name]
  (exists-present? (str ".fb-toolbox-group-drop:contains(\"" group-name "\")")))

(defn relationship-type? [field-type]
  (or (= field-type "Choice") (= field-type "Lookup") (= field-type "Relationship")))

;;todo: Need to delete this function Duplicated
(defn add-field-to-field-group [field-type group-name]
  (open-add-field-menu)
  (drag-n-drop (get-field-menu-item-element field-type) (get-field-group-element group-name))
  (close-add-field-menu))

(defn add-from-field-menu-to-field-group [field-type group-name]
  (open-add-field-menu)
  (drag-n-drop (get-field-menu-item-element field-type) (get-field-group-element group-name))
  (when-not (relationship-type? field-type) (close-add-field-menu)))

(defn add-new-field-group []
  (click "div[ng-click*=newFieldGroupClick]"))

(defn delete-field-group [group-name]
  (-> (element (str ".fb-toolbox-group:contains(" group-name ")"))
      (#(->actions *driver* (move-to-element %))))
  (click (str ".fb-toolbox-group:contains(" group-name ") [ng-click*=removeFieldGroup]")))

(defn get-toolbox-search-text []
  (value ".sp-search input"))

(defn set-toolbox-search-text [search-text]
  (clear ".sp-search input")
  (input-text ".sp-search input" search-text))

(defn clear-toolbox-search-text []
  (clear ".sp-search input"))

;; Get/set field name in toolbox.

(defn set-field-name [name new-name]
  (let [prefix ".fb-toolbox-group-drop .fb-editable-field-label"]
    ;; go into edit mode
    (click (str prefix " .editable-label-readonly-container:contains(\"" name "\")"))
    ;; set the value
    (let [e (first (filter #(= (attribute % "value") name)
                           (elements (str prefix " .editable-label-edit-container input"))))]
      (set-input-value e new-name))
    ;; leave edit mode
    (click ".fb-toolbox")))

(defn get-field-name [field-name]
  (text (get-toolbox-field-container-element-str field-name)))

(defn set-field-group-name [group-name value]
  (let [prefix ".fb-toolbox-group .fb-editable-definition-label"]
    ;; go into edit mode
    (click (str prefix " .editable-label-readonly-container:contains(" group-name ")"))
    ;; set the value
    (let [e (first (elements (str ".fb-toolbox-group:contains(" group-name ") .editable-label-edit-container input")))]
      ;; Enter the value and leave edit mode
      ;; We used to click 'elsewhere' to leave edit mode but that doesn't work in FF
      ;; so now we add ENTER, and since that often results in stale ref we catch and ignore
      (try (set-input-value e (str value Keys/ENTER))
           (catch Exception _)))))

(defn get-field-group-name [group-name]
  (text (str ".fb-toolbox-group:contains(" group-name ")")))

;; Driver functions for relationship viewer

(defn lookup-menu-opened? []
  ;;(evaluate-angular-expression ".fb-toolbox-control[uib-popover-template*=newRelationshipPopover]" "popover.isOpen")
  ;;the above no longer works with angular ui bootstrap, so we might need to find a better solution than that below
  (taxi/exists? ".fb-popover-fields"))

(defn open-add-lookup-menu []
  (when (not (lookup-menu-opened?))
    (click ".fb-toolbox-control[uib-popover-template*=newRelationshipPopover]")))

(defn close-add-lookup-menu []
  (when (lookup-menu-opened?)
    (click ".fb-toolbox-control[uib-popover-template*=newRelationshipPopover]")))

(defn lookup-selected? [lookup-name]
  (evaluate-angular-expression (str ".fb-popover-body div:contains(" lookup-name ") input") "lookup.selected"))

(defn check-lookup-from-lookup-menu [lookup-name]
  (when (not (lookup-selected? lookup-name))
    (click (str ".fb-popover-body div:contains(" lookup-name ")"))))

(defn uncheck-lookup-from-lookup-menu [lookup-name]
  (when (lookup-selected? lookup-name)
    (click (str ".fb-popover-body div:contains(" lookup-name ")"))))

(defn select-lookup-from-relationship-viewer [lookup-name]
  (click (str ".fb-toolbox-groupitems tr:contains(" lookup-name ")")))

(defn delete-lookup-from-relationship-viewer [lookup-name]
  (-> (str ".fb-toolbox-groupitems tr:contains(" lookup-name ")")
      (element)
      (#(do (->actions *driver* (move-to-element %)) %))
      )
  (click (str ".fb-toolbox-groupitems tr:contains(" lookup-name ") span[ng-click*=removeLookup]")))

;; Driver functions for form builder page

(defn get-default-form-drop-target []
  (last (elements ".sp-form-builder-container:not([field-container])")))

(defn is-container? [element]
  (not (evaluate-angular-expression element "fieldContainer")))

(defn get-container-elements []
  (filter #(is-container? %) (elements ".sp-form-builder-container-content .sp-form-builder-toolbar")))

(defn get-form-control-elements [control-name]
  (let [q (str ".sp-form-builder-container-content:contains(" control-name ")")]
    (wait-for-jq q 2000)
    (filter #(evaluate-angular-expression % "fieldContainer") (elements q))))

(defn get-container-header-elements []
  (elements ".sp-form-builder-container-content .sp-form-builder-container-header"))

(defn get-tab-container-elements []
  (elements ".sp-form-builder-container-content sp-tab-container-control"))

(defn get-tab-element [tab-container-index tab-index]
  (find-element-under (nth (get-tab-container-elements) tab-container-index) {:tag :li, :index tab-index}))

(defn set-form-control-name [control-name value]
  (let [elm (first (get-form-control-elements control-name))
        input (find-element-under elm {:tag :input, :css ".editable-label-edit"})]
    ;; go into edit mode
    (click (find-element-under elm {:tag :div, :css ".editable-label-readonly-container"}))
    ;; set the value
    (set-input-value input value)
    ;; leave edit mode
    ;;(click ".sp-Edit-Form-Heading")
    (taxi/send-keys input Keys/ENTER)))

(defn get-form-control-name [control-name]
  (text (first (get-form-control-elements control-name))))

(defn set-container-name [container-index value]
  (let [elm (nth (get-container-header-elements) container-index)
        input (find-element-under elm {:tag :input, :css ".editable-label-edit"})]
    ;; go into edit mode
    (click (find-element-under elm {:tag :div, :css ".editable-label-readonly-container"}))
    ;; set the value
    (set-input-value input value)
    ;; leave edit mode
    ;;(click elm)
    (taxi/send-keys input Keys/ENTER)))

(defn get-container-name [container-index]
  (text (nth (get-container-header-elements) container-index)))

(defn get-form-control-container-element-str [control-name]
  (str ".sp-form-builder-container.sp-form-builder-field-control:contains(\"" control-name "\")"))

(defn form-control-exist? [control-name]
  (exists-present? (get-form-control-container-element-str control-name)))

(defn open-form-control-configure-dialog [control-name]
  (click (str (get-form-control-container-element-str control-name) " [ng-click*=Config]"))
  (wait-for-angular))

(defn delete-form-control [control-name]
  (click (str (get-form-control-container-element-str control-name) " [ng-click*=onCloseClick]")))

(defn open-container-configure-dialog [container-index]
  (when (< container-index (count (get-container-elements)))
    (click (find-element-under (nth (get-container-elements) container-index) {:css "span[ng-click*=Config]"}))
    (wait-for-angular)))

(defn delete-container [container-index]
  (if (< container-index (count (get-container-elements)))
    (click (find-element-under (nth (get-container-elements) container-index) {:css "span[ng-click*=onCloseClick]"}))))


(defn add-tab-in-tab-container [tab-container-index]
  (if (< tab-container-index (count (get-tab-container-elements)))
    (click (find-element-under (nth (get-tab-container-elements) tab-container-index) {:css "span[ng-click*=addTab]"}))))

(defn get-tab-in-tab-container [tab-container-index tab-index]
  (text (get-tab-element tab-container-index tab-index)))

(defn select-tab-in-tab-container [tab-container-index tab-index]
  (click (get-tab-element tab-container-index tab-index)))

(defn open-tab-configure-dialog [tab-container-index tab-index]
  (when (< tab-container-index (count (get-tab-container-elements)))
    (click (find-element-under (get-tab-element tab-container-index tab-index) {:css "span[ng-click*=configureTab]"}))
    (wait-for-angular)))

(defn delete-tab-in-tab-container [tab-container-index tab-index]
  (if (< tab-container-index (count (get-tab-container-elements)))
    (click (find-element-under (get-tab-element tab-container-index tab-index) {:css "span[ng-click*=removeTab]"}))))


(defn add-from-field-menu-to-form [field-type]
  (open-add-field-menu)
  (drag-n-drop (get-field-menu-item-element field-type) (get-default-form-drop-target))
  (wait-for-angular)
  (when-not (relationship-type? field-type) (close-add-field-menu)))

;; Note: Here container-index means its consider both Container and Tab-container.
(defn add-from-field-menu-to-container [field-type container-index]
  (open-add-field-menu)
  (drag-n-drop (get-field-menu-item-element field-type) (nth (get-container-elements) container-index))
  (wait-for-angular)
  (when-not (relationship-type? field-type) (close-add-field-menu)))

(defn add-field-from-toolbox-to-form [field-name]
  (drag-n-drop (get-toolbox-field-container-element-str field-name) (get-default-form-drop-target)))

(defn add-field-group-from-toolbox-to-form [field-group-name]
  (drag-n-drop (get-field-group-element field-group-name) (get-default-form-drop-target)))

(defn add-display-option-from-toolbox-to-form [display-option-name]
  (expand-toolbox-section "Display")
  (drag-n-drop (first (elements (str ".fb-toolbox-item:contains(" display-option-name ")")))
               (element ".sp-form-builder-container-content:eq(0)")))

(defn add-field-from-toolbox-to-container [field-name container-index]
  (drag-n-drop (get-toolbox-field-container-element-str field-name) (nth (get-container-elements) container-index)))

(defn add-field-group-from-toolbox-to-container [field-group-name container-index]
  (drag-n-drop (get-field-group-element field-group-name) (nth (get-container-elements) container-index)))


(defn get-name []
  (text ".sp-Edit-Form-Heading"))

(defn set-name [value]
  (set-click-to-edit-value ".sp-Edit-Form-Heading" value))

(defn get-description []
  (text ".sp-Edit-Form-Description"))

(defn set-description [value]
  (set-click-to-edit-value ".sp-Edit-Form-Description" value))

(defn open-form-properties-dialog []
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip*=Properties]")
  (wait-for-angular))

(defn save []
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip*=Save]")
  (wait-for-angular)
  (wait-until #(not (navitem-isdirty?)) 60000 500))

(defn save-as [name]
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip*='Save As']")
  (set-input-value ".formSaveAsDialog input" name)
  (app/choose-modal-ok)
  (wait-for-angular)
  (when (rt.po.common/exists-present? ".formSaveAsDialog input")
    (throw (Exception. "Did not expect dialog to be present."))))

(defn close []
  (click "button[ng-click*='Cancel']")
  (wait-for-angular))

;;todo we cannot test the successful alert messages at the moment.Make this driver works in future.
(defn save-success? []
  )

(defn close-continue []
  (when (rt.po.common/exists-present? ".alert-danger")
    (click "button[test-id*=navContinue]")))

(defn close-cancel []
  (when (rt.po.common/exists-present? ".alert-danger")
    (click "button[test-id*=navCancel]")))


;; Old Driver functions, Probably will be not using anymore.

(defn get-container-element [container-label-or-index]
  ;;assume label for now
  {:pre [(string? container-label-or-index)]}
  ;; todo complete impl, both finding the correct container and supporting container index
  (first (elements ".sp-form-builder-container:not([field-container])")))

(defn set-field-control-attributes [field-name {:keys [display-name] :as attrs}]
  (when display-name (set-form-control-name field-name display-name)))

(defn set-field-attributes [field-name {:keys [name] :as attrs}]
  (when name (set-field-name field-name name)))

(defn get-choice-values
  "Get the current set of choice values in the currently open Choice field props dialog."
  []
  (execute-script-on-element
    "
    var choiceValues = angular.element(arguments[0]).scope().model.choiceValues;
    return _.map(choiceValues, 'name');
    "
    ".choicetable"))

(defn add-choice-value-in-choice-dialog [choice-value]
  ;; Note: clicking to edit, sending keys, and then ENTER, but it just wasn't reliable
  ;; so now we are hitting the Angular JS scope directly.

  (when-not ((set (get-choice-values)) choice-value)
    (let [jsbase "(model.choiceValues.length && model.choiceValues[model.choiceValues.length-1] || {})"]
      ;; add each new one (maybe not the best way, but is working)
      (click "button[ng-click*=addChoiceFieldValue]")
      (evaluate-angular-expression
        ".choicetable"
        (str jsbase ".name ='" choice-value "'")))))

(defn set-choice-values-in-choice-dialog [choice-values]
  ;; Note: clicking to edit, sending keys, and then ENTER, but it just wasn't reliable
  ;; so now we are hitting the Angular JS scope directly.

  ;; clear existing
  (evaluate-angular-expression ".choicetable" "clearExistingValues()")
  (doseq [choice-value choice-values]
    (add-choice-value-in-choice-dialog choice-value)))

(defn delete-choice-value-in-choice-dialog [choice-value]
  ;; should do this via the UI, but for now doing via ng scope
  (set-choice-values-in-choice-dialog (remove (set choice-value) (get-choice-values))))

(defn handle-choice-field-form
  "Do things in the choice field properties form."
  [& [{:keys [name display-name]}]]
  ;; assumes we are already in the form
  (let [name (or name "New Choice field")
        display-name (or display-name name)
        choice-value "Option 1"]

    ;; Set the name and display name
    ;; Both have same test-id... so until that is fixed in edit form...
    (set-input-value (first (elements (ef/string-field "Name"))) name)
    (when display-name (set-input-value (second (elements (ef/string-field "Name"))) display-name))

    ;; add some choice values
    (set-choice-values-in-choice-dialog [choice-value])

    ;; close the form
    (click-modal-dialog-button-and-wait ".modal-footer button[ng-click*='ok']")))

(defn set-lookup-entity-type [type-name]
  ;; assumes we are already in the form

  ;; open the entity type picker
  (let [q ".sp-relationship-properties-modal button[ng-click*=spEntityCompositePickerModal]"]
    (->actions *driver* (move-to-element (element q)))
    (click q))

  ;; filter to the desired type ... not necessary but doing it anyway
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" type-name)
  ;; choose the type
  (rv/select-row-by-text type-name ".entityReportPickerDialog .dataGrid-view")

  ;; ok the typepicker
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))

(defn handle-lookup-form []
  ;; assumes we are already in the form
  ;; todo - just default to the first type seen, and set the desired one later
  (set-lookup-entity-type "AA_Employee")
  (click-modal-dialog-button-and-wait ".sp-relationship-properties-modal .modal-footer button[ng-click*=ok]"))

(defn handle-relationship-form []
  ;; assumes we are already in the form
  ;; todo - just default to the first type seen, and set the desired one later
  (set-lookup-entity-type "AA_Drink")
  (click-modal-dialog-button-and-wait ".sp-relationship-properties-modal .modal-footer button[ng-click*=ok]"))

(defn add-container-to-form []
  (drag-n-drop (first (elements ".fb-toolbox-item:contains(Container)"))
               (element ".sp-form-builder-container-content:eq(0)")))

(defn add-tabbed-container-to-form []
  (drag-n-drop (first (elements ".fb-toolbox-item:contains(Tabbed Container)"))
               (element ".sp-form-builder-container-content:eq(0)")))

(defn add-field-to-form
  ;; Was having trouble getting more than one field onto the canvas until I included the :eq(0)
  ;; I think because the dnd sim server is automatically choosing the last element if more than
  ;; one is found, and while that made screen builder work, it doesn't work for form builder
  ;; TODO: improve the robustness of the dnd for fb and sb

  ([field-type]
   (add-field-to-form field-type (get-default-form-drop-target)))

  ([field-type drop-target]
   (open-add-field-menu)
   (drag-n-drop (get-field-menu-item-element field-type) drop-target)
   (wait-for-angular)
   (condp = field-type
     "Choice" (handle-choice-field-form)
     "Lookup" (handle-lookup-form)
     "Relationship" (handle-relationship-form)
     "Calculation" :default
     (close-add-field-menu))))

(defn add-field-to-container
  [field-type container-label-or-index]
  (add-field-to-form field-type (get-container-element container-label-or-index)))

(defn add-field-group-to-form [group-name]
  (drag-n-drop (get-field-group-element group-name) (get-default-form-drop-target)))

(defn get-field-container-element-str [field-name]
  (str ".sp-form-builder-container.sp-form-builder-field-control:contains(\"" field-name "\")"))

(defn show-field-properties-dialog [field-name]
  (let [q (str (get-field-container-element-str field-name) " [ng-click*=Config]")]
    (wait-for-jq q)
    (click q)
    (wait-for-angular)))

(defn wait-for-dialog-close
  ;; debugging
  ([] (wait-for-dialog-close 2500))
  ([msecs] (doseq [n (range 10)]
             (println "waiting.. modal backdrop" (count (elements ".modal-backdrop")))
             (Thread/sleep (/ msecs 10)))
    #_(wait-until #(not (exists-present? ".modal-backdrop")) msecs)))

(defn- get-dialog-control-elements []
  (get-repeated-elements ".modal-dialog sp-control-on-form"
                         {:label ".edit-form-title" :value ".edit-form-value"}))

(defn set-dialog-control-value [label value]
  (let [elem (->> (get-dialog-control-elements)
                  (filter #(re-find (re-pattern (str "(?i)^" label "[\\s:]*"))
                                    (taxi/text (:label %))))
                  first
                  :value)]
    (if-let [input-element (and elem (taxi/find-element-under elem {:css "input"}))]
      (rt.lib.wd/set-input-value input-element value)
      ;;else
      (throw (Exception. (str "Failed to find form control for:" label))))))

(defn run-field-properties-dialog [field-name {:keys [name, display-name] :as attrs}]

  (show-field-properties-dialog field-name)

  (when name (set-dialog-control-value "Name" name))
  (when display-name (set-dialog-control-value "Display Name" display-name))

  ;; TODO: support more of the props in this form

  (when (:choice-values attrs)
    (set-choice-values-in-choice-dialog (:choice-values attrs)))

  (when (:lookup-type attrs)
    (set-lookup-entity-type (:lookup-type attrs)))

  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)")
  (wait-for-dialog-close))

(comment
  ;; debugging issue with lookup form controls being confused with the form behind the dialog

  (count (elements ".modal-dialog .sp-relationship-properties-modal"))
  (taxi/flash (element ".modal-dialog .sp-relationship-properties-modal"))

  (->> (elements ".modal-dialog .sp-relationship-properties-modal sp-control-on-form .fieldControl")
       (map #(let [scope (rt.lib.wd-ng/execute-script-on-element
                           "var scope = angular.element(arguments[0]).scope();
                           //return { label: scope.labelValue, value: scope.model && scope.model.fieldValue || scope.value };
                           return scope.model && _.keys(scope.model) || _.keys(scope);
                           "
                           %)]
              (hash-map                                     ;:html (taxi/html %)
                :scope scope))))


  (->> (get-repeated-elements ".modal-dialog .sp-relationship-properties-modal sp-control-on-form"
                              {:label ".edit-form-title"
                               :input ".edit-form-value"
                               })
       (filter #(re-find #"^Name[\s:]*" (taxi/text (:label %))))
       first
       :input
       (#(taxi/find-element-under % {:css "input"}))
       (#(rt.lib.wd/set-input-value % "AA_Drink")))

  (set-dialog-control-value "Display Name" "hey")


  (taxi/flash (element ".modal-dialog .sp-relationship-properties-modal"))

  )

(defn run-lookup-properties-dialog [field-name {:keys [name, display-name] :as attrs}]

  (show-field-properties-dialog field-name)

  ;; we can use the ef functions below even though there are multiple 'Name' test-ids
  ;; - see comment in run-field-properties-dialog
  ;; BUT beware as we add setting other fields in this form

  (when name (set-dialog-control-value "Name" name))
  (when display-name (set-dialog-control-value "Display Name" display-name))

  (when (:lookup-type attrs)
    (set-lookup-entity-type (:lookup-type attrs)))

  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)")
  (wait-for-dialog-close))

(defn show-rel-properties-dialog [rel-name]
  ;; omg....
  (let [rel-elements (map (fn [e] {:e e :to-name (evaluate-angular-expression (:webelement e) "control.relationshipToRender.toName")})
                          (elements ".sp-form-builder-field-control [ng-click*=Config]"))
        e (filter #(= rel-name (:to-name %)) rel-elements)]
    (when (empty? e)
      (println "Failed to find" rel-name "in" (map :to-name rel-elements))
      (throw (Exception. (str "Failed to find relationship with To name " rel-name))))
    (click (:e (first e)))))

(defn run-rel-properties-dialog [rel-name {:keys [name, display-name] :as attrs}]

  (show-rel-properties-dialog rel-name)

  ;; TODO - refactor with run-lookup....

  (when name (set-dialog-control-value "Name" name))
  (when display-name (set-dialog-control-value "Display Name" display-name))

  (when (:lookup-type attrs)
    (set-lookup-entity-type (:lookup-type attrs)))

  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)")
  (wait-for-dialog-close))

(defn click-ok
  []
  (click-modal-dialog-button-and-wait ".modal-footer:last() button:contains(OK)")
  (wait-for-dialog-close))

(defn open-tab-content-configure-dialog [tab-container-index tab-index]
  (click (str ".sp-form-builder-container-content sp-tab-container-control:nth-child(" (inc tab-container-index) ") div#spTabSet ul.nav.nav-tabs li:nth-child(" (inc tab-index) ")"))
  (click (str ".sp-form-builder-container-content sp-tab-container-control:nth-child(" (inc tab-container-index) ") div#spTabSet div.tab-content div.tab-pane.active span.sp-form-builder-adornment[ng-click*='onConfigureClick']")))

(defn delete-fields-and-save [& field-names]
  (let [field-names (filter toolbox-field-exist? field-names)]
    (when (not-empty field-names)
      (doseq [field-name field-names]
        (delete-field field-name))
      (save))))

