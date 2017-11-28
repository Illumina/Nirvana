using VariantAnnotation.Interface;

namespace VariantAnnotation.Logger
{
    public sealed class NullLogger : ILogger
    {
        public void WriteLine()         { }
        public void WriteLine(string s) { }
        public void Write(string s)     { }
        public void SetBold()           { }
        public void ResetColor()        { }
    }
}
