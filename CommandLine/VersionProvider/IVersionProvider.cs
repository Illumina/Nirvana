namespace CommandLine.VersionProvider
{
    public interface IVersionProvider
    {
        string GetProgramVersion();

        string GetDataVersion();
    }
}
