public class ReadHoldingRegistersRequest : ModbusHttpRequest
{
    //1 byte
    public byte SlaveId { get; set; }

    //2 bytes
    public ushort Address { get; set; }

    //2 bytes
    public ushort Count { get; set; }
}