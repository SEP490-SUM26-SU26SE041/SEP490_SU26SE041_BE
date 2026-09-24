using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.IoTDevices;
using SmartFarmSEP490.Repository.Interfaces.Sensors;
using SmartFarmSEP490.Service.Interfaces.IoTDevices;

namespace SmartFarmSEP490.Service.Services.IoTDevices;

/// <summary>
/// Implementation của IIoTDeviceService.
/// </summary>
public class IoTDeviceService : IIoTDeviceService
{
    private readonly IIoTDeviceRepository _deviceRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IoTDeviceService> _logger;

    // Ngưỡng offline (phút) - mặc định 5 phút
    private int OfflineThresholdMinutes =>
        int.TryParse(_configuration["IoT:OfflineThresholdMinutes"], out var v) ? v : 5;

    public IoTDeviceService(
        IIoTDeviceRepository deviceRepository,
        ISensorRepository sensorRepository,
        IBatchRepository batchRepository,
        IAlertRepository alertRepository,
        IConfiguration configuration,
        ILogger<IoTDeviceService> logger)
    {
        _deviceRepository = deviceRepository;
        _sensorRepository = sensorRepository;
        _batchRepository = batchRepository;
        _alertRepository = alertRepository;
        _configuration = configuration;
        _logger = logger;
    }

    // ============== READ ==============

    public async Task<List<IoTDeviceResponseDto>> GetAllAsync()
    {
        var devices = await _deviceRepository.GetAllAsync();
        return devices.Select(MapToResponseDto).ToList();
    }

    public async Task<IoTDeviceResponseDto?> GetByIdAsync(Guid id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        return device == null ? null : MapToResponseDto(device);
    }

    public async Task<IoTDeviceResponseDto?> GetByDeviceCodeAsync(string deviceCode)
    {
        var device = await _deviceRepository.GetByDeviceCodeAsync(deviceCode);
        return device == null ? null : MapToResponseDto(device);
    }

