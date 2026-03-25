<!--
Sync Impact Report
- Version change: 1.0.0 -> 1.1.0
- Modified principles:
  - None
- Added sections:
  - None
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ .documentation/templates/plan-template.md
  - ✅ .documentation/templates/spec-template.md
  - ✅ .documentation/templates/tasks-template.md
  - ✅ README.md
  - ✅ .github/copilot-instructions.md
  - ✅ No command templates present under .documentation/templates/commands/
- Follow-up TODOs:
  - Replace the remaining executable-source password defaults with configuration-backed or clearly non-secret placeholder values
  - Add lightweight automation that flags hardcoded secrets in executable code paths
-->
# RESTRunner Constitution

## Core Principles

### I. Layered Domain-First Architecture
RESTRunner MUST preserve the existing domain-first layering of the solution.
Domain models and contracts belong in RESTRunner.Domain, execution behavior belongs
behind service interfaces, and console or web entry points MUST compose services
rather than absorb business logic. This keeps CompareRunner behavior reusable
across the console runner, web execution flows, and import paths.

- Domain models and interfaces MUST remain free of UI and transport concerns.
- Execution behavior MUST be exposed through DI-friendly service boundaries.
- New features MUST fit the existing project layering unless the plan documents a
	justified exception.

### II. Project-Wide C# Conventions
RESTRunner MUST keep the current C# defaults consistent across projects so the
solution behaves as one codebase rather than a collection of unrelated styles.
Projects MUST target the active solution framework, MUST enable nullable reference
types, and MUST enable implicit usings. Shared imports that are broadly used
within a project SHOULD be added to GlobalUsings.cs instead of repeated locally.

- Shared project-wide imports SHOULD live in GlobalUsings.cs.
- Namespace and folder structure SHOULD remain aligned with project boundaries.
- New cross-project types SHOULD be introduced in the owning project and then
	surfaced through that project's standard import conventions.

### III. MSTest Quality Gates
MSTest is the standard automated test framework for this repository, and quality
gates MUST include both build and test validation. New domain logic and
regression-prone behavior MUST include automated coverage. This rule is strict
because the repository already relies on MSTest conventions in the dedicated test
project and the public documentation already implies automated validation.

- Automated tests MUST use MSTest unless the constitution is amended.
- Domain and regression-prone changes MUST add or update automated tests.
- Changes MUST be validated with dotnet build and dotnet test before completion.
- Repository automation MUST enforce build and test checks once CI is added.

### IV. Contextual Logging And Public Documentation
Operational code MUST use the logging and documentation style that matches its
runtime context. Web and service layers MUST use ILogger with structured message
templates so failures and execution progress remain diagnosable. Console entry
points MAY use Console.WriteLine for operator-facing summaries. Public-facing
controllers, hubs, endpoints, and externally consumed models MUST include XML
documentation when practical so their contract is discoverable without code
inspection.

- Hosted services and controllers MUST log exceptions with relevant context.
- Web and service code MUST prefer structured ILogger messages.
- Public-facing surfaces MUST keep XML documentation current when behavior
	changes.

### V. Boundary Validation And Maintainability
External input SHOULD be validated at the application boundary before deeper
processing, using ASP.NET Core validation features, DataAnnotations,
antiforgery protection, request-size limits, or an approved equivalent. Large,
multi-responsibility files SHOULD be reduced over time when touched, because the
web project already contains several oversized files that are harder to review
and evolve safely. These are SHOULD-level rules because the current repository
shows partial adoption rather than universal enforcement.

- Public input SHOULD define validation, failure behavior, and size or format
	constraints when applicable.
- Unsafe uploads and malformed requests SHOULD fail fast with actionable errors.
- Oversized files SHOULD be decomposed when a change naturally exposes a clean
	extraction point.

### VI. Secure Configuration And Secret Hygiene
RESTRunner MUST keep secrets, credentials, and security-sensitive defaults out
of executable source code. Runtime configuration that requires passwords,
tokens, or connection secrets MUST come from environment variables, user input,
secret stores, or clearly non-secret placeholder values that cannot be mistaken
for deployable credentials. Sample data and developer scaffolding MUST make the
unsafe state obvious and MUST not normalize shipping hardcoded secrets.

Rationale: the approved CAP-2026-001 amendment closes a governance gap exposed
by repeated audit findings for hardcoded credential defaults in executable
source paths.

- Executable code MUST NOT embed real or plausible deployable secrets.
- Runtime credential inputs MUST come from configuration providers, secret
	stores, or explicit user-provided values.
- Sample or seeded values MUST use unmistakable non-secret placeholders and
	document how operators replace them safely.
- Security-sensitive configuration paths SHOULD fail fast when required secrets
	are absent instead of silently falling back to defaults.

## Engineering Standards

- File-backed JSON storage under RESTRunner.Web/Data is the default persistence
	model unless a plan explicitly introduces another store.
- Shared execution statistics MUST remain thread-safe and continue using safe
	concurrency primitives such as Interlocked, ConcurrentDictionary, and
	ConcurrentBag where shared mutation exists.
- HTTP-facing code SHOULD prefer explicit error responses such as ProblemDetails
	or equivalent typed failure payloads rather than ad hoc string errors.
- Auto-generated files MAY remain large when regeneration is the source of truth,
	but hand-maintained code SHOULD stay focused and cohesive.

## Delivery Workflow

- Specs, plans, and tasks MUST include a constitution check that verifies layer
	boundaries, testing impact, logging impact, documentation impact, and input
	validation impact, and secret/configuration impact.
- Feature plans SHOULD map work onto the real solution structure:
	RESTRunner.Domain, RESTRunner.Services.HttpClient, RESTRunner,
	RESTRunner.Web, RESTRunner.PostmanImport, and RESTRunner.Domain.Tests.
- Work that changes web assets SHOULD include npm build validation when the
	change affects RESTRunner.Web/package.json-managed assets.
- Pull request reviews SHOULD treat MUST rules as blocking and SHOULD rules as
	improvement guidance that can be deferred with rationale.
- Reviews, audits, and implementation plans SHOULD flag hardcoded secret-like
	literals in executable code as constitution violations unless an approved
	exception is documented.

## Governance

This constitution governs repository-wide engineering decisions and supersedes
conflicting ad hoc guidance. Amendments MUST be made by updating this file in the
same change set as any dependent templates or guidance files. The amendment
process is documented but informal: a change proposal MUST describe the reason,
the expected repository impact, and any follow-up migration work, but no
dedicated approval role is currently required beyond normal repository review.

Versioning follows semantic versioning for governance:

- MAJOR: Remove a principle, redefine a principle in a backward-incompatible way,
	or materially loosen a mandatory rule.
- MINOR: Add a new principle or materially expand mandatory guidance.
- PATCH: Clarify wording, fix references, or make non-semantic refinements.

Compliance review expectations:

- Every plan and task set MUST check for constitution alignment.
- Every implementation review SHOULD confirm testing, layering, logging,
	documentation, validation, and secret/configuration impacts.
- Temporary exceptions MUST be documented in the relevant plan or pull request
	with the simpler alternative that was rejected.

**Version**: 1.1.0 | **Ratified**: 2026-03-15 | **Last Amended**: 2026-03-25
