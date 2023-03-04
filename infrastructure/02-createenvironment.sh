#! /bin/bash

RESOURCE_GROUP_NAME=codebreaker
LOCATION=westeurope
ENVIRONMENT=codebreakerenv
REGISTRY_NAME=codebreaker
COSMOS_ACCOUNT_NAME=codebreaker
DATABASE_NAME=codebreaker
WORKSPACE_NAME=codebreakerlogs

# create a log analytics workspace
az monitor log-analytics workspace create \
    --resource-group $RESOURCE_GROUP_NAME \
    --location $LOCATION \
    --workspace-name $WORKSPACE_NAME

KEY=$(az monitor log-analytics workspace get-shared-keys \
    --resource-group $RESOURCE_GROUP_NAME\
    --workspace-name $WORKSPACE_NAME \
    --query primarySharedKey output tsv)

CUSTOMERID=$(az monitor log-analytics workspace show \
    --resource-group $RESOURCE_GROUP_NAME \
    --workspace-name $WORKSPACE_NAME \
    --query customerId --output tsv)

# create the environment
az containerapp env create --name $ENVIRONMENT \
    --resource-group $RESOURCE_GROUP_NAME \
    --location $LOCATION \
    --logs-workspace-id $CUSTOMERID \
    --logs-workspace-key $KEY \

# create the registry
az acr create --resource-group $RESOURCE_GROUP_NAME \
    --name $REGISTRY_NAME \
    --sku Basic \
    --admin-enabled true

# create the Cosmos account
az cosmosdb create --resource-group $RESOURCE_GROUP_NAME \
    --name $COSMOS_ACCOUNT_NAME \
    --kind GlobalDocumentDB \
    --enable-free-tier true

# create the database
az cosmosdb sql database create \
    --resource-group $RESOURCE_GROUP_NAME \
    --account-name $COSMOS_ACCOUNT_NAME \
    --name $DATABASE_NAME \
    --throughput 400
