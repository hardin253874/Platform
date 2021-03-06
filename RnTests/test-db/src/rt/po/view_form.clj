(ns rt.po.view-form
  (:require [rt.lib.wd :refer [right-click wait-for-jq wait-until-displayed set-input-value]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression execute-script-on-element]]
            [rt.po.app :as app :refer [make-app-url choose-context-menu enable-config-mode]]
            [rt.po.report-view :as rv]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.common :as common :refer [exists-present? wait-until]]
            [clj-webdriver.taxi :as taxi :refer [to exists? present? text attribute click element elements
                                                 find-element-under find-elements-under]]
            [clj-webdriver.core :refer [by-css by-xpath]]
            [clj-time.core :as t]
            [clj-time.format :as tf]
            [clj-time.local :as tl]
            [clojure.string :refer [trim]]
            [clj-time.coerce :as tc]))


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; TODO the following to be moved to a report namespace
;; for functions dealing with a report whether in
;; the report view, or on a form or screen

(defn open-action-menu
  "Open the action menu of a relationship control."
  ([] (open-action-menu nil))
  ([parent-elem]
   (let [parent-elem (taxi/element (or parent-elem "body"))
         menu-items (find-elements-under parent-elem {:css ".spreport-view ul.action-view"})
         open-button (find-element-under parent-elem {:css ".spreport-view button[title=Action]"})]
     (when-not (exists? (first menu-items))
       (click open-button)
       (wait-for-angular)))))

(defn open-action-menu-for-report [report-title]
	(if-let [btn-ele  (str ".report-render-control:has(.form-title:contains(\"" report-title "\")) button[title='Action']")]
    (do
       (click btn-ele)
       (wait-for-angular))
   ))

(defn get-report-control-search-box [title]
(str ".report-render-control:has(.form-title:contains(\"" title "\")) .sp-search-control-input"))

(defn set-search-text-for-report [title text]
  (if-let [search-ele  (get-report-control-search-box title)]
    (do
       (set-input-value search-ele text)
       (wait-for-angular))
   ))

(defn open-form-builder
  "Open the form builder on the current form via the configure icon."
  []
  (enable-config-mode)
  (click ".editForm-config-panel [src*=configure]")
  (choose-context-menu "Modify"))

(defn select-form-tab
  "Select the tab with the given heading text.
  Note that this works in view or edit mode."
  [name]
  (let [selector (str ".nav-tabs [heading='" name "'] a")]
    (wait-until-displayed selector)
    (click selector)
    (wait-for-angular)))

(defn form-tab-grid-query []
  ".tab-pane.active .dataGrid-view")

(defn get-tab-grid-values []
  (rv/get-loaded-grid-values (form-tab-grid-query)))

(defn form-nav-back []
  (rt.lib.wd/debug-click "button:contains('Back')")
  (wait-for-angular))

(defn form-record-name []
  (text ".form-title"))

(defn form-selected-tab-name []
  (text ".tabHeading.active"))

(defn open-form-lookup
  "Open the lookup record with the given name."
  [name]
  (click (first (find-elements-under
                  (str ".edit-form-control-container:contains('" name "')")
                  (by-css ".edit-form-value a"))))
  (wait-for-angular))

(defn to-view-form
  "Open the given record in view mode. Uses the given form if specified otherwis it uses the default"
  [id & [formId]]
  (to (make-app-url (str "/" id "/viewForm?formId=" (or formId "") "&test=true")))
  (wait-for-angular))

(defn string-field-value [name]
  (trim (text (str (test-id-css name) " [ng-class=fieldDisplayValue]"))))

(defn multiline-field-value [name]
  (trim (text (str (test-id-css name) " .multiLineTextField-text-read"))))

(defn number-field-value
  "Return value of the given numeric field.
  FIXME - be more locale aware.. at the moment it is stripping commas."
  [name]
  (read-string (clojure.string/replace (text (str (test-id-css name) " .fieldValue")) "," "")))

(defn field-exists? [id]
  (exists? (test-id-css id)))

