namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class StringKeyValueNode : IImportNode
    {
        public string Key { get; }
        public string Value { get; }

        public StringKeyValueNode(string key, string value)
        {
            Key   = key;
            Value = value;
        }
    }
}
