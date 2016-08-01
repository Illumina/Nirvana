using System;

namespace NDesk.Options
{
    public static class Help
    {
        public static void Show(OptionSet ops, string commonOptions, string description)
        {
            OutputHelper.WriteLabel("USAGE: ");
            Console.WriteLine("dotnet {0} {1}", OutputHelper.GetExecutableName(), commonOptions);
            Console.WriteLine("{0}\n", description);

            OutputHelper.WriteLabel("OPTIONS:");
            Console.WriteLine();
            ops.WriteOptionDescriptions(Console.Out);
        }
    }
}
