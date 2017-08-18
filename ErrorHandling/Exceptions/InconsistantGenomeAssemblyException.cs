using System;

namespace ErrorHandling.Exceptions
{
    public sealed class InconsistantGenomeAssemblyException : Exception
    {

		public InconsistantGenomeAssemblyException():base("Found more than one genome assembly represented in the selected data sources.")
		{
			
		}
		
    }
}
