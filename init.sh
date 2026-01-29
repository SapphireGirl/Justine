#!/bin/sh
set -e

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <environment>"
    exit 1
fi

cd portal
npm install
cd ..

echo "========================="
echo ${TF_VAR_account_alias}
echo "========================="

echo "initializing infrastructure"
cd infrastructure
make clean
export TF_LOG=TRACE

terraform init -backend-config=backend_configs/${TF_VAR_account_alias} -reconfigure
terraform workspace select $1
cd ..

echo "initializing apiDeploy"
cd apiDeploy
make clean
export TF_LOG=TRACE
terraform init -backend-config=backend_configs/${TF_VAR_account_alias} -reconfigure
terraform workspace select $1
cd ..
