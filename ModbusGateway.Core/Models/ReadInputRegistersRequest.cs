public class ReadInputRegistersRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    public ushort Count { get; set; }
}