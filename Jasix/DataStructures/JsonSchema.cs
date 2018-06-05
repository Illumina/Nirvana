using System.Collections.Generic;
using Intervals;

// ReSharper disable InconsistentNaming
// The names have to be this way as they have to match the json schema exactly

namespace Jasix.DataStructures
{
    // ReSharper disable once ClassNeverInstantiated.Global
	public class JsonSchema:IInterval
	{
	    // ReSharper disable UnassignedField.Global
		public string chromosome;	    
		public int position;
		public string refAllele;
		public IList<string> altAlleles;
		public int svEnd;

		// ReSharper restore UnassignedField.Global
	    public int Start => position;
	    public int End => Utilities.GetJsonEntryEnd(this);
    }
}
