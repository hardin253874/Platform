(ns rn.tablet.app
  (require rn.mobile.app
           [rt.lib.wd :refer [debug-click]]
           [rt.lib.wd-ng :refer [wait-for-angular]]
           [clj-webdriver.taxi :as taxi]
           [clojure.string :as string]))

;; Redirect to the mobile and common libraries where possible
(def logout rn.mobile.app/logout)
(def navigate-to-app-launcher rn.mobile.app/navigate-to-app-launcher)
(def navigate-to-application rn.mobile.app/navigate-to-application)
(def navigate-to-item rn.mobile.app/navigate-to-item)
(def open-navigator rn.mobile.app/open-navigator)
