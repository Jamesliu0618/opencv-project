# quickstart.md — PCB 視覺檢測 快速上手（Windows 工控環境）

## 目標
說明在 Windows 工業電腦上部署並快速驗證系統的步驟；包含相機（Basler/GigE）、Advantech IO 卡、必要套件與快速 smoke test。

## 先決條件
- Windows 10/11 或 Windows 10 IoT / 工業版
- 建議硬體：Quad-core CPU、16 GB RAM、100GB 可用磁碟、選用 NVIDIA GPU（如需 ML 加速並支援 CUDA）
- 安裝：Visual Studio 2019 或 2022（含 .NET Framework 4.8 開發工具）
- 相機驅動：Basler pylon SDK（若使用 Basler）或 vendor SDK（請依廠商指示安裝）
- IO 驅動：Advantech 驅動與 SDK（PCI-1245E / PCI-1756）

## 建立開發環境（Visual Studio + .NET Framework 4.8）
1. 安裝 Visual Studio 2019 / 2022，選取 "Desktop development with C++"（若需原生擴充）與 ".NET desktop development" 工具。
2. 建立或開啟解決方案 (.sln)。設定目標框架為 **.NET Framework 4.8**，PlatformTarget 為 x64（建議）。
3. 使用 NuGet 安裝影像處理套件（選一）：
   - Emgu CV: `Install-Package Emgu.CV -Version <latest>`
   - OpenCvSharp: `Install-Package OpenCvSharp4 -Version <latest>`
4. 安裝 Advantech SDK 與驅動，並確認 PCI 卡在 Device Manager 中可見。
5. 若需要 ML 推論，可選擇 ONNX Runtime (.NET)、ML.NET 或在獨立 Python ML 服務中部署模型並透過 IPC/REST 呼叫。

(已移除 Python 環境安裝步驟 — Python 僅作為可選的 ML 訓練或研究環境，可在獨立 pipeline 中使用。)

## 相機與 IO 設定
1. 安裝 Basler pylon（若使用 Basler）並確認相機能被系統辨識
2. 使用 vendor 工具設定相機 IP / trigger mode / exposure
3. 安裝 Advantech 驅動並用廠商工具設定 PCI-1245E / PCI-1756 的 port 宣告
4. 在 `config/camera_config.json` 與 `config/io_config.json` 中填入實際 mapping（參考 contracts/* 範例）

## 啟動測試（Mock 模式）
- 若尚未接上實體硬體，可執行 mock driver 測試：
  python -m app.cli.run --mode mock --fixtures tests/fixtures/images
- 執行 smoke test（會跑一張 golden image 的對比測試）
  pytest tests/integration/test_smoke.py -k smoke

## 啟動生產模式（使用實機）
1. 確保 camera driver able to `start_capture()` 並回傳 frames
2. 確保 IO driver 初始化成功且已正確 mapping signals
3. 以 service 模式啟動：
   python service.py --config config/production.yaml
4. 在 UI 上選擇 camera 與 PCB 模型，按 "Start"
5. 觸發一張檢測 → 在 UI 上觀察 annotated image 與判定結果

## Debug / Calibration
- 提供校正工具（`cli calibrate --camera Basler-1 --pattern chessboard --square-size 1.0`）以產生 pixel_to_mm 轉換
- 啟用 debug 模式會保存原始影像與 annotated 輸出於 `debug/` 以供分析

## 部署建議（Windows 工控環境）
- 以 Windows Service 或 NSSM 將後端 service 設為自動啟動
- 建議將相機與 IO 連接至工控網路並限制外部存取
- 建議在每天工廠班次開始時執行健康檢查腳本：檢查相機連線、IO 狀態、disk 空間、最近一次 smoke test
- 建議將 NG 影像與報表備份到公司 NAS（視資安政策而定）

## 常見問題 (FAQ)
- Q: 相機掉線怎麼辦？
  A: 系統會嘗試重試連線，並在連續 N 次失敗後通知操作員並暫停檢測。
- Q: 如何測試 IO 是否正確連線？
  A: 使用 `cli io-test --signal OUTPUT_OK` 發送脈衝並用外部示波器或 IO 診斷工具確認信號。

---

如果你需要，我可以把 step-by-step 的安裝腳本（PowerShell）與一個簡單的 `production.yaml` 範例一併產生供你執行與驗證。