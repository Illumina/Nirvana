using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace OptimizedCore
{
    public static class StringBuilderPool
    {
        private static readonly ObjectPool<StringBuilder> Pool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy(), 1024);

        public static StringBuilder Get()
        {
            var sb = Pool.Get();
            sb.Clear();
            return sb;
        }

        public static string GetStringAndReturn(StringBuilder sb)
        {
            var s = sb.ToString();
            Return(sb);
            return s;
        }

        public static void Return(StringBuilder sb)
        {
            if (sb == null) return;
            Pool.Return(sb);
        }
    }
}