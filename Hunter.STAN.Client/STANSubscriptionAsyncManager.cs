using DotNetty.Codecs.STAN.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAsyncManager : STANSubscriptionManager
    {
        public STANSubscriptionAsyncManager() { }

        public STANSubscriptionAsyncManager(EventResetMode mode) : base(mode) { }



    }
}
