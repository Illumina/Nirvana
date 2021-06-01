namespace SAUtils.CosmicGeneFusions.Conversion
{
    public sealed record RawCosmicGeneFusion(int SampleId, int FusionId, string PrimarySite, string SiteSubtype1, string PrimaryHistology,
        string HistologySubtype1, string HgvsNotation, int PubMedId);
}