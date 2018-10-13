// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Codecs.STAN.Packets;
using System.Text.RegularExpressions;

namespace DotNetty.Codecs.STAN
{
    internal static class STANExtensions
    {
        private static readonly Regex _subjectRegex = new Regex(@"^[a-zA-Z\d]+(\.(\*|\>|[a-zA-Z\d]+))*$", RegexOptions.Compiled);

        public static void ValidateTopicName(this PublishPacket packet)
        {
            if (!_subjectRegex.IsMatch(packet.Subject))
            {
                throw new DecoderException($"Invalid PUBLISH subject name: {packet.Subject}");
            }
        }
    }
}