(defn date-field-value
  "Return value of the given date field as a date in local time."
  [name]
  ;; We are directly getting from the angular model so we can get a
  ;; date object and convert to a standard format.
  (->> (rt.lib.wd-ng/execute-script-on-element
         "
         var scope = angular.element(arguments[0]).scope();
         console.log(scope.model.value);
         return JSON.stringify(scope.model.value);
         "
         (taxi/element (str (test-id-css name) " sp-date-control")))
       (clojure.data.json/read-json)
       (tf/parse (tf/formatters :date-time))
       (#(t/to-time-zone % (t/default-time-zone)))
       (tc/to-local-date-time)
       (tc/to-date)))

(defn choice-field-value
  "Return value of the given choice field."
  [name]
  (text (str (test-id-css name) " .fieldValue")))

(defn click-edit []
  (click (str "button[test-id='edit']")))

(defn delete-related-entity-via-tab [tab record]
  (select-form-tab tab)
  (get-tab-grid-values)
  (rv/set-search-text record)
  (rv/right-click-row-by-text record)
  (app/choose-context-menu "Delete")
  (app/choose-modal-ok))

;; lookup functions
(defn lookup-link-exists? [label]
  (exists? (str ".relControl:contains('" label "') a[ng-click*=handleLinkClick]")))

(defn click-lookup-link [label]
  (click (str ".relControl:contains('" label "') a[ng-click*=handleLinkClick]"))
  (wait-for-angular))

(defn get-lookup-link [label]
  (if (lookup-link-exists? label)
    (text (str ".relControl:contains('" label "') a[ng-click*=handleLinkClick]"))))

(defn multi-lookup-link-value-exists? [label value]
  (exists? (str ".relControl:contains('" label "') a[ng-click*=handleLinkClick]:contains('" value "')")))

(defn click-multi-lookup-link-value [label value]
  (click (str ".relControl:contains('" label "') a[ng-click*=handleLinkClick]:contains('" value "')"))
  (wait-for-angular))

(defn get-count-lookup-link-values [label]
  (count (elements  (str ".multilineRel:contains('" label "') a[ng-click*=handleLinkClick]"))))

(defn multi-lookup-link-expander-button-exists? [label]
  (exists? (str ".multilineRel:contains('" label "') button[ng-click*=spExpander_modal]")))

(defn click-multi-lookup-link-expander-button [label]
  (click (str ".multilineRel:contains('" label "') button[ng-click*=spExpander_modal]"))
  (wait-for-angular))

(defn multi-lookup-expander-dialog-link-value-exists? [value]
  (exists? (str ".expanderDialog a[ng-click*=linkClicked]:contains('" value "')")))

(defn click-multi-lookup-expander-dialog-link-value [value]
  (click (str ".expanderDialog a[ng-click*=linkClicked]:contains('" value "')"))
  (wait-for-angular))

(defn get-count-expander-dialog-lookup-link-values []
  (count (elements  (str ".expanderDialog a[ng-click*=linkClicked]"))))

(defn close-multi-lookup-expander-dialog []
  (click (str ".expanderDialog button[ng-click*=closeDetail]"))
  (wait-for-angular))

;; structure view functions

(defn find-structure-view-item [label]
  (str ".entityCompositePicker span:contains('" label "')"))

(defn structure-view-item-exists? [label]
  (exists? (str ".entityCompositePicker span:contains('" label "')")))

(defn structure-view-search-matched-item-exists? [label]
  (exists? (str ".entityCompositePicker span[class*='matched']:contains('" label "')")))

(defn get-count-all-search-matched-structure-view-items []
  (count(elements (str ".entityCompositePicker span[class*='matched']"))))

(defn get-count-search-matched-structure-view-items [label]
  (count(elements (str ".entityCompositePicker span[class*='matched']:contains('" label "')"))))

(defn get-count-structure-view-items [label]
  (count(elements (str ".entityCompositePicker span:contains('" label "')"))))

(defn get-count-all-structure-view-items []
  (count (elements ".entityCompositePicker div[class*='structureItem']")))

(defn get-count-all-selected-structure-view-items []
 (count (elements (str ".entityCompositePicker div[class='structureItem_selected']"))))

(defn is-structure-view-item-selected? [label]
 (exists? (str ".entityCompositePicker div[class='structureItem_selected'] span:contains('" label "')")))

(defn select-structure-view-item [label]
  (if-not (is-structure-view-item-selected? label)
    (if-let [element (find-structure-view-item label)]
     (click element))))

(defn collapse-structure-view-item-node [label]
  (if-let [element (str ".entityCompositePicker div[class*='structureItem']:contains('" label "') img[src*='hierarchy_opened']")]
     (click element))
  )

(defn expand-structure-view-item-node [label]
  (if-let [element (str ".entityCompositePicker div[class*='structureItem']:contains('" label "') img[src*='hierarchy_collapsed']")]
     (click element))
  )

(defn is-structure-view-item-opened? [label]
  (exists? (str ".entityCompositePicker div[class*='structureItem']:contains('" label "') img[src*='hierarchy_opened']"))
  )

(defn is-structure-view-item-collapsed? [label]
  (exists? (str ".entityCompositePicker div[class*='structureItem']:contains('" label "') img[src*='hierarchy_collapsed']"))
  )

(defn set-structure-view-search [text]
  (if-let [el (str ".entityCompositePicker input[class*='sp-search-control-input']")]
    (do
      (set-input-value el text)
      (wait-for-angular)
      (Thread/sleep 500)
      (wait-for-angular))
    (throw (Exception. "Cannot find displayed search box"))))


(defn select-structure-view-item-with-ctrl-key-by-text
  "Selects the specified row with the ctrl key down by text. This adds the specified element
  to the list of selected elements."
  ([text locator]
   (wait-for-angular)
   (rv/select-row-with-ctrl-key (element (find-structure-view-item text))))
  ([text]
   (select-structure-view-item-with-ctrl-key-by-text "")))


(defn open-tab-action-menu
  ([tab-name menu-item-name] (open-tab-action-menu 0 tab-name menu-item-name))
  ([tabs-index tab-name menu-item-name]
   (select-form-tab tab-name)
   (let [grid-selector (str ".tab-pane.active:nth(" tabs-index ")")]
     (wait-for-jq grid-selector)
     (rt.po.report-view/open-action-menu grid-selector))
   (app/choose-context-menu menu-item-name)
   (wait-for-angular)))

(defn open-tab-new-menu
  ([tab-name menu-item-name] (open-tab-new-menu 0 tab-name menu-item-name))
  ([tabs-index tab-name menu-item-name]
   (select-form-tab tab-name)
   (let [grid-selector (str ".tab-pane.active:nth(" tabs-index ")")]
     (wait-for-jq grid-selector)
     (rt.po.report-view/open-new-menu grid-selector))
   (app/choose-context-menu menu-item-name)))

(defn get-task-name
  "Get the name of the first active task for the current record.
  Returns empty if none."
  []
  (let [task-name-el (element ".sp-applicable-tasks .task-name")]
    (if (taxi/displayed? task-name-el)
      (taxi/text task-name-el)
      (taxi/text (first (taxi/selected-options ".sp-applicable-tasks select.task-list"))))))

(defn get-task-names
  "Get a list of the task names."
  []
  (try
    (wait-for-jq ".sp-applicable-tasks .sp-task-buttons")
    (rt.lib.wd-ng/execute-script-on-element
      "
      var scope = angular.element(arguments[0]).scope();
      return _.map(scope.taskList, function (r) { return r.name; });
      "
      ".sp-applicable-tasks")

    (catch org.openqa.selenium.TimeoutException _ [])))

(defn get-task-actions
  "Get a list of the available actions for the active task of the current record."
  []
  (try
    (wait-for-jq ".sp-applicable-tasks .sp-task-buttons")
    (wait-until #(> (count (taxi/elements ".sp-applicable-tasks .sp-task-buttons button:visible")) 0))
    (map taxi/text (taxi/elements ".sp-applicable-tasks .sp-task-buttons button"))

    (catch org.openqa.selenium.TimeoutException _ [])))

(defn choose-task-action
  "Choose the given action of the active task of the current record."
  [action-name]
  (let [buttons (taxi/elements (str ".sp-applicable-tasks .sp-task-buttons button:contains('" action-name "')"))]
    (when (empty? buttons)
      (throw (Exception. (str "Failed to find action button: " action-name))))
    (taxi/click (first buttons))))

(defn click-task-action-v2
  "Choose the given action of the active task of the current record."
  [action-name]
  (let [buttons (taxi/elements (str ".editForm-Action button:contains('" action-name "')"))]
    (when (empty? buttons)
      (throw (Exception. (str "Failed to find action button: " action-name))))
    (taxi/click (first buttons))))

(defn set-task-comment [s]
  (set-input-value ".sp-applicable-tasks input[placeholder=Comment]" s))

;;Driver functions form action

(defn action-button-on-form-exists? [label]
 (exists? (str ".editForm-Action button[title='" label "'] :visible")))

 (defn click-action-button-on-form [label]
 (click (str ".editForm-Action button[title='" label "']")))

;;Driver functions with out using test-id

(defn get-field-value [field-name]
  (let [elements (elements ".edit-form-control-container")
        element (first (filter #(.contains (text %) field-name) elements))]
    (trim (text (find-element-under element {:tag :div, :class "edit-form-value"})))))

(defn get-report-row-count
  ([expected-count]
    (if (>= expected-count 0)
      (rt.po.common/wait-until #(= (get-report-row-count -1) expected-count) 5000))
    (count (filter #(present? %) (elements ".ngRow"))))
  ([]
    (get-report-row-count -1)))

(defn expect-report-row-count [expected-count]
  (expect-equals expected-count (get-report-row-count expected-count)))

(defn remove-selected-report-item []
  (click (first (filter #(present? %) (elements "button[title='Remove Link']")))))

(defn- get-containers []
  (->> (taxi/elements "sp-vertical-stack-container-control,sp-horizontal-stack-container-control")
       (map #(hash-map :container %
                       :header-elem (first (taxi/find-elements-under
                                             %
                                             {:css ".structure-control-header"}))))
       ;; strip missing webelements... have seen this and it throws if not handled
       (filter (comp not nil? :webelement :header-elem))
       (map #(assoc % :header-text (taxi/text (:header-elem %))))))

(defn- get-container-element [title]
  (->> (get-containers)
       (filter #(= title (:header-text %)))
       first
       :container))

(defn open-action-menu-for-container [title]
  (open-action-menu (get-container-element title)))


(defn- get-relationships []
  (->> (taxi/elements "sp-tab-relationship-render-control")
       (map #(hash-map :container %
                       :header-elem (first (taxi/find-elements-under
                                             %
                                             {:css ".structure-control-header"}))))
       ;; strip missing webelements... have seen this and it throws if not handled
       (filter (comp not nil? :webelement :header-elem))
       (map #(assoc % :header-text (taxi/text (:header-elem %))))))

(defn- get-relationship-element [title]
  (->> (get-relationships)
       (filter #(= title (:header-text %)))
       first
       :container))

(defn open-action-menu-for-relationship [title]
  (open-action-menu (get-relationship-element title)))


(defn get-Rel-control-search-box [title]
(str ".tab-relationship-render-control:has(.structure-control-header:contains('" title "')) .sp-search-control-input"))

(defn set-search-text-for-relationship [title text]
  (if-let [search-ele  (get-Rel-control-search-box title)]
    (do
       (set-input-value search-ele text)
       (wait-for-angular))
   ))

(defn click-button-in-relationship [rel-title button-name]
	(if-let [btn-ele  (str ".tab-relationship-render-control:has(.structure-control-header:contains(\"" rel-title "\")) button[title='" button-name "']")]
    (do
       (click btn-ele)
       (wait-for-angular))
   ))

(defn click-button-in-relationship-v2 [rel-title button-name]
	(if-let [btn-ele  (str "sp-vertical-stack-container-control:contains('" rel-title "') button[title='" button-name "']")]
    (do
       (click btn-ele)
       (wait-for-angular))
   ))

(defn click-send-notification-button-in-relationship [rel-title]
	(if-let [btn-ele  (str ".tab-relationship-render-control:has(.structure-control-header:contains(\"" rel-title "\")) button[title='Send Notification']")]
    (do
       (click btn-ele)
       (wait-for-angular))
   ))


   

  

	

