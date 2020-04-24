namespace SAUtils.Omim.EntryApiResponse
{
    // ReSharper disable InconsistentNaming
    public sealed class EntryRoot
    {
        public RootItem omim;
    }

    // ReSharper disable ClassNeverInstantiated.Global
    public class RootItem
    {
        public string  version;
        public Entry[] entryList;
    }

    public class Entry
    {
        public EntryItem entry;
    }

    public class EntryItem
    {
        public char          prefix;
        public int           mimNumber;
        public string        status;
        public TextSection[] textSectionList;
        public GeneMap       geneMap;
    }

    public class TextSection
    {
        public TextSectionItem textSection;
    }

    public class TextSectionItem
    {
        public string textSectionName;
        public string textSectionTitle;
        public string textSectionContent;
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
}