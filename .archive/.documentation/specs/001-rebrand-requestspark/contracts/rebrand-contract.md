# RequestSpark Rebrand Contract

## Purpose

Define the externally relevant contracts for the RequestSpark rebrand so implementation can make deliberate breaking identity changes without altering runtime behavior.

## Canonical Brand Contract

- Active product name: `RequestSpark`
- Permitted first-party spelling: `RequestSpark` only
- Disallowed first-party variants: `Request Spark`, `requestSpark`, `request-spark`, `RequestSpark`, `RequestSpark`

## Rename Contract

The following first-party contract categories are intentionally renamed as part of the feature:

| Category | Current Form | Target Form | Consumer Impact |
|----------|--------------|-------------|-----------------|
| Solution and project identifiers | `RequestSpark*` | `RequestSpark*` | Build scripts, IDE solution references, and path-based tooling must update |
| Root namespaces | `RequestSpark.*` | `RequestSpark.*` | Source consumers must update using/import statements |
| Assembly and package metadata | RequestSpark-branded descriptions and names | RequestSpark-branded descriptions and names | Distributed artifacts and generated metadata change |
| Console output and generated artifact labels | RequestSpark branding and `RequestSpark.csv` | RequestSpark branding and `RequestSpark.csv` | Operator-visible output names change |
| Web UI and Swagger titles | RequestSpark branding | RequestSpark branding | Browser-visible labels and OpenAPI branding change |
| Repository-facing docs and badges | RequestSpark repo and product labels | RequestSpark repo and product labels | Links and badges change after repository rename |

## Preserved Contract

The following contracts must remain stable during the rebrand:

| Category | Contract | Why It Stays Stable |
|----------|----------|---------------------|
| Web routes | Existing MVC and minimal API routes, including `/api/employee`, `/api/department`, `/api/status`, `/docs`, `/Configuration`, `/Execution`, and related controller paths | These are behavior contracts, not brand identifiers |
| Port assignments | Current local hosting ports such as `https://localhost:7001` | The spec explicitly keeps route and port behavior unchanged |
| Third-party identifiers | Package names like `WebSpark.Bootswatch`, `WebSpark.HttpClientUtility`, `Newtonsoft.Json`, `Swashbuckle.AspNetCore` | They are external dependencies, not first-party branding |
| Technical REST terminology | Technical references to REST when describing APIs or protocol style | They describe the domain, not the product brand |

## Migration Documentation Contract

- Every renamed public-facing contract must record the retired name and the RequestSpark replacement in implementation-facing documentation or release notes.
- No compatibility aliases, wrapper namespaces, or obsolete fallback APIs may be introduced solely to preserve the RequestSpark brand.
- Historical references to RequestSpark may remain only when explicitly labeled as migration or history context.

## Validation Contract

Implementation is complete only when all of the following are true:

1. `dotnet build` succeeds for the renamed solution.
2. `dotnet test` succeeds for the renamed solution.
3. `npm run build` succeeds if `RequestSpark.Web/package.json` managed assets or output names changed.
4. A repository-wide search finds no unintended `RequestSpark` or `RequestSpark` references.
5. Representative console, web UI, Swagger, and documentation surfaces render RequestSpark branding.

