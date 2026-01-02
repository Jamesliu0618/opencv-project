# research.md — PCB Visual Inspection (001-pcb-visual-inspection)

## Purpose
Resolve open technical choices for Phase 0 and document decisions, rationale, and alternatives for critical implementation choices.

---

## Decision 1: Language & UI
- Decision: **C# / .NET Framework 4.8 (WinForms)** as the primary implementation stack; OpenCV functionality accessed via **EmguCV** or **OpenCvSharp**.
- Rationale: The existing codebase and UI stack are C# WinForms; choosing .NET 4.8 ensures minimal integration effort with existing systems (Advantech .NET SDK, Windows drivers) and simplifies deployment on Windows industrial PCs. EmguCV/OpenCvSharp provide robust OpenCV bindings for C#; ML inference may use ONNX Runtime or ML.NET, or a small Python microservice where necessary.
- Alternatives considered: Python (still supported as an auxiliary service for heavy ML training or research workflows) and C++ (higher performance but greater development cost and integration complexity).

---

## Decision 2: Camera integration
- Decision: Implement a **camera abstraction layer** and target **GenICam-compatible / vendor SDK (e.g., Basler pylon, FLIR Spinnaker)**. Default integration will use OpenCV VideoCapture when supported, and vendor SDKs for higher stability/feature support.
- Rationale: Industrial cameras vary by vendor; an abstraction reduces coupling and allows swapping drivers. GenICam-compatible SDKs are common on Windows.
- Alternatives considered: Rely only on OpenCV VideoCapture (simpler but limited feature support and less stable for industrial cameras).

---

## Decision 3: Defect detection approach
- Decision: Start with **deterministic rule-based CV pipelines** (thresholding, morphology, template matching, contour analysis), and provide an extensible ML path using **PyTorch** for complex cases (classification/segmentation) if accuracy/robustness demands it.
- Rationale: Rule-based methods are explainable, fast, deterministic, and easier to validate with golden images. ML models provide better generalization for ambiguous defects but require data, training, and model governance.
- Alternatives considered: ML-first (higher accuracy for complex defects but requires dataset and ML lifecycle); Hybrid (use ML for classification, CV for measurement) — planned as medium-term.

---

## Decision 4: Motion control interface
- Decision: **Digital IO (24V / TTL)** as primary interface; implement an IO abstraction to support vendor I/O modules (e.g., NI, Advantech) and later add Modbus / EtherCAT if required.
- Rationale: Digital IO is low-latency, widely supported, and easier to map to simple OK/NG signals for the existing line. Abstraction allows future protocol extensions.
- Alternatives considered: Modbus TCP (networked integration), EtherCAT (industrial-grade), but both increase integration complexity and may require vendor hardware.

---

## Decision 5: Data, licensing & privacy
- Decision: Store images & reports locally; require dataset metadata (consent, source, license) for any third-party assets. Draft a model license/provenance document in `contracts/models.md`.
- Rationale: Keeps compliance traceable and aligns with constitution gates. Local storage suffices for single-station deployment.
- Alternatives considered: Centralized database or S3-like storage for large-scale aggregation (deferred to future work if multi-station analytics required).

---

## Decision 6: Testing & CI
- Decision: Use **.NET test frameworks** (NUnit or xUnit) for unit/integration tests and a Windows-based CI job (GitHub Actions Windows runner) that builds the solution, runs tests, and executes visual regression smoke tests. Deterministic image fixtures remain in `tests/fixtures/images/`. For image comparison and performance harnesses, use .NET image libraries (OpenCvSharp/Emgu wrappers or ImageSharp) to perform pixel/structural comparisons and capture per-image timing metrics.
- Rationale: Aligns testing with the primary .NET codebase, leverages Visual Studio test tooling for easier local debugging, and keeps visual regression and performance gates in CI while preserving deterministic fixture-driven tests. Python-based ML training pipelines can still use pytest in separate jobs when applicable.

---

## Next steps (Phase 1 inputs)
- Collect a minimal deterministic fixture set (10–30 images) covering OK and common NG classes.
- Implement camera driver abstraction and a mock driver for local development.
- Implement a prototype pipeline for one defect class (e.g., missing component) and a smoke visual regression test.
- Create `data-model.md`, `contracts/camera.md`, `contracts/io.md`, and `quickstart.md`.
