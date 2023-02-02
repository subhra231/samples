﻿//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Microsoft.ServiceModel.AQS
{
    /// <summary>
    /// IOutputChannel implementation for AzureQueueStorage.
    /// </summary>
    internal class AzureQueueStorageOutputChannel : ChannelBase, IOutputChannel
    {
        #region member_variables
        private EndpointAddress _remoteAddress;
        private Uri _via;
        private MessageEncoder _encoder;
        private AzureQueueStorageChannelFactory _parent;
        private QueueClient _queueClient;
        private ArraySegment<byte> _messageBuffer;
        #endregion

        internal AzureQueueStorageOutputChannel(AzureQueueStorageChannelFactory factory, EndpointAddress remoteAddress, Uri via, MessageEncoder encoder)
            : base(factory)
        {
            this._remoteAddress = remoteAddress;
            this._via = via;
            this._encoder = encoder;

            Uri queueUri = AzureQueueStorageQueueNameConverter.ConvertToHttpEndpointUrl(via);
            var credential = new DefaultAzureCredential();
            _queueClient = new QueueClient(queueUri, credential);
        }

        #region IOutputChannel_Properties
        EndpointAddress IOutputChannel.RemoteAddress
        {
            get
            {
                return this._remoteAddress;
            }
        }

        Uri IOutputChannel.Via
        {
            get
            {
                return this._via;
            }
        }
        #endregion

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IOutputChannel))
            {
                return (T)(object)this;
            }

            T messageEncoderProperty = this._encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            return base.GetProperty<T>();
        }

        /// <summary>
        /// Open the channel for use. We do not have any blocking work to perform so this is a no-op
        /// </summary>
        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task.CompletedTask.ToApm(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            result.ToApmEnd();
        }


        #region Shutdown
        /// <summary>
        /// Shutdown ungracefully
        /// </summary>
        protected override void OnAbort()
        {

        }

        /// <summary>
        /// Shutdown gracefully
        /// </summary>
        protected override void OnClose(TimeSpan timeout)
        {

        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);
            return Task.CompletedTask.ToApm(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            result.ToApmEnd();
        }
        #endregion

        #region Send_Synchronous
        public void Send(Message message)
        {
            this.Send(message, default);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            CancellationTokenSource cts = new(timeout);
            try
            {
                ArraySegment<byte> messageBuffer = EncodeMessage(message);
                BinaryData binaryData = new(new ReadOnlyMemory<byte>(messageBuffer.Array, messageBuffer.Offset, messageBuffer.Count));
                _queueClient.SendMessage(binaryData, default, default, cts.Token);
            }
            catch (Exception e)
            {
                throw AzureQueueStorageChannelHelpers.ConvertTransferException(e);
            }
            finally
            {
                CleanupBuffer();
                cts.Dispose();
            }
        }
        #endregion

        #region Send_Asynchronous
        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {      
            return BeginSend(message, default, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            AzureQueueStorageChannelHelpers.ThrowIfDisposedOrNotOpen(state);
            return SendAsync(message, timeout).ToApm(callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            result.ToApmEnd();
        }

        private async Task SendAsync(Message message, TimeSpan timeout)
        {
            CancellationTokenSource cts = new(timeout);
            
            try
            {
                _messageBuffer = EncodeMessage(message);
                BinaryData binaryData = new(new ReadOnlyMemory<byte>(_messageBuffer.Array, _messageBuffer.Offset, _messageBuffer.Count));
                await _queueClient.SendMessageAsync(binaryData, default, default, cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw AzureQueueStorageChannelHelpers.ConvertTransferException(e);
            }
            finally
            {
                CleanupBuffer();
                cts.Dispose();
            }
        }
        #endregion

        /// <summary>
        /// Address the Message and serialize it into a byte array.
        /// </summary>
        private ArraySegment<byte> EncodeMessage(Message message)
        {
            try
            {
                this._remoteAddress.ApplyTo(message);
                return _encoder.WriteMessage(message, int.MaxValue, _parent.BufferManager);
            }
            finally
            {
                // We have consumed the message by serializing it, so clean up
                message.Close();
            }
        }

        private void CleanupBuffer()
        {
            if (_messageBuffer.Array != null)
            {
                _parent.BufferManager.ReturnBuffer(_messageBuffer.Array);
                _messageBuffer = new ArraySegment<byte>();
            }
        }
    }
}
