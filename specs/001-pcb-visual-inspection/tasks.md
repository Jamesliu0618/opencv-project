---
description: "Task list for PCB Visual Inspection feature"
---

# Tasks: PCB Visual Inspection (001-pcb-visual-inspection)

**Input**: Design docs from `/specs/001-pcb-visual-inspection/` (spec.md, plan.md, research.md)
**Prerequisites**: plan.md (done), spec.md (done), research.md (done)

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, developer workflows and environment for .NET/WinForms + OpenCV (Emgu/OpenCvSharp)

- [ ] T001 Create Visual Studio solution `PCBInspection.sln` with multi-project structure:
  - `PCBInspection.Core/` (.csproj) — 核心邏輯
  - `PCBInspection.Drivers/` (.csproj) — 硬體驅動層
  - `PCBInspection.UI/` (.csproj) — WinForms UI
  - `PCBInspection.Tests/` (.csproj) — 單元/整合測試
- [ ] T002 [P] Add `PCBInspection.Drivers/AdvantechAdapter.cs` (interface + stub) and `PCBInspection.Drivers/CameraAdapter.cs` (interface + stub)
- [ ] T003 Initialize NuGet packages and project references (Phase 1):
  - `OpenCvSharp4` (recommended, MIT) — OpenCV bindings for .NET
  - `NUnit` (recommended) — unit/integration testing framework
  - `Newtonsoft.Json` — report serialization
  - Note: **do NOT include ONNXRuntime** in Phase 1 (no ML inference required for MVP)
- [ ] T004 [P] Add `scripts/powershell/build-and-test.ps1`, `deploy-to-ipc.ps1`, `smoke-test.ps1` and document usage in `docs/` (already added)
- [ ] T005 Configure basic CI (draft GitHub Actions workflow) to run `build-and-test.ps1` on Windows runner (workflow file in `.github/workflows/`)
- [ ] T006 Create `tests/fixtures/images/` and add minimal deterministic fixture set (10–30 images) with license/metadata files in `tests/fixtures/metadata/`
- [ ] T007 [P] Add `specs/001-pcb-visual-inspection/checklists/requirements.md` to repository (done) and ensure checklist items are resolved

**Checkpoint**: Developers can build solution, run tests and smoke-tests locally and in CI

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infra—drivers, calibration, test harness, performance baseline, and observability

- [ ] T008 [P] Implement `PCBInspection.Drivers/mock_camera.cs` for CI and local development (reads from `PCBInspection.Tests/fixtures/images/`)
- [ ] T009 [P] Implement `PCBInspection.Drivers/mock_io.cs` for CI to simulate Advantech IO signals and log events
- [ ] T010 Implement `PCBInspection.Core/calibration.cs` which computes and stores `pixel_to_mm` transform and exposes API `GetPixelToMm()`
- [ ] T011 [P] Add deterministic visual regression test harness in `PCBInspection.Tests/integration/test_smoke.cs` which runs pipeline on a golden image and fails if output differs beyond tolerance
- [ ] T012 Add performance harness `tools/perf/measure.py` or `.NET` tool that records per-image latency and memory; baseline metrics stored in `artifacts/perf/baseline.json`
- [ ] T013 [P] Create `PCBInspection.Core/logging.cs` to emit structured logs (json) and save annotated images for debugging to `debug/`
- [ ] T014 Add `specs/001-pcb-visual-inspection/contracts/models.md` registry placeholder and rules for adding models (done)
- [ ] T015 [P] Implement health check endpoints / scripts used by `deploy-to-ipc.ps1` (e.g., `scripts/health-check.ps1` / application `--health` flag)

**Checkpoint**: Foundation complete—drivers and test harness in place and CI smoke test fails until implementation

---

## Phase 3: User Story 2 - 零件定位與尺寸測量 (Priority: P1)

**Goal**: Accurate component localization and measurement with reporting of coordinates & measurements
**Independent Test**: `PCBInspection.Tests/integration/test_us2_localization.cs` using calibration board and known positions

