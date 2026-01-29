dotnet dev-certs https -ep ./ssl/JustineClient.pfx -p tempPass
openssl pkcs12 -in ../ssl/JustineClient.pfx -nocerts -nodes -out ./ssl/JustineClient.key -passin pass:tempPass
openssl pkcs12 -in ../ssl/JustineClient.pfx -clcerts -nokeys -out ./ssl/JustineClient.crt -passin pass:tempPass