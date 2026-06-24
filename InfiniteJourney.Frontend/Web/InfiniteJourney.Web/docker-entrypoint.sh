#!/bin/sh
set -e

CONFIG_PATH="/usr/share/nginx/html/assets/app-config.json"

cat > "$CONFIG_PATH" <<EOF
{
  "apiPort": "${API_PORT:-5274}",
  "keycloak": {
    "url": "${KEYCLOAK_URL:-http://localhost:8080}",
    "realm": "${KEYCLOAK_REALM:-InfiniteJourney}",
    "clientId": "${KEYCLOAK_CLIENT_ID:-infinite-journey-web}"
  }
}
EOF

exec nginx -g 'daemon off;'