- [ ] T024 [P] [US2] Add unit tests for `PCBInspection.Core/calibration.cs` to validate pixel→mm conversion accuracy; **must support coordinate transforms (image → machine), camera angle & mirror correction, and include a Calibration Wizard tool**
- [ ] T025 [US2] Implement `PCBInspection.Core/localization.cs` to detect component centers and angles (returns Component entities with both pixel and mm coordinates)
- [ ] T026 [US2] Implement `PCBInspection.Core/measurement.cs` that computes sizes, gaps and compares to tolerances (outputs measurements in mm)
- [ ] T027 [US2] Add integration test `PCBInspection.Tests/integration/test_us2_localization.cs` validating precision ≤ ±0.1 mm and measurement error ≤ ±0.05 mm
- [ ] T028 [US2] Add visual verification tool `PCBInspection.UI/measure_tool.cs` to overlay measurement rulers and deltas for operator review

**Checkpoint**: Localization & measurement meet accuracy goals on validation fixtures

---

## Phase 4: User Story 1 - 生產線即時單張檢測 (Priority: P1) 🎯 MVP

**Goal**: End-to-end single image inspection producing OK/NG and annotated image & report within ≤3s
**Independent Test**: `PCBInspection.Tests/integration/test_us1_endtoend.cs` runs on fixtures and verifies output report and IO events

### Tests for US1
- [ ] T016 [P] [US1] Add contract test `PCBInspection.Tests/contract/test_inspection_contract.cs` validating `InspectionResult` schema and storage paths
- [ ] T017 [P] [US1] Add integration smoke test `PCBInspection.Tests/integration/test_us1_endtoend.cs` that runs the pipeline (mock camera + mock IO) and verifies OK/NG and report generation

### Implementation for US1
- [ ] T018 [US1] Implement `PCBInspection.Core/pipeline.cs` with stages: grab→preprocess→detect→postprocess→annotate→report (depends on T008, T010, T011, and **depends on T025, T026**)
- [ ] T019.1 [US1] Implement solder joint detection (template matching) `PCBInspection.Core/detectors/solder_defect_detector.cs` (depends on T025, T026)
- [ ] T019.2 [US1] Implement surface defect detection (morphology operations) `PCBInspection.Core/detectors/surface_defect_detector.cs` (depends on T025, T026)
- [ ] T019.3 [US1] Implement circuit anomaly detection (connected components analysis) `PCBInspection.Core/detectors/circuit_defect_detector.cs` (depends on T025, T026)
- [ ] T019.4 [US1] Integrate detection results and severity scoring (1–5) `PCBInspection.Core/detectors/aggregator.cs` (depends on T025, T026)
- [ ] T020 [US1] Implement annotated image writer `PCBInspection.Core/annotate.cs` to produce overlayed bounding boxes & text and save to `artifacts/annotated/`
- [ ] T021 [US1] Implement `PCBInspection.Core/services/reporting.cs` to generate JSON report and PDF export (report_path referenced in InspectionResult)
- [ ] T022 [US1] Implement IO event emission `PCBInspection.Drivers/AdvantechAdapter.cs` to send OUTPUT_OK/OUTPUT_NG pulses (uses mock_io for tests)
- [ ] T023 [P] [US1] Add UI acceptance view `PCBInspection.UI/inspection_view.cs` to show live annotated image and OK/NG status

**Checkpoint**: US1 passes contract & integration tests and smoke tests

---

## Phase 5: User Story 3 - 報告與可追溯性 (Priority: P2)

**Goal**: Produce full inspection report, persist records, and support exports for analytics
**Independent Test**: `tests/integration/test_us3_reporting.cs` ensures report completeness and exports

