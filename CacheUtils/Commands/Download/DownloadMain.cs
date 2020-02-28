using ErrorHandling;

namespace CacheUtils.Commands.Download
{
    public static class DownloadMain
    {
        private static ExitCodes ProgramExecution()
        {
            ExternalFiles.Download();
            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            return ProgramExecution();
        }
    }
}
