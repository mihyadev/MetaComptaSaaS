#Requires -Version 5.1
<#
.SYNOPSIS
  Wipe demo operational data (declarations, paie, écritures, services, exercices).
  Keeps cabinets, users, roles, reference data, clients master, PCGM/journaux.
.PARAMETER ConfirmWipe
  Required safety flag. Without it the script aborts.
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
    Write-Error "Refusing to wipe. Re-run with -ConfirmWipe. This is irreversible for ops data."
    exit 1
}

$sqlFile = Join-Path $PSScriptRoot "wipe_demo_ops_data.sql"
if (-not (Test-Path $sqlFile)) { throw "Missing $sqlFile" }

Write-Host "Wiping ops data on $Server / $Database ..."
sqlcmd -S $Server -d $Database -U $User -P $Password -C -b -I -i $sqlFile
if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed ($LASTEXITCODE)" }
Write-Host "Done. Create an open exercice per client before using declarations/compta/paie."
