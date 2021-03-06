(ns rt.po.app
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

;; shadow taxi click with debug-click
(def click debug-click)

(defn make-app-url [path]
  (str (get-app-url) "/sp/#/" (get-tenant) path))

(defn setup-for-testing
  "Put the app into test mode"
  []
  (execute-script " document.spAppKeyhole.setTestMode(true); " []))

(defn reset-app
  "Log the application out, clearing any auth cookies."
  []
  (execute-script "if (document.spAppKeyhole) document.spAppKeyhole.logout();" [])
  (refresh))

;; to think about what makes sense to return from these functions
;; that are driving the external client app...

(defn login-basic-auth
  "Log the app in using basic auth and the test auth account."
  []
  ;; todo - use the settings to get tenant, username and password
  (let [token (str->base64 "EDC\\Administrator:tacoT0wn")]
    (setup-for-testing)
    (execute-script
      "var ident = { tenant: arguments[0], username: arguments[1] };
       var token = arguments[2];
       document.spAppKeyhole.setTestMode(true, ident);
       if (token) document.spAppKeyhole.setAuthorization(token);"
      ["EDC" "Administrator" token])))

(defn get-client-app-settings []
  (execute-script
    "
    var body = angular.element('body');
    var a = body.injector().get('spAppSettings');
    a.__spTestCulture = body.scope().$root.__spTestCulture;
    a.__globalizeKeys = _.keys(Globalize.cultures);
    return JSON.stringify(a);
    "))

(defn login
  ([] (let [{:keys [username password]} (get-settings)]
        (login username password)))
  ([username password]
   (info "logging in with " username " and *******")
   (wait-until-displayed {:css "form[name='spForm'] input[name='username']"} 50000)
   (when-not (exists? {:css "form[name='spForm']"})
     (throw (Exception. "Cannot see login form")))
   (setup-for-testing)
   (timeit "form-submit"
           (do (set-input-value {:css "form[name='spForm'] input[name='username']"} username)
               (set-input-value {:css "form[name='spForm'] input[type='password']"} password)
               (click {:css "form[name='spForm'] button[type='submit']"})))
   (rt.po.common/wait-until #(= username (:username (get-authenticated-identity))) 5000)
   (let [identity (get-authenticated-identity)]
     (when (not= (some-> (:username identity) string/lower-case)
                 (string/lower-case username))
       #_(warn "WARNING:" (str "Logged in but authenticated identity does not match: expecting '"
                               username "', actual:'" (:username identity) "'"))
       (throw (Exception. (str "Logged in but authenticated identity does not match: expecting '"
                               username "', actual:'" (:username identity) "'")))))
   (info "Client App Settings (after login):" (get-client-app-settings))
   (to (make-app-url "/"))))

(defn login-direct
  ([] (let [{:keys [username password]} (get-settings)]
        (login-direct username password)))
  ([username password & [skip-setup?]]
   (info "logging in with " username " and " password)
   (rt.po.common/wait-until #(rt.po.common/exists-present? "form[name='spForm'] input[name='username']") 60000)
   (when-not (exists? {:css "form[name='spForm']"})
     (throw (Exception. "Cannot see login form")))
	(if-not skip-setup?
    (setup-for-testing))
	
   (taxi/execute-script
     (format "
     var body = angular.element('body');
     var a = body.injector().get('spLoginService');
     a.readiNowLogin(\"%s\", \"%s\", \"%s\", false);
     return JSON.stringify('');
     " (:tenant (get-settings)) username password))
   (wait-for-angular)
   (rt.po.common/wait-until #(= username (:username (get-authenticated-identity))) 60000)
   (let [identity (get-authenticated-identity)]
     (when (not= (some-> (:username identity) string/lower-case)
                 (string/lower-case username))
       #_(warn "WARNING:" (str "Logged in but authenticated identity does not match: expecting '"
                               username "', actual:'" (:username identity) "'"))
       (throw (Exception. (str "Logged in but authenticated identity does not match (probably a TIMEOUT): expecting '"
                               username "', actual:'" (:username identity) "'")))))
   (info "Client App Settings (after login):" (get-client-app-settings))
   (to (make-app-url "/"))))

(defn logout []
  (click ".btn-login")
  (click ".signout a[ng-click*=logout]"))

(defn logout-direct []
  (taxi/execute-script
    "
    var body = angular.element('body');
    var a = body.injector().get('spLoginService');
    a.logout();
    return JSON.stringify('');
    "))

(defn navigate-to-landing-home-mobile []
	(if (exists? "div[ng-if='spMobileContext.isMobile'] a[ng-click*='toggleNavigation']")
	(do
		(click "div[ng-if='spMobileContext.isMobile'] a[ng-click*='toggleNavigation']")
		(wait-for-angular)
		(wait-for-jq "a[ui-sref='landinghome']")
		(click "a[ui-sref='landinghome']"))))

(defn navigate-to-landing-home []
	(if (exists? ".navbar-brand img[ng-src*='assets/images/logo_RN_main.png']")
		(click ".navbar-brand img[ng-src*='assets/images/logo_RN_main.png']")))

(defn assert-on-launcher-page []
  (wait-for-jq ".app-launcher-tile")
  (if-not (exists? ".app-launcher-tile")
    (throw (Exception. "Expected to be on app launcher page. But we aren't."))
    true))

(defn app-tile-exists? [app-name]
  (taxi/exists? (str ".app-launcher-tile img[alt='" app-name "']")))

(defn get-client-version
  "Get the client version number as defined in the footer of the client app."
  []
  (attribute "body" "data-client-version"))

(defn get-server-version
  "Get the server version number as defined in the footer of the client app.
  Only exists after we have logged in."
  []
  (attribute "body" "data-server-version"))

(defn open-sp-client "Open the application with a default URL" []
  (to (make-app-url "/home")))

(defn choose-modal-ok []
  (click-ok))

(defn- menu-item-selector [name]
  (str "ul.contextmenu-view a.menuItem:contains(" name ")"))

(defn context-menu-exists? [name]
  (wait-for-jq "ul.contextmenu-view")
  (exists? (menu-item-selector name)))

(defn choose-context-menu
  "Choose the given menu item. Assumes the menu is already open.

  FIXME - the menu item lookup may find the wrong item if another item contains the same item text."
  [name]
  (let [s (menu-item-selector name)]
    ;; here we need to wait for the control... waiting for angular isn't sufficient
    (if (try
          (wait-for-jq s 5000)
          true
          (catch Exception _ false))
      (click s)
      (throw (Exception. (str "Failed to find menu or menu item: " name))))
    ;; most of the time the menu action causes things worth waiting on
    (wait-for-angular)))

(defn top-menu-name
  "Get the name of the currently selected top menu, aka application."
  {:rt-query true}
  []
  (let [q "span.appMenuItem"]
    (when (exists-present? q)
      (string/trim (text q)))))

(defn get-authenticated-tenant []
  (:tenant (get-authenticated-identity)))

(defn get-authenticated-user []
  (:username (get-authenticated-identity)))

(defn assert-app-name [app-name]
  (wait-for-jq "span.appMenuItem")
  (let [current-name (top-menu-name)]
    (when-not (= app-name current-name)
      (throw (Exception. (str "Expected to be at " app-name " but instead at " current-name))))))

(defn delete-app
  "With config mode on, hover over the app in combo box and click on config button."
  [app-name]
  (wait-for-jq ".application-menu a")
  (click ".application-menu a")
  (if-let [q (element (str "a.appMenuItem:contains(" app-name ")"))]
    (do
      (->actions taxi/*driver* (move-to-element q))
      (taxi/click (taxi/find-element-under q {:tag :button, :css ".nav-config-panel-button"}))
      (choose-context-menu "Delete Application")
      (rt.po.app/choose-modal-ok)
      )
    (throw (Exception. (str "Cannot find menu item for: " app-name)))))

(defn navigate-to
  "Use the application menu to navigate to an appication."
  [app-name]
  (wait-for-jq ".application-menu a")
  (click ".application-menu a")
  (if-let [q (element (str "a.appMenuItem:contains('" app-name "')"))]
    (click q)
    (throw (Exception. (str "Cannot find menu item for: " app-name))))
  (assert-app-name app-name))

(defn navigate-via-tiles [app-name]
  (if-let [q (element (str ".app-launcher-tile img[alt='" app-name "']"))]
    (click q)
    (throw (Exception. (str "Cannot find app tile for: " app-name))))
  (assert-app-name app-name))

(defn select-app-tab
  "Select the application tab if it exists, otherwise throw."
  [text]
  (click (str ".application-tabs li:contains(\"" text "\")")))

(defn get-app-tabs
  "Get the application tab names."
  []
  (mapv #(text %) (elements ".application-tabs li")))

(defn node-in-nav-tree-exists? [name]
  (exists-present? (str ".client-nav-panel div[class*=nav-type] :contains(" name ")")))

(defn node-icon-in-nav-tree-exists? [name]
  (exists-present? (str ".client-nav-panel div[class*=nav-type] :contains(" name ") img")))

(defn get-node-icon-background-colour [name]
  (rt.po.common/get-colour-from-css-colour
    (rt.po.common/get-element-css-value
      (str ".client-nav-panel div[class*=nav-type] :contains(" name ") img") "background-color")))

(defn open-nav-tree-node
  "Open the given navigation tree node. Attempts to check if already
  open and will leave it alone if it is.

  FIXME suffers from the contains text issue - discussed elsewhere"
  [name]
  (let [s (str ".client-nav-panel div[class*=nav-type] :contains(" name ")")]
    ;; not sure why I need the wait... to be investigated
    (wait-for-jq s 10000)
    (if (has-class s "closed")
      (click s))
    (when-not (has-class s "open")
      (throw (Exception. (str "Failed to open tree node: " name))))))

(defn open-nav-tree-node-by-name
  "Open the given navigation tree node. Attempts to check if already
  open and will leave it alone if it is.

  FIXME suffers from the contains text issue - discussed elsewhere"
  [name]
  (let [s (str ".client-nav-panel div[class*=nav-type] [title='" name "']")]
    ;; not sure why I need the wait... to be investigated
    (wait-for-jq s 10000)
    (if (has-class s "closed")
      (click s))
    (when-not (has-class s "open")
      (throw (Exception. (str "Failed to open tree node: " name))))))

;; TODO - reconcile this with open-nav-tree-node above
(defn select-navigator-item
  "Choose the first loaded navigation item that contains the given name.

  FIXME issue with mult items containing the name"
  [name]

  (let [s (str "a.nav-link:contains(" name ")")]

    ;; *sometimes* the element isn't quite ready, so wait a bit
    (wait-for-jq s 10000)

    ;; Don't click on sections that are already expanded
    (when-not (exists? (str s ".open"))
      (click s)
      (wait-for-angular))))

(defn select-non-toggle-navigator-item

  "Choose the nth loaded navigation item that contains the given name."
  [name index]

  (click (nth (elements (str "a.nav-link:contains(" name ")")) index))
  (wait-for-angular))

(defn- do-if-present [f selector]
  (let [e (element selector)]
    (when (and e (exists? e) (present? e))
      (f e))
    e))

(defn config-mode-visible?
  "True if the config mode icon is visible."
  []
  (or (exists-present? ".navConfigButton-view") (exists-present? ".navConfigButton-edit")))

(defn enable-config-mode
  "Turn on configure mode."
  []
  (wait-for-angular)
  (do-if-present click {:css ".navConfigButton-view"}))

(defn disable-config-mode
  "Turn off configuration mode.

  Example:
  =========

  (disable-config-mode)"
  []
  (wait-for-angular)
  (do-if-present click {:css ".navConfigButton-edit"}))


(defn app-toolbox-visible?
  "True if the admin toolbox icon is visible."
  []
  (or (exists-present? ".navAdminToolboxButton-view") (exists-present? ".navAdminToolboxButton-edit")))

(defn enable-app-toolbox
  "Choose the admin toolbox."
  []
  (enable-config-mode)
  (do-if-present click {:css ".sp-tool-box .navAdminToolboxButton-view"})
  (wait-for-angular)
  (rt.lib.wd/wait-for-jq ".fb-toolbox-section" 10000))

(defn disable-app-toolbox
  "Disable the admin toolbox.

  Warning - if the client has never selected an Application
  this is ineffective."
  []
  (do-if-present click {:css ".sp-tool-box .navAdminToolboxButton-edit"})
  (wait-for-angular))

(defn click-nav-builder-tool "Turn on the nav builder" []
  (click ".nb-toolbox-control")
  (rt.po.common/wait-until #(rt.po.common/exists-present? ".popover .popover-inner") 10000))

(defn add-new-nav-item [type parent]
  (let [selector (if (empty? parent)
                   ".client-nav-panel"
                   (str ".client-nav-panel div:contains('" parent "')"))]
    (enable-config-mode)
    (click-nav-builder-tool)
    (drag-n-drop (str ".nb-entry:contains('" type "')") selector)
    (wait-for-angular)))

(defn add-new-named-nav-item [type dlg-selector name parent]
  (add-new-nav-item type parent)
  (wait-for-angular)
  (let [name-input (str dlg-selector " input[name=fieldValue]")]
    (input-clear-all name-input)
    (input-text name-input name)
    (click-ok)))

(def add-section (partial add-new-named-nav-item "New Section" ".sp-navigation-element-dialog"))
(def add-folder (partial add-new-named-nav-item "New Folder" ".sp-navigation-element-dialog"))
(def add-screen (partial add-new-named-nav-item "New Screen" ".sp-navigation-element-dialog"))
(def add-chart (partial add-new-nav-item "New Chart"))      ;; pass parent name or empty
(def add-document-folder (partial add-new-named-nav-item "New Document Folder" ".sp-navigation-element-dialog"))
(def add-object (partial add-new-named-nav-item "New Object" ".sp-new-type-dialog"))
(def add-board (partial add-new-nav-item "New Board"))
(def add-private-content-section (partial add-new-named-nav-item "New Personal Section" ".sp-navigation-element-dialog"))

(defn new-nav-item-visible? [name]
  (let [q (str ".nb-entry:contains('" name "')")]
    (and (exists? q) (displayed? q))))

(def ^{:doc
       "Open new report dialog window by dropping into left navigation section.

       parent-name is the section or folder that gets the report. Or pass an empty string.

       # Syntax
           (rt.po.app/add-report)

       # Examples
           (rt.po.app/add-report \"Folder1\")

       # Prerequisites
           Configure mode"
       }
add-report (partial add-new-nav-item "New Report"))

(defn add-existing-report
  ([report parent]
   (add-new-nav-item "Existing Report" parent)
   (wait-for-angular)
   (if-let [el (->> (elements ".entityReportPickerDialog .sp-search-control input")
                    (filter displayed?) first)]
     (do
       (set-input-value el report)
       (Thread/sleep 500)
       (wait-for-angular))
     (throw (Exception. "Cannot find displayed search box")))
   (select-picker-dialog-grid-row-by-text report)
   (click-ok))
  ([report]
   (add-existing-report report "")))

(defn add-existing-chart
  ([chart parent]
   (add-new-nav-item "Existing Chart" parent)
   (wait-for-angular)
   (if-let [el (->> (elements ".entityReportPickerDialog .sp-search-control input")
                    (filter displayed?) first)]
     (do
       (set-input-value el chart)
       (Thread/sleep 500)
       (wait-for-angular))
     (throw (Exception. "Cannot find displayed search box")))
   (select-picker-dialog-grid-row-by-text chart)
   (click-ok))
  ([chart]
   (add-existing-chart chart "")))

(defn remove-nav-item [name]
  ;;TODO - issues with this relating to using jq contains and it matching the wrong names
  ;; and it deleting the wrong items.... need to fix this issue generally


  ; need a wait before executing script
  (wait-for-angular)
  ;; the button to show the menu only happens when the item is hovered over
  ;; and trying to do this doesn't work reliably... so using client side script.
  (let [q (str ".client-nav-panel div[class*=nav-type]:contains('" name "') button.nav-config-panel-button")]
    (execute-script
      "var elems = angular.element(arguments[0]);
      elems[0].click();
      return elems.length;"
      [q]))
  (let [q ".contextmenu-view a:contains('Remove')"]
    (if (and (exists? q) (displayed? q))
      (do (click q)
          )
      (error "failed to find remove menu"))))

(defn modify-nav-item [name]
  ;;TODO - issues with this relating to using jq contains and it matching the wrong names
  ;; and it deleting the wrong items.... need to fix this issue generally


  ; need a wait before executing script
  (wait-for-angular)
  ;; the button to show the menu only happens when the item is hovered over
  ;; and trying to do this doesn't work reliably... so using client side script.
  (let [q (str ".client-nav-panel div[class*=nav-type]:contains('" name "') button.nav-config-panel-button")]
    (execute-script
      "var elems = angular.element(arguments[0]);
      elems[0].click();
      return elems.length;"
      [q]))
  (let [q ".contextmenu-view a:contains('Modify')"]
    (if (and (exists? q) (displayed? q))
      (do (click q)
          )
      (error "failed to find modify menu"))))

(defn load-property-of-nav-item [name]
  ;;TODO - issues with this relating to using jq contains and it matching the wrong names
  ;; and it deleting the wrong items.... need to fix this issue generally


  ; need a wait before executing script
  (wait-for-angular)
  ;; the button to show the menu only happens when the item is hovered over
  ;; and trying to do this doesn't work reliably... so using client side script.
  (let [q (str ".client-nav-panel div[class*=nav-type]:contains('" name "') button.nav-config-panel-button")]
    (execute-script
      "var elems = angular.element(arguments[0]);
      elems[0].click();
      return elems.length;"
      [q]))
  (let [q ".contextmenu-view a:contains('Properties')"]
    (if (and (exists? q) (displayed? q))
      (do
        (click q)
        (wait-for-angular))
      (error "failed to find property menu"))))


(defn delete-nav-item [name]
  ;;TODO - issues with this relating to using jq contains and it matching the wrong names
  ;; and it deleting the wrong items.... need to fix this issue generally

  ; need a wait before executing script
  (wait-for-angular)
  ;; the button to show the menu only happens when the item is hovered over
  ;; and trying to do this doesn't work reliably... so using client side script.
  (let [q (str ".client-nav-panel div[class*=nav-type]:contains('" name "') button.nav-config-panel-button")]
    (execute-script
      "var elems = angular.element(arguments[0]);
      elems[0].click();
      return elems.length;"
      [q]))
  (let [q ".contextmenu-view a:contains('Delete')"]
    (if (and (exists? q) (displayed? q))
      (do (click q)
          (let [ok-buttons (elements "button:contains(OK)")
                ok-count (count ok-buttons)]
            (when (< 1 ok-count)
              (warn "Warning - found more than one OK button.. using the last. found" ok-count)
              (debug "Buttons are:" (map #(clj-webdriver.taxi/html %) ok-buttons)))
            (click (last ok-buttons))))
      (error "failed to find delete menu"))))

(defn remove-nav-item [name]
  ;;TODO - issues with this relating to using jq contains and it matching the wrong names
  ;; and it deleting the wrong items.... need to fix this issue generally
  ; need a wait before executing script
  (wait-for-angular)
  ;; the button to show the menu only happens when the item is hovered over
  ;; and trying to do this doesn't work reliably... so using client side script.
  (let [q (str ".client-nav-panel div[class*=nav-type]:contains('" name "') button.nav-config-panel-button")]
    (execute-script
      "var elems = angular.element(arguments[0]);
      elems[0].click();
      return elems.length;"
      [q]))
  (let [q ".contextmenu-view a:contains('Remove')"]
    (if (and (exists? q) (displayed? q))
      (click q)
      (error "failed to find remove menu"))))

(defn get-loaded-nav-items
  "Get the names of the loaded nav items. Remember that the navigation
  _tree_ is loaded on demand so only a subset of the possible nav items
  will typically be loaded."
  []
  (let [q ".client-nav-panel div[class*=nav-type]"]
    (map #(text %) (*finder-fn* q))))

(defn find-nav-items-matching [re]
  (filter #(re-matches re %) (get-loaded-nav-items)))

(defn count-matching-nav-items [nav-item]
  (count (filter #(= nav-item %) (get-loaded-nav-items))))

(defn delete-nav-items-matching [re]
  (doseq [item (find-nav-items-matching re)]
    (delete-nav-item item)))

(defn cancel-context-menu []
  (click ".contextmenu-backdrop"))

(defn are-changes-pending? []
  (exists? ".client-nav-pending-panel"))

(defn choose-to-continue-navigation []
  (click "button[test-id=navContinue]"))

(defn page-has-error-alert? []
  (exists-present? (str ".alerts-control-box  li[class='alert-error']")))

(defn page-has-any-alert? []
  (exists-present? (str ".alerts-control-box  li[class*='alert-']")))
  
(defn in-app-launcher-page? []
	(exists-present? (str ".app-launcher-head")))  

(defn clear-alerts
  "Clear any ReadiNow alerts that may be showing."
  ^:RT-action
  []
  (dorun (map #(click %) (elements ".alerts-control-box button"))))

(defn clear-all-alerts
  "Clear any dirty messages and alerts"
  []
  (try
    (when (are-changes-pending?)
      (debug "Clearing dirty message.")
      (choose-to-continue-navigation))
    (clear-alerts)

    (catch StaleElementReferenceException ex
      (warn "Ignoring StaleElementException" ex))))

(defn navigate-to-item
  "Navigate to the given application and then follow the given path of nav items"
  [app-name nav-path]
  (navigate-to app-name)
  (wait-for-angular)
  (when (are-changes-pending?)
    (choose-to-continue-navigation))
  (wait-for-angular)
  (doseq [item-name (string/split nav-path #"/")]
    (select-navigator-item item-name)
    (wait-for-angular)))

(defn- set-finder-fn []
  ;; trying out what happens if we don't wait for angular by default
  ;; as it is adding overhead to each and every webdriver call and this is really
  ;; poor performing on firefox
  #_(set-finder! jq-finder-wait-ng)
  (let [auto-ng-wait? (not (:disable-auto-ng-wait (get-settings)))]
    (info "Setting taxi finder function based on auto-ng-wait" auto-ng-wait?)
    (if auto-ng-wait?
      (set-finder! (add-angular-wait-to-finder jq-finder))
      (set-finder! jq-finder))))

(defn start-app [& [tenant, skip-setup?]]
  (when-not (is-webdriver-initialised?)
    (throw (Exception. "webdriver hasn't been initialized")))
  (when tenant
    (update-settings {:tenant tenant}))
  (info "starting on tenant " (get-tenant) "with url" (make-app-url "/"))
  (set-finder-fn)
  (open-sp-client)
  (if-not skip-setup?
    (setup-for-testing))
  (wait-for-jq "form[name='spForm']" 30000)
  true)

(defn open-parent-node [nav-item]
  (when (:parent nav-item)
    (open-nav-tree-node (:parent nav-item))))

(defn add-sections [new-sections]
  (doseq [nav-item new-sections]
    (open-parent-node nav-item)
    (add-section (:name nav-item) (:parent nav-item))))

(defn add-folders [new-folders]
  (doseq [nav-item new-folders]
    (open-parent-node nav-item)
    (add-folder (:name nav-item) (:parent nav-item))))

(defn add-screens [new-screens]
  (doseq [nav-item new-screens]
    (open-parent-node nav-item)
    (add-screen (:name nav-item) (:parent nav-item))))

(defn create-screen [name]
  (enable-config-mode)
  (add-screen name ""))

(defn add-new-tab
  "Adds a new application tab.

  Example:
  =========

  (add-new-tab \"New Tab Name\")"
  [name]
  (enable-config-mode)
  (click "li[title*='Add a new tab'] a[title*='Add a new tab']")
  (wait-for-angular)
  (input-clear-all ".sp-navigation-element-dialog input[name=fieldValue]")
  (input-text ".sp-navigation-element-dialog input[name=fieldValue]" name)
  (click-ok)
  (wait-for-angular))

(defn uninstall-application
  "Uninstalls an application.

  Example:
  =========

  (uninstall-application \"App Name\")"
  [name]
  (rn.app/open-admin "Application Library")
  ((resolve 'rt.po.report-view/choose-report-row-action) name "Uninstall")
  (rt.po.app/choose-modal-ok))
