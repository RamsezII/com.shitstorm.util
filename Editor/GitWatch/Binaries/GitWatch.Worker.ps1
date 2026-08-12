#requires -Version 5.1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Root,

    [ValidateSet('Status', 'Fetch', 'PullSafe')]
    [string] $Mode = 'Status',

    [Parameter(Mandatory = $true)]
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
$env:GIT_TERMINAL_PROMPT = '0'

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)] [string] $Repository,
        [Parameter(Mandatory = $true)] [string[]] $Arguments,
        [int] $TimeoutSeconds = 120
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = 'git.exe'
    $startInfo.WorkingDirectory = $Repository
    $startInfo.Arguments = $Arguments -join ' '
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $startInfo.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo

    try {
        [void] $process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()

        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            try { $process.Kill() } catch { }
            return [pscustomobject]@{
                Success = $false
                ExitCode = -1
                Output = ''
                Error = "Délai dépassé après $TimeoutSeconds secondes."
            }
        }

        $stdout = $stdoutTask.Result.Trim()
        $stderr = $stderrTask.Result.Trim()
        return [pscustomobject]@{
            Success = ($process.ExitCode -eq 0)
            ExitCode = $process.ExitCode
            Output = $stdout
            Error = $stderr
        }
    }
    catch {
        return [pscustomobject]@{
            Success = $false
            ExitCode = -1
            Output = ''
            Error = $_.Exception.Message
        }
    }
    finally {
        $process.Dispose()
    }
}

function Find-GitRepositories {
    param([Parameter(Mandatory = $true)] [string] $SearchRoot)

    $ignoredDirectories = @(
        '.git', '.vs', '.idea', '.cache',
        'Library', 'Temp', 'Logs', 'obj',
        'Build', 'Builds', 'MemoryCaptures', 'Recordings',
        'node_modules'
    )

    $found = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    $pending = New-Object 'System.Collections.Generic.Stack[string]'
    $pending.Push([System.IO.Path]::GetFullPath($SearchRoot))

    while ($pending.Count -gt 0) {
        $directory = $pending.Pop()
        $gitMarker = Join-Path $directory '.git'

        if (Test-Path -LiteralPath $gitMarker) {
            [void] $found.Add($directory)
        }

        foreach ($child in Get-ChildItem -LiteralPath $directory -Directory -Force -ErrorAction SilentlyContinue) {
            if ($ignoredDirectories -notcontains $child.Name) {
                $pending.Push($child.FullName)
            }
        }
    }

    return @($found | Sort-Object { if ($_ -eq [System.IO.Path]::GetFullPath($SearchRoot)) { '0' } else { '1' + $_ } })
}

function Format-RelativeTime {
    param([string] $IsoDate)

    try {
        $commitDate = [DateTimeOffset]::Parse($IsoDate, [System.Globalization.CultureInfo]::InvariantCulture)
        $minutes = [Math]::Max(0, ([DateTimeOffset]::Now - $commitDate).TotalMinutes)

        if ($minutes -lt 1) { return "à l'instant" }
        if ($minutes -lt 60) { return "il y a $([Math]::Floor($minutes)) min" }
        if ($minutes -lt 1440) { return "il y a $([Math]::Floor($minutes / 60)) h" }
        if ($minutes -lt 10080) { return "il y a $([Math]::Floor($minutes / 1440)) j" }
        if ($minutes -lt 43800) { return "il y a $([Math]::Floor($minutes / 10080)) sem." }
        if ($minutes -lt 525600) { return "il y a $([Math]::Floor($minutes / 43800)) mois" }

        $years = [Math]::Floor($minutes / 525600)
        if ($years -eq 1) { return 'il y a 1 an' }
        return "il y a $years ans"
    }
    catch {
        return $IsoDate
    }
}

