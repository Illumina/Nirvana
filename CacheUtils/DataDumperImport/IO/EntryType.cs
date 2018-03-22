namespace CacheUtils.DataDumperImport.IO
{
    internal enum EntryType
    {
        DigitKeyValue,
        DigitKey,
        EmptyListKeyValue,
        EmptyValueKeyValue,
        EndBraces,
        EndBracesWithDataType,
        ListObjectKeyValue,
        MultiLineKeyValue,
        ObjectKeyValue,
        OpenBraces,
        ReferenceStringKeyValue,
        RootObjectKeyValue,
        StringKeyValue,
        UndefKeyValue
    }
}