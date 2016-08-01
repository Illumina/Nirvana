# Configuration

$UnfilteredRefSeq72Path = "E:\Data\Nirvana\Cache\Test\RefSeq\72\chr1.ndb"
$Ensembl72Chr1Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr1.ndb"
$Ensembl79Chr1Path = "E:\Data\Nirvana\Cache\12\Ensembl\79\chr1.ndb"
$Ensembl72Chr3Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr3.ndb"
$Ensembl72Chr4Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr4.ndb"
$Ensembl72Chr7Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr7.ndb"
$Ensembl72Chr10Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr10.ndb"
$Ensembl72Chr15Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr15.ndb"
$Ensembl72Chr17Path = "E:\Data\Nirvana\Cache\12\Ensembl\72\chr17.ndb"
$OutputDir = "D:\Projects\Nirvana\NirvanaUnitTests\Resources\Caches"

$ExtractTranscriptsBin = "d:\Projects\Nirvana\Sandbox\x64\Release\ExtractTranscripts.exe"
$ExtractRegulatoryFeaturesBin = "d:\Projects\Nirvana\Sandbox\x64\Release\ExtractRegulatoryFeatures.exe"

# =======================================
# extract the Ensembl regulatory features
# =======================================

$Ensembl72RegulatoryFeatures = @("ENSR00000079256")

ForEach ($regFeature in $Ensembl72RegulatoryFeatures) {
	$outputPath = "$($OutputDir)\$($regFeature)_Ensembl72.ndb"
	& $ExtractRegulatoryFeaturesBin -i $Ensembl72Chr1Path -o $outputPath -r $regFeature
}

$Ensembl79RegulatoryFeatures = @("ENSR00001584270")

ForEach ($regFeature in $Ensembl79RegulatoryFeatures) {
	$outputPath = "$($OutputDir)\$($regFeature)_Ensembl79.ndb"
	& $ExtractRegulatoryFeaturesBin -i $Ensembl79Chr1Path -o $outputPath -r $regFeature
}

# ==============================
# extract the RefSeq transcripts
# ==============================

$RefSeqTranscripts = @("CCDS30708.1", "CCDS58003.1", "CCDS877.1", "ENSESTT00000006045", "ENSESTT00000008349", "ENSESTT00000011387", "ENSESTT00000011417", "ENSESTT00000012399", "ENSESTT00000034529", "ENSESTT00000034591", "ENSESTT00000034721", "ENSESTT00000034761", "ENSESTT00000051657", "ENSESTT00000056515", "ENSESTT00000058286", "ENSESTT00000064454", "ENSESTT00000064869", "ENSESTT00000079558", "ENSESTT00000082723", "ENSESTT00000082768", "ENSESTT00000083199", "ENSESTT00000083507", "ENSESTT00000085167", "ENSESTT00000086709", "NM_000644.2", "NM_001258340.1", "NM_002524.4", "NM_007158.5", "NM_024011.2", "NM_152665.2", "NM_176877.2", "NM_178221.2", "NR_024321.1", "NR_026752.1", "NR_027120.1", "NR_034014.1", "NR_034015.1", "NR_039983.2", "NR_046018.2", "XM_003846383.1", "NM_001080484.1")

ForEach ($transcript in $RefSeqTranscripts) {
	$outputPath = "$($OutputDir)\$($transcript)_RefSeq72.ndb"
	& $ExtractTranscriptsBin -i $UnfilteredRefSeq72Path -o $outputPath -t $transcript
}

# handle vcf entries
& $ExtractTranscriptsBin -i $UnfilteredRefSeq72Path -o "$($OutputDir)\chr1_115256529_RefSeq72.ndb" -v "chr1\t115256529\t.\tT\tA\t.\tPASS\t.\tGT:GQX:DP:DPF\t0/0:99:34:2"
& $ExtractTranscriptsBin -i $UnfilteredRefSeq72Path -o "$($OutputDir)\chr1_59758869_RefSeq72.ndb" -n chr1 -p 59758869 -r T -a G

# ======================================
# extract the Ensembl transcripts (chr1)
# ======================================

$EnsemblChr1Transcripts = @("ENST00000371614", "ENST00000255416", "ENST00000310991", "ENST00000327044", "ENST00000355439", "ENST00000368246", "ENST00000369535", "ENST00000374163", "ENST00000375759", "ENST00000378635", "ENST00000379407", "ENST00000487053", "ENST00000518655", "ENST00000391369")

ForEach ($transcript in $EnsemblChr1Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr1Path -o $outputPath -t $transcript
}

# ======================================
# extract the Ensembl transcripts (chr3)
# ======================================

$EnsemblChr3Transcripts = @("ENST00000422325")

ForEach ($transcript in $EnsemblChr3Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr3Path -o $outputPath -t $transcript
}

# ======================================
# extract the Ensembl transcripts (chr4)
# ======================================

$EnsemblChr4Transcripts = @("ENST00000288135")

ForEach ($transcript in $EnsemblChr4Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr4Path -o $outputPath -t $transcript
}

# ======================================
# extract the Ensembl transcripts (chr7)
# ======================================

$EnsemblChr7Transcripts = @("ENST00000275493")

ForEach ($transcript in $EnsemblChr7Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr7Path -o $outputPath -t $transcript
}

# =======================================
# extract the Ensembl transcripts (chr10)
# =======================================

$EnsemblChr10Transcripts = @("ENST00000348795")

ForEach ($transcript in $EnsemblChr10Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr10Path -o $outputPath -t $transcript
}

# =======================================
# extract the Ensembl transcripts (chr15)
# =======================================

$EnsemblChr15Transcripts = @("ENST00000543887")

ForEach ($transcript in $EnsemblChr15Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr15Path -o $outputPath -t $transcript
}

# =======================================
# extract the Ensembl transcripts (chr17)
# =======================================

$EnsemblChr17Transcripts = @("ENST00000269305", "ENST00000576171")

ForEach ($transcript in $EnsemblChr17Transcripts) {
	$outputPath = "$($OutputDir)\$($transcript)_Ensembl72.ndb"
	& $ExtractTranscriptsBin -i $Ensembl72Chr17Path -o $outputPath -t $transcript
}