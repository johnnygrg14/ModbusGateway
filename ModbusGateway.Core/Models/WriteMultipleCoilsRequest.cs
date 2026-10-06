public class WriteMultipleCoilsRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    public bool[] Values { get; set; } = Array.Empty<bool>();
}