# Implementation Plan: RequestSpark Rebrand

**Branch**: `001-rebrand-requestspark` | **Date**: 2026-03-30 | **Spec**: `C:\GitHub\MarkHazleton\RESTRunner\.documentation\specs\001-rebrand-requestspark\spec.md`
**Input**: Feature specification from `C:\GitHub\MarkHazleton\RESTRunner\.documentation\specs\001-rebrand-requestspark\spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.documentation/templates/commands/plan.md` for the execution workflow.

## Summary

Rebrand the full first-party RESTRunner solution to RequestSpark with a clean, in-place rename that updates project names, namespaces, assembly and package metadata, UI and console branding, solution and folder identifiers, primary documentation, and the engineering constitution while preserving existing runtime behavior. The implementation will sequence renames from foundational projects outward, keep web route and port contracts unchanged, document breaking public-name changes explicitly, include a blocking constitution-alignment gate in task execution, and validate the result with the repository's standard `dotnet build` and `dotnet test` workflow plus `npm run build` when web asset inputs are touched. Completion also includes a final manual GitHub repository rename and a repository-wide lingering-reference review covering active first-party assets plus historical labeling checks for `.archive/`.

## Technical Context

**Language/Version**: C# on .NET 10.0; npm-managed web assets; PowerShell automation  
**Primary Dependencies**: ASP.NET Core MVC and minimal APIs, MSTest v4, Swashbuckle.AspNetCore, Newtonsoft.Json, WebSpark.Bootswatch, WebSpark.HttpClientUtility  
**Storage**: File-backed JSON under `RESTRunner.Web/Data` plus CSV export artifacts from the console runner  
**Testing**: `dotnet test` across MSTest projects; `npm run build` for `RESTRunner.Web` is unconditionally required because T018 always modifies `package.json`; lingering-reference search must use case-insensitive matching (e.g., PowerShell `-match` or `grep -i`) to catch `RestRunner` and similar mixed-case variants  
**Target Platform**: Cross-platform .NET console and ASP.NET Core web app; local development currently assumes Windows paths in some console output  
**Project Type**: Multi-project .NET solution with domain library, console app, web app, import library, and dedicated test projects  
**Performance Goals**: Preserve current execution behavior and throughput; the rebrand must not materially change request execution, routes, or validation workflows  
**Constraints**: Clean rename only; no backward-compatibility aliases; preserve route patterns and ports; preserve Git history via in-place `git mv`; perform GitHub repository rename as the last separate step; do not rename external dependencies or technical uses of REST; review active first-party assets in the solution root, `.github/`, and `.documentation/`, while keeping `.archive/` historical rather than fully renamed; individual `.cs` files whose filenames carry the old brand (e.g., `RESTRunnerExceptions.cs`) require a separate `git mv` beyond the enclosing project-folder rename; the Dockerfile base images must be updated to .NET 10 (currently reference `mcr.microsoft.com/dotnet/aspnet:8.0` and `sdk:8.0`) as part of updating Docker packaging identifiers; lingering-reference audit must be case-insensitive to catch mixed-case variants such as `RestRunner`  
**Scale/Scope**: Solution-wide rename across 6 projects, shared docs, solution metadata, Docker/npm assets, and first-party branding surfaces; expected to touch dozens of files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Layered Domain-First Architecture**: PASS. The feature is identity-only and will not move business logic across layers. Plan work maps onto existing projects: domain, console, web, import, and tests.
- **Project-Wide C# Conventions**: PASS. Renames will preserve `.NET 10`, nullable, implicit usings, and project-level global usings. Namespace updates will remain aligned with project and folder boundaries.
- **MSTest Quality Gates**: PASS. The plan requires `dotnet build` and `dotnet test` at completion. Existing MSTest projects remain the validation mechanism because runtime behavior is unchanged.
- **Contextual Logging And Public Documentation**: PASS. Logging behavior is unchanged. Public documentation and externally visible labels will be updated as part of the rebrand, including Swagger/OpenAPI titles and primary README messaging.
- **Boundary Validation And Maintainability**: PASS. Input validation behavior is unchanged. Route contracts and request-size protections remain intact; no new boundary surface is introduced.
- **Delivery Workflow**: PASS. The plan includes constitution checks, solution-structure-aware work, and `npm run build` when `RESTRunner.Web/package.json` managed assets are modified.

**Tasking Implication**: The generated task set must open with an explicit constitution-alignment gate, must include a manual completion task for the final GitHub repository rename, and must include a lingering-reference audit across active first-party assets plus historical-label checks for `.archive/`. The task set must also include: (a) individual `git mv` for brand-named `.cs` files within renamed project folders; (b) an explicit fix for the Dockerfile .NET 8 base-image references; (c) explicit renaming of the `RESTRunnerException` base class identifier and the `SessionIdPrefix` constant; (d) an update to `.documentation/memory/constitution.md` to replace `RESTRunner.*` project references with `RequestSpark.*`; and (e) unconditional execution of `npm run build` since `package.json` is always touched.

**Post-Design Re-check**: PASS. Research and design artifacts keep the rename scoped to first-party identity surfaces, preserve layer boundaries and behavior, and retain existing validation and documentation obligations.

## Project Structure

### Documentation (this feature)

```text
.documentation/specs/001-rebrand-requestspark/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── rebrand-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
RESTRunner.sln
README.md
global.json
sample-collection.json
.documentation/
.github/
.archive/

RESTRunner/
├── Program.cs
├── GlobalUsings.cs
├── Infrastructure/
└── Extensions/

RESTRunner.Domain/
├── Models/
├── Interfaces/
├── Services/
├── Extensions/
├── Outputs/
├── Constants/
└── Exceptions/

RESTRunner.PostmanImport/
├── PostmanImport.cs
└── Models/

RESTRunner.Web/
├── Program.cs
├── Controllers/
├── Services/
├── Models/
├── Views/
├── wwwroot/
├── Data/
├── SampleCRUD/
├── Dockerfile
└── package.json

RESTRunner.Domain.Tests/
├── Models/
├── Outputs/
├── Extensions/
└── Services/

RESTRunner.Web.Tests/
├── Controllers/
└── Services/
```

**Structure Decision**: Use the existing multi-project solution structure. The implementation will rename first-party projects, folders, and namespaces in place rather than introducing new packages or reshaping the architecture. Active implementation work spans the solution projects, `.github/`, and `.documentation/`; `.archive/` remains historical-review-only.

## Complexity Tracking

No constitution violations are expected for this feature.

