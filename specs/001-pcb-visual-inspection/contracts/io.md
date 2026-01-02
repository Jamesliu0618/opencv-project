# contracts/io.md — Digital IO Contract (Advantech 規格兼容)

## 目的
定義與運動控制系統（Advantech PCI-1245E / PCI-1756 等）之 Digital IO 介面契約，包含訊號映射、時序、電平與錯誤處理。

## 支援硬體
- 明確支援：Advantech PCI-1245E、PCI-1756（需安裝 Advantech 驅動與 SDK）
- 其它支援：通用 I/O 模組（以 driver adapter 實作）

> 備註：實際硬體引腳與電平（24V vs TTL）請以安裝現場之 Advantech 模組文件為主；本契約設定提供抽象層與建議參數以確保相容性。

## 訊號定義建議
- OUTPUT_OK: 輸出訊號（channel id 指定） — 由系統在 OK 判定後發送短脈衝（建議 100 ms）
- OUTPUT_NG: 輸出訊號 — 由系統在 NG 判定後發送短脈衝（建議 100 ms）
- INPUT_RESET: 輸入訊號 — 手動/外部復位，系統需監控並記錄
- OUTPUT_ERROR: 輸出錯誤/故障警示（長脈衝或持續狀態）

### Example pin_map (JSON)
{
  "OUTPUT_OK": {"module":"PCI-1245E","port":0,"bit":0},
  "OUTPUT_NG": {"module":"PCI-1245E","port":0,"bit":1},
  "INPUT_RESET": {"module":"PCI-1245E","port":1,"bit":0}
}

## Electrical & Timing
- 電平：應與 Advantech 模組相容（通常 TTL 電平；若使用 24V 請確認隔離/轉接）
- Pulse duration: 建議 100 ms（可配置）
- Debounce / sampling: 10 ms filtering for inputs
- Latency requirement: signal send + acknowledge ≤ 100 ms

## IO Driver API (抽象介面)
- initialize(config) -> void
- set_output(signal_name, value, pulse_ms=null) -> status
- read_input(signal_name) -> bool
- register_callback(signal_name, callback) -> handler
- get_status() -> dict
- close() -> void

## Error handling & Safety
- 若 `set_output` 回傳失敗，系統應重試 N 次（exponential backoff 可選）並記錄 MotionEvent status=failed；重試次數與回溯策略為可配置。
- 若 IO 斷線或驅動錯誤，系統應：
  1. 立即停止發出新的檢測命令（進入安全暫停狀態）
  2. 通知操作員並記錄詳細錯誤（錯誤碼、堆疊、時間戳）
  3. 提供手動復位介面與診斷指引
- Timeout 處理：`set_output` 與 `read_input` 操作需提供 `timeout_ms` 參數，超時視為失敗並執行重試策略。
- 例外處理範例 (C# / pseudo-code):

```csharp
try
{
    var status = advantech.SetOutput("OUTPUT_OK", true, pulseMs:100);
    if (!status.Success) HandleFailure(status);
}
catch (TimeoutException ex)
{
    Log.Error("IO timeout", ex);
    // retry logic
}
catch (Exception ex)
{
    Log.Critical("Unexpected IO error", ex);
    // escalate / pause line
}
```

## Integration notes for Advantech PCI-1245E / PCI-1756
- 使用 Advantech 官方 .NET SDK（或 vendor 提供的 managed wrapper）並實作 `advantech_adapter`，將抽象信號映射到 module/port/bit。Adapter 應提供：初始化、set_output/read_input、register_callback、diagnostics（connection status, error counters）。
- 範例 C# usage (pseudo-code):

```csharp
var card = new AdvantechAdapter(config);
card.Initialize();
var okStatus = card.SetOutput("OUTPUT_OK", true, pulseMs:100);
if (!okStatus.Success) { /* handle */ }
var input = card.ReadInput("INPUT_RESET");
```

## PLC Interface (optional)
- 若需要與 PLC 整合，可支援 Modbus TCP 或 OPC UA：
  - **Modbus TCP**: 使用 `NModbus` 或類似 .NET library，定義 register map 與 coils 對照。
  - **OPC UA**: 使用 OPC Foundation .NET Standard SDK，定義 AddressSpace 與 Node 結構以貫通監控與控制資訊。

### Modbus example (C# / pseudo-code)
```csharp
var client = new ModbusTcpClient(plcIp, port);
client.WriteCoil(coilAddress, true); // send OK
var coilValue = client.ReadCoils(coilAddress, 1);
```

### OPC UA example (C# / pseudo-code)
```csharp
var client = new UaClient(endpointUrl);
client.WriteNode(nodeId, true); // send OK
var value = client.ReadNode(nodeId);
```

---

## Test strategy
- 提供模擬 IO 驅動（mock_io）供 CI 使用，模擬信號收發與失敗場景
- 集成測試：在實際硬體上驗證 OK/NG 的 pulse timing 與 latency
- 建議在安裝時執行 hardware smoke test：發送 OK/NG 並確認控制卡接收到訊號且機台做出預期動作