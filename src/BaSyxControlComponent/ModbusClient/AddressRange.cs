namespace BaSyxControlComponent.ModbusClient;

/// <summary>
///     Defines modbus address range
/// </summary>
public class AddressRange
{
    /// <summary>
    /// </summary>
    /// <param name="startingAddress">First input register to be read</param>
    /// <param name="quantity">Number of input registers to be read</param>
    public AddressRange(int startingAddress, int quantity)
    {
        StartingAddress = startingAddress;
        Quantity = quantity;
    }


    public int StartingAddress { get; }
    public int Quantity { get; }
}