# Tasks: RequestSpark Rebrand

**Input**: Design documents from `/.documentation/specs/001-rebrand-requestspark/`
**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/rebrand-contract.md`, `quickstart.md`

**Tests**: No new automated tests are planned for this feature. Validation uses the existing repository workflow: `dotnet clean`, `dotnet build`, `dotnet test`, plus `npm run build` when `RequestSpark.Web/package.json` managed assets change.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Phase 0: Constitution Check (Blocking Governance)

**Purpose**: Verify constitution alignment before structural rename work starts.

**⚠️ CRITICAL**: No setup or implementation work can begin until this phase is complete.

- [ ] T001 Verify constitution alignment for layering, testing impact, logging impact, documentation impact, and input-validation impact in `.documentation/specs/001-rebrand-requestspark/plan.md`, `.documentation/specs/001-rebrand-requestspark/spec.md`, and `.documentation/specs/001-rebrand-requestspark/tasks.md`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the filesystem and solution-level rename baseline used by all later work.

- [ ] T002 Rename `RESTRunner.sln` to `RequestSpark.sln` and update solution project entries inside `RequestSpark.sln`
- [ ] T003 Rename the first-party project folders and project files with `git mv`: `RESTRunner/RESTRunner.csproj`, `RESTRunner.Domain/RESTRunner.Domain.csproj`, `RESTRunner.PostmanImport/RESTRunner.PostmanImport.csproj`, `RESTRunner.Web/RESTRunner.Web.csproj`, `RESTRunner.Domain.Tests/RESTRunner.Domain.Tests.csproj`, and `RESTRunner.Web.Tests/RESTRunner.Web.Tests.csproj`; then issue additional `git mv` commands for individual `.cs` files within the renamed folders whose filenames carry the old brand, specifically `RequestSpark.Domain/Exceptions/RESTRunnerExceptions.cs → RequestSparkExceptions.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Restore the renamed solution graph and shared identifiers before user-story work begins.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 Update project references and solution paths in `RequestSpark.sln`, `RequestSpark/RequestSpark.csproj`, `RequestSpark.Web/RequestSpark.Web.csproj`, `RequestSpark.Domain.Tests/RequestSpark.Domain.Tests.csproj`, and `RequestSpark.Web.Tests/RequestSpark.Web.Tests.csproj`
- [ ] T005 [P] Rename domain namespaces and global usings from `RESTRunner.Domain` to `RequestSpark.Domain` in `RequestSpark.Domain/GlobalUsings.cs`, `RequestSpark.Domain/**/*.cs`, and `RequestSpark.Domain.Tests/**/*.cs`; also rename the `RESTRunnerException` base class identifier (and all subclasses in `RequestSparkExceptions.cs`) to `RequestSparkException`, `RequestSparkValidationException`, `RequestSparkConfigurationException`, and `RequestSparkRequestExecutionException` respectively, and rename the private class `RestRunnerBatchSink` in `RequestSpark.Domain/Services/ExecuteRunnerService.cs` to `RequestSparkBatchSink`
- [ ] T006 [P] Rename console and import namespaces from `RESTRunner*` to `RequestSpark*` in `RequestSpark/GlobalUsings.cs`, `RequestSpark/**/*.cs`, and `RequestSpark.PostmanImport/**/*.cs`
- [ ] T007 [P] Rename web and web-test namespaces from `RESTRunner.Web*` to `RequestSpark.Web*` in `RequestSpark.Web/**/*.cs`, `RequestSpark.Web.Tests/**/*.cs`, and `RequestSpark.Web/Views/_ViewImports.cshtml`
- [ ] T008 Update assembly metadata, description text, and project display names in `RequestSpark.Domain/RequestSpark.Domain.csproj`, `RequestSpark/RequestSpark.csproj`, `RequestSpark.PostmanImport/RequestSpark.PostmanImport.csproj`, `RequestSpark.Web/RequestSpark.Web.csproj`, `RequestSpark.Domain.Tests/RequestSpark.Domain.Tests.csproj`, and `RequestSpark.Web.Tests/RequestSpark.Web.Tests.csproj`

**Checkpoint**: The renamed solution structure compiles at the reference level and all user stories can proceed.

---

## Phase 3: User Story 1 - Present a consistent RequestSpark identity (Priority: P1) 🎯 MVP

**Goal**: Make every first-party entry point, screen, document, and primary artifact present the RequestSpark name.

**Independent Test**: Review the console entry point, the main web UI, Swagger/OpenAPI branding, and the primary README/sample artifacts to confirm RequestSpark appears consistently and RESTRunner does not remain unintentionally.

### Implementation for User Story 1

