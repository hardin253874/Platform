(ns rn.mobile.app
  (require [rt.lib.wd :refer [debug-click]]
           [rt.lib.wd-ng :refer [wait-for-angular]]
           [clj-webdriver.taxi :as taxi]
           [clojure.string :as string]
           rt.po.app))

;;todo - move to somewhere common
(defn checked-find-element [query-string]
  (or (taxi/element query-string)
      (throw (Exception. (str "Failed to find element: " query-string)))))

(defn- get-application-tile [app-name]
  (taxi/element (str ".app-launcher-tile img[alt='" app-name "']")))

(defn- get-show-nav-link []
  ; note - added the img criteria to make it work in a chart view
  (checked-find-element "a[ng-click*='toggle']:has(img[src*='toolbar_toggle'])"))

(defn- get-hide-nav-link []
  (checked-find-element "a[ng-click*='hideNav']:visible"))

(defn on-launcher-page? []
  (not-empty (taxi/elements ".app-launcher-page")))

(defn navigate-to-application [app-name]
  (assert (on-launcher-page?) "Not on app launcher page")
  (let [q (get-application-tile app-name)]
    (assert q (str "Cannot find application " app-name))
    (debug-click q)
    (wait-for-angular)
    (assert (not (on-launcher-page?)) "Still on app launch page!")))

(defn- get-back-to-apps-link []
  (checked-find-element "a.back-to-apps"))

(defn navigator-open? []
  (not (empty? (taxi/elements ".navigator-container.open"))))

(defn open-navigator []
  (when-not (navigator-open?)
    (debug-click (get-show-nav-link))
    (wait-for-angular)))

(defn close-navigator []
  (when (navigator-open?)
    (debug-click (get-hide-nav-link))
    (wait-for-angular)))

(defn navigate-to-app-launcher []
  (when-not (on-launcher-page?)
    (open-navigator)
    (Thread/sleep 500)
    (debug-click (get-back-to-apps-link))
    (Thread/sleep 500)
    (wait-for-angular)))

(defn are-changes-pending? []
  ;;todo: check this...
  (rt.po.app/are-changes-pending?))

(defn choose-to-continue-navigation []
  ;;todo: check this...
  (rt.po.app/choose-to-continue-navigation))

(defn select-navigator-item [item-name]
  (let [q (str ".nav-list-mob [ng-repeat*='i in itemList']:contains(" item-name ")")
        items (->> (taxi/elements q)
                   (map #(hash-map :e % :v (re-find #".*" (taxi/text %))))
                   (filter #(re-find (re-pattern item-name) (:v %)))
                   (filter (comp not empty? :v))
                   (map :e))
        item (first items)]
    (assert item (str "Cannot find item: " item-name))
    ;; not sure how to do the tapped or touch event
    ;; and clicking doesn't seem to work
    ;; but this does....
    (rt.lib.wd-ng/execute-script-on-element
      "
      var scope = angular.element(arguments[0]).scope();
      scope.itemTapped(scope.i);
      "
      item)
    (wait-for-angular)))

(defn navigate-to-item
  "Navigate to the given application and then follow the given
  path of nav items"
  [app-name nav-path]
  (open-navigator)
  (when app-name
    (navigate-to-app-launcher)
    (navigate-to-application app-name))
  (when (are-changes-pending?)
    (choose-to-continue-navigation))
  (doseq [item-name (string/split nav-path #"/")]
    (select-navigator-item item-name))
  (close-navigator)
  (wait-for-angular))

(defn logout []
  (debug-click (checked-find-element ".signout")))

