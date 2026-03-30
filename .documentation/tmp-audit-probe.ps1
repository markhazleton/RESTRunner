$ErrorActionPreference = 'Stop'

function Get-FirstSpecVersion {
    $changelog = Join-Path $PWD 'CHANGELOG.md'
    $pyproject = Join-Path $PWD 'pyproject.toml'
    if (Test-Path $changelog) {
        $m = Select-String -Path $changelog -Pattern '^##\s*\[(\d+\.\d+\.\d+)\]' | Select-Object -First 1
        if ($m) { return $m.Matches[0].Groups[1].Value }
    }
    if (Test-Path $pyproject) {
        $m = Select-String -Path $pyproject -Pattern '^version\s*=\s*"([^"]+)"' | Select-Object -First 1
        if ($m) { return $m.Matches[0].Groups[1].Value }
    }
    return 'unknown'
}

$latestSpecKit = Get-FirstSpecVersion

$speckitFields = [ordered]@{}
$speckitFile = '.documentation/SPECKIT_VERSION'
if (Test-Path $speckitFile) {
    $lines = Get-Content $speckitFile
    if ($lines.Count -gt 0 -and $lines[0].Trim()) { $speckitFields.version = $lines[0].Trim() }
    foreach ($line in ($lines | Select-Object -Skip 1)) {
        if ($line -match '^\s*([^:]+):\s*(.+)\s*$') {
            $k = $matches[1].Trim(); $v = $matches[2].Trim(); $speckitFields[$k] = $v
        }
    }
}

$githubFiles = Get-ChildItem '.github' -Recurse -File -ErrorAction SilentlyContinue
$ver3Matches = @()
foreach ($f in $githubFiles) {
    $hits = Select-String -Path $f.FullName -Pattern '\.documentation/|(^|[^A-Za-z0-9_])memory/' -AllMatches -ErrorAction SilentlyContinue
    if ($hits) {
        foreach ($h in $hits) {
            $ver3Matches += [pscustomobject]@{ file = ($f.FullName.Substring($PWD.Path.Length + 1) -replace '\\','/'); line = $h.LineNumber; text = $h.Line.Trim() }
        }
    }
}

$rootDirs = Get-ChildItem -Directory -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name
$ver4 = [ordered]@{ memory = ($rootDirs -contains 'memory'); scripts = ($rootDirs -contains 'scripts'); templates = ($rootDirs -contains 'templates') }

$ver5Files = Get-ChildItem '.github' -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^speckit\..*-old\.md$' } | ForEach-Object { $_.FullName.Substring($PWD.Path.Length + 1).Replace('\\','/') }

$audit = (& ./.documentation/scripts/powershell/site-audit.ps1 -Json) | ConvertFrom-Json

$fileCounts = [ordered]@{}
if ($audit.files.counts) { $fileCounts = $audit.files.counts }

$todoHits = Select-String -Path (Get-ChildItem -Recurse -File -Include *.cs,*.ts,*.js,*.md,*.ps1 -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|\.git|\.vs)\\' } | Select-Object -ExpandProperty FullName) -Pattern 'TODO|FIXME|HACK' -CaseSensitive:$false -ErrorAction SilentlyContinue |
    Select-Object -First 15 |
    ForEach-Object { [pscustomobject]@{ file = ($_.Path.Substring($PWD.Path.Length + 1) -replace '\\','/'); line = $_.LineNumber; text = $_.Line.Trim() } }

