using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.IoTDevices;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.IoTDevices;

/// <summary>
/// Implementation của IIoTDeviceRepository sử dụng Entity Framework Core.
/// </summary>
public class IoTDeviceRepository : IIoTDeviceRepository
{
    private readonly SmartFarmDbContext _context;

    public IoTDeviceRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    // ===================== IoTDevice =====================

    public async Task<IoTDevice?> GetByIdAsync(Guid id)
    {
        return await Task.FromResult(_context.IoTDevices
            .Include(d => d.Batch)
            .Include(d => d.DeviceSensors)
                .ThenInclude(ds => ds.Sensor)
            .FirstOrDefault(d => d.Id == id));
    }

    public async Task<IoTDevice?> GetByDeviceCodeAsync(string deviceCode)
    {
        return await Task.FromResult(_context.IoTDevices
            .Include(d => d.Batch)
            .Include(d => d.DeviceSensors)
                .ThenInclude(ds => ds.Sensor)
            .FirstOrDefault(d => d.DeviceCode == deviceCode));
    }

    public async Task<List<IoTDevice>> GetAllAsync()
    {
        return await Task.FromResult(_context.IoTDevices
            .Include(d => d.Batch)
            .Include(d => d.DeviceSensors)
                .ThenInclude(ds => ds.Sensor)
            .OrderByDescending(d => d.CreatedAt)
            .ToList());
    }

    public async Task<List<IoTDevice>> GetByBatchIdAsync(Guid batchId)
    {
        return await Task.FromResult(_context.IoTDevices
            .Include(d => d.Batch)
            .Include(d => d.DeviceSensors)
                .ThenInclude(ds => ds.Sensor)
            .Where(d => d.BatchId == batchId)
            .OrderByDescending(d => d.CreatedAt)
            .ToList());
    }

    public async Task<List<IoTDevice>> GetActiveDevicesAsync()
    {
        return await Task.FromResult(_context.IoTDevices
            .Include(d => d.Batch)
            .Where(d => d.IsActive && d.BatchId != null)
            .ToList());
    }

    public async Task AddAsync(IoTDevice device)
    {
        await _context.IoTDevices.AddAsync(device);
    }

    public async Task UpdateAsync(IoTDevice device)
    {
        _context.IoTDevices.Update(device);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(IoTDevice device)
    {
        _context.IoTDevices.Remove(device);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // ===================== IoTDeviceSensor =====================

    public async Task<List<IoTDeviceSensor>> GetDeviceSensorsAsync(Guid deviceId)
    {
        return await Task.FromResult(_context.IoTDeviceSensors
            .Include(ds => ds.Sensor)
            .Where(ds => ds.IoTDeviceId == deviceId)
            .ToList());
    }

    public async Task<IoTDeviceSensor?> GetDeviceSensorByIdAsync(Guid id)
    {
        return await Task.FromResult(_context.IoTDeviceSensors
            .Include(ds => ds.Sensor)
            .FirstOrDefault(ds => ds.Id == id));
    }

    public async Task<IoTDeviceSensor?> GetByDeviceAndMqttFieldAsync(Guid deviceId, string mqttFieldName)
    {
        return await Task.FromResult(_context.IoTDeviceSensors
            .Include(ds => ds.Sensor)
            .FirstOrDefault(ds => ds.IoTDeviceId == deviceId && ds.MqttFieldName == mqttFieldName));
    }

    public async Task AddDeviceSensorAsync(IoTDeviceSensor mapping)
    {
        await _context.IoTDeviceSensors.AddAsync(mapping);
    }

    public async Task AddDeviceSensorsAsync(IEnumerable<IoTDeviceSensor> mappings)
    {
        await _context.IoTDeviceSensors.AddRangeAsync(mappings);
    }

    public async Task DeleteDeviceSensorsByDeviceIdAsync(Guid deviceId)
    {
        var sensors = _context.IoTDeviceSensors.Where(ds => ds.IoTDeviceId == deviceId);
        _context.IoTDeviceSensors.RemoveRange(sensors);
        await Task.CompletedTask;
    }
}
