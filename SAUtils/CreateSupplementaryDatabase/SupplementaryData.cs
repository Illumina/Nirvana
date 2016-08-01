
namespace SAUtils.CreateSupplementaryDatabase
{
    /// <summary>
    /// This is the abstract class that will be extended by each supplementary data type (e.g. dbSnp, cosmic, clinvar, etc.)
    /// </summary>
    public abstract class SupplementaryData
    {
        protected string InputFileName;
        // constructor
        protected SupplementaryData()
        {
            InputFileName = null;
        }

        public abstract void LoadNextDataItem();
    }
}
