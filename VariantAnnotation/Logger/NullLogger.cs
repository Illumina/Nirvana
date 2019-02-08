using VariantAnnotation.Interface;

namespace VariantAnnotation.Logger
{
    public sealed class NullLogger : ILogger
    {
        public void WriteLine()
        {
            // no output desired
        }

        public void WriteLine(string s)
        {
            // no output desired
        }

        public void Write(string s)
        {
            // no output desired
        }

        public void SetBold()
        {
            // no output desired
        }

        public void ResetColor()
        {
            // no output desired
        }
    }
}
