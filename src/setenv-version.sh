#!/bin/bash

# Utilize .NET Core nuget, which is v4 as of this writing
# We must depend on setenv_windows or setenv_unix to set this
#export NUGET='dotnet nuget'

version_path=..
branch_name=$(${MB_USEFUL_SCRIPTS}/scm/get-branch-name.sh)

branch_version=$(<$version_path/.version.$branch_name)

project_version=$(<$version_path/.version/prefix.$branch_name)

if [ -z "$branch_version" ]
then
	branch_version=$(<$version_path/.version/$branch_name)

	if [ -z "$branch_version" ]
	then
		echo "No version available for this branch.  Do not upload NUGET packages"
		branch_version="0"
	fi
fi

if [ -z $project_version ]
then
	project_version=$(<$version_path/.project-version)
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
