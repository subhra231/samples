// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Security.Policy;

namespace Microsoft.ServiceModel.AQS
{
    internal class AzureQueueStorageQueueNameConverter
    {
        public static string ConvertToHttpEndpointUrl(Uri uri)
        {
            return uri.LocalPath.Replace("net.aqs", "https");
        }

        public static string ConvertToNetEndpointUrl(Uri uri)
        {
            return uri.LocalPath.Replace("https", "net.aqs");
        }
    }
}
