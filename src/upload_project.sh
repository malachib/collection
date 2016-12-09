#!/bin/bash

pushd $1

ASSEMBLY_NAME=$1.$VERSION
ASSEMBLY_PATH=bin/Debug/$ASSEMBLY_NAME

dotnet pack --version-suffix=$VERSION_SUFFIX

$NUGET push $ASSEMBLY_PATH.nupkg $NUGET_KEY -Source $NUGET_SOURCE
$NUGET push $ASSEMBLY_PATH.symbols.nupkg $NUGET_KEY -Source $NUGET_SOURCE

popd