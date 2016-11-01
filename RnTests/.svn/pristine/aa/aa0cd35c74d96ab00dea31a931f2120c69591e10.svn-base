(ns rt.po.form-builder-config
  (:require [rt.lib.wd :refer [right-click set-input-value double-click cancel-editable-edit-mode wait-for-jq]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression apply-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [drag-n-drop test-id-css]]
            [rt.po.edit-form :as ef]
            [rt.po.report-view :as rv]
            [clj-webdriver.taxi :refer [text attribute click send-keys elements element exists? displayed? find-elements-under selected? selected-options select-by-text clear input-text find-element value]]
            [clojure.string :as string]
			[rt.po.app :as app]
            [rt.po.common :refer [exists-present? set-search-text-input-value click-modal-dialog-button-and-wait wait-until]]
            [rt.po.common :as common]
            [clj-webdriver.taxi :as taxi]
            [clj-webdriver.core :refer [by-css]]))

;;helper functions

;; this function is to find fields in the properties dialog tab panes. for others try common/find-field
(defn find-field-in-tab-pane [label]
  (or (first
        (filter
          exists-present?
          (list
            (str ".tab-pane .tab-container div:has( > .edit-form-title:contains('" label "')) .edit-form-value")
            )
          ))
      (str ":contains('No row found for label " label "')"))) ;; encode warning message in a valid selector

;; BOOL FIELD FUNCTIONS for fields in properties dialog tab panes.
(defn get-bool-in-tab-pane [label]
  (selected? (str (find-field-in-tab-pane label) " input")))

(defn set-bool-in-tab-pane [label value]
  (when (not= (get-bool-in-tab-pane label) value)
    (click (str (find-field-in-tab-pane label) " input"))))


(defn get-lookup-field-element [options-name]
  (str "[options*=" options-name "] input"))

(defn get-lookup [options-name]
  (if (exists-present? (get-lookup-field-element options-name))
    (value (get-lookup-field-element options-name))))

(defn set-lookup [options-name value]
  ;; Open picker report
  (click (str "[options*=" options-name "] button[uib-popover=Edit]"))
  (wait-for-angular)
  ;; filter to the desired type ... not necessary but doing it anyway
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" value)
  (wait-for-angular)
  ;; choose the type
  (rv/select-row-by-text value ".entityReportPickerDialog .dataGrid-view")
  ;; ok the typepicker
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))

(defn clear-lookup [options-name]
  (click (str "[options*=" options-name "] button[uib-popover=Clear]")))

(defn set-radio-input [value]
  (when (not (empty? (find-element {:tag :input, :type :radio, :value value})))
    (click (find-element {:tag :input, :type :radio, :value value}))))

(defn find-field [label]
  (or (first
        (filter
          exists-present?
          (list
            (str ".control-configure-dialog .edit-form-control-container:has(.edit-form-title span:contains('" label "')) .edit-form-value")
            (str ".control-configure-dialog .edit-form-control-container:has(.edit-form-title label:contains('" label "')) .edit-form-value") ;; relationship properties dialog use label instead of span
            (str ".tab-container:has(.field-title:contains('" label ":')) :last-child") ;; for the misplaced hack in spEditFormsDialog
            )
          ))
      (str ":contains('No row found for label " label "')"))) ;; encode warning message in a valid selector


(defn get-string [label]
  (value (str (find-field label) " input")))

(defn set-string [label value]
  (clear (str (find-field label) " input"))
  (input-text (str (find-field label) " input") value))

(defn get-multiline [label]
  (value (str (find-field label) " textarea")))

(defn set-multiline [label value]
  (clear (str (find-field label) " textarea"))
  (input-text (str (find-field label) " textarea") value))

(defn get-bool [model-name]
  (selected? (str "[ng-model*=" model-name "]")))

(defn set-bool [model-name value]
  (when (not= (get-bool model-name) value)
    (click (str "[ng-model*=" model-name "]"))))

