# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [6.0.0] - 2026-03-30

### Changed

- Rebranded from RESTRunner to RequestSpark across the entire solution, documentation, UI, and build artifacts (PR #11, spec 001-rebrand-requestspark).
- Renamed solution file, all project files, namespaces, assembly metadata, and global usings to use the RequestSpark identity.
- Updated web application titles, Swagger/OpenAPI branding, console output, and sample data to present RequestSpark consistently.
- Replaced plausible seed credentials with `REPLACE_ME` placeholders per constitution secure-configuration conventions.
- Added `.gitignore` rule for generated result CSV files under `RequestSpark.Web/Data/results/`.

### Removed

- All first-party references to the former RESTRunner brand name.
- GitHub repository renamed from `RESTRunner` to `RequestSpark`; README badges and clone URLs updated (T025).
