.PHONY: all clean test

all: terraform.tfplan

clean:
	rm -f terraform.tfplan

test:
	terraform validate

# TODO: This makefile doesn't know how to build the .zip dependencies.
# TODO: Manually acquire a lock so plan doesn't waste time releasing it.
terraform.tfplan: $(shell find . -type f -name "*.tf" -o -name "*.yml") $(shell find ../api -type f -name "*.zip")
	terraform plan -out terraform.tfplan && \
	terraform apply -auto-approve terraform.tfplan
