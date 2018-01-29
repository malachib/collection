#!/bin/bash

# TODO: move this out into "useful-scripts" project

. ../ext/myget/setenv-nuget-beta.sh
. ../ext/useful-scripts/setenv.sh

# Utilize .NET Core nuget, which is v4 as of this writing
# We must depend on setenv_windows or setenv_unix to set this
#export NUGET='dotnet nuget'

export FACT_EXTENSIONS_COLLECTION_VERSIONPATH=$PWD/..

. ./setenv-version.sh

#export branch_name
#export branch_version
