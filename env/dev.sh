if [ -z $AWS_ACCESS_KEY_ID ] && [ -z $AWS_PROFILE ] && [ -f ~/.aws/credentials ]; then
    export AWS_PROFILE=dev
fi

export AWS_DEFAULT_REGION=us-east-1
export TF_VAR_account_alias=dev

export TF_VAR_route53_zone_name=dev.justine-developer.net
export TF_VAR_cert_domain_name=dev.justine-developer.net
export TF_VAR_api_cert_domain_name=*.dev.justine-developer.net
export TF_VAR_portal_domain_name=dev.justine-developer.net
export TF_VAR_cdn_domain_name=dev.justine-developer.net
export TF_VAR_domain_name=justine-developer.net
export TF_VAR_api_url=api.dev.justine-developer.net
export TF_VAR_s3_bucket_prefix=dev-
export TF_VAR_s3_bucket_name=dev-justine-developer-net
# export TF_VAR_portal_route53_record_name=dev2 # intentionally blank so that "" is the value
unset TF_VAR_portal_route53_record_name

export TF_VAR_account_number=792163935563
export TF_VAR_region=us-east-1
export TF_VAR_enable_point_in_time_recovery=false
export TF_VAR_sns_email="justinedeveloper@outlook.com"
export TF_VAR_sns_email_dynamo="justinedeveloper@outlook.com"