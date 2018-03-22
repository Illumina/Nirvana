namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class StringValueNode : IListMember
    {
        public string Key { get; }
        public StringValueNode(string key) => Key = key;
    }
}