function Get-RepositoryState {
    param(
        [Parameter(Mandatory = $true)] [string] $Repository,
        [string] $Operation = ''
    )

    $displayName = Split-Path -Leaf $Repository
    if ($displayName.StartsWith('_') -and $displayName.EndsWith('_')) {
        $displayName = $displayName.Trim('_')
    }

    $branchResult = Invoke-Git -Repository $Repository -Arguments @('branch', '--show-current') -TimeoutSeconds 20
    if (-not $branchResult.Success) {
        return [pscustomobject][ordered]@{
            Name = $displayName
            Path = $Repository
            Branch = '—'
            DirtyCount = 0
            LocalText = 'Inaccessible'
            Ahead = 0
            Behind = 0
            HasUpstream = $false
            SyncText = 'Erreur Git'
            StatusKey = 'Error'
            StatusColor = '#FF6B7A'
            StatusBackground = '#331820'
            Author = '—'
            LastCommit = '—'
            Subject = $branchResult.Error
            Remote = ''
            Operation = $Operation
            Error = $branchResult.Error
        }
    }

    $branch = $branchResult.Output
    if ([string]::IsNullOrWhiteSpace($branch)) {
        $headResult = Invoke-Git -Repository $Repository -Arguments @('rev-parse', '--short', 'HEAD') -TimeoutSeconds 20
        $branch = if ($headResult.Success) { "HEAD détachée · $($headResult.Output)" } else { 'Dépôt vide' }
    }

    $statusResult = Invoke-Git -Repository $Repository -Arguments @('status', '--porcelain', '--untracked-files=normal') -TimeoutSeconds 60
    $dirtyCount = 0
    if ($statusResult.Success -and -not [string]::IsNullOrWhiteSpace($statusResult.Output)) {
        $dirtyCount = @($statusResult.Output -split "`r?`n" | Where-Object { $_ }).Count
    }

    $upstreamResult = Invoke-Git -Repository $Repository -Arguments @('rev-parse', '--abbrev-ref', '--symbolic-full-name', '@{upstream}') -TimeoutSeconds 20
    $hasUpstream = $upstreamResult.Success -and -not [string]::IsNullOrWhiteSpace($upstreamResult.Output)
    $ahead = 0
    $behind = 0

    if ($hasUpstream) {
        $distanceResult = Invoke-Git -Repository $Repository -Arguments @('rev-list', '--left-right', '--count', "HEAD...$($upstreamResult.Output)") -TimeoutSeconds 30
        if ($distanceResult.Success -and $distanceResult.Output -match '^(\d+)\s+(\d+)$') {
            $ahead = [int] $Matches[1]
            $behind = [int] $Matches[2]
        }
    }

    $localText = if (-not $statusResult.Success) {
        'État inconnu'
    } elseif ($dirtyCount -eq 0) {
        'Propre'
    } elseif ($dirtyCount -eq 1) {
        '1 changement'
    } else {
        "$dirtyCount changements"
    }

    if ($ahead -gt 0 -and $behind -gt 0) {
        $syncText = "↕ $ahead devant · $behind derrière"
        $statusKey = 'Diverged'
        $statusColor = '#FF6B7A'
        $statusBackground = '#331820'
    }
    elseif ($behind -gt 0) {
        $syncText = "↓ $behind derrière"
        $statusKey = 'Behind'
        $statusColor = '#FFB45E'
        $statusBackground = '#352718'
    }
    elseif ($ahead -gt 0) {
        $syncText = "↑ $ahead devant"
        $statusKey = 'Ahead'
        $statusColor = '#66A6FF'
        $statusBackground = '#172A46'
    }
    elseif (-not $hasUpstream) {
        $syncText = 'Sans suivi distant'
        $statusKey = 'NoUpstream'
        $statusColor = '#AAB2C2'
        $statusBackground = '#252A34'
    }
    elseif ($dirtyCount -gt 0) {
        $syncText = 'Commits à jour'
        $statusKey = 'Modified'
        $statusColor = '#EAC66B'
        $statusBackground = '#302B1A'
    }
    else {
        $syncText = 'À jour'
        $statusKey = 'Clean'
        $statusColor = '#61D6A3'
        $statusBackground = '#183329'
    }

    $logResult = Invoke-Git -Repository $Repository -Arguments @('log', '-1', '--format=%an%x1f%aI%x1f%s') -TimeoutSeconds 20
    $author = '—'
    $lastCommit = 'Aucun commit'
    $subject = ''
    if ($logResult.Success -and -not [string]::IsNullOrWhiteSpace($logResult.Output)) {
        $logParts = $logResult.Output -split ([char]0x1f), 3
        if ($logParts.Count -ge 1) { $author = $logParts[0] }
        if ($logParts.Count -ge 2) { $lastCommit = Format-RelativeTime -IsoDate $logParts[1] }
        if ($logParts.Count -ge 3) { $subject = $logParts[2] }
    }

    $remoteResult = Invoke-Git -Repository $Repository -Arguments @('config', '--get', 'remote.origin.url') -TimeoutSeconds 20

    return [pscustomobject][ordered]@{
        Name = $displayName
        Path = $Repository
        Branch = $branch
        DirtyCount = $dirtyCount
        LocalText = $localText
        Ahead = $ahead
        Behind = $behind
        HasUpstream = $hasUpstream
        SyncText = $syncText
        StatusKey = $statusKey
        StatusColor = $statusColor
        StatusBackground = $statusBackground
        Author = $author
        LastCommit = $lastCommit
        Subject = $subject
        Remote = if ($remoteResult.Success) { $remoteResult.Output } else { '' }
        Operation = $Operation
        Error = if ($statusResult.Success) { '' } else { $statusResult.Error }
    }
}

