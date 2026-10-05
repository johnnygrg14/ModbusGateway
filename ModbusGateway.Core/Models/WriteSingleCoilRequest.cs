public class WriteSingleCoilRequest : ModbusHttpRequest
{
    public byte SlaveId { get; set; }

    public ushort Address { get; set; }

    //@todo this needs to change
    public bool Value { get; set; }
}