$largeFiles = Get-ChildItem -Recurse -File -Include *.cs,*.ts,*.js -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|\.git|\.vs)\\' } |
    ForEach-Object {
        $lineCount = (Get-Content $_.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines
        if ($lineCount -gt 500) {
            [pscustomobject]@{ file = ($_.FullName.Substring($PWD.Path.Length + 1) -replace '\\','/'); lines = $lineCount }
        }
    } |
    Sort-Object lines -Descending |
    Select-Object -First 15

$testFiles = Get-ChildItem -Recurse -File -Filter *Tests.cs -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
$testContents = @{}
foreach ($tf in $testFiles) { $testContents[$tf.FullName] = Get-Content $tf.FullName -Raw -ErrorAction SilentlyContinue }

$sourceFiles = @(
    Get-ChildItem RequestSpark.Web -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue
    Get-ChildItem RequestSpark -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue
) | Where-Object {
    $_.FullName -notmatch '\\(bin|obj)\\' -and
    $_.Name -notmatch 'Tests?\.cs$' -and
    $_.DirectoryName -notmatch 'Tests'
}

$untested = foreach ($sf in $sourceFiles) {
    $raw = Get-Content $sf.FullName -Raw -ErrorAction SilentlyContinue
    $classes = [regex]::Matches($raw, '\b(class|record)\s+(\w+)') | ForEach-Object { $_.Groups[2].Value } | Select-Object -Unique
    if (-not $classes) { continue }
    $matchedTests = @()
    foreach ($tc in $testContents.GetEnumerator()) {
        foreach ($cn in $classes) {
            if ($tc.Value -match "\\b$([regex]::Escape($cn))\\b") { $matchedTests += $tc.Key; break }
        }
    }
    $lineCount = (Get-Content $sf.FullName | Measure-Object -Line).Lines
    if (($matchedTests | Select-Object -Unique).Count -eq 0) {
        [pscustomobject]@{ file = ($sf.FullName.Substring($PWD.Path.Length + 1) -replace '\\','/'); lines = $lineCount; classNames = $classes }
    }
}
$untestedTop = $untested | Sort-Object lines -Descending | Select-Object -First 20

$xmlTargets = @('RequestSpark.Web/Controllers','RequestSpark.Web/Hubs','RequestSpark.Web/Models')
$xmlSignals = @()
foreach ($dir in $xmlTargets) {
    if (-not (Test-Path $dir)) { continue }
    $files = Get-ChildItem $dir -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $lines = Get-Content $file.FullName
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $trim = $lines[$i].Trim()
            $isPublicType = $trim -match '^public\s+(class|record|interface|enum)\s+\w+'
            $isPublicMethod = $trim -match '^public\s+.*\w+\s*\([^;]*\)\s*(\{|=>)?\s*$' -and $trim -notmatch '^public\s+(if|for|foreach|while)\b'
            if ($isPublicType -or $isPublicMethod) {
                $j = $i - 1
                while ($j -ge 0 -and [string]::IsNullOrWhiteSpace($lines[$j])) { $j-- }
                $hasXml = $false
                if ($j -ge 0 -and $lines[$j].Trim().StartsWith('///')) { $hasXml = $true }
                if (-not $hasXml) {
                    $xmlSignals += [pscustomobject]@{ file = ($file.FullName.Substring($PWD.Path.Length + 1) -replace '\\','/'); line = ($i+1); declaration = $trim }
                }
            }
        }
    }
}
$xmlTop = $xmlSignals | Select-Object -First 20

$validationSignals = @()
$programPath = 'RequestSpark.Web/Program.cs'
if (Test-Path $programPath) {
    $lines = Get-Content $programPath
    for ($i=0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match 'Map(Get|Post|Put|Delete)\s*\(') {
            $method = $matches[1].ToUpper()
            $chunk = $lines[$i]
            $k = $i+1
            while ($k -lt $lines.Count -and $chunk -notmatch ';\s*$' -and ($k - $i) -lt 30) { $chunk += "`n" + $lines[$k]; $k++ }
            $route = $null
            $rm = [regex]::Match($chunk, 'Map(?:Get|Post|Put|Delete)\s*\(\s*"([^"]+)"')
            if ($rm.Success) { $route = $rm.Groups[1].Value }
            $hasFromBody = $chunk -match '\[FromBody\]'
            $hasTypedModel = $chunk -match '\(([^)]*\b[A-Z]\w+\s+\w+[^)]*)\)'
            $hasDataAnnotations = $chunk -match '\[(Required|Range|StringLength|MinLength|MaxLength|RegularExpression|EmailAddress)\]'
            $hasManualChecks = $chunk -match 'if\s*\(.*(==\s*null|IsNullOrWhiteSpace|IsNullOrEmpty|\.Any\(|\.Count\s*==\s*0|BadRequest|ValidationProblem)'
            $candidate = (($method -in @('POST','PUT')) -and -not ($hasFromBody -or $hasTypedModel -or $hasDataAnnotations -or $hasManualChecks))
            $validationSignals += [pscustomobject]@{
                method = $method; line = ($i+1); route = $route; hasFromBody = $hasFromBody; hasTypedModel = $hasTypedModel; hasDataAnnotations = $hasDataAnnotations; hasManualChecks = $hasManualChecks; candidateLackingValidation = $candidate
            }
        }
    }
}

$result = [ordered]@{
    versionCheck = [ordered]@{
        latestSpecKit = $latestSpecKit
        speckitVersionFile = $speckitFields
    }
    scanCounts = [ordered]@{
        siteAudit = [ordered]@{
            fileCounts = $fileCounts
            hardcodedSecrets = @($audit.patterns.security.hardcoded_secrets).Count
            insecurePatterns = @($audit.patterns.security.insecure_patterns).Count
            todoComments = @($audit.patterns.quality.todo_comments).Count
            largeFilesCount = @($audit.metrics.large_files).Count
            largeFilesEntries = $audit.metrics.large_files
        }
    }
    patternFindings = [ordered]@{
        ver3 = [ordered]@{ count = $ver3Matches.Count; matches = ($ver3Matches | Select-Object -First 30) }
        ver4 = $ver4
        ver5 = [ordered]@{ count = @($ver5Files).Count; files = $ver5Files }
    }
    todoTop = $todoHits
    largeFiles = $largeFiles
    untestedSignals = [ordered]@{ count = @($untestedTop).Count; files = $untestedTop }
    xmlDocSignals = [ordered]@{ count = @($xmlTop).Count; entries = $xmlTop }
    validationSignals = [ordered]@{ count = @($validationSignals).Count; endpoints = $validationSignals; candidates = @($validationSignals | Where-Object { $_.candidateLackingValidation }) }
}

$result | ConvertTo-Json -Depth 8 -Compress

