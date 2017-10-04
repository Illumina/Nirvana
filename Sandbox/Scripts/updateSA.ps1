##############
# This program is used to update SA , miniSA and minCA when the SA schema changes.
# please update the file path whenever updated the datasource
##############

# ================
# global variables
# ================

$NirvanaRootDir="E:\Data\Nirvana"
$NirvanaSourceDir="D:\Projects\Nirvana"
$ExternalDataRootDir="\\ussd-prd-isi04\Nirvana\Development\IntermediateTsvs"

$RefVersion=5.2
$currentSAversion=40.1

$GRCh37="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh37.Nirvana.dat"
$GRCh38="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh38.Nirvana.dat"


$miniSAGRCh37="$NirvanaSourceDir\UnitTests\Resources\MiniSuppAnnot"
$miniSAGRCh38="$NirvanaSourceDir\UnitTests\Resources\MiniSuppAnnot\hg38"

$SAOutGRCh37="$NirvanaRootDir\SupplementaryDatabase\$currentSAversion\GRCh37"
$SAOutGRCh38="$NirvanaRootDir\SupplementaryDatabase\$currentSAversion\GRCh38"


$CreateSupplementaryDatabase="dotnet $NirvanaSourceDir\bin\Release\netcoreapp1.1\SAUtils.dll createSA"
$ExtractMiniSAdb="dotnet $NirvanaSourceDir\bin\Release\netcoreapp1.1\SAUtils.dll extractMiniSA"

$SAisilonPath="\\ussd-prd-isi04\Nirvana\Development\SupplementaryDatabase\$currentSAversion"
$PhylopFolder="\\ussd-prd-isi04\Nirvana\SupplementaryDatabase\PhyloP\latest"
#$OmimDatabase="\\ussd-prd-isi04\Nirvana\Development\OmimDatabase\3\genePhenotypeMap.mim"
# ================
# update files
# ================

$CVR37="$ExternalDataRootDir\2017-04\GRCh37\clinvar_20170403.tsv.gz"
$DBS37="$ExternalDataRootDir\2017-04\GRCh37\dbsnp_150.tsv.gz"
$GLOBAl37="$ExternalDataRootDir\2017-04\GRCh37\globalAllele_150.tsv.gz"
$CSM37="$ExternalDataRootDir\2017-04\GRCh37\cosmic_80.tsv.gz"
$DGV37="$ExternalDataRootDir\2017-04\GRCh37\dgv_20160515.interval.tsv.gz"
$CLINGEN37="$ExternalDataRootDir\2017-04\GRCh37\clinGen_20160414.interval.tsv.gz"


$CVR38="$ExternalDataRootDir\2017-04\GRCh38\clinvar_20170403.tsv.gz"
$DBS38="$ExternalDataRootDir\2017-04\GRCh38\dbsnp_150.tsv.gz"
$GLOBAl38="$ExternalDataRootDir\2017-04\GRCh38\globalAllele_150.tsv.gz"
$CSM38="$ExternalDataRootDir\2017-04\GRCh38\cosmic_80.tsv.gz"
$DGV38="$ExternalDataRootDir\2017-04\GRCh38\dgv_20160515.interval.tsv.gz"
$CLINGEN38="$ExternalDataRootDir\2017-04\GRCh38\clinGen_unknown.interval.tsv.gz"

# ==================
# files won't update
# ==================
$ONEK37="$ExternalDataRootDir\2017-04\GRCh37\oneKg_Phase_3_v5a.tsv.gz"
$ONEKSV37="$ExternalDataRootDir\2017-04\GRCh37\oneKg_Phase_3_v5a.interval.tsv.gz"
$EXAC37="$ExternalDataRootDir\2017-04\GRCh37\exac_0.3.1.tsv.gz"
$EVS37="$ExternalDataRootDir\2017-04\GRCh37\evs_2.tsv.gz"
$RefMinor37="$ExternalDataRootDir\2017-04\GRCh37\RefMinor_Phase_3_v5a.tsv.gz"

$EVS38="$ExternalDataRootDir\2017-04\GRCh38\evs_2.tsv.gz"
$ONEK38="$ExternalDataRootDir\2017-04\GRCh38\oneKg_Phase_3_v3plus.tsv.gz"
$RefMinor38="$ExternalDataRootDir\2017-04\GRCh38\RefMinor_Phase_3_v3plus.tsv.gz"




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

#============================
# copy OMIM
#============================
	Copy-Item $OmimDatabase $SAOutGRCh37
	Copy-Item $OmimDatabase $SAOutGRCh38

	

bg "SA-37" "$CreateSupplementaryDatabase --out $SAOutGRCh37 --ref $GRCh37 -t $DBS37 -t $CSM37 -t $EVS37 -t $CVR37 -t $ONEK37  -i $ONEKSV37 -i $DGV37 -i $CLINGEN37 -t $EXAC37 -t $GLOBAl37 -t $RefMinor37"


bg "SA-38" "$CreateSupplementaryDatabase --out $SAOutGRCh38 --ref $GRCh38 -t $DBS38 -t $CSM38 -t $EVS38 -t $CVR38 -t $ONEK38  -i $DGV38 -i $CLINGEN38  -t $GLOBAl38 -t $RefMinor38"

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

