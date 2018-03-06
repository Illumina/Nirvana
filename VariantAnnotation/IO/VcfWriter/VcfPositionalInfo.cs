namespace VariantAnnotation.IO.VcfWriter
{
    public sealed class VcfPositionalInfo
    {
        #region members

        private readonly string _key;
        private string _value;

        #endregion

        // constructor
        public VcfPositionalInfo(string key)
        {
            _key = key;
        }

        public void AddValue(string s)
        {
            if (s == null) return;
            _value= s;
        }


        /// <summary>
        /// returns a string representation of the VCF positional field
        /// </summary>
        public string GetString()
        {
            return string.IsNullOrEmpty(_value) ? null : $"{_key}={_value}";
        }
    }
}