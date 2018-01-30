#!/bin/bash

export version_newmode=1
version_path=$FACT_EXTENSIONS_COLLECTION_VERSIONPATH
$MB_USEFUL_SCRIPTS/scm/bump.sh $version_path
