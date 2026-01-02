# data-model.md — PCB 視覺檢測資料模型

## 目的
定義關鍵實體 (entities)、欄位、資料型別與驗證規則，確保檢測資料的一致性、可追溯性與測試可重現性。

---

## 單位與慣例
- 長度單位：mm
- 座標系：相機座標（像素）與機械座標（mm）兩套系統，明確記錄校正矩陣（pixel_to_mm matrix）
- 時間戳：ISO 8601 格式（UTC）
- 圖像檔：PNG/JPEG（lossless 建議 PNG）、檔名包含 pcb_id、timestamp

---

## Entities

### PCB
- id: string (unique)
- model: string (PCB 型號)
- batch: string (生產批次)
- calibration_id: string (camera ↔ board 校正參考)
- acquired_from: "camera" | "file"
- acquired_at: timestamp
- source_image_path: string
- status: enum [pending, processed, error]

### Component
- id: string (component reference id, e.g., R1, U2)
- pcb_id: string (FK)
- type: string (料號或類型)
- center_x_px: float
- center_y_px: float
- center_x_mm: float
- center_y_mm: float
- angle_deg: float
- size_w_mm: float
- size_h_mm: float
- presence: boolean
- match_score: float (0..1)
- detection_confidence: float (0..1)

Validation rules:
- presence=false → size_* may be null
- match_score and detection_confidence have thresholds configurable per PCB model

### Defect
- id: string
- pcb_id: string
- type: enum [solder_defect, surface_defect, circuit_defect, component_misplacement, other]
- severity: integer (1..5)  # 1=least, 5=most severe
- bounding_box_px: [x, y, w, h]
- bounding_box_mm: [x, y, w, h]
- confidence: float (0..1)
- related_component_id: string | null
- detected_at: timestamp
- image_excerpt_path: string (cropped image highlighting defect)

Acceptance:
- severity >=4 → auto NG
- severity ==3 → requires human review

### InspectionResult
- id: string
- pcb_id: string
- ok: boolean
- defects: array[Defect]
- measurements: array (each item: {name, value, unit, tolerance_min, tolerance_max})
- annotated_image_path: string
- report_path: string
- model_version: string | null
- processing_time_ms: integer
- created_at: timestamp

### MotionEvent
- id: string
- inspection_id: string
- event_type: enum [OK_SIGNAL_SENT, NG_SIGNAL_SENT, ACK_RECEIVED, ERROR]
- pin_map: dict (signal_name -> channel)
- sent_at: timestamp
- status: enum [sent, acknowledged, failed]
- details: string (error message or notes)

---

## State transitions
- PCB.status: pending → processed (on successful InspectionResult creation) or error (on processing failure)
- InspectionResult.ok true → MotionEvent OK_SIGNAL_SENT
- InspectionResult.ok false → MotionEvent NG_SIGNAL_SENT

---

## Example JSON (InspectionResult)
{
  "id": "ins-0001",
  "pcb_id": "PCB-123",
  "ok": false,
  "processing_time_ms": 1200,
  "defects": [
    {"id":"d-1","type":"solder_defect","severity":4,"bounding_box_px":[100,50,20,20],"confidence":0.94}
  ],
  "annotated_image_path":"/data/annotated/PCB-123_20260102_120000.png",
  "report_path":"/data/reports/PCB-123_20260102_120000.json",
  "model_version":"rule-v1"
}

---

## Notes
- 所有座標均記錄 pixel 與 mm 兩種形式並保存校正矩陣，避免 run-time 重複計算導致微小不一致。
- metadata（相機設定、exposure、gain、model_version、seed）必須包含在 report 中以利可重現性。