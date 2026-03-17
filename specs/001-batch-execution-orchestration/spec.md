# Feature Specification: Batch Execution Orchestration for HttpClientUtility

**Feature Branch**: `001-batch-execution-orchestration`
**Created**: 2026-03-16
**Status**: Draft
**Input**: User description: "Add batch execution orchestration capabilities to WebSpark.HttpClientUtility so RESTRunner can fully delegate HTTP execution to the package, eliminating the need for RESTRunner.Services.HttpClient and duplicated logic in RealExecutionService."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Parameterized Request Execution (Priority: P1)

As a library consumer, I want to define request templates with placeholder tokens in URLs and bodies, and have the system substitute values from a property dictionary before sending each request. This enables per-user or per-context parameterization without manual string replacement.

**Why this priority**: Template substitution is the foundational building block. Without it, none of the higher-level orchestration features can work because every request in a batch run typically needs user-specific or context-specific values injected into paths and bodies.

**Independent Test**: Can be fully tested by defining a request template with placeholders, providing a property dictionary, and verifying the sent request contains the substituted values. Delivers immediate value for any consumer needing parameterized HTTP calls.

**MSTest Coverage**: Unit tests must verify placeholder replacement in URL paths, request bodies, and combined scenarios. Edge cases for missing keys, empty values, and nested/escaped braces must be covered.

**Acceptance Scenarios**:

1. **Given** a request template with `{userId}` in the URL path and a properties dictionary containing `userId=42`, **When** the request is executed, **Then** the outbound URL contains `42` in place of `{userId}`.
2. **Given** a request body template with `{firstName}` and `{lastName}` placeholders and a properties dictionary with both keys, **When** the request is executed, **Then** the body sent contains the substituted values.
3. **Given** a request template with `{{encoded_user_name}}` in the path and a user context with username `john.doe`, **When** the request is executed, **Then** the path contains `john.doe` in place of the token.
4. **Given** a properties dictionary that is missing a key referenced in the template, **When** substitution occurs, **Then** the placeholder remains as-is (no error thrown) and the raw placeholder text is preserved in the request.

---

### User Story 2 - Combinatorial Batch Execution (Priority: P2)

As a library consumer, I want to define sets of target environments, user contexts, and request definitions, then execute all combinations in parallel with configurable concurrency limits and iteration counts. This enables load testing and multi-environment comparison scenarios.

**Why this priority**: This is the core orchestration capability that differentiates a simple HTTP client from a batch execution engine. It depends on P1 (template substitution) but is the primary reason RESTRunner needs these additions.

**Independent Test**: Can be tested by configuring 2 environments, 2 users, and 2 requests with 1 iteration, verifying that all 8 combinations execute and results are collected.

**MSTest Coverage**: Tests must verify correct combinatorial expansion (instance count x user count x request count x iteration count), concurrency throttling via semaphore, and cancellation token propagation.

**Acceptance Scenarios**:

1. **Given** 2 environments, 3 users, and 4 requests configured with 1 iteration, **When** batch execution runs, **Then** exactly 24 requests are executed (2 x 3 x 4).
2. **Given** a concurrency limit of 5, **When** batch execution runs with 100 pending requests, **Then** no more than 5 requests are in-flight simultaneously.
3. **Given** a running batch execution and the cancellation token is triggered, **When** the system processes the cancellation, **Then** no new requests are started and the execution completes gracefully with partial results.
4. **Given** 10 iterations configured, **When** batch execution completes, **Then** the total request count equals (environments x users x requests x 10).

---

### User Story 3 - Execution Statistics Collection (Priority: P3)

As a library consumer, I want to receive aggregated, thread-safe statistics from a batch execution run, including total/success/failure counts, response time percentiles, and breakdowns by method, environment, user, and status code. This enables performance analysis and regression detection.

**Why this priority**: Statistics collection transforms raw execution into actionable data. It depends on P2 (batch execution) to generate the volume of requests needed for meaningful statistics.

**Independent Test**: Can be tested by running a batch of known requests against a mock endpoint and verifying the returned statistics object contains accurate counts and percentile calculations.

