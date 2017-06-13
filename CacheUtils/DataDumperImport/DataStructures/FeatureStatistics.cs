namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class FeatureStatistics
    {
        private readonly string _description;
        private int _numAdded;
        private int _numUnique;

        /// <summary>
        /// constructor
        /// </summary>
        public FeatureStatistics(string description)
        {
            _description = description;
        }

        /// <summary>
        /// increments the statistics
        /// </summary>
        public void Increment(int numUnique, int numAdded)
        {
            _numUnique += numUnique;
            _numAdded  += numAdded;
        }

        /// <summary>
        /// returns a string representation of this object
        /// </summary>
        public override string ToString()
        {
            var description = $"{_description}:";
            double percent  = _numUnique / (double)_numAdded * 100.0;
            if (double.IsNaN(percent)) percent = 0.0;
            return $"{description,18} unique: {_numUnique,7}, added: {_numAdded,7} ({percent,5:0.0} %)";
        }
    }
}
