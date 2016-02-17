﻿using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Handlers;
using Microsoft.Identity.Client.Internal;

namespace Microsoft.Identity.Client
{
   public sealed class ConfidentialClientApplication : AbstractClientApplication
   {
       /// <summary>
       /// 
       /// </summary>
       public ClientCredential ClientCredential { get; private set; }

       public TokenCache AppTokenCache { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="redirectUri"></param>
        /// <param name="clientCredential"></param>
        public ConfidentialClientApplication(string clientId, string redirectUri,
           ClientCredential clientCredential):this(DEFAULT_AUTHORTIY, clientId, redirectUri, clientCredential)
       {
       }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <param name="redirectUri"></param>
        /// <param name="clientCredential"></param>
       public ConfidentialClientApplication(string authority, string clientId, string redirectUri, ClientCredential clientCredential):base(authority, clientId, redirectUri, true)
        {
            this.ClientCredential = clientCredential;
        }

       public async Task<AuthenticationResult> AcquireTokenAsync(string[] scope)
       {
           return
               await
                   this.AcquireTokenForClientCommonAsync(scope,
                       new ClientKey(this.ClientId, this.ClientCredential, this.Authenticator)).ConfigureAwait(false);
       }

        public async Task<AuthenticationResult> AcquireTokenAsync(string[] scope, User userId)
        {
            return
                await
                    this.AcquireTokenSilentCommonAsync(this.Authenticator, scope,
                        new ClientKey(this.ClientId, this.ClientCredential, this.Authenticator), userId, null, null)
                        .ConfigureAwait(false);
        }

        public async Task<AuthenticationResult> AcquireTokenAsync(string[] scope, User userId, string authority, string policy)
        {
            Authenticator localAuthenticator = new Authenticator(authority, this.ValidateAuthority);
            return
                await
                    this.AcquireTokenSilentCommonAsync(localAuthenticator, scope,
                        new ClientKey(this.ClientId, this.ClientCredential, localAuthenticator), userId, null, policy)
                        .ConfigureAwait(false);
        }

        public async Task<AuthenticationResult> AcquireTokenOnBehalfOfAsync(string[] scope, UserAssertion userAssertion)
        {
            return
                await
                    this.AcquireTokenOnBehalfCommonAsync(this.Authenticator, scope,
                        new ClientKey(this.ClientId, this.ClientCredential, this.Authenticator), userAssertion, null)
                        .ConfigureAwait(false);
        }
    

        public async Task<AuthenticationResult> AcquireTokenOnBehalfOfAsync(string[] scope, UserAssertion userAssertion, string authority, string policy)
        {
            Authenticator localAuthenticator = new Authenticator(authority, this.ValidateAuthority);
            return
                await
                    this.AcquireTokenOnBehalfCommonAsync(localAuthenticator, scope,
                        new ClientKey(this.ClientId, this.ClientCredential, localAuthenticator), userAssertion, policy)
                        .ConfigureAwait(false);
        }

        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string[] scope, string authorizationCode, string policy)
        {
            return
                await
                    this.AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, scope, new Uri(this.RedirectUri),
                        new ClientKey(this.ClientId, this.ClientCredential, this.Authenticator), policy).ConfigureAwait(false);
        }


        private async Task<AuthenticationResult> AcquireTokenForClientCommonAsync(string[] scope, ClientKey clientKey)
        {
            var handler = new AcquireTokenForClientHandler(this.Authenticator, this.AppTokenCache, scope, clientKey);
            return await handler.RunAsync();
        }

        private async Task<AuthenticationResult> AcquireTokenOnBehalfCommonAsync(Authenticator authenticator, string[] scope, ClientKey clientKey, UserAssertion userAssertion, string policy)
        {
            var handler = new AcquireTokenOnBehalfHandler(authenticator, this.UserTokenCache, scope, clientKey, userAssertion, policy);
            return await handler.RunAsync();
        }

        private async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeCommonAsync(string authorizationCode, string[] scope, Uri redirectUri, ClientKey clientKey, string policy)
        {
            var handler = new AcquireTokenByAuthorizationCodeHandler(this.Authenticator, this.UserTokenCache, scope, clientKey, authorizationCode, redirectUri, policy);
            return await handler.RunAsync();
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="loginHint"></param>
        /// <param name="extraQueryParameters"></param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(string[] scope, string loginHint, string extraQueryParameters)
        {
            var handler = new AcquireTokenInteractiveHandler(this.Authenticator, null, scope, null, this.ClientId,
                new Uri(this.RedirectUri), null, loginHint, UiOptions.SelectAccount, extraQueryParameters, null, null);
            return await handler.CreateAuthorizationUriAsync(this.CorrelationId).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="redirectUri"></param>
        /// <param name="loginHint"></param>
        /// <param name="options"></param>
        /// <param name="extraQueryParameters"></param>
        /// <param name="additionalScope"></param>
        /// <param name="authority"></param>
        /// <param name="policy"></param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(string[] scope, string redirectUri, string loginHint, UiOptions options, string extraQueryParameters, string[] additionalScope, string authority, string policy)
        {
            var handler = new AcquireTokenInteractiveHandler(this.Authenticator, null, scope, additionalScope,
                this.ClientId, new Uri(this.RedirectUri), null, loginHint, options, extraQueryParameters, policy, null);
            return await handler.CreateAuthorizationUriAsync(this.CorrelationId).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="scope">Identifier of the target scope that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="userId">Identifier of the user token is requested for. This parameter can be <see cref="UserIdentifier"/>.Any.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(string[] scope, string clientId, Uri redirectUri, User userId, string extraQueryParameters)
        {
            return null;
        }
    }
}