(defn get-bool-today []
  (selected? (str "[ng-model*=useToday]")))

(defn set-bool-today [value]
  (when (not= (get-bool-today) value)
    (click (str "[ng-model*=useToday]"))))

(defn get-bool-now []
  (selected? (str "[ng-model*=useNow]")))

(defn set-bool-now [value]
  (when (not= (get-bool-now) value)
    (click (str "[ng-model*=useNow]"))))

(defn get-combo [model-name]
  (text (first (selected-options (str "[ng-model*=" model-name "]")))))

(defn set-combo [model-name value]
  (select-by-text (str "[ng-model*=" model-name "]") value))

;; general features on configure dialogs
(defn config-dialog-visible? []
  (exists-present? ".control-configure-dialog"))

;; Expand/Collapse sections
(defn section-expanded? [section-name]
  (exists-present? (str ".modal-dialog:last() .option:contains(" section-name ") img[src*='up.png']")))

(defn expand-section [section-name]
  (println "section" section-name "html=" (taxi/html (str ".modal-dialog:last() .option:contains(" section-name ")")))
  (println "before: section" section-name "expanded?" (section-expanded? section-name))
  (when (not (section-expanded? section-name))
    (println "expanding section" section-name)
    (click (str ".modal-dialog:last() .option:contains(" section-name ")"))
    (wait-until #(section-expanded? section-name))
    (Thread/sleep 500))
  (println "after: section" section-name "expanded?" (section-expanded? section-name)))

(defn collapse-section [section-name]
  (when (section-expanded? section-name)
    (click (str ".modal-dialog:last() .option:contains(" section-name ")"))
    (wait-until #(not (section-expanded? section-name)))
    (Thread/sleep 500)))

(defn select-tab [tab-name]
  (click (str ".modal-dialog:last() .nav-tabs li[heading='" tab-name "'] a")))

(defn click-ok []
  (rt.po.common/click-ok))

(defn ok-disabled? []
  (common/exists-present? (str (common/get-button-selector "OK") "[disabled='disabled']")))

(defn click-cancel []
  (rt.po.common/click-cancel))

(defn has-error-indicator? []
  (> (count (->> (elements "img[alt*='Error on field']")
                 (filter #(exists-present? %)))) 0))

(defn has-validation-error? []
  (exists-present? ".has-error"))

(defn has-error? []
  (or (has-error-indicator?) (has-validation-error?)))

;; Driver functions for field properties

(defn get-name []
  (get-string "Name"))

(defn set-name [value]
  (set-string "Name" value))

(defn get-description []
  (get-multiline "Description"))

(defn set-description [value]
  (set-multiline "Description" value))

(defn get-calculation []
  (evaluate-angular-expression
    ".calcprop-calculation .expression-editor-control"
    "model.calc.script"))

(defn set-calculation [value]
  (apply-angular-expression
    ".calcprop-calculation .expression-editor-control"
    (str "model.calc.script = \"" value "\"")))

(defn calculation-visible? []
  (common/exists-present? ".calcprop-calculation"))

(defn calculation-read-only? []
  (common/exists-present? ".calcprop-calculation .expression-editor-control.disabled"))

(defn get-calculation-field-type []
  (get-combo "resultType"))

(defn set-calculation-field-type [value]
  (set-combo "resultType" value))

(defn calculation-field-type-visible? []
  (common/exists-present? ".calcprop-field-type"))

(defn calculation-field-type-read-only? []
  (common/exists-present? ".calcprop-field-type select[disabled='disabled']"))

(defn get-calculation-error []
  (if (rt.po.common/exists-present? ".expression-editor-control .error")
    (text ".expression-editor-control .error")
    nil))
  
(defn get-field-mandatory []
  (get-bool "isFieldRequired"))

(defn set-field-mandatory [value]
  (set-bool "isFieldRequired" value))


(defn get-relationship-mandatory []
  (get-bool "relationshipIsMandatory"))

(defn set-relationship-mandatory [value]
  (set-bool "relationshipIsMandatory" value))

(defn set-field-value-for-field-type [field-type name value]
  (condp = field-type
    "String" (ef/set-string-field-value name value)
    "Number" (ef/set-number-field-value name value)
    "Date" (ef/set-date-field-value name value)
    "Time" (ef/set-time-field-value name value)
    "DateTime" (ef/set-date-field-value name value)
    (ef/set-string-field-value name value)))

(defn get-field-minimum-value []
  (get-string "Minimum"))

(defn set-field-minimum-value [value]
  (set-string "Minimum" value))

(defn get-field-maximum-value []
  (get-string "Maximum"))

(defn set-field-maximum-value [value]
  (set-string "Maximum" value))

(defn get-field-default-value []
  (get-string "Default"))

(defn set-field-default-value [value]
  (set-string "Default" value))

(defn get-script-name []
  (get-string "Script Name"))

(defn set-script-name [value]
  (set-string "Script Name" value))

(defn set-bool-field-default-value [value]
  (set-bool-in-tab-pane "Default" value))

(defn get-multiline-field-default-value []
  (get-multiline "Default"))

(defn set-multiline-field-default-value [value]
  (set-multiline "Default" value))

(defn get-text-pattern []
  (get-combo "pattern"))

(defn set-text-pattern [value]
  (set-combo "pattern" value))

(defn clear-text-pattern []
  (select-by-text (str "[ng-model*=pattern]") "[Select]"))

(defn get-decimal-places []
  (get-string "Decimal"))

(defn set-decimal-places [value]
  (set-string "Decimal" value))

;;Driver function related to autonumber field properties.

(defn get-autonumber-starting-number []
  (get-string "Starting"))

(defn set-autonumber-starting-number [value]
  (set-string "Starting" value))

(defn get-autonumber-pattern []
  (get-string "Pattern"))

(defn set-autonumber-pattern [value]
  (set-string "Pattern" value))

;; Driver functions related to choice field properties

(defn get-choice-field-option []
  (if (selected? (first (elements (str "input[type=radio]")))) "New" "Use Existing")
  )

(defn set-choice-field-option [option-value]
  (if (= option-value "New")
    (click (first (elements (str "input[type=radio]"))))
    (click (last (elements (str "input[type=radio]")))))

  )

(defn get-choice-field-lookup []
  (get-lookup "choiceValuePickerOptions"))

(defn set-choice-field-lookup [value]
  (set-lookup "choiceValuePickerOptions" value))

(defn clear-choice-field-lookup []
  (clear-lookup "choiceValuePickerOptions"))

(defn get-choice-value-row [choice-value]
  (find-element {:tag :span, :text choice-value}))

(defn edit-choice-value-row [choice-value name]
  (double-click (get-choice-value-row choice-value))
  (clear ".choicetable [ng-edit-cell-if=\"isFocused && canEditChoiceValueCell(row)\"] input")
  (input-text ".choicetable [ng-edit-cell-if=\"isFocused && canEditChoiceValueCell(row)\"] input" name)
  (click (first (elements ".choicetable"))))

(defn add-new-choice-value []
  (when (not (evaluate-angular-expression ".choicetable" "isNewButtonDisabled()"))
    (click "button[ng-click*=addChoiceFieldValue]")))

(defn get-choice-value [choice-value]
  (text (get-choice-value-row choice-value)))

(defn set-choice-value [choice-value name]
  (if (empty? (get-choice-value-row name))
    (edit-choice-value-row choice-value name)))

(defn move-choice-value-up [choice-value]
  (click (get-choice-value-row choice-value))
  (when (not (evaluate-angular-expression ".choicetable" "isUpButtonDisabled()"))
    (click "button[ng-click*=moveUpChoiceFieldValue]")))

(defn move-choice-value-down [choice-value]
  (click (get-choice-value-row choice-value))
  (when (not (evaluate-angular-expression ".choicetable" "isDownButtonDisabled()"))
    (click "button[ng-click*=moveDownChoiceFieldValue]")))

(defn delete-choice-value [choice-value]
  (click (get-choice-value-row choice-value))
  (when (not (evaluate-angular-expression ".choicetable" "isDeleteButtonDisabled()"))
    (click "button[ng-click*=deleteChoiceFieldValue]")))

(defn get-choice-field-type []
  (text (first (selected-options (str "[ng-model*=selectedType]")))))

(defn set-choice-field-type [type]
  (select-by-text (str "[ng-model*=selectedType]") type))

(defn get-choice-field-default-value []
  (text (first (selected-options (str "[ng-model*=toTypeDefaultValue]")))))

(defn set-choice-field-default-value [value]
  (select-by-text (str "[ng-model*=toTypeDefaultValue]") value))

(defn clear-choice-field-default-value []
  (select-by-text (str "[ng-model*=toTypeDefaultValue]") "[Select]"))

;;Driver functions related to relationship/lookup properties.

(defn get-relationship-mandatory []
  (get-bool "relationshipIsMandatory"))

(defn set-relationship-mandatory [value]
  (set-bool "relationshipIsMandatory" value))

(defn get-object []
  (get-lookup "objectTypePickerOptions"))

(defn set-object [value]
  (set-lookup "objectTypePickerOptions" value))

(defn clear-object []
  (clear-lookup "objectTypePickerOptions"))

(defn get-relationship-type []
  (value (first (filter selected? (elements ".relationshiptype-header input")))))

(defn set-relationship-type [value]
  (set-radio-input value))

(defn get-ownership []
  (value (first (filter selected? (elements ".ownershiptype-header input")))))

(defn set-ownership [value]
  (set-radio-input value))

(defn get-relationship-default-value []
  (get-lookup "defaultValuePickerOptions"))

(defn set-relationship-default-value [value]
  (set-lookup "defaultValuePickerOptions" value))

(defn clear-relationship-default-value []
  (clear-lookup "defaultValuePickerOptions"))

(defn get-relationship-name []
  (get-string "Relationship Name"))

(defn set-relationship-name [value]
  (set-string "Relationship Name" value))

(defn get-reverse-name []
  (get-string "Reverse Name"))

(defn set-reverse-name [value]
  (set-string "Reverse Name" value))

(defn get-hide-in-reverse []
  (selected? (str "[ng-model*=hideOnToType]")))

(defn set-hide-in-reverse [value]
  (when (not= (get-hide-in-reverse) value)
    (click (str "[ng-model*=hideOnToType]"))))

;; Driver functions for form control properties.

(defn get-display-name []
  (get-string "Display Name"))

(defn set-display-name [value]
  (set-string "Display Name" value))

(defn get-control-mandatory []
  (get-bool "mandatoryControl"))

(defn set-control-mandatory [value]
  (set-bool "mandatoryControl" value))

(defn get-control-readonly []
  (get-bool "readOnlyControl"))

(defn set-control-readonly [value]
  (set-bool "readOnlyControl" value))

(defn get-hide-label []
  (get-bool "hideLabel"))

(defn set-hide-label [value]
  (set-bool "hideLabel" value))

(defn get-background-color []
  (text ".dropdownSelectedItemName"))

(defn set-background-color [color-name]
  (click "div.spColorPicker-view .dropdownIcon")
  (wait-until #(exists? "ul.colorPickerDropdownMenu:visible"))
  (let [elements (elements (str "ul.colorPickerDropdownMenu:visible a.dropdownMenuItem:has(span:contains('" color-name "'))"))
        element (first (filter #(= (text %) color-name) elements))]
    (click element)))

(defn get-horizontal-resize []
  (get-combo "selectedHorizontalMode"))

(defn set-horizontal-resize [value]
  ;; (when (not (evaluate-angular-expression  "select[ng-selected*=-selectedHorizontalMode]" "resizeOptions.isHresizeModeDisabled")))
  (set-combo "selectedHorizontalMode" value))

(defn get-vertical-resize []
  (get-combo "selectedVerticalMode"))

(defn set-vertical-resize [value]
  ;;(when (not (evaluate-angular-expression  "select[ng-selected*=-selectedVerticalMode]" "resizeOptions.isVresizeModeDisabled")))
  (set-combo "selectedVerticalMode" value))

;; Driver functions for image control.

(defn get-thumbnail-size []
  (get-combo "thumbnailSizeSetting"))

(defn set-thumbnail-size [value]
  (set-combo "thumbnailSizeSetting" value))

(defn get-thumbnail-scaling []
  (attribute (first (filter selected? (elements "[ng-repeat*=thumbNailScaling] input"))) "text"))

(defn set-thumbnail-scaling [value]
  (click (first (elements (str "[ng-repeat*=thumbNailScaling]:contains(" value ") input")))))

;; Driver functions for lookup/relationship control.

(defn get-display-as []
  (get-combo "selectedDisplayAs"))

(defn set-display-as [value]
  (set-combo "selectedDisplayAs" value))

(defn get-display-report []
  (get-combo "selectedDisplayReport"))

(defn set-display-report [value]
  (set-combo "selectedDisplayReport" value))

(defn get-picker-report []
  (get-combo "selectedPickerReport"))

(defn set-picker-report [value]
  (set-combo "selectedPickerReport" value))

(defn get-display-form []
  (get-combo "selectedConsoleForm"))

(defn set-display-form [value]
  (set-combo "selectedConsoleForm" value))

(defn get-enable-new []
  (get-combo "selectedCanCreate"))

(defn set-enable-new [value]
  (set-combo "selectedCanCreate" value))

; Form action drivers

(defn actions-popup-launcher-button-exists? []
 (exists? (str ".sp-Form-Builder-Action button[ng-click='showActionsDialog()'] :visible")))

(defn click-actions-popup-launcher-button []
 (click (str ".sp-Form-Builder-Action button[ng-click='showActionsDialog()'] :visible")))

(defn action-button-exists? [label]
 (exists? (str ".sp-Form-Builder-Action button[title='" label "']")))

 ; internal
 (defn get-action-enabled-checkbox [label]
   (element (str ".ngRow:contains('" label "') input[type='checkbox']")))
 
 ; action exists in config dialog?
 (defn action-exists? [label]
   (exists?(get-action-enabled-checkbox label))
  )
 
 ; returns true or false
 (defn get-action-enabled [label]
   (selected?(get-action-enabled-checkbox label))
  )

 ; checks or unchecks the enable checkbox for a given action
 (defn set-action-enabled [label value]  
  (when (not= (get-action-enabled label) value)    
   (click (get-action-enabled-checkbox label)))
    nil)
 


; Security related drivers

(defn set-secures-from
  [value]
  (let [isChecked (exists? ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesFrom']:checked")]
    (when (not (= value isChecked)) (click ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesFrom']"))))

(defn set-secures-to
  [value]
  (let [isChecked (exists? ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesTo']:checked")]
    (when (not (= value isChecked)) (click ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesTo']"))))

(defn get-secures-from
  []
  (selected? ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesFrom']"))

(defn get-secures-to
  []
  (selected? ".sp-relationship-properties-modal:last() table.security-table div.security-option :input.security-checkbox[ng-model='model.relationshipToRender.securesTo']"))

(defn click-show-properties-in-reverse-direction-link
  []
  (click ".sp-relationship-properties-modal a:contains('Show Properties in Reverse Direction')")
  (wait-for-angular)
  (Thread/sleep 500))