**MSTest Coverage**: Tests must verify thread-safe counter increments under concurrent access, accurate percentile calculations (P50, P95, P99), and correct aggregation across multiple dimensions.

**Acceptance Scenarios**:

1. **Given** a completed batch execution of 100 requests where 90 succeeded and 10 failed, **When** statistics are retrieved, **Then** total count is 100, success count is 90, and failure count is 10.
2. **Given** a set of recorded response times, **When** P50, P95, and P99 percentiles are calculated, **Then** the values accurately reflect the distribution of response times.
3. **Given** requests sent using GET, POST, and DELETE methods, **When** statistics are retrieved, **Then** per-method breakdown shows the correct count for each verb.
4. **Given** requests sent to 3 different environments, **When** statistics are retrieved, **Then** per-environment breakdown shows the correct count for each environment.

---

### User Story 4 - Response Comparison and Hashing (Priority: P4)

As a library consumer, I want the system to compute deterministic hashes of response bodies so I can compare responses across different environments or users for the same request. This enables the "compare" use case where the same API call is verified across staging/production or across user contexts.

**Why this priority**: Comparison is a specialized feature built on top of batch execution. It is valuable for regression detection but not required for basic batch execution to function.

**Independent Test**: Can be tested by sending the same request to two mock endpoints returning identical content and verifying the response hashes match, then changing one response and verifying the hashes differ.

**MSTest Coverage**: Tests must verify deterministic hash generation (same content always produces same hash), hash uniqueness for different content, and correct hash association with execution results.

**Acceptance Scenarios**:

1. **Given** two responses with identical body content, **When** hashes are computed, **Then** both hashes are equal.
2. **Given** two responses with different body content, **When** hashes are computed, **Then** the hashes are different.
3. **Given** a batch result set grouped by request, **When** results are compared across environments, **Then** matching hashes indicate equivalent responses and differing hashes indicate discrepancies.

---

### User Story 5 - Result Output Streaming (Priority: P5)

As a library consumer, I want to provide a result sink (output destination) that receives execution results as they complete, supporting multiple output formats such as CSV files, real-time streams, or custom handlers.

**Why this priority**: Output streaming is an integration point. The core orchestration must work before consumers can direct output anywhere.

**Independent Test**: Can be tested by providing a mock output handler, running a small batch, and verifying all results are delivered to the handler as they complete (not batched at the end).

**MSTest Coverage**: Tests must verify that the output handler receives results incrementally during execution, not only after all requests complete.

**Acceptance Scenarios**:

1. **Given** a custom output handler registered for a batch execution, **When** individual requests complete, **Then** results are streamed to the handler immediately (not buffered until the end).
2. **Given** a request that fails with an error, **When** the failure is reported, **Then** the output handler receives a failure result with error details.

---

### Edge Cases

