namespace Cloud.Messages.Custom
{
    // ReSharper disable NotAccessedField.Global
    // ReSharper disable InconsistentNaming
    public sealed class CustomResult
    {
        public string    id;
        public string    status;
        public string    genomeAssembly;
        public FileList  created;
        public bool      noValidEntries;
        public JwtFields jwtFields;
        public int       variantCount;
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore NotAccessedField.Global
}