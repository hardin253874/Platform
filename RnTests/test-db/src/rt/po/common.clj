(ns rt.po.common
  (:require [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            rt.setup
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [drag-n-drop set-click-to-edit-value]]
            [rt.lib.util :refer [timeit]]
            [clj-webdriver.taxi :refer [text value exists? present? selected? displayed? find-elements-under
                                        select-by-text selected-options element input-text click elements attribute
                                        present? visible? clear close]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clojure.java.io :as io]
            [clojure.string :as string]
            [clj-time.core :as t]
            [clj-time.local :as tl]
            [clj-time.format :as tf]
            [clj-time.coerce :as tc]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import (org.openqa.selenium StaleElementReferenceException)))


;; HELPERS
(defn exists-present? [q]
  (try
    (let [e (element q)
          e? (and e (exists? e))
          ;; check e? or present? can throw
          p? (and e? (present? e))]
      (when-not p?
        (debugf "exists-present? false for %s with exists? %s and present? %s" q e? p?))
      (if p? true false))
    (catch StaleElementReferenceException _
      ;; ignore these.. treat as element not existing
      (debugf "exists-present? on %s sees StaleElementReferenceException" q)
      false)))

(defn safe-text [q]
  ;; return text, or nil if not found
  (if (exists? q) (text q) nil))

(defn get-file-name [uri]
  "Return the file-name portion of a URL"
  (nth (re-find (re-pattern "(?:.*[/])([^?]*)") uri) 1))

(defn- safe-wait-until-pred [pred]
  (try
    (pred)
    (catch StaleElementReferenceException _
      ;; ignore these.. treat as element not existing
      false)))