- What happens when an environment base URL is unreachable for all requests? Statistics must still reflect the failures and execution must continue for other environments.
- How does the system handle template placeholders that contain special characters (braces within values, URL-unsafe characters)? Values should be substituted as-is; URL encoding is the consumer's responsibility.
- What happens when the iteration count is set to zero? The system should return an empty statistics result without executing any requests.
- How does the system behave when the user list or request list is empty? The system should return an empty statistics result without error.
- What happens when concurrent requests exceed the configured concurrency limit during high-throughput runs? The semaphore must block new requests until slots become available, preventing resource exhaustion.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The library MUST provide a template substitution engine that replaces `{key}` placeholders in request URL paths and bodies with values from a caller-supplied property dictionary.
- **FR-002**: The template engine MUST support the `{{encoded_user_name}}` special token pattern, replacing it with a user-context-provided identifier.
- **FR-003**: When a placeholder key is not found in the property dictionary, the system MUST preserve the raw placeholder text in the output (no exception, no empty string).
- **FR-004**: The library MUST provide a batch execution orchestrator that accepts collections of environments (base URLs), user contexts (properties + identifier), and request definitions (method, path template, body template).
- **FR-005**: The batch orchestrator MUST execute all combinations of (environments x users x requests x iterations), where iteration count is configurable by the caller.
- **FR-006**: The batch orchestrator MUST enforce a configurable concurrency limit (maximum simultaneous in-flight requests) using semaphore-based throttling.
- **FR-007**: The batch orchestrator MUST support cancellation via standard cancellation tokens, stopping new request dispatch while allowing in-flight requests to complete.
- **FR-008**: The library MUST support all standard HTTP methods (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS) and custom methods (e.g., MERGE, COPY) via arbitrary method name strings.
- **FR-009**: For methods that support a request body (POST, PUT, PATCH, MERGE, COPY), the system MUST attach the template-substituted body content. For methods that do not require a body (GET, DELETE, HEAD, OPTIONS), no body MUST be attached.
- **FR-010**: The library MUST collect thread-safe execution statistics during batch runs, including: total request count, success count, failure count, and per-request response time in milliseconds.
- **FR-011**: The statistics collector MUST provide response time percentile calculations (at minimum P50, P95, P99).
- **FR-012**: The statistics collector MUST provide per-dimension breakdowns: by HTTP method, by environment, by user, and by response status code.
- **FR-013**: The library MUST compute a deterministic hash of each response body, such that identical content always produces the same hash value.
- **FR-014**: The library MUST provide a result output abstraction (sink interface) that consumers implement to receive individual execution results as they complete during batch execution.
- **FR-015**: The output sink MUST be called incrementally as results arrive, not buffered until the batch completes.
- **FR-016**: Each execution result delivered to the output sink MUST include: environment identifier, request path, HTTP method, success/failure status, status code, response body hash, response duration in milliseconds, user identifier, and timestamp.
- **FR-017**: The batch orchestrator MUST integrate with the existing resilience features (Polly retry and circuit breaker) for individual request execution when those features are enabled.
- **FR-018**: The batch orchestrator MUST propagate correlation IDs from the existing telemetry layer to each request in the batch.

### Key Entities

- **Execution Configuration**: Represents the full batch run setup — collections of environments, user contexts, request definitions, iteration count, and concurrency limit.
- **Environment**: A target API endpoint defined by a base URL and a display name. Multiple environments enable cross-instance comparison.
- **User Context**: A named identity with a property dictionary used for template substitution. Represents a caller/persona whose credentials or attributes vary across requests.
- **Request Definition**: A template for an HTTP request — HTTP method, path template, body template, and metadata about whether the method requires a body.
- **Execution Result**: The outcome of a single request — environment, request, user, success/failure, status code, response hash, duration, and timestamp.
- **Execution Statistics**: Aggregated metrics from a complete batch run — counts, percentiles, and multi-dimensional breakdowns.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A consumer can configure and execute a batch of 1,000 parameterized requests across multiple environments with a single method call, receiving aggregated statistics upon completion.
- **SC-002**: Template substitution correctly replaces all placeholder tokens in 100% of test cases, including edge cases with missing keys and special characters.
- **SC-003**: Execution statistics percentile calculations (P50, P95, P99) are accurate to within 1% of the actual distribution when validated against a known dataset of response times.
- **SC-004**: The concurrency limiter holds simultaneous in-flight requests at or below the configured maximum throughout the entire batch execution.
- **SC-005**: Cancellation of a running batch halts new request dispatch within one concurrency cycle and returns partial statistics for completed requests.
- **SC-006**: Response body hashing produces identical hashes for identical content and distinct hashes for differing content across all test scenarios.
- **SC-007**: All existing WebSpark.HttpClientUtility features (resilience, caching, telemetry, authentication) continue to function correctly when the batch orchestration features are enabled, verified by the existing test suite passing without modification.

### Assumptions

- The batch orchestration features will be added to the existing `WebSpark.HttpClientUtility` NuGet package (not a separate package).
- The iteration count default will be 1 if not specified by the consumer.
- The concurrency limit default will be 10 if not specified by the consumer (matching the current RESTRunner behavior).
- The deterministic hash algorithm does not need to be cryptographically secure; it needs to be fast and collision-resistant for comparison purposes.
- Template substitution operates on raw strings only — no recursive substitution (a substituted value containing `{key}` is not re-expanded).
