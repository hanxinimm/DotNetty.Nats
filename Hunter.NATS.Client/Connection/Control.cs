using System;
using System.Collections.Generic;
using System.Text;

namespace Hunter.NATS.Client
{
    internal class Control
    {
        // for efficiency, assign these once in the contructor;
        internal string op;
        internal string args;

        static readonly internal char[] separator = { ' ' };

        // ensure this object is always created with a string.
        private Control() { }

        internal Control(string s)
        {
            string[] parts = s.Split(separator, 2);

            if (parts.Length == 1)
            {
                op = parts[0].Trim();
                args = NATSConstants._EMPTY_;
            }
            if (parts.Length == 2)
            {
                op = parts[0].Trim();
                args = parts[1].Trim();
            }
            else
            {
                op = NATSConstants._EMPTY_;
                args = NATSConstants._EMPTY_;
            }
        }
    }
}