(defn wait-until
  "
Wait up until the given timeout (msecs) for the condition defined
by the given predicate to return true. Return false if we time out.

Example:
(rt.po.common/wait-until #(taxi/visible \".myelementclass\") 5000)
  "
  ([pred timeout interval]
   (try
     (clj-webdriver.taxi/wait-until #(safe-wait-until-pred pred) timeout interval)
     true
     (catch Exception _
       (error "common/wait-until timed out")
       false)))
  ([pred timeout]
   (wait-until pred timeout 100))
  ([pred]
   (wait-until pred 5000 100)))

(defn count-busy-indicator-elements []
  (count (elements ".busyIndicator-view:visible")))

(defn wait-until-busy-indicator-done []
  (wait-until #(= (count-busy-indicator-elements) 0) 5000))

(defn count-modal-dialogs []
  (count (elements ".modal-dialog:visible")))

(defn wait-until-modal-dialog-count
  [count timeout]
  (wait-until #(= (count-modal-dialogs) count) timeout))

(defn click-modal-dialog-button-and-wait
  [button-selector]
  (let [dialog-count (count-modal-dialogs)]
    (do
      (click button-selector)
      (wait-until-modal-dialog-count (dec dialog-count) 5000)
      (wait-for-angular))))

(defn set-search-text-input-value
  [selector text]
  (wait-for-angular)
  (set-input-value selector text)
  ;; sleep a little to deal with debounce that seems to be as high as 500
  (Thread/sleep 1500)
  (wait-for-angular)
  (Thread/sleep 2500))

;; date time related functions

; this ignores the time component of the datetime string. expects datetime string in followinf formats
; "6/2/2016 12:00 AM" or "2016/12/12 12:00 AM"
(defn get-date-string-from-date-time-string [date-time-string]
  (if-let [space-index (clojure.string/index-of date-time-string " " 0)]
    (subs date-time-string 0 space-index)
    date-time-string
    ))

(defn get-now-date-time-sydney []
  (t/to-time-zone (t/now) (t/time-zone-for-id  "Australia/Sydney")))

;; creates a date-time from local now by ignoring time component
(defn get-today-local-date []
  (t/date-time (t/year (tl/local-now)) (t/month (tl/local-now)) (t/day (tl/local-now)))
  )

(defn get-today-local-date-string [custom-format]
  (tf/unparse (tf/formatter custom-format) (get-today-local-date)))

(defn get-today-local-date-string-us-format []
  (tf/unparse (tf/formatter "M/d/yyyy") (get-today-local-date)))

(defn get-today-local-date-string-aus-format []
  (tf/unparse (tf/formatter "d/M/yyyy") (get-today-local-date)))

(defn get-today-day []
  (t/day (tl/local-now)))

(defn get-today-month []
  (t/month (tl/local-now)))

(defn get-today-year []
  (t/year (tl/local-now)))

;; Grid helpers

(defn check-element [element & msgs]
  (let [msgs (if (empty? msgs) ["invalid element"] msgs)]
    (when-not (:webelement element) (throw (Exception. (apply str msgs))))))

(defn get-grid-row-element-by-text [row-text & [grid-locator column-index]]
  (wait-for-angular)
  (wait-until-busy-indicator-done)
  (let [grid-locator (or grid-locator ".spreport-view")
        cell-query (str ".//div[contains(@class,'dataGridCell') and "
                        (if column-index (str "contains(@class,'col" column-index "') and "))
                        " contains(.,'" row-text "')]")]
    ;;todo - tidy this up..... at some point waiting for angular wasn't cutting it so
    ;;putting in this wait for jq that I have. But need to clean all this up...
    (let [q (str ".dataGridCellText:contains(\"" row-text "\")")]
      (try (wait-until #(exists-present? q) 60000)
           (catch Exception e
             (error "Exception waiting for an element to appear")
             (debug "DEBUG: count visible and not" (count (elements (str q ":visible"))) (count (elements q))))))
    (let [elements (filter exists-present? (filter :webelement (find-elements-under grid-locator (by-xpath cell-query))))
          ;; look for exact match, falling back to the first found if none are exact
          element (first (filter #(= row-text (text %)) elements))
          element (or element (first elements))]
      (check-element element "Failed to find row for " row-text)
      element)))

(defn select-row-by-text [row-text & [grid-locator column-index]]
  (when-let [element (get-grid-row-element-by-text row-text grid-locator column-index)]
    (click element)))

(defn report-row-contains-text? [row-text & [grid-locator column-index]]
	(let [element (get-grid-row-element-by-text row-text grid-locator column-index)]
    (exists? element)))
	
(defn select-picker-dialog-grid-row-by-text [row-text]
  (when-let [element (get-grid-row-element-by-text row-text ".entityReportPickerDialog .dataGrid-view")]
    (click element)))


;; NAME AND DESCRIPTION - INLINE EDIT

(defn get-name []
  (text ".builder-Edit-Name"))

(defn get-description []
  (text ".builder-Edit-Description"))

(defn set-name [name]
  (set-click-to-edit-value ".builder-Edit-Name" name))

(defn set-description [description]
  (set-click-to-edit-value ".builder-Edit-Description" description))


;; GENERAL DIALOG FUNCTIONS

(defn get-button-selector [name]
  (str ".modal-dialog button:contains(" name ")"))

(defn click-button [name]
  (click (str ".modal-dialog button:contains(" name ")")))

(defn click-link [name]
  (click (str "A:contains(" name ")")))

(defn click-toolbar [name]
  (click (str ".builder-ToolBar button[uib-popover=" name "]")))

(defn click-ok []
  (click-modal-dialog-button-and-wait (get-button-selector "OK")))

(defn click-cancel []
  (click-modal-dialog-button-and-wait (get-button-selector "Cancel"))

  ;; we have a sometimes issue where clicking on the button doesn't work and
  ;; doesn't throw an error - have seen it about 1 in 10 times
  ;; on a modal cancel button... so this is a workaround
  (let [selector (str "button:contains(Cancel)")]
    (when (and (exists? selector) (displayed? selector))
      (warn "Unexpected - the cancel button is still displayed")
      (Thread/sleep 100)
      (click-button "Cancel"))))


;; FIELD FUNCTIONS

(defn find-field [label]
  (timeit (str "find-field: " label)
          (or (first
                (filter
                  exists-present?
                  (list
                    (str ".row:has(.cell:contains(" label ":))")
                    (str ".edit-form-control-container:has(.edit-form-title span:contains(" label ")) .edit-form-value")
                    ;; relationship properties dialog use label instead of span
                    (str ".edit-form-control-container:has(.edit-form-title label:contains(" label ")) .edit-form-value")
                    (str "div[style*=table-row]:has(div[style*=table-cell]:contains(" label ":))")
                    ;; for the misplaced hack in spEditFormsDialog
                    (str ".tab-container:has(.field-title:contains(" label ":)) :last-child"))))
              ;; encode warning message in a valid css selector so we see it in the log
              (str ":contains(No field found for label \"" label "\")"))))

(defn field-visible? [label]
  (exists-present? (find-field label)))

(defn field-read-only? [label]
  (let [field (find-field label)]
    (exists-present?
      (str field " input[disabled='disabled']"))))

(defn combo-read-only? [label]
  (let [field (find-field label)]
    (exists-present?
      (str field " select[disabled='disabled']"))))

;; STRING FIELD FUNCTIONS
(defn get-string-field [fieldSelector]
  (value (str fieldSelector " input")))

(defn get-string [label]
  (get-string-field (find-field label)))

(defn set-string-field [fieldSelector value]
  (clear (str fieldSelector " input"))
  (input-text (str fieldSelector " input") value)
  nil)

(defn set-string [label value]
  (set-string-field (find-field label) value))

(defn get-multiline-field [fieldSelector]
  (value (str fieldSelector " textarea")))

(defn get-multiline [label]
  (get-multiline-field (find-field label)))

(defn set-multiline-field [fieldSelector value]
  (clear (str fieldSelector " textarea"))
  (input-text (str fieldSelector " textarea") value)
  nil)

(defn set-multiline [label value]
  (set-multiline-field (find-field label) value))

;; BOOL FIELD FUNCTIONS
(defn get-bool-field [fieldSelector]
  (selected? (str fieldSelector " input")))  

(defn get-bool [label]
  (get-bool-field (find-field label)))

(defn set-bool-field [fieldSelector value]
  (when (not= (get-bool-field fieldSelector) value)
    (click (str fieldSelector " input")))
  nil)

(defn set-bool [label value]
  (set-bool-field (find-field label) value))

;; COMBO FIELD FUNCTIONS
(defn get-combo-field [fieldSelector]
  (text (first (selected-options (str fieldSelector " select")))))

(defn get-combo [label]
  (get-combo-field (find-field label)))

(defn set-combo-field [fieldSelector value]
  (select-by-text (str fieldSelector " select") value)
  nil)

(defn set-combo [label value]
  (set-combo-field (find-field label) value))  

;; TIME FIELD FUNCTIONS
(defn set-time-control-value [field h m meridian]
  (let [time-input (str field " div.sp-time-control-input-cell input[sp-time-control-validation]")]      
    (set-input-value time-input (str h ":" m " " meridian))))

(defn set-time-control-value-via-popup [field h m meridian]
  (let [time-button (str field " div.sp-time-control-button-cell button.btn-time")]
    (do
      (click time-button)
      (wait-until #(exists-present? "div.sp-time-control-popup-view:visible"))
      (let [hours-input "div.sp-time-control-popup-view:visible input[ng-model='hours']"
            minutes-input "div.sp-time-control-popup-view:visible input[ng-model='minutes']"
            meridian-button "div.sp-time-control-popup-view:visible button[ng-click*='toggleMeridian']"
            meridian-button-text (text "div.sp-time-control-popup-view:visible button[ng-click*='toggleMeridian']")
            done-button "div.sp-time-control-popup-view:visible button.button-done"]
        (do
          (set-input-value hours-input h)
          (set-input-value minutes-input m)
          
          (if (not= meridian-button-text meridian)
            (click meridian-button))

          (click done-button))))))      

;; LOOKUP FIELD FUNCTIONS

;;TODO - this is horrid slow.... need to fix
(defn get-lookup-field [fieldSelector]  
  (if (exists-present? (str fieldSelector " input"))
    (get-string-field fieldSelector)
    ;; restored the next line that someone deleted a long time ago?!
    (text (str fieldSelector " A.fieldValue"))))

(defn get-lookup [label]
  (get-lookup-field (find-field label)))

(defn set-lookup-field [fieldSelector value]
  ;; Open picker report
  (click (str fieldSelector " button[uib-popover=Edit]"))
  ;; filter to the desired type ... not necessary but doing it anyway
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" value)
  ;; choose the type
  (select-row-by-text value ".entityReportPickerDialog .dataGrid-view")
  ;; ok the typepicker
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))

(defn set-lookup [label value]
  (set-lookup-field (find-field label) value))  

(defn clear-lookup-field [fieldSelector]
  (click (str fieldSelector " button[uib-popover=Clear]")))

(defn clear-lookup [label]
  (clear-lookup-field (find-field label)))

;; COLOUR FIELD FUNCTIONS

(defn get-color [label]
  (text (str (find-field label) " .spColorPicker-view .dropdownSelectedItemName")))

(defn set-color [label value]
  (click (str (find-field label) " .dropdownIcon"))
  (click (str ".spColorPicker-view:last-child .dropdownMenuItem span:contains(" value ")")))

(defn close-window []
  (close))

;; OPTIONS PANEL
(defn options-expanded? []
  (exists-present? ".modal-dialog .collapse.in"))

(defn options-expand []
  (when (not (options-expanded?))
    (click ".modal-dialog .option")
    (wait-for-angular)))

(defn click-desktop []
  (click "button:contains(Desktop)"))

(defn click-tablet []
  (click "button:contains(Tablet)"))

(defn click-mobile []
  (click "button:contains(Mobile)"))

(defn enabled-on-desktop? []
  (= "desktop-checked" (attribute (element "button:contains(Desktop)") "class")))

(defn enabled-on-tablet? []
  (= "tablet-checked" (attribute (element "button:contains(Tablet)") "class")))

(defn enabled-on-mobile? []
  (= "mobile-checked" (attribute (element "button:contains(Mobile)") "class")))

;;Drivers related to files.

(defn get-data-file-path [file-name]
  (let [data-dir (or (:data-dir (rt.setup/get-settings)) "data")
        path-str (str (System/getProperty "user.dir") "/" data-dir "/" file-name)]
    (.getCanonicalPath (io/file path-str))))

(defn get-download-file-path [file-name]
  (let [path-str (str (System/getProperty "user.home") "/Downloads/" file-name)]
    (.getCanonicalPath (io/file path-str))))

(defn file-exist? [file-path]
  (.exists (io/file file-path)))

(defn wait-until-file-exist? 
  ([file-path]
  (wait-until-file-exist? file-path 30000))
  ([file-path timeout]
  (wait-until #(file-exist? file-path) timeout)))

(defn get-text-file-string [file-path]
  ;;put some wait before opening the file
  (wait-until #(file-exist? file-path) 50000)
  (when-not (file-exist? file-path)
    (throw (Exception. (str "File does not exist at: " file-path))))
  (with-open [rdr (io/reader file-path)]
    (doseq [line (line-seq rdr)]
      (def x (print-str line)))
    x))

(defn delete-file [file-path]
  (io/delete-file (io/file file-path)))

(defn delete-file-in-download-folder [file-name]
  (let [filepath (get-download-file-path file-name)]
    (if (file-exist? filepath)
      (delete-file filepath))))

(defn get-image-url-rgba
  "Returns the colour of the pixel for the specified image url at the specified x and y coordinate.
   When no x and y coordinate is specified the colour at centre of the image is returned.

   Example:
  =========

  (get-image-url-rgba \"http://blah/image.png\" 1 2)"
  ([image-url x y]
   (zipmap [:r :g :b :a]
           (rt.lib.wd/execute-async-script
             "
             var image = new Image();
              var canvas = $('<canvas/>')[0];
              var x = arguments[1];
              var y = arguments[2];
              var done = arguments[arguments.length -1];

              try {

              //console.log('get-image-url-rgba: loading image', arguments[0]);

              // Do stuff when the image is ready
              image.onload = function(){
                console.log('get-image-url-rgba: image loaded');
                if (!image.complete) {
                  return;
                }

                canvas.width = image.width;
                canvas.height = image.height;
                canvas.getContext('2d').drawImage(image, 0, 0, image.width, image.height);

                if (x === -1 || y === -1) {
                  x = image.width / 2;
                  y = image.height / 2;
                }
                //console.log('get-image-url-rgba: done');
                done(canvas.getContext('2d').getImageData(x, y, 1, 1).data);
              };

              image.src = arguments[0];

              } catch (e) {
                //console.log('get-image-url-rgba: exception');
                done(null);
              }
              "
             [image-url x y])))
  ([image-url]
   (get-image-url-rgba image-url -1 -1)))

(defn get-element-css-value
  "Returns the css property value for the specified element

  Example:
  =========

  (get-element-css-value \"div.navbar\" \"background-color\")"
  [selector property-name]
  (clj-webdriver.taxi/execute-script
    "return $(arguments[0]).css(arguments[1])",
    [selector, property-name]))

(defn get-element-background-url
  "Returns the background url for the specified element

  Example:
  =========

  (get-element-background-url \"div.navbar\")"
  [selector]
  (let [url (get-element-css-value selector "background-image")]
    (first (rest (re-find #"url\((.*)\)" (string/replace url #"\"" ""))))))

(defn get-element-image-src
  "Returns the src for the specified image element

  Example:
  =========

  (get-element-image-src \".navbar-brand img\")"
  [selector]
  (attribute selector "src"))

(defn get-colour-from-css-colour
  "Returns the colour from the specified css colour string

  Example:
  =========

  (get-colour-from-css-colour \"rgb(10,10,10)\")
  (get-colour-from-css-colour \"rgba(10,10,10,1)\")"
  [css-colour]
  (let [colourMap (zipmap [:r :g :b :ra :ga :ba :a] (rest (re-find #"rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)|rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)" css-colour)))]
    (let [r (Integer. (if (nil? (:a colourMap)) (:r colourMap) (:ra colourMap)))
          g (Integer. (if (nil? (:a colourMap)) (:g colourMap) (:ga colourMap)))
          b (Integer. (if (nil? (:a colourMap)) (:b colourMap) (:ba colourMap)))
          a (* (Integer. (if (nil? (:a colourMap)) 1 (:a colourMap))) 255)]
      {:r r :g g :b b :a a})))

(defn get-colour-from-name
  "Returns the colour from the specified colour name

  Example:
  =========

  (get-colour-from-name \"Red\")"
  [colourName]
  (get-colour-from-css-colour (rt.lib.wd-rn/get-css-color-from-name colourName)))

(defn get-header-background-colour []
  (get-colour-from-css-colour (get-element-css-value "div.navbar" "background-color")))

(defn get-header-background-image-colour []
  (get-image-url-rgba (get-element-background-url "div.navbar")))

(defn get-header-menu-text-colour []
  (get-colour-from-css-colour (get-element-css-value "div.application-menu a.btn-consolemenu" "color")))

(defn get-logo-image-colour []
  (get-image-url-rgba (get-element-image-src "a.navbar-brand img")))

(defn get-top-navigation-background-colour []
  (get-colour-from-css-colour (get-element-css-value "div.layout-top div.client-tabs-panel" "background-color")))

(defn get-top-navigation-background-image-colour []
  (get-image-url-rgba (get-element-background-url "div.client-tabs-panel")))

(defn get-top-navigation-selected-tab-colour []
  (get-colour-from-css-colour (get-element-css-value "div.client-tabs-panel ul.nav.application-tabs li.nav-level-2.nav-type-console-navSection a" "background-color")))

(defn get-top-navigation-selected-tab-font-colour []
  (get-colour-from-css-colour (get-element-css-value "div.client-tabs-panel ul.nav.application-tabs li.nav-level-2.nav-type-console-navSection.selected.active a" "color")))

(defn get-top-navigation-unselected-tab-colour [tabName]
  (get-colour-from-css-colour (get-element-css-value (str "div.client-tabs-panel ul.nav.application-tabs li.nav-level-2.nav-type-console-navSection a:contains('" tabName "')") "background-color")))

(defn get-top-navigation-unselected-tab-font-colour [tabName]
  (get-colour-from-css-colour (get-element-css-value (str "div.client-tabs-panel ul.nav.application-tabs li.nav-level-2.nav-type-console-navSection a:contains('" tabName "')") "color")))

(defn get-left-navigation-background-colour []
  (get-colour-from-css-colour (get-element-css-value "div.layout-left" "background-color")))

(defn get-left-navigation-background-image-colour []
  (get-image-url-rgba (get-element-background-url "div.layout-left")))

(defn get-left-navigation-selected-element-colour [sectionName]
  (get-colour-from-css-colour (get-element-css-value (str "div.client-nav-panel div.nav-level-1.nav-type-console-navSection a:contains('" sectionName "')") "background-color")))

(defn get-left-navigation-selected-font-colour [sectionName]
  (get-colour-from-css-colour (get-element-css-value (str "div.client-nav-panel div.nav-level-1.nav-type-console-navSection a:contains('" sectionName "')") "color")))

(defn get-nav-item-icon-colour [name]
  (get-image-url-rgba (get-element-image-src (str "div.client-nav-panel ul.nav.nav-list div[class*=nav-level] a:contains('" name "') img"))))

(defn click-tab-heading [value]
  (click (str "[heading='" value "'] a")))

 (defn form-scroll-to-bottom []
  (try
    (clj-webdriver.taxi/execute-script  "var elm = angular.element(document.getElementsByClassName('client-view-content'))[0]; elm.scrollTop = elm.scrollHeight - elm.clientHeight; "
      ) (catch Exception e
          (throw (Exception. "form scroll to bottom failed due to client exception")))))

(defn send-tab-key 
  "Send the tab key to the specified element.

  Example:
  =========

  (send-tab-key element)"
  [element]
  (clj-webdriver.taxi/send-keys element (clj-webdriver.core/key-code :tab)))

(defn send-up-key 
  "Send the up key to the specified element.

  Example:
  =========

  (send-up-key element)"
  [element]
  (clj-webdriver.taxi/send-keys element (clj-webdriver.core/key-code :up)))

(defn send-down-key 
  "Send the down key to the specified element.

  Example:
  =========

  (send-down-key element)"
  [element]
  (clj-webdriver.taxi/send-keys element (clj-webdriver.core/key-code :down)))

(defn get-active-element  
  "Gets the active element.

  Example:
  =========

  (get-active-element)"
  []
  (clj-webdriver.taxi/execute-script
    "return $(document.activeElement);"
    []))

(defn send-tab-key-to-active-element 
  "Sends the tab key to the active element.

  Example:
  =========

  (send-tab-key-to-active-element)"
  []  
  (let [activeElement (get-active-element)]
    (rt.po.common/send-tab-key activeElement)))

(defn send-up-key-to-active-element 
  "Sends the up key to the active element.

  Example:
  =========

  (send-up-key-to-active-element)"
  []  
  (let [activeElement (get-active-element)]
    (rt.po.common/send-up-key activeElement)))

(defn send-down-key-to-active-element 
  "Sends the down key to the active element.

  Example:
  =========

  (send-down-key-to-active-element)"
  []
  (let [activeElement (get-active-element)]
    (rt.po.common/send-down-key activeElement)))