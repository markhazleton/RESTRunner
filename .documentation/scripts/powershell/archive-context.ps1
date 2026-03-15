#!/usr/bin/env pwsh
# Archive context gathering script
# Scans .documentation/ for archive candidates (never reads .archive/)
# Outputs inventory for the AI to review and act on

param(
    [Parameter(Position = 0, ValueFromRemainingArguments)]
    [string[]]$Arguments,
    [switch]$Json
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot    = Get-RepoRoot
$docDir      = Join-Path $repoRoot '.documentation'
$archiveBase = Join-Path $repoRoot '.archive'
$today       = (Get-Date).ToString('yyyy-MM-dd')
$archiveDir  = ".archive/$today"
$timestamp   = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$guidePath   = '.documentation/Guide.md'
$changelogPath = 'CHANGELOG.md'

# Helper: list .md files under a dir, relative to repo root, never crossing .archive
function Get-RelativeMdFiles {
    param([string]$SubPath)
    $full = Join-Path $repoRoot $SubPath
    if (-not (Test-Path $full)) { return @() }
    Get-ChildItem -Path $full -Recurse -Filter '*.md' -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch [regex]::Escape('.archive') } |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1).Replace('\', '/') } |
        Sort-Object
}

# Candidate categories
$drafts           = Get-RelativeMdFiles '.documentation/drafts'
$sessionDocs      = Get-RelativeMdFiles '.documentation/copilot'
$implPlans        = @()
if (Test-Path $docDir) {
    $implPlans = Get-ChildItem -Path $docDir -MaxDepth 1 -Filter '*-implementation-plan.md' -ErrorAction SilentlyContinue |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1).Replace('\', '/') }
    $implPlans += Get-ChildItem -Path $docDir -MaxDepth 1 -Filter '*-plan.md' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notmatch '^Guide' } |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1).Replace('\', '/') }
}
$releaseDocs      = Get-RelativeMdFiles '.documentation/releases'
$quickfixRecords  = Get-RelativeMdFiles '.documentation/quickfixes'
$prReviews        = Get-RelativeMdFiles '.documentation/specs/pr-review'

# Current top-level .documentation/*.md (exclude already-caught plan files)
$currentDocs = @()
if (Test-Path $docDir) {
    $currentDocs = Get-ChildItem -Path $docDir -MaxDepth 1 -Filter '*.md' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notmatch '-implementation-plan\.md$' -and $_.Name -notmatch '-plan\.md$' } |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1).Replace('\', '/') } |
        Sort-Object
}

# Existing archive folders
$archiveExists     = Test-Path $archiveBase
$existingArchives  = @()
if ($archiveExists) {
    $existingArchives = Get-ChildItem -Path $archiveBase -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1).Replace('\', '/') } |
        Sort-Object
}

$guideExists     = Test-Path (Join-Path $repoRoot $guidePath)
$changelogExists = Test-Path (Join-Path $repoRoot $changelogPath)

if ($Json) {
    @{
        REPO_ROOT          = $repoRoot
        TIMESTAMP          = $timestamp
        ARCHIVE_DIR        = $archiveDir
        ARCHIVE_EXISTS     = $archiveExists
        EXISTING_ARCHIVES  = $existingArchives
        GUIDE_PATH         = $guidePath
        GUIDE_EXISTS       = $guideExists
        CHANGELOG_PATH     = $changelogPath
        CHANGELOG_EXISTS   = $changelogExists
        CANDIDATES         = @{
            drafts               = $drafts
            session_docs         = $sessionDocs
            implementation_plans = $implPlans
            release_docs         = $releaseDocs
            quickfix_records     = $quickfixRecords
            pr_reviews           = $prReviews
        }
        CURRENT_DOCS       = $currentDocs
    } | ConvertTo-Json -Depth 5 -Compress
}
else {
    Write-Output "Archive Context"
    Write-Output "==============="
    Write-Output "Repository:    $repoRoot"
    Write-Output "Archive dir:   $archiveDir (exists: $archiveExists)"
    Write-Output "Guide.md:      $guidePath (exists: $guideExists)"
    Write-Output "CHANGELOG.md:  $changelogPath (exists: $changelogExists)"
    Write-Output ""
    Write-Output "Candidates:"
    Write-Output "  Drafts:               $($drafts.Count)"
    Write-Output "  Session docs:         $($sessionDocs.Count)"
    Write-Output "  Implementation plans: $($implPlans.Count)"
    Write-Output "  Release docs:         $($releaseDocs.Count)"
    Write-Output "  Quickfix records:     $($quickfixRecords.Count)"
    Write-Output "  PR reviews:           $($prReviews.Count)"
}
