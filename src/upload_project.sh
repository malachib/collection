#!/bin/bash

dotnet pack --version-suffix=$VERSION_SUFFIX

$NUGET push $1.$VERSION.nupkg $NUGET_KEY -Source $NUGET_SOURCE
$NUGET push $1.$VERSION.symbols.nupkg $NUGET_KEY -Source $NUGET_SOURCE