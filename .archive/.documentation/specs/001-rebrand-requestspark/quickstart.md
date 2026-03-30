# Quickstart: Validate the RequestSpark Rebrand

## Preconditions

- Work from the feature branch `001-rebrand-requestspark`.
- Use in-place renames with `git mv` for first-party files and folders.
- Leave the GitHub repository rename until all local renames and documentation updates are complete.

## Implementation Sequence

1. Rename first-party projects and folders in dependency order: domain, console app, import library, web app, test projects, then solution file.
2. Update namespaces, global usings, project references, Docker entrypoint names, npm package metadata, and first-party branding text.
3. Update README, badges, sample artifacts, and migration guidance to reflect RequestSpark.
4. Confirm web routes, route patterns, and port assignments remain unchanged.

## Validation Commands

Run from the repository root after structural renames are complete:

```powershell
dotnet clean
dotnet build
dotnet test
```

If the web asset pipeline or Dockerfile was touched (T018 always touches `package.json`), also run:

```powershell
Set-Location RequestSpark.Web
npm run build
Set-Location ..
```

## Review Checklist

1. Launch the console app and verify startup text, completion text, statistics headings, and exported CSV naming use RequestSpark.
2. Launch the web app and verify layout branding, home page copy, Swagger title, and related UI labels use RequestSpark.
3. Search the repository for unexpected brand variants (spacing, hyphenation, or mixed case) using a case-insensitive search and confirm any remaining matches are intentional historical references only.
   ```powershell
   Get-ChildItem -Recurse -File | Select-String -Pattern 'requestspark|request\s*spark|request-spark' -CaseSensitive:$false
   ```
4. Verify project references, Docker entrypoint names, and README badges align with renamed project and repository identifiers.

## Final External Step

After local code, documentation, and validation are complete and merged, ensure the GitHub repository is named `RequestSpark`, then update any remaining badge or release URLs that depend on the final repository name.

