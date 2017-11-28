using System.IO;
using System.Text;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Import;
using CacheUtils.DataDumperImport.IO;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.CacheUtils.DataDumperImport.Import
{
    public sealed class ImportRegulatoryFeatureTests
    {
        private readonly ObjectValueNode _regulatoryFeatureNode;

        public ImportRegulatoryFeatureTests()
        {
            var dataDumperOutput   = GetDataDumperOutput();
            _regulatoryFeatureNode = GetObjectValueNode(dataDumperOutput);
        }

        #region Data::Dumper output data

        private static string GetDataDumperOutput()
        {
            return @"$VAR1 = {
          '22' => {
                    'RegulatoryFeature' => [
                                             bless( {
                                                      '_analysis_id' => 16,
                                                      '_bound_lengths' => [
                                                                            0,
                                                                            0
                                                                          ],
                                                      '_vep_feature_type' => 'RegulatoryFeature',
                                                      'cell_types' => {
                                                                        'A549' => 'INACTIVE',
                                                                        'Aorta' => 'NA',
                                                                        'B_cells_(PB)_Roadmap' => 'NA',
                                                                        'CD14+CD16-_monocyte_(CB)' => 'NA',
                                                                        'CD14+CD16-_monocyte_(VB)' => 'NA',
                                                                        'CD4+_ab_T_cell_(VB)' => 'NA',
                                                                        'CD8+_ab_T_cell_(CB)' => 'NA',
                                                                        'CM_CD4+_ab_T_cell_(VB)' => 'NA',
                                                                        'DND-41' => 'INACTIVE',
                                                                        'EPC_(VB)' => 'NA',
                                                                        'Fetal_Adrenal_Gland' => 'NA',
                                                                        'Fetal_Intestine_Large' => 'NA',
                                                                        'Fetal_Intestine_Small' => 'NA',
                                                                        'Fetal_Muscle_Leg' => 'NA',
                                                                        'Fetal_Muscle_Trunk' => 'NA',
                                                                        'Fetal_Stomach' => 'NA',
                                                                        'Fetal_Thymus' => 'NA',
                                                                        'GM12878' => 'INACTIVE',
                                                                        'Gastric' => 'NA',
                                                                        'H1-mesenchymal' => 'NA',
                                                                        'H1-neuronal_progenitor' => 'NA',
                                                                        'H1-trophoblast' => 'NA',
                                                                        'H1ESC' => 'INACTIVE',
                                                                        'H9' => 'NA',
                                                                        'HMEC' => 'INACTIVE',
                                                                        'HSMM' => 'INACTIVE',
                                                                        'HSMMtube' => 'INACTIVE',
                                                                        'HUVEC' => 'INACTIVE',
                                                                        'HUVEC_prol_(CB)' => 'NA',
                                                                        'HeLa-S3' => 'INACTIVE',
                                                                        'HepG2' => 'REPRESSED',
                                                                        'IMR90' => 'INACTIVE',
                                                                        'K562' => 'ACTIVE',
                                                                        'Left_Ventricle' => 'NA',
                                                                        'Lung' => 'NA',
                                                                        'M0_macrophage_(CB)' => 'NA',
                                                                        'M0_macrophage_(VB)' => 'NA',
                                                                        'M1_macrophage_(CB)' => 'NA',
                                                                        'M1_macrophage_(VB)' => 'NA',
                                                                        'M2_macrophage_(CB)' => 'NA',
                                                                        'M2_macrophage_(VB)' => 'NA',
                                                                        'MSC_(VB)' => 'NA',
                                                                        'Monocytes-CD14+' => 'INACTIVE',
                                                                        'Monocytes-CD14+_(PB)_Roadmap' => 'NA',
                                                                        'NH-A' => 'INACTIVE',
                                                                        'NHDF-AD' => 'INACTIVE',
                                                                        'NHEK' => 'INACTIVE',
                                                                        'NHLF' => 'INACTIVE',
                                                                        'Natural_Killer_cells_(PB)' => 'NA',
                                                                        'Osteobl' => 'INACTIVE',
                                                                        'Ovary' => 'NA',
                                                                        'Pancreas' => 'NA',
                                                                        'Placenta' => 'NA',
                                                                        'Psoas_Muscle' => 'NA',
                                                                        'Right_Atrium' => 'NA',
                                                                        'Small_Intestine' => 'NA',
                                                                        'Spleen' => 'NA',
                                                                        'T_cells_(PB)_Roadmap' => 'NA',
                                                                        'Thymus' => 'NA',
                                                                        'eosinophil_(VB)' => 'NA',
                                                                        'erythroblast_(CB)' => 'NA',
                                                                        'iPS-20b' => 'NA',
                                                                        'iPS_DF_19.11' => 'NA',
                                                                        'iPS_DF_6.9' => 'NA',
                                                                        'naive_B_cell_(VB)' => 'NA',
                                                                        'neutrophil_(CB)' => 'NA',
                                                                        'neutrophil_(VB)' => 'NA',
                                                                        'neutrophil_myelocyte_(BM)' => 'NA'
                                                                      },
                                                      'dbID' => '71269',
                                                      'end' => '50555915',
                                                      'epigenome_count' => 1,
                                                      'feature_type' => 'TF_binding_site',
                                                      'regulatory_build_id' => 1,
                                                      'slice' => bless( {
                                                                          'circular' => 0,
                                                                          'coord_system' => bless( {
                                                                                                     'dbID' => '2',
                                                                                                     'default' => 1,
                                                                                                     'name' => 'chromosome',
                                                                                                     'rank' => '1',
                                                                                                     'sequence_level' => 0,
                                                                                                     'top_level' => 0,
                                                                                                     'version' => 'GRCh37'
                                                                                                   }, 'Bio::EnsEMBL::CoordSystem' ),
                                                                          'end' => '51304566',
                                                                          'seq_region_length' => '51304566',
                                                                          'seq_region_name' => '22',
                                                                          'start' => 1,
                                                                          'strand' => 1
                                                                        }, 'Bio::EnsEMBL::Slice' ),
                                                      'stable_id' => 'ENSR00000394520',
                                                      'start' => '50555633',
                                                      'strand' => 0
                                                    }, 'Bio::EnsEMBL::Funcgen::RegulatoryFeature' )
                                           ]
                  }
        };";
        }

        #endregion

        private static ObjectValueNode GetObjectValueNode(string dataDumperOutput)
        {
            ObjectKeyValueNode rootNode;

            using (var ms = new MemoryStream())
            {
                using (var reader = new StringReader(dataDumperOutput))
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line == null) break;
                        writer.WriteLine(line);
                    }
                }

                ms.Position = 0;

                using (var reader = new DataDumperReader(ms)) rootNode = reader.GetRootNode();
            }

            var chr22Node = rootNode.Value.Values[0] as ObjectKeyValueNode;
            Assert.NotNull(chr22Node);

            var regulatoryFeatureNodes = chr22Node.Value.Values[0] as ListObjectKeyValueNode;
            Assert.NotNull(regulatoryFeatureNodes);

            return regulatoryFeatureNodes.Values[0] as ObjectValueNode;
        }

        [Fact]
        public void Parse_Nominal()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var regulatoryRegion = ImportRegulatoryFeature.Parse(_regulatoryFeatureNode, chromosome);
            Assert.NotNull(regulatoryRegion);

            Assert.Equal(chromosome.Index, regulatoryRegion.Chromosome.Index);
            Assert.Equal(50555633, regulatoryRegion.Start);
            Assert.Equal(50555915, regulatoryRegion.End);
            Assert.Equal("ENSR00000394520", regulatoryRegion.Id.WithoutVersion);
            Assert.Equal(RegulatoryRegionType.TF_binding_site, regulatoryRegion.Type);
        }
    }
}
