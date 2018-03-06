namespace VariantAnnotation.Interface
{
    public interface ILogger
    {
        void WriteLine();
        void WriteLine(string s);
        void Write(string s);
        void SetBold();
        void ResetColor();
    }
}
