#requires -Version 5.1

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Root,

    [ValidateSet('Status', 'Fetch', 'PullSafe', 'DiscardSuspicious', 'CommitPush')]
    [string] $Mode = 'Status',

    [Parameter(Mandatory = $true)]
    [string] $OutputPath,

    [string] $ProgressPath = '',

    [string] $CommitMessageBase64 = ''
)

$ErrorActionPreference = 'Stop'
$env:GIT_TERMINAL_PROMPT = '0'
$commitMessageFile = ''

function Write-ProgressEvent {
    param([Parameter(Mandatory = $true)] [string] $Message)

    if ([string]::IsNullOrWhiteSpace($ProgressPath)) { return }

    try {
        $progressDirectory = Split-Path -Parent $ProgressPath
        if (-not (Test-Path -LiteralPath $progressDirectory)) {
            [void] (New-Item -ItemType Directory -Path $progressDirectory -Force)
        }

        $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::AppendAllText($ProgressPath, $Message + [Environment]::NewLine, $utf8WithoutBom)
    }
    catch {
        # Le journal ne doit jamais interrompre une opération Git.
    }
}

function Write-RepositorySnapshot {
    param([Parameter(Mandatory = $true)] $State)

    if ([string]::IsNullOrWhiteSpace($ProgressPath)) { return }

    try {
        $json = $State | ConvertTo-Json -Depth 6 -Compress
        $payload = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($json))
        Write-ProgressEvent -Message ("@repo|" + $payload)
    }
    catch {
        # L'affichage progressif ne doit jamais interrompre l'analyse.
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)] [string] $Repository,
        [Parameter(Mandatory = $true)] [string[]] $Arguments,
        [int] $TimeoutSeconds = 120
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = 'git.exe'
    $startInfo.WorkingDirectory = $Repository
    $startInfo.Arguments = ($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') { '"' + $_.Replace('"', '\"') + '"' } else { $_ }
    }) -join ' '
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

function Get-ChangedPaths {
    param([Parameter(Mandatory = $true)] [string] $Repository)

    $paths = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    $commands = @(
        @('diff', '--name-only', '-z', '--'),
        @('diff', '--cached', '--name-only', '-z', '--'),
        @('ls-files', '--others', '--exclude-standard', '-z')
    )

    foreach ($arguments in $commands) {
        $result = Invoke-Git -Repository $Repository -Arguments $arguments -TimeoutSeconds 45
        if (-not $result.Success -or [string]::IsNullOrEmpty($result.Output)) { continue }

        foreach ($path in $result.Output -split "`0") {
            if (-not [string]::IsNullOrWhiteSpace($path)) {
                [void] $paths.Add($path)
            }
        }
    }

    return @($paths | Sort-Object)
}

function Get-TmpAtlasInfo {
    param([Parameter(Mandatory = $true)] [string] $AssetText)

    $texture = [regex]::Match(
        $AssetText,
        '(?ms)^Texture2D:\s*\r?\n.*?^  m_Width: (?<width>\d+)\r?$.*?^  m_Height: (?<height>\d+)\r?$'
    )
    $glyphBlock = [regex]::Match(
        $AssetText,
        '(?ms)^  m_GlyphTable:(?<glyphs>.*?)^  m_CharacterTable:'
    )

    return [pscustomobject]@{
        IsDynamicFontAsset = $AssetText -match '(?m)^  m_AtlasPopulationMode: 1\r?$'
        ClearDynamicDataOnBuild = $AssetText -match '(?m)^  m_ClearDynamicDataOnBuild: 1\r?$'
        Width = if ($texture.Success) { [int] $texture.Groups['width'].Value } else { 0 }
        Height = if ($texture.Success) { [int] $texture.Groups['height'].Value } else { 0 }
        GlyphCount = if ($glyphBlock.Success) {
            [regex]::Matches($glyphBlock.Groups['glyphs'].Value, '(?m)^  - m_Index: ').Count
        } else { 0 }
        GlyphTableIsEmpty = $AssetText -match '(?m)^  m_GlyphTable: \[\]\r?$'
        CharacterTableIsEmpty = $AssetText -match '(?m)^  m_CharacterTable: \[\]\r?$'
    }
}

