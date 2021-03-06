(ns rt.po.survey
  (:require [rt.lib.wd :refer [right-click wait-for-jq wait-until-displayed set-input-value]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression execute-script-on-element]]
            [rt.po.app :as app :refer [make-app-url choose-context-menu enable-config-mode]]
            [rt.po.report-view :as rv]
            [rt.po.common :as common :refer [exists-present?]]
            [clj-webdriver.taxi :as taxi :refer [to exists? present? text attribute click element elements
                                                 find-element-under find-elements-under]]
            [clj-webdriver.core :refer [by-css by-xpath]]
            [clj-time.core :as t]
            [clj-time.format :as tf]
            [clj-time.local :as tl]
            [clojure.string :refer [trim]]
            [clj-time.coerce :as tc]))

(defn- to-list [elements]
  (if (nil? (first (map :webelement elements))) () elements))

(defn- get-section-elements []
  (let [tab (taxi/element ".tab-pane.active")]
    (to-list (taxi/find-elements-under tab (clj-webdriver.core/by-class-name "user-survey-section")))))

(defn- get-section-element [name]
  (last
    (let [sections (get-section-elements)]
      (filter #(= (clojure.string/upper-case name) (let [s (let [el (taxi/find-element-under % (clj-webdriver.core/by-class-name "user-survey-section-heading"))] (taxi/text el))] s)) sections))))

(defn- get-section-header-elements []  
  (let [tab (taxi/element ".tab-pane.active")]
    (to-list (taxi/find-elements-under tab (clj-webdriver.core/by-class-name "user-survey-section-heading")))))

(defn- get-section-header-element [name]
  (let [section (->> (get-section-header-elements)
                     (filter #(.contains (taxi/text %) (clojure.string/upper-case name)))
                     last)] section))

(defn- get-question-elements-under [element]
  (to-list (taxi/find-elements-under element (clj-webdriver.core/by-class-name "ordered-form-row"))))

(defn- get-question-elements []
  (let [tab (taxi/element ".tab-pane.active")]
    (get-question-elements-under tab)))

(defn- get-question-elements-in-section [name]
  (let [sectionElement (get-section-element name)]
    (get-question-elements-under sectionElement)))

(defn- get-question-element [name]
  (last
    (let [rows (get-question-elements)]
      (filter #(= name (let [q (let [el (taxi/find-element-under % (clj-webdriver.core/by-class-name "user-survey-q-text"))] (taxi/text el))] q)) rows))))

(defn get-question-name [questionElement]
  (let [question (taxi/find-element-under questionElement (clj-webdriver.core/by-class-name "user-survey-q-text"))]
    (taxi/text question)))
	
(defn- get-question-element-in-section [section question]
  (let [sectionElement (get-section-element section)]
    (let [rows (get-question-elements-under sectionElement)]
      (last (filter #(= (get-question-name %) question) rows)))))

(defn get-sections []
  (let [elems (get-section-header-elements)]
    (for [el elems] (taxi/text el))))

(defn get-questions []
  (let [elems (get-question-elements)]
    (for [el elems] (get-question-name el))))

(defn get-questions-in-section [name]
  (let [elems (get-question-elements-in-section name)]
    (for [el elems] (get-question-name el))))

(defn- set-section-name [headerElement name]
  (let [el (taxi/find-element-under headerElement { :tag "sp-editable-label" })]
    (taxi/click el)
    (let [i (taxi/find-element-under el { :tag :input })]
      (taxi/send-keys i (str name (clj-webdriver.core/key-code :enter))))))

(defn expect-sections [n]
  (common/wait-until #(= (count (get-sections)) n) 5000)
  (rt.test.expects/expect-equals n (count (get-sections))))

(defn expect-section [name]
  (common/wait-until #(.contains (get-sections) (clojure.string/upper-case name)) 5000)
  (rt.test.expects/expect-equals true (.contains (get-sections) (clojure.string/upper-case name))))

(defn expect-questions [section n]
  (expect-section section)
  (common/wait-until #(= (count (get-questions-in-section section)) n) 5000)
  (rt.test.expects/expect-equals n (count (get-questions-in-section section))))

(defn expect-question [section question]
  (expect-section section)
  (common/wait-until #(.contains (get-questions-in-section section) question) 5000)
  (rt.test.expects/expect-equals true (.contains (get-questions-in-section section) question)))

(defn new-section [name]
  (click (str ".headerPanel button[title='New section']"))
  (let [headerElement (last (get-section-header-elements))]
    (set-section-name headerElement name)))

(defn rename-section [oldName newName]
  (let [headerElement (get-section-header-element oldName)]
    (set-section-name headerElement newName)))

(defn delete-section [name]
  (let [headerElement (get-section-header-element name)]
    (let [i (taxi/find-element-under headerElement { :tag :img })]
      (taxi/click i))))
	  
(defn add-break []
  (taxi/click (str ".headerPanel button[title='New break']")))	  

(defn- new-question [question]
  (taxi/click (str ".headerPanel button[title='New question']"))
  (taxi/click (str "ul.contextmenu-view span:contains('" question "')")))

(defn new-choice-question []
  (new-question "Choice Question"))

(defn new-text-question []
  (new-question "Text Question"))

(defn new-numeric-question []
  (new-question "Numeric Question"))

(defn question-allows-notes? [section question]
  (let [questionElement (rt.po.survey/get-question-element-in-section section question)]    
    (let [cb (taxi/find-element-under questionElement {:tag "sp-icon-checkbox" :title "Allow notes"})]
      (let [i (taxi/find-element-under cb {:tag :input})]
        (taxi/selected? i)))))

(defn question-allows-attachments? [section question]
  (let [questionElement (rt.po.survey/get-question-element-in-section section question)]
    (let [cb (taxi/find-element-under questionElement {:tag "sp-icon-checkbox" :title "Allow attachments"})]
      (let [i (taxi/find-element-under cb {:tag :input})]
        (taxi/selected? i)))))

(defn click-question-allows-notes [section question]
  (let [questionElement (rt.po.survey/get-question-element-in-section section question)]
    (let [n (taxi/find-element-under questionElement {:tag "sp-icon-checkbox" :title "Allow notes"})]
      (taxi/click n))))

(defn click-question-allows-attachments [section question]
  (let [questionElement (rt.po.survey/get-question-element-in-section section question)]
    (let [a (taxi/find-element-under questionElement {:tag "sp-icon-checkbox" :title "Allow attachments"})]
      (taxi/click a))))

(defn click-question-properties [section question]
  (let [questionElement (get-question-element-in-section section question)]
    (let [p (taxi/find-element-under questionElement { :tag :img :title "Properties" })]
      (taxi/click p))))

(defn delete-question [section question]
  (let [questionElement (get-question-element-in-section section question)]
    (let [d (taxi/find-element-under questionElement { :tag :img :title "Remove question" })]
      (taxi/click d))))

(defn- get-input-element-with-label [label]
  (let [el (first (filter #(= (taxi/text %) label) (taxi/elements "sp-choice-question-options-editor label")))]
    (taxi/find-element-under el {:tag :input})))

(defn choice-set-new? []
  (taxi/selected? (get-input-element-with-label "New")))

(defn choice-set-use-existing? []
  (taxi/selected? (get-input-element-with-label "Use Existing")))

(defn new-choice-set []
  (taxi/select (get-input-element-with-label "New")))

(defn use-existing-choice-set []
  (taxi/select (get-input-element-with-label "Use Existing")))

(defn- get-choice-set-picker-element []
  (taxi/element "sp-choice-question-options-editor .inlineRelPicker"))

(defn get-existing-choice-set-name []
  (let [el (get-choice-set-picker-element)]
    (let [i (taxi/find-element-under el {:tag :input})]
      (taxi/value i))))

(defn set-existing-choice-set [name]
  (let [el (get-choice-set-picker-element)]
    (let [btn (taxi/find-element-under el {:tag :button})]
      (taxi/click btn)
      (rt.lib.wd-ng/wait-for-angular)))
  (rt.po.edit-form/choose-in-entity-picker-dialog name))

(defn clear-existing-choice-set []
  (let [el (get-choice-set-picker-element)]
    (let [btn (last (taxi/find-elements-under el {:tag :button}))]
      (taxi/click btn))))

(defn- get-choice-set-name-element []
  (taxi/element "sp-choice-question-options-editor input[type='text']"))

(defn get-new-choice-set-name []
  (taxi/value (get-choice-set-name-element)))

(defn set-new-choice-set-name [name]
  (rt.lib.wd/set-input-value (get-choice-set-name-element) name))

(defn- get-choice-option-rows []
  (let [grid (taxi/element ".choice-question-options-grid")]
    (to-list (taxi/find-elements-under grid (clj-webdriver.core/by-class-name "ngRow")))))

(defn- get-choice-option-name-cell [row]
  (taxi/find-element-under row (clj-webdriver.core/by-class-name "col0")))
  
(defn- get-choice-option-description-cell [row]
  (taxi/find-element-under row (clj-webdriver.core/by-class-name "col1")))

(defn- get-choice-option-value-cell [row]
  (taxi/find-element-under row (clj-webdriver.core/by-class-name "col2")))

(defn- get-choice-option-row [name]
  (let [rows (rt.po.survey/get-choice-option-rows)]
    (last (filter #(= name (let [r (let [el (rt.po.survey/get-choice-option-name-cell %)] (clojure.string/trim (taxi/text el)))] r)) rows))))

(defn- get-choice-option-row-info [row]
  (let [c0 (clojure.string/trim (taxi/text (get-choice-option-name-cell row))),
        c1 (clojure.string/trim (taxi/text (get-choice-option-description-cell row))),
        c2 (clojure.string/trim (taxi/text (get-choice-option-value-cell row)))]
    {:name c0 :description c1 :value c2}))

(defn new-choice-option []
  (taxi/click (taxi/element ".choice-question-options button[title='New option']")))
      
(defn remove-selected-choice-option []
  (taxi/click (taxi/element ".choice-question-options button[title='Remove option']")))

(defn move-selected-choice-option-up []
  (taxi/click (taxi/element ".choice-question-options button[title='Move up']")))

(defn move-selected-choice-option-down []
  (taxi/click (taxi/element ".choice-question-options button[title='Move down']")))

(defn select-choice-option-row [name]
  (taxi/click (rt.po.survey/get-choice-option-row name)))

(defn- edit-choice-option-cell [cell value]
  (clj-webdriver.core/->actions taxi/*driver*
                                (clj-webdriver.core/double-click cell))
  (taxi/send-keys (taxi/find-element-under cell {:tag :input}) (str value (clj-webdriver.core/key-code :enter))))

(defn edit-choice-option-name [oldName newName]
  (let [r (rt.po.survey/get-choice-option-row oldName)]
    (let [c (rt.po.survey/get-choice-option-name-cell r)] c
      (rt.po.survey/edit-choice-option-cell c newName))))

(defn edit-choice-option-description [name description]
  (let [r (rt.po.survey/get-choice-option-row name)]
    (let [c (rt.po.survey/get-choice-option-description-cell r)] c
      (rt.po.survey/edit-choice-option-cell c description))))

(defn edit-choice-option-value [name value]
  (let [r (rt.po.survey/get-choice-option-row name)]
    (let [c (rt.po.survey/get-choice-option-value-cell r)] c
      (rt.po.survey/edit-choice-option-cell c value))))

(defn get-choice-option-names []
  (let [info (for [row (rt.po.survey/get-choice-option-rows)] (rt.po.survey/get-choice-option-row-info row))]
    (map :name info)))

(defn get-text-area-question-field []
	(element ".user-survey-text-question textarea[ng-model*=surveyAnswerString]"))	
	
(defn set-text-area-question-field-value [value]
	(set-input-value (get-text-area-question-field) (str value)))

(defn get-number-question-field []
	(element ".user-survey-number-question input[ng-model*=surveyAnswerNumber]"))	

(defn set-number-question-field-value [value]
	(set-input-value (get-number-question-field) (str value)))

(defn get-radio-question-field [option-value]
	(find-element-under (element (str "sp-entity-radio-picker [ng-repeat*=entities]:contains('" option-value "')")) (by-css ".entityRadioInput")))	
	
(defn set-radio-question-field [option-value]
	(click (get-radio-question-field option-value)))	
	
(defn get-survey-progress-value []
	(text (find-element-under (element ".progress-bar") {:tag :b})))
	
(defn click-next-survey-button []
	(click ".user-survey-paging a[ng-click*=pageNext]"))
	
(defn click-back-survey-button []
	(click ".user-survey-paging a[ng-click*=pageBack]"))		

(defn click-survey-save []
	(click ".user-survey-buttons button[ng-click*=save]")
)

(defn click-survey-complete []
	(click ".user-survey-buttons button[ng-click*=complete]")
)
	
(defn click-question-dialog-ok []
  (rt.po.common/click-modal-dialog-button-and-wait (str ".modal-footer button:contains('OK')")))

(defn click-question-dialog-cancel []
  (rt.po.common/click-modal-dialog-button-and-wait (str ".modal-footer button:contains('Cancel')")))
  
  
