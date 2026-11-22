namespace ModbusGateway.Core.Models;

public class WriteMultipleRegistersRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }
    //2 bytes
    public ushort Address { get; set; }
    //Array of 2 byte values (length is unknown until a request is created)
    public ushort[] Values { get; set; } = Array.Empty<ushort>();
}