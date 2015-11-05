using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Patchwork.Attributes;

namespace W2PWMod.Helper
{
    [NewType]
    public static class W2ModDebug
    {
        private static readonly TextWriter _logger;
        private static readonly Stream _stream;

        static W2ModDebug()
        {
            _stream = File.Open("W2PWMod.log", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            _logger = new StreamWriter(_stream);
        }

        public static void Log(object format)
        {
            if (format == null || (format is string && string.IsNullOrEmpty((string)format))) return;
            _logger.WriteLine(format);
            _logger.Flush();
            _stream.Flush();
        }

        public static void Log(string format, params object[] args)
        {
            Log((object)string.Format(format, args));
        }

    }
}
