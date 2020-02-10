using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowingBrakes
{
    public static class Logger
    {
        private const string mLogFile = @"scripts\GlowingBrakes\GlowingBrakes.log";

        public enum Level
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
            FATAL,
        }

        public static void Clear()
        {
            File.WriteAllText(mLogFile, string.Empty);
        }

        public static void Log(Level level, string message)
        {
            using (StreamWriter streamWriter = new StreamWriter(mLogFile, true))
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                streamWriter.WriteLine("[{0}] [{1, -5}] {2}", timestamp, level, message);
                streamWriter.Close();
            }
        }
    }
}
