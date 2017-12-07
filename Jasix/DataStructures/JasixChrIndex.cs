using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Caches.DataStructures;

namespace Jasix.DataStructures
{
    public sealed class JasixChrIndex
    {
        public readonly string ReferenceSequence;
        private readonly List<JasixNode> _nodes;
	    private JasixNode _currentNode;
		private readonly List<Interval<long>> _largeVariants;
        private IntervalArray<long> _intervalArray;

        public JasixChrIndex(string refName)
        {
            ReferenceSequence = refName;
            _nodes            = new List<JasixNode>();
            _largeVariants    = new List<Interval<long>>();
            _intervalArray    = null;
        }

        public JasixChrIndex(IExtendedBinaryReader reader) : this("")
        {
            ReferenceSequence = reader.ReadAsciiString();
            var count = reader.ReadOptInt32();
            for (var i = 0; i < count; i++)
                _nodes.Add(new JasixNode(reader));

            var intervalCount = reader.ReadOptInt32();
            if (intervalCount == 0) return;

            for (var i = 0; i < intervalCount; i++)
                _largeVariants.Add(ReadInterval(reader));

            _intervalArray = new IntervalArray<long>(_largeVariants.ToArray());
        }

        private static Interval<long> ReadInterval(IExtendedBinaryReader reader)
        {
            var begin    = reader.ReadOptInt32();
            var end      = reader.ReadOptInt32();
            var position = reader.ReadOptInt64();

            return new Interval<long>(begin, end, position);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
			if (_currentNode != null)
		        _nodes.Add(_currentNode);

	        writer.WriteOptAscii(ReferenceSequence);
	        writer.WriteOpt(_nodes.Count);
	        foreach (var node in _nodes)
	        {
		        node.Write(writer);
	        }

	        writer.WriteOpt(_largeVariants.Count);
	        if (_largeVariants.Count == 0) return;

	        foreach (var interval in _largeVariants.OrderBy(x => x.Begin).ThenBy(x => x.End))
	        {
		        WriteInterval(interval, writer);
	        }
		}

        private static void WriteInterval(Interval<long> interval, IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(interval.Begin);
            writer.WriteOpt(interval.End);
            writer.WriteOpt(interval.Value);
        }

        public void Add(int begin, int end, long filePosition)
        {
            if (begin > end)
                throw new UserErrorException($"start position {begin} is greater than end position{end}");

			if (Utilities.IsLargeVariant(begin,end))
            {
                _largeVariants.Add(new Interval<long>(begin, end, filePosition));
                end = begin;// large variants will be recorded as snvs so that we can query for all entries from a given position
            }

			if (_currentNode == null)
	        {
		        _currentNode = new JasixNode(begin, end, filePosition);
		        return;
	        }

	        if (_currentNode.TryAdd(begin, end)) return;
	        _nodes.Add(_currentNode);
	        _currentNode = new JasixNode(begin, end, filePosition);
        }

        public void Flush()
        {
			if (_currentNode != null)
		        _nodes.Add(_currentNode);
	        if (_largeVariants.Count != 0)
		        _intervalArray = new IntervalArray<long>(_largeVariants.ToArray());
		}


		public long FindFirstSmallVariant(int start, int end)
        {
			var searchNode = new JasixNode(start, end, 0);

	        var firstOverlappingNode = FindFirstOverlappingNode(searchNode);
	        
	       return  firstOverlappingNode?.FileLocation ?? -1;
		}

	    private JasixNode FindFirstOverlappingNode(JasixNode searchNode)
	    {
		    var index = _nodes.BinarySearch(searchNode);

		    if (index < 0)
			    index = ~index;

		    // if it is to the left of the first node, check if the end overlaps
		    if (index == 0)
		    {
			    return _nodes[index].Overlaps(searchNode) ? _nodes[index] : null;
		    }

		    if (index == _nodes.Count)
		    {
			    // if range overlaps the last node location of the last node, otherwise, -1
			    return _nodes[index - 1].Overlaps(searchNode) ? _nodes[index - 1] : null;
		    }

		    // if some intervals from the previous node overlaps the range
		    if (_nodes[index - 1].Overlaps(searchNode))
			    return _nodes[index - 1];

		    return _nodes[index].Overlaps(searchNode) ? _nodes[index] : null;
		}

	    public long[] FindLargeVariants(int begin, int end)
        {
            var positions = _intervalArray?.GetAllOverlappingValues(begin, end);

            if (positions == null || positions.Length == 0) return null;
            return positions;
        }
    }
}
