#!/bin/bash

. ./setenv.sh

./upload_project.sh Fact.Extensions.Caching
./upload_project.sh Fact.Extensions.Collection
./upload_project.sh Fact.Extensions.Collection.Interceptor
./upload_project.sh Fact.Extensions.Configuration
./upload_project.sh Fact.Extensions.Configuration.Policy
./upload_project.sh Fact.Extensions.Serialization
./upload_project.sh Fact.Extensions.Serialization.Newtonsoft
./upload_project.sh Fact.Extensions.Serialization.MessagePack
./upload_project.sh Fact.Extensions.Serialization.Pipelines
