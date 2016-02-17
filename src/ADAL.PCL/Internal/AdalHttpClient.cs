﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Interfaces;

namespace Microsoft.Identity.Client.Internal
{
    class AdalHttpClient
    {
        private const string DeviceAuthHeaderName = "x-ms-PKeyAuth";
        private const string DeviceAuthHeaderValue = "1.0";
        private const string WwwAuthenticateHeader = "WWW-Authenticate";
        private const string PKeyAuthName = "PKeyAuth";

        public AdalHttpClient(string uri, CallState callState)
        {
            this.Client = new HttpClientWrapper(CheckForExtraQueryParameter(uri), callState);
            this.CallState = callState;
        }

        public HttpClientWrapper Client { get; private set; }

        public CallState CallState { get; private set; }

        public async Task<T> GetResponseAsync<T>(string endpointType)
        {
            return await this.GetResponseAsync<T>(endpointType, true).ConfigureAwait(false);
        }

        private async Task<T> GetResponseAsync<T>(string endpointType, bool respondToDeviceAuthChallenge)
        {
            T typedResponse = default(T);
            IHttpWebResponse response;
            ClientMetrics clientMetrics = new ClientMetrics();

            try
            {
                clientMetrics.BeginClientMetricsRecord(this.CallState);
                
                    Dictionary<string, string> clientMetricsHeaders = clientMetrics.GetPreviousRequestRecord(this.CallState);
                    foreach (KeyValuePair<string, string> kvp in clientMetricsHeaders)
                    {
                        this.Client.Headers[kvp.Key] = kvp.Value;
                    }

                    IDictionary<string, string> adalIdHeaders = MsalIdHelper.GetAdalIdParameters();
                    foreach (KeyValuePair<string, string> kvp in adalIdHeaders)
                    {
                        this.Client.Headers[kvp.Key] = kvp.Value;
                    }
                

                //add pkeyauth header
                this.Client.Headers[DeviceAuthHeaderName] = DeviceAuthHeaderValue;
                using (response = await this.Client.GetResponseAsync().ConfigureAwait(false))
                {
                    typedResponse = DeserializeResponse<T>(response.ResponseStream);
                    clientMetrics.SetLastError(null);
                }
            }
            catch (HttpRequestWrapperException ex)
            {
                if (!this.isDeviceAuthChallenge(endpointType, ex.WebResponse, respondToDeviceAuthChallenge))
                {
                    MsalServiceException serviceEx;
                    if (ex.WebResponse != null)
                    {
                        TokenResponse tokenResponse = TokenResponse.CreateFromErrorResponse(ex.WebResponse);
                        string[] errorCodes = tokenResponse.ErrorCodes ?? new[] {ex.WebResponse.StatusCode.ToString()};
                        serviceEx = new MsalServiceException(tokenResponse.Error, tokenResponse.ErrorDescription,
                            errorCodes, ex);
                    }
                    else
                    {
                        serviceEx = new MsalServiceException(MsalError.Unknown, ex);
                    }

                    clientMetrics.SetLastError(serviceEx.ServiceErrorCodes);
                    PlatformPlugin.Logger.Error(CallState, serviceEx);
                    throw serviceEx;
                }
                else
                {
                    response = ex.WebResponse;
                }
            }
            finally
            {
                clientMetrics.EndClientMetricsRecord(endpointType, this.CallState);
            }

            //check for pkeyauth challenge
            if (this.isDeviceAuthChallenge(endpointType, response, respondToDeviceAuthChallenge))
            {
                return await HandleDeviceAuthChallenge<T>(endpointType, response).ConfigureAwait(false);
            }

            return typedResponse;
        }

        private bool isDeviceAuthChallenge(string endpointType, IHttpWebResponse response, bool respondToDeviceAuthChallenge)
        {
            return PlatformPlugin.DeviceAuthHelper.CanHandleDeviceAuthChallenge &&
                   respondToDeviceAuthChallenge &&
                   (response.Headers.ContainsKey(WwwAuthenticateHeader) &&
                    response.Headers[WwwAuthenticateHeader].StartsWith(PKeyAuthName)) &&
                   endpointType.Equals(ClientMetricsEndpointType.Token);
        }

        private IDictionary<string, string> ParseChallengeData(IHttpWebResponse response)
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            string wwwAuthenticate = response.Headers[WwwAuthenticateHeader];
            wwwAuthenticate = wwwAuthenticate.Substring(PKeyAuthName.Length + 1);
            wwwAuthenticate = wwwAuthenticate.Replace("\"", "");
            string[] headerPairs = wwwAuthenticate.Split(',');
            foreach (string pair in headerPairs)
            {
                string[] keyValue = pair.Split('=');
                data.Add(keyValue[0].Trim(),keyValue[1].Trim());
            }

            return data;
        }

        private async Task<T> HandleDeviceAuthChallenge<T>(string endpointType, IHttpWebResponse response)
        {
            IDictionary<string, string> responseDictionary = this.ParseChallengeData(response);
            string responseHeader = await PlatformPlugin.DeviceAuthHelper.CreateDeviceAuthChallengeResponse(responseDictionary).ConfigureAwait(false);
            IRequestParameters rp = this.Client.BodyParameters;
            this.Client = new HttpClientWrapper(CheckForExtraQueryParameter(responseDictionary["SubmitUrl"]), this.CallState);
            this.Client.BodyParameters = rp;
            this.Client.Headers["Authorization"] = responseHeader;
            return await this.GetResponseAsync<T>(endpointType, false).ConfigureAwait(false);
        }

        private static T DeserializeResponse<T>(Stream responseStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            if (responseStream == null)
            {
                return default(T);
            }

            using (Stream stream = responseStream)
            {
                return ((T)serializer.ReadObject(stream));
            }
        }

        private static string CheckForExtraQueryParameter(string url)
        {
            string extraQueryParameter = PlatformPlugin.PlatformInformation.GetEnvironmentVariable("ExtraQueryParameter");
            string delimiter = (url.IndexOf('?') > 0) ? "&" : "?";
            if (!string.IsNullOrWhiteSpace(extraQueryParameter))
            {
                url += string.Concat(delimiter, extraQueryParameter);
            }

            return url;
        }
    }
}
