using System.Collections.Generic;
using System.Linq;
using Phantom.Graph;
using Xunit;

namespace UnitTests.Phantom.Graph
{
    public sealed class GraphTests
    {
        [Fact]
        public void GetNeighbors_Undirected()
        {
            var expectedNeighbors = new LinkedList<int>();
            expectedNeighbors.AddLast(10);
            expectedNeighbors.AddLast(30);

            var graph = new Graph<int>(false);

            graph.TryAddVertex(10);
            graph.TryAddVertex(20);
            graph.TryAddVertex(30);
            graph.TryAddVertex(30); // add duplicate

            graph.AddEdge(10, 20);
            graph.AddEdge(20, 30);
            graph.AddEdge(10, 30);

            var observedNeighbors = graph.GetNeighbors(20);
            Assert.Equal(expectedNeighbors, observedNeighbors);
        }

        [Fact]
        public void GetNeighbors_Directed_Downstream()
        {
            var expectedNeighbors = new LinkedList<int>();
            expectedNeighbors.AddLast(20);

            var graph = new Graph<int>(true);

            graph.TryAddVertex(10);
            graph.AddEdge(10, 20);

            var observedNeighbors = graph.GetNeighbors(10);
            Assert.Equal(expectedNeighbors, observedNeighbors);
        }

        [Fact]
        public void GetNeighbors_Directed_Upstream_NullList()
        {
            var expectedNeighbors = new LinkedList<int>();

            var graph = new Graph<int>(true);

            graph.TryAddVertex(10);
            graph.AddEdge(10, 20);

            var observedNeighbors = graph.GetNeighbors(20);
            Assert.Equal(expectedNeighbors, observedNeighbors);
        }

        [Fact]
        public void FindAllConnectedComponents()
        {
            var graph = new Graph<int>(false);

            graph.TryAddVertex(10);
            graph.TryAddVertex(20);
            graph.TryAddVertex(30);
            graph.TryAddVertex(40);
            graph.TryAddVertex(50);

            graph.AddEdge(10, 30);
            graph.AddEdge(20, 30);

            var dict = graph.FindAllConnectedComponents();

            Assert.Equal(0, dict[10]);
            Assert.Equal(0, dict[20]);
            Assert.Equal(0, dict[30]);
            Assert.Equal(1, dict[40]);
            Assert.Equal(2, dict[50]);

            var componentIndexToMembers = Graph<int>.GetComponentToMembers(dict);

            Assert.Equal(new List<int> { 10, 20, 30 }, componentIndexToMembers[0].OrderBy(x => x));
            Assert.Equal(new List<int> { 40 },         componentIndexToMembers[1]);
            Assert.Equal(new List<int> { 50 },         componentIndexToMembers[2]);
        }
    }
}
