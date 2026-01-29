#!/usr/bin/env bash
set -euo pipefail

SSL_DIR="./ssl"
CERT_NAME="JustineClient"
PFX_PATH="$SSL_DIR/${CERT_NAME}.pfx"
CRT_PATH="$SSL_DIR/${CERT_NAME}.crt"
KEY_PATH="$SSL_DIR/${CERT_NAME}.key"
PW="$(openssl rand -base64 12)"  # random password for export

mkdir -p "$SSL_DIR"
chmod 700 "$SSL_DIR"

# Check dependencies
command -v dotnet >/dev/null 2>&1 || { echo "dotnet required"; exit 1; }
command -v openssl >/dev/null 2>&1 || { echo "openssl required"; exit 1; }

# Export dev cert to PFX
dotnet dev-certs https -ep "$PFX_PATH" -p "$PW"

# Extract key and cert
openssl pkcs12 -in "$PFX_PATH" -nocerts -nodes -out "$KEY_PATH" -passin pass:"$PW"
openssl pkcs12 -in "$PFX_PATH" -clcerts -nokeys -out "$CRT_PATH" -passin pass:"$PW"

# Restrict permissions
chmod 600 "$KEY_PATH" "$CRT_PATH"
# Optional: keep or remove PFX; keep for potential re-use, otherwise remove
rm -f "$PFX_PATH"

echo "Generated: $CRT_PATH and $KEY_PATH"

# Platform-specific trust
if command -v powershell.exe >/dev/null 2>&1; then
  echo "Trusting cert via PowerShell (Windows/Git Bash/WSL)..."
  # Convert forward slashes to backslashes for PowerShell path
  WIN_CRT_PATH="${CRT_PATH//\//\\}"
  # If path starts with .\, convert to relative Windows style .\ssl\...
  powershell.exe -NoProfile -Command "Import-Certificate -FilePath \"${WIN_CRT_PATH}\" -CertStoreLocation Cert:\CurrentUser\Root" || {
    echo "PowerShell trust step failed. Run the following in an elevated PowerShell:"
    echo "Import-Certificate -FilePath \"${WIN_CRT_PATH//\\\\/\\\/}\" -CertStoreLocation Cert:\CurrentUser\Root"
  }
else
  echo "Automatic trust not implemented for this OS. Import $CRT_PATH into your OS/browser trust store manually."
fi

echo "Done. Restart your browser if necessary."