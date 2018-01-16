#!/bin/bash

#export NUGET=/c/Projects/apprentice/src/Solutions/.nuget/nuget

# We want upload_all to always call setenv to ensure we're picking up latest version stamps
# . ./setenv.sh

export NUGET=$PWD/../ext/bin/nuget