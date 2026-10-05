public class WriteMultipleRegistersRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    public ushort[] Values { get; set; } = Array.Empty<ushort>();
}