function Write-ResultFile {
    param([Parameter(Mandatory = $true)] $Value)

    $parentDirectory = Split-Path -Parent $OutputPath
    if (-not (Test-Path -LiteralPath $parentDirectory)) {
        [void] (New-Item -ItemType Directory -Path $parentDirectory -Force)
    }

    $json = $Value | ConvertTo-Json -Depth 6
    $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($OutputPath, $json, $utf8WithoutBom)
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
    $repositories = @(Find-GitRepositories -SearchRoot $resolvedRoot)
    $states = @()

    foreach ($repository in $repositories) {
        $operation = ''

        if ($Mode -eq 'Fetch' -or $Mode -eq 'PullSafe') {
            $fetchResult = Invoke-Git -Repository $repository -Arguments @('fetch', '--all', '--prune', '--quiet') -TimeoutSeconds 180
            if ($fetchResult.Success) {
                $operation = 'Fetch effectué'
            }
            else {
                $operation = "Fetch impossible : $($fetchResult.Error)"
            }
        }

        $state = Get-RepositoryState -Repository $repository -Operation $operation

        if ($Mode -eq 'PullSafe' -and $state.Error -eq '') {
            if ($state.DirtyCount -gt 0) {
                $state.Operation = 'Ignoré : modifications locales'
            }
            elseif (-not $state.HasUpstream) {
                $state.Operation = 'Ignoré : aucune branche distante suivie'
            }
            elseif ($state.Ahead -gt 0 -and $state.Behind -gt 0) {
                $state.Operation = 'Ignoré : historique divergent'
            }
            elseif ($state.Behind -gt 0 -and $state.Ahead -eq 0) {
                $commitsToPull = $state.Behind
                $pullResult = Invoke-Git -Repository $repository -Arguments @('pull', '--ff-only', '--quiet') -TimeoutSeconds 180
                if ($pullResult.Success) {
                    $state = Get-RepositoryState -Repository $repository -Operation "Mis à jour · $commitsToPull commit(s)"
                }
                else {
                    $state.Operation = "Pull impossible : $($pullResult.Error)"
                    $state.StatusKey = 'Error'
                    $state.StatusColor = '#FF6B7A'
                    $state.StatusBackground = '#331820'
                }
            }
            elseif ($state.Ahead -gt 0) {
                $state.Operation = 'Aucun pull : dépôt local en avance'
            }
            else {
                $state.Operation = 'Déjà à jour'
            }
        }

        $states += $state
    }

    $stopwatch.Stop()
    $summary = [ordered]@{
        Total = $states.Count
        Clean = @($states | Where-Object { $_.StatusKey -eq 'Clean' }).Count
        Modified = @($states | Where-Object { $_.DirtyCount -gt 0 }).Count
        Behind = @($states | Where-Object { $_.Behind -gt 0 }).Count
        Ahead = @($states | Where-Object { $_.Ahead -gt 0 }).Count
        Attention = @($states | Where-Object { $_.StatusKey -in @('Diverged', 'Error', 'NoUpstream') }).Count
        Updated = @($states | Where-Object { $_.Operation -like 'Mis à jour*' }).Count
        Skipped = @($states | Where-Object { $_.Operation -like 'Ignoré*' }).Count
    }

    Write-ResultFile -Value ([ordered]@{
        Success = $true
        Mode = $Mode
        Root = $resolvedRoot
        GeneratedAt = [DateTime]::Now.ToString('o')
        DurationMs = $stopwatch.ElapsedMilliseconds
        Summary = $summary
        Repositories = @($states)
    })
}
catch {
    $stopwatch.Stop()
    Write-ResultFile -Value ([ordered]@{
        Success = $false
        Mode = $Mode
        Root = $Root
        GeneratedAt = [DateTime]::Now.ToString('o')
        DurationMs = $stopwatch.ElapsedMilliseconds
        Error = "$($_.Exception.Message)`n$($_.ScriptStackTrace)"
        Repositories = @()
    })
    exit 1
}
