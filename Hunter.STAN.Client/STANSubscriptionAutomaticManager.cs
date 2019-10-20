using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Hunter.STAN.Client
{
    public class STANSubscriptionAutomaticManager : STANSubscriptionManager
    {
        public STANSubscriptionAutomaticManager(int capacity) : base(EventResetMode.AutoReset)
        {
            Messages = new Queue<STANMsgContent>(capacity);
        }
        public Queue<STANMsgContent> Messages { get; set; }
    }
}
