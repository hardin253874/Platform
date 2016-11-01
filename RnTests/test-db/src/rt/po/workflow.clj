(ns rt.po.workflow
  (:require [clojure.test :refer :all]
            [rt.lib.wd :refer [find-partial-tree get-repeated-elements set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular apply-angular-expression evaluate-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [drag-n-drop execute-action set-click-to-edit-value get-entities-by-type-alias]]
            [rt.lib.util :refer [load-csv-objects]]
            [rt.po.app :refer [make-app-url]]
            [clj-webdriver.taxi :refer [to text click exists? elements element attribute location *driver*
                                        execute-script]
             :as taxi]
            [clj-webdriver.core :refer [->actions move-to-element move-by-offset click-and-hold release]]
            [rt.po.app :as app]
            [clj-time.format :as tf])
  (:import (org.openqa.selenium TimeoutException)
           (java.util Date)))

(defn design-surface-element []
  (element ".workflow-design-surface svg"))

(defn isDirty?
  "Returns true if the workflow has changes.

  Examples:
    (rt.po.workflow/isDirty?)
  or
    (expect-equals false (rt.po.workflow/isDirty?)

  Preconditions:
    Workflow Builder is open.
  "
  []
  (exists? ".workflow-title .dirty-true"))

(defn open-new
  "Navigate the app to the ___Workflow Builder___ on a new unsaved workflow.

  Examples:
    (rt.po.workflow/open-new)
  "
  []
  (to (make-app-url "/0/workflow/new")))

(defn open
  "Navigate the app to the ___Workflow Builder___ open on an existing workflow.
  The opts map can have either the existing workflow id or name using the :id or :name keyes resp."
  [opts]
  (cond
    (:id opts) (to (make-app-url (str "/" (:id opts) "/workflow")))
    (:name opts) (to (make-app-url (str "/0/workflow?name=" (:name opts))))))

(defn save
  "Save the workflow.

  Will throw if the workflow is dirty afterwards."
  []
  (click ".workflow-app-toolbar [title=Save]")
  (wait-for-angular)
  (when (isDirty?) (throw (Exception. "unexpected workflow dirty after save"))))

(defn close "Close the Workflow Builder." []
  (click ".workflow-app-toolbar [title=Close]"))

(defn open-run-page "Open the page to test run the workflow." []
  (click ".workflow-app-toolbar [title=Run]"))

(defn component-name
  "Return the name of the currently selected entity whether it is the workflow
  itself or a selected activity or sequence."
  []
  (text ".workflow-properties .name-property"))

(defn set-component-name
  "Set the properties form ___name___ input to the given _name_."
  [name]
  (set-click-to-edit-value ".workflow-properties .name-property" name))

(defn toolbox-item-query
  "Find the element with the given name, or the first if many.
  This returns the query as that is what is needed for our drag-and-drop function,
  but ideally we'd like this to take an element.
  FIXME make this find on name, not description."
  [name]
  (if (exists? (str "li[data-item-name='" name "']"))
    (str "li[data-item-name='" name "']")
    (str "li[data-item-name*='" name "']")))

(defn add-toolbox-item
  "Add an Activity or other item from the toolbox to the worklow diagram.
  FIXME - the keys are all screwy - fix the naming"
  ([{name :itemName x :x y :y new-name :name :as m}]
   (add-toolbox-item name x y new-name))
  ([name x y & [new-name]]
   (drag-n-drop (toolbox-item-query name) ".workflow-design-surface [diagram=diagram]" {"x" x "y" y})
   (when new-name (set-component-name new-name))))

(defn click-toolbox-item [name]
  (taxi/click (toolbox-item-query name)))

(defn add-sequence
  ([{:keys [fromName exitName toName]}]
   (add-sequence fromName exitName toName))
  ([from exit to]
   (wait-for-angular)
   (execute-action "addSequence"
                   {"fromName" from "exitName" exit "toName" to})))

(defn diagram-element-by-name
  "Return the diagram element with the given name."
  [name]
  (first (filter #(= (attribute % :data-element-name) name) (elements "[data-element-name]"))))

(defn select-element
  "Select the diagram element with the given name."
  [name]
  #_(click (diagram-element-by-name name))
  (execute-script
    "
    var name = arguments[0];
    var scope = angular.element('.workflow-design-surface svg').scope();
    var diagram = scope.diagramModel;
    var e = _.find(diagram.elements, {name: name});
    if (e) {
      console.log('setting selection to ', e.id);
      diagram.clearSelection();
      diagram.addToSelection([e.id]);
      scope.$apply();
      return true;
    }
    return false;
    "
    [name])
  (wait-for-angular))

(comment
  (let [e (wb/diagram-element-by-name "start")
        a (rt.lib.wd/prepare-script-arg e)]
    (execute-script
      "
      var e = arguments[0];
      return _.pick(angular.element(e).scope().diagram, \"elements\");
      "
      [a]))
  )

(defn move-element
  "Move the element by the given offset."
  [name dx dy]
  (->actions *driver*
             (move-to-element (diagram-element-by-name name))
             (click-and-hold)
             (move-by-offset dx dy)
             (release)))

(defn click-toolbox-item
  "Click on the given toolbox item"
  [name]
  (click (toolbox-item-query name)))

(defn load-workflow-data [path]
  (load-csv-objects path))

(defn set-parameter-expression-string
  "Set the expression of the given parameter for the selected activity."
  [param-name value]
  (let [expr-control (str ".workflow-expression-control:contains(\"" param-name "\")")
        param-expr (attribute expr-control :parameter)
        param-expr (str param-expr ".expression.expressionString = \"" value "\"")]
    (apply-angular-expression expr-control param-expr)))

(defn set-parameter-expression-bool
  "Set the expression of the given bool parameter for the selected activity."
  [param-name value]
  (let [expr-control (str "label:contains(\"" param-name "\") input")
        ]
    (when-not (= (taxi/selected? expr-control) value) (taxi/click expr-control))
    ))

(defn xset-parameter-expression-bool
  "Set the expression of the given bool parameter for the selected activity."
  [param-name value]
  (let [expr-control (str "label:contains(\"" param-name "\") input")
        
        param-expr (taxi/attribute expr-control "ng-model")
        param-expr (str param-expr " = " value )
        ]
    (rt.lib.wd-ng/apply-angular-expression expr-control param-expr)))

(defn- get-expression-control-info
  ([] (->> (taxi/elements ".workflow-properties-form .workflow-expression-control")
           (map #(let [p (taxi/attribute % :parameter)]
                  (hash-map :element %
                            :param-expr p
                            :param-name (evaluate-angular-expression % (str p ".argument.name"))
                            :param-value (evaluate-angular-expression % (str p ".expression.expressionString"))
                            :expr-input (taxi/find-element-under % {:css ".expression-input-control"})
                            :expr-editor (taxi/find-element-under % {:css ".expression-editor-control"})
                            :menu-button (taxi/find-element-under % {:css "span[sp-context-menu]"}))))))
  ([param-name] (let [result (first (filter #(= param-name (:param-name %)) (get-expression-control-info)))]
                  (when-not result
                    (throw (Exception. (str "Failed to find expression for " param-name))))
                  result)))

(defn delete-selected-item []
  (click ".workflow-app-toolbar button[title=Delete]"))

(defn- get-input-argument-elements []
  (get-repeated-elements ".workflow-properties-form [ng-repeat*=inputArguments]"
                         {:name-input      "input[ng-model*='argEntity.name']"
                          :arg-type-select "select[ng-model*='argEntity.id']"
                          :ent-type-input  "input[ng-model*='conformsToType.name']"
                          :ent-type-button "span[ng-click*=showConformsToTypePicker]"}))

(defn get-input-argument-names []
  (->> (get-input-argument-elements)
       (map #(taxi/attribute (:name-input %) "value"))))

(defn get-input-argument
  "Get the set of elements for the given input argument.
  Throws an execption if the argument is not found."
  [arg-name]
  (->> (get-input-argument-elements)
       (filter #(= arg-name (attribute (:name-input %) "value")))
       (#(if (empty? %) (throw (Exception. (str "Failed to find argument:" arg-name))) %))
       first))

(defn get-input-argument-type [arg-name]
  (->> (get-input-argument arg-name)
       :arg-type-select
       taxi/selected-options
       first
       taxi/text))

(defn set-input-argument-type
  "Set the input argument type. Type name is like:
  \"String\" or \"Record\".

  # Example

  (set-input-argument-type \"Input1\" \"Record\")
  "
  [arg-name arg-type]
  (->> (get-input-argument arg-name)
       :arg-type-select
       (#(taxi/select-option % {:text arg-type}))))

(defn get-input-argument-entity-type [arg-name]
  (->> (get-input-argument arg-name)
       :ent-type-input
       (#(taxi/attribute % "value"))))

(defn set-input-argument-entity-type
  "Assuming the given argument is a record type then set the specific type.

  # Example

  (set-input-argument-entity-type \"Input1\" \"Employee\")
  "
  [arg-name ent-type-name]
  ;; open the chooser
  (if-let [button (:ent-type-button (get-input-argument arg-name))]
    (taxi/click button)
    (throw (Exception. (str "Failed to find record type input argument: " arg-name))))
  ;; filter the list
  (rt.lib.wd/set-input-value ".workflow-chooser-view input[ng-model='options.filterText']" ent-type-name)
  ;; choose the element with the given name
  (wait-for-angular)
  (let [elements (taxi/elements (str ".workflow-chooser-view .body table td:contains('" ent-type-name "')"))]
    (when (empty? elements) (throw (Exception. (str "Cannot find entity type:" ent-type-name))))
    (taxi/click (first elements)))
  ;; ok on the picker
  (taxi/click ".workflow-chooser-view .footer button:contains(ok)"))

(defn add-input-argument
  "Add a new input argument to the workflow.

  #Usage

  (rt.po.workflow/add-input-argument)

  "
  [name]
  (click ".workflow-properties-form button[ng-click*=addInputArgument]")
  (-> (elements ".workflow-properties-form [ng-repeat*=inputArguments] input[ng-model*='argEntity.name']")
      last
      (rt.lib.wd/set-input-value name)))

(defn remove-input-argument
  "Remove the nth input argument from the workflow.

  #Usage

  (rt.po.workflow/remove-input-argument n)

  where n is the 0 based index of the argument to remove.

  #Todo

  remove by name rather than index

  "
  ([index] (click (nth (elements ".workflow-properties-form button[ng-click*=removeArgument]") index)))
  ([] (remove-input-argument 0)))


;; deprecated
(defn open-menu-for-expression-control-old [label menu]
  ;; warning - this doesn't work when the labels are not visible, like for the create and update activity configuration forms
  (->> (get-repeated-elements ".workflow-properties-form .workflow-expression-control"
                              {:label        "label"
                               :expr-control ".expression-input-control"
                               :menu-button  "span[sp-context-menu"})
       (map #(taxi/text (:label %)))
       (filter #(= label (taxi/text (:label %))))
       first
       :menu-button
       click)
  (app/choose-context-menu menu))

(defn open-menu-for-expression-control [label menu]
  (click (:menu-button (get-expression-control-info label)))
  (app/choose-context-menu menu)
  (wait-for-angular)
  ;; sleep for a little to allow CSS transition to complete
  (Thread/sleep 500))

(defn choose-expression-record
  "Choose a record in an open workflow record or parameter chooser."
  [record-name]
  ;; filter the list
  (set-input-value ".workflow-chooser-view input[ng-model='options.filterText']" record-name)
  ;; choose the element with the given name
  (Thread/sleep 500)
  (let [elements (taxi/elements (str ".workflow-chooser-view .body table td:contains('" record-name "')"))]
    (when (empty? elements) (throw (Exception. (str "Cannot find entity type:" record-name))))
    (taxi/click (first elements)))
  ;; ok on the picker
  (taxi/click ".workflow-chooser-view .footer button:contains(ok)"))

(defn set-expression-editor-view-value [value]
  (execute-script-on-element "angular.element(arguments[0]).get(0).CodeMirror.setValue(arguments[1]);"
                             ".workflow-expression-editor-view .CodeMirror"
                             value)
  ;; ok on the picker
  (taxi/click ".workflow-expression-editor-view .footer button:contains(ok)"))

(defn get-workflow-id [wf-name]
  (-> (get-entities-by-type-alias "core:workflow"
                                  {:expr {:field "name"} :oper "Equal" :val wf-name})
      first
      :_Id))


;; should move the various "update activity" function to its own namespace

(defn get-update-activity-field-keys []
  (execute-script-on-element "return _.map(angular.element(arguments[0]).scope().groups, 'key');"
                             ".workflow-properties div[ng-controller*=updateEntityActivity]"))

(defn add-update-activity-field-argument [field-name]
  (taxi/click "button[ng-click*=addMemberArg]")
  (let [keys (get-update-activity-field-keys)]
    (open-menu-for-expression-control (last keys) "Select Field")
    (choose-expression-record field-name)))

(defn add-update-activity-relationship-argument [rel-name]
  (taxi/click "button[ng-click*=addMemberArg]")
  (let [keys (get-update-activity-field-keys)]
    (open-menu-for-expression-control (last keys) "Select Relationship")
    (choose-expression-record rel-name)))

(defn safe-date-parse [fmtstr datestr]
  (if (empty? datestr) nil (tf/parse (tf/formatter fmtstr) datestr)))

(defn get-workflow-run-status-records
  "Return all status records for the workflow with the given name.

  Each result map has name, date, status (and also _Id)"
  [wf-name]
  (let [results (->> {:root    {:id      "workflowRun"
                                :related [{:rel       {:id "workflowRunStatus" :as "status"}
                                           :forward   true
                                           :mustExist false}
                                          #_{:rel       {:id "errorLogEntry" :as "log"}
                                           :forward   true
                                           :mustExist false}]}
                      :selects [{:field "name" :displayAs "name"}
                                {:field "runCompletedAt" :displayAs "date"}
								                {:field "createdDate" :displayAs "createdDate"}
                                {:field "name" :on "status" :displayAs "status"}
                                {:field "runStepCounter" :displayAs "step"}
                                #_{:field "description" :on "log" :displayAs "log"}]
                      :conds   [{:expr {:field "name"}
                                 :oper "Equal"
                                 :val  wf-name}]}
                     (rt.lib.wd-rn/run-query)
                     (rt.lib.wd-rn/query-results-as-objects)
                     ;; date's seem to be in the following format... not sure where
                     ;; they are being converted to this local string format, but I suspect
                     ;; its in the structured query web API
                     (map #(assoc % :date (safe-date-parse "dd/MM/yyyy HH:mm:ss" (:date %))
                                    :createdDate (safe-date-parse "dd/MM/yyyy HH:mm:ss" (:createdDate %)))))]
    (println "get-workflow-run-status-records=>" results)
    results))

(defn get-workflow-run-status-record
  "Return the most recent status record for the workflow with the given name.

  Result map has name, date, status (and also _Id)"
  [wf-name]
  (last (sort-by :date (get-workflow-run-status-records wf-name))))

(defn get-workflow-run-status [wf-name]
  (:status (get-workflow-run-status-record wf-name)))

(defn wait-for-workflow-run-status [wf-name status]
  (try
    (taxi/wait-until #(= status (:status (get-workflow-run-status-record wf-name))) 90000)
    status
    (catch TimeoutException _
      (println "Timed out waiting for the expected workflow run status.")
      nil)))

(defn set-run-option [label value]
  (let [e (element (str ".run-options input[type=checkbox][name*='" label "']"))]
    (when-not (= (taxi/selected? e) value) (taxi/click e))))

(defn set-run-input-text [label value]
  (let [tr (taxi/element (str "tr:has(.run-argument-label):contains(\"" label "\")"))]
    (let [td (taxi/find-element-under tr {:css ".run-argument-value"})]
      (let [i (taxi/find-element-under td {:tag :input})]
        (rt.lib.wd/set-input-value i (str value))))))

(defn set-run-input-select [label value]
  (let [tr (taxi/element (str "tr:has(.run-argument-label):contains(\"" label "\")"))]
    (let [td (taxi/find-element-under tr {:css ".run-argument-value"})]
      (let [i (taxi/find-element-under td {:tag :select})]
        (let [o (first (filter #(.contains (taxi/text %) (str value)) (taxi/find-elements-under i {:tag :option})))]
              (let [n (taxi/attribute o :index)]
                (taxi/select-option i {:index (read-string n)})))))))

(defn run "Run the workflow" []
  (taxi/click ".workflow-run-view button:contains(Run)"))


;; functions to move to a namespace for the gateway/switch

(defn- get-exit-controls
  [] (->> (get-repeated-elements ".workflow-properties-form .workflow-expression-control"
                                 {:name-input    "input[ng-model='e.name']"
                                  :expr-editor   ".expression-editor-control"
                                  :menu-button   "span[sp-context-menu]"
                                  :remove-button "button[ng-click*=removeExit"})
          (map #(assoc % :exit-name (taxi/attribute (:name-input %) "value")))))

(defn get-exit-controls-for-exit [exit-name]
  (first (filter #(= exit-name (:exit-name %)) (get-exit-controls))))

(defn set-exit-expression [exit-name expr]
  (taxi/click (:menu-button (get-exit-controls-for-exit exit-name)))
  ;; sleep for a little to allow CSS transition to complete
  (Thread/sleep 500)
  (set-expression-editor-view-value expr))

(comment

  ;; standard "get a console up" sequence
  (do
    (require '[rt.test.core :refer [*tc* *test-context*]])

    (rt.app/setup-environment {})

    (do
      (def tc (merge {:tenant   "EDC"
                      :username "Administrator"
                      :password "tacoT0wn"}
                     {:target :chrome}
                     {:wf-name      (rt.lib.util/make-test-name "RT-Workflow-Create-etal")
                      :section-name (rt.lib.util/make-test-name "RT-Section")
                      :report-name  (rt.lib.util/make-test-name "RT-Report")}
                     ))
      (alter-var-root (var *test-context*) (constantly tc))
      (alter-var-root (var *tc*) (constantly tc)))

    (rt.scripts.common/start-app-and-login))

  (do

    (do
      (comment "Adding a new lookup to an existing object")
      (rt.po.app/enable-config-mode)
      (rt.po.app/enable-app-toolbox)
      (rt.po.app-toolbox/set-application-filter "Test Solution")
      (rt.po.app-toolbox/set-object-filter "pizza")
      (rt.po.app-toolbox/choose-object-menu "Pizza" "Modify")
      (rt.po.form-builder/add-field-to-container "Lookup" "") ;; this defaults to adding a AA_Employee
      (rt.po.form-builder/run-lookup-properties-dialog "AA_Employee" {:name "AA_Drink" :display-name "Drink" :lookup-type "AA_Drink"})
      (rt.po.form-builder/save)
      (rt.po.form-builder/close))

    (do
      (comment "Creating a report to use in our test later")
      (rt.po.app/navigate-to-item "Home" "")
      (rt.po.app/add-section (:section-name *tc*) "")
      (rt.po.app/add-report (:section-name *tc*))
      (rt.po.report-new/set-name (:report-name *tc*))
      (rt.po.report-new/set-report-base-on "Pizza")
      (rt.po.report-new/click-ok)
      (rt.po.report-builder/select-field-checkboxes "Description" true true)
      (rt.po.report-builder/select-field-checkboxes "Kilojoules" true true)
      (rt.po.report-builder/select-field-checkboxes "AA_Drink" true true)
      (rt.po.report-builder/select-field-checkboxes "Pizza Image" true true)
      (rt.po.report-builder/save)
      (rt.po.report-builder/close))

    ;; create a new workflow...
    (do
      (comment "Create the workflow")
      (rt.po.app/navigate-to-item "Administration" "Workflows/Manage Workflows")
      (rt.po.report-view/open-action-menu)
      (rt.po.app/choose-context-menu "New")
      (rt.po.workflow/set-component-name (:wf-name *tc*))
      (rt.po.workflow/save)
      (rt.po.workflow/add-input-argument "Pizza")
      (rt.po.workflow/set-input-argument-type "Pizza" "Record Argument")
      (rt.po.workflow/set-input-argument-entity-type "Pizza" "Pizza")
      (rt.po.workflow/select-element "start")
      (rt.po.workflow/click-toolbox-item "Gateway")
      (rt.po.workflow/select-element "Gateway")
      (rt.po.workflow/set-exit-expression "exit" "[Pizza].[AA_Drink] is null")
      (rt.po.workflow/select-element "Gateway")
      (rt.po.workflow/click-toolbox-item "Create")
      (rt.po.workflow/select-element "Create")
      (rt.po.workflow/open-menu-for-expression-control "Object" "Object")
      (rt.po.workflow/choose-expression-record "AA_Drink")
      (rt.po.workflow/open-menu-for-expression-control "1_value" "Calculation")
      (rt.po.workflow/set-expression-editor-view-value "[Pizza] + ' Drink'")
      (rt.po.workflow/select-element "Create")
      (rt.po.workflow/click-toolbox-item "Update")
      (rt.po.workflow/select-element "Update")
      (rt.po.workflow/set-parameter-expression-string "Record" "[Pizza]")
      (rt.po.workflow/add-update-activity-relationship-argument "AA_Drink")
      (rt.po.workflow/open-menu-for-expression-control "1_value_1" "Parameter")
      (rt.po.workflow/choose-expression-record "Create.Record")
      (rt.po.workflow/save)
      (rt.po.workflow/close))

    #_(do
      (rt.po.workflow/open-run-page)
      (rt.po.workflow/set-run-option "Enable trace" true)
      (rt.po.workflow/set-run-option "Open follow-up tasks" false)
      (rt.po.workflow/run)
      (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Completed")
      (rt.po.workflow/close))

    ;; set up the trigger

    (do
      (comment "Set up a trigger for the workflow")
      (rt.po.app/navigate-to-item "Administration" "Workflows/Trigger on Create")
      (rt.po.report-view/open-action-menu)
      (rt.po.app/choose-context-menu "New")
      (rt.po.edit-form/set-string-field-value "Name" (str (:wf-name *tc*) "-trigger"))
      (rt.po.edit-form/set-choice-value "Triggered on" "Create Or Update")
      (rt.po.edit-form/set-lookup-value "Object" "Pizza")
      (rt.po.edit-form/set-lookup-value "Workflow to run" (:wf-name *tc*))
      (rt.po.view-form/open-tab-action-menu "Fields to Trigger on" "Add Existing")
      (rt.po.edit-form/choose-in-entity-picker-dialog "Kilojoules")
      (rt.po.edit-form/save))

    ;; now we edit a Student

    (do
      (comment "Do some edits and creates and verify the workflow ran.")
      (rt.po.app/navigate-to-item "Home" (str (:section-name *tc*) "/" (:report-name *tc*)))
      (rt.po.report-view/right-click-row-by-text "Muffuletta")
      (rt.po.app/choose-context-menu "Edit")
      (rt.po.edit-form/set-number-field-value "Kilojoules" ((fnil inc 1000) (rt.po.edit-form/number-field-value "Kilojoules")))
      (rt.po.edit-form/save)
      (rt.test.expects/expect-equals "Completed" (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Completed"))
      (rt.test.expects/expect-equals 1 (count (rt.po.workflow/get-workflow-run-status-records (:wf-name *tc*))))
      (rt.po.report-view/right-click-row-by-text "BBQ Chicken")
      (rt.po.app/choose-context-menu "Edit")
      (rt.po.edit-form/set-number-field-value "Kilojoules" ((fnil inc 1000) (rt.po.edit-form/number-field-value "Kilojoules")))
      (rt.po.edit-form/save)
      (rt.test.expects/expect-equals "Completed" (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Completed"))
      (rt.test.expects/expect-equals 2 (count (rt.po.workflow/get-workflow-run-status-records (:wf-name *tc*))))
      (rt.po.report-view/refresh-now)
      (rt.test.expects/expect-equals "Muffuletta Pizza Drink" (rt.po.report-view/get-value-for-row-and-column "Muffuletta Pizza" "AA_Drink"))
      (rt.test.expects/expect-equals "BBQ Chicken Pizza Drink" (rt.po.report-view/get-value-for-row-and-column "BBQ Chicken Pizza" "AA_Drink"))
      (rt.test.expects/expect (empty? (rt.po.report-view/get-value-for-row-and-column "Bacon Pizza" "AA_Drink")))
      (rt.po.report-view/open-action-menu)
      (rt.po.app/choose-context-menu "New")
      (rt.po.edit-form/set-string-field-value "Name" "Bacon Banana Pizza")
      (rt.po.edit-form/set-number-field-value "Kilojoules" ((fnil inc 1000) (rt.po.edit-form/number-field-value "Kilojoules")))
      (rt.po.edit-form/upload-image "Picture" (rt.po.common/get-data-file-path "PizzaBaconBanana.png"))
      (rt.po.edit-form/save)
      (rt.test.expects/expect-equals "Completed" (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Completed"))
      (rt.test.expects/expect-equals 3 (count (rt.po.workflow/get-workflow-run-status-records (:wf-name *tc*))))
      (rt.po.report-view/refresh-now)
      (rt.test.expects/expect-equals "Bacon Banana Pizza Drink" (rt.po.report-view/get-value-for-row-and-column "Bacon Banana Pizza" "AA_Drink"))))


  ;; wait for the workflow to start

  (rt.test.expects/expect-equals "Paused" (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Paused"))

  ;; now we check the workflow started
  ;; checking that a "approve" thingy is at the top of the form....

  (do
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student")
    (rt.po.report-view/double-click-row-by-text "Selma Terrell")
    (rt.test.expects/expect-equals "Approve" (rt.po.view-form/get-task-name))
    (rt.test.expects/expect-equals '("Approve" "Reject") (rt.po.view-form/get-task-actions))
    (rt.po.view-form/choose-task-action "Approve"))

  ;; and then the events our Log activity should have created

  (rt.test.expects/expect-equals "Paused" (rt.po.workflow/wait-for-workflow-run-status (:wf-name *tc*) "Completed"))

  (do
    (rt.po.app/navigate-to-item "Administration" "Tools/Event Log")
    (rt.test.expects/expect (some #(re-find (re-pattern (:wf-name *tc*)) %) (rt.po.report-view/get-loaded-grid-values nil 2))))

  ;; misc debugging

  (rt.po.workflow/get-input-argument-type "Input")
  (rt.po.workflow/get-input-argument-entity-type "Input")

  (:ent-type-button (first (get-input-argument-elements)))
  (elements ".workflow-properties-form [ng-repeat*=inputArguments]")
  (elements "[ng-click*=showConformsToTypePicker]")

  (taxi/html (element ".workflow-properties-form [ng-repeat*=inputArguments] span[ng-click*=showConformsToTypePicker]"))
  (taxi/html (element ".workflow-properties-form [ng-repeat*=inputArguments] span[ng-click*=showConformsToTypePicker]"))

  (-> (element ".workflow-chooser-view")
      (taxi/find-elements-under {:css "tr[ng-repeat='r in filteredData'] td:nth-child(2)"})
      first
      (taxi/html))

  ;; get index of Name column in the given table
  (->> (taxi/find-table-row ".workflow-chooser-view .body table" 0)
       (map taxi/text)
       (#(.indexOf % "Name")))

  (->> (get-repeated-elements ".workflow-properties-form .workflow-expression-control"
                              {:label        "label"
                               :expr-control ".expression-input-control"
                               :menu-button  "span[sp-context-menu"})
       (filter #(= "Form" (taxi/text (:label %))))
       first
       :menu-button
       click
       )

  (->> (get-repeated-elements "sp-vertical-stack-container-control"
                              {:header-div   ".structure-control-header"
                               :controls-on-form "sp-control-on-form"})
       (filter #(= "HEADER AREA" (taxi/text (:header-div %))))
       first)


  (rt.po.app/choose-context-menu "Record")

  (->> (get-expression-control-info)
       (map #(taxi/text (:label %))))

  (->> (get-repeated-elements ".workflow-properties-form .workflow-expression-control"
                              {:name-input    "input[ng-model='e.name']"
                               :expr-editor   ".expression-editor-control"
                               :menu-button   "span[sp-context-menu]"
                               :remove-button "button[ng-click*=removeExit"})
       (map #(assoc % :exit-name (taxi/attribute (:name-input %) "value"))))

  (->> (get-exit-controls-for-exit "exit")
       :expr-editor
       )

  )