####################################################################
# This program is used to update just the miniSA, CA, and CI files #
####################################################################

# ================
# global variables
# ================

$NirvanaRootDir="E:\Data\Nirvana"
$SaRootDir="$NirvanaRootDir\SA"
$IntermediateTsvsDir="$NirvanaRootDir\IntermediateTsvs"

$NirvanaSourceDir="D:\Projects\NirvanaDevelopment"
$ResourcesDir="$NirvanaSourceDir\UnitTests\Resources"

$RefVersion="5"
$SaVersion="38.2"

$CustomIntervalsBed="$ResourcesDir\customIntervals.bed"

$GRCh37="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh37.Nirvana.dat"
$GRCh38="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh38.Nirvana.dat"

# unit test resource directories
$miniSAGRCh37="$ResourcesDir\MiniSuppAnnot"
$miniSAGRCh38="$ResourcesDir\MiniSuppAnnot\hg38"
$miniCIGRCh37="$ResourcesDir\MiniSuppAnnot\CustomIntervals"
$miniCAGRCh37="$ResourcesDir\MiniSuppAnnot\CustomAnnotations"
$directoryIntegrity="$ResourcesDir\DirectoryIntegrity"

# intermediate TSV directories
$HgmdTsv="$IntermediateTsvsDir\HGMD"
$IcslIntervalsTsv="$IntermediateTsvsDir\IcslIntervals"
$InternalAfTsv="$IntermediateTsvsDir\InternalAF"

# SA directories
$SaGRCh37="$SaRootDir\$SaVersion\GRCh37"
$SaGRCh38="$SaRootDir\$SaVersion\GRCh38"
$SaHgmd="$SaRootDir\HGMD"
$SaIcslIntervals="$SaRootDir\IcslIntervals"
$SaInternalAF="$SaRootDir\InternalAF"

$SaUtils="$NirvanaSourceDir\bin\Release\netcoreapp1.1\SAUtils.dll"
$ExtractMiniSA="dotnet $SaUtils extractMiniSA"

# =========
# functions
# =========

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
		bg $name "$ExtractMiniSA --in $SADir\$refName.nsa --begin $start --end $end --ref $Ref --out $miniSADir"
	}
}

function updateMiniCA(){
	Param($name,$outputDir,$SADir,$ref,$targetDataSource)
	Get-ChildItem $outputDir -Filter *.nsa | 
	Foreach-Object {
		$miniCAfile=$_.BaseName
		$refName,$start,$end,$dataSource = $miniCAfile.Split('_',4)
		if($dataSource -match $targetDataSource) {
			bg $name "$ExtractMiniSA --in $SADir\$refName.nsa --begin $start --end $end --ref $Ref --out $outputDir -n $targetDataSource"
		}
	}
}

function copyIfNewer() {
	Param($sourceDir, $destDir, $filename)
	$localFile = Get-Item "$destDir\$filename"
	$remoteFile = Get-Item "$sourceDir\$filename"

	if ($remoteFile.LastWriteTime -gt $localFile.LastWriteTime)
	{
		Copy-Item $remoteFile $localFile
	}
}

# ===========================
# create the IcslIntervals SA
# ===========================

$IcslIntervalsChr1 = "$SaIcslIntervals\chr1.nsa"

if (!(Test-Path $IcslIntervalsChr1)) {
	New-Item -ItemType Directory -Force -Path $IcslIntervalsTsv | Out-Null
	& dotnet $SaUtils createTSV --bed $CustomIntervalsBed -r $GRCh37 -o $IcslIntervalsTsv
	& dotnet $SaUtils createSA -r $GRCh37 -d $IcslIntervalsTsv -o $SaIcslIntervals
}

# ==================
# create the HGMD SA
# ==================

$HgmdChr1 = "$SaHgmd\chr1.nsa"

if (!(Test-Path $HgmdChr1)) {
	New-Item -ItemType Directory -Force -Path $SaHgmd | Out-Null
	& dotnet $SaUtils createSA -r $GRCh37 -d $HgmdTsv -o $SaHgmd
}

# ========================
# create the InternalAF SA
# ========================

$InternalAfChr1 = "$SaInternalAF\chr1.nsa"

if (!(Test-Path $InternalAfChr1)) {
	New-Item -ItemType Directory -Force -Path $SaInternalAF | Out-Null
	& dotnet $SaUtils createSA -r $GRCh37 -d $InternalAfTsv -o $SaInternalAF
}

# ===============================
# copy chrM to DirectoryIntegrity
# ===============================

copyIfNewer $SaGRCh37 $directoryIntegrity "chrM.nsa"
copyIfNewer $SaGRCh37 $directoryIntegrity "chrM.nsa.idx"

# =============
# update miniSA 
# =============

updateMiniSA "SA-37" $miniSAGRCh37 $SaGRCh37 $GRCh37
updateMiniSA "SA-38" $miniSAGRCh38 $SaGRCh38 $GRCh38

# ====================================
# update the mini-CA and mini-CI files 
# ====================================

updateMiniCA "hgmd-37" $miniCAGRCh37 $SaHgmd $GRCh37 "hgmd"
updateMiniCA "internalAF-37" $miniCAGRCh37 $SaInternalAF $GRCh37 "internalAF"
updateMiniCA "IcslIntervals-37" $miniCIGRCh37 $SaIcslIntervals $GRCh37 "IcslIntervals"

Get-Job | Wait-Job