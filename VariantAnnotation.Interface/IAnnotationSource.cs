using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
	public interface IAnnotationSource
	{
		/// <summary>
		/// Performs annotation on a variant and returns an annotated variant
		/// </summary>
		/// <param name="variant">a variant object</param>
		/// <returns>an annotated variant</returns>
		IAnnotatedVariant Annotate(IVariant variant);

		IEnumerable<IDataSourceVersion> GetDataSourceVersions();

		string GetGenomeAssembly();

	    void AddGeneLevelAnnotation(List<string> annotatedGenes);

	    void EnableReferenceNoCalls(bool limitReferenceNoCallsToTranscripts);

        void EnableMitochondrialAnnotation();

        string GetDataVersion();
	}

	public interface IDataSourceVersion
	{
		string Name { get; }
		string Description { get; }
		string Version { get; }
		long ReleaseDateTicks { get; }
	}
}
