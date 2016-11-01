(ns rn.tablet.report
  (:require
    [clj-webdriver.core]
    [rt.scripts.common]
    [rn.mobile.report]))

;; Redirect to the mobile and common libraries where possible
(def click-new rn.mobile.report/click-new)
(def double-click-row-by-text rt.po.report-view/double-click-row-by-text)
(def expect-row-by-text rn.mobile.report/expect-row-by-text)
(def get-grid-row-element-by-text rt.po.common/get-grid-row-element-by-text)
(def scroll-to-last-record rn.mobile.report/scroll-to-last-record)
(def sort-column-ascending rn.mobile.report/sort-column-ascending)
(def sort-column-descending rn.mobile.report/sort-column-descending)

;; Ahhh, this is kind of weird and maybe not supported. Use at own risk.
;; Spoke to Scott and I'm no longer sure that screen updates are supposed to work on tablet!
(defn select-row-by-text [row-text & [grid-locator column-index]]
  (clj-webdriver.core/click-and-hold clj-webdriver.taxi/*driver* (get-grid-row-element-by-text row-text grid-locator column-index))
  (clj-webdriver.core/release clj-webdriver.taxi/*driver* (get-grid-row-element-by-text row-text grid-locator column-index)))