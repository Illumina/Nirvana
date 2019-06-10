using System.Collections.Generic;
using System.Linq;

namespace Phantom.Graph
{
    public sealed class Graph<T> : IGraph<T>
    {
        private readonly Dictionary<T, LinkedList<T>> _vertexToNeighbors = new Dictionary<T, LinkedList<T>>();
        private readonly bool _isDirected;

        public Graph(bool isDirected)
        {
            _isDirected = isDirected;
        }

        public void TryAddVertex(T vertex)
        {
            if (HasVertex(vertex)) return;
            _vertexToNeighbors[vertex] = new LinkedList<T>();
        }

        public void AddEdge(T sourceVertex, T targetVertex)
        {
            AddVertexAndNeighbor(sourceVertex, targetVertex);
            if (!_isDirected)
            {
                AddVertexAndNeighbor(targetVertex, sourceVertex);
            }
            // Try add the other vertex to the graph if directed  
            else
            {
                TryAddVertex(targetVertex);
            }
        }

        private void AddVertexAndNeighbor(T sourceVertex, T targetVertex)
        {
            if (_vertexToNeighbors.TryGetValue(sourceVertex, out var neighbors))
            {
                neighbors.AddLast(targetVertex);
            }
            else
            {
                neighbors = new LinkedList<T>();
                neighbors.AddLast(targetVertex);
                _vertexToNeighbors[sourceVertex] = neighbors;
            }
        }

        public IEnumerable<T> GetVertices() => _vertexToNeighbors.Keys;

        public LinkedList<T> GetNeighbors(T vertex) => _vertexToNeighbors[vertex];

        private bool HasVertex(T vertex) => _vertexToNeighbors.ContainsKey(vertex);

        public Dictionary<T, int> FindAllConnectedComponents()
        {
            var vertexToComponent = new Dictionary<T, int>();
            var componentIndex = 0;

            foreach (var vertex in GetVertices().OrderBy(x => x))
            {
                if (vertexToComponent.ContainsKey(vertex)) continue;

                vertexToComponent[vertex] = componentIndex;
                FindComponentMembers(vertex, componentIndex, vertexToComponent);
                componentIndex++;
            }

            return vertexToComponent;
        }

        private void FindComponentMembers(T vertex, int componentIndex, IDictionary<T, int> vertexToComponent)
        {
            foreach (var neighbor in GetNeighbors(vertex))
            {
                if (vertexToComponent.ContainsKey(neighbor)) continue;
                vertexToComponent[neighbor] = componentIndex;
                FindComponentMembers(neighbor, componentIndex, vertexToComponent);
            }
        }

        public static Dictionary<int, List<T>> GetComponentToMembers(Dictionary<T, int> vertexToComponent)
        {
            var componentToMembers = new Dictionary<int, List<T>>();
            foreach (var (vertex, componentIndex) in vertexToComponent.ToArray())
            {
                if (componentToMembers.TryGetValue(componentIndex, out var members))
                {
                    members.Add(vertex);
                }
                else
                {
                    componentToMembers.Add(componentIndex, new List<T> { vertex });
                }
            }
            return componentToMembers;

        }
    }
}
