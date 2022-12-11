namespace BaSyxControlComponent.Services;

public interface ICommunicationService
{
    // TODO: Extend here to access additional data from modbus if needed 
    void Connect();

    int? GetState(int machineIndex);
    int? GetProduced(int machineIndex);
    int? GetSpeed(int machineIndex);
    int? GetRpm(int machineIndex);
    int? GetVoltage_cV(int machineIndex);
    int? GetAmpere_cA(int machineIndex);
    int? GetPower_W(int machineIndex);
    int? GetTemperature_cC(int machineIndex);

    void Start(int machineIndex);
    void Stop(int machineIndex);
}