using ErrorHandling;
using VariantAnnotation.Logger;

namespace CacheUtils.Commands.Download
{
    public static class DownloadMain
    {
        private static ExitCodes ProgramExecution()
        {
            var logger = new ConsoleLogger();
            ExternalFiles.Download(logger);
            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            return ProgramExecution();
        }
    }
}
