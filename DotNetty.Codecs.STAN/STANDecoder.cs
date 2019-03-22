// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.STAN
{
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS;
    using DotNetty.Codecs.Protocol;
    using DotNetty.Codecs.STAN.Packets;
    using DotNetty.Codecs.STAN.Protocol;
    using DotNetty.Transport.Channels;
    using Google.Protobuf;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class STANDecoder : ZeroAllocationByteDecoder
    {
        public static readonly STANDecoder Instance = new STANDecoder();

        protected override ProtocolPacket DecodePacket(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            return DecodePacketInternal(buffer, packetSignature, context);
        }

        static string GetInbox(string subject)
        {
            if (subject.Length > 12)
            {
                return subject.Substring(0, 12);
            }
            return string.Empty;
        }

        static STANPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case STANSignatures.INFO:
                    return DecodeInfoPacket(buffer, context);
                case STANSignatures.MSG:
                    return DecodeMessagePacket(buffer, context);
                case STANSignatures.OK:
                    return DecodeOKPacket(buffer, context);
                case STANSignatures.PING:
                    return DecodePingPacket(buffer, context);
                case STANSignatures.PONG:
                    return DecodePongPacket(buffer, context);
                case STANSignatures.ERR:
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

        static STANPacket DecodeInfoPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.INFO, out var infoJson))
            {
                return InfoPacket.CreateFromJson(infoJson);
            }
            return null;
        }

        static STANPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            if (TryGetStringFromFieldDelimiter(buffer, STANSignatures.MSG, out var subject))
            {
                if (TryGetStringFromFieldDelimiter(buffer, STANSignatures.MSG, out var subscribeId))
                {
                    var ReplyTo = GetStringFromFieldDelimiter(buffer, STANSignatures.MSG);

                    if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.MSG, out var payloadSizeString))
                    {

                        if (int.TryParse(payloadSizeString, out int payloadSize))
                        {
                            if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, STANSignatures.MSG, out var payload))
                            {
                                return DecodeMessagePacket(subject, ReplyTo, payloadSize, payload);
                            }
                        }
                    }
                }
            }

            return null;
        }

        static STANPacket DecodeMessagePacket(string subject, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case STANInboxs.Heartbeat:
                    return GetMessagePacket<HeartbeatPacket>(subject, replyTo, payloadSize, payload);
                case STANInboxs.ConnectResponse:
                    return GetMessagePacket<ConnectResponsePacket, ConnectResponse>(subject, replyTo, payloadSize, payload);
                case STANInboxs.SubscriptionResponse:
                    return GetMessagePacket<SubscriptionResponsePacket, SubscriptionResponse>(subject, replyTo, payloadSize, payload);
                case STANInboxs.UnSubscriptionResponse:
                    return GetMessagePacket<UnSubscriptionResponsePacket>(subject, replyTo, payloadSize, payload);
                case STANInboxs.PubAck:
                    return GetMessagePacket<PubAckPacket, PubAck>(subject, replyTo, payloadSize, payload);
                case STANInboxs.MsgProto:
                    return GetMessagePacket<MsgProtoPacket, MsgProto>(subject, replyTo, payloadSize, payload);
                case STANInboxs.CloseResponse:
                    return GetMessagePacket<CloseResponsePacket, CloseResponse>(subject, replyTo, payloadSize, payload);
                default:
                    return null;
            }
        }

        static STANPacket GetMessagePacket<TMessagePacket, TMessage>(string subject, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket<TMessage>, new()
            where TMessage : IMessage, new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                ReplyTo = replyTo,
                PayloadSize = payloadSize
            };

            var Message = new TMessage();
            Message.MergeFrom(payload);
            Packet.Message = Message;

            return Packet;
        }

        static STANPacket GetMessagePacket<TMessagePacket>(string subject, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket, new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                ReplyTo = replyTo
            };
            return Packet;
        }

        static STANPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.ERR, out var error))
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

        static STANPacket DecodeOKPacket(IByteBuffer buffer, IChannelHandlerContext context)
        { 
            return new OKPacket();
        }

        static STANPacket DecodePingPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PingPacket();
        }

        static STANPacket DecodePongPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            return new PongPacket();
        }
    }
}