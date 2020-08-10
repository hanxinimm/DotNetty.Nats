using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAutomaticAsyncManager : STANSubscriptionAsyncManager
    {
        public STANSubscriptionAutomaticAsyncManager()
        {
            IsAutoUnSubscription = true;
            Messages = new Queue<STANMsgContent>();
        }

        public STANSubscriptionAutomaticAsyncManager(int capacity)
        {
            IsAutoUnSubscription = true;
            Messages = new Queue<STANMsgContent>(capacity);
        }
        public Queue<STANMsgContent> Messages { get; set; }
    }
}
