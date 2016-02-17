﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Identity.Client.Internal
{
    internal abstract class WebUI : IWebUI
    {
        protected Uri RequestUri { get; private set; }

        protected Uri CallbackUri { get; private set; }

        public Object OwnerWindow { get; set; }

        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, IDictionary<string, string> additionalHeaders, CallState callState)
        {
            AuthorizationResult authorizationResult = null;
            StringBuilder builder = new StringBuilder();

            if (additionalHeaders != null)
            {
                foreach (var key in additionalHeaders.Keys)
                {
                    builder.AppendFormat(@"{0}: {1}\r\n", key, additionalHeaders[key]);
                }
            }

            var sendAuthorizeRequest = new Action(
                delegate
                {
                    authorizationResult = this.Authenticate(authorizationUri, redirectUri, builder.ToString());
                });

            // If the thread is MTA, it cannot create or communicate with WebBrowser which is a COM control.
            // In this case, we have to create the browser in an STA thread via StaTaskScheduler object.
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
            {
                using (var staTaskScheduler = new StaTaskScheduler(1))
                {
                    try
                    {
                        Task.Factory.StartNew(sendAuthorizeRequest, CancellationToken.None, TaskCreationOptions.None, staTaskScheduler).Wait();
                    }
                    catch (AggregateException ae)
                    {
                        // Any exception thrown as a result of running task will cause AggregateException to be thrown with 
                        // actual exception as inner.
                        Exception innerException = ae.InnerExceptions[0];

                        // In MTA case, AggregateException is two layer deep, so checking the InnerException for that.
                        if (innerException is AggregateException)
                        {
                            innerException = ((AggregateException)innerException).InnerExceptions[0];
                        }

                        throw innerException;
                    }
                }
            }
            else
            {
                sendAuthorizeRequest();
            }

            return await Task.Factory.StartNew(() => authorizationResult).ConfigureAwait(false);
        }

        internal AuthorizationResult Authenticate(Uri requestUri, Uri callbackUri, string headers)
        {
            this.RequestUri = requestUri;
            this.CallbackUri = callbackUri;

            ThrowOnNetworkDown();
            return this.OnAuthenticate(headers);
        }

        private static void ThrowOnNetworkDown()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new MsalException(MsalError.NetworkNotAvailable);
            }
        }

        protected abstract AuthorizationResult OnAuthenticate(string headers);
    }
}
