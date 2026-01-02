# Feature Specification: PCB 視覺檢測應用 (PCB Visual Inspection)

**Feature Branch**: `001-pcb-visual-inspection`  
**Created**: 2026-01-02  
**Status**: Draft  
**Input**: 使用者需求（摘要）：建立一套基於 OpenCV 的自動化 PCB 品質檢測系統，支援工業相機即時擷取與離線影像處理，包含瑕疵檢測、零件定位、尺寸測量、OK/NG 判定與運動控制整合，達到高精度與高速檢測以符合產線要求。

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 生產線即時單張檢測 (Priority: P1)

操作員在產線上啟動檢測程式，系統使用工業相機或載入離線影像進行單張 PCB 檢測，回傳標註影像、測量結果與明確 OK/NG 判定，並在 NG 時儲存影像與詳細報告。

**Why this priority**: 直接取代人工目視檢查，屬於最小可行產品 (MVP) 的核心價值。  
**Independent Test**: 使用已標註的測試影像（deterministic fixtures）執行單張檢測，驗證輸出包含標註影像、OK/NG 與報告；在指定測試集上達到準確率與時效性要求。

**Acceptance Scenarios**:

1. **Given** 系統與相機已啟動並完成校正，**When** 一片 PCB 被定位並觸發擷取，**Then** 系統在 ≤3 秒內回傳包含瑕疵標註之影像與 OK/NG 判定。
2. **Given** 系統判定 NG，**When** 檢測完成，**Then** 儲存該 PCB 原始影像、標註影像與檢測報告至檔案庫，並發送 NG 訊號給運動控制系統 (回應時間 ≤100 ms)。

---

### User Story 2 - 零件定位與尺寸測量 (Priority: P1)

系統能定位 PCB 上每個元件中心與角度，判斷缺件/錯件/偏移，並測量元件尺寸與間距，與標準規格比對決定是否超出公差。

**Why this priority**: 定位及量測是判定瑕疵類型與嚴重度的基礎，為自動化分流與回饋提供必要數據。  
**Independent Test**: 使用具已知 ground-truth 的校正板與標準件資料集驗證定位與量測精度（定位誤差 ≤±0.1 mm，量測誤差 ≤±0.05 mm）。

**Acceptance Scenarios**:

1. **Given** 標定完成，**When** 檢測影像含已知零件，**Then** 回傳每個零件的中心座標 (mm) 與旋轉角度 (deg)，且誤差在 ±0.1 mm 以內。

---

### User Story 3 - 報告與可追溯性 (Priority: P2)

系統生成每片 PCB 的檢測報告（含原始影像、標註影像、測量數據、判定結果、時間戳與參數），並支援匯出統計資料供分析。

**Why this priority**: 品質管控需保存可追溯紀錄以便回溯與統計分析。  
**Independent Test**: 執行一批測試，驗證每筆檢測都產生完整報告，並確認統計匯出檔案格式正確且包含預期欄位。

**Acceptance Scenarios**:

1. **Given** 系統完成一段檢測批次，**When** 使用者匯出每日報表，**Then** 報表包含總檢測數、NG 數、NG 分類分布、平均處理時間等指標。

---

### User Story 4 - 運動控制整合 (Priority: P2)

系統在檢測完成後，根據 OK/NG 與分流規則發送訊號給運動控制卡 (或透過指定通訊協定) 以觸發分流或回收動作，並可提供缺陷座標供後續動作參考。

**Why this priority**: 與既有產線機台整合為驗收門檻，必須可靠且低延遲。  
**Independent Test**: 模擬運動控制收發介面或與實際控制卡測試，驗證訊號回應時間 ≤100 ms 且正確觸發分流動作。

**Acceptance Scenarios**:

1. **Given** 控制卡已連接並配置，**When** 系統判定 NG，**Then** 系統在 ≤100 ms 內發出 NG 訊號並記錄發送成功或失敗狀態。

---

### Edge Cases

- 光源明顯變化（極端低亮或高反光）導致偵測誤差，系統需能檢測並回報 "光源異常" 錯誤訊息。
- PCB 偏移大於設計容忍度（>±2 mm）時需能回報定位失敗或請求重新定位。
- 多塊 PCB 同時出現在影像中時，系統應能識別並拒絕（或分割成多次檢測）並回報錯誤。
- 相似類型瑕疵造成誤判（例如污漬 vs 焊錫），需提供人工覆核工作流或置信度閾值控制。

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: 系統 MUST 支援從工業相機即時擷取影像與載入離線影像檔案進行單張檢測（單次處理一片 PCB）。
- **FR-002**: 系統 MUST 能檢測下列瑕疵類型：焊點異常（冷焊、虛焊、漏焊、短路）、表面瑕疵（刮痕、污染、氧化）、電路異常（斷路、橋接），並可標註瑕疵位置與分級嚴重度，採用 **1–5 等級量表**（1 = 最輕微，5 = 最嚴重）。
  - **Severity 定義（1–5）**:
    - **1**: 輕微 cosmetic（僅記錄，監控）
    - **2**: 輕度功能風險（記錄，視情形需人工覆核）
    - **3**: 中度瑕疵（需人工覆核）
    - **4**: 嚴重瑕疵（自動判定為 NG，分流）
    - **5**: 致命/功能性失效（立即 NG，並觸發高優先度處理）
  - **處置範例**: 等級 1–2 → 記錄並繼續生產；等級 3 → 人工覆核；等級 4–5 → 自動 NG 並分流。
