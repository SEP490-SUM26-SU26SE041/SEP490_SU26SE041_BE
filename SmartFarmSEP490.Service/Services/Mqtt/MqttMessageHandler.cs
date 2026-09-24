using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.Interfaces.IoTDevices;
using SmartFarmSEP490.Repository.Interfaces.Sensors;
using SmartFarmSEP490.Service.Interfaces.Mqtt;

namespace SmartFarmSEP490.Service.Services.Mqtt;

/// <summary>
/// Parse payload MQTT từ ESP32-C3 và lưu vào SensorData.
/// Topic: watersensor/{deviceCode}/data
/// Payload: { dht_h, dht_t, ds18b20_t, light_pct, ph }
/// </summary>
public class MqttMessageHandler : IMqttMessageHandler
{
    private readonly ILogger<MqttMessageHandler> _logger;
    private readonly IIoTDeviceRepository _deviceRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IMqttSettings _mqttSettings;

    public MqttMessageHandler(
        ILogger<MqttMessageHandler> logger,
        IIoTDeviceRepository deviceRepository,
        ISensorRepository sensorRepository,
        IMqttSettings mqttSettings)
    {
        _logger = logger;
        _deviceRepository = deviceRepository;
        _sensorRepository = sensorRepository;
        _mqttSettings = mqttSettings;
    }

    public async Task HandleAsync(string topic, string payload)
    {
        try
        {
            // 1. Extract deviceCode từ topic
            // Topic: "watersensor/ESP001/data"
            var parts = topic.Split('/');
            if (parts.Length < 3)
            {
                _logger.LogWarning("[MQTT] Invalid topic format: {Topic}", topic);
                return;
            }
            var deviceCode = parts[1];
            
            // ✅ LOG để debug
            _logger.LogInformation("[MQTT] 📥 Received | Topic: {Topic} | DeviceCode: {DeviceCode} | Payload: {Payload}", 
                topic, deviceCode, payload);

            // 2. Parse JSON
            if (string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("[MQTT] Empty payload from {DeviceCode}", deviceCode);
                return;
            }

            JsonDocument? json;
            try
            {
                json = JsonDocument.Parse(payload);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "[MQTT] Invalid JSON from {DeviceCode}: {Payload}", deviceCode, payload);
                return;
            }

            using (json)
            {
                // 3. Tìm IoTDevice theo DeviceCode (lấy từ topic)
                _logger.LogInformation("[MQTT] 🔍 Looking for device: {DeviceCode}", deviceCode);
                
                var device = await _deviceRepository.GetByDeviceCodeAsync(deviceCode);
                
                // ✅ FALLBACK: Nếu không tìm thấy theo topic, dùng DefaultDeviceCode
                if (device == null)
                {
                    var defaultCode = _mqttSettings.DefaultDeviceCode;
                    _logger.LogWarning("[MQTT] ⚠️ Device code '{DeviceCode}' from topic not found. Trying DefaultDeviceCode '{Default}'...", 
                        deviceCode, defaultCode);
                    
                    device = await _deviceRepository.GetByDeviceCodeAsync(defaultCode);
                    
                    if (device == null)
                    {
                        _logger.LogWarning("[MQTT] ❌ Both topic code '{TopicCode}' and default '{DefaultCode}' not found. Ignore.",
                            deviceCode, defaultCode);
                        return;
                    }
                    
                    _logger.LogInformation("[MQTT] ✅ Using DefaultDeviceCode '{DefaultCode}' (topic had '{TopicCode}')", 
                        defaultCode, deviceCode);
                }

                // ✅ LOG trạng thái device
                _logger.LogInformation("[MQTT] ✅ Device found: IsActive={IsActive}, BatchId={BatchId}, DeviceSensors count={Count}", 
                    device.IsActive, device.BatchId, device.DeviceSensors?.Count ?? 0);

                // 4. Kiểm tra thiết bị có active và đã gán Batch không
                if (!device.IsActive || device.BatchId == null)
                {
                    _logger.LogWarning("[MQTT] ⚠️ Device {DeviceCode} is not active or no Batch assigned.", deviceCode);
                    return;
                }

                // 5. Lấy Batch
                var batch = device.Batch;
                if (batch == null)
                {
                    _logger.LogWarning("[MQTT] Device {DeviceCode} has BatchId but Batch is null.", deviceCode);
                    return;
                }

                // 6. Với mỗi field MQTT, tìm mapping → tạo SensorDatum
                var root = json.RootElement;
                var now = DateTime.UtcNow;
                int savedCount = 0;

                foreach (var mapping in device.DeviceSensors)
                {
                    if (!root.TryGetProperty(mapping.MqttFieldName, out var valueElement))
                    {
                        _logger.LogDebug("[MQTT] Field '{Field}' not found in JSON", mapping.MqttFieldName);
                        continue;
                    }

                    if (!TryGetDecimal(valueElement, out var value))
                    {
                        _logger.LogDebug("[MQTT] Field '{Field}' is not a valid number", mapping.MqttFieldName);
                        continue;
                    }

                    // ✅ Null-safe: mapping.Sensor có thể null
                    var sensorType = mapping.Sensor?.SensorType.ToString();

                    _logger.LogInformation("[MQTT] 📊 Saving: {SensorType} = {Value} (mapped from '{Field}')", 
                        sensorType, value, mapping.MqttFieldName);

                    var sensorData = new SensorDatum
                    {
                        Id = Guid.NewGuid(),
                        SensorId = mapping.SensorId,
                        ExperimentId = batch.ExperimentId,
                        BatchId = device.BatchId,
                        Value = value,
                        Unit = sensorType,
                        RecordedAt = now
                    };

                    await _sensorRepository.AddSensorDataAsync(sensorData);
                    savedCount++;
                }

                _logger.LogInformation("[MQTT] 📝 Total records saved: {Count}", savedCount);

                await _sensorRepository.SaveChangesAsync();

                // 7. Update LastActiveAt
                device.LastActiveAt = now;
                device.UpdatedAt = now;
                await _deviceRepository.UpdateAsync(device);
                await _deviceRepository.SaveChangesAsync();

                _logger.LogInformation("[MQTT] ✅ Processed data from {DeviceCode} → Batch {BatchCode}",
                    deviceCode, batch.BatchCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT] Unexpected error handling message");
            throw;
        }
    }

    private bool TryGetDecimal(JsonElement element, out decimal value)
    {
        value = 0;
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                return element.TryGetDecimal(out value);
            case JsonValueKind.String:
                return decimal.TryParse(element.GetString(), out value);
            default:
                return false;
        }
    }
}
