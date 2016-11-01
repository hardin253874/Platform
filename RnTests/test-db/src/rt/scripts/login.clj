(ns rt.scripts.login
  (:require [rt.setup :refer [get-app-url get-tenant get-settings update-settings]]
            [rt.lib.wd :refer :all]
            [rt.lib.wd-ng :refer :all]
            [rt.lib.wd-rn :refer :all]
            [rt.lib.util :refer :all]
            [rt.po.app :refer [setup-for-testing get-client-app-settings make-app-url]]
            [rt.po.common :refer [exists-present? select-picker-dialog-grid-row-by-text click-ok click-cancel wait-until]]
            [clj-webdriver.taxi :as taxi
             :refer [execute-script to refresh set-finder! *finder-fn* elements element find-element-under
                     text attribute input-text exists? displayed? present?
                     take-screenshot implicit-wait select-by-text click]]
            [clj-webdriver.core :refer [->actions move-to-element]]
            [clojure.string :as string]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]
            [rt.po.edit-form :as ef]))

(defn get-id-provider-config
  []
  {:client-id "253cd9c2-bbb5-45c5-85b5-21fec3e25eea", :client-secret "naCQE7Rmb_CtnDALVkLcwKZ61B7OIQGGXFg2racb", :config-url "https://rndev20adfs.sp.local/adfs/.well-known/openid-configuration"})

(defn create-adfs-id-provider
  [idp-name]
  (let [id-config (get-id-provider-config)]
    (ef/to-create-form "core:oidcIdentityProvider")
    (ef/set-string-field-value "Name" idp-name)
    (ef/set-string-field-value "Client Id" (:client-id id-config))
    (ef/set-string-field-value "Client secret" (:client-secret id-config))
    (ef/set-string-field-value "Identity claim" "upn")
    (ef/set-string-field-value "Identity provider configuration URL" (:config-url id-config))
    (ef/save)))

(defn create-adfs-id-provider-user
  [idp-user-name associated-account-name idp-name]
  (ef/to-create-form "core:oidcIdentityProviderUser")
  (ef/set-string-field-value "Name" idp-user-name)
  (ef/set-lookup "Associated account" associated-account-name)
  (ef/set-lookup "Identity provider" idp-name)
  (ef/save))

(defn login-with-id-provider
  ([idp-name]
   (info "logging in with provider " idp-name " and *******")
   (wait-until-displayed {:css "form[name='spForm'] select[name='idProvider']"} 50000)
   (when-not (exists? {:css "form[name='spForm']"})
     (throw (Exception. "Cannot see login form")))
   (setup-for-testing)
   (timeit "form-submit"
           (do (select-by-text "select[name=idProvider]" idp-name)
               (click {:css "form[name='spForm'] button[type='submit']"})))))

(defn login-to-adfs
  ([idp-user-name password]
   (info "logging in to ADFS with " idp-user-name " and *******")
   (wait-until-displayed {:css "form[id='loginForm'] input[id='userNameInput']"} 50000)
   (when-not (exists? {:css "form[id='loginForm']"})
     (throw (Exception. "Cannot see ADFS login form")))
   (timeit "form-submit"
           (do (set-input-value {:css "form[id='loginForm'] input[id='userNameInput']"} idp-user-name)
               (set-input-value {:css "form[id='loginForm'] input[id='passwordInput']"} password)
               (click {:css "form[id='loginForm'] span[id='submitButton']"})))))

(defn verify-logged-in
   ([user-name]
    (rt.po.common/wait-until #(= user-name (:username (get-authenticated-identity))) 10000)
    (let [identity (get-authenticated-identity)]
      (when (not= (some-> (:username identity) string/lower-case)
                  (string/lower-case user-name))
        #_(warn "WARNING:" (str "Logged in but authenticated identity does not match: expecting '"
                                user-name "', actual:'" (:username identity) "'"))
        (throw (Exception. (str "Logged in but authenticated identity does not match: expecting '"
                                user-name "', actual:'" (:username identity) "'")))))
    (info "Client App Settings (after login):" (get-client-app-settings))
    (to (make-app-url "/"))))

(defn verify-not-logged-in
   ([]
    (wait-until-displayed {:css "form[name='spForm']"} 50000)
    (when-not (exists? {:css "form[name='spForm']"})
      (throw (Exception. "Cannot see login form")))))
