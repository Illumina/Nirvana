namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class ObjectKeyValueNode : IImportNode
    {
        public string Key { get; }
        public ObjectValueNode Value { get; }

        public ObjectKeyValueNode(string key, ObjectValueNode value)
        {
            Key   = key;
            Value = value;
        }
    }
}
