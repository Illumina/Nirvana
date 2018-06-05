using System.Collections.Generic;

namespace Phantom.Graph
{
    public interface IGraph<T>
    {
        bool TryAddVertex(T vertex);
        void AddEdge(T sourceVertex, T targetVertex);
        IEnumerable<T> GetVertices();
        LinkedList<T> GetNeighbors(T vertex);
    }
}