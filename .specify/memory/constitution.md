<!--
Sync Impact Report
Version change: none → 0.1.0
Modified principles:
- I. Image-First Design (added)
- II. Performance & Real-time (added)
- III. Test-First & Reproducible (added)
- IV. Observability & Explainability (added)
- V. Cross-Platform & Minimal Dependencies (added)
Added sections: Performance & Resource Constraints; Data Privacy & Licensing; Constitution Check gates for CV features
Templates updated: 
- .specify/templates/plan-template.md ✅ updated
- .specify/templates/spec-template.md ✅ updated
- .specify/templates/tasks-template.md ✅ updated
Follow-ups: RATIFICATION_DATE TODO (needs confirmation)
-->

# OpenCV Application Constitution

## Core Principles

### I. Image-First Design
Every feature MUST begin with a clearly specified image/video data contract: formats, resolutions, color spaces, annotation schemas and preprocessing steps. Pipelines MUST be designed to be deterministic and independently testable; data contracts are treated as public interfaces and changes to them are breaking changes unless properly versioned and migrated.

### II. Performance & Real-time (NON-NEGOTIABLE where applicable)
When a feature processes images or video, the specification MUST define measurable performance targets (e.g., target FPS, p95 latency, memory and CPU/GPU limits). Implementations MUST include profiling plans, baseline measurements and regression tests; exceeding agreed degradation thresholds constitutes a breaking change.

### III. Test-First & Reproducible (NON-NEGOTIABLE)
Tests MUST be written before implementation. This includes unit tests for processing functions, integration tests using deterministic image fixtures, and reproducible model inference with fixed seeds. CI pipelines MUST run these tests and fail on regressions.

### IV. Observability & Explainability
All components MUST emit structured logs and metrics, and provide visual debug outputs (e.g., overlayed bounding boxes, difference images) to enable rapid diagnosis. Implementations MUST capture sufficient artifacts (inputs, outputs, parameters, seeds) to reproduce observed behavior.

### V. Cross-Platform & Minimal Dependencies
The project SHOULD support Windows, Linux and macOS. Core functionality SHOULD rely on OpenCV and well‑maintained, properly licensed dependencies. Any breaking changes to public interfaces, data contracts or supported platforms MUST follow the versioning policy documented below.

## Additional Constraints
- **Data Handling & Privacy**: Images that contain personal data MUST include a documented redaction policy, consent metadata, retention policy, and storage protections.
- **Licensing & Provenance**: All third‑party models and datasets used MUST include license and provenance information; inclusion of incompatible licensed assets is prohibited.
- **Platform Guarantees**: GPU acceleration is permitted but any GPU‑only feature MUST document a CPU fallback or clear platform limitation.

## Development Workflow
- PRs MUST include or update tests that validate new behaviors; at least one approver is required for merge.
- CI gates MUST include: unit tests, image smoke tests using fixtures, and performance baseline checks where applicable.
- Release & Versioning: Use semantic versioning where:
  - MAJOR: Breaking changes to data contracts, public APIs, or supported platforms.
  - MINOR: New, backwards-compatible features.
  - PATCH: Documentation, wording, or minor clarifications.
- Performance or data handling changes MUST include a migration and validation plan in the associated spec/plan.

## Governance
Amendments to this constitution MUST be introduced via PR that:
- Documents the proposed change and rationale
- Updates this constitution and any affected templates or specs
- Includes validation steps, tests, and a suggested version bump
- Receives approval from a majority of maintainers listed in the repository governance

**Version**: 0.1.0 | **Ratified**: 2026-01-02 | **Last Amended**: 2026-01-02

