using BaSyxControlComponent.ModbusClient;
using NLog;
using ILogger = NLog.ILogger;

namespace BaSyxControlComponent.Services;

/// <summary>
/// Implements mapping of modbus register and provides access to data for consumers
/// </summary>
public class CommunicationService : ICommunicationService
{
    // TODO: Extend here to handle additional data from modbus
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ModbusClient.ModbusClient _modbusClient;

    public CommunicationService(ModbusClient.ModbusClient modbusClient)
    {
        _modbusClient = modbusClient;
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(10, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(20, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(30, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(40, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(50, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(60, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(70, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(80, 10));
        _modbusClient.AddReadInputRegisterPolling(new AddressRange(90, 10));
    }

    public void Connect()
    {
        try
        {
            _modbusClient.Connect();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Cannot connect to MODBUS server");
        }
    }

    public int? GetState(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 0);
    }

    public int? GetProduced(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 1);
    }

    public int? GetSpeed(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 2);
    }

    public int? GetRpm(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 3);
    }

    public int? GetVoltage_cV(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 4);
    }

    public int? GetAmpere_cA(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 5);
    }

    public int? GetPower_W(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 6);
    }

    public int? GetTemperature_cC(int machineIndex)
    {
        return _modbusClient.GetInputRegisterValue(machineIndex + 7);
    }

    public void Start(int machineIndex)
    {
        _modbusClient.WriteCoil(machineIndex, true);
    }

    public void Stop(int machineIndex)
    {
        _modbusClient.WriteCoil(machineIndex, false);
    }
}