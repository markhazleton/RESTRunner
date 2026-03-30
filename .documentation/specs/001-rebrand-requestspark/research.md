# Phase 0 Research: RequestSpark Rebrand

## Decision 1: Rebrand all first-party identity surfaces, but preserve technical and external contracts

- **Decision**: Rename first-party project names, namespaces, solution identifiers, assembly and package metadata, UI and console branding, docs, file and folder names, and repository-facing labels from RESTRunner to RequestSpark. Do not rename external dependency identifiers, generic technical uses of REST, or stable web route and port contracts.
- **Rationale**: The spec requires a clean identity rename across first-party assets while preserving business behavior. The repository contains many first-party branding surfaces such as solution and project names, console output, Razor views, Swagger titles, and README badges. At the same time, route patterns like `/api/employee`, `/api/department`, `/api/status`, MVC controller paths, and package names such as `WebSpark.Bootswatch` are behavior or external contracts rather than product-brand surfaces.
- **Alternatives considered**:
  - Rename every occurrence of `REST` or `RESTRunner` indiscriminately. Rejected because it would incorrectly modify external dependencies and technical terminology.
  - Leave project and folder names unchanged while only updating UI text. Rejected because FR-002 and FR-007 require first-party identifier and path renames.

## Decision 2: Perform the rename in place and sequence changes from foundational projects outward

- **Decision**: Execute the rebrand as an in-place rename using `git mv`, starting with `RESTRunner.Domain`, then the console app, import library, web app, test projects, and finally the solution file and repository name.
- **Rationale**: `RESTRunner.Domain` is the root dependency for the rest of the solution, so renaming it first exposes broken references early and reduces ambiguity. Updating dependent `ProjectReference` paths immediately after each rename keeps the solution recoverable at each checkpoint. The GitHub repository rename must remain a last separate step so local code, docs, and automation can be updated coherently before external URLs redirect.
- **Alternatives considered**:
  - Rename the solution file first. Rejected because project path changes would still cascade later and make the solution unstable mid-flight.
  - Create a fresh RequestSpark repository. Rejected because the user chose to preserve Git history and perform a clean in-place rename.

## Decision 3: Treat documentation and migration guidance as the compatibility layer

- **Decision**: Document renamed public-facing surfaces explicitly in planning and implementation artifacts, but do not add wrapper APIs, aliases, or compatibility namespaces carrying the retired brand.
- **Rationale**: The spec allows breaking name changes and requires migration guidance rather than code-based compatibility. This keeps the runtime surface clean and avoids preserving RESTRunner as an active API concept while still making migration impact reviewable for maintainers and downstream consumers.
- **Alternatives considered**:
  - Introduce obsolete type aliases or compatibility classes. Rejected because FR-006 forbids compatibility shims whose only purpose is to preserve the old brand.
  - Omit explicit migration guidance and rely on search/replace by consumers. Rejected because FR-005 requires clear documentation of renamed public-facing surfaces.

## Decision 4: Validate the rebrand with the existing repository workflow plus a lingering-reference review

- **Decision**: Validate completion with `dotnet clean`, `dotnet build`, `dotnet test`, and `npm run build` when `RESTRunner.Web` asset-pipeline inputs change, followed by a repository-wide search for unintended `RESTRunner` and `REST Runner` references.
- **Rationale**: The constitution mandates build and test validation with MSTest. The repository also includes an npm-managed web asset pipeline, so asset builds must be included if `package.json`-managed inputs or output names change. A final text search is required to prove the clean rename and identify any intentionally preserved historical references.
- **Alternatives considered**:
  - Only run `dotnet build`. Rejected because the constitution explicitly requires `dotnet test`.
  - Skip the search step and rely on tests. Rejected because tests do not prove branding consistency or the absence of stale strings in docs and metadata.
