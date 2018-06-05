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

        public bool TryAddVertex(T vertex)
        {
            if (HasVertex(vertex)) return false;

            _vertexToNeighbors[vertex] = new LinkedList<T>();
            return true;
        }

        private void SetVertexAndNeighbors(T vertex, LinkedList<T> neighbors) => _vertexToNeighbors[vertex] = neighbors;

        public void AddEdge(T oneVertex, T otherVertex)
        {
            AddVertexAndNeighbor(oneVertex, otherVertex);
            if (!_isDirected)
            {
                AddVertexAndNeighbor(otherVertex, oneVertex);
            }
            // Try add the other vertex to the graph if directed  
            else
            {
                TryAddVertex(otherVertex);
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

        public void MergeDuplicatedEdges()
        {
            foreach (var vertex in GetVertices())
            {
                var uniqNeighbors = new LinkedList<T>(GetNeighbors(vertex).ToHashSet());
                SetVertexAndNeighbors(vertex, uniqNeighbors);
            }
        }

        public IEnumerable<T> GetVertices() => _vertexToNeighbors.Keys;

        public LinkedList<T> GetNeighbors(T vertex) => _vertexToNeighbors[vertex];

        private bool HasVertex(T vertex) => _vertexToNeighbors.ContainsKey(vertex);

        public Graph<T> CreateGraphFromLinkedVertices(IReadOnlyList<T> vertices)
        {
            var graph = new Graph<T>(true);
            int numVertices = vertices.Count;
            switch (numVertices)
            {
                case 0:
                    return graph;
                case 1:
                    graph.TryAddVertex(vertices[0]);
                    return graph;
            }
            for (int i = 1; i < numVertices; i++)
            {
                graph.AddEdge(vertices[i - 1], vertices[1]);
            }
            return graph;
        }

        public void MergeGraph(IGraph<T> other)
        {
            // check the existence of the vertices only once
            foreach (var vertex in other.GetVertices())
            {
                TryAddVertex(vertex);
            }
            foreach (var vertex in other.GetVertices())
            {
                foreach (var neighbor in other.GetNeighbors(vertex))
                {
                    _vertexToNeighbors[vertex].AddLast(neighbor);
                }
            }
        }

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

        private void FindComponentMembers(T vertex, int componentIndex, Dictionary<T, int> vertexToComponent)
        {
            foreach (var neighbor in GetNeighbors(vertex))
            {
                if (!vertexToComponent.ContainsKey(neighbor))
                {
                    vertexToComponent[neighbor] = componentIndex;
                    FindComponentMembers(neighbor, componentIndex, vertexToComponent);
                }
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
