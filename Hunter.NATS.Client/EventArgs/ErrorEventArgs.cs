using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    /// <summary>
    /// Provides details for an error encountered asynchronously
    /// by an <see cref="INATSConnection"/>.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        private NATSConnection c;
        private Subscription s;
        private String err;

        internal ErrorEventArgs(NATSConnection c, Subscription s, String err)
        {
            this.c = c;
            this.s = s;
            this.err = err;
        }

        /// <summary>
        /// Gets the <see cref="NATSConnection"/> associated with the event.
        /// </summary>
        public NATSConnection Conn
        {
            get { return c; }
        }

        /// <summary>
        /// Gets the <see cref="Hunter.NATS.Client.Subscription"/> associated with the event.
        /// </summary>
        public Subscription Subscription
        {
            get { return s; }
        }

        /// <summary>
        /// Gets the error message associated with the event.
        /// </summary>
        public string Error
        {
            get { return err; }
        }
    }
}