- [ ] T009 [P] [US1] Update console branding and exported artifact naming in `RequestSpark/Program.cs`
- [ ] T010 [P] [US1] Update shared web layout and home-page branding in `RequestSpark.Web/Views/Shared/_Layout.cshtml` and `RequestSpark.Web/Views/Home/Index.cshtml`
- [ ] T011 [P] [US1] Update Swagger/OpenAPI branding and application-level web text in `RequestSpark.Web/Program.cs`
- [ ] T012 [US1] Update primary product documentation and sample descriptions in `README.md` and `sample-collection.json`
- [ ] T013 [US1] Update remaining active first-party branding surfaces in `RequestSpark.Web/Views/Collection/**`, `RequestSpark.Web/Views/Configuration/**`, `RequestSpark.Web/Views/Execution/**`, `RequestSpark.Web/Views/OpenApi/**`, `RequestSpark.Web/Views/Runner/**`, `.github/copilot-instructions.md`, `.github/agents/copilot-instructions.md`, and `.github/ISSUE_TEMPLATES.md`

**Checkpoint**: User Story 1 is complete when the main product entry points and primary docs all present RequestSpark consistently.

---

## Phase 4: User Story 2 - Make breaking identity changes explicit (Priority: P2)

**Goal**: Document renamed public-facing surfaces clearly while avoiding legacy-brand compatibility wrappers.

**Independent Test**: Review active docs and public code surfaces to verify the retired name and RequestSpark replacement are clearly mapped and no legacy-brand aliases remain in active APIs.

### Implementation for User Story 2

- [ ] T014 [P] [US2] Add a migration section mapping retired and replacement names for solution, projects, namespaces, assemblies, and output artifacts in `README.md`
- [ ] T015 [P] [US2] Update contributor-facing rename guidance in `.github/copilot-instructions.md` and `.github/ISSUE_TEMPLATES.md`
- [ ] T016 [US2] Mark intentionally historical RESTRunner references as former-name context in `.github/upgrades/upgrade-complete-summary.md`, `.github/upgrades/DEPLOYMENT-COMPLETE.md`, `.github/upgrades/assessment.md`, `.github/upgrades/plan.md`, `.github/upgrades/tasks.md`, and any active `.documentation/` pages that retain historical references
- [ ] T017 [US2] Audit renamed public code surfaces and remove any legacy-brand aliases or wrappers from `RequestSpark.Domain/**/*.cs`, `RequestSpark/**/*.cs`, `RequestSpark.PostmanImport/**/*.cs`, and `RequestSpark.Web/**/*.cs`

**Checkpoint**: User Story 2 is complete when downstream users can identify breaking rename replacements quickly and active code exposes no RESTRunner compatibility layer.

---

## Phase 5: User Story 3 - Keep release and validation workflows trustworthy after the rebrand (Priority: P3)

**Goal**: Preserve build, test, packaging, and release workflows under the RequestSpark identity.

**Independent Test**: Execute the standard validation workflow and inspect representative artifacts and release-facing files to confirm the renamed solution still builds, tests, and packages cleanly.

### Implementation for User Story 3

- [ ] T018 [P] [US3] Update runtime and packaging identifiers for the renamed web project in `RequestSpark.Web/Dockerfile` and `RequestSpark.Web/package.json`; the Dockerfile update MUST also change the base image tags from `mcr.microsoft.com/dotnet/aspnet:8.0` and `mcr.microsoft.com/dotnet/sdk:8.0` to their `.NET 10` equivalents (`aspnet:10.0` and `sdk:10.0`) in addition to renaming `RESTRunner.Web.csproj` and `RESTRunner.Web.dll` references
- [ ] T019 [P] [US3] Update build, run, and release command references to renamed paths in `README.md`, `.github/copilot-instructions.md`, `.github/upgrades/upgrade-complete-summary.md`, `.github/upgrades/DEPLOYMENT-COMPLETE.md`, and `.github/upgrades/tasks.md`
- [ ] T020 [US3] Run structural cleanup and full .NET validation against `RequestSpark.sln` with `dotnet clean`, `dotnet build`, and `dotnet test`
- [ ] T021 [US3] Run web asset validation in `RequestSpark.Web/package.json` with `npm run build`; this step is unconditional because T018 always modifies `package.json`
- [ ] T022 [US3] Perform a case-insensitive lingering-brand review across `RequestSpark.sln`, `README.md`, `.documentation/**`, `.github/**`, `RequestSpark/**`, `RequestSpark.Domain/**`, `RequestSpark.PostmanImport/**`, `RequestSpark.Web/**`, `RequestSpark.Domain.Tests/**`, and `RequestSpark.Web.Tests/**` using a search that matches `RESTRunner`, `REST Runner`, and mixed-case variants such as `RestRunner` (PowerShell example: `Get-ChildItem -Recurse | Select-String -Pattern 'restrunner|rest.runner' -CaseSensitive:$false`); then verify any retained `.archive/**` references are explicitly historical and record intentional exceptions with documented justifications in `README.md`

**Checkpoint**: User Story 3 is complete when the renamed solution validates successfully and lingering-brand references are either removed or explicitly justified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Finish cross-story cleanup and prepare the final external repository rename step.

