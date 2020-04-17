using System;

namespace Downloader.Utilities
{
    public static class ConsoleEmbellishments
    {
        public static void PrintWarning(string s) => Highlight(s, ConsoleColor.Yellow);

        public static void PrintError(string s) => Highlight(s, ConsoleColor.Red);
        
        public static void PrintSuccess(string s) => Highlight(s, ConsoleColor.Green);

        private static void Highlight(string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(s);
            Console.ResetColor();
        }
    }
}