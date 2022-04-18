using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using Variants;

namespace VariantAnnotation.Providers;

public sealed class ScoreProvider : IAnnotationProvider
{
    public string                          Name               => "Supplementary annotation provider";
    public GenomeAssembly                  Assembly           { get; }
    public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

    private readonly ScoreReader[] _scoreReaders;

    public ScoreProvider(ScoreReader[] scoreReaders)
    {
        _scoreReaders                  = scoreReaders;
        (Assembly, DataSourceVersions) = GetReadersMetadata();
    }

    public void Annotate(IAnnotatedPosition annotatedPosition)
    {
        foreach (ScoreReader scoreReader in _scoreReaders)
        {
            foreach (IAnnotatedVariant annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                IVariant variant = annotatedVariant.Variant;
                    
                // Score provider is only limited to SNV type calls
                if (variant.Type != VariantType.SNV) continue;
                    
                Chromosome chromosome = variant.Chromosome;
                string     jsonString = scoreReader.GetAnnotationJson(chromosome.Index, variant.Start, variant.AltAllele);

                if (jsonString == null) continue;

                annotatedVariant.SaList.Add(new SupplementaryAnnotation(
                    scoreReader.JsonKey,
                    false,
                    true,
                    jsonString,
                    null
                ));
            }
        }
    }

    private (GenomeAssembly Assembly, IEnumerable<IDataSourceVersion> Versions) GetReadersMetadata()
    {
        HashSet<GenomeAssembly>  assemblies = new();
        List<IDataSourceVersion> versions   = new();
        var                      sb         = new StringBuilder();

        foreach (ScoreReader reader in _scoreReaders)
        {
            if (reader.Assembly != GenomeAssembly.rCRS && reader.Assembly != GenomeAssembly.Unknown) assemblies.Add(reader.Assembly);
            versions.Add(reader.Version);
            sb.AppendLine($"{reader.Version}, Assembly: {reader.Assembly}");
        }

        if (assemblies.Count == 1) return (assemblies.First(), versions);

        throw new UserErrorException($"Multiple genome assemblies detected in Supplementary annotation directory.\n{sb}");
    }


    public void PreLoad(Chromosome chromosome, List<int> positions)
    {
    }

    public void Dispose()
    {
    }
}