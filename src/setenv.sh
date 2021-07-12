#!/bin/bash

# v0.1

# TODO: move this out into "useful-scripts" project

# Sets VERSION_PREFIX, VERSION_SUFFIX
. ./setenv-version.sh

# Set up private keys for uploads
. ../ext/myget/setenv-nuget-alpha.sh

# Set up general pathing for useful-scripts discovery
. ../ext/useful-scripts/setenv.sh

export FACT_EXTENSIONS_COLLECTION_VERSIONPATH=$PWD/..
