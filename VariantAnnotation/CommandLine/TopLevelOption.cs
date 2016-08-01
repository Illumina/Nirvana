using System;

namespace VariantAnnotation.CommandLine
{
    public class TopLevelOption
    {
        public readonly string Description;
        public readonly Func<string, string[], int> CommandMethod;

        public TopLevelOption(string description, Func<string, string[], int> commandMethod)
        {
            Description   = description;
            CommandMethod = commandMethod;
        }
    }
}
