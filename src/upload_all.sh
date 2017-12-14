#!/bin/bash

# specify this manually, otherwise it gets confused if setenv.sh has already been issued when we
# reach here
# . ./setenv.sh

./upload_project.sh Fact.Extensions.Caching
./upload_project.sh Fact.Extensions.Collection
./upload_project.sh Fact.Extensions.Collection.Interceptor
./upload_project.sh Fact.Extensions.Configuration
./upload_project.sh Fact.Extensions.Serialization
./upload_project.sh Fact.Extensions.Serialization.Newtonsoft
./upload_project.sh Fact.Extensions.Serialization.MessagePack
./upload_project.sh Fact.Extensions.Serialization.Pipelines
./upload_project.sh Fact.Extensions.StateAccessor