- [ ] T029 [US3] Implement storage layer `PCBInspection.Core/services/storage.cs` (SQLite metadata + file system for images/reports)
- [ ] T030 [P] [US3] Implement report export `PCBInspection.Core/services/exporter.cs` for CSV/Excel summary and per-day stats
- [ ] T031 [US3] Add tests validating that NG images are saved and reports include model_version, seeds, parameters
- [ ] T032 [US3] Add UI report viewer `PCBInspection.UI/report_viewer.cs` to browse past inspections and export statistics

**Checkpoint**: Reporting & storage tested and ready for QA

---

## Phase 6: User Story 4 - 運動控制整合 (Priority: P2)

**Goal**: Reliable Digital IO signaling (Advantech PCI-1245E/PCI-1756 compatibility) and optional PLC interfaces
**Independent Test**: Hardware integration test on Advantech card (or full mock-driven validation in CI)

- [ ] T033.1 [P] [US4] Initialize Advantech APS Library and device configuration (`APS_initial()`), implement connection & config routines
- [ ] T033.2 [P] [US4] Implement Digital IO output wrapper `APS_write_d_output()` in `PCBInspection.Drivers/advantech_output.cs` (send pulses, configurable duration)
- [ ] T033.3 [P] [US4] Implement Digital IO input read `APS_read_d_input()` for handshake and acknowledge handling in `PCBInspection.Drivers/advantech_input.cs`
- [ ] T033.4 [P] [US4] Implement diagnostics and card health monitoring (`card health check`) and expose telemetry
- [ ] T034 [US4] Add robust error handling & retry logic for IO operations (`timeout_ms`, retries, escalate to safe state)
- [ ] T035 [US4] Add integration tests for IO using `PCBInspection.Drivers/mock_io` verifying pulses and latency ≤100 ms
- [ ] T036 [US4] Implement optional PLC gateway `PCBInspection.Drivers/plc_adapter.cs` supporting Modbus TCP / OPC UA and tests for these paths
- [ ] T042 [US4] Hardware test: Validate Digital IO on actual Advantech PCI-1245E card (pulse correctness, mapping, latency)
- [ ] T043 [US4] Hardware test: Validate image capture using actual Basler GigE camera (throughput, frame stability)
- [ ] T044 [US4] Hardware test: Validate end-to-end timing meets < 3s per PCB requirement in production configuration
- [ ] T045 [US4] Hardware test: Continuous run stability test 8+ hours and capture memory/cpu metrics

**Checkpoint**: IO reliability validated with mock and hardware smoke tests

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, packaging, performance tuning, and security

- [ ] T037 [P] Update `docs/quickstart.md` with final install instructions and add `production.yaml` example at `specs/001-pcb-visual-inspection/docs/production.yaml`
- [ ] T038 [P] Add telemetry hooks (metrics export) and performance dashboard scripts in `tools/metrics/`
- [ ] T039 [P] Add more unit tests and increase fixture coverage to >30 images
- [ ] T040 [P] Security hardening: review file write paths, sanitize inputs, ensure logs do not leak PII
- [ ] T041 [P] Prepare packaging & installer scripts (MSI or zip + installer PS script)

---

## Dependencies & Execution Order
- **Setup (Phase 1)** must finish before Phase 2 (Foundation)
- **Foundation** must finish before user story implementation begins
- **User Stories**: US1 and US2 (P1) should be prioritized and can run in parallel once foundation is done
- **US3/US4** (P2) depend on US1/US2 primitives but are independently testable

## Parallel Opportunities
- Driver implementations (`mock_camera`, `advantech_adapter`) and test harnesses can be implemented in parallel
- Model & detector tasks for different defect types can be parallelized as separate subtasks (mark with `[P]` accordingly)

## Implementation Strategy
- MVP: Phase 1 + Phase 2 + US1 + minimal US2 (component localization for key parts) → Validate with fixtures and smoke tests
- Incremental: Add full US2, US3, US4; expand fixtures and tests; performance tune

---

*Notes*: All tasks include explicit file paths so that each task can be executed by a developer or automated agent. Tests should be written first (contract tests) where possible.
