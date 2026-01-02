# contracts/models.md — Model Governance & Contract

## 目的
若在專案中使用 ML 模型（分類/分割/檢測），此文件定義模型上線前的治理要求、版本資訊、評估標準與授權/來源記錄。

## 模型元資料（範例）
- model_id: string
- name: string
- version: semver (e.g., v1.0.0)
- architecture: string (e.g., resnet50, custom UNet)
- task: enum [classification, segmentation, detection]
- input_size: [w,h]
- training_dataset: {name, version, license, provenance_uri}
- validation_dataset: {name, size, metrics}
- metrics: {precision, recall, f1, AP, IoU}
- license: string
- artifacts_uri: string (model binary, tokenizer, config)
- training_seed: int
- created_at: timestamp
- description: string

## Inclusion criteria
- license must be compatible (documented)
- validation metrics must meet thresholds defined in spec (e.g., detection accuracy ≥ 99% on validation set)
- model must have reproducible training recipe and seed

## Deployment contract
- runtime: supported frameworks (PyTorch stable versions)
- inference API: predict(input_image) -> detections/segmentations with confidence
- latency requirement: model must meet per-image latency budget (baseline recorded)
- model_version field recorded in each InspectionResult

## Evaluation & Monitoring
- continuous evaluation must be enabled: record metrics on representative production samples (sampling strategy)
- drift detection: capture distributional changes and alert when metrics degrade
- model rollback plan: if performance drops below threshold, automatically switch to previous model or rule-based fallback

## Provenance & Licensing
- every third-party dataset or pre-trained weights MUST have documented license and source URI
- keep a `models_registry.json` in `specs/001-pcb-visual-inspection/contracts/` recording included models and provenance

## Tests
- Unit tests for inference correctness on deterministic fixtures
- Regression tests comparing new model outputs against golden outputs (tolerances allowed)
- Performance tests for latency and memory
