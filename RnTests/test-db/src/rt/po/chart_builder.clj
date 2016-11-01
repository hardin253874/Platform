(ns rt.po.chart-builder
  (:require [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.po.report-view :refer [select-row-by-text]]
            [rt.po.chart-new :refer [get-chart-type set-chart-type]]
            [rt.po.common :as common :refer [exists-present? get-file-name find-field get-string set-string get-multiline set-multiline get-combo set-combo get-lookup set-lookup get-bool set-bool field-visible? click-link wait-until]]
            [rt.po.view-form :as vf :refer [select-form-tab]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [drag-n-drop set-click-to-edit-value]]
            [clj-webdriver.taxi :refer [text value exists? present? selected? select-by-text selected-options element input-text click elements attribute visible? clear]]))


;; TOOLBAR FUNCTIONS

(defn click-chart-toolbar [name]
  (click (str ".builder-ToolBar button[ng-click*=" name "]")))

(defn click-chart-toolbar-refresh []
  (click-chart-toolbar "refresh"))

(defn click-chart-toolbar-properties []
  (click-chart-toolbar "Props"))

(defn click-chart-toolbar-undo []
  (click-chart-toolbar "undo"))

(defn click-chart-toolbar-save []
  (click-chart-toolbar "Save"))

(defn click-chart-toolbar-close []
  (click-chart-toolbar "Close"))

(defn click-chart-toolbar-redo []
  (click-chart-toolbar "redo"))


;; SOURCE FUNCTIONS

(defn get-chart-report-name []
  (text ".chart-builder-toolbox a.report-name"))

(defn click-chart-report-name []
  (click ".chart-builder-toolbox .report-name")
  (wait-for-angular))

(defn click-chart-report-refresh []
  (click ".chart-builder-toolbox img[ng-click*=refreshSources]")
  (wait-for-angular))

