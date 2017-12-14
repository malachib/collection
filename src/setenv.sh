#!/bin/bash

# TODO: move this out into "useful-scripts" project

. ../ext/myget/setenv_nuget.sh
. ../ext/useful-scripts/setenv.sh

branch_name=$(${MB_USEFUL_SCRIPTS}/scm/get-branch-name.sh)

# Utilize .NET Core nuget, which is v4 as of this writing
# We must depend on setenv_windows or setenv_unix to set this
#export NUGET='dotnet nuget'

branch_version=$(<../.version.$branch_name)

project_version=$(<../.project-version)

if [ -z "$branch_version" ]
then
	echo "No version available for this branch.  Do not upload NUGET packages"
	branch_version="0"
fi

if [ -z $project_version ]
then
	echo "No version available for this project.  Do not upload NUGET packages"
	project_version="0.0.1"
fi

printf -v branch_version "%03d" $branch_version

export VERSION_PREFIX=$project_version
export VERSION_SUFFIX=$branch_name$branch_version
export VERSION=$VERSION_PREFIX-$VERSION_SUFFIX

echo "Using VERSION=$VERSION"

#export branch_name
#export branch_version
