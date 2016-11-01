(ns rt.po.board
  (:require [rt.setup :refer [get-app-url get-tenant get-settings update-settings]]
            [rt.lib.wd :refer :all]
            [rt.lib.wd-ng :refer :all]
            [rt.lib.wd-rn :refer :all]
            [rt.lib.util :refer :all]
            [rt.po.common :refer [exists-present? select-picker-dialog-grid-row-by-text click-ok click-cancel wait-until]]
            [clj-webdriver.taxi :as taxi
             :refer [execute-script to refresh set-finder! *finder-fn* elements element find-element-under
                     text attribute input-text exists? displayed? present?
                     take-screenshot implicit-wait]]
            [clj-webdriver.core :refer [->actions move-to-element]]
            [clojure.string :as string]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import (org.openqa.selenium StaleElementReferenceException)))

(defn board-header-icon-exist?[]
	(exists? (str ".board-view span[ng-if='board.iconInfo.headerIconUrl'] img")))

(defn get-board-header-icon-background-colour []
	(rt.po.common/get-colour-from-css-colour (rt.po.common/get-element-css-value ".board-view span[ng-if='board.iconInfo.headerIconUrl']" "background-color")))

(defn get-board-header-text []
  	(rt.po.common/wait-until #(rt.po.common/exists-present? ".board-header h1") 5000)
  	(clojure.string/trim (taxi/text ".board-header h1")))

(defn click-refresh []
  	(taxi/click "button[title='Refresh']"))

(defn quick-add-exists? []
  	(rt.po.common/exists-present? "input[placeholder='New item']"))

(defn set-quick-add [value]
  (rt.lib.wd/set-input-value "input[placeholder='New item']" value))

(defn get-quick-add []
  (taxi/attribute (taxi/element "input[placeholder='New item']") "value"))

(defn click-quick-add []
  (taxi/click "button[ng-click*='addItem']"))

(defn settings-open? []
  (taxi/visible? ".config-body"))

(defn click-settings []
  (taxi/click "button[title='Settings']"))

(defn get-card-template []
  (taxi/text (first (taxi/selected-options "select[ng-model='board.cardTemplateName']"))))

(defn set-card-template [value]
  (taxi/select (str "select[ng-model='board.cardTemplateName'] option[label='" value "']")))

(defn get-column-source []
  (taxi/text (first (taxi/selected-options "select[ng-model='board.colDimOrd']"))))

(defn set-column-source [value]
  (taxi/select (str "select[ng-model='board.colDimOrd'] option[label='" value "']")))

(defn get-row-source []
  (taxi/text (first (taxi/selected-options "select[ng-model='board.rowDimOrd']"))))

(defn set-row-source [value]
  (taxi/select (str "select[ng-model='board.rowDimOrd'] option[label='" value "']")))

(defn get-color-source []
  (taxi/text (first (taxi/selected-options "select[ng-model='board.styleDimOrd']"))))

(defn set-color-source [value]
  (taxi/select (str "select[ng-model='board.styleDimOrd'] option[label='" value "']")))

(defn- source-value-checked? [dimension value]
  (let [q (str "li[ng-repeat*='" dimension "'] label:contains('" value "') input[type='checkbox']")]
    (taxi/selected? q)))

(defn- set-source-value [dimension label check]
  (let [q (str "li[ng-repeat*='" dimension "'] label:contains('" label "') input[type='checkbox']")]
    (when-not (= (taxi/selected? q) check) (taxi/toggle q))))

(defn column-source-value-checked? [value]
  (source-value-checked? "colDimension" value))

(defn set-column-source-value [label check]
  (set-source-value "colDimension" label check))

(defn row-source-value-checked? [value]
  (source-value-checked? "rowDimension" value))

(defn set-row-source-value [label check]
  (set-source-value "rowDimension" label check))

(defn color-source-value-checked? [value]
  (source-value-checked? "styleDimension" value))

(defn set-color-source-value [label check]
  (set-source-value "styleDimension" label check))

(defn click-settings-dialog-save []
  (rt.po.common/click-modal-dialog-button-and-wait (str ".config-footer button:contains('Save')")))

(defn click-settings-dialog-close []
  (rt.po.common/click-modal-dialog-button-and-wait (str ".config-footer button:contains('Close')")))

(defn set-search [value]
  (rt.po.common/set-search-text-input-value "input[placeholder='Search']" value))

(defn clear-search []
  (taxi/click ".sp-search-control-clear"))

(defn- get-legend-element [value]
  (taxi/element (str "li.board-legend-item:contains('" value "')")))

(defn- get-legend-item [el]
  (let [n (taxi/find-element-under el {:tag :span}),
        c (taxi/find-element-under el (clj-webdriver.core/by-class-name "counter"))]
    {:text (clojure.string/trim (taxi/text n)), :count (read-string (clojure.string/trim (taxi/text c)))}))

(defn get-legend []
  (let [els (for [el (taxi/elements "li.board-legend-item")] (get-legend-item el))]
    (map :text els)))

(defn get-legend-item-count [value]
  (let [el (get-legend-element value),
        i (get-legend-item el)] 
    (:count i)))

(defn legend-contains? [value]
  (if (some #(= value %) (rt.po.board/get-legend)) true false))

(defn get-column-count []
  (count (taxi/elements ".board-column-header-item")))

(defn scroll-reset []
  (taxi/execute-script "var boardRows = angular.element(document.getElementsByClassName('board-rows'))[0]; boardRows.scrollTop = 0; boardRows.scrollLeft = 0;"))

(defn- scroll [a b c]
  (let [pos (taxi/execute-script (str "var boardRows = angular.element(document.getElementsByClassName('board-rows'))[0];
                                      var pos = boardRows.scroll" a ";
                                      boardRows.scroll" a " = boardRows.scroll" a " " b " boardRows.offset" c ";
                                      return boardRows.scroll" a " - pos;"))]
    (let [p (not (and (> pos -10) (< pos 10)))] p)))

(defn scroll-down []
  (scroll "Top" "+" "Height"))

(defn scroll-up []
  (scroll "Top" "-" "Height"))

(defn scroll-right []
  (scroll "Left" "+" "Width"))

(defn scroll-left []
  (scroll "Left" "-" "Width"))

(defn- get-visible-columns []
  (for [el (taxi/elements ".board-column-header-item")]
    (taxi/text el)))

(defn- column-visible? [value]
  (let [names (get-visible-columns)]
    (if (some #(clojure.string/starts-with? % value) names) true false)))

(defn column-exists? [value]
  (rt.po.board/scroll-reset)
  (while (and (not (column-visible? value)) (rt.po.board/scroll-right)))
  (column-visible? value))

(defn- get-column-element [value]
  (taxi/element (str ".board-column-header-item:contains('" value "')")))
      
(defn- get-column-item [el]
  (let [n0 (clojure.string/trim (taxi/text el)),
        c0 (clojure.string/last-index-of n0 "\n"),
        n1 (if-let [n c0] (subs n0 0 n) n0)
        c1 (if-let [c c0] (read-string (subs n0 (+ c 2) (clojure.string/last-index-of n0 ")"))) 0)]
    {:text n1, :count c1}))

(defn get-column-item-count [value]
  (let [el (get-column-element value),
        i (get-column-item el)]
    (:count i)))

(defn- index-of-column [e]
  (rt.po.board/scroll-reset)
  (while (and (not (column-visible? e)) (rt.po.board/scroll-right)))
  (first (keep-indexed #(if (clojure.string/starts-with? %2 e) %1) (rt.po.board/get-visible-columns))))

(defn get-rows []
  (for [el (taxi/elements ".board-row-title")]
    (taxi/text el)))

(defn row-exists? [value]
  (let [names (get-rows)]
    (if (some #(clojure.string/starts-with? % value) names) true false)))

(defn- get-legend-selector [legend]
  (str "li.board-legend-item:contains('" legend "')"))

(defn- get-dimension-selector [dim]
  (when-let [n (index-of-column dim)]
     (str ".board-dimension-item:nth-child(" (+ n 1) ")")))

(defn- get-dimension-in-row-selector [dim row]
  (when-let [n (index-of-column dim)]
     (str ".board-row:has(.board-row-title:contains('" row "')) .board-dimension-item:nth-child(" (+ n 1) ")")))

(defn- get-card-selector [dim card]
  (when-let [n (index-of-column dim)]
     (str ".board-dimension-item:nth-child(" (+ n 1) ") .board-card-item:contains('" card "')")))

(defn- get-card-in-row-selector [row dim card]
  (when-let [n (index-of-column dim)]
     (str ".board-row:has(.board-row-title:contains('" row "')) .board-dimension-item:nth-child(" (+ n 1) ") .board-card-item:contains('" card "')")))

(defn- get-dimension-element [dim]
  (when-let [s (get-dimension-selector dim)]
    (taxi/element s)))

(defn- get-dimension-element-in-row [dim row]
  (when-let [s (get-dimension-in-row-selector dim row)]
    (taxi/element s)))

(defn get-card-elements [dim]
  (let [n (index-of-column dim)]
    (taxi/elements (str ".board-dimension-item:nth-child(" (+ n 1) ") .board-card-item"))))

(defn get-card-elements-in-row [dim row]
  (let [n (index-of-column dim)]
    (taxi/elements (str ".board-row:has(.board-row-title:contains('" row "')) .board-dimension-item:nth-child(" (+ n 1) ") .board-card-item"))))

(defn get-card-element [dim card]
  (when-let [s (get-card-selector dim card)]
    (taxi/element s)))

(defn get-card-element-in-row [row dim card]
  (when-let [s (get-card-in-row-selector row dim card)]
    (taxi/element s)))

(defn card-exists? [dim card]
  (not (nil? (get-card-element dim card))))

(defn card-exists-in-row? [row dim card]
  (not (nil? (get-card-element-in-row row dim card))))

(defn right-click-card [dim card]
  (when-let [element (rt.po.board/get-card-element dim card)]
    (rt.lib.wd/right-click element)))

(defn right-click-card-in-row [row dim card]
  (when-let [element (rt.po.board/get-card-element-in-row row dim card)]
    (rt.lib.wd/right-click element)))

(defn click-card-link [dim card]
  (when-let [link (taxi/find-element-under (rt.po.board/get-card-element dim card) {:tag :a})]
    (taxi/click link)))

(defn click-card-link-in-row [row dim card]
  (when-let [link (taxi/find-element-under (rt.po.board/get-card-element-in-row row dim card) {:tag :a})]
    (taxi/click link)))

(defn- drag-and-drop [sourceSelector targetSelector]
  (taxi/execute-script (str "var dragData = {};
                            var el = $(\"" sourceSelector "\");
                            var t = $(\"" targetSelector "\");

                            var dragStart = document.createEvent(\"Event\");
                            dragStart.initEvent(\"dragstart\", true, true);
                            dragStart.dataTransfer = {
                            	setData: function(type, val) {
                            		dragData[type] = val;
                            		return val;
                            	}.bind(this),
                            	dropEffect: \"move\"
                            };
                            el[0].dispatchEvent(dragStart);

                            var drop = document.createEvent(\"Event\");
                            drop.initEvent(\"drop\", true, true);
                            if (t[0].dispatchEvent) {
                            	t[0].dispatchEvent(drop);
                            } else {
                            	el[0].dispatchEvent(drop);
                            }

                            var dragEnd = document.createEvent(\"Event\");
                            dragEnd.initEvent(\"dragend\", true, true);
                            el[0].dispatchEvent(dragEnd);"))
  (rn.common/safe-wait-for-angular))

(defn drag-card-to-dimension [card oldDim newDim]
  (let [src (get-card-selector oldDim card),
        tar (get-dimension-selector newDim)]
    (drag-and-drop src tar)))

(defn drag-card-to-dimension-and-row [card oldDim newDim oldRow newRow]
  (let [src (get-card-in-row-selector oldRow oldDim card),
        tar (get-dimension-in-row-selector newDim newRow)]
    (drag-and-drop src tar)))

(defn drag-card-to-legend [dim card legend]
  (let [src (get-card-selector dim card),
        tar (get-legend-selector legend)]
    (drag-and-drop src tar)))

(defn drag-card-in-row-to-legend [row dim card legend]
  (let [src (get-card-in-row-selector row dim card),
        tar (get-legend-selector legend)]
    (drag-and-drop src tar)))

(defn drag-legend-to-card [legend dim card]
  (let [src (get-legend-selector legend),
        tar (get-card-selector dim card)]
    (drag-and-drop src tar)))

(defn drag-legend-to-card-in-row [legend row dim card]
  (let [src (get-legend-selector legend),
        tar (get-card-in-row-selector row dim card)]
    (drag-and-drop src tar)))

