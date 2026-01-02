# contracts/camera.md — Camera Abstraction & Contract

## 目標
定義 camera abstraction 的 API、必要屬性、支持的相機類型（Basler / GigE Vision 等）與運作契約（觸發模式、校正、錯誤處理、mock 驅動）。

## 支援硬體
- 優先支援：Basler (GigE Vision, via pylon SDK)
- 兼容：任何支援 GenICam / GigE Vision 的工業相機
- 開發建議：使用相機供應商 SDK (pylon / Spinnaker) 以取得穩定的抓圖與硬體特性；fallback 可使用 OpenCV VideoCapture（限制：較低的穩定性與功能）

## Config 範例 (camera_config.json)
{
  "name": "Basler-1",
  "type": "Basler",
  "interface": "GigE",
  "resolution": [4096, 3000],
  "pixel_format": "Mono8",
  "trigger_mode": "Hardware" | "Software",
  "exposure_us": 3000,
  "gain": 10,
  "frame_rate": 10,
  "timeout_ms": 2000
}

## Camera Driver API (抽象介面)
- initialize(config) → void
- start_capture() → void
- stop_capture() → void
- capture_frame(timeout_ms) → Frame
- get_settings() → dict
- set_settings(settings) → void
- calibrate(chessboard_params) → CalibrationResult
- get_calibration() → CalibrationResult | null
- close() → void

Frame 物件:
- image (ndarray / in .NET: Mat or Bitmap via Emgu/OpenCvSharp)
- timestamp (ISO8601)
- exposure, gain, frame_id
- metadata (camera serial, model)

## C# Integration Example (EmguCV / OpenCvSharp)

Example using OpenCvSharp's VideoCapture (fallback):

```csharp
using OpenCvSharp;

public Mat CaptureFrame(string device)
{
    using var cap = new VideoCapture(device);
    var mat = new Mat();
    if (!cap.IsOpened()) throw new InvalidOperationException("Camera not opened");
    cap.Read(mat);
    if (mat.Empty()) throw new TimeoutException("No frame received within timeout");
    return mat;
}
```

Example using a vendor SDK wrapper (pseudo-code):

```csharp
// vendor SDK may provide a managed wrapper or C API; implement adapter:
public Frame CaptureFrameFromVendor(VendorCamera cam, int timeoutMs)
{
    cam.Start();
    var success = cam.WaitForFrame(timeoutMs, out var image);
    if (!success) throw new TimeoutException("Vendor camera timeout");
    return new Frame { Image = image, Timestamp = DateTime.UtcNow, Metadata = cam.GetInfo() };
}
```

## Triggering
- 支援 Software Trigger 與 Hardware Trigger；在產線上應優先使用 Hardware Trigger（同步性好，避免漏帧）。
- 當使用 Hardware Trigger 時，介面應支援外部脈衝邏輯的配置說明。

## 校正與坐標轉換
- 必須支援 camera→board 的 pixel_to_mm 轉換並保存校正參數（intrinsics & extrinsics）。
- 提供 API: compute_pixel_to_mm(calibration_images, pattern_size, square_size_mm) → matrix

## Mock 驅動
- 為了 CI 與本地開發，實作一個 mock driver，讀取 `tests/fixtures/images/` 並回傳 deterministic frames（支援 seed）。

## Error handling
- capture_frame 失敗應回傳明確錯誤碼並重試策略 (configurable retries)
- driver initialization 的失敗需記錄詳細錯誤與 recovery 建議（例如檢查網路 / 攝影機 IP / 驅動安裝）

## Performance & Observability
- driver 應報告每張影像的取得時間、frame_id、任何 dropped frames
- 建議在 debug 模式下保存原始影像到 `debug/` 並儲存 metadata（exposure、gain）以便追蹤

---

## Implementation notes
- 在 Windows 上，推薦使用 vendor SDK（例如 Basler pylon）以獲得穩定 GigE 性能；如採用 OpenCV VideoCapture，需加入監控與重連策略。
- 配置文件必須支援多相機情況（若未來需要）。