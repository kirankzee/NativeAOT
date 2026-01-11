# PowerShell script to publish both APIs
param(
    [string]$Configuration = "Release"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Publishing APIs..." -ForegroundColor Green

# Publish JIT API
Write-Host "Publishing JIT API..." -ForegroundColor Yellow
Set-Location "$ProjectRoot\src\Api.Jit"
dotnet publish -c $Configuration -o "$ProjectRoot\publish\jit"

# Publish AOT API
Write-Host "Publishing Native AOT API..." -ForegroundColor Yellow
Set-Location "$ProjectRoot\src\Api.Aot"
dotnet publish -c $Configuration -o "$ProjectRoot\publish\aot"

Write-Host "APIs published successfully!" -ForegroundColor Green
Write-Host "JIT API: $ProjectRoot\publish\jit"
Write-Host "AOT API: $ProjectRoot\publish\aot"

