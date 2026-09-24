# IoT Flow API - Tổng hợp cho Frontend

> Tài liệu này tổng hợp toàn bộ API liên quan đến luồng IoT (thiết bị ESP32-C3 + cảm biến + luồng dữ liệu MQTT).
> **Base URL:** `http://localhost:5000`
> **Auth:** Tất cả endpoints đều yêu cầu Bearer Token (JWT) + Role **Researcher**

---

## 📑 MỤC LỤC

1. [Luồng tổng quan](#-luồng-tổng-quan-frontend-cần-biết)
2. [Enums](#-enums-dùng-trong-api)
3. [Danh sách endpoints](#-danh-sách-endpoints)
   - [Quản lý thiết bị](#1-quản-lý-thiết-bị-crud)
   - [Gán batch & bật/tắt IoT](#2-gán-thiết-bị-vào-batch--bậttắt-iot)
   - [Đọc dữ liệu cảm biến](#3-đọc-dữ-liệu-cảm-biến)
4. [Luồng chạy thực tế (Step-by-step)](#-luồng-chạy-thực-tế-cho-fe)
5. [Cấu trúc response thống nhất](#-cấu-trúc-response-thống-nhất)
6. [Lỗi thường gặp](#-lỗi-thường-gặp)

---

## 🔁 LUỒNG TỔNG QUAN (Frontend cần biết)

```
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND WORKFLOW                         │
└─────────────────────────────────────────────────────────────┘

   [1] Tạo thiết bị IoT mới
       Researcher nhập: DeviceCode, DeviceName, Sensors (loại + MQTT field)
       POST /api/iot-devices
                       │
                       ▼
   [2] Bật IoT cho Batch (nếu chưa bật)
       PATCH /api/iot-devices/batch/toggle-iot  {batchId, isIoTEnabled: true}
                       │
                       ▼
   [3] Gán thiết bị vào Batch
       POST /api/iot-devices/assign-batch  {deviceId, batchId}
                       │
                       ▼
   [4] Thiết bị ESP32-C3 bắt đầu gửi data qua MQTT
       Topic: smartfarm/iot/{deviceCode}/data
                       │
                       ▼
   [5] Backend tự động lưu vào DB
       (KHÔNG CẦN FE gọi API - xử lý nền)
                       │
                       ▼
   [6] FE poll/lấy dữ liệu cảm biến để hiển thị chart
       GET /api/iot-devices/{id}/sensor-data?fromDate=...&toDate=...
```

**Điểm quan trọng:**
- ❗ Một thiết bị **BẮT BUỘC** phải được gán vào Batch có `IsIoTEnabled = true` thì mới hoạt động.
- ❗ `Sensors` được tạo cùng lúc với thiết bị (mapping MQTT field → Sensor).
- ❗ Backend tự nhận MQTT, FE chỉ cần đọc lại dữ liệu để hiển thị.

---

## 🏷️ ENUMS DÙNG TRONG API

### `IoTDeviceStatus`
```typescript
enum IoTDeviceStatus {
  Inactive = 0,  // Chưa gán Batch HOẶC đã tắt thủ công
  Active = 1     // Đã gán vào Batch có IsIoTEnabled=true
}
```

### `SensorType`
```typescript
enum SensorType {
  Temperature = 1,   // Nhiệt độ (°C)
  Humidity = 2,      // Độ ẩm không khí (%)
  SoilMoisture = 3,  // Độ ẩm đất (%)
  Light = 4,         // Ánh sáng (lux)
  PH = 5,            // Độ pH
  Other = 6          // Khác
}
```

### `SensorStatus`
```typescript
enum SensorStatus {
  Inactive = 0,
  Active = 1,
  Faulty = 2   // Lỗi
}
```

---

## 📡 DANH SÁCH ENDPOINTS

### 1. QUẢN LÝ THIẾT BỊ (CRUD)

#### 📍 `GET /api/iot-devices` — Lấy danh sách tất cả thiết bị

**Auth:** Researcher

**Request body:** Không

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "deviceCode": "ESP-001",
      "deviceName": "Cảm biến ngoài trời 1",
      "macAddress": "AA:BB:CC:DD:EE:FF",
      "deviceType": "ESP32-C3-Water-Sensor",
      "batchId": "guid",
      "batchCode": "BATCH-2026-001",
      "isActive": true,
      "status": 1,
      "isOnline": true,
      "createdAt": "2026-09-20T10:00:00Z",
      "updatedAt": "2026-09-24T08:30:00Z",
      "lastActiveAt": "2026-09-24T08:30:00Z",
      "sensors": [
        {
          "id": "guid",
          "sensorId": "guid",
          "sensorCode": "TEMP-AIR-ESP001-01",
          "sensorType": 1,
          "mqttFieldName": "dht_h"
        }
      ]
    }
  ]
}
```

**Lưu ý FE:**
- `isOnline`: Backend tự tính = `LastActiveAt` trong vòng 5 phút gần nhất.
- `status = 1` + `isActive = true` + `isOnline = true` → thiết bị đang hoạt động bình thường.

---

#### 📍 `GET /api/iot-devices/offline` — Lấy danh sách thiết bị đang offline

**Auth:** Researcher

**Dùng khi:** Hiển thị cảnh báo thiết bị mất kết nối.

**Request body:** Không

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "deviceCode": "ESP-001",
      "deviceName": "Cảm biến ngoài trời 1",
      "macAddress": "AA:BB:CC:DD:EE:FF",
      "deviceType": "ESP32-C3-Water-Sensor",
      "batchId": "guid",
      "batchCode": "BATCH-2026-001",
      "isActive": true,
      "status": 1,
      "isOnline": false,
      "lastActiveAt": "2026-09-24T03:00:00Z"
    }
  ]
}
```

**Lưu ý:** `IoTDeviceListItemDto` (không có `Sensors`) - response rút gọn cho list view.

---

#### 📍 `GET /api/iot-devices/{id}` — Lấy chi tiết 1 thiết bị

**Auth:** Researcher

**URL params:**
- `id` (Guid) — ID của thiết bị

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "deviceCode": "ESP-001",
    "deviceName": "Cảm biến ngoài trời 1",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "deviceType": "ESP32-C3-Water-Sensor",
    "batchId": "guid",
    "batchCode": "BATCH-2026-001",
    "isActive": true,
    "status": 1,
    "isOnline": true,
    "createdAt": "2026-09-20T10:00:00Z",
    "updatedAt": "2026-09-24T08:30:00Z",
    "lastActiveAt": "2026-09-24T08:30:00Z",
    "sensors": [
      {
        "id": "guid",
        "sensorId": "guid",
        "sensorCode": "TEMP-AIR-ESP001-01",
        "sensorType": 1,
        "mqttFieldName": "dht_h"
      }
    ]
  }
}
```

**Response 404:**
```json
{ "success": false, "message": "Không tìm thấy thiết bị." }
```

---

#### 📍 `GET /api/iot-devices/code/{deviceCode}` — Lấy thiết bị theo DeviceCode

**Auth:** Researcher

**URL params:**
- `deviceCode` (string) — Mã thiết bị, ví dụ: `ESP-001`

**Dùng khi:** Debug, tìm nhanh theo mã thiết bị (không cần biết ID).

**Response:** Giống `GET /api/iot-devices/{id}`.

---

#### 📍 `GET /api/iot-devices/batch/{batchId}` — Lấy danh sách thiết bị theo Batch

**Auth:** Researcher

**URL params:**
- `batchId` (Guid) — ID của batch

**Dùng khi:** Hiển thị thiết bị thuộc 1 batch trong trang chi tiết batch.

**Response:** Giống `GET /api/iot-devices` (array).

---

#### 📍 `POST /api/iot-devices` — Tạo mới thiết bị IoT

**Auth:** Researcher

**Request body:**
```json
{
  "deviceCode": "ESP-001",
  "deviceName": "Cảm biến ngoài trời 1",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "deviceType": "ESP32-C3-Water-Sensor",
  "batchId": "guid (optional - có thể null khi tạo trước, gán sau)",
  "isActive": true,
  "sensors": [
    {
      "sensorId": null,
      "sensorCode": "TEMP-AIR-ESP001-01",
      "sensorType": 1,
      "mqttFieldName": "dht_h"
    },
    {
      "sensorId": null,
      "sensorCode": "MOIST-SOIL-ESP001-01",
      "sensorType": 3,
      "mqttFieldName": "soil_1"
    }
  ]
}
```

**Giải thích fields:**

| Field | Bắt buộc | Mô tả |
|---|---|---|
| `deviceCode` | ✅ | Mã duy nhất (unique) — trùng với mã ESP32 in trên thiết bị |
| `deviceName` | ✅ | Tên hiển thị |
| `macAddress` | ❌ | MAC address của ESP32 |
| `deviceType` | ✅ | Loại thiết bị (mặc định: `"ESP32-C3-Water-Sensor"`) |
| `batchId` | ❌ | Gán vào batch ngay từ đầu (nếu đã có batch + đã bật IoT) |
| `isActive` | ✅ | `true` = kích hoạt, `false` = tạm tắt |
| `sensors` | ❌ | Danh sách mapping cảm biến — có thể tạo sau qua API Update |
| `sensorId` | ❌ | `null` = tự tạo Sensor mới, có giá trị = dùng sensor đã có |
| `sensorCode` | ✅ | Mã cảm biến (unique) |
| `sensorType` | ✅ | Xem enum `SensorType` |
| `mqttFieldName` | ✅ | **Tên field trong JSON MQTT** mà ESP32 gửi lên |

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "deviceCode": "ESP-001",
    // ... toàn bộ thông tin thiết bị vừa tạo + sensors
  }
}
```

**Response 400 (lỗi validation):**
```json
{ "success": false, "message": "DeviceCode 'ESP-001' đã tồn tại." }
// HOẶC
{ "success": false, "message": "BatchId 'xxx' không tồn tại." }
// HOẶC
{ "success": false, "message": "Batch 'BATCH-001' chưa bật IoT. Hãy bật IoT cho batch trước." }
```

---

#### 📍 `PUT /api/iot-devices/{id}` — Cập nhật thiết bị

**Auth:** Researcher

**URL params:**
- `id` (Guid) — ID thiết bị

**Request body:**
```json
{
  "deviceName": "Cảm biến ngoài trời 1 (cập nhật)",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "batchId": "guid (optional - null = gỡ khỏi batch)",
  "isActive": true,
  "sensors": [
    {
      "sensorId": "guid (optional - null = tạo mới)",
      "sensorCode": "TEMP-AIR-ESP001-01",
      "sensorType": 1,
      "mqttFieldName": "dht_h"
    }
  ]
}
```

**Giải thích:**
- Tất cả fields đều **optional** — chỉ gửi field muốn update.
- Gửi `sensors` = thay thế toàn bộ mapping cũ.
- Gửi `batchId: null` = gỡ thiết bị khỏi batch.

**Response 200:** Giống `GET /api/iot-devices/{id}`

**Response 404:**
```json
{ "success": false, "message": "Không tìm thấy IoTDevice với Id 'xxx'." }
```

---

#### 📍 `DELETE /api/iot-devices/{id}` — Xóa thiết bị

**Auth:** Researcher

**URL params:**
- `id` (Guid) — ID thiết bị

**⚠️ Lưu ý:** Xóa thiết bị sẽ xóa luôn tất cả sensor mappings + sensor data liên quan.

**Response 200:**
```json
{ "success": true, "message": "Đã xóa thiết bị." }
```

**Response 404:**
```json
{ "success": false, "message": "Không tìm thấy thiết bị." }
```

---

### 2. GÁN THIẾT BỊ VÀO BATCH & BẬT/TẮT IoT

#### 📍 `POST /api/iot-devices/assign-batch` — Gán thiết bị vào Batch

**Auth:** Researcher

**Request body:**
```json
{
  "deviceId": "guid",
  "batchId": "guid"   // null = GỠ khỏi batch
}
```

**Dùng khi:**
- Sau khi tạo thiết bị chưa gán batch.
- Chuyển thiết bị sang batch khác.
- Gỡ thiết bị khỏi batch (`batchId: null`).

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "deviceCode": "ESP-001",
    // ... toàn bộ thông tin thiết bị sau khi gán
  }
}
```

**Response 400 (lỗi):**
```json
{ "success": false, "message": "Batch 'BATCH-001' chưa bật IoT. Hãy bật IoT cho batch trước." }
```

---

#### 📍 `POST /api/iot-devices/batch/toggle-iot` — Bật/tắt IoT cho Batch

**Auth:** Researcher

**Request body:**
```json
{
  "batchId": "guid",
  "isIoTEnabled": true
}
```

**Dùng khi:**
- Bật IoT cho batch trước khi gán thiết bị (bắt buộc).
- Tạm tắt IoT cho cả batch (ví dụ: kết thúc experiment).

**Response 200:**
```json
{
  "success": true,
  "isIoTEnabled": true,
  "message": "Đã bật IoT cho batch."
}
// HOẶC khi tắt:
{
  "success": true,
  "isIoTEnabled": false,
  "message": "Đã tắt IoT cho batch."
}
```

**Response 404:**
```json
{ "success": false, "message": "Không tìm thấy batch." }
```

---

### 3. ĐỌC DỮ LIỆU CẢM BIẾN

#### 📍 `GET /api/iot-devices/{id}/sensor-data` — Lấy lịch sử dữ liệu cảm biến

**Auth:** Researcher

**URL params:**
- `id` (Guid) — ID thiết bị

**Query params:**

| Param | Type | Required | Default | Mô tả |
|---|---|---|---|---|
| `fromDate` | ISO DateTime | ❌ | null | Lọc từ ngày (VD: `2026-09-01T00:00:00Z`) |
| `toDate` | ISO DateTime | ❌ | null | Lọc đến ngày |
| `limit` | int | ❌ | 100 | Số bản ghi tối đa trả về |

**Response 200:**
```json
{
  "success": true,
  "data": {
    "deviceId": "guid",
    "deviceCode": "ESP-001",
    "batchCode": "BATCH-2026-001",
    "totalRecords": 150,
    "sensorData": [
      {
        "id": "guid",
        "sensorId": "guid",
        "sensorCode": "TEMP-AIR-ESP001-01",
        "sensorType": 1,
        "value": 28.5,
        "recordedAt": "2026-09-24T08:30:00Z"
      },
      {
        "id": "guid",
        "sensorId": "guid",
        "sensorCode": "MOIST-SOIL-ESP001-01",
        "sensorType": 3,
        "value": 65.2,
        "recordedAt": "2026-09-24T08:30:00Z"
      }
    ]
  }
}
```

**Dùng khi:** Vẽ biểu đồ lịch sử cảm biến (chart).

**Gợi ý FE:**
- Filter theo `sensorType` ở client-side để vẽ nhiều line chart.
- Polling mỗi 30-60 giây để cập nhật real-time.

---

#### 📍 `GET /api/iot-devices/{id}/sensor-data/latest` — Lấy giá trị cảm biến mới nhất

**Auth:** Researcher

**URL params:**
- `id` (Guid) — ID thiết bị

**Response 200:**
```json
{
  "success": true,
  "data": {
    "deviceId": "guid",
    "deviceCode": "ESP-001",
    "batchCode": "BATCH-2026-001",
    "totalRecords": 2,
    "sensorData": [
      {
        "id": "guid",
        "sensorId": "guid",
        "sensorCode": "TEMP-AIR-ESP001-01",
        "sensorType": 1,
        "value": 28.5,
        "recordedAt": "2026-09-24T08:30:00Z"
      }
    ]
  }
}
```

**Dùng khi:** Hiển thị giá trị hiện tại trên dashboard (gauge, indicator).

---

## 🚀 LUỒNG CHẠY THỰC TẾ CHO FE

### Scenario A: Người dùng có sẵn Batch, muốn thêm thiết bị IoT

```
BƯỚC 1: Kiểm tra Batch đã bật IoT chưa
        GET /api/batches/{batchId} → kiểm tra batch.isIoTEnabled
        
        Nếu false → gọi BƯỚC 2
        Nếu true → bỏ qua BƯỚC 2

BƯỚC 2: Bật IoT cho Batch (nếu chưa bật)
        POST /api/iot-devices/batch/toggle-iot
        Body: { batchId, isIoTEnabled: true }

BƯỚC 3: Tạo thiết bị mới + gán luôn vào batch
        POST /api/iot-devices
        Body: {
          deviceCode, deviceName, macAddress, deviceType,
          batchId: <batchId>,
          isActive: true,
          sensors: [{ sensorCode, sensorType, mqttFieldName, ... }]
        }

BƯỚC 4: ESP32-C3 bắt đầu gửi data
        (Không cần FE can thiệp - MQTT tự xử lý)

BƯỚC 5: Hiển thị dữ liệu
        GET /api/iot-devices/{id}/sensor-data/latest  (mỗi 30s)
        GET /api/iot-devices/{id}/sensor-data?fromDate=...&toDate=... (chart)
```

### Scenario B: Người dùng muốn tạo thiết bị trước, gán batch sau

```
BƯỚC 1: Tạo thiết bị không gán batch
        POST /api/iot-devices
        Body: {
          deviceCode, deviceName, ..., 
          batchId: null,        ← Không gán batch
          isActive: false,      ← Tạm tắt
          sensors: []
        }

BƯỚC 2: Bật IoT cho batch (nếu cần)
        POST /api/iot-devices/batch/toggle-iot

BƯỚC 3: Gán thiết bị vào batch
        POST /api/iot-devices/assign-batch
        Body: { deviceId, batchId }

BƯỚC 4: Bật thiết bị
        PUT /api/iot-devices/{id}
        Body: { isActive: true }
```

### Scenario C: Xem dashboard thiết bị IoT của 1 batch

```
BƯỚC 1: Lấy danh sách thiết bị của batch
        GET /api/iot-devices/batch/{batchId}
        → Hiển thị danh sách với status (online/offline/active/inactive)

BƯỚC 2: Khi click vào 1 thiết bị → lấy chi tiết
        GET /api/iot-devices/{id}
        → Hiển thị sensors + MQTT field mapping

BƯỚC 3: Poll dữ liệu mỗi 30s
        GET /api/iot-devices/{id}/sensor-data/latest
        → Hiển thị giá trị hiện tại (gauge card)

BƯỚC 4: Vẽ chart lịch sử
        GET /api/iot-devices/{id}/sensor-data?fromDate=...&toDate=...
        → Group by sensorType, vẽ line chart
```

### Scenario D: Cảnh báo thiết bị offline

```
BƯỚC 1: Lấy danh sách thiết bị offline (mỗi 1-5 phút)
        GET /api/iot-devices/offline
        → Hiển thị banner cảnh báo nếu có thiết bị offline
```

---

## 📦 CẤU TRÚC RESPONSE THỐNG NHẤT

**Mọi response đều có format:**
```json
{
  "success": true | false,
  "data": <object | array> | null,    // optional
  "message": "<string>"                // optional, có khi success=false hoặc info
}
```

**HTTP Status Codes:**

| Code | Ý nghĩa |
|---|---|
| 200 | OK - Thành công |
| 201 | Created - Tạo mới thành công |
| 400 | Bad Request - Validation lỗi (DeviceCode trùng, Batch chưa bật IoT, ...) |
| 401 | Unauthorized - Token không hợp lệ / hết hạn |
| 403 | Forbidden - Không đủ quyền (không phải Researcher) |
| 404 | Not Found - Không tìm thấy resource |
| 500 | Internal Server Error - Lỗi server |

---

## ⚠️ LỖI THƯỜNG GẶP

### 1. `"DeviceCode 'XXX' đã tồn tại"`
**Nguyên nhân:** `deviceCode` đã được dùng cho thiết bị khác.
**Fix:** Đổi `deviceCode` khác (phải unique).

### 2. `"Batch 'XXX' chưa bật IoT. Hãy bật IoT cho batch trước."`
**Nguyên nhân:** Batch chưa có `isIoTEnabled = true`.
**Fix:** Gọi `POST /api/iot-devices/batch/toggle-iot` trước.

### 3. `"BatchId 'XXX' không tồn tại"`
**Nguyên nhân:** `batchId` không hợp lệ.
**Fix:** Kiểm tra lại batch có tồn tại không (gọi `GET /api/batches/{id}`).

### 4. `403 Forbidden`
**Nguyên nhân:** User không phải Researcher.
**Fix:** Đăng nhập bằng tài khoản có role Researcher.

### 5. Thiết bị tạo xong nhưng không nhận được data từ ESP32
**Checklist debug:**
- [ ] Batch đã `isIoTEnabled = true`?
- [ ] Thiết bị đã được gán vào batch?
- [ ] `mqttFieldName` trong sensor mapping **KHỚP** với field trong JSON MQTT ESP32 gửi lên?
- [ ] ESP32 đã connect được vào MQTT broker chưa?
- [ ] `deviceCode` ESP32 gửi lên có khớp với `deviceCode` trong DB?

### 6. MQTT Message Format mà ESP32 cần gửi:
```
Topic: smartfarm/iot/{deviceCode}/data
Payload (JSON):
{
  "dht_h": 28.5,
  "soil_1": 65.2,
  "lux": 12345,
  "ph": 6.8
}
```
**Lưu ý:** Tên field trong JSON phải khớp với `mqttFieldName` đã khai báo khi tạo sensor.

---

## 📋 QUICK REFERENCE - TẤT CẢ ENDPOINTS

| # | Method | Endpoint | Mục đích |
|---|---|---|---|
| 1 | GET | `/api/iot-devices` | Danh sách tất cả thiết bị |
| 2 | GET | `/api/iot-devices/offline` | Danh sách thiết bị offline |
| 3 | GET | `/api/iot-devices/{id}` | Chi tiết 1 thiết bị |
| 4 | GET | `/api/iot-devices/code/{deviceCode}` | Tìm theo mã thiết bị |
| 5 | GET | `/api/iot-devices/batch/{batchId}` | Danh sách thiết bị theo batch |
| 6 | POST | `/api/iot-devices` | Tạo thiết bị mới |
| 7 | PUT | `/api/iot-devices/{id}` | Cập nhật thiết bị |
| 8 | DELETE | `/api/iot-devices/{id}` | Xóa thiết bị |
| 9 | POST | `/api/iot-devices/assign-batch` | Gán / gỡ thiết bị khỏi batch |
| 10 | POST | `/api/iot-devices/batch/toggle-iot` | Bật / tắt IoT cho batch |
| 11 | GET | `/api/iot-devices/{id}/sensor-data` | Lịch sử cảm biến (có filter) |
| 12 | GET | `/api/iot-devices/{id}/sensor-data/latest` | Giá trị cảm biến mới nhất |

**Auth:** Tất cả đều yêu cầu **Researcher role** + Bearer token.

---

**Version:** 1.0
**Last updated:** 2026-09-25
**Maintainer:** Backend IoT Team
