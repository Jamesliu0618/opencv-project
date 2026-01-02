# Implementation Plan: PCB Visual Inspection (001-pcb-visual-inspection)

**Branch**: `001-pcb-visual-inspection` | **Date**: 2026-01-02 | **Spec**: `spec.md`
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a Windows-based automated PCB quality inspection application using OpenCV for image processing, supporting live capture from industrial cameras, offline image analysis, defect detection (rule-based CV + optional ML fallback), component localization, dimension measurement, OK/NG decisioning, report generation and Digital IO-based motion-control integration. The approach emphasizes deterministic fixtures, CI-driven visual regression, and a modular driver/abstraction layer for camera and motion control to permit vendor-specific integration.

## Technical Context

**Language/Version**: Python 3.11 (primary)
**Primary Dependencies**: OpenCV (opencv-python), NumPy, scikit-image, PySide6 (UI), pandas, PyTorch (optional for ML), pytest
**Storage**: File-system for images & reports, SQLite for inspection metadata and indexing
**Testing**: pytest + custom image test harness, deterministic fixtures in `tests/fixtures/images/`, golden image visual regression tests, performance harness for per-image timing
**Target Platform**: Windows industrial PC (x64). Minimum: quad-core CPU, 8 GB RAM. Recommended: 16 GB RAM + optional NVIDIA GPU for ML
**Project Type**: Single desktop application (service + native UI) with modular drivers
**Performance Goals**: Single-image processing ≤3s (p95), startup ≤10s, motion-control signal latency ≤100 ms, detection accuracy ≥99%, FP <1%
**Constraints**: Must operate 8+ hours continuously, tolerate ±2 mm PCB shift, robust to moderate illumination variation
**Scale/Scope**: Single-station inspection, designed to be replicable across multiple stations

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Gates determined based on constitution file:

- **Performance & Resources**: Performance goals MUST be specified when applicable (e.g., target FPS, p95 latency, memory budgets). Plan MUST include how these will be measured.
- **Data Handling & Privacy**: Data processing MUST document PII handling, consent, retention, and storage protections for sample datasets.
- **Test Artifacts**: Tests MUST include deterministic image fixtures, seeds for reproducibility, and sample inputs that exercise primary flows.
- **Model & License Compliance**: Any third‑party models or datasets MUST have license provenance documented and approved before inclusion.
- **Observability & Debugging**: Plan MUST describe logging, visual-debug outputs (e.g., overlay images), and metrics to capture for validation.
- **CI Smoke Tests**: CI SHOULD include at least one smoke image test that fails before implementation (tests written first).

(These gates derive from the project constitution and are mandatory checks for CV-oriented features.)

## Project Structure

### Documentation (this feature)

```text
specs/001-pcb-visual-inspection/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── checklists/requirements.md
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: Single-project layout with driver abstractions (see `src/drivers/`) to isolate hardware/vendor specifics and keep processing and UI decoupled.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
