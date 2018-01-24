#!/bin/bash

branch_name=$(${MB_USEFUL_SCRIPTS}/scm/get-branch-name.sh)

echo 0 > .version/$branch_name