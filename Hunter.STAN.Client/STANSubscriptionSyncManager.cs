using DotNetty.Codecs.STAN.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionSyncManager : STANSubscriptionManager
    {
        public STANSubscriptionSyncManager() { }

        public STANSubscriptionSyncManager(EventResetMode mode) : base(mode) { }
    }
}
