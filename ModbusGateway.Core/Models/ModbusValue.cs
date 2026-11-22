namespace ModbusGateway.Core.Models;

/// <summary>
/// This class is used for response formatting
/// It has two attributes.The address and the value
/// The address attribute refers to the address of a modbus value
/// and the value is a generic type so the formatter can support
/// boolean and numeric values
/// This is used to create responses for modbus read functions.
/// </summary>
/// <typeparam name="T"></typeparam>

public class ModbusValue<T>
{
    public int Address { get; set; }
    public T? Value { get; set; }
}