// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATS
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS.Packets;
    using DotNetty.Codecs.Protocol;
    using DotNetty.Transport.Channels;

    public sealed class NATSDecoder : ZeroAllocationByteDecoder
    {
        public static readonly NATSDecoder Instance = new NATSDecoder();

        protected override ProtocolPacket DecodePacket(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            return DecodePacketInternal(buffer, packetSignature, context);
        }

        static NATSPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case ProtocolSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case ProtocolSignatures.MSG:
                    return DecodeMessagePacket(buffer, context);
                case ProtocolSignatures.OK:
                    return DecodeOKPacket(buffer, context);
                case ProtocolSignatures.PING:
                    return DecodePingPacket(buffer, context);
                case ProtocolSignatures.PONG:
                    return DecodePongPacket(buffer, context);
                case ProtocolSignatures.ERR:
                    return DecodeErrorPacket(buffer, context);
                default:
#if DEBUG
                    Console.WriteLine("--|{0}|--", packetSignature);
                    throw new DecoderException($"NATS protocol operation name of `{packetSignature}` is invalid.");
#else
                    return null;
#endif
            }
        }

        static NATSPacket DecodeInfoPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, ProtocolSignatures.INFO, out var infoJson))
            {
                return InfoPacket.CreateFromJson(infoJson);
            }
            return null;
        }

        static NATSPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            if (TryGetStringFromFieldDelimiter(buffer, ProtocolSignatures.MSG, out var subject))
            {
                if (TryGetStringFromFieldDelimiter(buffer, ProtocolSignatures.MSG, out var subscribeId))
                {
                    var ReplyTo = GetStringFromFieldDelimiter(buffer, ProtocolSignatures.MSG);

                    if (TryGetStringFromNewlineDelimiter(buffer, ProtocolSignatures.MSG, out var payloadSizeString))
                    {

                        if (int.TryParse(payloadSizeString, out int payloadSize))
                        {
                            if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, ProtocolSignatures.MSG, out var payload))
                            {

                                return new MessagePacket
                                {
                                    Subject = subject,
                                    SubscribeId = subscribeId,
                                    ReplyTo = ReplyTo,
                                    PayloadSize = payloadSize,
                                    Payload = payload
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }

        static NATSPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, ProtocolSignatures.ERR, out var error))
            {
                switch (error)
                {
                    case NATSErrors.UnknownProtocolOperation:
                        return new UnknownProtocolOperationErrorPacket();
                    case NATSErrors.AttemptedToConnectToRoutePort:
                        return new AttemptedToConnectToRoutePortErrorPacket();
                    case NATSErrors.AuthorizationViolation:
                        return new AuthorizationViolationErrorPacket();
                    case NATSErrors.AuthorizationTimeout:
                        return new AuthorizationTimeoutErrorPacket();
                    case NATSErrors.InvalidClientProtocol:
                        return new InvalidClientProtocolErrorPacket();
                    case NATSErrors.MaximumControlLineExceeded:
                        return new MaximumControlLineExceededErrorPacket();
                    case NATSErrors.ParserError:
                        return new ParserErrorPacket();
                    case NATSErrors.SecureConnection_TLSRequired:
                        return new SecureConnectionTLSRequiredErrorPacket();
                    case NATSErrors.StaleConnection:
                        return new StaleConnectionErrorPacket();
                    case NATSErrors.MaximumConnectionsExceeded:
                        return new MaximumConnectionsExceededErrorPacket();
                    case NATSErrors.SlowConsumer:
                        return new SlowConsumerErrorPacket();
                    case NATSErrors.MaximumPayloadViolation:
                        return new MaximumPayloadViolationErrorPacket();

                    case NATSErrors.InvalidSubject:
                        return new InvalidSubjectErrorPacket();
                    default:

                        break;
                }

                var PublishSubjectError = NATSErrors.PermissionsViolationForPublish.Match(error);
                if (PublishSubjectError.Success)
                {
                    return new PermissionsViolationForPublishErrorPacket(PublishSubjectError.Groups[1].Value);
                }
                var SubscriptionSubjectError = NATSErrors.PermissionsViolationForSubscription.Match(error);
                if (SubscriptionSubjectError.Success)
                {
                    return new PermissionsViolationForSubscriptionErrorPacket(PublishSubjectError.Groups[1].Value);
                }

                return new UnknownErrorPacket(error);
            }
            return null;
        }

        static NATSPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new OKPacket();
        }

        static NATSPacket DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PingPacket();
        }

        static NATSPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PongPacket();
        }

    }
}