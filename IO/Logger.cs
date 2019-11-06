using System;
using System.Text;

namespace IO
{
    public static class Logger
    {
        // can be redirected to any logger
        public static Action<string> LogLine { get; set; }
        static Logger() => LogLine = Console.WriteLine;

        public static void Log(Exception e)
        {
            var sb = new StringBuilder();
            var line = new string('-', 80);
            sb.AppendLine(line);

            const string vcfLine = "VcfLine";

            while (e != null)
            {
                sb.AppendLine($"{e.GetType()}: {e.Message}");
                sb.AppendLine($"Stack trace: {e.StackTrace}");
                if (e.Data.Contains(vcfLine)) sb.AppendLine($"VCF line: {e.Data[vcfLine]}");

                sb.AppendLine(line);
                e = e.InnerException;
            }

            LogLine(sb.ToString());
        }
    }
}