(defn get-sources []
  "Get a list of source names."
  (->> (elements ".chart-builder-toolbox div[test-source]")
       (map #(attribute % "test-source"))))

(defn ^:private find-source [source]
  (str ".chart-builder-toolbox div[test-source=\"" source "\"]"))

(defn get-source-icon [source]
  (-> (str (find-source source) " .itemicon img")
      (attribute "src")
      (get-file-name)
      ))


;; SERIES FUNCTIONS

(defn find-series [name]
  (let [series-name (if (string? name) name (:series name))]
  (cond
    (= series-name nil) ".chart-series .panel:first-child"
    (= series-name "") ".chart-series .panel:first-child"
    (= series-name "First") ".chart-series .panel:first-child"
    (= series-name "Last") ".chart-series .panel:last-child"
    :else (str ".chart-series .panel[test-series*='" name "']")
    )))

(defn get-series-list []
  "Get a list of series names."
  (->> (elements ".chart-series .panel[test-series]")
       (map #(attribute % "test-series"))))

(defn add-series []
  (click ".chart-series button[ng-click*=addSeries]"))

(defn remove-series [name]
  (click (str (find-series name) " img[ng-click*=removeSeries]")))

(defn set-series-name [old-name new-name]
  (set-click-to-edit-value (find-series old-name) new-name))

(defn series-expanded? [series]
  (exists? (str (find-series series) " .ui-icon-triangle-1-s")))

(defn expand-series [series]
  (when (not (series-expanded? series))
    (click (str (find-series series) " .ui-icon-triangle-1-e"))
    (Thread/sleep 100)))                                    ;; wait for animate

(defn collapse-series [series]
  (when (series-expanded? series)
    (click (str (find-series series) " .ui-icon-triangle-1-s"))
    (Thread/sleep 100)))                                    ;; wait for animate


;; SERIES CHART-TYPE OPTIONS

(defn toggle-series-chart-type [series]
  (click (str (find-series series) " img[src*=chartType]")))

(defn series-chart-type-open? [series]
  (exists-present? (str (find-series series) " .charttype-dropdown .chart-types")))

(defn open-series-chart-type [series]
  (when (not (series-chart-type-open? series))
    (toggle-series-chart-type series)))

(defn close-series-chart-type []
  (click ".chart-builder-toolbox .builder-Header"))

(defn ^:private do-series-chart-type-task [series callback]
  ;; Do the callback task, but first open the dialog if necessary
  ;; and close it if we had to open it.
  (if (series-chart-type-open? series)
    (callback)
    (do
      (open-series-chart-type series)
      (let [result (callback)]
        (close-series-chart-type)
        result))))

(defn get-series-chart-type [series]
  (do-series-chart-type-task series #(get-chart-type)))

(defn set-series-chart-type [series value]
  (do-series-chart-type-task series #(set-chart-type value)))


;; SERIES AXIS-SHARING OPTIONS

(defn series-has-axis-options? [series]
  (let [q (str (find-series series) " .axis-sharing")]
    (try
      (wait-until #(exists-present? q))
      true
      (catch Exception e false))))

(defn get-share-axis [series axis-label]
  (selected? (str (find-series series) " input[ng-model*=" axis-label "]")))

(defn set-share-axis [series axis-label value]
  (when (not= (get-share-axis series axis-label) value)
    (click (str (find-series series) " input[ng-model*=" axis-label "]"))))

(defn get-share-primary [series]
  (get-share-axis series "primaryAxis"))

(defn get-share-values [series]
  (get-share-axis series "valueAxis"))

(defn set-share-primary [series value]
  (set-share-axis series "primaryAxis" value))

(defn set-share-values [series value]
  (set-share-axis series "valueAxis" value))



;; TARGETS FUNCTIONS

(defn get-targets [series]
  "Get a list of target names for the given series."
  (->> (elements (str (find-series series) " div[test-target]"))
       (filter #(visible? %))
       (map #(attribute % "test-target"))))

(defn find-target [target]
  "Pass a target name in the first series, or a {:series series :target target} structure. Returns a selector."
  (if (:target target)
    (str (find-series target) " div[test-target*='" (:target target) "']")
    (str (find-series "First") " div[test-target*='" target "']")
    ))

(defn target-has-properties? [target]
  (exists-present? (str (find-target target) " button[ng-click*=showProps]")))

(defn click-target-properties [target]
  (click (str (find-target target) " button[ng-click*=showProps]"))
  (wait-for-angular)
  )

(defn get-target-source [target]
  "Gets the name of the source currently assigned to a target."
  (-> (str (find-target target) " .itemname")
      (text)
      ))

(defn get-target-icon [target]
  "Gets the data-type icon for a chart target."
  (-> (str (find-target target) " .itemicon img")
      (attribute "src")
      (get-file-name)
      ))



;; TARGETS DROPDOWN FUNCTIONS

(defn find-target-dropdown [target]
  (str (find-target target) " .sourceicon"))

(defn target-has-dropdown? [target]
  (exists-present? (find-target-dropdown target)))

(defn target-dropdown-open? [target]
  (exists-present? (str (find-target target) " .dropdown-menu")))

(defn click-target-dropdown [target]
  (click (str (find-target target) " img.sourceicon")))

(defn close-target-dropdown []
  (when (exists-present? ".contextmenu-backdrop")
    (click ".contextmenu-backdrop")))

(defn open-target-dropdown [target]
  (when (not (target-dropdown-open? target))
    (close-target-dropdown)    ;; in case a different target dropdown is open
    (click-target-dropdown target)))

(defn do-target-dropdown-task [target callback]
  ;; Do the callback task, but first open the menu if necessary
  ;; and close it if we had to open it.
  (if (target-dropdown-open? target)
    (callback)
    (do
      (open-target-dropdown target)
      (let [result (callback)]
        (close-target-dropdown)
        result))))

(defn get-target-dropdown-list [target]
  "Get a list of dropdown items for a target names."
  (do-target-dropdown-task target
                           (fn []
                             (doall  ;; map the values immediately. We can't lazy eval the text mapping because the element disappears once the popup is closed.
                               (->> (elements ".dropdown-menu .menuItem span")
                                    (map #(text %)))
                               ))))

(defn get-target-aggregate [target]
  "Get a list of dropdown items for a target names."
  (do-target-dropdown-task target
    #(text ".dropdown-menu .menuItem:has(div[style*=menutick]) span")))

(defn set-target-aggregate [target value]
  "Get a list of dropdown items for a target names."
  (do-target-dropdown-task target
    #(click (str ".dropdown-menu .menuItem span:contains(" value ")"))))


;; DRAG-DROP
;; Use (drag-n-drop from to) with any combination of the following
;; E.g. (drag-n-drop (dd-source "Count") (dd-target series "Primary"))

(defn dd-source [source]
  "Drag-drop location for a source."
  (str (find-source source) " .itemname" ))

(defn dd-target [target]
  "Drag-drop location for a target."
  (str (find-target target) " .chartSource div" ))

(defn dd-series [series]
  "Drag-drop location for a series."
  (str (find-series series) " .panel-title div[sp-draggable]" ))

(defn dd-background []
  "Drag-drop location for a background."
  ".chart-view")

(defn drag-source-to-target [source target]
  "Drag a source and drop it on a target."
  (drag-n-drop (dd-source source) (dd-target target)))

(defn drag-target-to-target [from-target to-target]
  "Drag from one target to another targer."
  (drag-n-drop (dd-target from-target) (dd-target to-target)))

(defn drag-target-to-background [target]
  "Drag a target onto the background."
  (drag-n-drop (dd-target target) (dd-background)))

(defn drag-series-to-series [from-series to-series]
  "Drag a target onto the background."
  (drag-n-drop (dd-series from-series) (dd-series to-series)))


;; GENERAL FUNCTIONS

(defn save-chart []
  (click-chart-toolbar-save))

(defn close []
  (click-chart-toolbar-close))

(defn ^:private get-axis-limit [label]
  (let [f (find-field label)]
    (cond
      (selected? (str f " input[value=auto]")) "Auto"
      (selected? (str f " input[value=manual]")) (value (str f " input[type=text]"))
      :else "")
    ))

(defn ^:private set-axis-limit [label value]
  (let [f (find-field label)]
    (if (= value "Auto")
      (click (str f " input[value=auto]"))
      (do
        (click (str f " input[value=manual]"))
        (clear (str f " input[type=text]"))                 ;; will cause jump back to auto
        (click (str f " input[value=manual]"))
        (input-text (str f " input[type=text]") value)
        )
      )))

(defn get-maximum []
  (get-axis-limit "Maximum"))

(defn get-minimum []
  (get-axis-limit "Minimum"))

(defn set-maximum [value]
  (set-axis-limit "Maximum" value))

(defn set-minimum [value]
  (set-axis-limit "Minimum" value))


;; WRAPPERS AROUND COMMON
(defn click-ok [] (common/click-ok))
(defn click-cancel [] (common/click-cancel))

;; WRAPPED CHART PROPERTIES DIALOG
(defn get-name [] (common/get-string "Name"))
(defn set-name [value] (common/set-string "Name" value))
(defn get-description [] (common/get-multiline "Description"))
(defn set-description [value] (common/set-multiline "Description" value))
(defn get-chart-title [] (common/get-string "Chart title"))
(defn set-chart-title [value] (common/set-string "Chart title" value))
(defn get-alignment [] (common/get-combo "Alignment"))
(defn set-alignment [value] (common/set-combo "Alignment" value))
(defn get-report [] (common/get-lookup "Report"))

(defn get-applications []
  (common/options-expand)
  (vf/select-form-tab "Advanced")
  (common/get-lookup "Applications"))

(defn set-applications [value]
  (common/options-expand)
  (vf/select-form-tab "Advanced")
  (common/set-lookup "Applications" value))

(defn clear-applications []
  (common/options-expand)
  (vf/select-form-tab "Advanced")
  (common/clear-lookup "Applications"))

(defn get-icon []
  (common/options-expand)
  (vf/select-form-tab "Format")
  (common/get-lookup "Icon"))

(defn set-icon [value]
  (common/options-expand)
  (vf/select-form-tab "Format")
  (common/set-lookup "Icon" value))

(defn clear-icon []
  (common/options-expand)
  (vf/select-form-tab "Format")
  (common/clear-lookup "Icon"))

;; WRAPPED AXIS PROPERTIES DIALOG
(defn get-axis-label [] (common/get-string "Axis label"))
(defn set-axis-label [value] (common/set-string "Axis label" value))
(defn minimum-visible? [] (common/field-visible? "Minimum"))
(defn maximum-visible? [] (common/field-visible? "Maximum"))
(defn get-show-grid [] (common/get-bool "Show grid"))
(defn set-show-grid [value] (common/set-bool "Show grid" value))
(defn get-show-all-values [] (common/get-bool "Show all values"))
(defn set-show-all-values [value] (common/set-bool "Show all values" value))
(defn show-all-values-visible? [] (common/field-visible? "Show all values"))
(defn get-show-labels [] (common/get-combo "Show labels"))
(defn set-show-labels [value] (common/set-combo "Show labels" value))
(defn show-labels-visible? [] (common/field-visible? "Show labels"))
(defn get-stack [] (common/get-combo "Stack"))
(defn set-stack [value] (common/set-combo "Stack" value))
(defn stack-visible? [] (common/field-visible? "Stack"))
(defn get-axis-type [] (common/get-combo "Axis Type"))
(defn set-axis-type [value] (common/set-combo "Axis Type" value))
(defn axis-type-visible? [] (common/field-visible? "Axis Type"))

;; WRAPPED COLOR PROPERTIES DIALOG
(defn get-color [] (common/get-color "Colour"))
(defn set-color [value] (common/set-color "Colour" value))
(defn get-negatives [] (common/get-color "Negatives"))
(defn set-negatives [value] (common/set-color "Negatives" value))
(defn get-hide-legend [] (common/get-bool "Hide legend"))
(defn set-hide-legend [value] (common/set-bool "Hide legend" value))
(defn get-conditional-formatting [] (common/get-bool "Conditional formatting"))
(defn set-conditional-formatting [value] (common/set-bool "Conditional formatting" value))
(defn click-defaults [] (common/click-link "Defaults"))

;; WRAPPED SYMBOL PROPERTIES DIALOG
(defn get-marker [] (common/get-combo "Marker"))
(defn set-marker [value] (common/set-combo "Marker" value))
(defn get-size [] (common/get-combo "Size"))
(defn set-size [value] (common/set-combo "Size" value))
