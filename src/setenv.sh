#!/bin/bash

export VERSION_PREFIX=1.0.0
export VERSION_SUFFIX=alpha012
export VERSION=$VERSION_PREFIX-$VERSION_SUFFIX
export NUGET_KEY=73f3dd5e-d8f0-4acd-aa3a-889843cb27fe
export NUGET_SOURCE=https://www.myget.org/F/malachib/api/v2/package
#if [ -z ${NUGET+x} ]; then 
#	echo "Looking for nuget";
#	# export NUGET=$(readlink -f ../Solutions/.nuget/nuget); 
#	if [ -z ${NUGET+x} ]; then 
#		echo "Can't find it, so assigning to just 'nuget' and hoping for the best";
#		export NUGET=nuget; 
#	fi
#else 
#	echo "Have nuget as: ${NUGET}"
#fi
echo "using NUGET=${NUGET}"
