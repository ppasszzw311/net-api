# 台電用電量圖表API使用指南

本指南說明如何使用整合的台電用電量圖表生成功能。

## 🚀 功能概述

我們已經成功整合了 Draw API 服務 (`https://mak-draw-api.zeabur.app`) 到您的 .NET API 項目中，提供以下圖表生成功能：

- **PNG 格式圖表**：直接下載圖表文件
- **Base64 格式圖表**：API 返回 Base64 編碼的圖表數據
- **多種時間範圍**：全部數據、指定時間區間、最近N天
- **健康檢查**：檢查圖表服務可用性

## 📡 API 端點

### 1. PNG 格式圖表

#### 生成全部資料的圖表
```http
GET /api/taipowerdatacontroller/chart/all
```
**響應**: PNG 圖片文件下載

#### 生成特定時間區段的圖表
```http
GET /api/taipowerdatacontroller/chart/range/{start}/{end}
```
**參數**:
- `start`: 開始日期 (格式: yyyy-MM-dd)
- `end`: 結束日期 (格式: yyyy-MM-dd)

**範例**:
```http
GET /api/taipowerdatacontroller/chart/range/2024-01-01/2024-01-07
```

#### 生成最近N天的圖表
```http
GET /api/taipowerdatacontroller/chart/recent/{days}
```
**參數**:
- `days`: 天數 (1-365)

**範例**:
```http
GET /api/taipowerdatacontroller/chart/recent/7
```

### 2. Base64 格式圖表

#### 生成全部資料的Base64圖表
```http
GET /api/taipowerdatacontroller/chart/base64/all
```
**響應**:
```json
{
  "success": true,
  "data": {
    "chartBase64": "iVBORw0KGgoAAAANSUhEUgAA...",
    "format": "png",
    "timestamp": "2025-06-22T14:30:00Z"
  },
  "message": "圖表生成成功"
}
```

#### 生成特定時間區段的Base64圖表
```http
GET /api/taipowerdatacontroller/chart/base64/range/{start}/{end}
```

### 3. 健康檢查

#### 檢查圖表服務狀態
```http
GET /api/taipowerdatacontroller/chart/health
```
**響應**:
```json
{
  "success": true,
  "chartServiceHealthy": true,
  "message": "圖表服務運行正常",
  "timestamp": "2024-01-01T12:00:00"
}
```

## 🎯 使用範例

### JavaScript/Fetch 範例

```javascript
// 下載PNG圖表
async function downloadChart() {
  const response = await fetch('/api/taipowerdatacontroller/chart/recent/7');
  const blob = await response.blob();
  
  // 創建下載連結
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.style.display = 'none';
  a.href = url;
  a.download = 'taipower_chart.png';
  document.body.appendChild(a);
  a.click();
  window.URL.revokeObjectURL(url);
}

// 獲取Base64圖表
async function getChartBase64() {
  const response = await fetch('/api/taipowerdatacontroller/chart/base64/all');
  const data = await response.json();
  
  if (data.success) {
    // 顯示圖表
    const img = document.createElement('img');
    img.src = `data:image/png;base64,${data.data.chartBase64}`;
    document.body.appendChild(img);
  }
}

// 檢查服務健康狀態
async function checkChartService() {
  const response = await fetch('/api/taipowerdatacontroller/chart/health');
  const data = await response.json();
  console.log('圖表服務狀態:', data.message);
}
```

### cURL 範例

```bash
# 下載PNG圖表
curl -X GET "http://localhost:5000/api/taipowerdatacontroller/chart/recent/7" \
  --output taipower_chart.png

# 獲取Base64圖表
curl -X GET "http://localhost:5000/api/taipowerdatacontroller/chart/base64/range/2024-01-01/2024-01-07" \
  -H "Accept: application/json"

# 檢查服務健康狀態
curl -X GET "http://localhost:5000/api/taipowerdatacontroller/chart/health"
```

## 🛠️ 技術詳情

### 服務架構
- **TaiPowerChartService**: 負責與 Draw API 通信
- **HttpClient**: 配置30秒超時，自動重試機制
- **資料轉換**: 自動將資料庫時間轉換為正確格式

### 資料格式
圖表服務接收的資料格式：
```json
{
  "data": [
    {
      "time": "2024-01-01T00:00:00",
      "eastConsumption": 150.5,
      "centralConsumption": 230.8,
      "northConsumption": 420.2,
      "southConsumption": 180.7
    }
  ]
}
```