- [ ] T023 [P] Normalize remaining first-party comments, constants, and sample labels in `RequestSpark.Domain/Constants/DomainConstants.cs` (including renaming the `SessionIdPrefix` constant value from `"RESTRunner"` to `"RequestSpark"` and updating any XML doc comments that still reference the old brand), `RequestSpark.Domain/**/*.cs`, and `RequestSpark.Web/**/*.cs`
- [ ] T024 [P] Prepare the post-merge GitHub repository rename checklist and badge/link updates in `README.md`
- [ ] T025 Execute the final manual GitHub repository rename from `RESTRunner` to `RequestSpark` after merge, then reconcile badge, release, and repository URLs in `README.md` and any active `.github/` documentation that still references the former repository path
- [ ] T026 Run the manual verification flow from `.documentation/specs/001-rebrand-requestspark/quickstart.md` and fix any last branding mismatches in affected files
- [ ] T027 [P] Update `.documentation/memory/constitution.md` to replace all first-party project name references (`RESTRunner.Domain`, `RESTRunner.Web`, `RESTRunner.PostmanImport`, `RESTRunner`, `RESTRunner.Domain.Tests`, `RESTRunner.Web.Tests`) with their `RequestSpark.*` equivalents; also update the file-backed storage path note from `RESTRunner.Web/Data` to `RequestSpark.Web/Data`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Constitution Check (Phase 0)**: No dependencies; blocks all other work until complete.
- **Setup (Phase 1)**: Depends on Phase 0.
- **Foundational (Phase 2)**: Depends on Phase 1 and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Phase 2; delivers the MVP.
- **User Story 2 (Phase 4)**: Depends on Phase 2; can run after or alongside User Story 1 once renamed paths are stable.
- **User Story 3 (Phase 5)**: Depends on Phase 2 and should finish after the branding/file changes it validates are in place.
- **Polish (Phase 6)**: Depends on the desired user stories being complete; T027 (constitution update) may begin as soon as Phase 2 foundational renames are stable.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational; no dependency on other stories.
- **User Story 2 (P2)**: Starts after Foundational; references outputs from User Story 1 but remains independently testable through documentation and API-surface review.
- **User Story 3 (P3)**: Starts after Foundational; validates the renamed outputs produced by User Stories 1 and 2.

### Within Each User Story

- Rename structure and namespaces before editing branding copy in story-specific files.
- Update operator/UI/docs surfaces before running validation commands.
- Complete the story checkpoint before declaring the story done.

### Parallel Opportunities

- T005, T006, and T007 can run in parallel once Phase 1 renames complete.
- T009, T010, and T011 can run in parallel because they touch separate console, web-view, and web-startup files.
- T014 and T015 can run in parallel because they update different active documentation files.
- T018 and T019 can run in parallel before final validation.
- T023, T024, and T027 can run in parallel during polish.

---

## Parallel Example: User Story 1

```text
T009 [US1] Update console branding and exported artifact naming in RequestSpark/Program.cs
T010 [US1] Update shared web layout and home-page branding in RequestSpark.Web/Views/Shared/_Layout.cshtml and RequestSpark.Web/Views/Home/Index.cshtml
T011 [US1] Update Swagger/OpenAPI branding and application-level web text in RequestSpark.Web/Program.cs
```

## Parallel Example: User Story 2

```text
T014 [US2] Add a migration section mapping retired and replacement names in README.md
T015 [US2] Update contributor-facing rename guidance in .github/copilot-instructions.md and .github/ISSUE_TEMPLATES.md
```

## Parallel Example: User Story 3

```text
T018 [US3] Update runtime and packaging identifiers in RequestSpark.Web/Dockerfile and RequestSpark.Web/package.json
T019 [US3] Update build, run, and release command references in README.md and .github/upgrades/*.md
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 0: Constitution Check.
2. Complete Phase 1: Setup.
3. Complete Phase 2: Foundational.
4. Complete Phase 3: User Story 1.
5. Validate User Story 1 independently by reviewing console, web UI, Swagger, and README branding.

### Incremental Delivery

1. Finish Constitution Check, Setup, and Foundational to stabilize renamed paths and namespaces.
2. Deliver User Story 1 for visible product-brand consistency.
3. Deliver User Story 2 to document breaking rename impacts and confirm no compatibility aliases remain.
4. Deliver User Story 3 to validate build, test, packaging, and lingering-reference checks.
5. Finish with Phase 6 polish, then execute the final manual repository rename step.

### Parallel Team Strategy

1. One engineer handles the Phase 0 governance gate plus solution/project rename mechanics in Phases 1 and 2.
2. After Phase 2, separate engineers can take story-specific work: UI/docs branding, migration guidance, and validation/release updates.
3. Rejoin for final validation, lingering-reference review, and the manual repository-rename step.

---

## Notes

- `[P]` means the task can proceed in parallel because it targets different files with no incomplete-task dependency.
- Story labels map every story-phase task back to its corresponding user story.
- The GitHub repository rename from `RESTRunner` to `RequestSpark` remains a manual post-merge step, but it is now an explicit completion task in this task set.