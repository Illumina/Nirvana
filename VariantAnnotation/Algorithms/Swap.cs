namespace VariantAnnotation.Algorithms
{
    public static class Swap
    {
        /// <summary>
        /// swaps two integers
        /// </summary>
        public static void Int(ref int a, ref int b)
        {
            var temp = a;
            a = b;
            b = temp;
        }
    }
}