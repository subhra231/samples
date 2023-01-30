//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ServiceModel.AQS
{
    public class SampleProfileAzureQueueStorageBinding : Binding
    {
        private AzureQueueStorageTransportBindingElement _transport;
        private MessageEncodingBindingElement _encoding;

        public SampleProfileAzureQueueStorageBinding()
        {
            Initialize();
        }

        public override string Scheme { get { return "soap.aqs"; } }

        public EnvelopeVersion SoapVersion
        {
            get { return EnvelopeVersion.Soap12; }
        }

        /// <summary>
        /// Create the set of binding elements that make up this binding. 
        /// NOTE: order of binding elements is important.
        /// </summary>
        /// <returns></returns>
        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection
            {
                _encoding,
                _transport
            };

            return bindingElements.Clone();
        }

        private void Initialize()
        {
            _transport = new AzureQueueStorageTransportBindingElement();
            _encoding = new TextMessageEncodingBindingElement();
        }
    }
}
