// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Web.Http;
using EDC.Exceptions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.Security;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
    [RoutePrefix("data/v1/password")]
    public class PasswordManagementController : ApiController
    {
        /// <summary>
        /// Change the user's password.
        /// </summary>
        /// <param name="passwordChangeInfo">
        /// The new password. This cannot be null. 
        /// </param>
        [Route("")]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]PasswordChangeInfo passwordChangeInfo)
        {
			if ( passwordChangeInfo?.CurrentPassword == null )
			{
				// Return 400 Bad Request
				throw new WebArgumentNullException( "Current password omitted" );
			}

			if ( passwordChangeInfo == null
				|| passwordChangeInfo.Password == null )
			{
				// Return 400 Bad Request
				throw new WebArgumentNullException( "Password omitted" );
			}

			UserAccount userAccount;
			RequestContext requestContext;

			requestContext = ReadiNow.IO.RequestContext.GetContext( );
			if (requestContext == null)
            {
                // Return 401 Unauthenticated
                throw new InvalidCredentialException(UserAccountValidator.InvalidUserNameOrPasswordMessage);
            }

            userAccount = ReadiNow.Model.Entity.Get<UserAccount>(requestContext.Identity.Id);
            if (userAccount == null)
            {
                // Return 404 Not found
                throw new WebArgumentNotFoundException("User does not exist or is inaccessible");
            }

            try
            {
				if ( !CryptoHelper.ValidatePassword( passwordChangeInfo.CurrentPassword, userAccount.Password ) )
				{
					throw new InvalidCredentialException( "Invalid password specified." );
				}

				using (new SecurityBypassContext())
                {
                    userAccount = userAccount.AsWritable<UserAccount>();
                    userAccount.Password = passwordChangeInfo.Password;
                    userAccount.ChangePasswordAtNextLogon = false;
                    userAccount.Save();
                }
            }
            catch (Exception ex)
            {
                // Return 400 Bad Request
                throw new WebArgumentException(ex.Message, ex);
            }

            // Return 200 OK
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}