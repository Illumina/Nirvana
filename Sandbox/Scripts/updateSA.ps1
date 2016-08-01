##############
# This program is used to update SA , miniSA and minCA when the SA schema changes.
# please update the file path whenever updated the datasource
##############

# ================
# global variables
# ================

$NirvanaRootDir="E:\Data\Nirvana"
$NirvanaSourceDir="D:\Projects\Nirvana"
$ExternalDataRootDir="\\ussd-prd-isi04\Nirvana\Development\ExternalDataSources"

$RefVersion=5.2
$currentSAversion=32.3

$GRCh37="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh37.Nirvana.dat"
$GRCh38="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh38.Nirvana.dat"


$miniSAGRCh37="$NirvanaSourceDir\NirvanaUnitTests\Resources\MiniSuppAnnot"
$miniSAGRCh38="$NirvanaSourceDir\NirvanaUnitTests\Resources\MiniSuppAnnot\hg38"

$SAOutGRCh37="$NirvanaRootDir\SupplementaryAnnotation\$currentSAversion\GRCh37"
$SAOutGRCh38="$NirvanaRootDir\SupplementaryAnnotation\$currentSAversion\GRCh38"


$CreateSupplementaryDatabase="$NirvanaSourceDir\x64\Release\SAUtils.exe createSA"
$ExtractMiniSAdb="$NirvanaSourceDir\sandbox\x64\Release\ExtractMiniSAdb.exe"

$SAisilonPath="\\ussd-prd-isi04\Nirvana\Development\SupplementaryDatabase\$currentSAversion"
$PhylopFolder="\\ussd-prd-isi04\Nirvana\SupplementaryDatabase\PhyloP\latest"

# ================
# update files
# ================

$DBS37="$ExternalDataRootDir\dbSNP\147\GRCh37\dbSNP_v147.lexi.vcf.gz"
$CSM37="$ExternalDataRootDir\COSMIC\v77\GRCh37\NirvanaFiles\allCosmicMutations_sorted.vcf.gz"
$TSV37="$ExternalDataRootDir\COSMIC\v77\GRCh37\NirvanaFiles\combinedMutationStudies.tsv.gz"
$CVR37="$ExternalDataRootDir\ClinVar\20160705\GRCh37\clinvar_20160705.lexi.vcf.gz"
$DGV37="$ExternalDataRootDir\DGV\2016-05-15\GRCh37\GRCh37_hg19_variants_2016-05-15_sorted.txt.gz"
$CLINGEN37="$ExternalDataRootDir\ClinGen\2016-04-14_UCSC\GRCh37\ClinGen_GRCh37_unified_sorted.tsv.gz"
$PUB37="$ExternalDataRootDir\ClinVar\20160705\GRCh37\var_citations.txt.gz"
$EVAL37="$ExternalDataRootDir\ClinVar\20160705\GRCh37\variant_summary.txt.gz"

$DBS38="$ExternalDataRootDir\dbSNP\147\GRCh38\dbSNP_v147.lexi.vcf.gz"
$CSM38="$ExternalDataRootDir\COSMIC\v77\GRCh38\NirvanaFiles\allCosmicMutations_sorted.vcf.gz"
$TSV38="$ExternalDataRootDir\COSMIC\v77\GRCh38\NirvanaFiles\combinedMutationStudies.tsv.gz"
$CVR38="$ExternalDataRootDir\ClinVar\20160705\GRCh38\clinvar_20160705.lexi.vcf.gz"
$DGV38="$ExternalDataRootDir\DGV\2016-05-15\GRCh38\GRCh38_hg38_variants_2016-05-15_sorted.txt.gz"
$CLINGEN38="$ExternalDataRootDir\ClinGen\2016-04-14_UCSC\GRCh38\ClinGen_GRCh38_unified_sorted.tsv.gz"
$PUB38="$ExternalDataRootDir\ClinVar\20160705\GRCh38\var_citations.txt.gz"
$EVAL38="$ExternalDataRootDir\ClinVar\20160705\GRCh38\variant_summary.txt.gz"

