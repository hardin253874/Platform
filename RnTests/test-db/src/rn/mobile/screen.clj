(ns rn.mobile.screen
  (require [clj-webdriver.taxi :as taxi]
           [rt.lib.wd :as wd]
           [rt.lib.wd-ng :as wd-ng]
           rn.mobile.report
           rt.po.report-view
           rt.po.screen))

(def is-report-on-screen rt.po.screen/is-report-on-screen)
(def is-form-on-screen rt.po.screen/is-form-on-screen)

; Row Count
(def count-report-row-in-screen rt.po.report-view/count-report-row-in-screen)
(def wait-until-report-row-count-in-screen rt.po.report-view/wait-until-report-row-count-in-screen)
(def expect-report-row-count-in-screen rt.po.report-view/expect-report-row-count-in-screen)

; Column Count
(defn count-report-column-in-screen [report-name]
  (count (taxi/elements (str ".report-render-control:contains('" report-name "') .ngHeaderCell"))))

(defn wait-until-report-column-count-in-screen [count report-name]
  (rt.po.common/wait-until #(= (count-report-column-in-screen report-name) count) 5000))

(defn expect-report-column-count-in-screen [count report-name]
  (wait-until-report-column-count-in-screen count report-name)
  (rt.test.expects/expect-equals count (count-report-column-in-screen report-name)))

(defn view-full-report []
  (wd/debug-click (taxi/element "button[ng-click*='viewFullReport']"))
  (wd-ng/wait-for-angular))