- **FR-003**: 系統 MUST 能定位元件位置、類型、方向，並偵測缺件、錯件、偏移；回傳中心座標 (mm) 與旋轉角度 (deg)。
- **FR-004**: 系統 MUST 能測量元件尺寸（長、寬）、元件間距與焊點尺寸，並與指定標準規格進行公差比對，回報是否超出容差。
- **FR-005**: 系統 MUST 針對每片 PCB 做出明確的 OK/NG 判定；若 NG，需列出不良原因、嚴重度與影像上的位置標註。
- **FR-006**: 系統 MUST 自動儲存 NG 影像與對應報告並保存檢測紀錄以利追溯與稽核（包含時間戳、參數、模型版本）。
- **FR-007**: 系統 MUST 與運動控制系統整合；**首選通訊方式為 Digital IO（24V/TTL）**，支援簡單的 OK/NG 輸出訊號與必要的手動復位信號；系統應記錄發送狀態與時間戳，並在判定完成後發送對應訊號（OK/NG）且回應時間 ≤100 ms。
- **FR-008**: 系統 MUST 提供簡潔易用的使用者介面（操作步驟 ≤3 步），並在螢幕上以綠/紅燈、顯示標註影像與文字回饋顯示結果。
- **FR-009**: 系統 MUST 支援報告生成與匯出（包含影像、測量數據、判定結果），並能匯出統計資料以供品質分析。
- **FR-010**: 系統 MUST 在光源變化與輕微 PCB 偏移（±2 mm）下維持要求的檢測準確度或在無法判定時回報可復現的錯誤訊息。
- **FR-011**: 系統 MUST 記錄測試所使用之 deterministic fixtures 與測試種子，並在 CI 中包含視覺回歸測試 (golden images) 以阻止回歸。

*Assumptions*: 所有度量以 mm 為單位；系統將依照使用者提供之 PCB 規格（零件清單與容差）進行比對；相機與光源需先行校正與穩定。

### Performance & Resource Constraints (MANDATORY for CV features)

- **PR-001**: 單張影像完整檢測時間（含擷取、前處理、推論、後處理、標註與報告）必須 ≤ 3 秒（目標）。
- **PR-002**: 系統啟動時間 ≤ 10 秒（冷啟動到可進行第一張檢測）。
- **PR-003**: 運動控制訊號回應時間 ≤ 100 ms。
- **PR-004**: 定位精度 ≤ ±0.1 mm，尺寸測量誤差 ≤ ±0.05 mm，整體瑕疵檢測準確率 ≥ 99%，誤判率 (False Positive) < 1%。
- **PR-005**: 系統須能連續穩定運轉 8 小時以上，並在光源隨時間衰減下仍能維持性能（或在超出能力時回報光源異常）。

### Data Privacy & Licensing (MANDATORY for data-driven features)

- **DP-001**: 測試資料與生產影像中若包含可辨識個資（例如人像、條碼等）必須有明確的同意與去識別化策略。  
- **DP-002**: 引入之第三方模型與資料集須含 license 與 provenance 文件並在 spec 中記錄。  
- **DP-003**: NG 影像儲存必須遵守公司資料保存政策，並支援匯出/刪除策略。

### Key Entities *(include if feature involves data)*

- **PCB**: 代表一片待檢測電路板；屬性：id、型號、批次、校正參數、檢測時間、來源(相機/離線檔案)。
- **Component**: 電子零件；屬性：type、reference、center_x, center_y (mm)、angle (deg)、size (w,h)、presence (present/missing)、match_score。
- **Defect**: 瑕疵事件；屬性：type、severity、bounding_box、confidence、related_component (optional)、timestamp。
- **InspectionResult**: 單次檢測結果；屬性：pcb_id、OK/NG、defects[], measurements[], annotated_image_path、report_path、model_version、processing_time_ms。
- **MotionEvent**: 與運動控制系統互動之訊息紀錄；屬性：event_type (OK/NG)、timestamp、sent_status、coordinate (optional)。

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 單片 PCB 檢測準確率 ≥ 99%（基於預先標註的驗證資料集）。
- **SC-002**: 定位精度 ≤ ±0.1 mm，尺寸測量誤差 ≤ ±0.05 mm（在校正條件下驗證）。
- **SC-003**: 單張影像檢測處理時間 ≤ 3 秒（含影像擷取與報告生成）。
- **SC-004**: 系統在 8 小時連續運行中維持可用性（無致命崩潰，且平均處理時間在允收範圍）。
- **SC-005**: 運動控制訊號在判定完成後 ≤100 ms 內送出並獲回應。

---

**Next step**: 如無異議，我將建立規格品質檢查表並針對上述 [NEEDS CLARIFICATION] 項目提出具體選項供確認。
