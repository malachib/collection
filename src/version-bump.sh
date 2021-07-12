#!/bin/bash

export version_newmode=1

# path set by setenv.sh
version_path=$FACT_EXTENSIONS_COLLECTION_VERSIONPATH
$MB_USEFUL_SCRIPTS/scm/bump.sh $version_path
