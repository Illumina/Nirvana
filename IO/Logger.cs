using System;

namespace IO
{
    public static class Logger
    {
        // can be redirected to any logger
        public static Action<string> LogLine { get; set; }
        static Logger() => LogLine = Console.WriteLine;
    }
}