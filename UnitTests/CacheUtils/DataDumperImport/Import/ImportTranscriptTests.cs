using System.IO;
using System.Text;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Import;
using CacheUtils.DataDumperImport.IO;
using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.CacheUtils.DataDumperImport.Import
{
    public sealed class ImportTranscriptTests
    {
        private readonly ObjectValueNode _transcriptNode;

        public ImportTranscriptTests()
        {
            var dataDumperOutput = GetDataDumperOutput();
            _transcriptNode      = GetObjectValueNode(dataDumperOutput);
        }

        #region Data::Dumper output data

        private static string GetDataDumperOutput()
        {
            return @"$VAR1 = {
          '22' => [
                    bless( {
                             '_ccds' => 'CCDS14080.1',
                             '_gene' => bless( {
                                                 'end' => '50051190',
                                                 'stable_id' => 'ENSG00000188511',
                                                 'start' => '49808176',
                                                 'strand' => -1
                                               }, 'Bio::EnsEMBL::Gene' ),
                             '_gene_hgnc_id' => '28010',
                             '_gene_phenotype' => 0,
                             '_gene_stable_id' => 'ENSG00000188511',
                             '_gene_symbol' => 'C22orf34',
                             '_gene_symbol_source' => 'HGNC',
                             '_protein' => 'ENSP00000394865',
                             '_refseq' => 'NM_014577.1',
                             '_swissprot' => '-',
                             '_trans_exon_array' => [
                                                      bless( {
                                                               'end' => '50051152',
                                                               'end_phase' => 1,
                                                               'phase' => -1,
                                                               'stable_id' => 'ENSE00001657619',
                                                               'start' => '50051053',
                                                               'strand' => -1
                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                      bless( {
                                                               'end' => '49834861',
                                                               'end_phase' => -1,
                                                               'phase' => 1,
                                                               'stable_id' => 'ENSE00001694252',
                                                               'start' => '49834525',
                                                               'strand' => -1
                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                      bless( {
                                                               'end' => '49810577',
                                                               'end_phase' => -1,
                                                               'phase' => -1,
                                                               'stable_id' => 'ENSE00001775575',
                                                               'start' => '49810464',
                                                               'strand' => -1
                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                      bless( {
                                                               'end' => '49810384',
                                                               'end_phase' => -1,
                                                               'phase' => -1,
                                                               'stable_id' => 'ENSE00001669960',
                                                               'start' => '49810251',
                                                               'strand' => -1
                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                      bless( {
                                                               'end' => '49809684',
                                                               'end_phase' => -1,
                                                               'phase' => -1,
                                                               'stable_id' => 'ENSE00001595042',
                                                               'start' => '49808176',
                                                               'strand' => -1
                                                             }, 'Bio::EnsEMBL::Exon' )
                                                    ],
                             '_trembl' => 'F2Z342',
                             '_uniparc' => 'UPI00004105EF',
                             '_variation_effect_feature_cache' => {
                                                                    'codon_table' => 1,
                                                                    'five_prime_utr' => bless( {
                                                                                                 '_root_verbose' => 0,
                                                                                                 'primary_seq' => bless( {
                                                                                                                           '_nowarnonempty' => undef,
                                                                                                                           '_root_verbose' => 0,
                                                                                                                           'alphabet' => 'dna',
                                                                                                                           'display_id' => 'ENST00000414287',
                                                                                                                           'length' => 45,
                                                                                                                           'seq' => 'GCT'
                                                                                                                         }, 'Bio::PrimarySeq' )
                                                                                               }, 'Bio::Seq' ),
                                                                    'introns' => [
                                                                                   bless( {
                                                                                            'adaptor' => undef,
                                                                                            'analysis' => undef,
                                                                                            'dbID' => undef,
                                                                                            'end' => '50051052',
                                                                                            'next' => bless( {
                                                                                                               'end' => '49834861',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => 1,
                                                                                                               'stable_id' => 'ENSE00001694252',
                                                                                                               'start' => '49834525',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'prev' => bless( {
                                                                                                               'end' => '50051152',
                                                                                                               'end_phase' => 1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001657619',
                                                                                                               'start' => '50051053',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'seqname' => undef,
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
                                                                                            'start' => '49834862',
                                                                                            'strand' => -1
                                                                                          }, 'Bio::EnsEMBL::Intron' ),
                                                                                   bless( {
                                                                                            'adaptor' => undef,
                                                                                            'analysis' => undef,
                                                                                            'dbID' => undef,
                                                                                            'end' => '49834524',
                                                                                            'next' => bless( {
                                                                                                               'end' => '49810577',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001775575',
                                                                                                               'start' => '49810464',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'prev' => bless( {
                                                                                                               'end' => '49834861',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => 1,
                                                                                                               'stable_id' => 'ENSE00001694252',
                                                                                                               'start' => '49834525',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'seqname' => undef,
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
                                                                                            'start' => '49810578',
                                                                                            'strand' => -1
                                                                                          }, 'Bio::EnsEMBL::Intron' ),
                                                                                   bless( {
                                                                                            'adaptor' => undef,
                                                                                            'analysis' => undef,
                                                                                            'dbID' => undef,
                                                                                            'end' => '49810463',
                                                                                            'next' => bless( {
                                                                                                               'end' => '49810384',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001669960',
                                                                                                               'start' => '49810251',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'prev' => bless( {
                                                                                                               'end' => '49810577',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001775575',
                                                                                                               'start' => '49810464',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'seqname' => undef,
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
                                                                                            'start' => '49810385',
                                                                                            'strand' => -1
                                                                                          }, 'Bio::EnsEMBL::Intron' ),
                                                                                   bless( {
                                                                                            'adaptor' => undef,
                                                                                            'analysis' => undef,
                                                                                            'dbID' => undef,
                                                                                            'end' => '49810250',
                                                                                            'next' => bless( {
                                                                                                               'end' => '49809684',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001595042',
                                                                                                               'start' => '49808176',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'prev' => bless( {
                                                                                                               'end' => '49810384',
                                                                                                               'end_phase' => -1,
                                                                                                               'phase' => -1,
                                                                                                               'stable_id' => 'ENSE00001669960',
                                                                                                               'start' => '49810251',
                                                                                                               'strand' => -1
                                                                                                             }, 'Bio::EnsEMBL::Exon' ),
                                                                                            'seqname' => undef,
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
                                                                                            'start' => '49809685',
                                                                                            'strand' => -1
                                                                                          }, 'Bio::EnsEMBL::Intron' )
                                                                                 ],
                                                                    'mapper' => bless( {
                                                                                         'cdna_coding_end' => '225',
                                                                                         'cdna_coding_start' => 46,
                                                                                         'exon_coord_mapper' => bless( {
                                                                                                                         '_is_sorted' => 0,
                                                                                                                         '_pair_cdna' => {
                                                                                                                                           'CDNA' => [
                                                                                                                                                       bless( {
                                                                                                                                                                'from' => bless( {
                                                                                                                                                                                   'end' => 100,
                                                                                                                                                                                   'id' => 'cdna',
                                                                                                                                                                                   'start' => 1
                                                                                                                                                                                 }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                'ori' => -1,
                                                                                                                                                                'to' => bless( {
                                                                                                                                                                                 'end' => '50051152',
                                                                                                                                                                                 'id' => 'genome',
                                                                                                                                                                                 'start' => '50051053'
                                                                                                                                                                               }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                              }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                       bless( {
                                                                                                                                                                'from' => bless( {
                                                                                                                                                                                   'end' => '437',
                                                                                                                                                                                   'id' => 'cdna',
                                                                                                                                                                                   'start' => 101
                                                                                                                                                                                 }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                'ori' => -1,
                                                                                                                                                                'to' => bless( {
                                                                                                                                                                                 'end' => '49834861',
                                                                                                                                                                                 'id' => 'genome',
                                                                                                                                                                                 'start' => '49834525'
                                                                                                                                                                               }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                              }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                       bless( {
                                                                                                                                                                'from' => bless( {
                                                                                                                                                                                   'end' => '551',
                                                                                                                                                                                   'id' => 'cdna',
                                                                                                                                                                                   'start' => '438'
                                                                                                                                                                                 }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                'ori' => -1,
                                                                                                                                                                'to' => bless( {
                                                                                                                                                                                 'end' => '49810577',
                                                                                                                                                                                 'id' => 'genome',
                                                                                                                                                                                 'start' => '49810464'
                                                                                                                                                                               }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                              }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                       bless( {
                                                                                                                                                                'from' => bless( {
                                                                                                                                                                                   'end' => '685',
                                                                                                                                                                                   'id' => 'cdna',
                                                                                                                                                                                   'start' => '552'
                                                                                                                                                                                 }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                'ori' => -1,
                                                                                                                                                                'to' => bless( {
                                                                                                                                                                                 'end' => '49810384',
                                                                                                                                                                                 'id' => 'genome',
                                                                                                                                                                                 'start' => '49810251'
                                                                                                                                                                               }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                              }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                       bless( {
                                                                                                                                                                'from' => bless( {
                                                                                                                                                                                   'end' => '2194',
                                                                                                                                                                                   'id' => 'cdna',
                                                                                                                                                                                   'start' => '686'
                                                                                                                                                                                 }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                'ori' => -1,
                                                                                                                                                                'to' => bless( {
                                                                                                                                                                                 'end' => '49809684',
                                                                                                                                                                                 'id' => 'genome',
                                                                                                                                                                                 'start' => '49808176'
                                                                                                                                                                               }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                              }, 'Bio::EnsEMBL::Mapper::Pair' )
                                                                                                                                                     ]
                                                                                                                                         },
                                                                                                                         '_pair_genomic' => {
                                                                                                                                              'GENOME' => [
                                                                                                                                                            bless( {
                                                                                                                                                                     'from' => bless( {
                                                                                                                                                                                        'end' => 100,
                                                                                                                                                                                        'id' => 'cdna',
                                                                                                                                                                                        'start' => 1
                                                                                                                                                                                      }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                     'ori' => -1,
                                                                                                                                                                     'to' => bless( {
                                                                                                                                                                                      'end' => '50051152',
                                                                                                                                                                                      'id' => 'genome',
                                                                                                                                                                                      'start' => '50051053'
                                                                                                                                                                                    }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                                   }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                            bless( {
                                                                                                                                                                     'from' => bless( {
                                                                                                                                                                                        'end' => '437',
                                                                                                                                                                                        'id' => 'cdna',
                                                                                                                                                                                        'start' => 101
                                                                                                                                                                                      }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                     'ori' => -1,
                                                                                                                                                                     'to' => bless( {
                                                                                                                                                                                      'end' => '49834861',
                                                                                                                                                                                      'id' => 'genome',
                                                                                                                                                                                      'start' => '49834525'
                                                                                                                                                                                    }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                                   }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                            bless( {
                                                                                                                                                                     'from' => bless( {
                                                                                                                                                                                        'end' => '551',
                                                                                                                                                                                        'id' => 'cdna',
                                                                                                                                                                                        'start' => '438'
                                                                                                                                                                                      }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                     'ori' => -1,
                                                                                                                                                                     'to' => bless( {
                                                                                                                                                                                      'end' => '49810577',
                                                                                                                                                                                      'id' => 'genome',
                                                                                                                                                                                      'start' => '49810464'
                                                                                                                                                                                    }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                                   }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                            bless( {
                                                                                                                                                                     'from' => bless( {
                                                                                                                                                                                        'end' => '685',
                                                                                                                                                                                        'id' => 'cdna',
                                                                                                                                                                                        'start' => '552'
                                                                                                                                                                                      }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                     'ori' => -1,
                                                                                                                                                                     'to' => bless( {
                                                                                                                                                                                      'end' => '49810384',
                                                                                                                                                                                      'id' => 'genome',
                                                                                                                                                                                      'start' => '49810251'
                                                                                                                                                                                    }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                                   }, 'Bio::EnsEMBL::Mapper::Pair' ),
                                                                                                                                                            bless( {
                                                                                                                                                                     'from' => bless( {
                                                                                                                                                                                        'end' => '2194',
                                                                                                                                                                                        'id' => 'cdna',
                                                                                                                                                                                        'start' => '686'
                                                                                                                                                                                      }, 'Bio::EnsEMBL::Mapper::Unit' ),
                                                                                                                                                                     'ori' => -1,
                                                                                                                                                                     'to' => bless( {
                                                                                                                                                                                      'end' => '49809684',
                                                                                                                                                                                      'id' => 'genome',
                                                                                                                                                                                      'start' => '49808176'
                                                                                                                                                                                    }, 'Bio::EnsEMBL::Mapper::Unit' )
                                                                                                                                                                   }, 'Bio::EnsEMBL::Mapper::Pair' )
                                                                                                                                                          ]
                                                                                                                                            },
                                                                                                                         'from' => 'cdna',
                                                                                                                         'from_cs' => undef,
                                                                                                                         'pair_count' => 5,
                                                                                                                         'to' => 'genomic',
                                                                                                                         'to_cs' => undef
                                                                                                                       }, 'Bio::EnsEMBL::Mapper' ),
                                                                                         'start_phase' => -1
                                                                                       }, 'Bio::EnsEMBL::TranscriptMapper' ),
                                                                    'peptide' => 'MIV',
                                                                    'protein_features' => [
                                                                                            bless( {
                                                                                                     'analysis' => bless( {
                                                                                                                            '_display_label' => 'Low complexity (Seg)'
                                                                                                                          }, 'Bio::EnsEMBL::Analysis' ),
                                                                                                     'end' => '58',
                                                                                                     'hseqname' => 'seg',
                                                                                                     'start' => '39'
                                                                                                   }, 'Bio::EnsEMBL::ProteinFeature' )
                                                                                          ],
                                                                    'protein_function_predictions' => {
                                                                                                        'polyphen_humdiv' => bless( {
                                                                                                                                      'analysis' => 'polyphen',
                                                                                                                                      'matrix' => 'VkVQ-humdiv',
                                                                                                                                      'matrix_compressed' => 1,
                                                                                                                                      'peptide_length' => undef,
                                                                                                                                      'sub_analysis' => 'humdiv',
                                                                                                                                      'translation_md5' => '84229aef711b14371f4c0c6f5ec78ebe'
                                                                                                                                    }, 'Bio::EnsEMBL::Variation::ProteinFunctionPredictionMatrix' ),
                                                                                                        'polyphen_humvar' => bless( {
                                                                                                                                      'analysis' => 'polyphen',
                                                                                                                                      'matrix' => 'VkVQ-humvar',
                                                                                                                                      'matrix_compressed' => 1,
                                                                                                                                      'peptide_length' => undef,
                                                                                                                                      'sub_analysis' => 'humvar',
                                                                                                                                      'translation_md5' => '84229aef711b14371f4c0c6f5ec78ebe'
                                                                                                                                    }, 'Bio::EnsEMBL::Variation::ProteinFunctionPredictionMatrix' ),
                                                                                                        'sift' => bless( {
                                                                                                                           'analysis' => 'sift',
                                                                                                                           'matrix' => 'VkVQ-sift',
                                                                                                                           'matrix_compressed' => 1,
                                                                                                                           'peptide_length' => undef,
                                                                                                                           'sub_analysis' => undef,
                                                                                                                           'translation_md5' => '63fc5b02b6c430f970688d120e14647c'
                                                                                                                         }, 'Bio::EnsEMBL::Variation::ProteinFunctionPredictionMatrix' )
                                                                                                      },
                                                                    'seq_edits' => [
                                                                                     bless( {
                                                                                              'alt_seq' => 'U',
                                                                                              'code' => '_selenocysteine',
                                                                                              'description' => undef,
                                                                                              'end' => '667',
                                                                                              'name' => 'Selenocysteine',
                                                                                              'start' => '667'
                                                                                            }, 'Bio::EnsEMBL::SeqEdit' )
                                                                                   ],
                                                                    'sorted_exons' => [
                                                                                        bless( {
                                                                                                 'end' => '49809684',
                                                                                                 'end_phase' => -1,
                                                                                                 'phase' => -1,
                                                                                                 'stable_id' => 'ENSE00001595042',
                                                                                                 'start' => '49808176',
                                                                                                 'strand' => -1
                                                                                               }, 'Bio::EnsEMBL::Exon' ),
                                                                                        bless( {
                                                                                                 'end' => '49810384',
                                                                                                 'end_phase' => -1,
                                                                                                 'phase' => -1,
                                                                                                 'stable_id' => 'ENSE00001669960',
                                                                                                 'start' => '49810251',
                                                                                                 'strand' => -1
                                                                                               }, 'Bio::EnsEMBL::Exon' ),
                                                                                        bless( {
                                                                                                 'end' => '49810577',
                                                                                                 'end_phase' => -1,
                                                                                                 'phase' => -1,
                                                                                                 'stable_id' => 'ENSE00001775575',
                                                                                                 'start' => '49810464',
                                                                                                 'strand' => -1
                                                                                               }, 'Bio::EnsEMBL::Exon' ),
                                                                                        bless( {
                                                                                                 'end' => '49834861',
                                                                                                 'end_phase' => -1,
                                                                                                 'phase' => 1,
                                                                                                 'stable_id' => 'ENSE00001694252',
                                                                                                 'start' => '49834525',
                                                                                                 'strand' => -1
                                                                                               }, 'Bio::EnsEMBL::Exon' ),
                                                                                        bless( {
                                                                                                 'end' => '50051152',
                                                                                                 'end_phase' => 1,
                                                                                                 'phase' => -1,
                                                                                                 'stable_id' => 'ENSE00001657619',
                                                                                                 'start' => '50051053',
                                                                                                 'strand' => -1
                                                                                               }, 'Bio::EnsEMBL::Exon' )
                                                                                      ],
                                                                    'three_prime_utr' => bless( {
                                                                                                  '_root_verbose' => 0,
                                                                                                  'primary_seq' => bless( {
                                                                                                                            '_nowarnonempty' => undef,
                                                                                                                            '_root_verbose' => 0,
                                                                                                                            'alphabet' => 'dna',
                                                                                                                            'display_id' => 'ENST00000414287',
                                                                                                                            'length' => '1969',
                                                                                                                            'seq' => 'CAC'
                                                                                                                          }, 'Bio::PrimarySeq' )
                                                                                                }, 'Bio::Seq' ),
                                                                    'translateable_seq' => 'ATG'
                                                                  },
                             '_vep_lazy_loaded' => 1,
                             'attributes' => [
                                               bless( {
                                                        'code' => 'miRNA',
                                                        'name' => 'Micro RNA',
                                                        'value' => '62-83'
                                                      }, 'Bio::EnsEMBL::Attribute' ),
                                               bless( {
                                                        'code' => 'cds_start_NF',
                                                        'name' => 'CDS start not found',
                                                        'value' => '1'
                                                      }, 'Bio::EnsEMBL::Attribute' )
                                             ],
                             'biotype' => 'nonsense_mediated_decay',
                             'cdna_coding_end' => '225',
                             'cdna_coding_start' => 46,
                             'coding_region_end' => undef,
                             'coding_region_start' => undef,
                             'dbID' => '2441076',
                             'description' => undef,
                             'end' => '50051152',
                             'is_canonical' => 1,
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
                             'source' => 'havana',
                             'stable_id' => 'ENST00000414287',
                             'start' => '49808176',
                             'strand' => -1,
                             'translation' => bless( {
                                                       'dbID' => '1232784',
                                                       'end' => 125,
                                                       'end_exon' => bless( {
                                                                              'end' => '49834861',
                                                                              'end_phase' => -1,
                                                                              'phase' => 1,
                                                                              'stable_id' => 'ENSE00001694252',
                                                                              'start' => '49834525',
                                                                              'strand' => -1
                                                                            }, 'Bio::EnsEMBL::Exon' ),
                                                       'seq' => undef,
                                                       'stable_id' => 'ENSP00000394865',
                                                       'start' => 46,
                                                       'start_exon' => bless( {
                                                                                'end' => '50051152',
                                                                                'end_phase' => 1,
                                                                                'phase' => 1,
                                                                                'stable_id' => 'ENSE00001657619',
                                                                                'start' => '50051053',
                                                                                'strand' => -1
                                                                              }, 'Bio::EnsEMBL::Exon' ),
                                                       'transcript' => $VAR1->{'22'}[0],
                                                       'version' => 1
                                                     }, 'Bio::EnsEMBL::Translation' ),
                             'version' => 1
                           }, 'Bio::EnsEMBL::Transcript' )
                  ]
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

            var chr22Node = rootNode.Value.Values[0] as ListObjectKeyValueNode;
            Assert.NotNull(chr22Node);

            return chr22Node.Values[0] as ObjectValueNode;
        }

        [Fact]
        public void Parse_Nominal()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var mutableTranscript = ImportTranscript.Parse(_transcriptNode, chromosome, Source.Ensembl);
            Assert.NotNull(mutableTranscript);

            Assert.Equal(chromosome.Index, mutableTranscript.Chromosome.Index);
            Assert.Equal(49808176, mutableTranscript.Start);
            Assert.Equal(50051152, mutableTranscript.End);
            Assert.Equal("ENST00000414287", mutableTranscript.Id);
            Assert.Equal(1, mutableTranscript.Version);
            Assert.Equal("CCDS14080.1", mutableTranscript.CcdsId);
            Assert.Equal("NM_014577.1", mutableTranscript.RefSeqId);
            Assert.Equal(Source.Ensembl, mutableTranscript.Source);
            Assert.Equal(49808176, mutableTranscript.Gene.Start);
            Assert.Equal(50051190, mutableTranscript.Gene.End);
            Assert.Equal("ENSG00000188511", mutableTranscript.Gene.GeneId);
            Assert.Equal("C22orf34", mutableTranscript.Gene.Symbol);
            Assert.Equal(28010, mutableTranscript.Gene.HgncId);
            Assert.Equal(chromosome.Index, mutableTranscript.Gene.Chromosome.Index);
            Assert.True(mutableTranscript.Gene.OnReverseStrand);
            Assert.Equal(GeneSymbolSource.HGNC, mutableTranscript.Gene.SymbolSource);
            Assert.Equal(5, mutableTranscript.Exons.Length);
            Assert.Equal(50051053, mutableTranscript.Exons[0].Start);
            Assert.Equal(50051152, mutableTranscript.Exons[0].End);
            Assert.Equal(-1, mutableTranscript.Exons[0].Phase);
            Assert.Equal(2194, mutableTranscript.TotalExonLength);
            Assert.Equal(4, mutableTranscript.Introns.Length);
            Assert.Equal(49834862, mutableTranscript.Introns[0].Start);
            Assert.Equal(50051052, mutableTranscript.Introns[0].End);
            Assert.Equal("ATG", mutableTranscript.TranslateableSequence);
            Assert.Equal(new IInterval[] { new Interval(62, 83) }, mutableTranscript.MicroRnas);
            Assert.True(mutableTranscript.CdsStartNotFound);
            Assert.False(mutableTranscript.CdsEndNotFound);
            Assert.Equal(new[] { 667 }, mutableTranscript.SelenocysteinePositions);
            Assert.Equal(1, mutableTranscript.StartExonPhase);
            Assert.Equal(BioType.nonsense_mediated_decay, mutableTranscript.BioType);
            Assert.True(mutableTranscript.IsCanonical);
            Assert.Equal(5, mutableTranscript.CdnaMaps.Length);
            Assert.Equal(50051053, mutableTranscript.CdnaMaps[0].Start);
            Assert.Equal(50051152, mutableTranscript.CdnaMaps[0].End);
            Assert.Equal(1, mutableTranscript.CdnaMaps[0].CdnaStart);
            Assert.Equal(100, mutableTranscript.CdnaMaps[0].CdnaEnd);
            Assert.Equal(49834737, mutableTranscript.CodingRegion.Start);
            Assert.Equal(50051107, mutableTranscript.CodingRegion.End);
            Assert.Equal(46, mutableTranscript.CodingRegion.CdnaStart);
            Assert.Equal(225, mutableTranscript.CodingRegion.CdnaEnd);
            Assert.Equal("ENSP00000394865", mutableTranscript.ProteinId);
            Assert.Equal(1, mutableTranscript.ProteinVersion);
            Assert.Equal("MIV", mutableTranscript.PeptideSequence);
            Assert.Equal("VkVQ-sift", mutableTranscript.SiftData);
            Assert.Equal("VkVQ-humvar", mutableTranscript.PolyphenData);
        }
    }
}
