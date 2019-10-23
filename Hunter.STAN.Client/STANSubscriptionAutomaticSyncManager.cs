﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAutomaticSyncManager : STANSubscriptionSyncManager
    {
        public STANSubscriptionAutomaticSyncManager(int capacity)
        {
            IsAutoUnSubscription = true;
            Messages = new Queue<STANMsgContent>(capacity);
        }
        public Queue<STANMsgContent> Messages { get; set; }
    }
}
