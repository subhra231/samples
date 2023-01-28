//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.Buffers;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Microsoft.Samples.AzureQueueStorage
{
    /// <summary>
    /// IOutputChannel implementation for AzureQueueStorage.
    /// </summary>
    internal class AzureQueueStorageOutputChannel : ChannelBase, IOutputChannel
    {
        #region member_variables
        private EndpointAddress remoteAddress;
        private Uri via;
        private EndPoint remoteEndPoint;
        private MessageEncoder encoder;
        private AzureQueueStorageChannelFactory parent;
        private QueueClient queueClient;
        #endregion

        internal AzureQueueStorageOutputChannel(AzureQueueStorageChannelFactory factory, EndpointAddress remoteAddress, Uri via, MessageEncoder encoder)
            : base(factory)
        {
            //To be implemented
        }

        #region IOutputChannel_Properties
        EndpointAddress IOutputChannel.RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        Uri IOutputChannel.Via
        {
            get
            {
                return this.via;
            }
        }
        #endregion

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IOutputChannel))
            {
                return (T)(object)this;
            }

            T messageEncoderProperty = this.encoder.GetProperty<T>();
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
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }


        #region Socket_Shutdown
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
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }
        #endregion

        #region Send_Synchronous
        /// <summary>
        /// Address the Message and serialize it into a byte array.
        /// </summary>
        private ArraySegment<byte> EncodeMessage(Message message)
        {
            try
            {
                this.remoteAddress.ApplyTo(message);
                return encoder.WriteMessage(message, int.MaxValue, parent.BufferManager);
            }
            finally
            {
                // We have consumed the message by serializing it, so clean up
                message.Close();
            }
        }

        public void Send(Message message)
        {
            try
            {
                queueClient.SendMessage(message.ToString());
            }
            catch
            {

            }
            finally
            {
               
            }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            // UDP does not block so we do not need timeouts.
            this.Send(message);
        }
        #endregion

        #region Send_Asynchronous
        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            //base.ThrowIfDisposedOrNotOpen();
            return new SendAsyncResult(this, message, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            // UDP does not block so we do not need timeouts.
            return this.BeginSend(message, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            SendAsyncResult.End(result);
        }

        /// <summary>
        /// Implementation of async send for Udp. 
        /// </summary>
        private class SendAsyncResult : AsyncResult
        {
            private ArraySegment<byte> messageBuffer;
            private AzureQueueStorageOutputChannel channel;

            public SendAsyncResult(AzureQueueStorageOutputChannel channel, Message message, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.channel = channel;
                this.messageBuffer = channel.EncodeMessage(message);
                try
                {
                    IAsyncResult result = null;
                    try
                    {
                        
                    }
                    catch (SocketException socketException)
                    {
                        throw AzureQueueStorageChannelHelpers.ConvertTransferException(socketException);
                    }

                    if (!result.CompletedSynchronously)
                        return;

                    CompleteSend(result, true);
                }
                catch
                {
                    CleanupBuffer();
                    throw;
                }
            }

            private void CleanupBuffer()
            {
                if (messageBuffer.Array != null)
                {
                    this.channel.parent.BufferManager.ReturnBuffer(messageBuffer.Array);
                    messageBuffer = new ArraySegment<byte>();
                }
            }

            private void CompleteSend(IAsyncResult result, bool synchronous)
            {
                try
                {
                    
                }
                catch (SocketException socketException)
                {
                    throw AzureQueueStorageChannelHelpers.ConvertTransferException(socketException);
                }
                finally
                {
                    CleanupBuffer();
                }

                base.Complete(synchronous);
            }

            private void OnSend(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                try
                {
                    CompleteSend(result, false);
                }
                catch (Exception e)
                {
                    base.Complete(false, e);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SendAsyncResult>(result);
            }
        }
        #endregion
    }
}