function Find-SuspiciousChanges {
    param(
        [Parameter(Mandatory = $true)] [string] $Repository,
        [Parameter(Mandatory = $true)] [string[]] $ChangedPaths
    )

    $findings = @()

    foreach ($relativePath in $ChangedPaths) {
        if ([System.IO.Path]::GetExtension($relativePath) -ine '.asset') { continue }

        $absolutePath = Join-Path $Repository ($relativePath -replace '/', [System.IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $absolutePath -PathType Leaf)) { continue }

        try {
            $workingText = [System.IO.File]::ReadAllText($absolutePath)
        }
        catch {
            continue
        }

        if ($workingText -notmatch '(?m)^  m_AtlasPopulationMode: 1\r?$') { continue }

        $headResult = Invoke-Git -Repository $Repository -Arguments @('show', "HEAD:$relativePath") -TimeoutSeconds 60
        if (-not $headResult.Success) { continue }

        $headInfo = Get-TmpAtlasInfo -AssetText $headResult.Output
        $workingInfo = Get-TmpAtlasInfo -AssetText $workingText

        $atlasWasCleared =
            $headInfo.IsDynamicFontAsset -and
            $workingInfo.IsDynamicFontAsset -and
            $workingInfo.ClearDynamicDataOnBuild -and
            $headInfo.Width -gt 1 -and
            $headInfo.Height -gt 1 -and
            $headInfo.GlyphCount -gt 0 -and
            $workingInfo.Width -eq 1 -and
            $workingInfo.Height -eq 1 -and
            $workingInfo.GlyphTableIsEmpty -and
            $workingInfo.CharacterTableIsEmpty

        if ($atlasWasCleared) {
            $findings += [pscustomobject][ordered]@{
                Path = $relativePath
                Kind = 'TMPAtlasCleared'
                Label = 'Atlas TMP vidé'
                Reason = "Atlas TMP vidé automatiquement ($($headInfo.Width)×$($headInfo.Height) → 1×1 ; $($headInfo.GlyphCount) glyphes supprimés)."
                Advice = "À restaurer sauf si cette remise à zéro de la police était volontaire."
            }
        }
    }

    return @($findings)
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
            LocalColor = '#FF6B7A'
            LocalBackground = '#331820'
            ChangedFiles = @()
            SuspiciousChanges = @()
            SuspiciousCount = 0
            DiagnosticText = 'Erreur Git'
            RestoredCount = 0
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
    $changedPaths = @()
    $suspiciousChanges = @()
    if ($statusResult.Success -and -not [string]::IsNullOrWhiteSpace($statusResult.Output)) {
        $changedPaths = @(Get-ChangedPaths -Repository $Repository)
        $dirtyCount = $changedPaths.Count
        if ($changedPaths.Count -gt 0) {
            $suspiciousChanges = @(Find-SuspiciousChanges -Repository $Repository -ChangedPaths $changedPaths)
        }
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

    if (-not $statusResult.Success) {
        $localColor = '#FF6B7A'
        $localBackground = '#331820'
    }
    elseif ($dirtyCount -eq 0) {
        $localColor = '#61D6A3'
        $localBackground = '#183329'
    }
    else {
        $localColor = '#EAC66B'
        $localBackground = '#302B1A'
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
        $syncText = 'À jour'
        $statusKey = 'Modified'
        $statusColor = '#61D6A3'
        $statusBackground = '#183329'
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
        LocalColor = $localColor
        LocalBackground = $localBackground
        ChangedFiles = @($changedPaths)
        SuspiciousChanges = @($suspiciousChanges)
        SuspiciousCount = $suspiciousChanges.Count
        DiagnosticText = if ($suspiciousChanges.Count -eq 1) {
            '1 atlas TMP suspect'
        } elseif ($suspiciousChanges.Count -gt 1) {
            "$($suspiciousChanges.Count) atlas TMP suspects"
        } else { '' }
        RestoredCount = 0
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

    if ($Mode -eq 'CommitPush') {
        if ([string]::IsNullOrWhiteSpace($CommitMessageBase64)) {
            throw 'Le message de commit est manquant.'
        }

        try {
            $commitMessage = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($CommitMessageBase64))
        }
        catch {
            throw 'Le message de commit est illisible.'
        }

        if ([string]::IsNullOrWhiteSpace($commitMessage)) {
            throw 'Le message de commit ne peut pas être vide.'
        }

        $commitMessageFile = Join-Path ([System.IO.Path]::GetTempPath()) ("shitstorm-git-watch-message-{0}.txt" -f [Guid]::NewGuid().ToString('N'))
        $utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($commitMessageFile, $commitMessage.Trim(), $utf8WithoutBom)
    }

    Write-ProgressEvent -Message '@phase|discovery'
    Write-ProgressEvent -Message 'Recherche des dépôts Git…'
    $repositories = @(Find-GitRepositories -SearchRoot $resolvedRoot)
    $detectedLabel = if ($repositories.Count -eq 1) { '1 dépôt détecté' } else { "$($repositories.Count) dépôts détectés" }
    Write-ProgressEvent -Message $detectedLabel
    Write-ProgressEvent -Message "@progress|0|$($repositories.Count)"
    $states = @()

    for ($repositoryIndex = 0; $repositoryIndex -lt $repositories.Count; $repositoryIndex++) {
        $repository = $repositories[$repositoryIndex]
        $repositoryName = Split-Path -Leaf $repository
        if ($repositoryName.StartsWith('_') -and $repositoryName.EndsWith('_')) {
            $repositoryName = $repositoryName.Trim('_')
        }
        $position = $repositoryIndex + 1
        $operation = ''

        Write-ProgressEvent -Message "Analyse $position/$($repositories.Count) · $repositoryName"

        if ($Mode -eq 'Fetch' -or $Mode -eq 'PullSafe') {
            Write-ProgressEvent -Message "Fetch $position/$($repositories.Count) · $repositoryName"
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

        if ($Mode -eq 'CommitPush' -and $state.Error -eq '') {
            if ($state.DirtyCount -le 0) {
                $state.Operation = ''
            }
            else {
                Write-ProgressEvent -Message "Ajout au commit $position/$($repositories.Count) · $repositoryName"
                $addResult = Invoke-Git -Repository $repository -Arguments @('add', '--all') -TimeoutSeconds 120

                if (-not $addResult.Success) {
                    $state.Operation = "Ajout impossible : $($addResult.Error)"
                    $state.StatusKey = 'Error'
                    $state.StatusColor = '#FF6B7A'
                    $state.StatusBackground = '#331820'
                }
                else {
                    Write-ProgressEvent -Message "Commit $position/$($repositories.Count) · $repositoryName"
                    $commitResult = Invoke-Git -Repository $repository -Arguments @('commit', "--file=$commitMessageFile") -TimeoutSeconds 240

                    if (-not $commitResult.Success) {
                        $state = Get-RepositoryState -Repository $repository -Operation "Commit impossible : $($commitResult.Error)"
                        $state.StatusKey = 'Error'
                        $state.StatusColor = '#FF6B7A'
                        $state.StatusBackground = '#331820'
                    }
                    else {
                        $state = Get-RepositoryState -Repository $repository -Operation 'Commit effectué · push en attente'
                        Write-ProgressEvent -Message "Push $position/$($repositories.Count) · $repositoryName"

                        if ($state.HasUpstream) {
                            $pushResult = Invoke-Git -Repository $repository -Arguments @('push', '--quiet') -TimeoutSeconds 240
                        }
                        else {
                            $currentBranchResult = Invoke-Git -Repository $repository -Arguments @('symbolic-ref', '--quiet', '--short', 'HEAD') -TimeoutSeconds 20
                            if (-not $currentBranchResult.Success) {
                                $pushResult = [pscustomobject]@{ Success = $false; Error = 'HEAD est détachée : aucune branche ne peut être envoyée automatiquement.' }
                            }
                            elseif ([string]::IsNullOrWhiteSpace($state.Remote)) {
                                $pushResult = [pscustomobject]@{ Success = $false; Error = 'Aucun remote origin configuré.' }
                            }
                            else {
                                $pushResult = Invoke-Git -Repository $repository -Arguments @('push', '--quiet', '--set-upstream', 'origin', $currentBranchResult.Output) -TimeoutSeconds 240
                            }
                        }

                        if ($pushResult.Success) {
                            $state = Get-RepositoryState -Repository $repository -Operation 'Commit & push effectués'
                        }
                        else {
                            $state = Get-RepositoryState -Repository $repository -Operation "Commit effectué · push impossible : $($pushResult.Error)"
                            $state.StatusKey = 'Error'
                            $state.StatusColor = '#FF6B7A'
                            $state.StatusBackground = '#331820'
                        }
                    }
                }
            }
        }

        if ($Mode -eq 'DiscardSuspicious' -and $state.Error -eq '' -and $state.SuspiciousCount -gt 0) {
            $pathsToRestore = @($state.SuspiciousChanges | ForEach-Object { $_.Path })
            Write-ProgressEvent -Message "Restauration · $repositoryName · $($pathsToRestore.Count) atlas TMP"
            $restoreArguments = @('restore', '--source=HEAD', '--staged', '--worktree', '--') + $pathsToRestore
            $restoreResult = Invoke-Git -Repository $repository -Arguments $restoreArguments -TimeoutSeconds 120

            if ($restoreResult.Success) {
                $restoredCount = $pathsToRestore.Count
                $restoredLabel = if ($restoredCount -eq 1) { '1 atlas TMP restauré' } else { "$restoredCount atlas TMP restaurés" }
                $state = Get-RepositoryState -Repository $repository -Operation $restoredLabel
                $state.RestoredCount = $restoredCount
            }
            else {
                $state.Operation = "Restauration impossible : $($restoreResult.Error)"
                $state.StatusKey = 'Error'
                $state.StatusColor = '#FF6B7A'
                $state.StatusBackground = '#331820'
            }
        }

        $diagnosticSuffix = if ($state.SuspiciousCount -gt 0) { " · $($state.DiagnosticText)" } else { '' }
        Write-RepositorySnapshot -State $state
        Write-ProgressEvent -Message "Trouvé $($state.Name) · $($state.Branch) · $($state.LocalText) · $($state.SyncText)$diagnosticSuffix"
        Write-ProgressEvent -Message "@progress|$position|$($repositories.Count)"
        $states += $state
    }

    $stopwatch.Stop()
    $summary = [ordered]@{
        Total = $states.Count
        Clean = @($states | Where-Object { $_.StatusKey -eq 'Clean' }).Count
        Modified = @($states | Where-Object { $_.DirtyCount -gt 0 }).Count
        Behind = @($states | Where-Object { $_.Behind -gt 0 }).Count
        Ahead = @($states | Where-Object { $_.Ahead -gt 0 }).Count
        Attention = @($states | Where-Object {
            $_.StatusKey -in @('Diverged', 'Error', 'NoUpstream') -or $_.SuspiciousCount -gt 0
        }).Count
        SuspiciousFiles = ($states | Measure-Object -Property SuspiciousCount -Sum).Sum
        RestoredFiles = ($states | Measure-Object -Property RestoredCount -Sum).Sum
        Updated = @($states | Where-Object { $_.Operation -like 'Mis à jour*' }).Count
        Skipped = @($states | Where-Object { $_.Operation -like 'Ignoré*' }).Count
        Committed = @($states | Where-Object {
            $_.Operation -eq 'Commit & push effectués' -or $_.Operation -like 'Commit effectué · push impossible*'
        }).Count
        Pushed = @($states | Where-Object { $_.Operation -eq 'Commit & push effectués' }).Count
        Failed = @($states | Where-Object { $_.Operation -like '*impossible*' }).Count
    }

    $finishedLabel = if ($states.Count -eq 1) { 'Terminé · 1 dépôt analysé' } else { "Terminé · $($states.Count) dépôts analysés" }
    Write-ProgressEvent -Message $finishedLabel

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
finally {
    if (-not [string]::IsNullOrWhiteSpace($commitMessageFile) -and (Test-Path -LiteralPath $commitMessageFile)) {
        Remove-Item -LiteralPath $commitMessageFile -Force -ErrorAction SilentlyContinue
    }
}
