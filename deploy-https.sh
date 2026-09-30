#!/usr/bin/env bash
# MetaCompta — enable HTTPS + point metacompta.ma to BackOffice
# Run on the VPS as ubuntu:  bash deploy-https.sh
set -euo pipefail

APP_DIR="/home/ubuntu/MetaComptaSAS"
cd "$APP_DIR"

echo "==> Writing Caddyfile"
cat > Caddyfile <<'EOF'
metacompta.ma, www.metacompta.ma {
	encode gzip
	reverse_proxy backoffice:8080
}

api.metacompta.ma {
	encode gzip
	reverse_proxy api:8080
}

extranet.metacompta.ma {
	encode gzip
	reverse_proxy extranet:8080
}

admin.metacompta.ma {
	encode gzip
	reverse_proxy admin:8080
}
EOF

echo "==> Patching docker-compose.yml for Caddy + internal ports"
python3 <<'PY'
from pathlib import Path
p = Path("docker-compose.yml")
text = p.read_text()
if "metacompta-caddy" in text and "caddy-data" in text:
    print("compose already has caddy")
else:
    # Ensure caddy service exists at top of services
    if "  caddy:" not in text:
        insert = '''  caddy:
    image: caddy:2-alpine
    container_name: metacompta-caddy
    ports:
      - "80:80"
      - "443:443"
      - "443:443/udp"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy-data:/data
      - caddy-config:/config
    depends_on:
      - api
      - backoffice
      - extranet
      - admin
    networks:
      - metacompta-network
    restart: unless-stopped

'''
        text = text.replace("services:\n", "services:\n" + insert, 1)
    if "caddy-data:" not in text:
        if "volumes:\n  mssql-data:" in text:
            text = text.replace(
                "volumes:\n  mssql-data:\n  metacompta-uploads:",
                "volumes:\n  mssql-data:\n  metacompta-uploads:\n  caddy-data:\n  caddy-config:",
                1,
            )
        else:
            text += "\n  caddy-data:\n  caddy-config:\n"
    p.write_text(text)
    print("compose updated")
PY

# Prefer expose over public 710x so only Caddy is public (keep working if ports already mapped)
# Stop hello-web nginx that occupies port 80
echo "==> Stopping containers that occupy port 80/443"
sudo docker rm -f hello-web 2>/dev/null || true
sudo docker ps -a --format '{{.Names}} {{.Ports}}' | grep -E '0\.0\.0\.0:80->|:::80' | awk '{print $1}' | while read -r n; do
  [ "$n" = "metacompta-caddy" ] && continue
  echo "removing $n (uses port 80)"
  sudo docker rm -f "$n" || true
done

echo "==> Firewall (ufw)"
sudo ufw allow 22/tcp || true
sudo ufw allow 80/tcp || true
sudo ufw allow 443/tcp || true
sudo ufw reload || true

echo "==> Starting stack with Caddy"
sudo docker compose up -d
# Force recreate caddy if needed
sudo docker compose up -d caddy --force-recreate

echo "==> Waiting for HTTPS / HTTP"
sleep 8
sudo docker compose ps
echo "--- local http ---"
curl -sI -H 'Host: metacompta.ma' http://127.0.0.1/ | head -5 || true
echo "--- caddy logs ---"
sudo docker logs metacompta-caddy --tail 40 || true

echo ""
echo "DONE. Open: https://metacompta.ma/"
echo "Also: https://api.metacompta.ma  https://extranet.metacompta.ma  https://admin.metacompta.ma"
echo "(subdomains need DNS A records → 57.128.183.107)"
