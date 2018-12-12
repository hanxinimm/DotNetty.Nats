// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Codecs.NATS.Packets;
using System.Text.RegularExpressions;

namespace DotNetty.Codecs.NATS
{
    public static class NATSVerifications
    {
        private static readonly Regex _subjectRegex = new Regex(@"^[a-zA-Z\d]+(\.[a-zA-Z\d]+)*$", RegexOptions.Compiled);
        private static readonly Regex _wildcardSubjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public static bool ValidateSubject(string subject)
        {
            return _subjectRegex.IsMatch(subject);
        }

        public static bool ValidateWildcardSubscribeSubject(string subject)
        {
            return _wildcardSubjectRegex.IsMatch(subject);
        }
    }
}