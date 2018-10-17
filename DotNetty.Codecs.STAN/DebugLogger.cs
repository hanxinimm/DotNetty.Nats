using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotNetty.Codecs.STAN
{
    public static class DebugLogger
    {
        private static readonly string LogFileName;
        private static bool IsDebug = true;
        static DebugLogger()
        {
            if (IsDebug)
                LogFileName = $@"D:\decod-{Guid.NewGuid().ToString("N")}.log";
        }

        public static void LogSignature(string content)
        {
            if (IsDebug)
                File.AppendAllText(LogFileName, $"[SIGNATURE]{Clear(content)}[SIGNATURE]\r\n");
        }

        public static void LogBaseMSG(string content)
        {
            if (IsDebug)
                File.AppendAllText(LogFileName, $"[BASEMSG]{Clear(content)}[BASEMSG]\r\n");
        }

        public static void LogMSG(string content)
        {
            if (IsDebug)
                File.AppendAllText(LogFileName, $"[MSG]{Clear(content)}[MSG]\r\n");
        }

        public static string Clear(string content)
        {
            return content.Replace("\r", " ").Replace("\n", " ");
        }

    }
}
