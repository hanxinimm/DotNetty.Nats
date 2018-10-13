using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Provides decoded messages received by subscriptions or requests.
    /// </summary>
    public class EncodedMessageEventArgs : EventArgs
    {
        public string subject = null;
        public string reply = null;
        public object obj = null;
        public Message msg = null;

        public EncodedMessageEventArgs() { }

        /// <summary>
        /// Gets the subject for the received <see cref="Client.Message"/>.
        /// </summary>
        public string Subject
        {
            get { return subject; }
        }

        /// <summary>
        /// Gets the reply topic for the received <see cref="Client.Message"/>.
        /// </summary>
        public string Reply
        {
            get { return reply; }
        }

        /// <summary>
        /// Gets the object decoded (deserialized) from the incoming message.
        /// </summary>
        public object ReceivedObject
        {
            get
            {
                return obj;
            }
        }

        /// <summary>
        /// Gets the original <see cref="Client.Message"/> that <see cref="ReceivedObject"/> was deserialized from.
        /// </summary>
        public Message Message
        {
            get
            {
                return this.msg;
            }
        }
    }
}
