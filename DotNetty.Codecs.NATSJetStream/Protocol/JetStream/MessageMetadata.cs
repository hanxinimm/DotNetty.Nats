using DotNetty.Codecs.NATS.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.NATSJetStream.Protocol
{
    public class MessageMetadata
    {
        public MessageMetadata() { }

        public MessageMetadata(MessagePacket packet)
        {
            var parts = packet.ReplyTo.Split('.');
            if (parts.Length < 8 || parts.Length > 9 || !"$JS".Equals(parts[0]) || !"ACK".Equals(parts[1]))
            {
                throw new NotSupportedException("packet not a  JetStream message");
            }

            Stream = parts[2];
            Consumer = parts[3];
            Delivered = long.Parse(parts[4]);
            StreamSequence = long.Parse(parts[5]);
            ConsumerSequence = long.Parse(parts[6]);

            // not so clever way to separate nanos from seconds
            long tsi = long.Parse(parts[7]);

            try
            {
                Timestamp = NATSJetStreamConvertTimeUnits.ConvertToDateTime(tsi);
            }
            catch (Exception ex)
            { 
                
            }

            if (parts.Length == 9)
            {
                Pending = long.Parse(parts[8]);
            }
        }

        public string Stream { get; set; }
        public string Consumer { get; set; }
        public long Delivered { get; set; }
        public long StreamSequence { get; set; }
        public long ConsumerSequence { get; set; }
        public DateTime Timestamp { get; set; }
        public long? Pending { get; set; }

        public override string ToString()
        {
            return @$"NatsJetStreamMetaData {{
                    stream={ Stream }
                    , consumer='{ Consumer }'
                    , delivered={ Delivered }
                    , streamSeq={ StreamSequence }
                    , consumerSeq={ ConsumerSequence }
                    , timestamp={ Timestamp }
                    , pending={ Pending }
                    }}";
        }
    }
}
