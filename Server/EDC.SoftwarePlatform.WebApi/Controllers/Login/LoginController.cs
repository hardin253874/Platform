// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using System.Security.Authentication;
using System.Web;
using System.Web.Http;
using EDC.Exceptions;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AuditLog;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Globalization;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.WebApi.Controllers.Login.OpenIdConnect;
using EDC.ReadiNow.Email;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Login controller.
	/// </summary>
	[RoutePrefix( "data/v1/login" )]
	public class LoginController : ApiController
	{		
	    /// <summary>
	    /// The open id connect configuration manager.
	    /// </summary>
	    private static readonly IOpenIdConnectConfigurationManager OpenIdConnectConfigurationManager = new OpenIdConnectConfigurationManager();

        /// <summary>
        ///     Challenges this instance.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("challenge")]
        public IHttpActionResult Challenge([FromUri] string clientVersion = null)
        {
            try
            {
                UserAccountValidator.Challenge();

                return Ok();
            }
            catch
            {
                EventLog.Application.WriteWarning("Failed Challenge.");
            }


            return Unauthorized();
        }

        /// <summary>
        ///     Perform a sign-in for Software Platform users using HTTP POST.
        /// </summary>
        /// <param name="jsonLoginCredential">The json login credential.</param>
        /// <returns>
        ///     A standard HTTP Response code.
        /// </returns>
        [HttpPost]
        [Route("spsignin")]
        [AllowAnonymous]
        public HttpResponseMessage<LoginResult> SigninSoftwarePlatform_Post([FromBody] JsonLoginCredential jsonLoginCredential)
        {
            return SigninSoftwarePlatform(jsonLoginCredential);
        }

        /// <summary>
        ///     Perform a password change using HTTP POST.        
        /// </summary>
        /// <param name="jsonPasswordChangeRequest">The json password change request data.</param>
        /// <returns>
        ///     A standard HTTP Response code.
        /// </returns>
        /// <remarks>
        ///     This is intended to be used only for accounts whose password has expired.
        /// </remarks>
        [HttpPost]
        [Route("spchangepassword")]
        [AllowAnonymous]
        public HttpResponseMessage ChangePasswordSoftwarePlatform_Post([FromBody] JsonPasswordChangeRequest jsonPasswordChangeRequest)
        {
            return ChangePasswordSoftwarePlatform(jsonPasswordChangeRequest);
        }


        [HttpPost]
        [Route( "spsubmitemail" )]
        [AllowAnonymous]
        public HttpResponseMessage SubmitEmailSoftwarePlatform_Post( [FromBody] JsonSubmitEmailRequest jsonSubmitEmailRequest )
        {
            return SubmitEmailSoftwarePlatform( jsonSubmitEmailRequest );
        }


        [HttpPost]
        [Route( "spresetpassword" )]
        [AllowAnonymous]
        public HttpResponseMessage ResetPasswordSoftwarePlatform_Post( [FromBody] JsonResetPasswordRequest jsonResetPasswordRequest )
        {
            return ResetPasswordSoftwarePlatform( jsonResetPasswordRequest );
        }

        /// <summary>
        ///     Perform a sign-in for Software Platform users using the cookie
        /// </summary>
        [HttpGet]
        [Route("spsignincookie")]

        public HttpResponseMessage<LoginResult> SigninSoftwarePlatformWithCookie_Get()
        {
            return SigninSoftwarePlatformWithCookie();
        }
		

		[HttpPost]
		[Route("signout")]
        [AllowAnonymous]
		public HttpResponseMessage<ResponseResult> SignOut()
		{
            return SignoutImpl(false);
		}

        /// <summary>
        /// Authenticated signed out.
        /// Writes to the audit log and signs out.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("signoutauth")]
        public HttpResponseMessage<ResponseResult> SignOutAuth()
        {            
            return SignoutImpl(true);
        }

        /// <summary>
        /// Signout of the console..
        /// </summary>
        /// <param name="isAuthorized">if set to <c>true</c> is authorized.</param>
        /// <returns></returns>
        private HttpResponseMessage<ResponseResult> SignoutImpl(bool isAuthorized)
	    {
            if (isAuthorized)
            {
                // Audit the logout event
                AuditLogInstance.Get().OnLogoff(true, ReadiNow.IO.RequestContext.GetContext().Identity.Name);     
            }            

            try
            {
                CookieHelper.DeleteAuthenticationAndXsrfCookies();
                return new HttpResponseMessage<ResponseResult>(new ResponseResult { Success = true }, HttpStatusCode.OK);
            }
            catch (Exception exc)
            {
                EventLog.Application.WriteWarning("Software Platform signout error. Cleanup of .ASPXAUTH cookie failed. {0}", exc.Message);
            }

            return new HttpResponseMessage<ResponseResult>(new ResponseResult { Success = false }, HttpStatusCode.InternalServerError);
	    }		      		

		/// <summary>
		///     Gets the successful login result.
		/// </summary>
		/// <returns></returns>
		private static LoginResult GetSuccessfulLoginResult(RequestContextData requestContextData, long accountId = -1)
		{

            /////
            // Construct the initial settings.
            /////
            var loginResult = new LoginResult
            {
                InitialSettings = new InitialSettings
                {
                    PlatformVersion = SystemInfo.PlatformVersion,
                    RequiredClientVersion = GetRequiredClientVersion(),
                    BranchName = SystemInfo.BranchName,
                    Culture = requestContextData.Culture,
                    FeatureSwitches = Factory.FeatureSwitch.GetFeatureListString()
                }
			};

			if ( accountId != -1 )
			{
				loginResult.ActiveAccountInfo = new ActiveAccountInfo
				{
					AccountId = accountId,
				};

				if (requestContextData != null)
				{
					loginResult.ActiveAccountInfo.Tenant = requestContextData.Tenant.Name;
					loginResult.ActiveAccountInfo.Username = requestContextData.Identity.Name;
					loginResult.ActiveAccountInfo.Provider = requestContextData.Identity.IdentityProviderTypeAlias;
                    loginResult.ConsoleTimeoutMinutes = LoginConstants.Cookie.Timeout;

				    if (requestContextData.Identity.IdentityProviderId != WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance)
				    {
                        // Ensure we keep the cookie alive for non native identity providers
				        loginResult.ConsoleTimeoutMinutes /= 2;
				    }
				}
                                                                                                                                                                                                                                                                                                                                                                                                                 
                // This is only set during integration tests

                loginResult.TestToken = TestAuthorization.IsEnabled ? TestAuthorization.Instance.GetTestToken(requestContextData.Tenant.Id, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, requestContextData.Identity.Name) : null;
			}


			return loginResult;
		}

        private static string GetRequiredClientVersion()
        {
            return ConfigurationSettings.GetServerConfigurationSection().Client.GetMinClientVersion(SystemInfo.PlatformVersion);
        }

		
		/// <summary>
		///     Signs a software platform user into the system.
		/// </summary>
		/// <param name="jsonLoginCredential">The json login credential.</param>
		/// <returns></returns>
		private HttpResponseMessage<LoginResult> SigninSoftwarePlatform( JsonLoginCredential jsonLoginCredential )
		{		    
            var userAgent = Request?.Headers?.UserAgent?.ToString();

            try
			{
				using ( new SecurityBypassContext( ) )
				{
				    try
				    {
                        UserAccountValidator.Authenticate(jsonLoginCredential.Username, jsonLoginCredential.Password,
                            jsonLoginCredential.Tenant, true);
				    }
				    catch (ArgumentException ex)
				    {
				        throw new InvalidCredentialException("Invalid user name, password or tenant", ex);
				    }

					RequestContext context = ReadiNow.IO.RequestContext.GetContext( );
				    RequestContextData contextData;

                    UserAccount account = ReadiNow.Model.Entity.Get<UserAccount>(context.Identity.Id, true);

					if ( account != null )
					{                        
						/////
						// If we are in integration test mode, update the test authorization info.
						/////
						if ( TestAuthorization.IsEnabled )
						{
                            TestAuthorization.Instance.SetTokenIdentifier(context.Tenant.Id, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, account.Name);
						}

                        // Update cache
                        contextData = Factory.IdentityProviderRequestContextCache.GetRequestContextData(ReadiNow.IO.RequestContext.TenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, jsonLoginCredential.Username, true);
					}
					else
					{                        
						throw new InvalidOperationException( "No UserAccount found." );
					}

				    ReadiNowIdentityProvider identityProvider = ReadiNow.Model.Entity.Get<ReadiNowIdentityProvider>(WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance);

                    if (!(identityProvider.IsProviderEnabled ?? true))
                    {
                        throw new AuthenticationException("The identity provider is not enabled.");
                    }

                    contextData.Identity.IdentityProviderTypeAlias = LoginConstants.ReadiNowIdentityProviderTypeAlias;                                        
                    
					CookieHelper.CreateAuthenticationAndXsrfCookies(ReadiNow.IO.RequestContext.TenantId, WellKnownAliases.CurrentTenant.ReadiNowIdentityProviderInstance, jsonLoginCredential.Username, account.Id, jsonLoginCredential.Persistent );

                    AuditLogInstance.Get().OnLogon(true, jsonLoginCredential.Username, userAgent);

                    return new HttpResponseMessage<LoginResult>(GetSuccessfulLoginResult(contextData, account.Id), HttpStatusCode.OK);
				}
			}
			catch ( Exception exc )
			{
			    if (ReadiNow.IO.RequestContext.IsSet)
			    {
			        AuditLogInstance.Get().OnLogon(false, jsonLoginCredential.Username, userAgent);
			    }

			    EventLog.Application.WriteWarning( "Software Platform login error. Username: {0}, Tenant: {1}. {2}", jsonLoginCredential.Username ?? "<null>", jsonLoginCredential.Tenant ?? "<null>", exc.Message );

                throw;  // rely on the ExceptionFilter to handle it.
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonPasswordChangeRequest"></param>
        /// <returns></returns>
        private HttpResponseMessage SubmitEmailSoftwarePlatform( JsonSubmitEmailRequest jsonPasswordChangeRequest )
        {

            string tenant = jsonPasswordChangeRequest.Tenant;
            string email = jsonPasswordChangeRequest.Email;

            if ( string.IsNullOrEmpty( email ) )
            {
                return new HttpResponseMessage<string>("We can't find a user that links this email address, or the user account has expired or has been disabled. Please contact your administrator.", HttpStatusCode.BadRequest );
            }

            if ( string.IsNullOrEmpty( tenant ) )
            {
                return new HttpResponseMessage<string>( "The tenant is invalid", HttpStatusCode.BadRequest );
            }
            try
            {
                using (new TenantAdministratorContext(tenant))
                {
                    //get account by email


                    // Create query            
                    var type = new EntityRef("core", "userAccount");

                    var query = new EDC.ReadiNow.Metadata.Query.Structured.StructuredQuery
                    {
                        RootEntity = new EDC.ReadiNow.Metadata.Query.Structured.ResourceEntity(type)
                    };
                    var accountHolderEntity = new EDC.ReadiNow.Metadata.Query.Structured.RelatedResource(new EntityRef("core:accountHolder"), ReadiNow.Metadata.RelationshipDirection.Reverse);

                    query.RootEntity.RelatedEntities.Add(accountHolderEntity);

                    query.Conditions.Add(new EDC.ReadiNow.Metadata.Query.Structured.QueryCondition
                    {
                        Expression = new EDC.ReadiNow.Metadata.Query.Structured.ResourceDataColumn(accountHolderEntity, new EntityRef("core:name")),
                        Operator = EDC.ReadiNow.Metadata.Query.Structured.ConditionType.IsNotNull
                    });

                    query.Conditions.Add(new EDC.ReadiNow.Metadata.Query.Structured.QueryCondition
                    {
                        Expression = new EDC.ReadiNow.Metadata.Query.Structured.ResourceDataColumn(accountHolderEntity, new EntityRef("shared:businessEmail")),
                        Operator = EDC.ReadiNow.Metadata.Query.Structured.ConditionType.Equal,
                        Argument = new EDC.ReadiNow.Metadata.TypedValue(email)
                    });


                    // Get results
                    IEnumerable<UserAccount> entities = ReadiNow.Model.Entity.GetMatches<UserAccount>(query);

                    if (entities == null || entities.Count() == 0)
                    {
                        return new HttpResponseMessage<string>("We can't find a user that links this email address, or the user account has expired or has been disabled. Please contact your administrator.", HttpStatusCode.NotFound);
                    }

                    bool success = false;
                    bool disabledAccount = true;
                    //as required from task 27183, if multiple user accounts are associated with one account holder, then each account will be received email
                    foreach (UserAccount account in entities)
                    {
                        if (account.AccountStatus_Enum != UserAccountStatusEnum_Enumeration.Expired && account.AccountStatus_Enum != UserAccountStatusEnum_Enumeration.Disabled)
                        {
                            success = SendEmail(account, email, tenant);
                            disabledAccount = false;
                        }
                    }

                    //account.Id
                    if (success == true && disabledAccount == false)
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    else if (disabledAccount == true)
                        return new HttpResponseMessage<string>("Your account has expired or has been disabled. Please contact your administrator", HttpStatusCode.BadRequest);
                    else
                        return new HttpResponseMessage<string>("There is an error to send reset password email. Please contact your administrator", HttpStatusCode.BadRequest);
                }
            }
            catch(EntityNotFoundException tenantEx)
            {
                string errorMessage = tenantEx.InnerException != null ? tenantEx.InnerException.Message : tenantEx.Message;
                EventLog.Application.WriteError("Submit Forget Password Email: Unhandled internal Entity Not Found Exception: " + errorMessage);
                return new HttpResponseMessage<string>(errorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception otherEx)
            {
                EventLog.Application.WriteError("Submit Forget Password Email: Unhandled internal Exception: " + otherEx.Message);
                return new HttpResponseMessage<string>(otherEx.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
		///     Send reset password Email.
		/// </summary>		
		/// <param name="displayName">The display name.</param>
		/// <returns></returns>
		private bool SendEmail( UserAccount account, string email, string tenant )
        {
            bool successSentEmail = false;

            try
            {
                account = account.AsWritable<UserAccount>();

                //only check current account is available or not
                if (account == null)
                {
                    return successSentEmail;
                }

                string key = EDC.Security.CryptoHelper.GetMd5Hash(account.Id.ToString());
                if (account.PasswordReset != null)
                {
                    account.PasswordReset.AsWritable<PasswordResetRecord>();
                    account.PasswordReset.Delete();
                }


                account.PasswordReset = new PasswordResetRecord { PasswordResetKey = key, PasswordResetExpiry = DateTime.UtcNow.AddDays(1.0) };
                account.Save();

                EventLog.Application.WriteInformation("set password reset key {0}", key);

                var mail = new SentEmailMessage() { EmIsHtml = true, EmSentDate = DateTime.UtcNow, };

                string accountFullName = account.AccountHolder.FirstName + " " + account.AccountHolder.LastName;
                if (accountFullName.Trim().Length == 0)
                    accountFullName = account.AccountHolder.Name;

                //string toMail = email;
                const string subject = "Reset Password - ReadiNow";
                string link = string.Format("{0}#/{1}/?key={2}&type=reset", Request.Headers.Referrer.ToString(), tenant, key);
                string mailBoday = string.Format("Hi {0} <br> <br> We've received a request to reset your account {1} password for email address {2} <br> If you didn't make the request, please contact your administrator. <br> Otherwise, you can reset your password using this link: {3} <br><br>Best Regards, <br>ReadiNow", accountFullName, account.Name, email, link);

                var emailServer = ReadiNow.Model.Entity.Get<TenantEmailSetting>("tenantEmailSettingsInstance", TenantEmailSetting.AllFields);
                
                if (!string.IsNullOrEmpty(emailServer.SmtpServer) && !string.IsNullOrEmpty(emailServer.EmailNoReplyAddress))
                {
                    EventLog.Application.WriteInformation("Send user {0} reset password email to account '{1}' by email '{2}'", accountFullName, account.Name, email);

                    var sentMessage = new SentEmailMessage()
                    {
                        EmTo = email,
                        EmFrom = emailServer.EmailNoReplyAddress,
                        EmSubject = subject,
                        EmBody = mailBoday,
                        EmIsHtml = true,
                        EmSentDate = DateTime.UtcNow,
                        SentFromEmailServer = emailServer
                    };
                    sentMessage.Save();

                    var sender = new SmtpEmailSender(emailServer);
                    sender.SendMessages(sentMessage.ToMailMessage().ToEnumerable().ToList());
                }

                successSentEmail = true;

                
            }
            catch(Exception ex)
            {
                EventLog.Application.WriteError("Send Email: Unhandled internal exception: " + ex.ToString());
            }

            return successSentEmail;
        }


        /// <summary>
        /// Change the specified user account password.
        /// </summary>
        /// <param name="jsonPasswordChangeRequest">The password change details.</param>
        /// <returns></returns>
        private HttpResponseMessage ChangePasswordSoftwarePlatform(JsonPasswordChangeRequest jsonPasswordChangeRequest)
        {            
            using (new SecurityBypassContext())
            {
                try
                {
                    UserAccountValidator.Authenticate(jsonPasswordChangeRequest.Username, jsonPasswordChangeRequest.OldPassword,
                        jsonPasswordChangeRequest.Tenant, true, true);                    
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidCredentialException("Invalid user name, password or tenant", ex);
                }

                RequestContext context = ReadiNow.IO.RequestContext.GetContext();
                UserAccount account = ReadiNow.Model.Entity.Get<UserAccount>(context.Identity.Id, true);

                // Can only change the password via this method if the password is already expired.
                if (account == null || !UserAccountValidator.HasAccountPasswordExpired(account))
                {
                    throw new InvalidCredentialException("Invalid user name, password or tenant");
                }

                account.Password = jsonPasswordChangeRequest.NewPassword;
                account.Save();                
            }                                

            return new HttpResponseMessage(HttpStatusCode.OK);                        
        }


        /// <summary>
        /// Reset the specified user account password.
        /// </summary>
        /// <param name="jsonResetPasswordRequest">The password reset details.</param>
        /// <returns></returns>
        private HttpResponseMessage ResetPasswordSoftwarePlatform( JsonResetPasswordRequest jsonResetPasswordRequest )
        {
            string tenant = jsonResetPasswordRequest.Tenant;
            string newPassword = jsonResetPasswordRequest.NewPassword;

            if ( string.IsNullOrEmpty( newPassword ) )
            {
                return new HttpResponseMessage<string>( "The new password is invalid", HttpStatusCode.BadRequest );
            }

            if ( string.IsNullOrEmpty( tenant ) )
            {
                return new HttpResponseMessage<string>( "The tenant is invalid", HttpStatusCode.BadRequest );
            }

            try
            {
                using (new TenantAdministratorContext(tenant))
                {
                    string key = jsonResetPasswordRequest.Key;

                    // Create query            
                    var type = new EntityRef("core", "userAccount");

                    var query = new EDC.ReadiNow.Metadata.Query.Structured.StructuredQuery
                    {
                        RootEntity = new EDC.ReadiNow.Metadata.Query.Structured.ResourceEntity(type)
                    };
                    var passwordResetEntity = new EDC.ReadiNow.Metadata.Query.Structured.RelatedResource(new EntityRef("core:passwordReset"));

                    query.RootEntity.RelatedEntities.Add(passwordResetEntity);

                    query.Conditions.Add(new EDC.ReadiNow.Metadata.Query.Structured.QueryCondition
                    {
                        Expression = new EDC.ReadiNow.Metadata.Query.Structured.ResourceDataColumn(passwordResetEntity, new EntityRef("core:passwordResetKey")),
                        Operator = EDC.ReadiNow.Metadata.Query.Structured.ConditionType.Equal,
                        Argument = new EDC.ReadiNow.Metadata.TypedValue(key)
                    });
                    // Get results
                    IEnumerable<UserAccount> entities = ReadiNow.Model.Entity.GetMatches<UserAccount>(query);

                    UserAccount accountInfo = entities != null && entities.Count() > 0 ? entities.FirstOrDefault() : null;

                    if (accountInfo == null || accountInfo.PasswordReset == null)
                    {
                        return new HttpResponseMessage<string>("This key is invalid, Please contact your administrator.", HttpStatusCode.BadRequest);
                    }

                    if (accountInfo.PasswordReset.PasswordResetExpiry < DateTime.UtcNow)
                    {
                        return new HttpResponseMessage<string>("This key is expired, Please contact your administrator.", HttpStatusCode.BadRequest);
                    }

                    UserAccount account = accountInfo.AsWritable<UserAccount>();

                    if (account.AccountStatus_Enum == UserAccountStatusEnum_Enumeration.Expired || account.AccountStatus_Enum == UserAccountStatusEnum_Enumeration.Disabled)
                    {
                        return new HttpResponseMessage<string>("Your account has expired or has been disabled. Please contact your administrator", HttpStatusCode.BadRequest);
                    }

                    if (account == null)
                    {
                        throw new InvalidCredentialException("Invalid account, Please contact your administrator.");
                    }

                    account.Password = newPassword;
                    account.PasswordReset.AsWritable<PasswordResetRecord>();
                    account.PasswordReset.Delete();

                    //unlock the account
                    account.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
                    try
                    {
                        account.Save();
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteError("Reset Password: Unhandled internal exception: " + ex.ToString());
                        return new HttpResponseMessage<string>("There is an error to reset your new password. Please contact your administrator", HttpStatusCode.BadRequest);
                    }
                }

            }
            catch (EntityNotFoundException tenantEx)
            {
                string errorMessage = tenantEx.InnerException != null ? tenantEx.InnerException.Message : tenantEx.Message;
                EventLog.Application.WriteError("Reset Password: Unhandled internal Entity Not Found Exception: " + errorMessage);
                return new HttpResponseMessage<string>(errorMessage, HttpStatusCode.BadRequest);
            }
            catch (Exception otherEx)
            {
                EventLog.Application.WriteError("Reset Password: Unhandled internal Exception: " + otherEx.Message);
                return new HttpResponseMessage<string>(otherEx.Message, HttpStatusCode.BadRequest);
            }            
            

            return new HttpResponseMessage( HttpStatusCode.OK );
        }
        

	    /// <summary>
        ///     Signs a software platform user into the system using an existing cookie.
        /// </summary>
        private HttpResponseMessage<LoginResult> SigninSoftwarePlatformWithCookie()
        {
            var userAgent = Request?.Headers?.UserAgent?.ToString();

            var context = EDC.ReadiNow.IO.RequestContext.GetContext();
            var contextData = new RequestContextData(context.Identity, context.Tenant, CultureHelper.GetUiThreadCulture(CultureType.Specific), context.TimeZone);

            AuditLogInstance.Get().OnLogon(true, context.Identity.Name, userAgent);

            return new HttpResponseMessage<LoginResult>(GetSuccessfulLoginResult(contextData, context.Identity.Id), HttpStatusCode.OK);
        }

	    /// <summary>
	    /// Gets the identity providers for the specified tenant .
	    /// </summary>
	    /// <param name="tenant">The tenant name.</param>
	    /// <returns></returns>
	    [HttpGet]
	    [Route("idproviders")]
	    [AllowAnonymous]
	    public IHttpActionResult GetIdentityProvidersForTenant(string tenant)
	    {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new WebArgumentNullException("tenant");
            }
            
            var identityProviderRepository = new IdentityProviderRepository();
            TenantIdentityProviderResponse response = identityProviderRepository.GetIdentityProviders(tenant);

            return Ok(response);
        }

	    /// <summary>
	    /// The OpenId connect authorization response handler.
	    /// </summary>
	    /// <param name="tenant">The tenant. This is specified so that we can route requests to specific front end servers.</param>
	    /// <param name="code">The code.</param>
	    /// <param name="state">The state.</param>
	    /// <param name="error">The error.</param>
	    /// <param name="error_description">The error_description.</param>
	    /// <returns>Task&lt;IHttpActionResult&gt;.</returns>
	    [HttpGet]
	    [Route("oidc/authresponse/{tenant}")]
	    [AllowAnonymous]
        // ReSharper disable once InconsistentNaming
	    public async Task<IHttpActionResult> OidcAuthResponseCallback(string tenant, string code = null, string state = null, string error = null, string error_description = null)
        {
            OpenIdConnectAuthorizationState authState = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    return BadRequest($"An error occurred during authorization. Error:{error}. Description:{error_description}");
                }

                if (string.IsNullOrWhiteSpace(tenant))
                {
                    throw new WebArgumentNullException("tenant", "The tenant was not specified");
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new WebArgumentNullException("code", "The code was not specified");
                }

                if (string.IsNullOrWhiteSpace(state))
                {
                    throw new WebArgumentNullException("state", "The state was not specified");
                }

                // For some reason ADFS is replacing the + signs (even the encoded ones) with spaces, so we undo it.
                // State is Base64 encoded so it should not contain spaces.
                if (!string.IsNullOrWhiteSpace(state))
                {
                    state = state.Replace(' ', '+');
                }

                var oidcLoginHandler = new OpenIdConnectLoginHandler(OpenIdConnectConfigurationManager);
                if (oidcLoginHandler.IsTokenValidationDisabled)
                {
                    EventLog.Application.WriteError("OpenIdConnectLoginHandler has token validation disabled. This is not be used in production.");
                    throw new AuthenticationException();
                }

                // Validate the state.
                authState = oidcLoginHandler.ValidateAuthState(state);

                using (var httpClient = new BasicHttpClient())
                {
                    // Process the authorization response.
                    var result = await oidcLoginHandler.ProcessOidcAuthorizationResponse(tenant, code, authState, new Uri(Request.RequestUri.GetLeftPart(UriPartial.Authority)), httpClient);

                    CookieHelper.CreateAuthenticationAndXsrfCookies(authState.TenantId, authState.IdentityProviderId, result.IdentityProviderUserName, result.RequestContextData.Identity.Id, true);
                }                

                return Redirect(authState.RedirectUrl);
            }
            catch(Exception exception)
            {
                var oidcConfigException = exception as OidcProviderInvalidConfigurationException;                

                EventLog.Application.WriteError("Failed to process oidc authorization response. Error: {0}", exception.ToString());

                CookieHelper.DeleteAuthenticationAndXsrfCookies();
                if (!string.IsNullOrWhiteSpace(authState?.RedirectUrl))
                {                    
                    string delimiter = authState.RedirectUrl.Contains("?") ? "&" : "?";
                    string errorType = oidcConfigException != null ? "idpconfigerror" : "autherror";
                    return Redirect(authState.RedirectUrl + delimiter + "error=" + errorType);
                }

                return Unauthorized();
            }      
        }


	    /// <summary>
	    /// Gets the authorization url for an OIDC provider.
	    /// </summary>
	    /// <returns>The authorization url.</returns>
	    [HttpPost]
	    [Route("oidc/authorizeurl")]
	    [AllowAnonymous]
	    public async Task<IHttpActionResult> GetIdentityProviderAuthorizeUrl([FromBody] IdentityProviderLoginRequest idpLoginRequest)
	    {
	        if (idpLoginRequest == null)
	        {
	            throw new WebArgumentException("idpLoginRequest", "The identity provider login request is invalid.");
	        }

	        var oidcLoginHandler = new OpenIdConnectLoginHandler(OpenIdConnectConfigurationManager);
	        if (oidcLoginHandler.IsTokenValidationDisabled)
	        {
	            EventLog.Application.WriteError("OpenIdConnectLoginHandler has token validation disabled. This is not be used in production.");
                throw new AuthenticationException();
	        }

	        try
	        {
	            var authorizeUrl = await oidcLoginHandler.GetAuthorizationCodeRequestUrl(idpLoginRequest, new Uri(Request.RequestUri.GetLeftPart(UriPartial.Authority)));

	            return Ok(authorizeUrl.ToString());
	        }
	        catch (Exception exception)
	        {
	            EventLog.Application.WriteError("Failed to get identity provider authorize url. Error: {0}",
	                exception.ToString());
	            throw;
	        }
	    }
	}
}