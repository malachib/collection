#!/bin/bash

# v0.1

# Depends on setenv.sh
# Ultimately sets version_path to $(CWD)/.version

version_path=$FACT_EXTENSIONS_COLLECTION_VERSIONPATH

# sets VERSION_PREFIX, VERSION_SUFFIX and VERSION
. ${MB_USEFUL_SCRIPTS}/scm/setenv-version.sh $version_path