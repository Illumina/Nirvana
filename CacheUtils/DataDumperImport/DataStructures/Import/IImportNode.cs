namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public interface IImportNode
    {
        string Key { get; }
    }

    public interface IListMember : IImportNode { }
}
