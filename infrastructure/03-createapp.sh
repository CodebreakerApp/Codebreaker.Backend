#! /bin/bash

RESOURCE_GROUP_NAME=codebreaker
LOCATION=westeurope
ENVIRONMENT=codebreakerenv
REGISTRY_NAME=codebreaker
COSMOS_NAME=codebreaker
WORKSPACE_NAME=codebreakerlogs
API_APP=codebreakerapi
REGISTRY_USERNAME=codebreaker
REGISTRY_PASSWORD=<add registry password>

# build the docker image with the tag for deployment

# login to the ACR
az acr login --name $REGISTRY_NAME

# publish the docker image (in src directory)
docker build . -t $REGISTRY_NAME.azurecr.io/codebreakerapi:latest -f api/CodeBreaker.APIs/Dockerfile

docker push $REGISTRY_NAME.azurecr.io/codebreakerapi:latest

# create the Azure Container App

docker login $REGISTRY_NAME.azurecr.io -u $REGISTRY_USERNAME -p $REGISTRY_PASSWORD

az containerapp create --name $API_APP \
    --resource-group $RESOURCE_GROUP_NAME \
    --image $REGISTRY_NAME.azurecr.io/codebreakerapi:latest \
    --environment $ENVIRONMENT \
    --registry-server $REGISTRY_NAME  \
    --registry-username $REGISTRY_USERNAME \
    --registry-password $REGISTRY_PASSWORD \
    --ingress external --target-port 80 

# for using CI/CD, the service principal created needs to have access for pulling from the registry