# ================
# global variables
# ================

$NirvanaRootDir="E:\Data\Nirvana"
$NirvanaSourceDir="D:\Projects\Nirvana"

$CacheVersion=23
$VepVersion=84
$RefVersion=4

$CanonicalDir="$NirvanaRootDir\External_data_sources\Canonical"
$GeneSymbolsDir="$NirvanaRootDir\External_data_sources\GeneSymbols"
$VepCacheDir="$NirvanaRootDir\VEP_caches"
$CacheDir="$NirvanaRootDir\Cache"

$GRCh37="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh37.Nirvana.dat"
$GRCh38="$NirvanaRootDir\References\$RefVersion\Homo_sapiens.GRCh38.Nirvana.dat"

$EnsemblGRCh37="$CacheDir\$CacheVersion\GRCh37\Ensembl\$VepVersion"
$RefSeqGRCh37="$CacheDir\$CacheVersion\GRCh37\RefSeq\$VepVersion"
$BothGRCh37="$CacheDir\$CacheVersion\GRCh37\Both\$VepVersion"
$EnsemblGRCh38="$CacheDir\$CacheVersion\GRCh38\Ensembl\$VepVersion"
$RefSeqGRCh38="$CacheDir\$CacheVersion\GRCh38\RefSeq\$VepVersion"
$BothGRCh38="$CacheDir\$CacheVersion\GRCh38\Both\$VepVersion"

$TestEnsemblGRCh37="$CacheDir\Test\GRCh37\Ensembl\$VepVersion"
$TestRefSeqGRCh37="$CacheDir\Test\GRCh37\RefSeq\$VepVersion"

$HgncIds="$GeneSymbolsDir\HgncIds_21.txt"

$ParseGeneSymbols="$NirvanaSourceDir\Sandbox\x64\Release\ParseGeneSymbols.exe"
$CreateNirvanaDatabase="$NirvanaSourceDir\Sandbox\x64\Release\CreateNirvanaDatabase.exe"

$Date=Get-Date -UFormat "%Y-%m-%d"
#$Date="2016-04-29"

$LrgRefSeqGene="$CanonicalDir\LRG_RefSeqGene_$Date"
$GeneInfo="$GeneSymbolsDir\gene_info_$Date.gz"
$Gene2RefSeq="$GeneSymbolsDir\gene2refseq_$Date.gz"
$GeneSymbols="$GeneSymbolsDir\GeneSymbols_$Date.tsv"

$progressPreference = 'silentlyContinue'

# =========
# functions
# =========

function Download-File {
	Param ($Description, $Url, $OutputPath)
	
	Write-Host "- downloading $Description... " -nonewline

	if(Test-Path $OutputPath) {
		Write-Host "skipped (already exists)."
	} else {
		Invoke-WebRequest -Uri $Url -OutFile $OutputPath
		Write-Host "finished."
	}
}

function bg() {
	Param ($name, $job)
	$script=[scriptblock]::Create($job)
	Start-Job -Name $name -ScriptBlock $script
}

# ============================
# create the cache directories
# ============================

Write-Host "- creating cache directories... " -nonewline

New-Item -ItemType Directory -Force -Path $EnsemblGRCh37 | Out-Null
New-Item -ItemType Directory -Force -Path $RefSeqGRCh37 | Out-Null
New-Item -ItemType Directory -Force -Path $BothGRCh37 | Out-Null
New-Item -ItemType Directory -Force -Path $EnsemblGRCh38 | Out-Null
New-Item -ItemType Directory -Force -Path $RefSeqGRCh38 | Out-Null
New-Item -ItemType Directory -Force -Path $BothGRCh38 | Out-Null
New-Item -ItemType Directory -Force -Path $TestEnsemblGRCh37 | Out-Null
New-Item -ItemType Directory -Force -Path $TestRefSeqGRCh37 | Out-Null

Write-Host "finished."

# =========================
# download the latest files
# =========================

Download-File "gene_info" "ftp://ftp.ncbi.nlm.nih.gov/gene/DATA/gene_info.gz" $GeneInfo
Download-File "gene2refseq" "ftp://ftp.ncbi.nlm.nih.gov/gene/DATA/gene2refseq.gz" $Gene2RefSeq
Download-File "LRG_RefSeqGene" "ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/RefSeqGene/LRG_RefSeqGene" $LrgRefSeqGene

# ===========================
# create the GeneSymbols file
# ===========================

if(Test-Path $GeneSymbols) {
	Write-Host "- creating GeneSymbols... skipped (already exists)."
} else {
	iex "$ParseGeneSymbols --geneinfo $GeneInfo --gene2refseq $Gene2RefSeq --out $GeneSymbols"
}

# =====================
# convert the databases
# =====================

bg "ensembl37" "$CreateNirvanaDatabase --date ""$Date"" --ensembl -i ""$VepCacheDir\${VepVersion}\homo_sapiens\${VepVersion}_GRCh37"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh37"" -o ""$EnsemblGRCh37"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh37 -n chr1"

bg "refseq37" "$CreateNirvanaDatabase --date ""$Date"" --refseq -i ""$VepCacheDir\${VepVersion}\homo_sapiens_refseq\${VepVersion}_GRCh37"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh37"" -o ""$RefSeqGRCh37"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh37 > ""$RefSeqGRCh37\output.txt"""

bg "ensembl38" "$CreateNirvanaDatabase --date ""$Date"" --ensembl -i ""$VepCacheDir\${VepVersion}\homo_sapiens\${VepVersion}_GRCh38"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh38"" -o ""$EnsemblGRCh38"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh38 > ""$EnsemblGRCh38\output.txt"""

bg "refseq38" "$CreateNirvanaDatabase --date ""$Date"" --refseq -i ""$VepCacheDir\${VepVersion}\homo_sapiens_refseq\${VepVersion}_GRCh38"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh38"" -o ""$RefSeqGRCh38"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh38 > ""$RefSeqGRCh38\output.txt"""

# =============================================
# create the unfiltered test databases for chr1
# =============================================

bg "testEnsembl37" "$CreateNirvanaDatabase --date ""$Date"" --ensembl -i ""$VepCacheDir\${VepVersion}\homo_sapiens\${VepVersion}_GRCh37"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh37"" -o ""$TestEnsemblGRCh37"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh37 --no-filter -n chr1 > ""$TestEnsemblGRCh37\output.txt"""

bg "testRefseq37" "$CreateNirvanaDatabase --date ""$Date"" --refseq -i ""$VepCacheDir\${VepVersion}\homo_sapiens_refseq\${VepVersion}_GRCh37"" -l ""$LrgRefSeqGene"" --vep $VepVersion -r ""$GRCh37"" -o ""$TestRefSeqGRCh37"" -g ""$GeneSymbols"" --hgncids ""$HgncIds"" --ga GRCh37 --no-filter -n chr1 > ""$TestRefSeqGRCh37\output.txt"""

Get-Job | Wait-Job
