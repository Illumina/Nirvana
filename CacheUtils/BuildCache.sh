#!/bin/sh

# =============
# configuration
# =============

DOTNET=dotnet
RELEASE_DIR=/d/Projects/NirvanaCacheUtils/bin/Release/netcoreapp2.0
CACHE_UTILS=$RELEASE_DIR/CacheUtils.dll
VEP_VERSION=90
CACHE_VERSION=25

DATA_ROOT=/e/Data/Nirvana
INTERMEDIATE_CACHE_DIR=$DATA_ROOT/IntermediateCache/$VEP_VERSION
CACHE_DIR=$DATA_ROOT/Cache/$CACHE_VERSION
REFERENCE_DIR=$DATA_ROOT/References/5

ENSEMBL37_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/Ensembl${VEP_VERSION}_GRCh37.transcripts.gz
ENSEMBL38_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/Ensembl${VEP_VERSION}_GRCh38.transcripts.gz
REFSEQ37_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/RefSeq${VEP_VERSION}_GRCh37.transcripts.gz
REFSEQ38_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/RefSeq${VEP_VERSION}_GRCh38.transcripts.gz

ENSEMBL37_CACHE_PATH=$CACHE_DIR/GRCh37/Ensembl${VEP_VERSION}.transcripts.ndb
ENSEMBL38_CACHE_PATH=$CACHE_DIR/GRCh38/Ensembl${VEP_VERSION}.transcripts.ndb
REFSEQ37_CACHE_PATH=$CACHE_DIR/GRCh37/RefSeq${VEP_VERSION}.transcripts.ndb
REFSEQ38_CACHE_PATH=$CACHE_DIR/GRCh38/RefSeq${VEP_VERSION}.transcripts.ndb

ENSEMBL38_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/Ensembl${VEP_VERSION}_GRCh38.transcripts.gz
REFSEQ37_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/RefSeq${VEP_VERSION}_GRCh37.transcripts.gz
REFSEQ38_TRANSCRIPT_PATH=$INTERMEDIATE_CACHE_DIR/RefSeq${VEP_VERSION}_GRCh38.transcripts.gz


ENSEMBL37_URL="ftp://ftp.ensembl.org/pub/release-${VEP_VERSION}/variation/VEP/homo_sapiens_vep_${VEP_VERSION}_GRCh37.tar.gz"
ENSEMBL38_URL="ftp://ftp.ensembl.org/pub/release-${VEP_VERSION}/variation/VEP/homo_sapiens_vep_${VEP_VERSION}_GRCh38.tar.gz"
REFSEQ37_URL="ftp://ftp.ensembl.org/pub/release-${VEP_VERSION}/variation/VEP/homo_sapiens_refseq_vep_${VEP_VERSION}_GRCh37.tar.gz"
REFSEQ38_URL="ftp://ftp.ensembl.org/pub/release-${VEP_VERSION}/variation/VEP/homo_sapiens_refseq_vep_${VEP_VERSION}_GRCh38.tar.gz"

# =========
# functions
# =========

CreateCache() {

	GA=$1
	TS=$2

	$DOTNET $CACHE_UTILS create -i $INTERMEDIATE_CACHE_DIR/${TS}${VEP_VERSION}_${GA} -r $REFERENCE_DIR/Homo_sapiens.${GA}.Nirvana.dat -o $CACHE_DIR/${GA}/${TS}${VEP_VERSION}

	if [ ! $? -eq 0 ]; then
		echo "ERROR: Unable to generate the cache successfully (Genome assembly: ${GA}, transcript source: ${TS})"
		exit 1
	fi
}

export -f CreateCache

# =============
# main workflow
# =============

# download all the required files for building the cache
$DOTNET $CACHE_UTILS download

# create the intermediate cache files for each configuration
# if [ ! -f ENSEMBL37_TRANSCRIPT_PATH ]
# then
	# echo "Not implemented yet."
	# exit 1
# fi

# if [ ! -f ENSEMBL38_TRANSCRIPT_PATH ]
# then
	# echo "Not implemented yet."
	# exit 1
# fi

# if [ ! -f REFSEQ37_TRANSCRIPT_PATH ]
# then
	# echo "Not implemented yet."
	# exit 1
# fi

# if [ ! -f REFSEQ38_TRANSCRIPT_PATH ]
# then
	# echo "Not implemented yet."
	# exit 1
# fi

# create the universal gene archive
$DOTNET $CACHE_UTILS gene -r $REFERENCE_DIR -i $INTERMEDIATE_CACHE_DIR

# create the actual cache files
CACHE_LIST=""

if [ ! -f ENSEMBL37_CACHE_PATH ]
then
	CACHE_LIST="$CACHE_LIST GRCh37 Ensembl"
fi

if [ ! -f ENSEMBL38_CACHE_PATH ]
then
	CACHE_LIST="$CACHE_LIST GRCh38 Ensembl"
fi

if [ ! -f REFSEQ37_CACHE_PATH ]
then
	CACHE_LIST="$CACHE_LIST GRCh37 RefSeq"
fi

if [ ! -f REFSEQ38_CACHE_PATH ]
then
	CACHE_LIST="$CACHE_LIST GRCh38 RefSeq"
fi

if [ ! -z "$CACHE_LIST" ]
then
	echo "- creating cache files in parallel... "
	echo $CACHE_LIST | xargs -n 2 -P 8 bash -c 'CreateCache "$@"' -- 
	echo "finished."
fi
