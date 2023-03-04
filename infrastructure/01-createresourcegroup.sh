#! /bin/bash

RESOURCE_GROUP_NAME=codebreaker
LOCATION=westeurope

# create the resource group
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION