using DotNetty.Codecs.STAN.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetty.Codecs.STAN.Packets
{
    public class MsgProtoPacket : STANPacket<MsgProto>
    {
        public MsgProtoPacket()
        {
            Message = new MsgProto();
        }

        public override STANPacketType PacketType => STANPacketType.MsgProto;

    }
}
