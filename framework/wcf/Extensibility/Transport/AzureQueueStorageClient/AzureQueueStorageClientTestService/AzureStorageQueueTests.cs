// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Azure.Storage.Queues;
using Microsoft.ServiceModel.AQS;
using Xunit;

namespace Microsoft.ServiceModel.AQS.Tests
{
    public class QueueDeclareConfigurationFixture
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=https://127.0.0.1:10001/devstoreaccount1;";
        private readonly string _queueName = "TestQueue";
        private readonly string _remoteAddress = "";
        private readonly string _viaAddress = "";

        public AzureQueueStorageBinding azureQueueStorageBinding;
        public AzureQueueStorageTransportBindingElement azureQueueStorageTransportBindingElement;
        public AzureQueueStorageChannelFactory azureQueueStorageChannelFactory;
        public AzureQueueStorageOutputChannel azureQueueStorageOutputChannel;

        public QueueDeclareConfigurationFixture()
        {
            azureQueueStorageBinding = new AzureQueueStorageBinding(AzureQueueStorageMessageEncoding.Binary);
            azureQueueStorageTransportBindingElement = new AzureQueueStorageTransportBindingElement();
            azureQueueStorageChannelFactory = new AzureQueueStorageChannelFactory(azureQueueStorageTransportBindingElement, null);
            azureQueueStorageOutputChannel = new AzureQueueStorageOutputChannel(azureQueueStorageChannelFactory, new EndpointAddress(new Uri(_remoteAddress)), new Uri(_viaAddress), azureQueueStorageChannelFactory.MessageEncoderFactory.Encoder);
        }
    }

    public class AzureStorageQueueTests : IClassFixture<QueueDeclareConfigurationFixture>
    {
        private QueueDeclareConfigurationFixture _fixture;

        public AzureStorageQueueTests(QueueDeclareConfigurationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AzureQueueStorageBinding_MessageEncoding_IsBinary()
        {
            Assert.Equal(AzureQueueStorageMessageEncoding.Binary, _fixture.azureQueueStorageBinding.MessageEncoding);
        }

        [Fact]
        public void AzureQueueStorageBinding_Scheme()
        {
            Assert.Equal("net.aqs", _fixture.azureQueueStorageBinding.Scheme);
        }

        [Fact]
        public void AzureQueueStorageOutputChannel_SendMessage()
        {
            Message clientMessage = Message.CreateMessage(MessageVersion.Soap11, "Test");
            _fixture.azureQueueStorageOutputChannel.Send(clientMessage);
        }
    }
}
