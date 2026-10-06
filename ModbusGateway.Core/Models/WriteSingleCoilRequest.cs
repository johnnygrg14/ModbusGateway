public class WriteSingleCoilRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    public bool Value { get; set; }
}