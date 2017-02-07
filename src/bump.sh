#!/bin/bash

# bumps a version # up for this branch
# TODO: add a provision for reset
# TODO: consolidate branch_name discovery into useful-script utility

branch_name=$(git symbolic-ref -q HEAD)
branch_name=${branch_name##refs/heads/}
branch_name=${branch_name:-HEAD}

filename=../.version.$branch_name 

branch_version=$(<$filename)

if [ -z "$branch_version" ]
then
	echo "Unable to bump branch version.  Ensure file '$filename' exists"
else
	((branch_version++))

	echo $branch_version > $filename

	git commit -am "Version bump to $branch_version on branch: $branch_name"
fi
