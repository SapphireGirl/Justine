locals {
  # NOTE: these are correlated to infrastructure locals
  cors_origin = var.account_alias == "dev" ? "*" : "https://${var.portal_domain_name}"

  Products_table_name         = "Products"
  Baskets_table_name		  = "Baskets"
  Orders_table_name			  = "Orders"
  

# For node lambda
  
}

