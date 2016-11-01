(ns rn.mobile.form
  (require [clj-webdriver.taxi :as taxi]
           [clojure.string :as string]
           [clojure.pprint :as pp]
           [rt.lib.wd :as wd]
           [rt.lib.wd-ng :as wd-ng]
           rt.po.edit-form
           rt.po.view-form
           [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(defn select-page
  "Select the page of the edit form given the 1-based page number."
  [page-index]
  {:pre (number? page-index)}
  (wd-ng/execute-script-on-element
    (str "
    var scope = angular.element(arguments[0]).scope();
    scope.selectPage(" (dec page-index) ");
    scope.$apply();
    return scope.selectedPage;
    ")
    ;; stuffed if I know why we need the ul here....
    ".sp-page-selector")
  (wd-ng/wait-for-angular))

(defn get-selected-page
  "Return the 1 based page number for the current form"
  []
  (wd-ng/execute-script-on-element
    (str "
    var scope = angular.element(arguments[0]).scope();
    return scope.selectedPage + 1;
    ")
    ;; stuffed if I know why we need the ul here....
    ".sp-page-selector"))

(defn back []
  (rt.po.edit-form/click-back-button))

(defn edit []
  (wd/debug-click (taxi/element "button[test-id='edit']")))

(def save rt.po.edit-form/save)

(def get-field-value rt.po.view-form/get-field-value)

(def get-field-control-element rt.po.edit-form/get-field-control-element)
(def get-text-field-value rt.po.edit-form/get-text-field-value)
(def set-text-field-value rt.po.edit-form/set-text-field-value)
(def set-choice-value rt.po.edit-form/set-choice-value)
(def get-lookup rt.po.edit-form/get-lookup)

(defn set-lookup [label value & [column-index]]
  ;; Open picker report
  (wd/debug-click (str (rt.po.edit-form/find-lookup label) " button[uib-popover=Edit]"))
  (wd/wait-for-jq ".entityReportPickerDialog")
  ;; filter to the desired type ... not necessary but doing it anyway

  ;; having trouble with the click sometimes not bringing up the picker
  (when-not (rt.po.common/exists-present? ".entityReportPickerDialog")
    (debug "RETRYING opening the picker")
    (wd/debug-click (str (rt.po.edit-form/find-lookup label) " button[uib-popover=Edit]"))
    (wd/wait-for-jq ".entityReportPickerDialog"))

  (rt.po.common/set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" value)
  ;; choose the type
  (rt.po.report-view/select-row-by-text value ".entityReportPickerDialog .dataGrid-view" column-index)
  ;; ok the typepicker
  ;;NOTE - not doing the following as selecting the row seems to submit the form...
  ;;- happened I think in the most recent chrome update (we are testing in chrome emulator only)
  #_(click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))


(defn clear-lookup [field-name]
  (let [selector (str "sp-control-on-form:contains(\"" field-name "\"):last button[ng-click*=clear]")]
    (wd/wait-for-jq selector)
    (taxi/click selector)))

(defn show-lookup-selector [field-name]
  (let [selector (str "sp-control-on-form:contains(\"" field-name "\"):last button[ng-click*=spEntityCompositePickerModal]")]
    (wd/wait-for-jq selector)
    (taxi/click selector)
    (wd-ng/wait-for-angular)))

(def get-multi-select-choice-value rt.po.edit-form/get-multi-select-choice-value)

(defn set-multi-select-choice-value
  ([field-name choice-value] (set-multi-select-choice-value field-name choice-value true))
  ([field-name choice-value checked?]
   (let [el (rn.mobile.form/get-field-control-element field-name)
         ddb (taxi/find-element-under el {:css ".dropdownIcon"})
         dd-selector {:css ".entityMultiComboPickerDropdownPopupMenu"}]

     (wd-ng/wait-for-angular)
     (let [dd (taxi/element dd-selector)]
       (when (or (not dd) ((comp not rt.po.common/exists-present?) dd) ((comp not taxi/visible?) dd))
         (wd/debug-click ddb)))

     (wd-ng/wait-for-angular)
     (rt.po.common/wait-until #(taxi/visible? dd-selector) 5000)

     ; find the popup elements
     (let [li (->> (taxi/elements "li[ng-repeat*='entityCheckBoxItem in']")
                   (filter #(.contains (taxi/text %) choice-value))
                   first)
           input (some-> li (taxi/find-element-under {:tag :input}))
           selected? (some-> input taxi/selected?)]
       (when input
         (when (not= checked? selected?)
           (wd/debug-click input)
           ;; trying to deal with intermittant issue where click on ddb below doesn't work
           (Thread/sleep 500))))

     ; close the popup again
     (wd/debug-click ddb)

     ;; check it did close (otherwise we see errors later in tests... confusing)
     (let [dd (taxi/element dd-selector)]
       (when (and dd (rt.po.common/exists-present? dd) (taxi/visible? dd))
         (throw (Exception. "Failed to close choice popup.")))))))

(def set-number-field-value rt.po.edit-form/set-number-field-value)

(def get-number-field-value rt.po.edit-form/number-field-value)

(defn- get-date-input-element [label]
  (let [elems (taxi/elements ".edit-form-control-container")
        ;; todo - fix this... looking a little fragile to me
        elem (first (filter #(.contains (taxi/text %) label) elems))]
    (taxi/find-element-under elem {:css ".sp-date-mobile-control input[type=date]"})))

(defn get-date-field-value [label]
  (let [elem (get-date-input-element label)]
    (taxi/attribute elem "value")))

(defn get-date-field-placeholder [label]
  (let [elem (get-date-input-element label)]
    (taxi/attribute elem "placeholder")))

(defn set-date-field-value [label value]
  (let [elem (get-date-input-element label)]
    ;; resorting to directly setting on the underlying model with JS
    ;; as trying to interact with the INPUT type=date control in the
    ;; chrome emulator just didn't work - although not sure whether the
    ;; emulator or our code...
    (rt.lib.wd-ng/execute-script-on-element
      "
      var scope = angular.element(arguments[0]).scope();
      scope.model.value = arguments[1];
      scope.$apply();
      return scope.model.value
      "
      elem value)))

(defn expect-form-title [title]
  (rt.po.common/wait-until #(= (taxi/text (last (taxi/elements ".form-title"))) title) 5000)
  (rt.test.expects/expect-equals (taxi/text (last (taxi/elements ".form-title"))) title))