    public async Task<List<IoTDeviceResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var devices = await _deviceRepository.GetByBatchIdAsync(batchId);
        return devices.Select(MapToResponseDto).ToList();
    }

    public async Task<List<IoTDeviceListItemDto>> GetOfflineDevicesAsync()
    {
        var devices = await _deviceRepository.GetActiveDevicesAsync();
        var threshold = OfflineThresholdMinutes;
        var now = DateTime.UtcNow;

        return devices
            .Where(d => d.LastActiveAt.HasValue &&
                        (now - d.LastActiveAt.Value).TotalMinutes > threshold)
            .Select(d => MapToListItemDto(d))
            .ToList();
    }

    // ============== CREATE ==============

    public async Task<IoTDeviceResponseDto> CreateAsync(CreateIoTDeviceDto dto)
    {
        // Validate DeviceCode uniqueness
        var existing = await _deviceRepository.GetByDeviceCodeAsync(dto.DeviceCode);
        if (existing != null)
            throw new InvalidOperationException($"DeviceCode '{dto.DeviceCode}' đã tồn tại.");

        // Validate BatchId (nếu có)
        Guid? validBatchId = null;
        if (dto.BatchId.HasValue)
        {
            var batch = await _batchRepository.GetByIdAsync(dto.BatchId.Value);
            if (batch == null)
                throw new InvalidOperationException($"BatchId '{dto.BatchId}' không tồn tại.");

            if (!batch.IsIoTEnabled)
                throw new InvalidOperationException(
                    $"Batch '{batch.BatchCode}' chưa bật IoT. Hãy bật IoT cho batch trước.");

            validBatchId = dto.BatchId;
        }

        var now = DateTime.UtcNow;
        var device = new IoTDevice
        {
            Id = Guid.NewGuid(),
            DeviceCode = dto.DeviceCode,
            DeviceName = dto.DeviceName,
            MacAddress = dto.MacAddress,
            DeviceType = dto.DeviceType,
            BatchId = validBatchId,
            IsActive = dto.IsActive,
            Status = (validBatchId.HasValue && dto.IsActive) ? IoTDeviceStatus.Active : IoTDeviceStatus.Inactive,
            CreatedAt = now,
            UpdatedAt = now,
            LastActiveAt = null
        };

        await _deviceRepository.AddAsync(device);

        // Tạo mappings với Sensors
        if (dto.Sensors != null && dto.Sensors.Any())
        {
            await CreateOrUpdateSensorMappings(device.Id, dto.Sensors);
        }

        await _deviceRepository.SaveChangesAsync();
        _logger.LogInformation("Created IoTDevice {DeviceCode} with {SensorCount} sensor mappings",
            device.DeviceCode, dto.Sensors?.Count ?? 0);

        return (await GetByIdAsync(device.Id))!;
    }

    // ============== UPDATE ==============

    public async Task<IoTDeviceResponseDto> UpdateAsync(Guid id, UpdateIoTDeviceDto dto)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        if (device == null)
            throw new KeyNotFoundException($"Không tìm thấy IoTDevice với Id '{id}'.");

        if (!string.IsNullOrWhiteSpace(dto.DeviceName))
            device.DeviceName = dto.DeviceName;

        if (dto.MacAddress != null)
            device.MacAddress = dto.MacAddress;

        if (dto.BatchId.HasValue || dto.IsActive.HasValue)
        {
            // Update BatchId nếu có
            if (dto.BatchId.HasValue)
            {
                if (dto.BatchId.Value == Guid.Empty)
                {
                    // Gỡ khỏi Batch
                    device.BatchId = null;
                }
                else
                {
                    var batch = await _batchRepository.GetByIdAsync(dto.BatchId.Value);
                    if (batch == null)
                        throw new InvalidOperationException($"BatchId '{dto.BatchId}' không tồn tại.");

                    if (!batch.IsIoTEnabled)
                        throw new InvalidOperationException(
                            $"Batch '{batch.BatchCode}' chưa bật IoT.");

                    device.BatchId = dto.BatchId.Value;
                }
            }

            // Update IsActive
            if (dto.IsActive.HasValue)
                device.IsActive = dto.IsActive.Value;

            // Recompute Status
            device.Status = (device.IsActive && device.BatchId.HasValue)
                ? IoTDeviceStatus.Active
                : IoTDeviceStatus.Inactive;
        }

        device.UpdatedAt = DateTime.UtcNow;

        await _deviceRepository.UpdateAsync(device);

        // Update sensor mappings nếu có
        if (dto.Sensors != null)
        {
            await _deviceRepository.DeleteDeviceSensorsByDeviceIdAsync(device.Id);
            await _deviceRepository.SaveChangesAsync();

            if (dto.Sensors.Any())
                await CreateOrUpdateSensorMappings(device.Id, dto.Sensors);

            await _deviceRepository.SaveChangesAsync();
        }
        else
        {
            await _deviceRepository.SaveChangesAsync();
        }

        _logger.LogInformation("Updated IoTDevice {DeviceCode}", device.DeviceCode);
        return (await GetByIdAsync(device.Id))!;
    }

    // ============== DELETE ==============

    public async Task<bool> DeleteAsync(Guid id)
    {
        var device = await _deviceRepository.GetByIdAsync(id);
        if (device == null) return false;

        await _deviceRepository.DeleteAsync(device);
        await _deviceRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted IoTDevice {DeviceCode}", device.DeviceCode);
        return true;
    }

    // ============== ASSIGN TO BATCH ==============

    public async Task<IoTDeviceResponseDto> AssignToBatchAsync(AssignDeviceToBatchDto dto)
    {
        var device = await _deviceRepository.GetByIdAsync(dto.DeviceId);
        if (device == null)
            throw new KeyNotFoundException($"Không tìm thấy IoTDevice với Id '{dto.DeviceId}'.");

        if (dto.BatchId.HasValue && dto.BatchId.Value != Guid.Empty)
        {
            var batch = await _batchRepository.GetByIdAsync(dto.BatchId.Value);
            if (batch == null)
                throw new InvalidOperationException($"BatchId '{dto.BatchId}' không tồn tại.");

            if (!batch.IsIoTEnabled)
                throw new InvalidOperationException(
                    $"Batch '{batch.BatchCode}' chưa bật IoT. Hãy bật IoT cho batch trước.");

            device.BatchId = dto.BatchId.Value;
            device.IsActive = true;
            device.Status = IoTDeviceStatus.Active;
            device.LastActiveAt = DateTime.UtcNow;
        }
        else
        {
            // Gỡ khỏi Batch
            device.BatchId = null;
            device.IsActive = false;
            device.Status = IoTDeviceStatus.Inactive;
        }

        device.UpdatedAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);
        await _deviceRepository.SaveChangesAsync();

        _logger.LogInformation("Assigned IoTDevice {DeviceCode} to BatchId={BatchId}",
            device.DeviceCode, device.BatchId);

        return (await GetByIdAsync(device.Id))!;
    }

    // ============== TOGGLE BATCH IoT ==============

    public async Task<bool> ToggleBatchIoTAsync(ToggleBatchIoTDto dto)
    {
        var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
        if (batch == null)
            throw new KeyNotFoundException($"Không tìm thấy Batch với Id '{dto.BatchId}'.");

        batch.IsIoTEnabled = dto.IsIoTEnabled;
        batch.IoTEnabledAt = dto.IsIoTEnabled ? DateTime.UtcNow : null;

        await _batchRepository.UpdateAsync(batch);
        await _deviceRepository.SaveChangesAsync();

        _logger.LogInformation("Toggled BatchIoT for {BatchCode} → {IsIoTEnabled}",
            batch.BatchCode, dto.IsIoTEnabled);

        return dto.IsIoTEnabled;
    }

    // ============== SENSOR DATA ==============

    public async Task<IoTDeviceSensorHistoryResponseDto?> GetSensorDataAsync(
        Guid deviceId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null) return null;

        var sensorIds = device.DeviceSensors.Select(ds => ds.SensorId).ToList();
        if (!sensorIds.Any())
        {
            return new IoTDeviceSensorHistoryResponseDto(
                deviceId, device.DeviceCode, device.Batch?.BatchCode, 0, new List<IoTDeviceSensorDataDto>());
        }

        var sensorDict = device.DeviceSensors.ToDictionary(ds => ds.SensorId, ds => ds.Sensor);
        var allData = new List<SensorDatum>();

        foreach (var sensorId in sensorIds)
        {
            var data = await _sensorRepository.GetSensorDataAsync(sensorId, fromDate, toDate, limit);
            allData.AddRange(data);
        }

        var ordered = allData.OrderByDescending(sd => sd.RecordedAt).Take(limit).ToList();

        var dataList = ordered.Select(sd =>
        {
            var sensor = sensorDict.TryGetValue(sd.SensorId, out var s) ? s : null;
            return new IoTDeviceSensorDataDto(
                sd.Id,
                sd.SensorId,
                sensor?.SensorCode ?? "",
                sensor?.SensorType ?? SensorType.Other,
                sd.Value,
                sd.RecordedAt
            );
        }).ToList();

        return new IoTDeviceSensorHistoryResponseDto(
            deviceId, device.DeviceCode, device.Batch?.BatchCode,
            dataList.Count, dataList);
    }

    public async Task<List<IoTDeviceSensorDataDto>?> GetLatestSensorDataAsync(Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null) return null;

        var result = new List<IoTDeviceSensorDataDto>();
        foreach (var ds in device.DeviceSensors)
        {
            var latest = await _sensorRepository.GetLatestReadingBySensorAsync(ds.SensorId);
            if (latest != null)
            {
                result.Add(new IoTDeviceSensorDataDto(
                    latest.Id,
                    latest.SensorId,
                    ds.Sensor.SensorCode,
                    ds.Sensor.SensorType,
                    latest.Value,
                    latest.RecordedAt));
            }
        }
        return result.OrderByDescending(x => x.RecordedAt).ToList();
    }

    // ============== PRIVATE HELPERS ==============

    /// <summary>
    /// Tạo Sensor (nếu chưa có) + IoTDeviceSensor mapping.
    /// </summary>
    private async Task CreateOrUpdateSensorMappings(Guid deviceId, List<IoTDeviceSensorDto> dtos)
    {
        var mappings = new List<IoTDeviceSensor>();

        foreach (var dto in dtos)
        {
            Guid sensorId;

            if (dto.SensorId.HasValue && dto.SensorId.Value != Guid.Empty)
            {
                // Dùng Sensor đã có
                var existing = await _sensorRepository.GetByIdAsync(dto.SensorId.Value);
                if (existing == null)
                    throw new InvalidOperationException($"SensorId '{dto.SensorId}' không tồn tại.");
                sensorId = existing.Id;
            }
            else
            {
                // Tạo Sensor mới hoặc dùng Sensor đã tồn tại theo code
                var existing = await _sensorRepository.GetByCodeAsync(dto.SensorCode);
                if (existing != null)
                {
                    sensorId = existing.Id;
                }
                else
                {
                    var newSensor = new Sensor
                    {
                        Id = Guid.NewGuid(),
                        SensorCode = dto.SensorCode,
                        SensorType = dto.SensorType,
                        Description = $"Auto-created for IoTDevice {deviceId}",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _sensorRepository.AddAsync(newSensor);
                    await _sensorRepository.SaveChangesAsync();
                    sensorId = newSensor.Id;
                }
            }

            mappings.Add(new IoTDeviceSensor
            {
                Id = Guid.NewGuid(),
                IoTDeviceId = deviceId,
                SensorId = sensorId,
                MqttFieldName = dto.MqttFieldName,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (mappings.Any())
            await _deviceRepository.AddDeviceSensorsAsync(mappings);
    }

    private IoTDeviceResponseDto MapToResponseDto(IoTDevice d)
    {
        var sensors = d.DeviceSensors.Select(ds => new IoTDeviceSensorResponseDto(
            ds.Id,
            ds.SensorId,
            ds.Sensor?.SensorCode ?? "",
            ds.Sensor?.SensorType ?? SensorType.Other,
            ds.MqttFieldName
        )).ToList();

        return new IoTDeviceResponseDto(
            d.Id,
            d.DeviceCode,
            d.DeviceName,
            d.MacAddress,
            d.DeviceType,
            d.BatchId,
            d.Batch?.BatchCode,
            d.IsActive,
            d.Status,
            IsOnline(d),
            d.CreatedAt,
            d.UpdatedAt,
            d.LastActiveAt,
            sensors);
    }

    private IoTDeviceListItemDto MapToListItemDto(IoTDevice d) =>
        new(
            d.Id, d.DeviceCode, d.DeviceName, d.MacAddress, d.DeviceType,
            d.BatchId, d.Batch?.BatchCode, d.IsActive, d.Status,
            IsOnline(d), d.LastActiveAt);

    private bool IsOnline(IoTDevice d)
    {
        if (!d.IsActive || d.BatchId == null || !d.LastActiveAt.HasValue)
            return false;
        var threshold = OfflineThresholdMinutes;
        return (DateTime.UtcNow - d.LastActiveAt.Value).TotalMinutes <= threshold;
    }
}