# ==================
# files won't update
# ==================
$ONEK37="$ExternalDataRootDir\1000Genomes\v5a\GRCh37\ALL_1000Genome_GRCh37_snvRes_5_20_2016_remove_conflicting_entries.removeExtraField.vcf.gz"
$ONEKSV37="$ExternalDataRootDir\1000Genomes\v5a\GRCh37\ALL_1000Genome_svRes_4_6.txt.gz"
$EXAC37="$ExternalDataRootDir\ExAc\0.3.1\GRCh37\ExAC.r0.3.1.sites.vep.sorted.vcf.gz"
$EVS37="$ExternalDataRootDir\EVS\V2-SSA137\GRCh37\NirvanaFile\ESP6500SI-V2-SSA137.all.vcf.gz"

$EVS38="$ExternalDataRootDir\EVS\V2-SSA137\GRCh38\allEvsSorted.vcf.gz"
$ONEK38="$ExternalDataRootDir\1000Genomes\v5a\GRCh38\ALL_1000Genome_GRCh38_snvRes_5_23_2016_remove_conflicting_entries.removeExtraField.vcf.gz"






function bg() {
	Param ($name, $job)
	$script=[scriptblock]::Create($job)
	Start-Job -Name $name -ScriptBlock $script
}




function updateMiniSA(){
	Param($name,$miniSADir,$SADir,$ref)
	Get-ChildItem $miniSADir -Filter *.nsa | 
	Foreach-Object {
		$miniSAfile=$_.BaseName
		$refName,$start,$end = $miniSAfile.Split('_',3)
		bg $name "$ExtractMiniSAdb --in $SADir\$refName.nsa --begin $start --end $end --ref $Ref --out $miniSADir"
	}

}


# =========================================
# Create Supplementary database 
# =========================================

mkdir $SAOutGRCh37
mkdir $SAOutGRCh38

bg "SA-37" "$CreateSupplementaryDatabase --out $SAOutGRCh37 --ref $GRCh37 --dbs $DBS37 --csm $CSM37 --tsv $TSV37 --evs $EVS37 --cvr $CVR37 --pub $PUB37 --eval $EVAL37 --onek $ONEK37  --onekSv $ONEKSV37 --dgv $DGV37 --clinGen $CLINGEN37 --exac $EXAC37"


bg "SA-38" "$CreateSupplementaryDatabase --out $SAOutGRCh38 --ref $GRCh38 --dbs $DBS38 --csm $CSM38 --tsv $TSV38 --evs $EVS38 --cvr $CVR38 --pub $PUB38 --eval $EVAL38 --onek $ONEK38  --dgv $DGV38 --clinGen $CLINGEN38"

get-job|wait-job


# =========================
# update miniSA 
# =========================

updateMiniSA "update-37" $miniSAGRCh37 $SAOutGRCh37 $GRCh37

updateMiniSA "update-38" $miniSAGRCh38 $SAOutGRCh38 $GRCh38

get-job|wait-job



#===========================
#update custom annotation
#===========================

function updateMiniCA(){
	Param($name,$miniCADir,$CADir,$ref)
	Get-ChildItem $miniCADir -Filter *.nsa | 
	Foreach-Object {
		$miniCAfile=$_.BaseName
		$refName,$start,$end = $miniCAfile.Split('_',3)
		bg $name "$ExtractMiniSAdb --in $CADir\$refName.nsa --begin $start --end $end --ref $Ref --name --out $miniSADir"
	}

}


##########
# copy the SA to isilon
#########
mkdir $SAisilonPath

Copy-Item $SAOutGRCh37 $SAisilonPath\GRCh37 -Force -Recurse
Copy-Item $SAOutGRCh38 $SAisilonPath\GRCh38 -Force -Recurse

Import-Module PSCX

#============================
# Add hardLink to phylop
#============================
Get-ChildItem "$PhylopFolder\GRCh37" -Filter *.npd |
	Foreach-Object {
	$npdFile=$_.Name
	New-HardLink "$SAisilonPath\GRCh37\$npdFile" "$PhylopFolder\GRCh37\$npdFile"
	}

	Get-ChildItem "$PhylopFolder\GRCh38" -Filter *.npd |
	Foreach-Object {
	$npdFile=$_.Name
	New-HardLink "$SAisilonPath\GRCh38\$npdFile" "$PhylopFolder\GRCh38\$npdFile"
	}

