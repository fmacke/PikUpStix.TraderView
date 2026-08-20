# start-app.ps1
# Intelligent start script that rebuilds Docker containers only if code has changed

param(
    [switch]$Force,
    [switch]$NoBuild,
    [switch]$Logs,
    [switch]$NoCache
)

Write-Host "=== TraderView Application Deployment ===" -ForegroundColor Cyan

# Configuration
$ImageName = "traderview-app"
$ContainerName = "traderview-app"
$BuildMarkerFile = ".last-build-hash"
$SourcePaths = @(
    "traderview/traderview.Server",
    "traderview/traderview.client"
)

# Function to calculate hash of source files
function Get-SourceHash {
    $files = Get-ChildItem -Path $SourcePaths -Include *.cs,*.ts,*.tsx,*.jsx,*.html,*.css,*.scss,*.sass,*.json,*.csproj,*.esproj -Recurse -File
    $hashes = $files | ForEach-Object { 
        (Get-FileHash -Path $_.FullName -Algorithm MD5).Hash 
    }
    $combinedHash = ($hashes -join '') | Get-FileHash -Algorithm MD5
    return $combinedHash.Hash
}

# Function to check if rebuild is needed
function Test-RebuildNeeded {
    Write-Host "`nChecking if rebuild is needed..." -ForegroundColor Yellow
    
    # Get current source hash
    $currentHash = Get-SourceHash
    Write-Host "Current source hash: $currentHash" -ForegroundColor Gray
    
    # Check if marker file exists
    if (-not (Test-Path $BuildMarkerFile)) {
        Write-Host "No previous build marker found. Rebuild needed." -ForegroundColor Yellow
        return $true
    }
    
    # Read previous hash
    $previousHash = Get-Content $BuildMarkerFile -Raw
    Write-Host "Previous build hash: $previousHash" -ForegroundColor Gray
    
    # Check if Docker image exists
    $imageExists = docker images -q $ImageName
    if (-not $imageExists) {
        Write-Host "Docker image not found. Rebuild needed." -ForegroundColor Yellow
        return $true
    }
    
    # Compare hashes
    if ($currentHash -ne $previousHash) {
        Write-Host "Source code has changed. Rebuild needed." -ForegroundColor Yellow
        return $true
    }
    
    Write-Host "Source code unchanged. Using existing Docker image." -ForegroundColor Green
    return $false
}

# Function to build Docker images
function Build-DockerImages {
    param([bool]$UseNoCache = $false)
    
    Write-Host "`nBuilding Docker images..." -ForegroundColor Cyan
    
    if ($UseNoCache) {
        Write-Host "Building with --no-cache flag..." -ForegroundColor Yellow
        docker-compose build --no-cache
    } else {
        docker-compose build
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Docker build completed successfully!" -ForegroundColor Green
        
        # Save the current hash
        $currentHash = Get-SourceHash
        $currentHash | Out-File -FilePath $BuildMarkerFile -NoNewline
        Write-Host "Build marker updated: $currentHash" -ForegroundColor Gray
        return $true
    } else {
        Write-Host "Docker build failed!" -ForegroundColor Red
        return $false
    }
}

# Function to start containers
function Start-Containers {
    Write-Host "`nStarting Docker containers..." -ForegroundColor Cyan
    docker-compose up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Containers started successfully!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "Failed to start containers!" -ForegroundColor Red
        return $false
    }
}

# Main execution logic
try {
    # Check if Force flag is set
    if ($Force) {
        Write-Host "Force rebuild requested." -ForegroundColor Yellow
        $needsRebuild = $true
    } elseif ($NoBuild) {
        Write-Host "Skipping build check (NoBuild flag set)." -ForegroundColor Yellow
        $needsRebuild = $false
    } else {
        $needsRebuild = Test-RebuildNeeded
    }
    
    # Build if needed
    if ($needsRebuild) {
        $buildSuccess = Build-DockerImages
        if (-not $buildSuccess) {
            Write-Host "`nDeployment failed due to build errors." -ForegroundColor Red
            exit 1
        }
    }
    
    # Start containers
    $startSuccess = Start-Containers
    if (-not $startSuccess) {
        Write-Host "`nDeployment failed due to container startup errors." -ForegroundColor Red
        exit 1
    }
    
    # Show status
    Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
    Write-Host "`nContainer Status:" -ForegroundColor Cyan
    docker-compose ps
    
    # Show logs if requested
    if ($Logs) {
        Write-Host "`nShowing logs (Ctrl+C to exit)..." -ForegroundColor Cyan
        docker-compose logs -f $ContainerName
    } else {
        Write-Host "`nApplication is running. Access at: http://localhost:5000" -ForegroundColor Green
        Write-Host "To view logs, run: docker-compose logs -f" -ForegroundColor Gray
        Write-Host "To stop, run: docker-compose down" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "`nError: $_" -ForegroundColor Red
    exit 1
}