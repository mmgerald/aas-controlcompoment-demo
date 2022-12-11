using System.Collections.Concurrent;
using System.Net.Sockets;
using NLog;
using ILogger = NLog.ILogger;

namespace BaSyxControlComponent.ModbusClient;

/// <summary>
///     Implements the dice modbus client
/// </summary>
public class ModbusClient : IDisposable
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentDictionary<AddressRange, int[]?> _readInputRegistersDictionary;
    private EasyModbus.ModbusClient? _modbusClient;

    /// <summary>
    ///     Constructor
    /// </summary>
    public ModbusClient()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _readInputRegistersDictionary = new ConcurrentDictionary<AddressRange, int[]?>();
    }

    public int PollingTimeMs { get; set; } = 2000;
    public int ReconnectTimeMs { get; set; } = 3000;
    public bool IsAutoReconnectEnabled { get; set; } = true;
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 502;


    public void Dispose()
    {
        Disconnect();
    }

    public void Connect()
    {
        _modbusClient = new EasyModbus.ModbusClient(IpAddress, Port);
        _modbusClient.ConnectedChanged += _ => { OnConnect(); };
        OnConnect();

        Task.Run(OnRun, _cancellationTokenSource.Token);
    }

    private void OnConnect()
    {
        Task.Run(() =>
        {
            while (_modbusClient != null && IsAutoReconnectEnabled && !_modbusClient.Connected)
                try
                {
                    Logger.Info($"Connecting to MODBUS server (address: {_modbusClient.IPAddress}, port: {_modbusClient.Port})");

                    _modbusClient.Connect();
                    Logger.Info("Connected to MODBUS server.");

                    break;
                }
                catch (SocketException)
                {
                    Logger.Error($"Cannot connect to MODBUS server. Trying again in {ReconnectTimeMs}ms.");
                    Thread.Sleep(ReconnectTimeMs);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Cannot connect to MODBUS server.");
                    break;
                }
        }, _cancellationTokenSource.Token);
    }

    private void OnRun()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (_modbusClient is { Connected: true })
                foreach (var keyValuePair in _readInputRegistersDictionary)
                {
                    var readInputRegisters =
                        _modbusClient.ReadInputRegisters(keyValuePair.Key.StartingAddress, keyValuePair.Key.Quantity);

                    _readInputRegistersDictionary.AddOrUpdate(keyValuePair.Key, _ => readInputRegisters,
                        (_, _) => readInputRegisters);
                }


            Thread.Sleep(PollingTimeMs);
        }
    }

    public void Disconnect()
    {
        _modbusClient?.Disconnect();
    }

    /// <summary>
    ///     Defines an address range that gets polled as MODBUS InputRegister
    /// </summary>
    /// <param name="addressRange">The address range polled for</param>
    public void AddReadInputRegisterPolling(AddressRange addressRange)
    {
        _readInputRegistersDictionary.AddOrUpdate(addressRange, _ => null, (_, _) => null);
    }

    /// <summary>
    ///     Returns the value of an address
    /// </summary>
    /// <param name="address">The address (needs to be defined before for polling)</param>
    /// <returns></returns>
    public int? GetInputRegisterValue(int address)
    {
        foreach (var keyValuePair in _readInputRegistersDictionary)
            if (keyValuePair.Key.StartingAddress <= address &&
                keyValuePair.Key.StartingAddress + keyValuePair.Key.Quantity > address)
            {
                var elementIndex = address - 1 - keyValuePair.Key.StartingAddress;
                return keyValuePair.Value?[elementIndex];
            }

        return null;
    }

    public void WriteCoil(int address, bool value)
    {
        _modbusClient?.WriteSingleCoil(address - 1, value);
    }
}