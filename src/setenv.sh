#!/bin/bash

. ../ext/myget/setenv_nuget.sh

branch_name=$(git symbolic-ref -q HEAD)
branch_name=${branch_name##refs/heads/}
branch_name=${branch_name:-HEAD}

branch_version=$(<../.version.$branch_name)

if [ -z "$branch_version" ]
then
	echo "No version available for this branch.  Do not upload NUGET packages"
	branch_version="0"
fi

printf -v branch_version "%03d" $branch_version

export VERSION_PREFIX=1.0.0
export VERSION_SUFFIX=$branch_name$branch_version
export VERSION=$VERSION_PREFIX-$VERSION_SUFFIX

echo "Using VERSION_SUFFIX=$branch_name$branch_version"

export branch_name
export branch_version