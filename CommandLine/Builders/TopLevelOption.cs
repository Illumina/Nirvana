using System;
using ErrorHandling;

namespace CommandLine.Builders
{
    public sealed class TopLevelOption
    {
        public readonly string Description;
        public readonly Func<string, string[], ExitCodes> CommandMethod;

        public TopLevelOption(string description, Func<string, string[], ExitCodes> commandMethod)
        {
            Description   = description;
            CommandMethod = commandMethod;
        }
    }
}
