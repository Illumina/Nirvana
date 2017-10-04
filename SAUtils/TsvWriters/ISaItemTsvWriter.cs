using System;
using System.Collections.Generic;
using SAUtils.DataStructures;

namespace SAUtils.TsvWriters
{
	public interface ISaItemTsvWriter:IDisposable
	{
		void WritePosition(IEnumerable<SupplementaryDataItem> saItems);
	}
}
