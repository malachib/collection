#!/bin/bash

# TODO: move this out into "useful-scripts" project

. ./setenv-version.sh

. ../ext/myget/setenv-nuget-alpha.sh
. ../ext/useful-scripts/setenv.sh

export FACT_EXTENSIONS_COLLECTION_VERSIONPATH=$PWD/..
