#!/bin/bash

branch_name=$(${MB_USEFUL_SCRIPTS}/scm/get-branch-name.sh)
version_path=$FACT_EXTENSIONS_COLLECTION_VERSIONPATH

echo 1 > $version_path/.version/$branch_name