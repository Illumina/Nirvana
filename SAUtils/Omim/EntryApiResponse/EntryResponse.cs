namespace SAUtils.Omim.EntryApiResponse
{
    public class EntryRoot
    {
        public RootItem omim;
    }


    public class RootItem
    {
        public Entry[] entryList;
    }

    public class Entry
    {
        public EntryItem entry;
    }

    public class EntryItem
    {
        public int mimNumber;
        public TextSection[] textSectionList;
        public GeneMap geneMap;
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
}