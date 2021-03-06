(ns rn.app
  (require [rn.services.entity :refer [get-entity-id-for-name delete-entity-for-name]]
           [rt.po.app :refer [make-app-url]]
           [rt.po.common :refer [wait-until exists-present?]]
           [rt.lib.wd-ng :refer [wait-for-angular]]
           [rt.lib.wd-rn :refer [report-grid-scroll-to-top notify-loggedin]]
           [clj-webdriver.taxi :as taxi]
           [rt.lib.wd :refer [wait-until-displayed]]
           [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(defn- build-url [state-name id params]
  (make-app-url (str "/" id "/" state-name (and params (str "?" params)))))

(defn open-page [{:keys [name type-alias app-name folder-name page-name params]}]
  (-> (get-entity-id-for-name name type-alias {:app-name app-name :folder-name folder-name})
      (#(if-not % (throw (Exception. (format "%s %s not found" type-alias name))) %))
      (#(build-url page-name % params))
      (taxi/to))
  (wait-for-angular))

(defn open-board [name & [options]]
  (open-page (merge {:name name} options {:page-name "board" :type-alias "board"}))
  (wait-until-displayed ".board-view .board-header"))

(defn open-report [name & [options]]
  (open-page (merge {:name name} options {:page-name "report" :type-alias "report"}))
  (wait-until-displayed ".report-view .report-header")
  (report-grid-scroll-to-top))

(defn open-report-builder [name & [options]]
  (open-page (merge {:name name} options {:page-name "reportBuilder" :type-alias "report"}))
  (report-grid-scroll-to-top))

(defn open-form [name & [{:keys [form-name] :as options}]]
  (let [form-id (and form-name (-> (get-entity-id-for-name form-name "console:customEditForm")
                                   (#(if-not % (throw (Exception. (format "form %s not found" form-name))) %))))
        params (and form-id (str "formId=" form-id))]
    (open-page (merge {:name name} options {:page-name "viewForm" :type-alias "resource" :params params}))))

(defn open-form-builder [name & [options]]
  (open-page (merge {:name name} options {:page-name "formBuilder" :type-alias "console:customEditForm"}))
  (wait-until #(exists-present? (str ".sp-Edit-Form-Heading:contains('" name "')"))))

(defn open-security [name & [options]]
  (let [page-options (condp = name
                       "Navigation Access" {:page-name "securityCustomiseUI" :type-alias "securityCustomiseUIPageType"}
                       "Record Access" {:page-name "securityQueries" :type-alias "securityQueriesPageType"}
                       "User Accounts" {:page-name "report" :type-alias "report"}
                       "User Roles" {:page-name "report" :type-alias "report"}
                       "Audit Log" {:page-name "report" :type-alias "report"}
                       "Audit Log Settings" {:page-name "viewForm" :type-alias "auditLogSettings" :app-name "ReadiNow Core"}
                       "Password Policy" {:page-name "viewForm" :type-alias "passwordPolicy" :app-name "ReadiNow Console"}
                       {:page-name "securityQueries" :type-alias "securityQueriesPageType"})
        options (merge {:name name} options page-options)]
    (open-page options)))

(defn open-admin [name & [options]]
  (let [page-options (condp = name
                       "Email Settings" {:page-name "viewForm" :type-alias "tenantEmailSetting" :app-name "ReadiNow Console" :folder-name "Settings"}
                       "General Settings" {:page-name "viewForm" :type-alias "tenantGeneralSettings" :app-name "ReadiNow Console" :folder-name "Settings"}
                       "Application Library" {:page-name "appManager" :type-alias "console:staticPage" :app-name "ReadiNow Console" :folder-name "Applications"}
                       {})
        options (merge {:name name :page-name "report" :type-alias "report" :app-name "ReadiNow Core Data" :folder-name "Resources"}
                       options page-options)]
    (open-page options)))

(defn delete-report [name & [options]]
  (taxi/to (make-app-url "/"))
  (delete-entity-for-name name "report" options)
  (notify-loggedin))

(defn delete-form [name & [options]]
  (taxi/to (make-app-url "/"))
  (delete-entity-for-name name "form" options)
  (notify-loggedin))


(comment
  (get-entity-id-for-name "Navigation Access" "resource")
  (build-url "report" %)

  (let [name "Staff Report"
        {:keys [app-name folder-name]} {:app-name "Foster University", :folder-name "Reports"}]
    (->> (rn.services.entity/get-entity-id-for-name name "report" {:app-name app-name :folder-name folder-name})
         (build-url "report")))

  (rn.app/open-report "Staff Report" {:app-name "Foster University" :folder-name "Reports"})
  (rn.app/open-report "Alt Staff Report" {:app-name "Foster University" :folder-name "Reports"})
  (rn.app/open-report-builder "Staff Report" {:app-name "Foster University" :folder-name "Reports"})
  (rn.app/open-report-builder "Alt Staff Report" {:app-name "Foster University" :folder-name "Reports"})
  (rn.app/open-form "Ina Harmon" {:app-name "Foster University Data"})
  (rn.app/open-form "Ina Harmon" {:form-name "Alt Staff Form" :app-name "Foster University Data"})
  (rn.app/open-form-builder "Staff Form" {:app-name "Foster University"})
  (rn.app/open-form-builder "Alt Staff Form" {:app-name "Foster University"})

  (doall (map open-security ["Navigation Access" "Audit Log Settings" "Record Access"
                             "User Accounts" "User Roles" "Audit Log" "Password Policy"]))

  (doall (map open-admin ["Objects" "Relationships" "Reports" "Charts" "Forms"
                          "Hierarchies" "Screens" "Choice Fields" "Resource Keys"
                          "Email Settings"]))

  (doall (map open-admin ["Email Settings" "General Settings"]))
  )