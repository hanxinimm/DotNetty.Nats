// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs.NATSJetStream
{
    using DotNetty.Buffers;
    using DotNetty.Codecs.NATS;
    using DotNetty.Codecs.Protocol;
    using DotNetty.Codecs.NATSJetStream.Packets;
    using DotNetty.Codecs.NATSJetStream.Protocol;
    using DotNetty.Transport.Channels;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;
    using DotNetty.Codecs.NATS.Packets;

    public sealed class STANDecoder : ZeroAllocationByteDecoder
    {
        public STANDecoder(ILogger logger) : base(logger) { }

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

        NATSPacket DecodePacketInternal(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            switch (packetSignature)
            {
                case NATSJetStreamSignatures.MSG:
                    return DecodeMessagePacket(buffer, context);
                default:
#if DEBUG
                    Console.WriteLine("--|{0}|--", packetSignature);
                    throw new DecoderException($"NATS protocol operation name of `{packetSignature}` is invalid.");
#else
                    return null;
#endif
            }
        }

        NATSPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {

            if (TryGetStringFromFieldDelimiter(buffer, NATSJetStreamSignatures.MSG, out var subject))
            {
                if (TryGetStringFromFieldDelimiter(buffer, NATSJetStreamSignatures.MSG, out var subscribeId))
                {
                    var ReplyTo = GetStringFromFieldDelimiter(buffer, NATSJetStreamSignatures.MSG);

                    if (TryGetStringFromNewlineDelimiter(buffer, NATSJetStreamSignatures.MSG, out var payloadSizeString))
                    {

                        if (int.TryParse(payloadSizeString, out int payloadSize))
                        {
                            if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, NATSJetStreamSignatures.MSG, out var payload))
                            {
                                return DecodeMessagePacket(subject, ReplyTo, payloadSize, payload);
                            }
                        }
                    }
                }
            }

            return null;
        }

        static NATSPacket DecodeMessagePacket(string subject, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case NATSJetStreamInboxs.CreateResponse:
                    return GetMessagePacket<CreateResponsePacket, CreateResponse>(subject, replyTo, payloadSize, payload);
                default:
                    return null;
            }
        }

        static NATSPacket GetMessagePacket<TMessagePacket, TMessage>(string subject, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket<TMessage>, new()
            where TMessage : new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                ReplyTo = replyTo,
                PayloadSize = payloadSize
            };

            Packet.Message = JsonConvert.DeserializeObject<TMessage>(Encoding.UTF8.GetString(payload));

            return Packet;
        }

        static NATSPacket GetMessagePacket<TMessagePacket>(string subject, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket, new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                ReplyTo = replyTo
            };
            return Packet;
        }

        NATSPacket DecodeErrorPacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            //if (TryGetStringFromNewlineDelimiter(buffer, STANSignatures.ERR, out var error))
            //{
            //    switch (error)
            //    {
            //        case NATSErrors.UnknownProtocolOperation:
            //            return new UnknownProtocolOperationErrorPacket();
            //        case NATSErrors.AttemptedToConnectToRoutePort:
            //            return new AttemptedToConnectToRoutePortErrorPacket();
            //        case NATSErrors.AuthorizationViolation:
            //            return new AuthorizationViolationErrorPacket();
            //        case NATSErrors.AuthorizationTimeout:
            //            return new AuthorizationTimeoutErrorPacket();
            //        case NATSErrors.InvalidClientProtocol:
            //            return new InvalidClientProtocolErrorPacket();
            //        case NATSErrors.MaximumControlLineExceeded:
            //            return new MaximumControlLineExceededErrorPacket();
            //        case NATSErrors.ParserError:
            //            return new ParserErrorPacket();
            //        case NATSErrors.SecureConnection_TLSRequired:
            //            return new SecureConnectionTLSRequiredErrorPacket();
            //        case NATSErrors.StaleConnection:
            //            return new StaleConnectionErrorPacket();
            //        case NATSErrors.MaximumConnectionsExceeded:
            //            return new MaximumConnectionsExceededErrorPacket();
            //        case NATSErrors.SlowConsumer:
            //            return new SlowConsumerErrorPacket();
            //        case NATSErrors.MaximumPayloadViolation:
            //            return new MaximumPayloadViolationErrorPacket();

            //        case NATSErrors.InvalidSubject:
            //            return new InvalidSubjectErrorPacket();
            //        default:

            //            break;
            //    }

            //    var PublishSubjectError = NATSErrors.PermissionsViolationForPublish.Match(error);
            //    if (PublishSubjectError.Success)
            //    {
            //        return new PermissionsViolationForPublishErrorPacket(PublishSubjectError.Groups[1].Value);
            //    }
            //    var SubscriptionSubjectError = NATSErrors.PermissionsViolationForSubscription.Match(error);
            //    if (SubscriptionSubjectError.Success)
            //    {
            //        return new PermissionsViolationForSubscriptionErrorPacket(PublishSubjectError.Groups[1].Value);
            //    }
            //    return new UnknownErrorPacket(error);
            //}
            return null;
        }
    }
}