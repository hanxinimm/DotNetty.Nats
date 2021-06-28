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
        public NATSJetStreamDecoder(ILogger logger) : base(logger) { }

        protected override NATSPacket DecodeMessagePacket(string subject, string subscribeId, string replyTo, int payloadSize, byte[] payload)
        {
            switch (GetInbox(subject))
            {
                case NATSJetStreamInboxs.CreateResponse:
                    return GetMessagePacket<CreateResponsePacket, CreateResponse>(subject, subscribeId, replyTo, payloadSize, payload);
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