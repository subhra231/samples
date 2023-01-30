//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ServiceModel.AQS
{
    /// <summary>
    /// Collection of constants used by the AzureQueueStorage Channel classes
    /// </summary>
    internal static class AzureQueueStorageConstants
    {
        internal const string EventLogSourceName = "Microsoft.Samples.AzureQueueStorage";
        internal const string Scheme = "soap.aqs";
        internal const string AzureQueueStorageBindingSectionName = "system.serviceModel/bindings/sampleProfileAzureQueueStorageBinding";
        internal const string AzureQueueStorageTransportSectionName = "aqsTransport";
        internal const int WSAETIMEDOUT = 10060;
        private static MessageEncoderFactory messageEncoderFactory;
        static AzureQueueStorageConstants()
        {
            messageEncoderFactory = new TextMessageEncodingBindingElement().CreateMessageEncoderFactory();
        }

        // ensure our advertised MessageVersion matches the version we're
        // using to serialize/deserialize data to/from the wire
        internal static MessageVersion MessageVersion
        {
            get
            {
                return messageEncoderFactory.MessageVersion;
            }
        }

        // we can use the same encoder for all our Udp Channels as it's free-threaded
        internal static MessageEncoderFactory DefaultMessageEncoderFactory
       {
            get
            {
                return messageEncoderFactory;
            }
        }
    }

    internal static class UdpConfigurationStrings
    {
        public const string MaxBufferPoolSize = "maxBufferPoolSize";
        public const string MaxReceivedMessageSize = "maxMessageSize";
        public const string Multicast = "multicast";
        public const string OrderedSession = "orderedSession";
        public const string ReliableSessionEnabled = "reliableSessionEnabled";
        public const string SessionInactivityTimeout = "sessionInactivityTimeout";
        public const string ClientBaseAddress = "clientBaseAddress";
    }

    internal static class UdpPolicyStrings
    {
        public const string UdpNamespace = "http://sample.schemas.microsoft.com/policy/aqs";
        public const string Prefix = "aqs";
        public const string MulticastAssertion = "Multicast";
        public const string TransportAssertion = "soap.aqs";
    }

    internal static class AzureQueueStorageChannelHelpers
    {
        /// <summary>
        /// The Channel layer normalizes exceptions thrown by the underlying networking implementations
        /// into subclasses of CommunicationException, so that Channels can be used polymorphically from
        /// an exception handling perspective.
        /// </summary>
        internal static CommunicationException ConvertTransferException(SocketException socketException)
        {
            return new CommunicationException(
                string.Format(CultureInfo.CurrentCulture, 
                "A Udp error ({0}: {1}) occurred while transmitting data.", socketException.ErrorCode, socketException.Message), 
                socketException);
        }

        internal static bool IsInMulticastRange(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                // 224.0.0.0 through 239.255.255.255
                byte[] addressBytes = address.GetAddressBytes();
                return ((addressBytes[0] & 0xE0) == 0xE0);
                //(address.Address & MulticastIPAddress.IPv4MulticastMask) == MulticastIPAddress.IPv4MulticastMask);
            }
            else
            {
                return address.IsIPv6Multicast;
            }
        }

        internal static void ValidateTimeout(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout must be greater than or equal to TimeSpan.Zero. To disable timeout, specify TimeSpan.MaxValue.");
            }
        }
    }

    internal static class UdpDefaults
    {
        internal const long MaxBufferPoolSize = 64 * 1024;
        internal const int MaxReceivedMessageSize = 5 * 1024 * 1024; //64 * 1024;
        internal const bool Multicast = false;
        internal const bool OrderedSession = true;
        internal const bool ReliableSessionEnabled = true;
        internal const string SessionInactivityTimeoutString = "00:10:00";
    }

    internal static class AddressingVersionConstants
    {
        internal const string WSAddressing10NameSpace = "http://www.w3.org/2005/08/addressing";
        internal const string WSAddressingAugust2004NameSpace = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
    }
}
