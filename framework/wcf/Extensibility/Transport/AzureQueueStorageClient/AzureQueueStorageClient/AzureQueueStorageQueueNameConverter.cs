// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Security.Policy;
using Azure.Storage.Queues;

namespace Microsoft.ServiceModel.AQS
{
    internal class AzureQueueStorageQueueNameConverter
    {
        public static Uri ConvertToHttpEndpointUrl(Uri uri)
        {
            QueueUriBuilder builder = new QueueUriBuilder(uri);
            return new Uri("https://" + builder.AccountName.ToString() +"." + builder.Host.ToString() + "/" + builder.QueueName.ToString() + ":" + builder.Port.ToString());
        }

        public static Uri ConvertToNetEndpointUrl(Uri uri)
        {
            QueueUriBuilder builder = new QueueUriBuilder(uri);
            return new Uri("net.aqs://" + builder.AccountName.ToString() + "." + builder.QueueName.ToString() + "." + builder.Host.ToString() + ":" + builder.Port.ToString());
        }
    }
}
