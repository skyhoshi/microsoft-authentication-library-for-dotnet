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
using System.Globalization;

namespace Microsoft.Identity.Client.Internal
{
    /// <summary>
    /// The exception type thrown when an error occurs during token acquisition.
    /// </summary>
    public class MsalException : Exception
    {
        internal enum ErrorFormat
        {
            Json,
            Other
        }

        /// <summary>
        ///  Initializes a new instance of the exception class.
        /// </summary>
        public MsalException()
            : base(MsalErrorMessage.Unknown)
        {
            this.ErrorCode = MsalError.Unknown;
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code.
        /// </summary>
        /// <param name="errorCode">The error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        public MsalException(string errorCode)
            : base(GetErrorMessage(errorCode))
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and error message.
        /// </summary>
        /// <param name="errorCode">The error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public MsalException(string errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        public MsalException(string errorCode, Exception innerException)
            : base(GetErrorMessage(errorCode), innerException)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code, error message and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        public MsalException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.
        /// </summary>
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format("\n\tErrorCode: {0}", this.ErrorCode);
        }

        protected static string GetErrorMessage(string errorCode)
        {
            string message;
            switch (errorCode)
            {
                case MsalError.InvalidCredentialType: 
                    message = MsalErrorMessage.InvalidCredentialType;
                    break;

                case MsalError.IdentityProtocolLoginUrlNull:
                    message = MsalErrorMessage.IdentityProtocolLoginUrlNull;
                    break;

                case MsalError.IdentityProtocolMismatch:
                    message = MsalErrorMessage.IdentityProtocolMismatch;
                    break;

                case MsalError.EmailAddressSuffixMismatch:
                    message = MsalErrorMessage.EmailAddressSuffixMismatch;
                    break;

                case MsalError.IdentityProviderRequestFailed:
                    message = MsalErrorMessage.IdentityProviderRequestFailed;
                    break;

                case MsalError.StsTokenRequestFailed:
                    message = MsalErrorMessage.StsTokenRequestFailed;
                    break;

                case MsalError.EncodedTokenTooLong:
                    message = MsalErrorMessage.EncodedTokenTooLong;
                    break;

                case MsalError.StsMetadataRequestFailed:
                    message = MsalErrorMessage.StsMetadataRequestFailed;
                    break;

                case MsalError.AuthorityNotInValidList:
                    message = MsalErrorMessage.AuthorityNotInValidList;
                    break;

                case MsalError.UnsupportedUserType:
                    message = MsalErrorMessage.UnsupportedUserType;
                    break;

                case MsalError.UnknownUser:
                    message = MsalErrorMessage.UnknownUser;
                    break;

                case MsalError.UserRealmDiscoveryFailed:
                    message = MsalErrorMessage.UserRealmDiscoveryFailed;
                    break;

                case MsalError.AccessingWsMetadataExchangeFailed:
                    message = MsalErrorMessage.AccessingMetadataDocumentFailed;
                    break;

                case MsalError.ParsingWsMetadataExchangeFailed:
                    message = MsalErrorMessage.ParsingMetadataDocumentFailed;
                    break;

                case MsalError.WsTrustEndpointNotFoundInMetadataDocument:
                    message = MsalErrorMessage.WsTrustEndpointNotFoundInMetadataDocument;
                    break;

                case MsalError.ParsingWsTrustResponseFailed:
                    message = MsalErrorMessage.ParsingWsTrustResponseFailed;
                    break;

                case MsalError.AuthenticationCanceled:
                    message = MsalErrorMessage.AuthenticationCanceled;
                    break;

                case MsalError.NetworkNotAvailable:
                    message = MsalErrorMessage.NetworkIsNotAvailable;
                    break;

                case MsalError.AuthenticationUiFailed:
                    message = MsalErrorMessage.AuthenticationUiFailed;
                    break;

                case MsalError.UserInteractionRequired:
                    message = MsalErrorMessage.UserInteractionRequired;
                    break;

                case MsalError.MissingFederationMetadataUrl:
                    message = MsalErrorMessage.MissingFederationMetadataUrl;
                    break;

                case MsalError.IntegratedAuthFailed:
                    message = MsalErrorMessage.IntegratedAuthFailed;
                    break;

                case MsalError.UnauthorizedResponseExpected:
                    message = MsalErrorMessage.UnauthorizedResponseExpected;
                    break;

                case MsalError.MultipleTokensMatched:
                    message = MsalErrorMessage.MultipleTokensMatched;
                    break;

                case MsalError.PasswordRequiredForManagedUserError:
                    message = MsalErrorMessage.PasswordRequiredForManagedUserError;
                    break;

                case MsalError.GetUserNameFailed:
                    message = MsalErrorMessage.GetUserNameFailed;
                    break;

                default:
                    message = MsalErrorMessage.Unknown;
                    break;
            }

            return String.Format(CultureInfo.InvariantCulture, "{0}: {1}", errorCode, message);
        }
    }
}
