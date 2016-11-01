(ns rn.mobile.report
  (require [rt.lib.wd :refer [checked-find-element debug-click prepare-script-arg]]
           [rt.lib.wd-ng :refer [wait-for-angular execute-script-on-element]]
           [clj-webdriver.taxi :as taxi]
           [clojure.string :as string]
           [rt.lib.wd :as wd]
           [rt.lib.wd-ng :as wd-ng]
           rt.po.report-view
           [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(def select-row-by-text rt.po.report-view/select-row-by-text)
(def double-click-row-by-text rt.po.report-view/double-click-row-by-text)

(defn wait-for-row-by-text [row-text & [grid-locator column-index]]
  (rt.po.common/wait-until #(= (nil? (rt.po.common/get-grid-row-element-by-text row-text grid-locator column-index)) false) 5000))

(defn expect-row-by-text [row-text & [grid-locator column-index]]
  (wait-for-row-by-text row-text grid-locator column-index)
  (rt.test.expects/expect-equals false (nil? (rt.po.common/get-grid-row-element-by-text row-text grid-locator column-index))))

(defn back []
  (debug-click (checked-find-element "a[ng-click*='navigateToParent']:has(img[src*='toolbar_back'])")))

(defn click-new []
  (debug-click (checked-find-element "button[title=New]:visible"))
  (wait-for-angular))

(defn click-add []
  ;; there is a bug right now where this button is not visible
  ;; and so we have workaround ...
  ;(taxi/click (checked-find-element "button[title*=xisting]:visible"))
  (info "***** click-add: NEED TO FIX THIS WHEN THE STYLING IS FIXED FOR THIS BUTTON ****")
  (execute-script-on-element
    "angular.element(arguments[0]).scope().ab.execute();"
    "button[title*=xisting")
  (wait-for-angular))

(defn choose-picker-row [value & [column-index]]
  ;; filter to the desired type ... not necessary but doing it anyway
  (rt.po.common/set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" value)
  ;; choose the type
  (select-row-by-text value ".entityReportPickerDialog .dataGrid-view" column-index))

(defn close-picker-ok []
  (rt.po.common/click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))

(def get-loaded-column-values rt.po.report-view/get-loaded-column-values)

(defn set-search-text [value]
  (rt.po.common/set-search-text-input-value ".sp-search-control input" value))

(defn call-row-press [e]
  (taxi/execute-script
    "
    var elem = angular.element(arguments[0]);
    if (!elem || _.isEmpty(elem)) return 'failed to find element';
    var scope = elem.scope();
    if (!scope) return 'element has no angular scope';
    if (!scope.rowPress) return 'element has no rowPress member function';
    var noop = function(){};
    var event = {preventDefault:noop,srcEvent:{preventDefault:noop}};
    console.log('calling rowPress', scope, !!scope.rowPress);
    scope.rowPress(event, scope.row);
    "
    [(prepare-script-arg e)]))

(defn show-context-menu [e]
  (taxi/execute-script
    "
    var elem = angular.element(arguments[0]);
    if (!elem || _.isEmpty(elem)) return 'failed to find element';
    var scope = elem.scope();
    if (!scope) return 'element has no angular scope';
    if (!scope.showContextMenuForTest) return 'element has no showContextMenuForTest member function';
    scope.showContextMenuForTest()
    "
    [(prepare-script-arg e)]))

(defn right-click-row-by-text [row-text & [grid-locator]]
  (when-let [element (rt.po.report-view/get-grid-row-element-by-text row-text grid-locator)]
    ;; this isn't working ... only seems to be selecting the row, and not bring up the menu...
    ;; todo - get this to cause the menu to show
    (call-row-press element)
    (show-context-menu element)
    (wait-for-angular)))

(defn choose-context-menu-this-works-but-maybe-unecessary
  "Choose the given menu item. Assumes the menu is already open.

  FIXME - the menu item lookup may find the wrong item if another item contains the same item text."
  [name]
  (let [s (str "ul.contextmenu-view a.menuItem:contains('" name "')")]
    ;; here we need to wait for the control... waiting for angular isn't sufficient
    (if (try
          (rt.lib.wd/wait-for-jq s)
          true
          (catch Exception _ false))
      (let [expr (taxi/attribute s "sp-first-touch")]
        (debug "running " s expr)
        (rt.lib.wd-ng/evaluate-angular-expression s expr))
      (throw (Exception. (str "Failed to find menu or menu item: " name))))
    ;; most of the time the menu action causes things worth waiting on
    (wait-for-angular)))

;; todo - move this to more generic namespace
(defn choose-context-menu
  "Choose the given menu item. Assumes the menu is already open.

  FIXME - the menu item lookup may find the wrong item if another item contains the same item text."
  [name]
  (let [s (str "ul.contextmenu-view a.menuItem:contains('" name "')")
        expr (taxi/attribute s "hm-tap")]
    (rt.lib.wd-ng/evaluate-angular-expression s expr)
    ;; most of the time the menu action causes things worth waiting on
    (wait-for-angular)))

;; to come back to this one, but for now do it behind the scenes
(defn delete-record
  "delete the given record in the current report"
  [name]
  (set-search-text name)
  (when-not (empty? (rt.po.report-view/get-loaded-grid-values))
    (right-click-row-by-text name)
    (choose-context-menu "Delete")
    (rt.po.common/click-button "OK"))
  (set-search-text "")
  (wait-for-angular))

(defn delete-record-the-direct-way
  "delete the given record in the current report"
  [name]
  (doseq [{id :id} (rt.lib.wd-rn/get-entities-of-type "resource" "id,name" {"filter" (str "Name='" name "'")})]
    (debug "deleting record id" id ", name" name)
    (rt.lib.wd-rn/delete-entity id)))

(defn count-report-columns []
  (count (taxi/elements ".ngHeaderCell")))

(defn wait-until-report-column-count [count]
  (rt.po.common/wait-until #(= (count-report-columns) count) 5000))

(defn expect-report-column-count [count]
  (wait-until-report-column-count count)
  (rt.test.expects/expect-equals count (count-report-columns)))

(defn count-report-rows []
  (count (taxi/elements ".ngRow")))

(defn wait-until-report-row-count [count]
  (rt.po.common/wait-until #(= (count-report-rows) count) 5000))

(defn expect-report-row-count [count]
  (wait-until-report-row-count count)
  (rt.test.expects/expect-equals count (count-report-rows)))

(def get-report-cell-text-content rt.po.report-view/get-report-cell-text-content)

(defn- get-header-cell-element [column-name]
  (let [cols (taxi/elements {:css ".ngHeaderSortColumn"})]
    (let [el (first (filter #(= (taxi/text %) column-name) cols))] el)))

(defn column-sort-ascending? [column-name]
  (let [col (get-header-cell-element column-name)]
    (let [i (first (taxi/find-elements-under col {:css ".ngSortButtonDown"}))]
      (not (nil? (get i :webelement))))))

(defn column-sort-descending? [column-name]
  (let [col (get-header-cell-element column-name)]
    (let [i (first (taxi/find-elements-under col {:css ".ngSortButtonUp"}))]
      (not (nil? (get i :webelement))))))

(defn sort-column-ascending [column-name]
  (let [el (get-header-cell-element column-name)]
    (when (not (column-sort-ascending? column-name))
      (do
        (wd/debug-click el)
        (wd-ng/wait-for-angular)
        (sort-column-ascending column-name)))))

(defn sort-column-descending [column-name]
  (let [el (get-header-cell-element column-name)]
    (when (not (column-sort-descending? column-name))
      (do
        (wd/debug-click el)
        (wd-ng/wait-for-angular)
        (sort-column-descending column-name)))))

(defn scroll-to-last-record
  ([] (scroll-to-last-record 0))
  ([last-scroll]
    (try
      (let [rows (count-report-rows)]
        (let [scroll (taxi/execute-script
           "
              var reportControl = angular.element(document.getElementsByClassName('ngViewport'))[0];
              reportControl.scrollTop = reportControl.scrollHeight - reportControl.offsetHeight;
              return reportControl.scrollTop;
            ")]
          ;(clojure.pprint/pprint scroll)
          ;(clojure.pprint/pprint last-scroll)
          (wd-ng/wait-for-angular)
          ; seems like the delay load of rows can be a timing problem?
          ; with no other indication of further rows to come, we kind of have to wait :(
          (rt.po.common/wait-until #(> (count-report-rows) rows) 1000)
          (when (< last-scroll scroll) (scroll-to-last-record scroll))))
      (catch Exception e
        (throw (Exception. "failed to scroll to last record due to client exception"))))))
