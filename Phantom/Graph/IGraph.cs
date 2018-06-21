using System.Collections.Generic;

namespace Phantom.Graph
{
    public interface IGraph<T>
    {
        IEnumerable<T> GetVertices();
        LinkedList<T> GetNeighbors(T vertex);
    }
}