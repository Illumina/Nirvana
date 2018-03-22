using System;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Logger
{
    public sealed class ConsoleLogger : ILogger
    {
        public void WriteLine()         => Console.WriteLine();
        public void WriteLine(string s) => Console.WriteLine(s);
        public void Write(string s)     => Console.Write(s);
        public void SetBold()           => Console.ForegroundColor = ConsoleColor.Yellow;
        public void ResetColor()        => Console.ResetColor();
    }
}