### 錯誤處理
- **400**: 日期格式錯誤
- **404**: 沒有找到資料
- **500**: Draw API 服務錯誤或網路問題

## 🔧 配置說明

圖表服務已在 `Program.cs` 中配置：

```csharp
// 註冊 HttpClient 和 TaiPowerChartService
builder.Services.AddHttpClient<TaiPowerChartService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // 設定30秒超時
});
builder.Services.AddScoped<TaiPowerChartService>();
```

## 📊 圖表特性

生成的圖表包含：
- **四個地區的用電量趨勢線**：東部、中部、北部、南部
- **時間軸**：根據資料範圍自動調整
- **圖例**：清楚標示各地區
- **高品質 PNG 格式**：適合報告和展示

## ⚡ 效能建議

1. **資料量控制**：建議單次請求不超過1000個資料點
2. **快取策略**：考慮在前端實施圖表快取
3. **非同步處理**：使用非同步方法避免阻塞
4. **錯誤重試**：網路問題時可重試2-3次

## 🎨 整合建議

### 前端整合
- 可以將 Base64 圖表直接嵌入 HTML
- PNG 檔案適合報告生成和郵件附件
- 支援響應式設計，圖表會自動調整大小

### 後端整合  
- 可與現有的 Excel/Word/PDF 匯出功能結合
- 支援批次處理多個時間範圍
- 可擴展支援更多圖表類型

## 📄 PDF 圖表改善

我們已經大幅改善了 PDF 匯出功能中的圖表生成：

### ✅ 改善內容

1. **優先使用 Draw API**：PDF 現在優先使用穩定的 Draw API 生成高品質圖表
2. **備用方案**：如果 Draw API 無法使用，自動切換到 HTML Chart Service
3. **更好的錯誤處理**：提供詳細的錯誤資訊和日誌記錄
4. **替代統計表格**：當圖表無法生成時，顯示統計數據表格
5. **改善的圖表樣式**：更大的圖表尺寸和更好的說明文字
6. **控制器層數據限制**：PDF數據限制邏輯移到控制器層面，自動限制為最近24小時的數據

### 🔄 PDF 圖表生成流程

```
1. 控制器層面篩選最近24小時數據
   ↓
2. 將篩選後的數據傳遞給PDF服務
   ↓
3. 嘗試使用 Draw API 生成圖表
   ↓ (如果成功)
4. 將高品質圖表嵌入PDF
   ↓ (如果失敗)
5. 使用 HTML Chart Service 備用方案
   ↓ (如果成功)
6. 將備用圖表嵌入PDF
   ↓ (如果都失敗)
7. 顯示統計數據表格作為替代
```

### 📊 PDF 圖表特色

- **高解析度**：Draw API 生成的圖表品質更高
- **更快速度**：無需啟動瀏覽器，生成速度更快
- **更穩定**：減少瀏覽器相關的錯誤
- **智能備援**：多層備用方案確保PDF總能成功生成
- **詳細說明**：包含圖表說明和統計資訊
- **數據優化**：控制器層面自動限制為最近24小時數據，確保PDF檔案大小適中和生成速度
- **架構清晰**：數據限制邏輯在控制器層面處理，服務層專注於圖表生成

### 🎯 使用範例

現有的PDF端點將自動使用改善的圖表功能：

```bash
# 生成包含改善圖表的PDF
GET /api/taipowerdatacontroller/export/pdf/all
GET /api/taipowerdatacontroller/export/pdf/range/2024-01-01/2024-01-07
```

---

**注意**: 
- 確保 Draw API 服務 (`https://mak-draw-api.zeabur.app`) 可正常訪問
- PDF 功能已包含完整的錯誤處理和備用方案，即使 Draw API 暫時無法使用，PDF 仍可正常生成
- **數據限制**：PDF 中的圖表和表格數據在控制器層面自動限制為最近24小時。如果原始數據超過24小時，系統會自動篩選最近24小時的數據
- 對於大量數據的圖表需求，建議使用獨立的圖表端點 (`/chart/all` 或 `/chart/range/{start}/{end}`) 來生成完整的圖表
- **架構優勢**：數據限制邏輯在控制器層面處理，符合分層架構原則，服務層專注於圖表生成功能 