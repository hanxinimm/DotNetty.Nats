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

    public sealed class NATSJetStreamDecoder : NATSDecoder
    {
        delegate bool TryGetStringFromDelimiterDelegate(Span<byte> input, ref int startIndex, out string value);

        public NATSJetStreamDecoder(ILogger logger) : base(logger) { }

        protected override NATSPacket DoHighFrequency(IByteBuffer buffer, string packetSignature, IChannelHandlerContext context)
        {
            var packet = base.DoHighFrequency(buffer, packetSignature, context);
            if (packet != null) return packet;

            switch (packetSignature)
            {
                case NATSSignatures.HMSG:
                    return DecodeMessagePacket(buffer, context);
                default:
                    return null;
            }
        }

        NATSPacket DecodeMessagePacket(IByteBuffer buffer, IChannelHandlerContext context)
        {
            if (TryGetStringFromFieldDelimiter(buffer, NATSSignatures.HMSG, out var subject))
            {
                if (TryGetStringFromFieldDelimiter(buffer, NATSSignatures.HMSG, out var subscribeId))
                {
                    var ReplyTo = GetStringFromFieldDelimiter(buffer, NATSSignatures.HMSG);

                    if (TryGetStringFromFieldDelimiter(buffer, NATSSignatures.HMSG, out var headerSizeString))
                    {
                        if (int.TryParse(headerSizeString, out int headerSize))
                        {
                            if (TryGetStringFromNewlineDelimiter(buffer, NATSSignatures.HMSG, out var totalSizeString))
                            {
                                if (int.TryParse(totalSizeString, out int totalSize))
                                {
                                    if (TryGetBytesFromNewlineDelimiter(buffer, headerSize - 2, NATSSignatures.HMSG, out var header))
                                    {
                                        var payloadSize = totalSize - headerSize;
                                        if (TryGetBytesFromNewlineDelimiter(buffer, payloadSize, NATSSignatures.HMSG, out var payload))
                                        {
                                            return DecodeMessagePacket(subject, subscribeId, ReplyTo, headerSize, header, totalSize, payload);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        NATSPacket DecodeMessagePacket(
            string subject, 
            string subscribeId, 
            string replyTo, 
            int headerSize,
            byte[] header,
            int payloadSize, 
            byte[] payload)
        {
            TryGetStringFromDelimiterDelegate TryGetStringFromDelimiter = TryGetStringFromColonDelimiter;

            var headerDictionary = new Dictionary<string, string>();

            int startIndex = 0;

            if (TryGetStringFromNewlineDelimiter(header, ref startIndex, out string headerVersion))
            {
                startIndex++;

                while (startIndex < header.Length)
                {
                    if (TryGetStringFromDelimiter(header, ref startIndex, out string headerKey))
                    {
                        startIndex++;

                        TryGetStringFromDelimiter = TryGetStringFromNewlineDelimiter;

                        while (startIndex < header.Length)
                        {
                            if (TryGetStringFromDelimiter(header, ref startIndex, out string headerValue))
                            {
                                startIndex++;

                                headerDictionary.Add(headerKey, headerValue);

                                TryGetStringFromDelimiter = TryGetStringFromColonDelimiter;

                                break;
                            }
                            else
                            {
                                startIndex++;
                            }
                        }
                    }
                    else
                    {
                        startIndex++;
                    }
                }
            }

            return new MessageHigherPacket
            {
                Subject = subject,
                SubscribeId = subscribeId,
                ReplyTo = replyTo,
                PayloadSize = payloadSize,
                Payload = payload,
                Version = headerVersion,
                Headers = headerDictionary
            };
        }


        protected override NATSPacket DecodeMessagePacket(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case NATSJetStreamInboxs.GetMessageResponse:
                    return GetMessagePacket<GetMessageResponsePacket, GetMessageResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.PublishResponse:
                    return GetMessagePacket<GetMessageResponsePacket, GetMessageResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.CreateResponse:
                    return GetMessagePacket<CreateResponsePacket, CreateResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.UpdateResponse:
                    return GetMessagePacket<UpdateResponsePacket, UpdateResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.DeleteResponse:
                    return GetMessagePacket<DeleteResponsePacket, DeleteResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ConsumerCreateResponse:
                    return GetMessagePacket<ConsumerCreateResponsePacket, ConsumerCreateResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.NamesResponse:
                    return GetMessagePacket<NamesResponsePacket, NamesResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ListResponse:
                    return GetMessagePacket<ListResponsePacket, ListResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ConsumerInfoResponse:
                    return GetMessagePacket<ConsumerInfoResponsePacket, ConsumerInfoResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ConsumerNamesResponse:
                    return GetMessagePacket<ConsumerNamesResponsePacket, ConsumerNamesResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ConsumerListResponse:
                    return GetMessagePacket<ConsumerListResponsePacket, ConsumerListResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.ConsumerDeleteResponse:
                    return GetMessagePacket<ConsumerDeleteResponsePacket, ConsumerDeleteResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.PurgeResponse:
                    return GetMessagePacket<PurgeResponsePacket, PurgeResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.SnapshotResponse:
                    return GetMessagePacket<SnapshotResponsePacket, SnapshotResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.DeleteMessageResponse:
                    return GetMessagePacket<DeleteMessageResponsePacket, DeleteMessageResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.RemovePeerResponse:
                    return GetMessagePacket<RemovePeerResponsePacket, RemovePeerResponse>(subject, subscribeId, replyTo, payloadSize, payload);
                case NATSJetStreamInboxs.InfoResponse:
                    return GetMessagePacket<InfoResponsePacket, InfoResponse>(subject, subscribeId, replyTo, payloadSize, payload);

                default:
                    return base.DecodeMessagePacket(subject, subscribeId, replyTo, payloadSize, payload);
            }
        }

        static NATSPacket GetMessagePacket<TMessagePacket, TMessage>(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
            where TMessagePacket : MessagePacket<TMessage>, new()
            where TMessage : new()
        {
            var Packet = new TMessagePacket
            {
                Subject = subject,
                SubscribeId = subscribeId,
                ReplyTo = replyTo,
                PayloadSize = payloadSize
            };

            Packet.Message = JsonConvert.DeserializeObject<TMessage>(Encoding.UTF8.GetString(payload));

            return Packet;
        }

        static string GetInbox(string subject)
        {
            if (subject.Length > 17)
            {
                return subject.Substring(0, 17);
            }
            return string.Empty;
        }
    }
}