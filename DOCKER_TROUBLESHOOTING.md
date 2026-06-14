# Docker Build Issues - Troubleshooting Guide

## Issue
Docker can't pull images from mcr.microsoft.com due to a persistent proxy configuration at `http.docker.internal:3128`.

## What We've Tried
1. ✅ Disabled proxy in Docker Desktop UI - still persists
2. ✅ Updated daemon.json with explicit proxy="" - still persists  
3. ✅ Created custom buildx builder without proxy - still persists
4. ✅ Verified containers CAN reach MCR (curl test worked)
5. ✅ The issue is specifically with Docker build daemon, not containers

## Workaround Solutions

### Option 1: Reset Docker Desktop (Recommended)
This will clear all Docker configuration including the persistent proxy:
1. Open Docker Desktop
2. Go to Settings → Troubleshoot  
3. Click "Reset to factory defaults"
4. Restart Docker Desktop
5. Run: `docker-compose up --build`

⚠️ This will delete all your Docker containers, images, and volumes!

### Option 2: Use Visual Studio's Docker Support
Visual Studio might handle the proxy differently:
1. Right-click `traderview.Server` project in Solution Explorer
2. Select "Add" → "Docker Support..."
3. Choose "Linux"
4. Let VS generate the Dockerfile
5. Right-click solution → "Set Startup Projects"
6. Choose "Docker Compose"
7. Press F5 to run

### Option 3: Manual Image Pull via Network Troubleshooting
The proxy `http.docker.internal:3128` suggests Docker Desktop is trying to use a corporate proxy or VPN tunnel.

Check:
- Are you connected to a corporate VPN? Try disconnecting
- Do you have corporate security software (Zscaler, Cisco AnyConnect, etc.)? Try temporarily disabling
- Check Windows → Settings → Network & Internet → Proxy - ensure it's off

### Option 4: Build on a Different Machine/Environment
- Use WSL2 and Docker in Linux instead of Docker Desktop
- Use GitHub Actions or Azure DevOps to build the images
- Build on a cloud VM

## Current Configuration Status
- Docker Version: 4.77.0  
- Proxy showing in `docker info`: http.docker.internal:3128
- Direct container network access: ✅ Working
- Docker build network access: ❌ Blocked by proxy

## Files Created
All Docker files are ready in your workspace:
- `traderview/traderview.Server/Dockerfile`
- `traderview/traderview.client/Dockerfile`  
- `docker-compose.yml`
- `.dockerignore`
- `.env.example`

Once the proxy issue is resolved, simply run:
```powershell
docker-compose up --build
```
