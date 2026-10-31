[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$Project,
    [ValidateSet('Auto','Git','Local')]
    [string]$SourceType = 'Auto',
    [string]$Owner = 'Boca01',
    [string]$GitRoot = 'C:\Users\gdljboca\source\GIT',
    [string]$LocalRoot = 'C:\Users\gdljboca\source\Local',
    [string]$MirrorRoot = 'C:\Users\gdljboca\OneDrive\RepositoryVS\CloneGit',
    [switch]$All,
    [switch]$Preview
)

$ErrorActionPreference = 'Stop'

$ExcludedDirectories = @(
    '.git', '.vs', 'bin', 'obj', 'Debug', 'Release',
    'publish', 'Publish', 'TestResults', 'packages', 'node_modules'
)

function Invoke-Mirror {
    param(
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][string]$Destination
    )

    if (-not (Test-Path -LiteralPath $Source -PathType Container)) {
        throw "No existe la carpeta origen: $Source"
    }

    if (-not (Test-Path -LiteralPath $Destination)) {
        New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    }

    $args = @(
        $Source,
        $Destination,
        '/MIR',
        '/FFT',
        '/R:2',
        '/W:1',
        '/COPY:DAT',
        '/DCOPY:DAT',
        '/XJ',
        '/NP',
        '/XD'
    ) + $ExcludedDirectories

    if ($Preview) {
        $args += '/L'
    }

    Write-Host "`nOrigen:  $Source" -ForegroundColor Cyan
    Write-Host "Espejo:  $Destination" -ForegroundColor Cyan
    if ($Preview) {
        Write-Host 'MODO PREVIEW: no se modificara nada.' -ForegroundColor Yellow
    }

    & robocopy @args
    $code = $LASTEXITCODE

    # Robocopy usa 0-7 para resultados correctos/con cambios; 8+ indica fallo.
    if ($code -ge 8) {
        throw "Robocopy termino con codigo de error $code."
    }

    Write-Host "Sincronizacion terminada. Codigo Robocopy: $code" -ForegroundColor Green
}

function Get-ProjectMapping {
    param([Parameter(Mandatory)][string]$Name)

    $gitSource = Join-Path (Join-Path $GitRoot $Owner) $Name
    $localSource = Join-Path $LocalRoot $Name

    if ($SourceType -eq 'Git' -or ($SourceType -eq 'Auto' -and (Test-Path -LiteralPath $gitSource))) {
        return [pscustomobject]@{
            Source = $gitSource
            Destination = Join-Path (Join-Path $MirrorRoot $Owner) $Name
        }
    }

    if ($SourceType -eq 'Local' -or ($SourceType -eq 'Auto' -and (Test-Path -LiteralPath $localSource))) {
        return [pscustomobject]@{
            Source = $localSource
            Destination = Join-Path (Join-Path $MirrorRoot 'Local') $Name
        }
    }

    throw "No encontre '$Name' en GIT/$Owner ni en Local."
}

if ($All) {
    $items = @()

    $gitOwnerRoot = Join-Path $GitRoot $Owner
    if (Test-Path -LiteralPath $gitOwnerRoot) {
        Get-ChildItem -LiteralPath $gitOwnerRoot -Directory | ForEach-Object {
            $items += [pscustomobject]@{
                Source = $_.FullName
                Destination = Join-Path (Join-Path $MirrorRoot $Owner) $_.Name
            }
        }
    }

    if (Test-Path -LiteralPath $LocalRoot) {
        Get-ChildItem -LiteralPath $LocalRoot -Directory | ForEach-Object {
            $items += [pscustomobject]@{
                Source = $_.FullName
                Destination = Join-Path (Join-Path $MirrorRoot 'Local') $_.Name
            }
        }
    }

    foreach ($item in $items) {
        Invoke-Mirror -Source $item.Source -Destination $item.Destination
    }

    return
}

if ([string]::IsNullOrWhiteSpace($Project)) {
    throw "Indica -Project <nombre> o usa -All."
}

$mapping = Get-ProjectMapping -Name $Project
Invoke-Mirror -Source $mapping.Source -Destination $mapping.Destination
