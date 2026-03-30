# Data Model: RequestSpark Rebrand

This feature does not add runtime persistence entities. The planning data model captures the identity surfaces and validation artifacts that must move together during the rebrand.

## Entity: BrandIdentifier

| Field | Type | Description |
|-------|------|-------------|
| `kind` | enum | Category of first-party identity surface: `solution`, `project`, `namespace`, `assembly`, `package-metadata`, `ui-text`, `console-text`, `documentation`, `file-path`, `folder-path`, `swagger-label` |
| `currentValue` | string | Existing RESTRunner-branded value |
| `targetValue` | string | Required RequestSpark-branded replacement |
| `location` | string | File path or artifact path containing the identifier |
| `publiclyVisible` | boolean | Whether downstream users or operators can observe the identifier directly |
| `requiresMigrationNote` | boolean | Whether the rename must appear in migration guidance |
| `preserveBehavior` | boolean | Always `true` for this feature because identity changes must not alter behavior |

**Validation rules**

- `targetValue` must use the canonical brand form `RequestSpark` where the identifier format allows it.
- A `BrandIdentifier` must not point to third-party package names or generic technical uses of REST.
- If `publiclyVisible` is `true` and the rename changes an external contract, `requiresMigrationNote` must be `true`.

## Entity: RepositoryIdentifier

| Field | Type | Description |
|-------|------|-------------|
| `surface` | enum | `repository-name`, `badge-url`, `release-link`, `folder-name`, `solution-name` |
| `currentValue` | string | Existing repository-facing label or path |
| `targetValue` | string | RequestSpark replacement |
| `renamePhase` | enum | `local-rename`, `documentation-update`, `final-repo-rename` |
| `dependsOn` | string[] | Upstream renames that must land first |

**Validation rules**

- `repository-name` changes must be scheduled in `final-repo-rename`.
- Badge and link updates must point to the final repository identifier only after the repository rename occurs.

## Entity: PublicContract

| Field | Type | Description |
|-------|------|-------------|
| `contractType` | enum | `namespace`, `project-reference`, `assembly-name`, `swagger-title`, `documented-command`, `route`, `output-artifact` |
| `surface` | string | Specific contract value visible to consumers |
| `changeType` | enum | `renamed`, `preserved`, `documented-only` |
| `reason` | string | Why the surface changes or stays stable |
| `migrationReplacement` | string | RequestSpark replacement when `changeType = renamed` |

**Validation rules**

- Contracts with `changeType = renamed` must appear in migration guidance.
- Contracts with `contractType = route` must stay `preserved` for this feature.
- No preserved contract may keep the RESTRunner brand unless it is clearly documented as historical.

## Entity: ValidationArtifact

| Field | Type | Description |
|-------|------|-------------|
| `artifactType` | enum | `build-result`, `test-result`, `asset-build`, `search-report`, `docker-entrypoint-check`, `manual-ui-check` |
| `commandOrCheck` | string | Command or review step used to validate the artifact |
| `successSignal` | string | Observable success condition |
| `appliesWhen` | string | Condition that determines whether this artifact is required |

**Validation rules**

- `build-result` and `test-result` are mandatory for every implementation.
- `asset-build` is required whenever `RESTRunner.Web/package.json` managed files or output names change.
- `search-report` must verify zero unintended uses of `RESTRunner` or `REST Runner`.

## Relationships

- A `RepositoryIdentifier` is a specialized `BrandIdentifier` that affects external source-control visibility.
- A `PublicContract` can reference one or more `BrandIdentifier` records when the rename changes an externally visible surface.
- Every renamed `PublicContract` requires at least one `ValidationArtifact` proving the renamed surface still builds, runs, or renders correctly.

## State Transitions

| Entity | From | To | Trigger |
|--------|------|----|---------|
| `BrandIdentifier` | `legacy` | `renamed` | Identifier updated to RequestSpark |
| `PublicContract` | `undocumented-change` | `documented-change` | Migration guidance records old and new names |
| `RepositoryIdentifier` | `local-ready` | `externally-renamed` | GitHub repository renamed after local merge |
| `ValidationArtifact` | `pending` | `passed` | Command or manual check succeeds |
