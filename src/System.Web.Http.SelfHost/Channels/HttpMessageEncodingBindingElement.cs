// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.Channels
{
    /// <summary>
    /// Provides an <see cref="HttpMessageEncoderFactory"/> that returns a <see cref="MessageEncoder"/> 
    /// that is able to produce and consume <see cref="HttpMessage"/> instances.
    /// </summary>
    internal sealed class HttpMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        private static readonly Type _replyChannelType = typeof(IReplyChannel);

        /// <summary>
        /// Gets or sets the message version that can be handled by the message encoders produced by the message encoder factory.
        /// </summary>
        /// <returns>The <see cref="MessageVersion"/> used by the encoders produced by the message encoder factory.</returns>
        public override MessageVersion MessageVersion
        {
            get { return MessageVersion.None; }

            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("value");
                }

                if (value != MessageVersion.None)
                {
                    throw Error.NotSupported(SRResources.OnlyMessageVersionNoneSupportedOnHttpMessageEncodingBindingElement, typeof(HttpMessageEncodingBindingElement).Name);
                }
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the binding element can build a listener for a specific type of channel.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the listener accepts.</typeparam>
        /// <param name="context">The <see cref="BindingContext"/> that provides context for the binding element</param>
        /// <returns>true if the <see cref="IChannelListener{TChannel}"/> of type <see cref="IChannel"/> can be built by the binding element; otherwise, false.</returns>
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the binding element can build a channel factory for a specific type of channel.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the channel factory produces.</typeparam>
        /// <param name="context">The <see cref="BindingContext"/> that provides context for the binding element</param>
        /// <returns>ALways false.</returns>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            throw Error.NotSupported(SRResources.ChannelFactoryNotSupported, typeof(HttpMessageEncodingBindingElement).Name, typeof(IChannelFactory).Name);
        }

        /// <summary>
        /// Returns a value that indicates whether the binding element can build a channel factory for a specific type of channel.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the channel factory produces.</typeparam>
        /// <param name="context">The <see cref="BindingContext"/> that provides context for the binding element</param>
        /// <returns>ALways false.</returns>
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            context.BindingParameters.Add(this);

            return IsChannelShapeSupported<TChannel>() && context.CanBuildInnerChannelListener<TChannel>();
        }

        /// <summary>
        /// Initializes a channel listener to accept channels of a specified type from the binding context.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the listener is built to accept.</typeparam>
        /// <param name="context">The <see cref="BindingContext"/> that provides context for the binding element</param>
        /// <returns>The <see cref="IChannelListener{TChannel}"/> of type <see cref="IChannel"/> initialized from the context.</returns>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (!IsChannelShapeSupported<TChannel>())
            {
                throw Error.NotSupported(SRResources.ChannelShapeNotSupported, typeof(HttpMessageEncodingBindingElement).Name, typeof(IReplyChannel).Name);
            }

            context.BindingParameters.Add(this);

            IChannelListener<IReplyChannel> innerListener = context.BuildInnerChannelListener<IReplyChannel>();

            if (innerListener == null)
            {
                return null;
            }

            return (IChannelListener<TChannel>)new HttpMessageEncodingChannelListener(context.Binding, innerListener);
        }

        /// <summary>
        /// Returns a copy of the binding element object.
        /// </summary>
        /// <returns>A <see cref="BindingElement"/> object that is a deep clone of the original.</returns>
        public override BindingElement Clone()
        {
            return new HttpMessageEncodingBindingElement();
        }

        /// <summary>
        /// Creates a factory for producing message encoders that are able to 
        /// produce and consume <see cref="HttpMessage"/> instances.
        /// </summary>
        /// <returns>
        /// The <see cref="MessageEncoderFactory"/> used to produce message encoders that are able to 
        /// produce and consume <see cref="HttpMessage"/> instances.
        /// </returns>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new HttpMessageEncoderFactory();
        }

        private static bool IsChannelShapeSupported<TChannel>()
        {
            return typeof(TChannel) == _replyChannelType;
        }
    }
}
