#Requires -Version 5.1
<#
.SYNOPSIS
  Full clean-slate wipe for local demo DB.
  Deletes clients, contracts, declarations, exercices, workflows instances, commercial, etc.
  Keeps cabinet employees/users, roles, reference data, departments, WF templates.
.PARAMETER ConfirmWipe
  Required safety flag.
#>
param(
    [switch]$ConfirmWipe,
    [string]$Server = "localhost,58080",
    [string]$Database = "MetaComptaSASDb_Dev",
    [string]$User = "sa",
    [string]$Password = "RS2iRS2i"
)

$ErrorActionPreference = "Stop"
if (-not $ConfirmWipe) {
    Write-Error "Refusing to wipe. Re-run with -ConfirmWipe. This deletes ALL clients and ops data."
    exit 1
}

$sqlFile = Join-Path $PSScriptRoot "wipe_clean_slate.sql"
if (-not (Test-Path $sqlFile)) { throw "Missing $sqlFile" }

Write-Host "CLEAN SLATE on $Server / $Database ..." -ForegroundColor Yellow
Write-Host "Keeping cabinet staff users. Deleting clients/contracts/declarations/exercices/workflows..." -ForegroundColor Yellow
sqlcmd -S $Server -d $Database -U $User -P $Password -C -b -I -i $sqlFile
if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed ($LASTEXITCODE)" }

Write-Host ""
Write-Host "Done. Log in as gerant@cabinet-demo.ma and start fresh:" -ForegroundColor Green
Write-Host "  1) Create client"
Write-Host "  2) Create + validate contrat (services)"
Write-Host "  3) Open exercice (menu Exercices)"
Write-Host "  4) Test declarations / CNSS / clôture"
