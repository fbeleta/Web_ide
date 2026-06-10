#!/bin/sh
# Generates a self-signed TLS cert for local dev at the same path certbot would use.
# Run before nginx starts. If the cert already exists (e.g. real Let's Encrypt cert), do nothing.
#
# In production: certbot's volume mounts the real cert at /etc/letsencrypt/live/<domain>/
# and this script is a no-op because fullchain.pem already exists.

CERT_DIR="/etc/letsencrypt/live/localhost"

if [ -f "$CERT_DIR/fullchain.pem" ]; then
  echo "TLS cert already exists at $CERT_DIR — skipping self-signed generation."
  exit 0
fi

echo "Generating self-signed cert for local dev at $CERT_DIR ..."
mkdir -p "$CERT_DIR"

openssl req -x509 -nodes -newkey rsa:2048 -days 365 \
  -keyout "$CERT_DIR/privkey.pem" \
  -out    "$CERT_DIR/fullchain.pem" \
  -subj   "/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"

echo "Self-signed cert generated."
