#!/usr/bin/env bash

LAMBDA_DIRS=(AnnotationLambda CustomAnnotationLambda GeneAnnotationLambda NirvanaLambda SingleAnnotationLambda)
OUTPUT_DIR=bin/Release/netcoreapp2.1
ARTIFACT_S3_DIR=${ARTIFACT_S3_DIR:=develop}
S3_PREFIX=s3://nirvana-cloudformation/$ARTIFACT_S3_DIR

# install Amazon.Lambda.Tools if it's not already there
dotnet tool list -g | grep dotnet-lambda &> /dev/null

if [ $? -ne 0 ]; then
	dotnet tool install -g Amazon.Lambda.Tools
fi

# get the script's directory
TOP_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# get the version
VERSION=$(git describe --long | cut -c 2-)

# some fancy formatting
function Header ()
{
	echo -e "\n\e[91m\e[1m${1}\e[0m"
}

# silence pushd and popd
pushd () {
	command pushd "$@" > /dev/null
}

popd () {
	command popd "$@" > /dev/null
}

# create the zip files
for LAMBDA_DIR in "${LAMBDA_DIRS[@]}"
do
	LAMBDA_PATH=$TOP_DIR/$LAMBDA_DIR
	pushd $LAMBDA_PATH
	
	# create the zip file
	dotnet lambda package //p:Version=$VERSION -c Release
	
	# upload the file to S3
	Header "Uploading ${LAMBDA_DIR}:"
	ZIP_PATH=${LAMBDA_PATH}/${OUTPUT_DIR}/${LAMBDA_DIR}.zip
	aws s3 cp $ZIP_PATH ${S3_PREFIX}/${LAMBDA_DIR}-${VERSION}.zip
	
	popd
done

Header "All zip files have been uploaded to ${S3_PREFIX}"