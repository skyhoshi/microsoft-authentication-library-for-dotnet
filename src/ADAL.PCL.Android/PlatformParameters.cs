//----------------------------------------------------------------------
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

using Android.App;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Additional parameters used in acquiring user's authorization
    /// </summary>
    public class PlatformParameters : IPlatformParameters
    {
        private PlatformParameters()
        {
            SkipBroker = true;
        }

        public PlatformParameters(Activity callerActivity)
        {
            this.CallerActivity = callerActivity;
        }

        public PlatformParameters(Activity callerActivity, bool skipBroker) : this(callerActivity)
        {
            SkipBroker = skipBroker;
        }

        public bool SkipBroker { get; set; }

        /// <summary>
        /// Caller Android Activity
        /// </summary>
        public Activity CallerActivity { get; private set; } 
    }
}