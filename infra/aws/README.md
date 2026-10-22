# AWS EC2 deployment

This directory contains a production-oriented single-host pilot manifest. It
does not provision AWS resources. Caddy owns ports 80/443 and automatic TLS,
Nginx serves Angular and forwards same-origin `/api` and `/hubs` traffic, while
the API and PostgreSQL remain private to the Docker network. The API trusts only
the fixed Nginx container address for forwarded HTTPS headers.

## Host layout

Clone the repositories as sibling directories. The directory names are part of
the Compose build contexts:

```text
~/campfire-deploy/
  api/  # https://github.com/nguyentuananh2808/campfire-Api.git
  web/  # https://github.com/nguyentuananh2808/campfire.git
```

## Configure and start

Point an `A` DNS record at the EC2 Elastic IP first. Then run:

```bash
cd ~/campfire-deploy/api/infra/aws
cp .env.ec2.example .env.ec2
chmod 600 .env.ec2
# Edit .env.ec2 and replace every placeholder with real values.
docker compose --env-file .env.ec2 -f compose.ec2.yml config --quiet
docker compose --env-file .env.ec2 -f compose.ec2.yml build
docker compose --env-file .env.ec2 -f compose.ec2.yml up -d
docker compose --env-file .env.ec2 -f compose.ec2.yml ps
```

The EC2 security group should allow TCP 80/443, UDP 443, LiveKit TCP 7881 and
UDP 50000-50020 from the Internet. Restrict TCP 22 to the administrator's IP.
Do not expose PostgreSQL 5432 or API 8080. Persist and back up the PostgreSQL,
Data Protection and Caddy volumes. Test restore before relying on the instance.

## Update

```bash
cd ~/campfire-deploy/api && git pull --ff-only origin main
cd ~/campfire-deploy/web && git pull --ff-only origin main
cd ~/campfire-deploy/api/infra/aws
docker compose --env-file .env.ec2 -f compose.ec2.yml build
docker compose --env-file .env.ec2 -f compose.ec2.yml up -d
```

Public registration and Stranger Chat should remain controlled until email
recovery/verification, moderation, privacy and support operations are ready.
